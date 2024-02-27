using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using plweb.Internals;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeclockController : ControllerBase
    {
        // GET: api/Timeclock/5
        [HttpGet("{agentId}", Name = "Get")]
        public TimeclockRequest Get(int agentId)
        {
            return new AgentAgent().GetTimeclockHistory(agentId);
        }

        // POST: api/Timeclock/Timeclock
        [HttpPost("Timeclock")]
        public IActionResult Post([FromBody] TimeclockAction action)
        {
            return Ok(new AgentAgent().InsertTimeclockAction(action));
        }

        //// PUT: api/Timeclock/5
        [HttpPut]
        public IActionResult Put(TimeclockAction timeclockAction)
        {
            return Ok(new AgentAgent().GetClockedInOutHistory(timeclockAction));
        }

        [HttpPost("PayReport")]
        public ActionResult PayReport([FromBody] TimeclockAction timeclockAction)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            AgentAgent agentAgent = new AgentAgent();
            var payReport = agentAgent.GetPayReport(timeclockAction);
            try
            {
                var memory = new MemoryStream();
                if (payReport != null)
                {
                    var path = Path.Combine(payReport.FilePath, payReport.FileName);


                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        stream.CopyTo(memory);
                    }
                    memory.Position = 0;
                    return File(memory, Util.GetContentType(payReport.FileFullPath), payReport.FileName);
                }
                return File(memory, "application/pdf", "");
            }
            catch (Exception ex)
            {
                Logger.Log("PLWEB.ERROR Timeclock.PayReport ", ex.Message + ex.StackTrace);
                return null;
            }
        }

        [HttpGet]
        public ContentResult ReportTest()
        {
            return Content("Test Result");
        }
    }

}
