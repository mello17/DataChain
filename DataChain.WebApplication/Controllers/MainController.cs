using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.Web.Http.Results;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Configuration;
using DataChain.Infrastructure;
using DataChain.Abstractions;
using DataChain.WebApi.Models;
using DataChain.WebApplication.Models;
using DataChain.DataProvider;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Net.Http;
using System.Web.WebSockets;
using NLog;

namespace DataChain.WebApplication.Controllers
{

    public class MainController : ApiController
    {
        #region fields

        private readonly IUnitOfWork work;
        private Logger log ;
        private ChainConnector connector;

        #endregion

        #region constructs
        public MainController(IUnitOfWork _work)
        {
            work = _work;
            log = LogManager.GetCurrentClassLogger(typeof(MainController));
            connector = new ChainConnector();
        }

        public MainController()
        {
            work = new UnitOfWork();
            log = LogManager.GetCurrentClassLogger(typeof(MainController));
            connector = new ChainConnector();
            
        }
        #endregion

        #region actions
        [HttpGet]
        [Route("api/main/getchain")]
        public HttpResponseMessage GetChain()
        {
            var currentContext = HttpContext.Current;
            var ip = currentContext.Request.Url.Authority;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                
                currentContext.AcceptWebSocketRequest(SendChainFromWebSockets);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);

            }

            var accountKey = currentContext.Request.Form["key"];
            if (accountKey == null)
            {
                CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Key can't be null  ");
            }
            HexString hexKey = KeyParser(accountKey); 

            Account account = work.Accounts.GetAccount(hexKey);
            if (account == null)
            {
                BadRequest();
            }

            if (account.Role == UserRole.Unset || account.Role == UserRole.Writer)
            {
                CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Permission denied. User not have permission for reading  ");
            }

            
            var chain = connector.GetLocalChain();
            var rawChain = JsonConvert.SerializeObject(chain);
           
