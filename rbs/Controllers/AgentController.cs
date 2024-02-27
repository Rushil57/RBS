using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllAgents()
        {
            var agents = new AgentAgent();
            return Ok(agents.GetAllAgents());
        }

        [HttpPost]
        public IActionResult GetAgentById(Agent agent)
        {
            var agents = new AgentAgent();
            return Ok(agents.GetAgentById(agent.AgentId));
        }
        //api/Agent/agentTypeId
        [HttpGet("{agentTypeId}")]
        public List<Agent> GetAllAgents(int agentTypeId)
        {
            var agents = new AgentAgent();
            return agents.GetAllAgentsByAgentTypeId(agentTypeId);
        }

        [HttpGet("{agentType}")]
        public List<Agent> GetAllAgentsByAgentTypeIds(string agentType)
        {
            var agents = new AgentAgent();
            return agents.GetAgentByType(agentType);
        }

        [HttpPut("GetPayReport/{payreport}")]
        public PayReport GetPayReport(PayReport payreport)
        {
            TimeclockAction timeclockAction = new TimeclockAction()
            {
                AgentId = payreport.AgentId,
                StartDate = payreport.FromDate,
                EndDate = payreport.ToDate
            };
            return new AgentAgent().GetPayReport(timeclockAction);
        }

        [HttpGet("AgentForIndex/")]
        public List<Agent> GetAllAgentsByAgentTypeIds()
        {
            var agents = new AgentAgent();
            return agents.GetAllActiveAgents();
        }

        [HttpGet("GetAllAgentType")]
        public List<AgentType> GetAllAgentType()
        {
            var agents = new AgentAgent();
            return agents.GetAllAgentType();
        }

        [HttpPost]
        [Route("CreateAgent")]
        public IActionResult CreateAgent(Agent agent)
        {
            var agentAgent = new AgentAgent();
            var result = agentAgent.CreateOrUpdateAgent(agent);
            return Ok(result.ToString());
        }

        [HttpPut]
        [Route("GetAllAgentsList")]
        public List<Agent> GetAllAgentsList([FromBody]SearchRequest search)
        {
            try
            {
                var accountAgent = new AgentAgent();
                List<Agent> agentData = accountAgent.GetAllAgentList();

                if (search.SearchField == "name")
                {
                    agentData = agentData.FindAll(m => m.FirstName.ToLower().Contains(search.SearchTerm.ToLower()) || m.LastName.ToLower().Contains(search.SearchTerm.ToLower()));
                }
                else if (search.SearchField == "phone")
                {
                    agentData = agentData.FindAll(m => m.Phone.ToLower().Contains(search.SearchTerm.ToLower()));
                }
                else if (search.SearchField == "email")
                {
                    agentData = agentData.FindAll(m => m.Email.ToLower().Contains(search.SearchTerm.ToLower()));
                }
                else if (search.SearchField == "agenttype")
                {
                    agentData = agentData.FindAll(m => m.AgentTypeName.ToLower().Contains(search.SearchTerm.ToLower()));
                }
                else if (search.SearchField == "status")
                {
                    if (search.SearchTerm.ToLower() == "active")
                    {
                        agentData = agentData.FindAll(m => m.Status == true);
                    }
                    else
                    {
                        agentData = agentData.FindAll(m => m.Status == false);
                    }

                }
                return agentData;
            }
            catch (Exception e)
            {
                Logger.Log("AccountController.GetAllAgentsList calling api", e.Message + e.StackTrace);
                return null;
            }

        }

        [HttpGet]
        [Route("GetAgentById/{agentId}")]
        public IActionResult GetAgentById(int agentId)
        {
            var agents = new AgentAgent();
            return Ok(agents.GetAgentById(agentId));
        }

        [HttpGet]
        [Route("GetAgentStatusById/{agentId}")]
        public IActionResult GetAgentStatusById(int agentId)
        {
            bool IsStatus = false;
            var agents = new AgentAgent();
            var data = agents.GetAgentStatusById(agentId);
            if (data.Status == true)
            {
                IsStatus = true;
            }
            else
            {
                IsStatus = false;
            }
            return Ok(IsStatus);
        }


        [HttpPost]
        [Route("InsertUserOtherInfo")]
        public IActionResult InsertUserOtherInfo(Agent agent)
        {
            var agentAgent = new AgentAgent();
            //var result = agentAgent.CreateOrUpdateAgent(agent);
            return Ok();
        }
    }
}