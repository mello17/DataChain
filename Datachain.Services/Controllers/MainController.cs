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
            HexString parseKey = HexString.Empty;

            try
            {
                parseKey = HexString.Parse(key ?? "");
            }
            catch
            {
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Key is not valid"
                });
            }

            Record result = (await recSubscribe.GetRecordsByValueAsync(parseKey));

            return Json(result);

        }
        public void GetRecordByIndex(int blockIndex)
        {

        }

        [Route("commit")]
        [HttpPost]
        public Task PostTransaction([FromUri]string key, [FromUri]string  trans)
        {

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
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "Transaction not found"
                });
            }

            var response = new Transaction(tx.Instance,tx.TimeStamp, tx.Data, tx.Hash);


            return response;
        }
    

        public async Task<JsonResult<Transaction>> JsonTransaction([FromBody]uint id)
        {

        var tx = await txSubscribe.GetTransactionAsync(id);

        if(tx == null)
        {
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "Transaction not found"
                });
        }

            var response = new Transaction(tx.Instance, tx.TimeStamp, tx.Data, tx.Hash);

            return Json(response);
        }

        private object GetRecordJson(Record records)
        {
            return new
            {
                key  = records.Key,
                value  = records.Value
            };
        }
    }
}
