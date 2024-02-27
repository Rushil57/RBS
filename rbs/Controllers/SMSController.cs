namespace plweb.Controllers
{
    using System.IO;
    using Twilio.TwiML;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Twilio.AspNet.Core;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Twilio.AspNet.Common;
    using System.Diagnostics;
    using Microsoft.Win32;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : TwilioController
    {
        // GET: api/SMS/5
        [HttpGet("{techId}")]
        public List<SMSMessage> Get(int techId)
        {
            return new SMSAgent().GetAllSMSForAgentLetest(techId, 0, 0);
        }

        //Temp
        [HttpGet("GetLetest/{agentid}/{loginagentId}/{agentTypeId}/{messageId}/{agentTypeIds}")]
        public List<SMSMessage> GetLetest(int agentid, int loginagentId, int agentTypeId, int? messageId, int? agentTypeIds)
        {
            if (messageId == 0)
            {
                messageId = null;
            }
            return new SMSAgent().GetAllSMSForAgentLetest(agentid, loginagentId, agentTypeId, messageId, agentTypeIds);
        }

        [HttpGet("GetSMSLHS/{agentTypeId}")]
        public List<SMSMessage> GetSMSLHS(int agentTypeId)
        {
            return new SMSAgent().GetSMSLHS(agentTypeId);
        }

        // POST: api/SMS
        [HttpPost]
        public TwiMLResult Post(string incomingMessage)
        {
            Logger.Log("SmsController.Post", $"Start {incomingMessage}");
            var messagingResponse = new MessagingResponse();

            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();
                Logger.Log("SmsController.Post", $"body {body}");
                messagingResponse = new SMSAgent().RecieveSMS(body);
            }

            Logger.Log("SmsController.Post", $"before response {messagingResponse.ToString()}");
            return TwiML(messagingResponse);
        }

        [HttpPut]
        [Route("SendSMSWithAttechment/")]
        public IActionResult SendSMSWithAttechment(IFormFile file)
        {
            try
            {
                var smsAgent = new SMSAgent();
                SMSMessage message = new SMSMessage();

                if (file.Length > 0)
                {
                    var agentSentBy = Convert.ToInt32(Request.Form["agentSentBy"].ToString());
                    var Body = Convert.ToString(Request.Form["Body"].ToString());
                    var SmsMessageSid = Convert.ToString(Request.Form["SmsMessageSid"].ToString());
                    var AgentTypeId = Convert.ToInt32(Request.Form["AgentTypeId"].ToString());
                    var agentId = Convert.ToInt32(Request.Form["agentId"].ToString());                    
                    string filename = Guid.NewGuid().ToString() + ".jpg";

                    message.AgentSentBy = agentSentBy;
                    message.Body = Body;
                    message.SmsMessageSid = SmsMessageSid;
                    message.AgentTypeId = AgentTypeId;
                    message.FileUrl = filename;
                    message.agentId = agentId;
                    string SavePath = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Util.filefolderpath + "SMSMedia\\" : Util.filefolderpath + "SMSMedia/";
                   // string SavePath = Util.filefolderpath + "SMSMedia\\";
                    if (!Directory.Exists(SavePath))
                    {
                        Directory.CreateDirectory(SavePath);
                    }
                 
                    string fullpath = SavePath + filename;                                     
                    using (var fileStream = new FileStream(fullpath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }                    
                }

                return Ok(smsAgent.SendSMS(message));
            }
            catch (Exception e)
            {
                Logger.Log("SMSController.SendSMSWithAttechment ", e.Message + e.StackTrace);
                return null;
            }

        }

        [HttpPut]
        [Route("SendSMS/")]
        public IActionResult SendSMS(SMSMessage message)
        {
            try
            {
                var smsAgent = new SMSAgent();
                return Ok(smsAgent.SendSMS(message));

            }
            catch (Exception e)
            {
                Logger.Log("SMSController.SendSMS ", e.Message + e.StackTrace);
                return null;
            }

        }

        [HttpGet("GetSMSNotification")]
        public string GetSMSNotification()
        {
            string Notification = "";
            Notification = new SMSAgent().GetSMSNotification();

            return Notification;
        }

        //public async Task<TwiMLResult> ReceivedSMS(SmsRequest request, int numMedia)
        //{
        //    for (var i = 0; i < numMedia; i++)
        //    {
        //        var mediaUrl = Request.Form[$"MediaUrl{i}"];
        //        //Trace.WriteLine(mediaUrl);
        //        var contentType = Request.Form[$"MediaContentType{i}"];
        //        var smsAgent = new SMSAgent();

        //        var filePath = smsAgent.GetMediaFileName(mediaUrl, contentType);
        //        await SMSAgent.DownloadUrlToFileAsync(mediaUrl, filePath);
        //    }

        //    var response = new MessagingResponse();
        //    var body = numMedia == 0 ? "Send us an image!" :
        //        $"Thanks for sending us {numMedia} file(s)!";
        //    response.Message(body);
        //    return TwiML(response);
        //}

        [HttpGet("GetAllSmsHistorybyAgent/{agentid}/{loginagentId}/{agentTypeId}/{messageId}")]
        public List<SMSMessage> GetAllSmsHistorybyAgent(int agentid, int loginagentId, int agentTypeId, int? messageId)
        {
            if (messageId == 0)
            {
                messageId = null;
            }
            return new SMSAgent().GetAllSmsHistorybyAgent(agentid, loginagentId, agentTypeId, messageId);
        }
    }
}
