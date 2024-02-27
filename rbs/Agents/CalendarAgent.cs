using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;

public class CalendarAgent
{
    /// <summary>
    /// This one is for Techs
    /// </summary>
    public static readonly MailAddress CSR = new MailAddress("csr@prolinkprotection.com", "Prolink CSR");
    public static readonly NetworkCredential CSR_CREDS = new NetworkCredential("csr@prolinkprotection.com", "Pro1ink2019!");
    /// <summary>
    /// This one is for FieldReps
    /// </summary>
    public static readonly MailAddress InternalSales = new MailAddress("InternalSales@prolinkprotection.com", "Prolink Internal Sales");
    public static readonly NetworkCredential IS_CREDS = new NetworkCredential("internalsales@prolinkprotection.com", "ISTprolink!");

    /// <summary>
    /// This one is the internal service account
    /// </summary>
    public static readonly MailAddress NOREPLY = new MailAddress("noreply@prolinkprotection.com", "Prolink");
    public static readonly NetworkCredential NOREPLY_CREDS = new NetworkCredential("noreply@prolinkprotection.com", "Pro1ink2019!");

    public bool SendCalendar(DispatchAgentRequest dispatch)
    {
        var success = true;
        try
        {
            var agentBeingDispatched = new AgentAgent().GetAgentById(dispatch.AgentBeingDispatched);
            var location = dispatch.LeadToDispatch.Address + ", " + dispatch.LeadToDispatch.City + ", AZ";
            var emailCalendar = (agentBeingDispatched.AgentTypeId == 20) ? InternalSales : CSR;
            var emailCreds = NOREPLY_CREDS;

            // add from,to mailaddresses
            MailMessage msg = new MailMessage();
            msg.From = NOREPLY;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //only do on linux - i.e. prod
            {
                msg.To.Add(emailCalendar);
            }
            msg.To.Add(new MailAddress(agentBeingDispatched.Email, agentBeingDispatched.FirstName + " " + agentBeingDispatched.LastName));
            msg.ReplyToList.Add(emailCalendar);
            msg.IsBodyHtml = true;
            //todo - do we need to build in the body different?
            //need to build the body
            //https://developers.google.com/gmail/markup/getting-started
            //https://jsonld.com/event/
            msg.Body = dispatch.FormattedDispatchBody;
            msg.Subject = (agentBeingDispatched.AgentTypeId == 20)
                ? "PROLINK LEAD AT: " + location
                : "PROLINK INSTALL AT: " + location;

            StringBuilder str = new StringBuilder();
            str.AppendLine("BEGIN:VCALENDAR");
            str.AppendLine("PRODID:-//Schedule a Meeting");
            str.AppendLine("VERSION:2.0");
            str.AppendLine("METHOD:REQUEST");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmss}", dispatch.LeadToDispatch.SchedApptDate));
            str.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmss}", DateTime.UtcNow));
            if (agentBeingDispatched.AgentTypeId == 20)
            {
                str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmss}", dispatch.LeadToDispatch.SchedApptDate.AddMinutes(+30)));
            }
            else
            {
                str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmss}", dispatch.LeadToDispatch.SchedApptDate.AddMinutes(+60)));
            }
            str.AppendLine("LOCATION: " + location);
            str.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
            str.AppendLine(string.Format("DESCRIPTION:{0}", msg.Body));
            str.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", msg.Body));
            str.AppendLine(string.Format("SUMMARY:{0}", msg.Subject));
            str.AppendLine(string.Format("ORGANIZER:MAILTO:{0}", msg.From.Address));
            str.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", msg.To[0].DisplayName, msg.To[0].Address));

            str.AppendLine("BEGIN:VALARM");
            str.AppendLine("TRIGGER:-PT15M");
            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");

            byte[] byteArray = Encoding.ASCII.GetBytes(str.ToString());
            MemoryStream stream = new MemoryStream(byteArray);

            Attachment attach = new Attachment(stream, dispatch.LeadToDispatch.LastName + "_install.ics");

            msg.Attachments.Add(attach);

            System.Net.Mime.ContentType contype = new System.Net.Mime.ContentType("text/calendar");
            contype.Parameters.Add("method", "REQUEST");
            AlternateView avCal = AlternateView.CreateAlternateViewFromString(str.ToString(), contype);
            msg.Headers.Add("Content-class", "urn:content-classes:calendarmessage");
            msg.AlternateViews.Add(avCal);

            //Now sending a mail with attachment ICS file. 
            success = new SMTPAgent().SendEmail(msg,emailCreds);
        }
        catch (Exception err)
        {
            Logger.Log("CalendarAgent.SendCalendar", $" Message: {err.Message}, StackTrace: {err.StackTrace}");
            success = false;
        }

        return success;
    }

    //GetCalendarLeads
    public CalendarLeads GetCalendarLeads()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetCalendarLeads();
    }

    public CalendarLeads GetCalendarSalesClosedLeads()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetCalendarSalesClosedLeads();
    }
}