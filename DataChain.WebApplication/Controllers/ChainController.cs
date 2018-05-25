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

namespace DataChain.WebApplication.Controllers
{
    public class ChainController : ApiController
    {

        public int GetIndex(int id)
        {
            return id;
        }
    }
}
