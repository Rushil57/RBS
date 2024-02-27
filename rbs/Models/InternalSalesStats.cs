using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class InternalSalesStats
{
    public int TotalNewLead { get; set; }
    public int TotalFollowup { get; set; }
    public int TotalRawLead { get; set; }
	public int TotalRetry { get; set; }
    public int ToReady { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? AgentId { get; set; }
    public string ReportType { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalAppointmentSet { get; set; }
    public int TotalDailPerDay { get; set; }
    public int TotalAppointmentsDispatch { get; set; }
    public int Total { get; set; }
    public int Connects { get; set; }
    public int Today { get; set; }
    public int ApptToday { get; set; }
    public int Yesterday { get; set; }
    public int ApptYesterday { get; set; }
    public int ThisWeek { get; set; }
    public int ApptThisWeek { get; set; }
    public int LastWeek { get; set; }
    public int ApptLastWeek { get; set; }
    public DateTime InsertDate { get; set; }   
}

