using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepDispatchController : ControllerBase
    {
        // GET: api/RepDispatch
        [HttpGet]
        public IActionResult GetLeadsToDispatch()
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.GetConfirmedLeadsToDispatch());
        }

        //// GET: api/RepDispatch/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/RepDispatch
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT: api/RepDispatch/5
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
