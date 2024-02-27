using System;

public class SMSMessage
{
    public int LeadId { get; set; }
    public int AccountId { get; set; }
    public int AgentSentBy { get; set; }
    public string SmsMessageSid { get; set; }
    public string FromZip { get; set; }
    public string SmsSid { get; set; }
    public string FromState { get; set; }
    public string MessageStatus { get; set; }
    public string FromCity { get; set; }
    public string Body { get; set; }
    public string ToPhone { get; set; }
    public int NumSegments { get; set; }
    public string MessageSid { get; set; }
    public string AccountSid { get; set; }
    public string FromPhone { get; set; }
    public string ApiVersion { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateSent { get; set; }
    public string AgentName { get; set; }
    public int Id { get; set; }
    public bool? isread { get; set; }
    public int agentId { get; set; }
    public string agentPhone { get; set; }
    public int AgentTypeId { get; set; }
    public string FileUrl { get; set; }
    public string ChatAgentPhone { get; set; }
}

public class LeadTempClass
{
    public string Phone { get; set; }
    
    public bool? isread { get; set; }
    
    public int leadid { get; set; }
    public int Leadstatusid { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }

}