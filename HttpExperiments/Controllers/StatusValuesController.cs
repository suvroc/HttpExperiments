using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Ploeh.Hyprlinkr;

namespace HttpExperiments.Controllers
{
    [RoutePrefix("apiv1/Values")]
    public class StatusValuesController : ApiController
    {
        private static List<KeyValuePair<int, string>> values = new List<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(1, "value1"),
            new KeyValuePair<int, string>(2, "value2")
        };

        // GET: api/Values
        [HttpGet]
        [Route("", Name = "aaa")]
        public IEnumerable<KeyValuePair<int, string>> Get()
        {
            return values;
        }

        // GET: api/Values/5
        // KeyValuePair<int, string>
        [HttpGet]
        [Route("{id}", Name = "aaa2")]
        public HttpResponseMessage Get(HttpRequestMessage request, int id)
        {
            if (!values.Exists(x => x.Key == id))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, values.Find(x => x.Key == id));
        }

        // POST: api/Values
        // OK, Created, 
        // Header: Location; address of created resource
        [HttpPost]
        [Route("")]
        public HttpResponseMessage Post(HttpRequestMessage request, [FromBody] string value)
        {
            var key = values.Max(x => x.Key) + 1;
            values.Add(new KeyValuePair<int, string>(key, value));

            var response = Request.CreateResponse(HttpStatusCode.Created);
            // TODO: https://github.com/ploeh/Hyprlinkr

            var aaa = this.Url.GetLink<StatusValuesController>(a => a.Get(null, key));

            var uriString = this.Url.Link("DefaultApi", new { action = "Get", id = key });
            response.Headers.Location = new Uri(uriString);
            return response;
        }

        // PUT: api/Values/5
        [HttpPut]
        [Route("{id}")]
        //[Route("api/Values/{}")]
        public void Put(int id, [FromBody] string value)
        {
            if (values.Exists(x => x.Key == id))
            {
                values.RemoveAll(x => x.Key == id);
                values.Add(new KeyValuePair<int, string>(id, value));
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public HttpResponseMessage Delete(int id)
        {
            var deletedCount = values.RemoveAll(x => x.Key == id);

            if (deletedCount <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
