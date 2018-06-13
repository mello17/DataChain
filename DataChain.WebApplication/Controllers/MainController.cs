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
using System.Runtime.Serialization.Formatters.Binary;

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

        public MainController()
        {
            work = new UnitOfWork();
            log = LogManager.GetCurrentClassLogger();
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

            var rawKey = AccountKeyBuilder.Decode(hexKey.ToByteArray());
            Account account = work.Accounts.GetAccount(rawKey);
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

        public async Task<JsonResult<IEnumerable<Block>>> GetGlobalChain()
        {
            var context = HttpContext.Current;
            var stream = new WebSocketBlockStream(context.Request.Url);
            await stream.ProcessRequest(context);

            return Json(stream.GlobalChain);
        }

        [HttpPost]
        [Route("api/main/newAccount")]
        public async Task CreateNewAccount()
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

            
            if (!(jsonContent["accountKey"] is JValue) && !(jsonContent["login"] is JValue) && !(jsonContent["password"] is JValue))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Bad json");
            }


            //var key = KeyParser((string)jsonContent["accountKey"]);

            //Account account = work.Accounts.GetAccount(key);
            //if (account == null)
            //{
            //    BadRequest();
            //}

            //if ( account.Role != UserRole.Admin)
            //{
            //    CreateErrorResponse(HttpStatusCode.Unauthorized, "Need admin permissions ");
            //}
            AccountKeyBuilder keyBuilder = new AccountKeyBuilder();
            var password =(string)jsonContent["password"];
            Account newAcc;
            using (var hasher = SHA256.Create())
            {
                 newAcc = new Account()
                {
                    Key = keyBuilder.CreateAccKey(),
                    Login = (string)jsonContent["login"],
                    Password = new HexString(hasher.ComputeHash
                    (DataProvider.Serializer.ToBinaryArray(password))),
                    Role = UserRole.Admin
                };
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, newAcc);
            Record record = new Record(1, "New acc "+ newAcc.Login,
                new HexString(stream.ToArray()), TypeData.Account);
            var pub_key = new ECKeyValidator().RSA.ToXmlString(false);

            new ECKeyValidator().RSA.PersistKeyInCsp = true;
            await work.TransactionValidator.ValidateTransaction( record , pub_key);
           
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

            Account user = this.work.Accounts.GetAccount(login);

            if (user == null)
            {
                CreateErrorResponse(HttpStatusCode.Unauthorized, "User not found.");
            }

            var hash_password = hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));
           
            if (!hash_password.SequenceEqual(user.Password.ToByteArray()))
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid password");
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
        [Route("record/{name}/{accountKey}")]
        public async  Task<object> GetRecord(string name, string accountKey)
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
           
            var result = await this.work.Records.GetRecordByNameAsync(name);
           
            if (result.TypeRecord == TypeData.Account)
            {

                Account acc = null;

                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream(result.Value.ToByteArray()))
                {
                   acc =  formatter.Deserialize(stream) as Account;
                }

                return JsonConvert.SerializeObject(new { acc.Login, acc.Role, result.Name, result.Version });

            }

            var serializedValue = Encoding.UTF8.GetString(result.Value.ToByteArray());

            return Json(new { result.Name, result.TypeRecord,
                              result.Version, serializedValue });

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

        [NonAction]
        public async Task<string> RawTransaction( byte[] transactionHash)
        {
            var tx = await this.work.Transactions.GetTransactionAsync(transactionHash);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }


            return GetHashString(tx.Hash.ToByteArray());
        }

        [NonAction]
        public async Task<Transaction> JsonTransaction(byte[] transactionHash)
        {

            var tx = await this.work.Transactions.GetTransactionAsync(transactionHash);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }
           
            return tx;
        }

        [HttpGet]
        [Route("api/main/block/{id}")]
        public async Task<string> GetBlock(int id)
        {
            var rawBlock = await this.work.Blocks.GetBlock(id);

            if (rawBlock == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Block is not found");
            }


            var formattedHash = GetHashString(rawBlock.Hash.ToByteArray());
            var prevHash = GetHashString(rawBlock.PreviousHash.ToByteArray());
            var merkle = GetHashString(rawBlock.MerkleRoot.ToByteArray());

            return JsonConvert.SerializeObject(new {
                formattedHash,
                rawBlock.Index,
                rawBlock.TimeStamp,
                prevHash,
                rawBlock.CurrentTransactions
            });
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

        private string GetHashString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }

        private async Task<object> SendChainFromWebSockets(AspNetWebSocketContext context)
        {
            var handler = new BlockChainHandler();

            byte[] buffer = new byte[1024 * 1024];
            ChainSerializer chainSerializer = new ChainSerializer();

            var encode_tuple = chainSerializer.Encode(connector.GetLocalChain().BlockChain);
            ArraySegment<byte> segment = 
                new ArraySegment<byte>(chainSerializer.ConcateByteArray(encode_tuple));

            WebSocket socket = context.WebSocket;

            try
            {
                handler.OnOpen();
                
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
