public class DialRatio
{
    public int? AgentId { get; set; }
    public string Name { get; set; }
    public int? Dials { get; set; }
    public int? NoAnswer { get; set; }
    public decimal? NoAnswerRatio { get; set; }
    public int? Bad { get; set; }
    public decimal? BadRatio { get; set; }
    public int? Answered { get; set; }
    public decimal? AnsweredRatio { get; set; }
    public decimal? DialsPerLead { get; set; }
}

public class LeadRatio
{
    public int? AgentId { get; set; }
    public string Name { get; set; }
    public int? Leads { get; set; }
    public int? Retry { get; set; }
    public decimal? RetryRatio { get; set; }
    public int? Bad { get; set; }
    public decimal? BadRatio { get; set; }
    public int? NotInterested { get; set; }
    public decimal? NotInterestedRatio { get; set; }
    public int? Followup { get; set; }
    public decimal? FollowupRatio { get; set; }
    public int? ApptSet { get; set; }
    public decimal? ApptSetRatio { get; set; }
}

public class SAToInstall
{
    public int? AgentId { get; set; }
    public string Name { get; set; }
    public int? SchedAppt { get; set; }
    public int? Confirmed { get; set; }
    public int? FRDispatched { get; set; }
    public int? UFS { get; set; }
    public int? NoShow { get; set; }
    public int? Sold { get; set; }
    public int? Installed { get; set; }
}