            return Request.CreateResponse(HttpStatusCode.OK, rawChain);


        }


        [Route("api/main/globalchain")]
        public JsonResult<IEnumerable<Block>> GetGlobalChain()
        {
            var context = HttpContext.Current;
            var stream = new WebSocketBlockStream(context.Request.Url);
            stream.ProcessRequest(context).Wait();

            return Json(stream.GlobalChain);
        }

        [HttpPost]
        [Route("api/main/authentication")]
        public async Task<string> Authentication()
        {

            JObject jsonContent = null;
            try
            {
                var body = await this.Request.Content.ReadAsStringAsync();
                jsonContent = JObject.Parse(body);
            }
            catch (JsonReaderException)
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Reading JSON Exception");
            }

            string password = null, login = null;
            if (!(jsonContent["login"] is JValue) && !(jsonContent["password"] is JValue) && !(jsonContent["key"] is JValue))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Bad json");
            }
            password = (string)jsonContent["password"];
            login = (string)jsonContent["login"];

            if (string.IsNullOrEmpty(login))
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Login cannot be empty or have null value.");
            }

            if (string.IsNullOrEmpty(password) || password.Count() < 6 || password.Count() > 30)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Пароль не может быть пустым или равным null.");
            }
            SHA256 hasher = SHA256.Create();
            var hash_password = hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));

            if (hash_password != hasher.ComputeHash(password.ToHexString()))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid password");
            }

            Account user = this.work.Accounts.GetAccount(login);

            if (user == null)
            {
                CreateErrorResponse(HttpStatusCode.Unauthorized, "User not found.");
            }

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                    issuer: ConfigurationManager.AppSettings["issuer"],
                    audience: ConfigurationManager.AppSettings["audience"],
                    notBefore: now,
                    claims: GetClaims(user.Login, user.Role).Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var token = new
            {
                access_token = encodedJwt,
                username = user.Login
            };
            var response = JsonConvert.SerializeObject(token, new JsonSerializerSettings { Formatting = Formatting.Indented });

            return response;
        }

        [HttpGet]
        [Route("record/{name}")]
        public JsonResult<Record> GetRecord(string name, string accountKey)
        {

            HexString hexKey = HexString.Empty;
            try
            {
                hexKey = HexString.Parse(accountKey ?? "");
            }
            catch (FormatException)
            {
                BadRequest();
            }

            Account account = work.Accounts.GetAccount(hexKey);
            if(account == null)
            {
                BadRequest();
            }

            if (account.Role == UserRole.Unset || account.Role == UserRole.Writer)
            {
                CreateErrorResponse(HttpStatusCode.Unauthorized, 
                    "Permission denied. User not have permission for reading  ");
            }

            if (string.IsNullOrEmpty(name))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Name cannot be empty");
            }
           
            var result = this.work.Records.GetRecordByNameAsync(name);

            return Json(result);

        }

      
        [HttpPost]
        [Route("commit")]
        public async Task<string> PostTransaction([FromBody]string key, 
            [FromBody]string transaction, [FromBody] string accountKey)
        {

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(accountKey))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Authentication failed");
            }

            HexString rawToken = KeyParser(accountKey);

            Account user = this.work.Accounts.GetAccount(rawToken);
            if(user != null)
                if(user.Role  == UserRole.Reader || user.Role == UserRole.Unset)
                {
                    CreateErrorResponse(HttpStatusCode.Unauthorized, "Permission denied. User not have permission for adding transaction ");
                }
            

            JObject bodyContent = null;
            try
            {
                 bodyContent = JObject.Parse(transaction);
            }

            catch (JsonReaderException)
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Bad json");
            }

            if (!(bodyContent["records"] is JArray))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Not valid records");
            }

            HexString txid = null;
            Transaction newTransaction = null;

            try
            {
                newTransaction = await this.work.TransactionValidator.ValidateTransaction(bodyContent["records"] as JArray, key);
                txid = newTransaction.Hash;
            }
            catch(Exception ex)
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid transactions, reason: "+ex.Message);

            }
            // catch()

            return txid.ToString();
        }

        [HttpGet]
        [Route("tx/{transactionHash}")]
        public async Task<object> GetTransaction( string transactionHash, DataFormat format = DataFormat.Json )
        {

            var hash = KeyParser(transactionHash);

            if(format == DataFormat.Json)
            {
               return await JsonTransaction(hash.ToByteArray());
            }

            return await RawTransaction(hash.ToByteArray());

        }

       
        public async Task<string> RawTransaction( byte[] transactionHash)
        {
            var tx = await this.work.Transactions.GetTransactionAsync(transactionHash);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }
            var hash = BitConverter.ToString(tx.Hash.ToByteArray());

            var formattedHash = hash.Replace("-", "")
                                    .ToLower();

            return formattedHash;
        }
    

        public async Task<JsonResult<Transaction>> JsonTransaction(byte[] transactionHash)
        {

            var tx = await this.work.Transactions.GetTransactionAsync(transactionHash);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }

            var response = new Transaction( tx.TimeStamp, tx.Data, tx.Hash, tx.Sign, tx.PubKey);

            return Json(response);
        }

        [HttpGet]
        [Route("block/{id}")]
        public async Task<JsonResult<Block>> GetBlock(int id)
        {
            var rawBlock = await this.work.Blocks.GetBlock(id);

            if (rawBlock == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Block is not found");
            }

          
           return Json(rawBlock);
        }
        #endregion

        #region Вспомогательные методы

        private void CreateErrorResponse(HttpStatusCode code,string reason)
        {
            throw new HttpResponseException(new HttpResponseMessage
            {
                StatusCode = code,
                ReasonPhrase = reason
            });
        }

        private HexString KeyParser(string key)
        {
            HexString parseKey = HexString.Empty;

            try
            {
                parseKey = HexString.Parse(key ?? "");
            }
            catch
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Key is not valid");
            }

            return parseKey;
        }

        private ClaimsIdentity GetClaims(string login, UserRole role)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, Enum.GetName(typeof(UserRole),role))
                };

            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }

        private async Task<object> SendChainFromWebSockets(AspNetWebSocketContext context)
        {
            var handler = new BlockChainHandler();

            byte[] buffer = new byte[1024 * 1024];
            ChainSerializer chainSerializer = new ChainSerializer();

            buffer = chainSerializer.Encode(connector.GetLocalChain().BlockChain);
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            WebSocket socket = context.WebSocket;

            try
            {
                handler.OnOpen();
                var user = context.User;
                WebSocketReceiveResult receiveResult;

                while (socket.State == WebSocketState.Open)
                {
                    do
                    {
                        receiveResult = await socket.ReceiveAsync(
                           segment, CancellationToken.None);

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }
                    } while (!receiveResult.EndOfMessage);

                    await socket.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);

                }
            }

            catch (Exception ex)
            {
                log.Error("Error when respond blockchain " + ex.Message);
                throw new InvalidBlockException("Error when respond blockchain ");
            }
            finally
            {

                handler.OnClose();
            }

            var processTask = handler.ProcessWebSocketRequestAsync(context);
            return processTask;
        }

        #endregion
    }
}
