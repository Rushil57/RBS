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
    public class LoginController : ControllerBase
    {
        
        [HttpPost]
        public IActionResult LoginAgent(Agent agent)
        {
            return Ok(new AgentAgent().LoginAgent(agent));
        }
    }
}
