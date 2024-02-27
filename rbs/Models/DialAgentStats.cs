using System;
using System.Collections.Generic;

public class DialAgentStats
{
    public string FirstName { get; set; }
    public int? AgentId { get; set; }
    public int? Today { get; set; }
    public int? ApptToday { get; set; }
    public int? Yesterday { get; set; }
    public int? ApptYesterday { get; set; }
    public int? ThisWeek { get; set; }
    public int? ApptThisWeek { get; set; }
    public int? LastWeek { get; set; }
    public int? ApptLastWeek { get; set; }
    public int? LastDialed { get; set; }
    public bool IsClockedIn { get; set; }
    public bool Status { get; set; }
}

public class DialReport
{
    public int LeadId { get; set; }
    public string Marked { get; set; }
    public DateTime DialedTime { get; set; }
}

public class DialReportResponse
{
    public List<DialReport> TodaysDials { get; set; }
    public List<DialReport> YesterdaysDials { get; set; }
}
