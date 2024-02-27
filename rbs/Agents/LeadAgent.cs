using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LeadAgent
{
    public bool CreateOrUpdateLead(LeadUpdateRequest leadToUpdate)
    {
        Logger.Log("LeadAgent.CreateOrUpdateLead start LEAD BUG", $"LeadAgent.CreateOrUpdateLead start");
        var success = true;
        var dataagent = new MySqlDataAgent();

        leadToUpdate.LeadToUpdate.Phone1 = (leadToUpdate.LeadToUpdate.Phone1 == null) ? null : leadToUpdate.LeadToUpdate.Phone1.Replace("-", "");
        leadToUpdate.LeadToUpdate.Phone2 = (leadToUpdate.LeadToUpdate.Phone2 == null) ? null : leadToUpdate.LeadToUpdate.Phone2.Replace("-", "");
        leadToUpdate.LeadToUpdate.Phone3 = (leadToUpdate.LeadToUpdate.Phone3 == null) ? null : leadToUpdate.LeadToUpdate.Phone3.Replace("-", "");
        leadToUpdate.LeadToUpdate.Phone4 = (leadToUpdate.LeadToUpdate.Phone4 == null) ? null : leadToUpdate.LeadToUpdate.Phone4.Replace("-", "");
        leadToUpdate.LeadToUpdate.Phone5 = (leadToUpdate.LeadToUpdate.Phone5 == null) ? null : leadToUpdate.LeadToUpdate.Phone5.Replace("-", "");

        leadToUpdate.LeadToUpdate.Phone1Status = (leadToUpdate.LeadToUpdate.Phone1Status == null) ? 0 : (leadToUpdate.LeadToUpdate.Phone1 == "" ? 0 : leadToUpdate.LeadToUpdate.Phone1Status);
        leadToUpdate.LeadToUpdate.Phone2Status = (leadToUpdate.LeadToUpdate.Phone2Status == null) ? 0 : (leadToUpdate.LeadToUpdate.Phone2 == "" ? 0 : leadToUpdate.LeadToUpdate.Phone2Status);
        leadToUpdate.LeadToUpdate.Phone3Status = (leadToUpdate.LeadToUpdate.Phone3Status == null) ? 0 : (leadToUpdate.LeadToUpdate.Phone3 == "" ? 0 : leadToUpdate.LeadToUpdate.Phone3Status);
        leadToUpdate.LeadToUpdate.Phone4Status = (leadToUpdate.LeadToUpdate.Phone4Status == null) ? 0 : (leadToUpdate.LeadToUpdate.Phone4 == "" ? 0 : leadToUpdate.LeadToUpdate.Phone4Status);
        leadToUpdate.LeadToUpdate.Phone5Status = (leadToUpdate.LeadToUpdate.Phone5Status == null) ? 0 : (leadToUpdate.LeadToUpdate.Phone5 == "" ? 0 : leadToUpdate.LeadToUpdate.Phone5Status);

        leadToUpdate.LeadToUpdate.Phone1Dial = (leadToUpdate.LeadToUpdate.Phone1Dial == null) ? 0 : leadToUpdate.LeadToUpdate.Phone1Dial;
        leadToUpdate.LeadToUpdate.Phone2Dial = (leadToUpdate.LeadToUpdate.Phone2Dial == null) ? 0 : leadToUpdate.LeadToUpdate.Phone2Dial;
        leadToUpdate.LeadToUpdate.Phone3Dial = (leadToUpdate.LeadToUpdate.Phone3Dial == null) ? 0 : leadToUpdate.LeadToUpdate.Phone3Dial;
        leadToUpdate.LeadToUpdate.Phone4Dial = (leadToUpdate.LeadToUpdate.Phone4Dial == null) ? 0 : leadToUpdate.LeadToUpdate.Phone4Dial;
        leadToUpdate.LeadToUpdate.Phone5Dial = (leadToUpdate.LeadToUpdate.Phone5Dial == null) ? 0 : leadToUpdate.LeadToUpdate.Phone5Dial;

        if (leadToUpdate.LeadToUpdate.Phone1Status == 4 &&
            leadToUpdate.LeadToUpdate.Phone2Status == 4 &&
            leadToUpdate.LeadToUpdate.Phone3Status == 4 &&
            leadToUpdate.LeadToUpdate.Phone4Status == 4 &&
            leadToUpdate.LeadToUpdate.Phone5Status == 4 &&
            leadToUpdate.LeadToUpdate.LeadStatusId == 50)
        {
            leadToUpdate.LeadToUpdate.LeadStatusId = 2;
        }

        if (AreThereNoCallableNumbers(leadToUpdate))
        {
            leadToUpdate.LeadToUpdate.LeadStatusId = 10;
        }

        //leaving this code so we remember this is happening, but had to move it into the 
        //UpdateLead method so that we could handle the dates simpler
        //if (leadToUpdate.LeadToUpdate.LeadStatusId == 50
        //    && leadToUpdate.LeadToUpdate.FormerLeadStatusId == 52)
        //{
        //    leadToUpdate.LeadToUpdate.LeadStatusId = 52; //keep it at 52
        //}

        leadToUpdate.LeadToUpdate.Address = (leadToUpdate.LeadToUpdate.Address == null) ? null : leadToUpdate.LeadToUpdate.Address;
        leadToUpdate.LeadToUpdate.City = leadToUpdate.LeadToUpdate.City == null ? null : leadToUpdate.LeadToUpdate.City;
        leadToUpdate.LeadToUpdate.State = leadToUpdate.LeadToUpdate.State == null ? null : leadToUpdate.LeadToUpdate.State;

        int intSuccess = -1;
        int _statusForNotes = leadToUpdate.LeadToUpdate.LeadStatusId;
        if (leadToUpdate.PageRequestingUpdate == "createlead")
        {
            intSuccess = dataagent.InsertLead(leadToUpdate.LeadToUpdate);
            leadToUpdate.LeadToUpdate.LeadId = intSuccess;
            _statusForNotes = leadToUpdate.LeadToUpdate.LeadStatusId;
        }
        else
        {
            intSuccess = (dataagent.UpdateLead(leadToUpdate.LeadToUpdate)) ? 1 : 0;
        }

        if (intSuccess > 0)
        {
            var systemNoteBody = string.Empty;
            var smsbody = string.Empty;
            switch (_statusForNotes)
            {
                case 0:
                    systemNoteBody = $"New RAW Lead!!!";
                    break;
                case 1:
                    systemNoteBody = $"New Lead!!!";
                    break;
                case 3:
                    systemNoteBody =
                    $"Marked as No Answer, but all numbers bad.  Kicking it out as Person Not Found";
                    break;
                case 10:
                    systemNoteBody =
                    $"All numbers are bad or not set. Kicking it out as Person Not Found";
                    break;
                case 50:
                    systemNoteBody =
                        $"Marked as No Answer. Set to Retry {DateTime.UtcNow.AddHours(Util.RETRYHOURS).ToString("yyyy-MM-dd HH: mm: ss")}";
                    break;
                case 51:
                    systemNoteBody =
                        $"Marked as Not Interested (answered).";
                    break;
                case 52:
                    systemNoteBody =
                        $"Marked as Follow Up for {leadToUpdate.LeadToUpdate.FollowUpDate.ToString("yyyy-MM-dd HH:mm:ss")}";
                    break;
                case 53:
                    systemNoteBody =
                        $"Marked as Schedule Appt for {leadToUpdate.LeadToUpdate.SchedApptDate.ToString("yyyy-MM-dd HH:mm:ss")}";

                    smsbody = $"SCHEDULED Appt: https://z.prolinkhome.net/sadispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
                    SendStatusSMS(smsbody, leadToUpdate);
                    break;
                case 54:
                    systemNoteBody =
                        $"Marked as Confirmed for {leadToUpdate.LeadToUpdate.SchedApptDate.ToString("yyyy-MM-dd HH:mm:ss")}";
                    break;
                case 55:
                    systemNoteBody =
                        $"Marked as FieldRep Dispatched for {leadToUpdate.LeadToUpdate.SchedApptDate.ToString("yyyy-MM-dd HH:mm:ss")}";
                    break;
                case 56:
                    systemNoteBody =
                        $"Marked as Sold!";
                    smsbody = $"SOLD: https://z.prolinkhome.net/repdispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
                    SendStatusSMS(smsbody, leadToUpdate);
                    break;
                case 57:
                    systemNoteBody = $"Marked as Installed!";
                    smsbody = $"INSTALLED Appt: https://z.prolinkhome.net/repdispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
                    SendStatusSMS(smsbody, leadToUpdate);
                    break;
            }

            //save if there is Note text
            var noteAgent = new NotesAgent();
            if (!string.IsNullOrEmpty(leadToUpdate.NoteToSave?.NoteText))
            {
                var noteFromText = new Note()
                {
                    LeadId = leadToUpdate.LeadToUpdate.LeadId,
                    LeadStatusId = leadToUpdate.LeadToUpdate?.LeadStatusId,
                    AccountId = -1, //for now
                    AgentId = leadToUpdate.LeadToUpdate.AgentIdSubmitting,
                    NoteText = leadToUpdate.NoteToSave.NoteText
                };
                success = noteAgent.SaveNote(noteFromText);
            }

            //now save "system" note
            var systemNote = new Note()
            {
                LeadId = leadToUpdate.LeadToUpdate.LeadId,
                AccountId = leadToUpdate.AccountId,
                AgentId = leadToUpdate.LeadToUpdate.AgentIdSubmitting,
                NoteText = systemNoteBody
            };
            success = noteAgent.SaveNote(systemNote);

            Logger.Log("CreateOrUpdateLead end LEAD BUG", $"LeadAgent.CreateOrUpdateLead end");
        }

        return success;
    }

    private bool SendStatusSMS(string body, LeadUpdateRequest leadToUpdate)
    {
        //8=Danny, 1=Spencer (testing)
        var devOrProdId = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? 1 : 8;
        var _agent = new AgentAgent().GetAgentById(devOrProdId);

        //build the SMS
        var smsMessage = new SMSMessage()
        {
            AgentSentBy = _agent.AgentId,
            ToPhone = _agent.Phone,
            FromPhone = SMSAgent.TwilioFieldRepPhone,
            LeadId = leadToUpdate.LeadToUpdate.LeadId,
            AccountId = leadToUpdate.AccountId,
            Body = body,
            agentId = _agent.AgentId
        };
        //send sms
        var smsresult = new SMSAgent().SendSMS(smsMessage);
        Logger.Log("LeadAgent.UpdateLeadFromSADispatch SendStatusSMS result:", smsresult.ToString());
        return smsresult;
    }

    public bool SetLeadToFollowUp(LeadUpdateRequest leadToUpdate)
    {
        var success = true;
        var sqlDataAgent = new MySqlDataAgent();
        success = sqlDataAgent.SetLeadToFollowUp(leadToUpdate.LeadToUpdate);

        //now save "system" note
        var noteAgent = new NotesAgent();
        var systemNote = new Note()
        {
            LeadId = leadToUpdate.LeadToUpdate.LeadId,
            AccountId = leadToUpdate.AccountId,
            AgentId = leadToUpdate.LeadToUpdate.AgentIdSubmitting,
            NoteText = $"Unable to get a hold of.  Setting to Follow-Up for {DateTime.UtcNow.ToString("yyyy - MM - dd HH: mm: ss")}",
            LeadStatusId = leadToUpdate.LeadToUpdate.LeadStatusId
        };
        //both have to be successful
        success = (noteAgent.SaveNote(systemNote) && success);

        return success;
    }

    public bool SetLeadToSold(LeadUpdateRequest leadToUpdate)
    {
        var AgentIdSubmittingId = leadToUpdate.LeadToUpdate.AgentIdSubmitting;
        var success = true;
        var sqlDataAgent = new MySqlDataAgent();
        success = sqlDataAgent.SetLeadToSold(leadToUpdate.LeadToUpdate);

        //now save "system" note
        var noteAgent = new NotesAgent();
        var systemNote = new Note()
        {
            LeadId = leadToUpdate.LeadToUpdate.LeadId,
            AccountId = leadToUpdate.AccountId,
            AgentId = AgentIdSubmittingId,
            NoteText = "Marked as Sold!",
            LeadStatusId = leadToUpdate.LeadToUpdate.LeadStatusId
        };
        var smsbody = $"SOLD: https://z.prolinkhome.net/repdispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
        var smsSuccess = SendStatusSMS(smsbody, leadToUpdate);

        //both have to be successful
        success = (noteAgent.SaveNote(systemNote) && success && smsSuccess);

        return success;
    }

    public bool SetLeadToInstalled(LeadUpdateRequest leadToUpdate)
    {
        var success = true;
        var sqlDataAgent = new MySqlDataAgent();
        success = sqlDataAgent.SetLeadToInstalled(leadToUpdate.LeadToUpdate);

        //now save "system" note
        var noteAgent = new NotesAgent();
        var systemNote = new Note()
        {
            LeadId = leadToUpdate.LeadToUpdate.LeadId,
            AccountId = leadToUpdate.AccountId,
            AgentId = leadToUpdate.LeadToUpdate.AgentIdSubmitting,
            NoteText = "Marked as Installed!",
            LeadStatusId = leadToUpdate.LeadToUpdate.LeadStatusId
        };

        var smsbody = $"INSTALLED APPT: https://z.prolinkhome.net/repdispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
        var smsSuccess = SendStatusSMS(smsbody, leadToUpdate);

        //both have to be successful
        success = (noteAgent.SaveNote(systemNote) && success && smsSuccess);

        return success;
    }

    public bool UpdateLeadFromSADispatch(LeadUpdateRequest leadToUpdate)
    {
        var success = true;
        var sqlDataAgent = new MySqlDataAgent();
        try
        {
            var AgentIdSubmittingId = leadToUpdate.LeadToUpdate.AgentIdSubmitting;
            success = sqlDataAgent.UpdateLeadFromSADispatch(leadToUpdate.LeadToUpdate);

            if (leadToUpdate.PageRequestingUpdate == "SetLeadToUFS" || leadToUpdate.PageRequestingUpdate == "SetLeadToNoShow")
            {
                var accountAgent = new AccountAgent();                
                var newAccount = accountAgent.MapLeadToAccount(leadToUpdate.LeadToUpdate);
                
                var _accountId = accountAgent.DoesTheAccountExist(newAccount);
                newAccount.AgentId = AgentIdSubmittingId;
                if (_accountId == -1)
                {
                    //create a new account from the lead and insert into the account table
                    newAccount.AgentId = AgentIdSubmittingId;
                    newAccount.FieldRepId = leadToUpdate.LeadToUpdate.FieldRepId;
                    _accountId = newAccount.AccountId = sqlDataAgent.CreateAccount(newAccount);
                }
                else
                {
                    // if account already exist and create lead than insert data in accountaudit table 
                    newAccount.AccountId = _accountId;
                    var accountdata = sqlDataAgent.GetAccountById(_accountId);                    
                    accountdata.LeadId = leadToUpdate.LeadToUpdate.LeadId;
                    accountdata.AccountStatusId = leadToUpdate.LeadToUpdate.LeadStatusId;
                    accountdata.AgentId = AgentIdSubmittingId;
                    //accountdata.TechId= leadToUpdate.LeadToUpdate.TechId;
                    accountdata.FieldRepId = leadToUpdate.LeadToUpdate.FieldRepId;
                    var account = sqlDataAgent.UpdateAccount(accountdata);
                }
            }

            //save if there is Note text
            var noteAgent = new NotesAgent();
            if (!string.IsNullOrEmpty(leadToUpdate.NoteToSave?.NoteText))
            {
                var noteFromText = new Note()
                {
                    LeadId = leadToUpdate.LeadToUpdate.LeadId,
                    LeadStatusId = leadToUpdate.LeadToUpdate.LeadStatusId,
                    AccountId = -1, //for now
                    AgentId = AgentIdSubmittingId,
                    NoteText = leadToUpdate.NoteToSave.NoteText
                };
                success = (noteAgent.SaveNote(noteFromText) && success);
            }

            //now save "system" note
            var systemNote = new Note()
            {
                LeadId = leadToUpdate.LeadToUpdate.LeadId,
                AccountId = -1, //for now
                AgentId = AgentIdSubmittingId,
                NoteText = $"Updated lead! {leadToUpdate.PageRequestingUpdate}",
                LeadStatusId = leadToUpdate.LeadToUpdate.LeadStatusId
            };
            //both have to be successful
            success = (noteAgent.SaveNote(systemNote) && success);

            //If SetLeadToConfirm, then shoot a cool text to Danny
            if (leadToUpdate.LeadToUpdate.LeadStatusId == 54
                && leadToUpdate.PageRequestingUpdate == "SetLeadToConfirm")
            {
                //8=Danny, 1=Spencer (testing)
                var devOrProdId = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? 1 : 8;
                var _agent = new AgentAgent().GetAgentById(devOrProdId);

                //build the SMS
                var _body = $"CONFIRMED Appt - ready for dispatch: https://z.prolinkhome.net/repdispatch.html?lid={leadToUpdate.LeadToUpdate.LeadId}";
                var smsMessage = new SMSMessage()
                {
                    AgentSentBy = _agent.AgentId,
                    ToPhone = _agent.Phone,
                    FromPhone = SMSAgent.TwilioFieldRepPhone,
                    LeadId = leadToUpdate.LeadToUpdate.LeadId,
                    AccountId = -1, //for now
                    Body = _body,
                    agentId = _agent.AgentId
                };
                //send sms
                var smsResult = new SMSAgent().SendSMS(smsMessage);
                Logger.Log("LeadAgent.UpdateLeadFromSADispatch SMS result:", smsResult.ToString());
            }
        }
        catch (Exception e)
        {
            Logger.Log("PLWEB.ERROR LeadAgent.UpdateLeadFromSADispatch ", e.Message + e.StackTrace);
            success = false;
        }

        return success;
    }

    public List<Lead> GetLeadsByField(string searchTerm, string searchField)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetLeadsByField(searchTerm, searchField);
    }

    public List<Lead> GetLeadsToDispatch()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetLeadsToDispatch();
    }

    public List<Lead> GetConfirmedLeadsToDispatch()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetConfirmedLeadsToDispatch();
    }

    public SchedApptDispatch GetLeadForDispatchById(int leadId)
    {
        var sqlDataAgent = new MySqlDataAgent();
        var saDispatch = new SchedApptDispatch();
        saDispatch.LeadToDispatch = sqlDataAgent.GetLeadForDispatchById(leadId);
        saDispatch.Agents = sqlDataAgent.GetAgentByType("20"); //fieldreps
        saDispatch.Notes = sqlDataAgent.GetNotes(new Note() { LeadId = leadId });
        return saDispatch;
    }

    public Lead GetLeadById(int leadId)
    {
        if (leadId == -99999)
        {
            var sqlDataAgent = new MySqlDataAgent();
            return sqlDataAgent.GetRandomRawLead();
        }
        else
        {
            var sqlDataAgent = new MySqlDataAgent();
            return sqlDataAgent.GetLeadById(leadId);
        }
    }

    public Lead GetRandomTOLead()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetRandomTOLead();
    }

    public Lead GetRandomLead()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetRandomLead();
    }

    public RawLeadCount TotalRawLeads()
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.TotalRawLeads();
    }

    private bool AreThereNoCallableNumbers(LeadUpdateRequest leadToUpdate)
    {
        if (leadToUpdate.PageRequestingUpdate != "updaterawlead" &&
         leadToUpdate.PageRequestingUpdate != "updatelead" &&
         leadToUpdate.PageRequestingUpdate != "createlead")
        {
            var phone1NotCallable = leadToUpdate.LeadToUpdate.Phone1Status == 4 || string.IsNullOrEmpty(leadToUpdate.LeadToUpdate.Phone1);
            var phone2NotCallable = leadToUpdate.LeadToUpdate.Phone2Status == 4 || string.IsNullOrEmpty(leadToUpdate.LeadToUpdate.Phone2);
            var phone3NotCallable = leadToUpdate.LeadToUpdate.Phone3Status == 4 || string.IsNullOrEmpty(leadToUpdate.LeadToUpdate.Phone3);
            var phone4NotCallable = leadToUpdate.LeadToUpdate.Phone4Status == 4 || string.IsNullOrEmpty(leadToUpdate.LeadToUpdate.Phone4);
            var phone5NotCallable = leadToUpdate.LeadToUpdate.Phone5Status == 4 || string.IsNullOrEmpty(leadToUpdate.LeadToUpdate.Phone5);

            return (phone1NotCallable && phone2NotCallable && phone3NotCallable && phone4NotCallable && phone5NotCallable);
        }
        //else
        return false;
    }

    public List<Lead> GetRepLeads(int agentId, string password)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetRepLeads(agentId, password);
    }
}
