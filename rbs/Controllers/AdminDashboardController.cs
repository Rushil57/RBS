using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        //// GET: api/Dashboard
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Dashboard/5
        //[HttpGet("{request}", Name = "Get")]
        //public DashboardResponse Get(DashboardRequest request)
        //{

        //}

        //// POST: api/Dashboard
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT: api/Dashboard/
        [HttpPut]
        public DashboardResponse Put([FromBody] DashboardRequest request)
        {
            return new AgentAgent().GetDashboard(request);
        }
       
    }
}
