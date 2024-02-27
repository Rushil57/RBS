using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

public class AgentAgent
{
    public bool LoginAgent(Agent agent)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.LoginAgent(agent);
    }

    public List<Agent> GetAllAgents()
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllAgents();
    }

    public List<Agent> GetAllActiveAgents()
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllActiveAgents();
    }

    public Agent GetAgentById(int AgentId)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAgentById(AgentId);
    }

    public Agent GetAgentStatusById(int AgentId)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAgentById(AgentId);
    }

    public List<Agent> GetAllAgentsByAgentTypeId(int agentTypeId)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllAgentsByAgentTypeId(agentTypeId);
    }

    public List<Agent> GetAgentByType(string agentType)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAgentByType(agentType);
    }

    public bool DispatchAgent(DispatchAgentRequest dispatch)
    {
        var dataAgent = new MySqlDataAgent();
        var account = new Account();
        try
        {
            //int agentid = dispatch.SubmittingAgent;
            //load that agent
            var agent = GetAgentById(dispatch.AgentBeingDispatched);
            int techId = dispatch.LeadToDispatch.TechId;
            int fieldRepId = dispatch.LeadToDispatch.FieldRepId;
            //agentType = 50 should have had enough of the model populated client side
            //todo - this is a temp hack until we get the account stuff (phase 2) started.
            //then they would have to create an "Account" and launch it from there.
            if (agent.AgentTypeId == 20) //fieldrep
            {
                var leadAgent = new LeadAgent();
                //grab the sched appt coming from the UI (not the DB)
                var _schedDate = dispatch.LeadToDispatch.SchedApptDate;

                //should have valid lead
                dispatch.LeadToDispatch = leadAgent.GetLeadById(dispatch.LeadToDispatch.LeadId);
                dispatch.LeadToDispatch.SchedApptDate = _schedDate;
                dispatch.LeadToDispatch.LeadStatusId = 55; //FieldRep Dispatched
                //ST - this needs to be the rep that is getting paid for this lead
                dispatch.LeadToDispatch.AgentIdSubmitting = dataAgent.GetLastAgentScheduledAppt(dispatch.LeadToDispatch.LeadId);
                //update the lead to set to 54 so that we can track they have been dispatched
                var leadUpdate = new LeadUpdateRequest()
                {
                    LeadToUpdate = dispatch.LeadToDispatch,
                    PageRequestingUpdate = dispatch.PageRequestingDispatch
                };
                leadAgent.CreateOrUpdateLead(leadUpdate);

                //
                Lead lead = new Lead();
                lead = dispatch.LeadToDispatch;
                lead.AgentIdSubmitting = dispatch.AgentBeingDispatched;
                bool IsSuccess = dataAgent.InsertNonDialAudit(lead);

                //transition Lead --> Account
                //account = new AccountAgent().CreateAccountFromLead(leadUpdate.LeadToUpdate);
                leadUpdate.LeadToUpdate.AgentIdSubmitting = agent.AgentId;
                //leadUpdate.LeadToUpdate.TechId = techId;
                leadUpdate.LeadToUpdate.FieldRepId = fieldRepId;
                account = new AccountAgent().CreateAccountFromLead(leadUpdate.LeadToUpdate);

            }
            else //tech dispatch - lead could be null
            {
                account = dataAgent.GetAccountById(dispatch.AccountToDispatch.AccountId);
                if (account != null)
                {
                    account.TechId = dispatch.AccountToDispatch.TechId;
                    //account.FieldRepId= dispatch.AccountToDispatch.FieldRepId;
                    account.AgentId = agent.AgentId;
                    //TODO - if this is a service ticket then we need to use 210
                    account.AccountStatusId = 110; //Tech Dispatched
                    account.TechSchedDate = Util.ConvertToTZ(dispatch.LeadToDispatch.SchedApptDate, dispatch.LeadToDispatch.TimeZone);
                    bool IsSuccess = dataAgent.UpdateAccount(account);
                }
                //else
                //TODO - we should never be hitting an empty account.  We need to create an account for every service ticket.
            }

            //ST - this needs to be the agent that is actually dispatching,
            //not the one that gets paid for the sched appt
            dispatch.LeadToDispatch.AgentIdSubmitting = dispatch.SubmittingAgent;
            var body = BuildDispatchMessageBody(dispatch);
            dispatch.FormattedDispatchBody = body;
            var fromPhone = (agent.AgentTypeId == 20) ? SMSAgent.TwilioFieldRepPhone : SMSAgent.TwilioTechPhone;

            var _accountId = (account != null) ? account.AccountId : -1;

            //build the SMS
            try
            {
                var smsMessage = new SMSMessage()
                {
                    AgentSentBy = agent.AgentId,
                    ToPhone = agent.Phone,
                    FromPhone = fromPhone,
                    LeadId = dispatch.LeadToDispatch.LeadId,
                    AccountId = _accountId,
                    Body = body,
                    agentId = agent.AgentId
                };
                //send sms
                var smsResult = new SMSAgent().SendSMS(smsMessage);
                body += $"<br/>SMS Sent: {smsResult.ToString()}";
            }
            catch (Exception e)
            {
                Logger.Log("PLWEB.ERROR AgentAgent.DispatchAgent error sending SMS", e.Message + e.StackTrace);
                Logger.Log("PLWEB.ERROR AgentAgent.DispatchAgent error error sending SMS",
                    JsonConvert.SerializeObject(dispatch));
            }

            //insert calendar invite
            var calResult = new CalendarAgent().SendCalendar(dispatch);
            body += $"<br/>Calender set: {calResult.ToString()}";
            body = $"DISPATCH NOTE: <br/> Agent being dispatched: {agent.FirstName} {agent.LastName} <br/> {body}";

            var _noteStatusId = (agent.AgentTypeId == 20) ? dispatch.LeadToDispatch.LeadStatusId :
                (account != null) ? 110 //Tech Dispatched
                : 210; //Service Ticket Dispatched

            //build note
            var note = new Note()
            {
                LeadId = dispatch.LeadToDispatch.LeadId,
                LeadStatusId = _noteStatusId,
                AccountId = _accountId,
                AgentId = agent.AgentId,
                NoteText = body
            };
            //submit note
            return new NotesAgent().SaveNote(note);
        }
        catch (Exception e)
        {
            Logger.Log("PLWEB.ERROR AgentAgent.DispatchAgent error", e.Message + e.StackTrace);
            Logger.Log("PLWEB.ERROR AgentAgent.DispatchAgent error dispatch",
                JsonConvert.SerializeObject(dispatch));
            return false;
        }
    }

    public bool ScheduleTech(DispatchAgentRequest dispatch)
    {
        var dataAgent = new MySqlDataAgent();
        var account = dataAgent.GetAccountById(dispatch.AccountToDispatch.AccountId);
        if (account != null)
        {
            //int agentid = dispatch.SubmittingAgent;
            //account.AgentId = agentid;

            //TODO - if this is a service ticket then we need to use 210
            account.AccountStatusId = 100; //Tech Scheduled
            account.TechSchedDate = Util.ConvertToTZ(dispatch.LeadToDispatch.SchedApptDate, dispatch.LeadToDispatch.TimeZone);
            bool IsSuccess = dataAgent.UpdateAccount(account);

            var body = BuildDispatchMessageBody(dispatch);

            //build note
            var note = new Note()
            {
                LeadId = dispatch.LeadToDispatch.LeadId,
                LeadStatusId = 100,
                AccountId = dispatch.AccountToDispatch.AccountId,
                AgentId = dispatch.SubmittingAgent,
                NoteText = $"SET TO TECH SCHEDULED!<br/>Note: <br/> {body}"
            };
            //submit note
            return (new NotesAgent().SaveNote(note) && IsSuccess);
        }
        else
        {
            Logger.Log("PLWEB.ERROR AgentAgent.ScheduleTech error", "Account is null.  That is super bad.  Shouldn't be here.");
            Logger.Log("PLWEB.ERROR AgentAgent.ScheduleTech error dispatch",
                JsonConvert.SerializeObject(dispatch));
            return false;
        }
    }

    private string BuildDispatchMessageBody(DispatchAgentRequest dispatch)
    {
        var agentBeingDispatched = new AgentAgent().GetAgentById(dispatch.AgentBeingDispatched);
        var sb = new StringBuilder();

        if (dispatch.AgentBeingDispatched == -99999)
        {
            sb.AppendLine("SCHEDULED INSTALL for: ");
        }
        else if (agentBeingDispatched.AgentTypeId == 20)
        {
            sb.AppendLine("LEAD for: ");
        }
        else
        {
            sb.AppendLine("INSTALL for: ");
        }
        sb.AppendLine(dispatch.LeadToDispatch.FirstName + " " + dispatch.LeadToDispatch.LastName);
        sb.AppendLine();
        sb.Append("Appt time: ");
        sb.Append(dispatch.LeadToDispatch.SchedApptDate.ToShortDateString());
        sb.Append(" ");
        sb.AppendLine(dispatch.LeadToDispatch.SchedApptDate.ToShortTimeString());
        sb.AppendLine();
        var address = dispatch.LeadToDispatch.Address + ", " + dispatch.LeadToDispatch.City + ", " + dispatch.LeadToDispatch.State;
        sb.AppendLine(address);
        sb.AppendLine();
        //slt - removing this for now, may add it back in the future
        //sb.AppendLine(dispatch.LeadToDispatch.Phone1);
        sb.AppendLine();
        if (agentBeingDispatched != null && agentBeingDispatched.AgentTypeId == 20)
        {
            sb.AppendLine("Notes:");
        }
        else
        {
            sb.AppendLine("Equipment/Notes:");
        }
        sb.AppendLine(dispatch.EquipmentOrder);
        sb.AppendLine();
        var longUrl = "http://maps.google.com/?q=" + Uri.EscapeDataString(address);
        //todo - I couldn't get this to work!!!
        //var shortUrl = Util.GetShortenedUrl(longUrl);
        //shortUrl = string.IsNullOrWhiteSpace(shortUrl) ? longUrl : shortUrl;
        sb.AppendLine(longUrl);
        sb.AppendLine();
        if (dispatch.AgentBeingDispatched != -99999)
        {
            sb.AppendLine("Please respond to acknowledge this and provide your ETA if it is an install.");
        }

        return sb.ToString();
    }

    public TimeclockRequest GetTimeclockHistory(int agentId)
    {
        var history = new MySqlDataAgent().GetTimeclockHistory(agentId);
        var lastActionType = 0;
        //if (history.Count > 0)
        //{
        //lastActionType = history.OrderByDescending(h => h.ActionTime).First().ActionType;
        //}

        return new TimeclockRequest()
        {
            History = history,
            LastActionType = lastActionType
        };
    }

    public TimeclockAction InsertTimeclockAction(TimeclockAction action)
    {
        TimeclockAction result = null;
        var timeClock = new MySqlDataAgent().IsUserClockedIn(action);

        if (timeClock == null)
        {
            //insert clocked in time.
            action.ClockedInAt = DateTime.Now;
            result = new MySqlDataAgent().InsertTimeclockAction(action);
        }
        else
        {
            action.TimeclockId = timeClock.TimeclockId;
            result = new MySqlDataAgent().UpdateTimeclockAction(action);
        }

        return result;
    }

    public DashboardResponse GetDashboard(DashboardRequest request)
    {
        var statsAgent = new StatsAgent();

        return new DashboardResponse()
        {
            LeadGenStatsList = statsAgent.GetLeadGenStats(request.FromDateTime, request.ToDateTime),
            DialAgentStatsList = statsAgent.GetDialAgentStats(),
            DialRatioList = statsAgent.GetDialRatio(request.FromDateTime, request.ToDateTime),
            LeadRatioList = statsAgent.GetLeadRatio(request.FromDateTime, request.ToDateTime),
            SAToInstall = statsAgent.GetSAToInstalls(request.FromDateTime, request.ToDateTime)
        };
    }

    public List<TimeclockAction> GetClockedInOutHistory(TimeclockAction timeclockAction)
    {
        return new MySqlDataAgent().GetClockedInOutHistory(timeclockAction);
    }

    public DialReportResponse GetDialReport(int agentId)
    {
        var sqlagent = new MySqlDataAgent();
        return new DialReportResponse()
        {
            TodaysDials = sqlagent.GetDaysDailReport(true, agentId),
            YesterdaysDials = sqlagent.GetDaysDailReport(false, agentId)
        };
    }

    public PayReport GetPayReport(TimeclockAction timeclockAction)
    {
        var payReport = new PayReport();
        try
        {
            var sqlagent = new MySqlDataAgent();
            var leads = sqlagent.GetPayReportList(timeclockAction);

            foreach (var lead in leads)
            {
                switch (lead.LeadStatusId)
                {
                    case 54:
                        payReport.ConfirmedAppointments.Add(lead);
                        break;
                    case 57:
                        payReport.CompletedInstalls.Add(lead);
                        break;
                    case 59:
                        payReport.NoShowLead.Add(lead);
                        break;
                }
            }
            var _timeClockActions = sqlagent.GetCheckinCheckOutReport(timeclockAction);

            payReport = GeneratePDF(payReport, _timeClockActions, timeclockAction);
            return payReport;
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR AgentAgent.GetPayReport ", err.Message + err.StackTrace);
            return null;
        }
    }

    public PayReport GeneratePDF(PayReport payreport, List<TimeclockAction> lsttimeclockActions, TimeclockAction request)
    {
        Document doc = new Document(PageSize.A4);
        try
        {
            if (payreport.CompletedInstalls.Count > 0 || payreport.ConfirmedAppointments.Count > 0 || lsttimeclockActions.Count > 0)
            {

                var agentsTotlaHoure = lsttimeclockActions.Where(a => a.HrsWorked != null).Select(x => x.HrsWorked).ToList();

                foreach (TimeSpan item in agentsTotlaHoure)
                {
                    if (item != null)
                    {
                        payreport.TotalMinutes += item.TotalMinutes;
                    }
                }
                TimeSpan TotalHoursTime = TimeSpan.FromMinutes(payreport.TotalMinutes);
                string TotalHours = (((TotalHoursTime.Days * 24) + TotalHoursTime.Hours).ToString() + ":" + TotalHoursTime.Minutes).ToString();//+ ":" + TotalHoursTime.Seconds
                double TotalPAyAmount = (Convert.ToDouble(TotalHoursTime.TotalHours) * 15);
                if (!request.MaxDollarPerHour)
                {
                    TotalPAyAmount = (Convert.ToDouble(TotalHoursTime.TotalHours) * 11);
                }

                int ConfirmedCount = payreport.ConfirmedAppointments.Count;
                int InstalledCount = payreport.CompletedInstalls.Count;
                int NoShowCount = payreport.NoShowLead.Count;

                double ConfirmedPay = ConfirmedCount * 10;
                double InstalledPay = InstalledCount * 45;
                double NoShowDeduction = NoShowCount * 15;
                string filePath = Path.Combine(Util.filefolderpath);
                string filename = "PayReport" + DateTime.Now.Ticks + ".pdf";
                string fullpath = filePath + filename;
                payreport.FilePath = filePath;
                payreport.FileName = filename;
                payreport.FileFullPath = fullpath;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(fullpath, FileMode.Create));
                doc.Open();
                BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
                BaseFont bf2 = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
                var titleFont = new iTextSharp.text.Font(bf, 13.0f, 1, BaseColor.BLACK);
                var fnt = new iTextSharp.text.Font(bf2, 8.0f, 1, BaseColor.BLACK);
                var tblHeader = new iTextSharp.text.Font(bf, 8.5f, 1, BaseColor.BLACK);
                string date = request.StartDate.ToString("dd MMMM yyyy") + " to " + request.EndDate.ToString("dd MMMM yyyy");

                string Paydate = string.Empty;

                if (request.PayDate != null)
                {
                    Paydate = request.PayDate.Value.ToString("dd MMMM yyyy");
                }
                Image logo = Image.GetInstance(Util.GetResourceFileAsStream("logo.png"));
                logo.ScaleAbsolute(50, 50);
                logo.Alignment = Element.ALIGN_RIGHT;

                doc.Add(logo);

                string agentName = string.Empty;

                if (lsttimeclockActions != null)
                {
                    agentName = lsttimeclockActions[0].FirstName + " " + lsttimeclockActions[0].LastName;
                }
                else
                {
                    var sqlagent = new MySqlDataAgent();
                    var agent = sqlagent.GetAgentById(request.AgentId);
                    if (agent != null)
                    {
                        agentName = agent.FirstName + " " + agent.LastName;
                    }
                }
                PdfPTable headerMaintbl = new PdfPTable(2);
                headerMaintbl.PaddingTop = 100;
                headerMaintbl.DefaultCell.Border = 0;
                headerMaintbl.WidthPercentage = 100;
                headerMaintbl.TotalWidth = 100;
                headerMaintbl.SpacingBefore = 10;
                headerMaintbl.SpacingAfter = 10;
                PdfPTable headerToptbl = new PdfPTable(1);
                headerToptbl.DefaultCell.Border = 0;

                headerToptbl.AddCell(new Phrase("Pay Report", titleFont));
                headerToptbl.AddCell(new Phrase("Agent Name: " + agentName, fnt));

                headerToptbl.AddCell(new Phrase(date, fnt));
                if (Paydate != string.Empty)
                {
                    headerToptbl.AddCell(new Phrase("Pay Date: " + Paydate, fnt));
                }
                float[] width = { 70, 30 };
                headerMaintbl.SetWidths(width);
                headerMaintbl.AddCell(new PdfPTable(headerToptbl));

                PdfPTable headertbl = new PdfPTable(2);
                headertbl.DefaultCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                headertbl.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                headertbl.HorizontalAlignment = Element.ALIGN_LEFT;
                headertbl.DefaultCell.BorderColor = BaseColor.GRAY;
                headertbl.AddCell(new Phrase("Appointments Set: ", fnt));
                headertbl.AddCell(new Phrase("$" + string.Format("{0:0.00}", ConfirmedPay), fnt));

                headertbl.AddCell(new Phrase("Appointments Closed: ", fnt));
                headertbl.AddCell(new Phrase("$" + string.Format("{0:0.00}", InstalledPay), fnt));

                headertbl.AddCell(new Phrase("No-show: ", fnt));
                headertbl.AddCell(new Phrase("-$" + string.Format("{0:0.00}", NoShowDeduction), fnt));

                headertbl.AddCell(new Phrase("Agent Hours: ", fnt));
                headertbl.AddCell(new Phrase("" + TotalHours, fnt));

                //before we round
                var Total = TotalPAyAmount + InstalledPay + ConfirmedPay - NoShowDeduction;
                //now round
                TotalPAyAmount = Math.Round(TotalPAyAmount, 2, MidpointRounding.AwayFromZero);
                headertbl.AddCell(new Phrase("Agent Pay: ", fnt));
                headertbl.AddCell(new Phrase("$" + TotalPAyAmount, fnt));

                Total = Math.Round(Total, 2, MidpointRounding.AwayFromZero);
                headertbl.AddCell(new Phrase("Total: ", fnt));
                headertbl.AddCell(new Phrase("$" + Total, fnt));

                //doc.Add(headertbl);

                headerMaintbl.AddCell(new PdfPTable(headertbl));
                doc.Add(headerMaintbl);

                doc.Add(new Paragraph("Agent Check in Check out Detail", tblHeader));
                PdfPTable tblCheckIn = new PdfPTable(3);
                tblCheckIn.HorizontalAlignment = Element.ALIGN_MIDDLE;
                tblCheckIn.SpacingAfter = 20;
                tblCheckIn.SpacingBefore = 8;
                tblCheckIn.WidthPercentage = 100;
                tblCheckIn.DefaultCell.BorderColor = BaseColor.GRAY;

                var CheckInHeader = tblCheckIn.GetHeader();
                tblCheckIn.AddCell(new Phrase("Date", tblHeader));
                tblCheckIn.AddCell(new Phrase("Hours", tblHeader));
                if (request.MaxDollarPerHour)
                {
                    tblCheckIn.AddCell(new Phrase("Payment (Hours * $15)", tblHeader));
                }
                else
                {
                    tblCheckIn.AddCell(new Phrase("Payment (Hours * $11)", tblHeader));
                }

                TimeSpan totalHours = new TimeSpan();
                double TotalPayment = 0;
                foreach (TimeclockAction row in lsttimeclockActions)
                {
                    double Payment = 0;
                    tblCheckIn.AddCell(new Phrase(row.ClockedInAt.ToString("dd MMMM yyyy"), fnt));
                    if (row.HrsWorked != null)
                    {
                        TimeSpan Hours = TimeSpan.Parse(row.HrsWorked.ToString());
                        if (request.MaxDollarPerHour)
                        {
                            Payment = Hours.TotalHours * 15;
                        }
                        else
                        {
                            Payment = Hours.TotalHours * 11;
                        }
                        string strHours = row.HrsWorked.ToString();
                        strHours = strHours.Remove(strHours.Length - 3);
                        tblCheckIn.AddCell(new Phrase(Convert.ToString(strHours) != null ? Convert.ToString(strHours) : "", fnt));
                        totalHours += Hours;
                        TotalPayment += Payment;
                    }
                    else
                    {
                        tblCheckIn.AddCell(new Phrase("00:00", fnt));
                    }
                    tblCheckIn.AddCell(new Phrase(Payment.ToString() != null ? Convert.ToString("$" + Payment.ToString("0.00")) : "", fnt));
                }
                PdfPCell TotalLabelCell = new PdfPCell(new Phrase("Total", tblHeader));
                TotalLabelCell.PaddingBottom = 5;
                TotalLabelCell.PaddingTop = 5;
                tblCheckIn.AddCell(TotalLabelCell);

                string totalWorkingHours = (totalHours.Days * 24 + totalHours.Hours) + ":" + totalHours.Minutes;//":" + totalHours.Seconds
                PdfPCell TotalHoursCell = new PdfPCell(new Phrase(totalWorkingHours.ToString(), tblHeader));
                TotalHoursCell.PaddingBottom = 5;
                TotalHoursCell.PaddingTop = 5;
                tblCheckIn.AddCell(TotalHoursCell);

                PdfPCell TotalPayCell = new PdfPCell(new Phrase("$" + TotalPayment.ToString("0.00"), tblHeader));
                TotalPayCell.PaddingBottom = 5;
                TotalPayCell.PaddingTop = 5;
                tblCheckIn.AddCell(TotalPayCell);
                doc.Add(tblCheckIn);

                PdfPTable tbl = new PdfPTable(5);
                tbl.HorizontalAlignment = Element.ALIGN_MIDDLE;
                tbl.SpacingBefore = 20;
                tbl.SpacingAfter = 20;
                tbl.WidthPercentage = 100;
                tbl.DefaultCell.BorderColor = BaseColor.GRAY;

                PdfPCell Headercell1 = new PdfPCell(new Phrase("Appointments Set: $" + ConfirmedPay, tblHeader));
                Headercell1.Colspan = 5;
                Headercell1.PaddingBottom = 5;
                Headercell1.PaddingTop = 5;
                Headercell1.Border = 0;
                tbl.AddCell(Headercell1);

                var header = tbl.GetHeader();
                tbl.AddCell(new Phrase("Date", tblHeader));
                tbl.AddCell(new Phrase("Lead Status", tblHeader));
                tbl.AddCell(new Phrase("Lead Type", tblHeader));
                tbl.AddCell(new Phrase("LeadId", tblHeader));
                tbl.AddCell(new Phrase("Payment ($" + ConfirmedPay + "/Appointment)", tblHeader));

                foreach (Lead row in payreport.ConfirmedAppointments)
                {
                    tbl.AddCell(new Phrase(Convert.ToString(row.InsertDate.ToString("dd MMMM yyyy")), fnt));
                    tbl.AddCell(new Phrase(Convert.ToString(row.StatusText) != null ? row.StatusText.ToString() : "", fnt));
                    tbl.AddCell(new Phrase(Convert.ToString(row.LeadTypeText) != null ? Convert.ToString(row.LeadTypeText) : "", fnt));
                    tbl.AddCell(new Phrase(row.LeadId.ToString(), fnt));
                    double dispay = (ConfirmedPay / payreport.ConfirmedAppointments.Count);
                    tbl.AddCell(new Phrase("$" + dispay, fnt));
                }

                doc.Add(tbl);

                PdfPTable tblCompleted = new PdfPTable(5);
                tblCompleted.HorizontalAlignment = Element.ALIGN_MIDDLE;
                tblCompleted.SpacingBefore = 20;
                tblCompleted.SpacingAfter = 20;
                tblCompleted.WidthPercentage = 100;
                tblCompleted.PaddingTop = 20;
                tblCompleted.DefaultCell.BorderColor = BaseColor.GRAY;

                PdfPCell Headercell2 = new PdfPCell(new Phrase("Appointments Closed: $" + InstalledPay, tblHeader));
                Headercell2.Colspan = 5;
                Headercell2.PaddingBottom = 5;
                Headercell2.PaddingTop = 5;
                Headercell2.Border = 0;
                tblCompleted.AddCell(Headercell2);
                var header2 = tblCompleted.GetHeader();
                tblCompleted.AddCell(new Phrase("Date", tblHeader));
                tblCompleted.AddCell(new Phrase("Lead Status", tblHeader));
                tblCompleted.AddCell(new Phrase("Lead Type", tblHeader));
                tblCompleted.AddCell(new Phrase("LeadId", tblHeader));
                tblCompleted.AddCell(new Phrase("Payment ($" + InstalledPay + "/Appointment)", tblHeader));

                foreach (Lead row in payreport.CompletedInstalls)
                {
                    tblCompleted.AddCell(new Phrase(Convert.ToString(row.InsertDate.ToString("dd MMMM yyyy")), fnt));
                    tblCompleted.AddCell(new Phrase(Convert.ToString(row.StatusText) != null ? row.StatusText.ToString() : "", fnt));
                    tblCompleted.AddCell(new Phrase(Convert.ToString(row.LeadTypeText) != null ? Convert.ToString(row.LeadTypeText) : "", fnt));
                    tblCompleted.AddCell(new Phrase(row.LeadId.ToString(), fnt));
                    double instpay = (InstalledPay / payreport.CompletedInstalls.Count);
                    tblCompleted.AddCell(new Phrase("$" + instpay, fnt));
                }

                doc.Add(tblCompleted);

                // No-Show
                PdfPTable tblNoShow = new PdfPTable(5);
                tblNoShow.HorizontalAlignment = Element.ALIGN_MIDDLE;
                tblNoShow.SpacingBefore = 20;
                tblNoShow.SpacingAfter = 20;
                tblNoShow.WidthPercentage = 100;
                tblNoShow.PaddingTop = 20;
                tblNoShow.DefaultCell.BorderColor = BaseColor.GRAY;

                PdfPCell HeadercellNoShow2 = new PdfPCell(new Phrase("No-Show: $" + NoShowDeduction, tblHeader));
                HeadercellNoShow2.Colspan = 5;
                HeadercellNoShow2.PaddingBottom = 5;
                HeadercellNoShow2.PaddingTop = 5;
                HeadercellNoShow2.Border = 0;
                tblNoShow.AddCell(HeadercellNoShow2);
                //var header2 = tblNoShow.GetHeader();
                tblNoShow.AddCell(new Phrase("Date", tblHeader));
                tblNoShow.AddCell(new Phrase("Lead Status", tblHeader));
                tblNoShow.AddCell(new Phrase("Lead Type", tblHeader));
                tblNoShow.AddCell(new Phrase("LeadId", tblHeader));
                tblNoShow.AddCell(new Phrase("Deduction ($" + NoShowDeduction + "/No-Show)", tblHeader));

                foreach (Lead row in payreport.NoShowLead)
                {
                    tblNoShow.AddCell(new Phrase(Convert.ToString(row.InsertDate.ToString("dd MMMM yyyy")), fnt));
                    tblNoShow.AddCell(new Phrase(Convert.ToString(row.StatusText) != null ? row.StatusText.ToString() : "", fnt));
                    tblNoShow.AddCell(new Phrase(Convert.ToString(row.LeadTypeText) != null ? Convert.ToString(row.LeadTypeText) : "", fnt));
                    tblNoShow.AddCell(new Phrase(row.LeadId.ToString(), fnt));
                    double instpay = (NoShowDeduction / payreport.NoShowLead.Count);
                    tblNoShow.AddCell(new Phrase("$" + instpay, fnt));
                }

                doc.Add(tblNoShow);

                doc.Close();
            }
            else
            {
                payreport = null;
            }

        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR AgentAgent.GeneratePDF ", err.Message + err.StackTrace);
            payreport = null;
        }

        return payreport;
    }

    public FileStream DownloadFile(PayReport payreport)
    {
        try
        {
            FileStream fs = new FileStream(payreport.FilePath, FileMode.Open, FileAccess.Read);

            byte[] bytes = new byte[(int)fs.Length];
            string fileName = payreport.FileName;
            fs.Read(bytes, 0, (int)fs.Length);

            return fs;
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR AgentAgent.DownloadFile ", err.Message + err.StackTrace);
            return null;
        }
    }

    public List<AgentType> GetAllAgentType()
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllAgentType();
    }

    public List<Agent> GetAllAgentList()
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAllAgentList();
    }

    public bool CreateOrUpdateAgent(Agent agent)
    {
        bool success = false;
        try
        {
            agent.Phone = agent.Phone.Replace("(", "").Replace(")", "").Replace(".", "").Replace("-", "").Replace(" ", "");
            if (agent.AgentId > 0)
            {
                success = new MySqlDataAgent().UpdateAgent(agent);
            }
            else
            {
                var _agentId = new MySqlDataAgent().InsertAgent(agent);
                if (_agentId > 0)
                {
                    success = true;
                    //if ISR, email the new agent
                    if (agent.AgentTypeId == 40)
                    {
                        // add from,to mailaddresses
                        MailMessage msg = new MailMessage();
                        msg.From = CalendarAgent.NOREPLY;
                        msg.To.Add(agent.Email);
                        msg.IsBodyHtml = true;
                        msg.Subject = $"CRM Login - {agent.FirstName} {agent.LastName}";

                        var _body = $@"<a href='https://z.prolinkhome.net/internalsales.html'>https://z.prolinkhome.net/internalsales.html</a> <br/>
                        Pick your firstname<br/>
                        ID: {agent.Email}<br/>
                        pw: {agent.AgentPassword}<br/>";
                        msg.Body = _body;

                        success = new SMTPAgent().SendEmail(msg);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log("AccountController.CreateAgent calling api ", e.Message + e.StackTrace);
            success = false;
        }

        return success;
    }

}

