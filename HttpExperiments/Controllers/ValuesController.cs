using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HttpExperiments.Controllers
{
    public class ValuesController : ApiController
    {
        static List<KeyValuePair<int, string>> values = new List<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(1, "value1"),
            new KeyValuePair<int, string>(2, "value2")
        };

        // GET: api/Values
        public IEnumerable<KeyValuePair<int, string>> Get()
        {
            return values;
        }

        // GET: api/Values/5
        public KeyValuePair<int, string> Get(int id)
        {
            return values.Find(x => x.Key == id);
        }

        // POST: api/Values
        public void Post([FromBody]string value)
        {
            values.Add(new KeyValuePair<int, string>(values.Max(x => x.Key)+1, value));
        }

        // PUT: api/Values/5
        [HttpPut]
        //[Route("api/Values/{}")]
        public void Put(int id, [FromBody]string value)
        {
            if (values.Exists(x => x.Key == id))
            {
                values.RemoveAll(x => x.Key == id);
                values.Add(new KeyValuePair<int, string>(id, value));
            }
        }

        // DELETE: api/Values/5
        public void Delete(int id)
        {
            values.RemoveAll(x => x.Key == id);
        }
    }
}
