using Microsoft.AspNetCore.Mvc;
using System;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternalSalesStatsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStats()
        {
            var stateAgent = new StatsAgent();
            return Ok(stateAgent.GetStats());
        }
        [HttpPut]
        public IActionResult TotalDailPerDay(InternalSalesStats salesStats)
        {
            var stateAgent = new StatsAgent();
            return Ok(stateAgent.GenerateReport(salesStats));
        }

        [HttpPost]
        public IActionResult Post(Inventory inventory)
        {
           return Ok(new InventoryAgent().InventoryReport(inventory.FromDate, inventory.ToDate));
        }
    }
}