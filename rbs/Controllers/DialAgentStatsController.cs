namespace plweb.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class DialAgentStatsController : ControllerBase
    {
        // GET: api/DialAgentStats
        [HttpGet]
        public IEnumerable<DialAgentStats> Get()
        {
            return new StatsAgent().GetDialAgentStats();
        }
        
        //// GET: api/DialAgentStats/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/DialAgentStats
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT: api/DialAgentStats/5
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
