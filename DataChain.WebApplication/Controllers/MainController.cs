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
using Microsoft.AspNet;
using System.Configuration;
using DataChain.Infrastructures;
using DataChain.DataLayer;
using DataChain.WebServices.Models;
using DataChain.WebApplication.Models;
using DataChain.EntityFramework;
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
        private readonly IUnitOfWork work = new UnitOfWork();
        private Logger log ;

        public MainController(IUnitOfWork _work)
        {
            work = _work;
            log = LogManager.GetCurrentClassLogger();
        }

        public MainController()
        {
            log = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [Route("api/main/getchain")]
        public HttpResponseMessage GetChain()
        {
            var currentContext = HttpContext.Current;
            var ip = currentContext.Request.Url.Authority;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);

            }

            var accountKey = currentContext.Request.Form["key"];
            HexString hexKey = KeyParser(accountKey);
            try
            {
                hexKey = HexString.Parse(accountKey ?? "");
            }
            catch (FormatException)
            {
                BadRequest();
            }

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

            ChainConnector connector = new ChainConnector();
            var chain = connector.GetLocalChain();
            var rawChain = JsonConvert.SerializeObject(chain);
            return Request.CreateResponse(HttpStatusCode.OK, rawChain);


        }

        private async Task<object> ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            var handler = new NewHandler();

            byte[] buffer = new byte[1024 * 1024];
            ChainSerializer chainSerializer = new ChainSerializer();
            ChainConnector connector = new ChainConnector();
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


                    using (MemoryStream stream = new MemoryStream())
                    {
                        await socket.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
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
        public void GetAllRecords()
        {

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

            Account user = this.work.Accounts.GetAccount(login);

            if (user == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "User not found.");
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
        public async Task<JsonResult<Record>> GetRecord(string name, string accountKey)
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
           
            Record result = await this.work.Records.GetRecordsByNameAsync(name);

            return Json(result);

        }

        [HttpGet]
        [Route("recordbyindex/{hash}")]
        public void GetRecordByHash(string hash, int recordType)
        {

        }

        [HttpPost]
        [Route("commit")]
        public async Task<HexString> PostTransaction([FromBody]string key, 
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

            var block = await this.work.Blocks.GetBlock(rawToken);
            

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
                newTransaction = await this.work.TransactionValidator.ValidateTransaction((JArray)bodyContent["records"], key);
                txid = newTransaction.Hash;
            }
            catch(FormatException)
            {
                //do something
            }
            // catch()

            return txid ?? HexString.Empty;
        }

        [HttpGet]
        [Route("tx/{id}")]
        public async Task<object> GetTransaction( string transactionHash, DataFormat format = DataFormat.Json )
        {

            var hash = HexString.Parse(transactionHash);

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
    

        public async Task<JsonResult<Transaction>> JsonTransaction( byte[] transactionHash)
        {

            var tx = await this.work.Transactions.GetTransactionAsync(transactionHash);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }

            var response = new Transaction( tx.TimeStamp, tx.Data, tx.Hash, tx.Sign, tx.PubKey);

            return Json(response);
        }

        #region Вспомогательные методы
        private object GetRecordJson(Record records)
        {
            return new
            {
                version = records.Version,
                name  = records.Name,
                value  = records.Value?.Value
            };
        }

        private void CreateErrorResponse(HttpStatusCode code,string reason)
        {
            throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
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


        #endregion
    }
}
