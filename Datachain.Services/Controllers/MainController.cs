using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DataChain.Infrastructures;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;
using DataChain.Services.Models;

namespace DataChain.Services.Controllers
{
    public class MainController : ApiController
    {
        private ITransactionSubscriber txSubscribe;
        private IRecordSubscriber recSubscribe;

        public MainController(ITransactionSubscriber _subscribe, IRecordSubscriber _recSubscribe)
        {
            txSubscribe = _subscribe;
            recSubscribe = _recSubscribe;
        }

        public async Task<Transaction> Broadcast()
        {
            Transaction tx = null;
          
            WebSocketTransactionStream stream = new WebSocketTransactionStream();
            return  tx;
           

        }

        public void GetAllRecords()
        {

        }

        [HttpGet]
        [Route("record")]
        public async Task<JsonResult<Record>> GetRecord([FromUri] string key)
        {
           HexString parseKey = KeyParser(key);

            Record result = (await recSubscribe.GetRecordsByValueAsync(parseKey));

            return Json(result);

        }
        public void GetRecordByIndex(int blockIndex)
        {

        }

        [Route("commit")]
        [HttpPost]
        public Task PostTransaction([FromBody]string key,
                                    [FromBody]string sign, 
                                    [FromBody]string  trans)
        {

            if(key == String.Empty || sign == String.Empty)
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Authentication failed");
            }
           // IList<Signature> auth = new List<Signature>();

            HexString parseKey = KeyParser(key);
            JObject bodyContent;
            try
            {
                 bodyContent = JObject.Parse(trans);
            }

            catch (JsonReaderException)
            {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Bad json");
            }

            Signature auth = new Signature(HexString.Parse(sign), HexString.Parse(key));

            try
            {
                auth.VerifyMessage((string)bodyContent["record"]);
            }




           

            return;
        }

        [Route("tx/{id}")]
        [HttpGet]
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
                CreateErrorResponse(HttpStatusCode.BadRequest, "Transaction is not found");
            }

            var response = new Transaction(tx.Instance,tx.TimeStamp, tx.Data, tx.Hash);


            return response;
        }
    

        public async Task<JsonResult<Transaction>> JsonTransaction([FromBody]uint id)
        {

        var tx = await txSubscribe.GetTransactionAsync(id);

        if(tx == null)
        {
                CreateErrorResponse(HttpStatusCode.BadRequest, "Transaction is not found");
            }

            var response = new Transaction(tx.Instance, tx.TimeStamp, tx.Data, tx.Hash);

            return Json(response);
        }

        #region Вспомогательные методы
        private object GetRecordJson(Record records)
        {
            return new
            {
                key  = records.Key,
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
