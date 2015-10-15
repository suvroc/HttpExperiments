using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Caching;
using System.Web.Http;

namespace HttpExperiments.Controllers
{
    public class CacheController : ApiController
    {
        [Route("getDate")]
        public string GetDate()
        {
            return DateTime.Now.ToString();
        }
    }
}
