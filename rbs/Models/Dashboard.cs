using System;
using System.Collections.Generic;

public class DashboardRequest
{
    public DateTime FromDateTime { get; set; }
    public DateTime ToDateTime { get; set; }
    public string Day { get; set; }
}

public class DashboardResponse
{
    public List<LeadGenStats> LeadGenStatsList { get; set; }
    public List<DialAgentStats> DialAgentStatsList { get; set; }
    public List<DialRatio> DialRatioList { get; set; }
    public List<LeadRatio> LeadRatioList { get; set; }
    public List<SAToInstall> SAToInstall { get; set; }
}