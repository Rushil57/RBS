using System;

public class LookupAudit
{
    public int LookupAuditId { get; set; }
    public int LeadId { get; set; }
    public int AgentId { get; set; }
    public DateTime InsertDate { get; set; }
    internal string JsonLookupData { get; set; }
    internal string JsonSubmittedLeadInfo { get; set; }
    public bool PersonFound { get; set; }
    public string PiplResponse { get; set; }
}

