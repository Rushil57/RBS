using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Win32;
using Newtonsoft.Json;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

public class SMSAgent
{
    public const string TwilioFieldRepPhone = "+14805315005";
    public const string TwilioTechPhone = "+14804000754";
    public const string TwilioLeadPhone = "+14804000271";
    //Twilio Credentials
    private const string _accountsid = "AC00ccbda37c9620b827d19864caf9191f";
    private const string _authtoken = "97a1b40e95e1966d51cff12bdd531838";
    private static MySqlDataAgent mySqldataAgent = new MySqlDataAgent();

    public bool SendSMS(SMSMessage message)
    {
        var success = true;
        try
        {
            string phone = string.Empty;
            if (message.agentId > 0)
            {
                if (message.AgentTypeId == 10)
                {
                    var Lead = new LeadAgent().GetLeadById(message.agentId);
                    if (Lead.Phone1Status == 3)
                    {
                        phone = Lead.Phone1;
                    }
                    else if (Lead.Phone2Status == 3)
                    {
                        phone = Lead.Phone2;
                    }
                    else if (Lead.Phone3Status == 3)
                    {
                        phone = Lead.Phone3;
                    }
                    else if (Lead.Phone4Status == 3)
                    {
                        phone = Lead.Phone4;
                    }
                    else if (Lead.Phone5Status == 3)
                    {
                        phone = Lead.Phone5;
                    }
                    else
                    {
                        phone = Lead.Phone1;
                    }
                    phone = phone.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "").Replace(".", "");
                    if (phone.IndexOf("+1") == -1)
                    {
                        phone = "+1" + phone;
                    }
                    message.ToPhone = phone;
                    message.FromPhone = TwilioLeadPhone;

                    //check from number in lead table 
                    Lead lead = mySqldataAgent.GetLeadByPhone(message.ToPhone);
                    if (lead != null)
                    {
                        message.LeadId = lead.LeadId;
                    }
                }
                else
                {
                    var agent = new AgentAgent().GetAgentById(message.agentId);
                    phone = agent.Phone;

                    phone = phone.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "").Replace(".", "");
                    if (phone.IndexOf("+1") == -1)
                    {
                        phone = "+1" + phone;
                    }
                    message.ToPhone = phone;
                    if (agent.AgentTypeId == 20)
                    {
                        message.FromPhone = TwilioFieldRepPhone;
                    }
                    else if (agent.AgentTypeId == 50)
                    {
                        message.FromPhone = TwilioTechPhone;
                    }
                    else if (agent.AgentTypeId == 10 || agent.AgentId == 8)
                    {
                        message.FromPhone = TwilioLeadPhone;
                    }
                    else
                    {
                        Logger.Log("SMSAgent.SendSMS PLWEB.ERROR",
                            $"We shouldn't be here.  AgentTypeId: {agent.AgentTypeId}, {JsonConvert.SerializeObject(message)}");
                        message.FromPhone = TwilioLeadPhone;
                    }
                }
            }
            else
            {                
                message.FromPhone = TwilioLeadPhone;
                phone = message.ToPhone;
                //check from number in lead table 
                var FromPhone = message.FromPhone.Trim().Replace("+1", "");
                FromPhone = (FromPhone.StartsWith("1")) ? FromPhone.Substring(1) : FromPhone;
                if (FromPhone == TwilioLeadPhone.Replace("+1", ""))
                {
                    Lead lead = mySqldataAgent.GetLeadByPhone(message.ToPhone);
                    if (lead != null)
                    {
                        message.LeadId = lead.LeadId;
                    }
                }
            }

