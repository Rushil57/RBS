namespace plweb.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        //// GET: api/Lookup
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Lookup/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Lookup
        [HttpPost]
        public LookupResponse Post([FromBody] Lead lead)
        {
            //var test = Util.GetResourceFileAsString("michael_renkwitz.txt");
            //return new PiplAgent().LookupPipl(lead, test);
            //prod below
            return new PiplAgent().LookupPipl(lead);
        }

        //// PUT: api/Lookup/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
