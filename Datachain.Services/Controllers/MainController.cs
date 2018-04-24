using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;
using DataChain.Services.Models;

namespace DataChain.Services.Controllers
{
    public class MainController : ApiController
    {
        private ITransactionSubscriber subscribe;

        public MainController(ITransactionSubscriber _subscribe)
        {
            subscribe = _subscribe;
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

        public void GetRecordByIndex(int blockIndex)
        {

        }

        public void AddRecord(string  trans)
        {
            
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

        public async Task<System.Web.Http.Results.JsonResult<Transaction>> RawTransaction(uint id)
        {
            var tx = await subscribe.GetTransactionAsync(id);

            if (tx == null)
            {
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "Transaction not found"
                });
            }

            var response = new Transaction(tx.Instance,tx.TimeStamp, tx.Data, tx.Hash);


            return Json( response);
        }
    

        public async Task<Transaction> JsonTransaction([FromBody]uint id)
        {

        var tx = await subscribe.GetTransactionAsync(id);

        if(tx == null)
        {
                throw new HttpResponseException(new System.Net.Http.HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "Transaction not found"
                });
        }

            var response = new Transaction(tx.Instance, tx.TimeStamp, tx.Data, tx.Hash);

            return response;
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