            message.ToPhone = phone;
            mySqldataAgent.InsertSMS(message);
            success = TwilioSendSMS(message);
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.SendSMS PLWEB.ERROR", error);
            success = false;
        }

        return success;
    }

    public bool TwilioSendSMS(SMSMessage message)
    {
        var success = true;
        try
        {
            TwilioClient.Init(_accountsid, _authtoken);

            var tMessage = MessageResource.Create(
                body: message.Body,
                @from: new Twilio.Types.PhoneNumber(message.FromPhone),
                to: new Twilio.Types.PhoneNumber(message.ToPhone)
            );

            var response = JsonConvert.SerializeObject(tMessage);
            if (tMessage.Status != MessageResource.StatusEnum.Failed
                && tMessage.Status != MessageResource.StatusEnum.Undelivered)
            {
                Lead lead = mySqldataAgent.GetLeadByPhone(message.ToPhone);
                Account account = mySqldataAgent.GetAccountByPhone(message.ToPhone);

                //build note
                var _leadid = -1;
                if (lead != null && lead.LeadId > 0)
                {
                    _leadid = lead.LeadId;
                    //todo - insert sms-audit
                }

                var _accountid = -1;
                if (account != null && account.AccountId > 0)
                {
                    _accountid = account.LeadId;
                    //todo - insert sms-audit
                }

                var note = new Note()
                {
                    LeadId = _leadid,
                    LeadStatusId = -1,
                    AccountId = _accountid,
                    AgentId = 0,
                    NoteText = $"SMS sent to {message.ToPhone}, body: {message.Body}"
                };
                //submit note
                return new NotesAgent().SaveNote(note);
            }
            else
            {
                Logger.Log("SMSAgent.SendSMS PLWEB.ERROR", response);
            }
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.SendSMS PLWEB.ERROR", error);
            success = false;
        }

        return success;
    }

    public MessagingResponse RecieveSMS(string body)
    {
        var messagingResponse = new MessagingResponse();
        Lead lead = new Lead();
        try
        {
            Logger.Log("SMSAgent.RecieveSMS", $"inside RecieveSMS - body:{body}");
            var sms = MapBodyToSMSMessage(body);
            sms.Body = sms.Body.Replace("'", "2%2");
            Logger.Log("SMSAgent.RecieveSMS", "before insert");
            try
            {
                if (!string.IsNullOrEmpty(sms.FileUrl))
                {
                    string contentType = "image/jpeg";
                    string filename = Guid.NewGuid().ToString() + ".jpg";
                    var filePath = GetMediaFileName(sms.FileUrl, contentType, filename);
                    DownloadUrlToFileAsync(sms.FileUrl, filePath).Wait();
                    sms.FileUrl = filename;
                }
            }
            catch (Exception err)
            {
                var error = err.Message + err.StackTrace;
                Logger.Log("SMSAgent.RecieveSMS_Media PLWEB.ERROR", error);
            }

            //check from number in lead table 
            var toPhone = sms.ToPhone.Trim().Replace("+1", "");
            toPhone = (toPhone.StartsWith("1")) ? toPhone.Substring(1) : toPhone;
            if (toPhone == TwilioLeadPhone.Replace("+1", ""))
            {
                var mySqldataAgent = new MySqlDataAgent();
                lead = mySqldataAgent.GetLeadByPhone(sms.FromPhone);
                if (lead != null)
                {
                    sms.LeadId = lead.LeadId;
                }
            }
            //insert incoming message
            var dataAgent = new MySqlDataAgent();
            dataAgent.InsertSMS(sms);

            if (sms.Body != null && sms.Body.ToLower() == "stop")
            {
                //mark the lead as SMS-STOP
                lead.LeadStatusId = 19;
                dataAgent.UpdateLead(lead);
                Logger.Log("SMSAgent.RecieveSMS SMS-STOP", $"Marked SMS-STOP {sms.FromPhone}");
            }

            Logger.Log("SMSAgent.RecieveSMS", "after insert");
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.RecieveSMS PLWEB.ERROR", error);
        }
        return messagingResponse;
    }

    public SMSMessage MapBodyToSMSMessage(string body)
    {
        var parsedBody = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(body));

        return new SMSMessage()
        {
            Body = parsedBody["Body"],
            FromPhone = parsedBody["From"]?.Replace("+1", ""),
            ToPhone = parsedBody["To"]?.Replace("+1", ""),
            MessageStatus = parsedBody["SmsStatus"],
            AgentSentBy = 0,
            agentId = 0,
            FileUrl = parsedBody["MediaUrl"] != null ? parsedBody["MediaUrl"] : parsedBody["MediaUrl0"]
        };

        //var parsedBody = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(body));
        //var dataAgent = new MySqlDataAgent();

        //List<Agent> agent = dataAgent.GetAllAgents();
        //int ReceiverAgentId = agent.Find(a => a.Phone == parsedBody["To"]).AgentId;
        //int? SenderAgentId = agent.Find(a => a.Phone == parsedBody["From"]).AgentId;
        //int sAjentId = 0;
        //if (SenderAgentId!=null)
        //{
        //    sAjentId = Convert.ToInt32(SenderAgentId);
        //}

        //return new SMSMessage()
        //{
        //    Body = parsedBody["Body"],
        //    FromPhone = parsedBody["From"]?.Replace("+1", ""),
        //    ToPhone = parsedBody["To"]?.Replace("+1", ""),
        //    MessageStatus = parsedBody["SmsStatus"],
        //    AgentSentBy = sAjentId,
        //    agentId = ReceiverAgentId,
        //};
    }

    public List<SMSMessage> GetAllSMSForAgent(int agentId)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllSmsMessagesByAgentId(agentId, 0, null);
    }

    public List<SMSMessage> GetAllSMSForAgentLetest(int agentId, int loginagentId, int agentTypeId, int? messageId = null, int? agentTypeIds = null)
    {
        var dataAgent = new MySqlDataAgent();

        string agentPhone = string.Empty;
        if (agentTypeId == 20)
        {
            agentPhone = TwilioFieldRepPhone;
        }
        else if (agentTypeId == 50)
        {
            agentPhone = TwilioTechPhone;
        }
        else if (agentTypeId == 10)
        {
            agentPhone = TwilioLeadPhone;
        }
        var data = dataAgent.GetAllSmsMessagesByAgentId(agentId, loginagentId, agentPhone, messageId, agentTypeIds);
        return data;
    }

    public List<SMSMessage> GetSMSLHS(int agentTypeId)
    {
        var dataAgent = new MySqlDataAgent();
        string agentPhone = string.Empty;
        if (agentTypeId == 20)
        {
            agentPhone = TwilioFieldRepPhone;
        }
        else if (agentTypeId == 50)
        {
            agentPhone = TwilioTechPhone;
        }
        else if (agentTypeId == 10)
        {
            agentPhone = TwilioLeadPhone;
        }
        return dataAgent.GetSMSLHS(agentTypeId, agentPhone);
    }

    public string GetAllMessages(SMSMessage message)
    {
        string strMessage = "";
        try
        {


            var dataAgent = new MySqlDataAgent();
            List<SMSMessage> SMSData = dataAgent.GetAllMessages();
            message.ToPhone = message.ToPhone.Replace("-", "").TrimEnd(',');
            var TophoneList = message.ToPhone.Split(',');
            string TwilioLeadPhoneNumber = TwilioLeadPhone.Replace("+1", "");
            bool IsReply = false;

            foreach (var tophone in TophoneList)
            {

                var data1 = SMSData.FindAll(a => a.ToPhone == tophone && a.FromPhone == TwilioLeadPhoneNumber);
                if (data1.Count > 0)
                {
                    var data = SMSData.FindAll(a => a.FromPhone == tophone && a.ToPhone == TwilioLeadPhoneNumber && (a.Body.Contains("1") || a.Body.ToLower().Contains("yes")));
                    if (data.Count > 0)
                    {
                        IsReply = true;
                        strMessage = "confirmed";
                        break;
                    }
                    strMessage = "Waiting";
                }
                else
                {
                    strMessage = "No Message";
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log("SMSAgent.GetAllMessages: PLWEB.ERROR", e.Message + e.StackTrace);
            return "";
        }
        return strMessage;
    }

    public string GetSMSNotification()
    {
        string strNotify = "";
        try
        {
            var dataAgent = new MySqlDataAgent();
            List<SMSMessage> lstmsg = dataAgent.GetSMSNotification();
            if (lstmsg != null)
            {
                lstmsg = lstmsg.FindAll(a => a.isread == false);
                if (lstmsg.Count > 0)
                {
                    var TechNotify = lstmsg.FindAll(a => a.ToPhone == TwilioTechPhone.Replace("+1", ""));
                    if (TechNotify.Count > 0)
                    {
                        strNotify = "Tech,";
                    }
                    var FieldNotify = lstmsg.FindAll(a => a.ToPhone == TwilioFieldRepPhone.Replace("+1", ""));
                    if (FieldNotify.Count > 0)
                    {
                        strNotify += "FieldRep,";
                    }
                    var LeadNotify = lstmsg.FindAll(a => a.ToPhone == TwilioLeadPhone.Replace("+1", ""));
                    if (LeadNotify.Count > 0)
                    {
                        strNotify += "Lead,";
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log("SMSAgent.GetSMSNotification: PLWEB.ERROR", e.Message + e.StackTrace);
            return "";
        }
        return strNotify;
    }

    public List<SMSMessage> GetAllSmsHistorybyAgent(int agentId, int loginagentId, int agentTypeId, int? messageId = null)
    {
        var dataAgent = new MySqlDataAgent();

        string agentPhone = string.Empty;
        if (agentTypeId == 20)
        {
            agentPhone = TwilioFieldRepPhone;
        }
        else if (agentTypeId == 50)
        {
            agentPhone = TwilioTechPhone;
        }
        else if (agentTypeId == 10)
        {
            agentPhone = TwilioLeadPhone;
        }
        var data = dataAgent.GetAllSmsHistorybyAgent(agentId, loginagentId, agentPhone, messageId, agentTypeId);
        return data;
    }

    #region Media SMS

    public string GetMediaFileName(string mediaUrl,
        string contentType, string filename)
    {
        try
        {
            string SavePath = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Util.filefolderpath + "SMSMedia\\" : Util.filefolderpath + "SMSMedia/";

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            return SavePath +
                filename +
                GetDefaultExtension(contentType);
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.GetMediaFileName PLWEB.ERROR", error);
            return null;
        }


    }

    public static async Task DownloadUrlToFileAsync(string mediaUrl,
        string filePath)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(mediaUrl);
                var httpStream = await response.Content.ReadAsStreamAsync();
                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await httpStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.DownloadUrlToFileAsync PLWEB.ERROR", error);
        }

    }

    public static string GetDefaultExtension(string mimeType)
    {
        // NOTE: This implementation is Windows specific (uses Registry)
        // Platform independent way might be to download a known list of
        // mime type mappings like: http://bit.ly/2gJYKO0

        try
        {
            var key = Registry.ClassesRoot.OpenSubKey(
          @"MIME\Database\Content Type\" + mimeType, false);
            var ext = key?.GetValue("Extension", null)?.ToString();

            return ext ?? "application/octet-stream";
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("SMSAgent.GetDefaultExtension PLWEB.ERROR", error);
            return "";
        }

    }
    #endregion
}
