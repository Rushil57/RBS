using Microsoft.AspNetCore.Mvc;
using plweb.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : Controller
    {
        [HttpGet("GetAllAnnouncement/")]        
        public IActionResult GetAllAnnouncement()
        {
            var agents = new AnnouncementsAgent();
            return Ok(agents.GetAllAnnouncement());
        }
    }
}
