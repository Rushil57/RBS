using System;
using System.Collections.Generic;

public class StatsAgent
{
    public InternalSalesStats GetStats()
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetStats();
    }
    public List<InternalSalesStats> TotalAppointmentsSet(InternalSalesStats salesStats)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.TotalAppointmentsSet(salesStats);
    }

    public List<InternalSalesStats> TotalDials()
    {
        return new MySqlDataAgent().TotalDials();
    }

    public List<InternalSalesStats> GenerateReport(InternalSalesStats salesStats)
    {
        var sqlDataAgent = new MySqlDataAgent();
        List<InternalSalesStats> stats = new List<InternalSalesStats>();

        if (salesStats.ReportType == "Dail")
        {
            stats = sqlDataAgent.TotalDailPerDay(salesStats);
        }
        if (salesStats.ReportType == "AppointmentSet")
        {
            stats = sqlDataAgent.TotalAppointmentsSet(salesStats);
        }
        if (salesStats.ReportType == "AppointmentDispatch")
        {
            stats = sqlDataAgent.TotalAppointmentsDispatch(salesStats);
        }
       
        return stats;
    }

    public List<LeadGenStats> GetLeadGenStats(DateTime fromDate, DateTime toDate)
    {
        return new MySqlDataAgent().GetLeadGenStats(fromDate, toDate);
    }

    public List<DialAgentStats> GetDialAgentStats()
    {
        return new MySqlDataAgent().GetDialAgentStats();
    }

    public List<DialRatio> GetDialRatio(DateTime FromDate, DateTime ToDate)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetDialRatioStats(FromDate, ToDate);
    }

    public List<LeadRatio> GetLeadRatio(DateTime FromDate, DateTime ToDate)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetLeadRatioStats(FromDate, ToDate);
    }

    public List<SAToInstall> GetSAToInstalls(DateTime FromDate, DateTime ToDate)
    {
        var sqlDataAgent = new MySqlDataAgent();
        return sqlDataAgent.GetSAToInstalls(FromDate, ToDate);
    }
}

