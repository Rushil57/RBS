namespace plweb.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class LeadController : ControllerBase
    {
        // GET: api/Lead/5
        [HttpGet("{leadId}")]
        public Lead Get(int leadId)
        {
            var leadAgent = new LeadAgent();
            return leadAgent.GetLeadById(leadId);
        }

        [HttpGet]
        public IActionResult GetRandomLead()
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.GetRandomLead());
        }

        [HttpGet("GetRandomTOLead/")]
        public IActionResult GetRandomTOLead()
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.GetRandomTOLead());
        }

        [HttpGet("GetCalendarLeads/")]
        public IActionResult GetCalendarLeads()
        {
            var calAgent = new CalendarAgent();
            return Ok(calAgent.GetCalendarLeads());
        }

        [HttpGet("GetCalendarSalesClosedLeads/")]
        public IActionResult GetCalendarSalesClosedLeads()
        {
            var calAgent = new CalendarAgent();
            return Ok(calAgent.GetCalendarSalesClosedLeads());
        }

        
        // PUT: api/Lead
        [HttpPut]
        public List<Lead> Put([FromBody]SearchRequest search)
        {
            var leadAgent = new LeadAgent();
            return leadAgent.GetLeadsByField(search.SearchTerm, search.SearchField);
        }

        //// POST: api/Lead
        [HttpPost]
        [Route("UpdateLead")]
        public IActionResult UpdateLead(LeadUpdateRequest leadtoUpdate)
         {
            try
            {
                var dataagent = new MySqlDataAgent();

                var agent = dataagent.GetAgentById(leadtoUpdate.LeadToUpdate.AgentIdSubmitting);
                if (agent != null)
                {
                    int agenttypeId = agent.AgentTypeId;
                    if (agenttypeId == 11)
                    {
                        var leadAddress = dataagent.SearchLeadByAddress(leadtoUpdate.LeadToUpdate.Address.TrimEnd());
                        if (leadAddress.Count > 0)
                        {
                            string strmsg = "IsExist";
                            return new JsonResult(strmsg);                          
                        }
                    }
                }

                Logger.Log("LeadController.UpdateLead start LEAD BUG", $"LeadController.UpdateLead  start");
                var updateLead = new LeadAgent();
                var success = updateLead.CreateOrUpdateLead(leadtoUpdate);
                Logger.Log("LeadController.UpdateLead end LEAD BUG", $"LeadController.UpdateLead  end {success}");
                return Ok(success);
            }
            catch (Exception e)
            {
                Logger.Log("PLWEB.ERROR Lead.UpdateLead ", e.Message + e.StackTrace);
                return Ok(false);
            }
        }

        [HttpPut]
        [Route("SendLeadSMS")]
        public IActionResult SendLeadSMS(SMSMessage message)
        {
            var smsAgent = new SMSAgent();
            SMSMessage smsMessage = new SMSMessage();
            message.ToPhone = message.ToPhone.Replace("-", "").TrimEnd(',');
            var TophoneList = message.ToPhone.Split(',');
            foreach (var tophone in TophoneList)
            {
                smsMessage = new SMSMessage();
                smsMessage.AgentSentBy = message.AgentSentBy;
                smsMessage.ToPhone = tophone;
                smsMessage.Body = message.Body;
                smsMessage.LeadId = message.LeadId;
                smsMessage.agentId = 0;
                smsAgent.SendSMS(smsMessage);
            }
            //smsAgent.SendSMS(message)
            return Ok(smsMessage);
        }

        [HttpPost]
        [Route("GetResponse")]
        public IActionResult GetResponse(SMSMessage message)
        {
            var smsAgent = new SMSAgent();

            string strResponse = smsAgent.GetAllMessages(message);

            return Ok(strResponse);
        }

        [HttpGet("GetRepLeads/{agentId}/{password}")]
        public IActionResult GetRepLeads(int agentId, string password)
        {
            var leadAgent = new LeadAgent();
            return Ok(leadAgent.GetRepLeads(agentId, password));
        }
    }
}
