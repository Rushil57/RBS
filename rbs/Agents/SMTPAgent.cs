using System;
using System.Net;
using System.Net.Mail;

public class SMTPAgent
{
    /// <summary>
    /// This one is the internal service account
    /// </summary>
    public MailAddress NOREPLY = new MailAddress("noreply@prolinkprotection.com", "Prolink");
    public NetworkCredential NOREPLY_CREDS = new NetworkCredential("noreply@prolinkprotection.com", "Pro1ink2019!");

    public bool SendEmail(MailMessage msg, NetworkCredential creds = null)
    {
        bool success = false;
        try
        {
            if (creds == null)
            {
                creds = NOREPLY_CREDS;
            }

            //Now sending a mail with attachment ICS file. 
            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = creds

            };
            smtp.Send(msg);
            success = true; //we didn't error out
        }
        catch (Exception err)
        {
            Logger.Log("SMTPAgent.SendEmail", $" Message: {err.Message}, StackTrace: {err.StackTrace}");
            success = false;
        }

        return success;
    }

}

