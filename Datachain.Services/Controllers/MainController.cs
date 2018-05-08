using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http.Results;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet;
using System.Configuration;
using DataChain.Infrastructures;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;
using DataChain.Services.Models;
using System.Security.Cryptography;
using System.Text;

namespace DataChain.Services.Controllers
{

    [System.Web.Http.Route("api/[controller]")]
    public class MainController : ApiController
    {
        private ITransactionSubscriber txSubscribe;
        private IRecordSubscriber recSubscribe;
        private IBlockSubscriber blcSubscribe;
        private TransactionValidator validator;

        public MainController(ITransactionSubscriber _subscribe, IRecordSubscriber _recSubscribe, TransactionValidator _validator)
        {
            txSubscribe = _subscribe;
            recSubscribe = _recSubscribe;
            validator = _validator;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("get")]
        public void Get()
        {
           // Transaction tx = null;
          
            WebSocketTransactionStream stream = new WebSocketTransactionStream(new Uri(this.Request.RequestUri.ToString()));
            HttpContext context = HttpContext.Current;
            stream.ProcessRequest(context);

        }

        public void GetAllRecords()
        {

        }

        [System.Web.Http.HttpGet]
        [ValidateAntiForgeryToken]
        [System.Web.Http.Route("authority")]
        public bool Authorize([FromBody]string login, [FromBody]string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException(nameof(login), "Логин не может быть пустым или равным null.");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Пароль не может быть пустым или равным null.");
            }
            SHA256 hasher = SHA256.Create();
            var hash_password = hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));

            return true;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("record/{key}")]
        public async Task<JsonResult<Record>> GetRecord([FromUri] string key)
        {
            HexString parseKey = KeyParser(key);
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast,31);
            Record result = await recSubscribe.GetRecordsByValueAsync(parseKey);

            return Json(result);

        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("recordbyindex/{blockIndex}")]
        public void GetRecordByIndex(int blockIndex)
        {

        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("commit")]
        public async Task<HexString> PostTransaction([FromBody]string key,
                                    [FromBody]string  trans)
        {

            if(string.IsNullOrEmpty(key) )
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Authentication failed");
            }       
            // IList<Signature> auth = new List<Signature>();
            var wssv = new WebSocketServer(4649);
           
            HexString parseKey = KeyParser(key);
            var block = await blcSubscribe.GetBlock(parseKey);
            

            JObject bodyContent = null;
            try
            {
                 bodyContent = JObject.Parse(trans);
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
                newTransaction = await validator.ValidateTransaction( bodyContent, key);
                txid = newTransaction.Hash;
            }
            catch(FormatException)
            {
                //do something
            }
            // catch()
  
            block.Metadata.CurrentTransactions.Add(newTransaction);

            return txid ?? HexString.Empty;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("tx/{id}")]
        public async Task<object> GetTransaction(uint id, DataFormat format = DataFormat.Json )
        {
            if(format == DataFormat.Json)
            {
               return await JsonTransaction(id);
            }

            return await RawTransaction(id);

        }

        public async Task<Transaction> RawTransaction(uint id)
        {
            var tx = await txSubscribe.GetTransactionAsync(id);

            if (tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }

            var response = new Transaction(tx.TimeStamp, tx.Data, tx.Hash, tx.Sign);


            return response;
        }
    

        public async Task<JsonResult<Transaction>> JsonTransaction([FromBody]uint id)
        {

            var tx = await txSubscribe.GetTransactionAsync(id);

            if(tx == null)
            {
                CreateErrorResponse(HttpStatusCode.NotFound, "Transaction is not found");
            }

            var response = new Transaction( tx.TimeStamp, tx.Data, tx.Hash, tx.Sign);

            return Json(response);
        }

        #region Вспомогательные методы
        private object GetRecordJson(Record records)
        {
            return new
            {
                version = records.Version,
                name  = records.Name,
                value  = records.Value
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
        #endregion
    }
}
