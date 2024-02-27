using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawCountController : ControllerBase
    {
        [HttpGet]
        public IActionResult TotalRawLeads()
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.TotalRawLeads());
        }

        //api/RawCount/agentid
        [HttpGet("{agentid}")]
        public DialReportResponse GetAllAgents(int agentid)
        {
            var agents = new AgentAgent();
            return agents.GetDialReport(agentid);
        }
    }
}