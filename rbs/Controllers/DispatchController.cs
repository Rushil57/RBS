using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        // POST: api/Dispatch
        [HttpPost]
        public bool Post([FromBody] DispatchAgentRequest dispatch)
        {
            var agentAgent = new AgentAgent();
            if (dispatch.AgentBeingDispatched == -99999) //Tech Scheduled
            {
                return agentAgent.ScheduleTech(dispatch);
            }
            //else
            return agentAgent.DispatchAgent(dispatch);
        }

        [HttpGet]
        public IActionResult GetLeadsToDispatch()
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.GetLeadsToDispatch());
        }

        // GET: api/Dispatch/5
        [HttpGet("{leadId}")]
        public SchedApptDispatch Get(int leadId)
        {
            var leadAgent = new LeadAgent();
            return leadAgent.GetLeadForDispatchById(leadId);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LeadUpdateRequest updaterequest)
        {
            var success = true;
            var leadAgent = new LeadAgent();
            switch (updaterequest.PageRequestingUpdate)
            {
                case "SetLeadToFollowUp":
                    success = leadAgent.SetLeadToFollowUp(updaterequest);
                    break;
                case "SetLeadToSold":
                    success = leadAgent.SetLeadToSold(updaterequest);
                    break;
                case "SetLeadToInstalled":
                    success = leadAgent.SetLeadToInstalled(updaterequest);
                    break;
                case "UpdateLeadFromSADispatch":
                case "SetLeadToConfirm":
                case "SetLeadToUFS":
                case "SetLeadToNoShow":
                case "SetLeadToResched":
                case "MarkLeadNotInterested":
                    success = leadAgent.UpdateLeadFromSADispatch(updaterequest);
                    break;
            }

            return Ok(success);
        }
    }

}
