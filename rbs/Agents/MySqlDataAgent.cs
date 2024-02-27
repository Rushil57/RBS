using MySql.Data.MySqlClient;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using System.IO;
using System.Data;
using System.Threading.Tasks;

public class MySqlDataAgent
{
    private static readonly string database = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "prolink_dev" : "prolink";
    //private static readonly string database = "prolink";
    private static readonly string MySqlConnStr = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ?
        $"server=z.prolinkhome.net;User Id=pldev;Persist Security Info=True;database={database};Pwd=srUJydB5aBep8mVFneeZT7VbzwH3KxUn6unH;Convert Zero Datetime=True;Allow Zero Datetime=True;SslMode=none;Connect Timeout=60" :
       $"server=localhost;User Id=plapp;Persist Security Info=True;database={database};Pwd=pfBBG3FyPMWFrMJDjCuNdNxqTEkBhZ26ak94;Convert Zero Datetime=True;Allow Zero Datetime=True;SslMode=none;Connect Timeout=60";
    //private static readonly string MySqlConnStr = $"server=z.prolinkhome.net;User Id=pldev;Persist Security Info=True;database={database};Pwd=srUJydB5aBep8mVFneeZT7VbzwH3KxUn6unH;Convert Zero Datetime=True;Allow Zero Datetime=True;SslMode=none;Connect Timeout=60";    

    #region Lead
    public Lead GetRandomLead()
    {
        var lead = new Lead();
        //weighted with 52 highest, then 50, then 1
        //var sqlQuery = $@"select leadid from (
        //    select leadid, case leadstatusid when 52 then 1 when 1 then 2 when 50 then 3 end leadstatusweight from leads where leadstatusid in (1,50) AND leadtypeid in (0,1) AND (followupdate is null || followupdate < Now())) as leadqueue
        //    order by log(RAND())/leadstatusweight limit 1;";

        //totally random.  Mix the new and the retry
        //var sqlQuery = $@"select leadid from (
        //select leadid from leads where leadstatusid in (1,50) AND leadtypeid in (0,1) AND (followupdate is null || followupdate < Now())) as leadqueue
        //order by RAND() limit 1;";

        //Give them in order of solddate
        //var sqlQuery = $@"select leadid from (
        //select leadid, datediff(now(), solddate)/7 weight from leads where leadstatusid in (1,50) AND leadtypeid in (0,1) AND COALESCE(followupdate, now()) <= Now() order by solddate) as a
        //order by rand() limit 1";

        //GetRandomLeadByField giving you the city weighted leads
        //GetRandomLead  soldweek weight
        var sqlQuery = @"call GetRandomLead";

        //new that drives off the 
        //var sqlQuery = @"call getrandomleadplus";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.QuerySingleOrDefault<Lead>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetRandomLead", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public Lead GetRandomTOLead()
    {
        var lead = new Lead();

        var sqlQuery = $@"select leadid from (
        select leadid, datediff(now(), insertdate)/ 7 weight from leads where leadstatusid in (1, 50) AND leadtypeid = 3 AND COALESCE(followupdate, now()) <= Now() order by insertdate) as a
            order by rand() limit 1";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.QuerySingleOrDefault<Lead>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetRandomLead", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public Lead GetRandomRawLead()
    {
        var lead = new Lead();
        //var sqlQuery = $@"SELECT * from leads where leadid in
        //        (select leadid from (select leadid from leads
        //        where leadstatusid in (0,4) and datediff(now(), coalesce(lastfetched, date_add(now(), interval -3 DAY))) >= 3 and COALESCE(followupdate, now()) <= Now()
        //        ORDER BY log(RAND())/(leadstatusid + 1) LIMIT 1) as t);";

        var sqlQuery = "CALL GetRandomRawLead;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.Query<Lead>(sqlQuery).SingleOrDefault();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetRandomRawLead", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public RawLeadCount TotalRawLeads()
    {
        string sqlQuery = $@"
select * from
(Select count(*) as TX from leads where leadstatusid IN (0, 4)
and COALESCE(followupdate, now()) <= Now()
and datediff(now(), coalesce(lastfetched, date_add(now(), interval -3 DAY))) >= 3
and solddate > date_add(now(), interval -30 DAY)
and state = 'TX') as TX,
(Select count(*) as AZ from leads where leadstatusid IN (0, 4)
and COALESCE(followupdate, now()) <= Now()
and datediff(now(), coalesce(lastfetched, date_add(now(), interval -3 DAY))) >= 3
and solddate > date_add(now(), interval -30 DAY)
and state = 'AZ') as AZ;";
        RawLeadCount count = new RawLeadCount();
        var leads = new List<Lead>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                count = conn.QuerySingleOrDefault<RawLeadCount>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.TotalRawLeads", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            count = null;
        }
        return count;
    }

    public List<InternalSalesStats> TotalDials()
    {
        var stats = new List<InternalSalesStats>();
        string sqlQuery = @"SELECT SUM(phone1dial+phone2dial+phone3dial+phone4dial+phone5dial) Total,
                           sum(case when phone1status=3 OR phone2status=3 OR phone3status=3 OR phone4status=3 OR phone5status=3 then 1 else 0 end) Connects,
                           agentid
                      from leadaudit
                      where leadstatusid in (50, 51, 52, 53)
                      AND CONVERT_TZ(insertdate,'+00:00','-07:00')
                              between date_format(convert_tz(date_sub(utc_timestamp(), interval 1 day),'+00:00','-07:00'), '%y-%m-%d 00:00:00')
                              AND date_format(convert_tz(date_sub(utc_timestamp(), interval 1 day),'+00:00','-07:00'), '%y-%m-%d 23:59:59')
                    Group by agentId;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<InternalSalesStats>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }
        return stats;
    }

    public List<InternalSalesStats> TotalDailPerDay(InternalSalesStats salesStats)
    {
        var stats = new List<InternalSalesStats>();
        string sqlQuery = "";
        if (salesStats.AgentId == 0)
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00')InsertDate, SUM(phone1dial+phone2dial+phone3dial+phone4dial+phone5dial) as TotalDailPerDay, AgentId
                            from leadaudit
                            where leadstatusid in (50, 51, 52, 53) 
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 23:59:59')
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,NULL,
                            SUM(phone1dial+phone2dial+phone3dial+phone4dial+phone5dial) Total, NULL
                            FROM leadaudit 
                            Where CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 23:59:59')";
        }
        else
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00')InsertDate, SUM(phone1dial+phone2dial+phone3dial+phone4dial+phone5dial) as TotalDailPerDay, AgentId
                            from leadaudit
                            where leadstatusid in (50, 51, 52, 53) AND agentId = {salesStats.AgentId}
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 23:59:59')
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,NULL,
                            SUM(phone1dial+phone2dial+phone3dial+phone4dial+phone5dial) Total, NULL
                            FROM leadaudit 
                            Where agentId = {salesStats.AgentId} AND CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 23:59:59')";
        }

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<InternalSalesStats>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }
        return stats;
    }

    public List<InternalSalesStats> TotalAppointmentsSet(InternalSalesStats salesStats)
    {
        string sqlQuery = "";
        if (salesStats.AgentId == 0)
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00')InsertDate,Count(*) as TotalAppointmentSet,
                            AgentId
                            from leadaudit
                            where leadstatusid in (53) 
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59')
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,null,
                            Count(*) Total, null
                            FROM leadaudit  
                            Where leadstatusid in (53)  And CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59')";
        }
        else
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00')InsertDate,Count(*) as TotalAppointmentSet,
                            AgentId
                            from leadaudit
                            where leadstatusid in (53) AND agentId = {salesStats.AgentId}
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59')
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,null,
                            Count(*) Total, null
                            FROM leadaudit  
                            Where leadstatusid in (53) AND agentId = {salesStats.AgentId}  And CONVERT_TZ(insertdate,'+00:00','-07:00') BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}' ,'+00:00','-07:00'), '%y-%m-%d 23:59:59')";
        }
        var stats = new List<InternalSalesStats>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<InternalSalesStats>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }

        return stats;
    }

    public List<InternalSalesStats> TotalAppointmentsDispatch(InternalSalesStats salesStats)
    {
        string sqlQuery = "";
        if (salesStats.AgentId == 0)
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00') InsertDate,Count(*) as TotalAppointmentsDispatch,
                            AgentId
                            from leadaudit
                            where leadstatusid in (54) 
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00')  BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59') 
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,null,
                            Count(*) Total, null
                            FROM leadaudit  
                            Where leadstatusid in (54)  And CONVERT_TZ(insertdate,'+00:00','-07:00')  BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59')";
        }
        else
        {
            sqlQuery = $@"select (Select firstname from agents where agentId = leadaudit.agentid) as FirstName,
                            CONVERT_TZ(insertdate,'+00:00','-07:00') InsertDate,Count(*) as TotalAppointmentsDispatch,
                            AgentId
                            from leadaudit
                            where leadstatusid in (54) AND agentId = {salesStats.AgentId}
                            AND  CONVERT_TZ(insertdate,'+00:00','-07:00')  BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59') 
                            Group by date_format(convert_tz(insertdate,'+00:00','-07:00'), '%y-%m-%d'), agentid
                            UNION ALL
                            SELECT 'Total' ,null,
                            Count(*) Total, null
                            FROM leadaudit  
                            Where leadstatusid in (54) AND agentId = {salesStats.AgentId}  And CONVERT_TZ(insertdate,'+00:00','-07:00')  BETWEEN 
                            date_format(convert_tz('{salesStats.FromDate.ToString("yyyy-MM-dd HH:mm")}','+00:00','-07:00'), '%y-%m-%d 00:00:00')
                            AND 
                            date_format(convert_tz('{salesStats.ToDate.ToString("yyyy-MM-dd HH:mm")}','+00:00' ,'-07:00'), '%y-%m-%d 23:59:59')";
        }
        var stats = new List<InternalSalesStats>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<InternalSalesStats>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }

        return stats;
    }

    public InternalSalesStats GetStats()
    {
        string sqlQuery = $@"select
sum(case when leadstatusid = 52 and followupdate < Now() AND leadtypeid in (0, 1) then 1 else 0 end) TotalFollowup,
sum(case when leadstatusid = 50 and followupdate < Now() AND leadtypeid in (0, 1) then 1 else 0 end) TotalRetry,
sum(case when leadstatusid = 1 AND leadtypeid in (0, 1) then 1 else 0 end) TotalNewLead,
sum(case when leadtypeid = 3 AND leadstatusid IN (1, 50, 52, 53) and (followupdate is null OR followupdate < Now()) then 1 else 0 end) ToReady
from leads";
        var stats = new InternalSalesStats();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.QuerySingleOrDefault<InternalSalesStats>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }
        return stats;
    }

    public Lead GetLeadById(int leadId)
    {
        var lead = new Lead();
        var sqlQuery = $@"SELECT leads.* FROM leads where leadId = {leadId}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.QuerySingleOrDefault<Lead>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public Lead GetLeadForDispatchById(int leadId)
    {
        var lead = new Lead();
        var sqlQuery = $@"SELECT * FROM leads where leadId = {leadId}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.QuerySingleOrDefault<Lead>(sqlQuery);
                var mstdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
                lead.SchedApptDate = mstdate;
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadForDispatchById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public List<Lead> SearchLeadByAddress(string address)
    {
        var leads = new List<Lead>();
        var sqlQuery = $"select * from leads where address collate latin1_swedish_ci = '{address}'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadsByField", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }
        return leads;
    }

    public List<Lead> GetLeadsToDispatch()
    {
        var leads = new List<Lead>();

        string sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, l.schedapptdate, l.timezone
from leads l
WHERE l.schedapptdate IS NOT NULL
AND leadstatusid = 53
ORDER BY schedapptdate desc";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadsToDispatch PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }
        foreach (var lead in leads)
        {
            Logger.Log("MySqlDataAgent.GetLeadsToDispatch TZ", $"SA:{lead.SchedApptDate} TZ:{lead.TimeZone}");
            var mstdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
            lead.SchedApptDate = mstdate;
        }
        return leads;
    }

    public List<Lead> GetConfirmedLeadsToDispatch()
    {
        var leads = new List<Lead>();

        string sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, l.schedapptdate, l.leadstatusid, l.timezone
from leads l
WHERE l.schedapptdate IS NOT NULL
AND leadstatusid in (54, 55, 56)
ORDER BY schedapptdate desc";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetConfirmedLeadsToDispatch PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }
        foreach (var lead in leads)
        {
            Logger.Log("MySqlDataAgent.GetConfirmedLeadsToDispatch TZ", $"SA:{lead.SchedApptDate} TZ:{lead.TimeZone}");
            var mstdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
            lead.SchedApptDate = mstdate;
        }
        return leads;
    }

    public List<Lead> GetLeadsByField(string searchTerm, string field)
    {
        var leads = new List<Lead>();
        string sqlQuery = string.Empty;

        if (field == "to")
        {
            sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate,l.followupdate
from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
where  leadtypeid = 3
AND l.leadstatusid IN (1, 9, 10, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60)
AND (l.followupdate IS NULL OR l.followupdate < NOW())
ORDER BY followupdate DESC";
        }
        else if (field == "installed")
        {
            sqlQuery = $@"select leadid, firstname, lastname, address, city, solddate, followupdate, leadstatusid
            from leads WHERE leadstatusid = 57 ORDER BY schedapptdate desc";
        }
        else if (field == "agentfollowup")
        {
            sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate, l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
            WHERE l.leadstatusid = 52
            AND (l.followupdate IS NULL OR l.followupdate < NOW())
            and leadid in (select distinct(leadid) from leadaudit where agentid = {searchTerm} and leadstatusid = 52)
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc";
        }
        else //default search (follow ups)
        {
            sqlQuery = $@"
            select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate, l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
            WHERE l.leadstatusid = 52
            AND (l.followupdate IS NULL OR l.followupdate < NOW())
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc";
        }

        if (!string.IsNullOrEmpty(searchTerm) && field != "agentfollowup")
        {
            var fieldSearch = (field == "name") ? "CONCAT(l.firstname, ' ', l.lastname)" : field;
            if (field == "address")
            {
                fieldSearch = "CONCAT(l.address, ',', l.city)";
                sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate,l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid 
            where  {fieldSearch} collate latin1_swedish_ci LIKE '%{searchTerm}%' 
            AND l.leadstatusid IN (1, 9, 10, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60)
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc            
            LIMIT 20";
            }
            else if (field == "phone")
            {
                fieldSearch = $@"(phone1 LIKE '%{searchTerm}%' OR phone2 LIKE '%{searchTerm}%' OR phone3 LIKE '%{searchTerm}%'
                                  OR phone4 LIKE '%{searchTerm}%' OR phone5 LIKE '%{searchTerm}%')";

                sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate, l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
            where  {fieldSearch}  
            AND l.leadstatusid IN (1, 9, 10, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60)
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc            
            LIMIT 20";
            }
            else if (field == "leadid") //
            {
                sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate,l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
            where leadid={searchTerm} 
            AND l.leadstatusid IN (1, 9, 10, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60)
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc
            LIMIT 20";
            }
            else
            {
                sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, ls.leadstatustext as statustext, l.solddate,l.followupdate
            from leads l JOIN leadstatus ls on l.leadstatusid = ls.leadstatusid
            where  {fieldSearch} collate latin1_swedish_ci LIKE '%{searchTerm}%' 
            AND l.leadstatusid IN (1, 9, 10, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60)
            ORDER BY case l.leadstatusid when 1 then 2 when 52 then 1 when 50 then 3 end, followupdate DESC, solddate asc            
            LIMIT 20";
            }
        }
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadsByField", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }
        return leads;
    }

    public bool SetLeadToFollowUp(Lead lead)
    {
        var runSuccessful = true;
        var followupdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var updateLead = $@"UPDATE leads SET leadstatusid=52, followupdate='{followupdate}' Where leadid = {lead.LeadId};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                //we don't need to insert an audit record because we don't need stats for these.
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.SetLeadToFollowUp", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool SetLeadToSold(Lead lead)
    {
        var runSuccessful = true;
        var updateLead = $@"UPDATE leads SET leadstatusid=56 Where leadid = {lead.LeadId};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                lead.AgentIdSubmitting = GetLastAgentScheduledAppt(lead.LeadId);
                lead.LeadStatusId = 56;
                InsertNonDialAudit(lead);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.SetLeadToInstalled", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool SetLeadToInstalled(Lead lead)
    {
        var runSuccessful = true;
        var updateLead = $@"UPDATE leads SET leadstatusid=57 Where leadid = {lead.LeadId};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                lead.AgentIdSubmitting = GetLastAgentScheduledAppt(lead.LeadId);
                lead.LeadStatusId = 57;
                InsertNonDialAudit(lead);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.SetLeadToInstalled", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool SetLeadToREF(Lead lead)
    {
        var runSuccessful = true;
        var updateLead = $@"UPDATE leads SET leadstatusid=53, leadtypeid=4, schedapptdate=NULL Where leadid = {lead.LeadId};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                //don't need an audit for this since we don't want to affect stats
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.SetLeadToREF", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public int InsertLead(Lead lead)
    {
        int lastinsertId = -1;
        string soldDateAsString = Util.ConvertToTZ(lead.SoldDate, lead.TimeZone).ToString("yyyy-MM-dd");
        if (lead.SoldDate.ToString("yyyy-MM-dd").Contains("0001-01-01"))
        {
            Logger.Log("MySqlDataAgent.InsertLead bad date", "SoldDate contains 0001-01-01.  Set to Jan 1 as default");
            soldDateAsString = "2018-10-01";
        }
        else
        {
            soldDateAsString = Util.ConvertToTZ(lead.SoldDate, lead.TimeZone).ToString("yyyy-MM-dd");
        }

        var insertLead = $@"INSERT INTO leads 
                    (firstname, lastname, email, address, city, state, postal, phone1, phone2, phone3, phone4, phone5, county, leadstatusid, solddate,leadtypeid,
                        phone1status, phone2status, phone3status, phone4status, phone5status,homevalue) 
                VALUES ('{lead.FirstName}', '{lead.LastName}', '{lead.Email}', '{lead.Address.Trim()}', '{lead.City.Trim()}', '{lead.State.Trim()}', '{lead.Postal.Trim()}', 
                    '{lead.Phone1}', '{lead.Phone2}', '{lead.Phone3}', '{lead.Phone4}','{lead.Phone5}','{lead.County}', {lead.LeadStatusId}, '{soldDateAsString}', '{lead.LeadTypeId}'
                    ,{lead.Phone1Status},{lead.Phone2Status},{lead.Phone3Status},{lead.Phone4Status},{lead.Phone5Status},{lead.HomeValue});
                SELECT LAST_INSERT_ID();";
        var auditInsert = string.Empty;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lastinsertId = Convert.ToInt32(conn.ExecuteScalar(insertLead));
                auditInsert = $@"INSERT INTO leadaudit 
	                (leadid, leadtypeid, leadstatusid, agentid,
	                phone1status, phone2status, phone3status, phone4status, phone5status, jsonlead) 
                SELECT leadid, leadtypeid, leadstatusid, {lead.AgentIdSubmitting},
	                phone1status, phone2status, phone3status, phone4status, phone5status, '{JsonConvert.SerializeObject(lead)}'
                FROM leads
                WHERE leadid = {lastinsertId};";
                var affectedRows = conn.Execute(auditInsert);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertLead", $"Query: {insertLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lastinsertId = -1;
        }

        return lastinsertId;
    }

    public int InsertLeadWithFollowUp(Lead lead)
    {
        int lastinsertId = -1;
        string soldDateAsString = Util.ConvertToTZ(lead.SoldDate, lead.TimeZone).ToString("yyyy-MM-dd");
        string fuDateAsString = Util.ConvertToTZ(lead.FollowUpDate, lead.TimeZone).ToString("yyyy-MM-dd");
        if (lead.SoldDate.ToString("yyyy-MM-dd").Contains("0001-01-01"))
        {
            Logger.Log("MySqlDataAgent.InsertLead bad date", "SoldDate contains 0001-01-01.  Set to Jan 1 as default");
            soldDateAsString = "2018-10-01";
        }
        else
        {
            soldDateAsString = Util.ConvertToTZ(lead.SoldDate, lead.TimeZone).ToString("yyyy-MM-dd");
        }

        var insertLead = $@"INSERT INTO leads 
                    (firstname, lastname, email, address, city, state, postal, phone1, phone2, phone3, phone4, phone5, county, leadstatusid, solddate,leadtypeid,
                        phone1status, phone2status, phone3status, phone4status, phone5status,homevalue,followupdate) 
                VALUES ('{lead.FirstName}', '{lead.LastName}', '{lead.Email}', '{lead.Address.Trim()}', '{lead.City.Trim()}', '{lead.State.Trim()}', '{lead.Postal.Trim()}', 
                    '{lead.Phone1}', '{lead.Phone2}', '{lead.Phone3}', '{lead.Phone4}','{lead.Phone5}','{lead.County}', {lead.LeadStatusId}, '{soldDateAsString}', '{lead.LeadTypeId}'
                    ,{lead.Phone1Status},{lead.Phone2Status},{lead.Phone3Status},{lead.Phone4Status},{lead.Phone5Status},{lead.HomeValue}, '{fuDateAsString}');
                SELECT LAST_INSERT_ID();";
        var auditInsert = string.Empty;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lastinsertId = Convert.ToInt32(conn.ExecuteScalar(insertLead));
                auditInsert = $@"INSERT INTO leadaudit 
	                (leadid, leadtypeid, leadstatusid, agentid,
	                phone1status, phone2status, phone3status, phone4status, phone5status, jsonlead) 
                SELECT leadid, leadtypeid, leadstatusid, {lead.AgentIdSubmitting},
	                phone1status, phone2status, phone3status, phone4status, phone5status, '{JsonConvert.SerializeObject(lead)}'
                FROM leads
                WHERE leadid = {lastinsertId};";
                var affectedRows = conn.Execute(auditInsert);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertLead", $"Query: {insertLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lastinsertId = -1;
        }

        return lastinsertId;
    }

    public bool UpdateLeadFromSADispatch(Lead lead)
    {
        bool IsCountState = lead.IsCountState;
        var runSuccessful = true;
        string schedApptDate = Util.ConvertDateToUTC(lead.SchedApptDate).ToString("yyyy-MM-dd HH:mm:ss");
        var updateLead = $@"UPDATE leads SET leadstatusid={lead.LeadStatusId}, schedapptdate='{schedApptDate}' Where leadid = {lead.LeadId};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                lead.AgentIdSubmitting = GetLastAgentScheduledAppt(lead.LeadId);
                if (lead.LeadStatusId != 53) //was causing the double-count
                {
                    if (IsCountState == true)
                    {
                        InsertNonDialAudit(lead);
                    }
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.UpdateLeadFromSADispatch LEAD BUG", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool UpdateLead(Lead lead)
    {
        //Logger.Log("MySqlDataAgent.UpdateLeadDail start LEAD BUG", $"MySqlDataAgent.UpdateLeadDail start");
        lead.LastName = lead.LastName.Replace("'", "");
        lead.FirstName = lead.FirstName.Replace("'", "");
        var runSuccessful = true;
        string followupdate = "";
        string schedApptDate = "";

        //this first if logic really belongs in agent - but moved here to simplify date handling
        //this is so that when a call is follow-up, but marked as No Answer, that it stays a FU
        var auditStatusId = lead.LeadStatusId;
        if (lead.LeadStatusId == 50
            && lead.FormerLeadStatusId == 52)
        {
            lead.LeadStatusId = 52;
            followupdate = DateTime.UtcNow.AddHours(Util.RETRYHOURS).ToString("yyyy-MM-dd HH:mm:ss");
        }
        else if (lead.LeadStatusId == 53) //Sched Appt
        {
            schedApptDate = Util.ConvertDateToUTC(lead.SchedApptDate).ToString("yyyy-MM-dd HH:mm:ss");
        }
        else if (lead.LeadStatusId == 50) //No Answer, i.e. Retry
        {
            followupdate = DateTime.UtcNow.AddHours(Util.RETRYHOURS).ToString("yyyy-MM-dd HH:mm:ss");
        }
        else if (lead.LeadStatusId == 52) //FollowUp actually set
        {
            followupdate = Util.ConvertDateToUTC(lead.FollowUpDate).ToString("yyyy-MM-dd HH:mm:ss");
        }

        string updateLead = "";
        if (schedApptDate != "")
        {
            updateLead = $@"UPDATE leads SET firstname = '{lead.FirstName}', lastname = '{lead.LastName}', email = '{lead.Email}', address = '{lead.Address}', city = '{lead.City}', state='{lead.State}',
                    phone1 = '{lead.Phone1}', phone2 = '{lead.Phone2}', phone3 = '{lead.Phone3}', phone4 = '{lead.Phone4}', phone5 = '{lead.Phone5}',
                    phone1status='{lead.Phone1Status}', phone2status ='{lead.Phone2Status}', phone3status='{lead.Phone3Status}', phone4status='{lead.Phone4Status}',
                    phone5status='{lead.Phone5Status}', leadstatusid={lead.LeadStatusId}, schedapptdate='{schedApptDate}' Where leadid = {lead.LeadId};";
        }
        else if (followupdate != "")
        {
            updateLead = $@"UPDATE leads SET firstname = '{lead.FirstName}', lastname = '{lead.LastName}', email = '{lead.Email}', address = '{lead.Address}', city = '{lead.City}', state='{lead.State}',
                    phone1 = '{lead.Phone1}', phone2 = '{lead.Phone2}', phone3 = '{lead.Phone3}', phone4 = '{lead.Phone4}', phone5 = '{lead.Phone5}',
                    phone1status='{lead.Phone1Status}', phone2status ='{lead.Phone2Status}', phone3status='{lead.Phone3Status}', phone4status='{lead.Phone4Status}',
                    phone5status='{lead.Phone5Status}', leadstatusid={lead.LeadStatusId}, followupdate='{followupdate}' Where leadid = {lead.LeadId};";
        }
        else
        {
            updateLead = $@"UPDATE leads SET firstname = '{lead.FirstName}', lastname = '{lead.LastName}', email = '{lead.Email}', address = '{lead.Address}', city = '{lead.City}', state='{lead.State}', postal = '{lead.Postal}', 
                    phone1 = '{lead.Phone1}', phone2 = '{lead.Phone2}', phone3 = '{lead.Phone3}', phone4 = '{lead.Phone4}', phone5 = '{lead.Phone5}', leadstatusid={lead.LeadStatusId},
                    phone1status='{lead.Phone1Status}', phone2status ='{lead.Phone2Status}', phone3status='{lead.Phone3Status}', phone4status='{lead.Phone4Status}',
                    phone5status='{lead.Phone5Status}', leadstatusid={lead.LeadStatusId}, leadtypeid={lead.LeadTypeId} Where leadid = {lead.LeadId};";
        }

        var auditInsert = string.Empty;

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var affectedRows = conn.Execute(updateLead);
                //Logger.Log("MySqlDataAgent.UpdateLeadDail affectedRows 1 LEAD BUG", $"Query: {affectedRows}");

                auditInsert = $@"INSERT INTO leadaudit 
	                (leadid, leadtypeid, leadstatusid, agentid,
	                phone1status, phone2status, phone3status, phone4status, phone5status,phone1dial,phone2dial,phone3dial,
                    phone4dial, phone5dial, jsonlead) 
                SELECT leadid, leadtypeid, {auditStatusId}, {lead.AgentIdSubmitting},
	                phone1status, phone2status, phone3status, phone4status, phone5status,'{lead.Phone1Dial}','{lead.Phone2Dial}',
                    '{lead.Phone3Dial}','{lead.Phone4Dial}','{lead.Phone5Dial}', '{JsonConvert.SerializeObject(lead)}'
                FROM leads
                WHERE leadid = {lead.LeadId};";

                //Logger.Log("MySqlDataAgent.UpdateLeadDail auditInsert", $"Query: {auditInsert}");
                affectedRows = conn.Execute(auditInsert);
                //Logger.Log("MySqlDataAgent.UpdateLeadDail affectedRows 2", $"Query: {affectedRows}");
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.UpdateLeadDail LEAD BUG", $"Query: {updateLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool InsertNonDialAudit(Lead lead)
    {
        var runSuccessful = true;
        var auditInsert = string.Empty;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                auditInsert = $@"INSERT INTO leadaudit (leadid, leadtypeid, leadstatusid, agentid)
VALUES ({lead.LeadId}, {lead.LeadTypeId}, {lead.LeadStatusId}, {lead.AgentIdSubmitting});";

                Logger.Log("MySqlDataAgent.InsertNonDialAudit auditInsert", $"Query: {auditInsert}");
                var affectedRows = conn.Execute(auditInsert);
                Logger.Log("MySqlDataAgent.InsertNonDialAudit affectedRows 2", $"Query: {affectedRows}");
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertNonDialAudit LEAD BUG", $"Query: {auditInsert}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public int GetLastAgentScheduledAppt(int leadid)
    {
        int agentid = -1;
        string sqlQuery = $@"Select agentid from leadaudit where leadstatusid = 53 and leadid = {leadid} order by insertdate desc limit 1";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agentid = conn.QueryFirstOrDefault<int>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLastAgentScheduledAppt", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return agentid;
    }

    public CalendarLeads GetCalendarLeads()
    {
        var calendarLeads = new CalendarLeads();

        string sqlQuery = $@"select * from leads where leadstatusid >= 52 
        AND leadstatusid NOT IN (58, 60)
        AND convert_tz(schedapptdate, '+00:00', '-07:00') >= previousweek(1, '-07:00') order by schedapptdate asc;";

        var _leads = new List<Lead>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                _leads = conn.Query<Lead>(sqlQuery).AsList();

                DateTime currentDate = DateTime.Now;
                foreach (var lead in _leads)
                {
                    var tzdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
                    lead.SchedApptDate = tzdate;
                    lead.DispatchedRepsName = GetDispatchedRepsName(lead);
                    currentDate = Util.ConvertToTZ(DateTime.Now, lead.TimeZone);
                }

                calendarLeads.Today = _leads.FindAll(a => a.SchedApptDate.Date == currentDate.Date).ToList();
                calendarLeads.Yesterday = _leads.FindAll(a => a.SchedApptDate.Date.AddDays(1) == currentDate.Date).ToList();

                DateTime startDayOfWeek = currentDate.Date.AddDays(-1 * (int)(currentDate.Date.DayOfWeek));
                DateTime endDayOfWeek = currentDate.Date.AddDays(6 - (int)currentDate.Date.DayOfWeek);

                calendarLeads.ThisWeek = _leads.FindAll(a => a.SchedApptDate.Date >= startDayOfWeek.Date.AddDays(1) && a.SchedApptDate.Date <= endDayOfWeek).ToList();

                DateTime LastWeekEndDay = startDayOfWeek.AddDays(-1);
                DateTime LastWeekStartDay = currentDate.Date.AddDays(-7 - (int)currentDate.Date.DayOfWeek);

                calendarLeads.LastWeek = _leads.FindAll(a => a.SchedApptDate.Date >= LastWeekStartDay.Date.AddDays(1) && a.SchedApptDate.Date <= LastWeekEndDay).ToList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCalendarLeads PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            _leads = null;
        }

        ////Today
        //sqlQuery = $@"select * from leads where leadstatusid >= 52 AND leadstatusid <> 58 AND convert_tz(schedapptdate, '+00:00', '-07:00') between today(1, '-07:00') and today(2, '-07:00') order by schedapptdate desc";

        ////Yesterday
        //sqlQuery = $@"select * from leads where leadstatusid >= 52 AND leadstatusid <> 58 AND convert_tz(schedapptdate, '+00:00', '-07:00') between yesterday(1, '-07:00') and yesterday(2, '-07:00') order by schedapptdate desc";

        ////This Week
        //sqlQuery = $@"select * from leads where leadstatusid >= 52 AND leadstatusid <> 58 AND convert_tz(schedapptdate, '+00:00', '-07:00') between thisweek(1, '-07:00') and thisweek(2, '-07:00') order by schedapptdate desc";

        ////Last Week
        //sqlQuery = $@"select * from leads where leadstatusid >= 52 AND leadstatusid <> 58 AND convert_tz(schedapptdate, '+00:00', '-07:00') between previousweek(1, '-07:00') and previousweek(2, '-07:00') order by schedapptdate desc";

        //future
        //sqlQuery = $@"select * from leads where leadstatusid >= 52 AND leadstatusid <> 58 AND convert_tz(schedapptdate, '+00:00', '-07:00') > today(2, '-07:00') order by schedapptdate desc";

        //UFS
        sqlQuery = $@"select * from leads where leadstatusid = 58 order by schedapptdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                _leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCalendarLeads PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            _leads = null;
        }
        foreach (var lead in _leads)
        {
            var tzdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
            lead.SchedApptDate = tzdate;
            lead.DispatchedRepsName = GetDispatchedRepsName(lead);
        }
        calendarLeads.UFS = _leads;

        //reschedule
        sqlQuery = $@"select * from leads where leadstatusid = 60 order by schedapptdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                _leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCalendarLeads PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            _leads = null;
        }
        foreach (var lead in _leads)
        {
            var tzdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
            lead.SchedApptDate = tzdate;
            lead.DispatchedRepsName = GetDispatchedRepsName(lead);
        }
        calendarLeads.Reschedule = _leads;

        //Get lastweek + current week lead data in a grid
        sqlQuery = $@"select l.leadid, l.firstname, l.lastname, l.address, l.city, l.phone1, ls.leadstatustext as StatusText, la.insertdate
from leads l
join leadstatus ls
on l.leadstatusid = ls.leadstatusid
join (select leadid, max(insertdate) insertdate
from leadaudit
where leadstatusid >= 53 and convert_tz(insertdate, '+00:00', '-07:00') > previousweek(1, '-07:00')
group by leadid having max(insertdate)
) la
on l.leadid = la.leadid
order by insertdate desc";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                _leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCalendarLeads PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            _leads = null;
        }
        calendarLeads.LastWeekThisWeekLeadData = _leads;

        return calendarLeads;
    }

    private string GetDispatchedRepsName(Lead lead)
    {
        var repsName = string.Empty;
        if (lead.LeadStatusId == 55
            || lead.LeadStatusId == 56
            || lead.LeadStatusId == 58
            || lead.LeadStatusId == 59)
        {
            string sqlQuery = $@"select CONCAT(a.firstname, ' ', a.lastname) as DispatchedName
            from leadaudit la join agents a on la.agentid = a.agentid
            where la.leadid = {lead.LeadId}
            and la.leadstatusid = 55 and agenttypeid = 20
            order by la.insertdate limit 1";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
                {
                    repsName = conn.QueryFirstOrDefault<string>(sqlQuery);
                }
            }
            catch (Exception err)
            {
                Logger.Log("MySqlDataAgent.GetDispatchedRepsName", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            }
        }

        return repsName;
    }

    private string GetSARepsName(Lead lead)
    {
        var repsName = string.Empty;
        string sqlQuery = $@"select CONCAT(a.firstname, ' ', a.lastname) as DispatchedName
            from leadaudit la join agents a on la.agentid = a.agentid
            where la.leadid = {lead.LeadId}
            and la.leadstatusid = 53 -- and agenttypeid in (40,60)
            order by la.insertdate limit 1";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                repsName = conn.QueryFirstOrDefault<string>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetSARepsName", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }

        return repsName;
    }

    public List<Lead> GetRepLeads(int agentId, string password)
    {
        var leads = new List<Lead>();

        string sqlQuery = $@"select lastname, leadstatusid, address, city from leads
where leadid in
      (select la.leadid from leadaudit la
join agents a
on la.agentid = a.agentid
where la.leadstatusid = 55
and la.agentid = {agentId}
and a.password = '{password}'
and convert_tz(la.insertdate, '+00:00', '-07:00') >= ( CURDATE() - INTERVAL 2 week ))";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadsToDispatch PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }

        return leads;
    }

    public Lead GetLeadByPhone(string phone)
    {
        var fromPhone = phone.Trim().Replace("+1", "");
        fromPhone = (fromPhone.StartsWith("1")) ? fromPhone.Substring(1) : fromPhone;
        var lead = new Lead();
        var sqlQuery = $@"SELECT * FROM leads where phone1 = {fromPhone} or phone2 = {fromPhone} or phone3 = {fromPhone} or phone4 = {fromPhone} or phone5 = {fromPhone} limit 1";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lead = conn.QuerySingleOrDefault<Lead>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadByPhone", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lead = null;
        }
        return lead;
    }

    public Account GetAccountByPhone(string phone)
    {
        var fromPhone = phone.Trim().Replace("+1", "");
        fromPhone = (fromPhone.StartsWith("1")) ? fromPhone.Substring(1) : fromPhone;
        var account = new Account();
        var sqlQuery = $@"SELECT * FROM account where phone1 = {fromPhone} or phone2 = {fromPhone} limit 1";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.QuerySingleOrDefault<Account>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadByPhone", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }

    public CalendarLeads GetCalendarSalesClosedLeads()
    {
        var calendarLeads = new CalendarLeads();

        string sqlQuery = $@"select * from leads where leadid in
(select leadid from leadaudit where leadstatusid > 55
and convert_tz(insertdate, '+00:00', '-07:00') > thisweek(1, '-07:00')
GROUP BY CONCAT(leadid, '-',leadstatusid ))
and leadstatusid >= 55";

        var _leads = new List<Lead>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                _leads = conn.Query<Lead>(sqlQuery).AsList();

                DateTime currentDate = DateTime.Now;
                foreach (var lead in _leads)
                {
                    var tzdate = Util.ConvertToTZ(lead.SchedApptDate, lead.TimeZone);
                    lead.SchedApptDate = tzdate;
                    lead.DispatchedRepsName = GetSARepsName(lead);
                }
                calendarLeads.ThisWeek = _leads;
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCalendarSalesClosedLeads PLWEB.ERROR TZ", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            _leads = null;
        }

        return calendarLeads;
    }
    #endregion

    #region Account
    public int CreateAccount(Account account)
    {
        var success = true;
        var lastinsertId = -1;

        if (!string.IsNullOrEmpty(account.Phone1))
        {
            account.Phone1 = account.Phone1.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.Phone2))
        {
            account.Phone2 = account.Phone2.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.EmerPhone1))
        {
            account.EmerPhone1 = account.EmerPhone1.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.EmerPhone2))
        {
            account.EmerPhone2 = account.EmerPhone2.Replace("-", "");
        }

        var insertLead = $@"INSERT INTO account 
        (leadid, accountstatusId, firstname, lastname, company, 
        address, city, state, zip, county, baddress, bcity, bstate, bzip, email, hoverification, 
        phone1, phone2, emername1, emerphone1, emername2, emerphone2, dob, area, 
        signalsconf, onlineconf, preinstall, postinstall, accountholder, monitoring, 
        installeddate, saledate, techscheddate, submitteddate, fundeddate, chargedbackdate, datecancelled, 
        cancelreason, contractterm, creditgrade, creditscore, contractstartdate, mmr, 
        buyoutamount,verbalpasscode,fieldrepid,techid) 
        VALUES 
        ({account.LeadId}, {account.AccountStatusId}, '{account.FirstName}', '{account.LastName}', '{account.Company}', 
        '{account.Address}', '{account.City}', '{account.State}', '{account.Zip}', '{account.County}', '{account.bAddress}', '{account.bCity}', '{account.bState}', '{account.bZip}', '{account.Email}', {account.HoVerification},
        '{account.Phone1}', '{account.Phone2}', '{account.EmerName1}', '{account.EmerPhone1}', '{account.EmerName2}', '{account.EmerPhone2}', 
        '{account.DOB.ToString("yyyy-MM-dd")}', 
        '{account.Area}', 
        '{account.Signalsconf}', '{account.OnlineConf}', {account.Preinstall}, {account.Postinstall}, '{account.AccountHolder}','{account.Monitoring}',
        '{Util.ConvertDateToUTC(account.InstalledDate).ToString("yyyy-MM-dd HH:mm:ss")}', 
        '{Util.ConvertDateToUTC(account.SaleDate).ToString("yyyy-MM-dd HH:mm:ss")}', 
        '{Util.ConvertDateToUTC(account.TechSchedDate).ToString("yyyy-MM-dd HH:mm:ss")}', 
        '{Util.ConvertDateToUTC(account.SubmittedDate).ToString("yyyy-MM-dd HH:mm:ss")}', 
        '{Util.ConvertDateToUTC(account.FundedDate).ToString("yyyy-MM-dd HH:mm:ss")}', 
        '{Util.ConvertDateToUTC(account.ChargedbackDate).ToString("yyyy-MM-dd HH:mm:ss")}',  
        '{Util.ConvertDateToUTC(account.DateCancelled).ToString("yyyy-MM-dd HH:mm:ss")}',  
        '{account.CancelReason}', {account.ContractTerm}, '{account.CreditGrade}', {account.CreditScore}, 
        '{Util.ConvertDateToUTC(account.ContractStartDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        {account.MMR},{account.BuyoutAmount},'{account.VerbalPasscode}',{account.FieldRepId},{account.TechId});
        SELECT LAST_INSERT_ID();";

        var auditInsert = string.Empty;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lastinsertId = account.AccountId = Convert.ToInt32(conn.ExecuteScalar(insertLead));
                auditInsert = BuildAccountAuditSql(account);
                var affectedRows = conn.Execute(auditInsert);

            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.CreateAccount", $"Query: {insertLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            success = false;
        }

        return lastinsertId;
    }

    private string BuildAccountAuditSql(Account account)
    {
        var insertAccount = $@"INSERT INTO accountaudit 
        (agentid, accountid, leadid, accountstatusId, firstname, lastname, company, 
        address, city, state, zip, county, baddress, bcity, bstate, bzip, email, hoverification, 
        phone1, phone2, emername1, emerphone1, emername2, emerphone2, dob, area, 
        signalsconf, onlineconf, preinstall, postinstall, accountholder, monitoring, 
        installeddate, saledate, techscheddate, submitteddate, fundeddate, chargedbackdate, datecancelled, 
        cancelreason, contractterm, creditgrade, creditscore, contractstartdate, mmr, 
        buyoutamount,verbalpasscode,fieldrepid,techid) 
        SELECT {account.AgentId}, {account.AccountId}, leadid, accountstatusId, firstname, lastname, company, 
        address, city, state, zip, county, baddress, bcity, bstate, bzip, email, hoverification, 
        phone1, phone2, emername1, emerphone1, emername2, emerphone2, dob, area, 
        signalsconf, onlineconf, preinstall, postinstall, accountholder, monitoring,
        installeddate, saledate, techscheddate, submitteddate, fundeddate, chargedbackdate, datecancelled, 
        cancelreason, contractterm, creditgrade, creditscore, contractstartdate, mmr, 
        buyoutamount,verbalpasscode,fieldrepid,techid
        FROM account WHERE accountid = {account.AccountId};";

        return insertAccount;
    }

    public int CreateAccountAudit(Account account)
    {
        var lastinsertId = -1;

        var auditInsert = string.Empty;
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                auditInsert = BuildAccountAuditSql(account);
                var affectedRows = conn.Execute(auditInsert);
                lastinsertId = affectedRows;
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.CreateAccountAudit", $"Query: {auditInsert}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }

        return lastinsertId;
    }

    public bool UpdateAccount(Account account)
    {

        if (!string.IsNullOrEmpty(account.Phone1))
        {
            account.Phone1 = account.Phone1.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.Phone2))
        {
            account.Phone2 = account.Phone2.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.EmerPhone1))
        {
            account.EmerPhone1 = account.EmerPhone1.Replace("-", "");
        }
        if (!string.IsNullOrEmpty(account.EmerPhone2))
        {
            account.EmerPhone2 = account.EmerPhone2.Replace("-", "");
        }

        bool success = false;
        var lastUpdateId = account.AccountId;
        var auditInsert = string.Empty;
        var updateLead = $@"UPDATE account 
        SET leadid={account.LeadId},accountstatusId={account.AccountStatusId},firstname='{account.FirstName}', lastname='{account.LastName}',
        company='{account.Company}',address='{account.Address}',city='{account.City}',state='{account.State}',zip='{account.Zip}', county='{account.County}',
        baddress='{account.bAddress}',bcity='{account.bCity}',bstate='{account.bState}',bzip='{account.bZip}',
        email='{account.Email}',hoverification={account.HoVerification},phone1='{account.Phone1}',phone2='{account.Phone2}',emername1='{account.EmerName1}',emername2='{account.EmerName2}',emerphone1='{account.EmerPhone1}',emerphone2='{account.EmerPhone2}',
        dob='{account.DOB.ToString("yyyy-MM-dd")}',area='{account.Area}',signalsconf='{account.Signalsconf}',onlineconf='{account.OnlineConf}',preinstall={account.Preinstall},postinstall={account.Postinstall},
        accountholder='{account.AccountHolder}',monitoring='{account.Monitoring}',
        installeddate='{Util.ConvertDateToUTC(account.InstalledDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        saledate='{Util.ConvertDateToUTC(account.SaleDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        techscheddate='{Util.ConvertDateToUTC(account.TechSchedDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        submitteddate='{Util.ConvertDateToUTC(account.SubmittedDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        fundeddate='{Util.ConvertDateToUTC(account.FundedDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        chargedbackdate='{Util.ConvertDateToUTC(account.ChargedbackDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        datecancelled='{Util.ConvertDateToUTC(account.DateCancelled).ToString("yyyy-MM-dd HH:mm:ss")}',
        cancelreason='{account.CancelReason}',contractterm='{account.ContractTerm}',creditgrade='{account.CreditGrade}',creditscore={account.CreditScore},
        contractstartdate='{Util.ConvertDateToUTC(account.ContractStartDate).ToString("yyyy-MM-dd HH:mm:ss")}',
        mmr={account.MMR},buyoutamount={account.BuyoutAmount},verbalpasscode='{account.VerbalPasscode}',
        fieldrepid={account.FieldRepId},techid={account.TechId}
        where accountid={account.AccountId}";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lastUpdateId = Convert.ToInt32(conn.ExecuteScalar(updateLead));
                auditInsert = BuildAccountAuditSql(account);
                var affectedRows = conn.Execute(auditInsert);
                success = true;
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertLead", $"Query: {updateLead }, Message: {err.Message}, StackTrace: {err.StackTrace}");
            success = false;
        }

        return success;
    }

    public int CreateUpdateAccount(Account account)
    {
        var auditInsert = string.Empty;
        int lastinsertId = -1;
        var insertLead = @"createaccount";
        try
        {

            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(insertLead, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@leadid", account.LeadId);
                    cmd.Parameters.AddWithValue("@AccountStatusId", account.AccountStatusId);
                    cmd.Parameters.AddWithValue("@FirstName", account.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", account.LastName);
                    cmd.Parameters.AddWithValue("@Company", account.Company);
                    cmd.Parameters.AddWithValue("@Address", account.Address);
                    cmd.Parameters.AddWithValue("@City", account.City);
                    cmd.Parameters.AddWithValue("@State", account.State);

                    cmd.Parameters.AddWithValue("@Zip", account.Zip);
                    cmd.Parameters.AddWithValue("@County", account.County);
                    cmd.Parameters.AddWithValue("@bAddress", account.Address);
                    cmd.Parameters.AddWithValue("@bCity", account.bCity);
                    cmd.Parameters.AddWithValue("@bState", account.bState);
                    cmd.Parameters.AddWithValue("@bZip", account.bZip);
                    cmd.Parameters.AddWithValue("@Email", account.Email);
                    cmd.Parameters.AddWithValue("@HoVerification", account.HoVerification);

                    cmd.Parameters.AddWithValue("@Phone1", account.Phone1);
                    cmd.Parameters.AddWithValue("@Phone2", account.Phone2);
                    cmd.Parameters.AddWithValue("@EmerName1", account.EmerName1);
                    cmd.Parameters.AddWithValue("@EmerName2", account.EmerName2);
                    cmd.Parameters.AddWithValue("@EmerPhone1", account.EmerPhone1);
                    cmd.Parameters.AddWithValue("@EmerPhone2", account.EmerPhone2);
                    cmd.Parameters.AddWithValue("@DOB", account.DOB);
                    cmd.Parameters.AddWithValue("@Area", account.Area);

                    cmd.Parameters.AddWithValue("@Signalsconf", account.Signalsconf);
                    cmd.Parameters.AddWithValue("@OnlineConf", account.OnlineConf);
                    cmd.Parameters.AddWithValue("@Preinstall", account.Preinstall);
                    cmd.Parameters.AddWithValue("@Postinstall", account.Postinstall);
                    cmd.Parameters.AddWithValue("@AccountHolder", account.AccountHolder);
                    cmd.Parameters.AddWithValue("@Monitoring", account.Monitoring);
                    cmd.Parameters.AddWithValue("@InstalledDate", account.InstalledDate);
                    cmd.Parameters.AddWithValue("@SaleDate", account.SaleDate);

                    cmd.Parameters.AddWithValue("@TechSchedDate", account.TechSchedDate);
                    cmd.Parameters.AddWithValue("@SubmittedDate", account.SubmittedDate);
                    cmd.Parameters.AddWithValue("@FundedDate", account.FundedDate);
                    cmd.Parameters.AddWithValue("@ChargedbackDate", account.ChargedbackDate);
                    cmd.Parameters.AddWithValue("@DateCancelled", account.DateCancelled);
                    cmd.Parameters.AddWithValue("@CancelReason", account.CancelReason);
                    cmd.Parameters.AddWithValue("@ContractTerm", account.ContractTerm);
                    cmd.Parameters.AddWithValue("@CreditGrade", account.CreditGrade);

                    cmd.Parameters.AddWithValue("@CreditScore", account.CreditScore);
                    cmd.Parameters.AddWithValue("@ContractStartDate", account.ContractStartDate);
                    cmd.Parameters.AddWithValue("@MMR", account.MMR);
                    cmd.Parameters.AddWithValue("@BuyoutAmount", account.BuyoutAmount);
                    cmd.Parameters.AddWithValue("@VerbalPasscode", account.VerbalPasscode);
                    cmd.Parameters.AddWithValue("@InsertDate", account.InsertDate);

                    //cmd.Parameters.AddWithValue("@AccountId", MySqlDbType.Int32);
                    //cmd.Parameters["@AccountId"].Direction = ParameterDirection.Output;
                    var _aid = cmd.ExecuteScalar();
                    lastinsertId = account.AccountId = Convert.ToInt32(_aid);
                    //var _accountId = cmd.Parameters["@AccountId"].Value;
                }



                //  lastinsertId = account.AccountId = conn.ExecuteScalar<int>(insertLead, account, commandType: CommandType.StoredProcedure);
                //conn.Close();                                
                auditInsert = BuildAccountAuditSql(account);
                var affectedRows = conn.Execute(auditInsert);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.CreateAccount", $"Query: {insertLead}, Message: {err.Message}, StackTrace: {err.StackTrace}");

        }
        return lastinsertId;
    }

    public int GetLeadIdByAccountId(long accountid)
    {
        var leadid = -1;
        var sqlQuery = $"select leadid from account where accountid = {accountid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leadid = conn.QuerySingleOrDefault<int>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadIdByAccountId",
                $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return leadid;
    }

    public int GetAccountIdByLeadId(long leadid)
    {
        var accountid = -1;
        var sqlQuery = $"select accountid from account where leadid = {leadid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accountid = conn.QuerySingleOrDefault<int>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAccountIdByLeadId",
                $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return accountid;
    }

    public List<Account> GetAllAccount()
    {
        var account = new List<Account>();
        var sqlQuery = $"select * from account";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccount", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }

    public List<Account> GetAllAccountByEmail(string email)
    {
        var account = new List<Account>();
        var sqlQuery = $"select * from account where email='{email}' and email!=''";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccount", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }

    public List<Account> GetAllAccountsName()
    {
        var account = new List<Account>();
        var sqlQuery = $"select accountid,firstname,lastname from account";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccount", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }

    public Account GetAccountById(int accountid)
    {
        var account = new Account();
        var sqlQuery = $@"select *,
            (select firstname from agents where agentid=fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=techid) as TechFirstName,
            (select lastname from agents where agentid=fieldrepid) as RepLastName,
            (select lastname from agents where agentid=techid) as TechLastName
            from account where accountid={accountid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.QuerySingleOrDefault<Account>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAccountById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }

    public List<Account> GetAccountListById(int accountid)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"select *,
            (select firstname from agents where agentid=fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=techid) as TechFirstName,
            (select lastname from agents where agentid=fieldrepid) as RepLastName,
            (select lastname from agents where agentid=techid) as TechLastName
            from account where accountid={accountid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).ToList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAccountById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public TechCalendar GetTechCalendar()
    {
        var techCalendar = new TechCalendar();
        var account = new List<Account>();

        //var sqlQuery = $@"select *,max(aa.insertdate)  as lasttouched aa from accountaudit where convert_tz(installeddate, '+00:00', '-07:00') >= previousweek(1, '-07:00') and accountstatusId not in (199, 198);";
        var sqlQuery = $@"select * from 
                        (
                            select a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode, max(aa.insertdate)  as lasttouched,
                            (select ls.leadstatustext from leadstatus ls where ls.leadstatusid=a.accountstatusId  order by a.insertdate desc limit 1) as leadstatustext   
                            from account a
                            join accountaudit aa
                            on a.accountid = aa.accountid
                            where convert_tz(aa.insertdate, '+00:00', '-07:00') >= previousweek(1, '-07:00')
                            and aa.accountstatusId not in (199, 198)
                            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
                            order by aa.insertdate desc
                        )tbl
                        order by insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.Query<Account>(sqlQuery).AsList();

                foreach (var accounts in account)
                {
                    var insertDate = Util.ConvertToTZ(accounts.InsertDate, "MST");
                    accounts.InsertDate = insertDate;
                    var lastTouched = Util.ConvertToTZ(accounts.LastTouched, "MST");
                    accounts.LastTouched = lastTouched;
                }

                DateTime currentDate = DateTime.Now;

                DateTime startDayOfWeek = currentDate.Date.AddDays(-1 * (int)(currentDate.Date.DayOfWeek));
                DateTime endDayOfWeek = currentDate.Date.AddDays(6 - (int)currentDate.Date.DayOfWeek);

                techCalendar.ThisWeek = account.FindAll(a => a.InsertDate.Date >= startDayOfWeek.Date.AddDays(1) && a.InsertDate.Date <= endDayOfWeek).ToList();

                DateTime LastWeekEndDay = startDayOfWeek.AddDays(-1);
                DateTime LastWeekStartDay = currentDate.Date.AddDays(-7 - (int)currentDate.Date.DayOfWeek);

                techCalendar.LastWeek = account.FindAll(a => a.InsertDate.Date >= LastWeekStartDay.Date.AddDays(1) && a.InsertDate.Date <= LastWeekEndDay).ToList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetTechCalendar", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return techCalendar;
    }

    public List<Account> SearchAccountByAddressAndZip(string address, string zip)
    {
        var accounts = new List<Account>();
        var sqlQuery = $"select * from account where address = '{address}' and zip = '{zip}'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByAddressAndZip", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByAddress(string address)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.address LIKE '%{address}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;
            ";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByAddress", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByCity(string city)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.city LIKE '%{city}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;
            ";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByCity", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByState(string state)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.state LIKE '%{state}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByState", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByArea(string area)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.area LIKE '%{area}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByAddress", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByFieldRepId(int fieldRepId)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.fieldrepid ={fieldRepId}
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByFieldRepId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByTechId(int techId)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where aa.techid ={techId}
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByTechId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByName(string name)
    {
        var accounts = new List<Account>();
        var sqlQuery = "";
        sqlQuery = $@"select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where CONCAT(aa.firstname, ' ', aa.lastname) LIKE '%{name}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByName", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByPhone(string phone)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
            select a.*, max(aa.insertdate)  as lasttouched,
            (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
            (select firstname from agents where agentid=a.techid) as TechFirstName,
            (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
            (select lastname from agents where agentid=a.techid) as TechLastName
            from account a
            join accountaudit aa
            on a.accountid = aa.accountid
            where CONCAT(aa.phone1, ' ', aa.phone2) LIKE '%{phone}%'
            group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
            order by aa.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByPhone", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByEmail(string email)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
select a.*, max(aa.insertdate)  as lasttouched from account a
join accountaudit aa
on a.accountid = aa.accountid
where aa.email LIKE '%{email}%'
group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
order by aa.insertdate desc;
";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByEmail", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByAccountStatusId(int accountStatusId)
    {
        var accounts = new List<Account>();
        var sqlQuery = $@"
                        select a.*, max(aa.insertdate)  as lasttouched,
                        (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
                        (select firstname from agents where agentid=a.techid) as TechFirstName,
                        (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
                        (select lastname from agents where agentid=a.techid) as TechLastName
                        from account a
                        join accountaudit aa
                        on a.accountid = aa.accountid
                        where a.accountstatusid = {accountStatusId}
                        group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
                        order by aa.insertdate desc;
                        ";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByAccountStatusId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> SearchAccountByDates(DateTime fromDate, DateTime toDate, string searchBy)
    {
        var accounts = new List<Account>();
        if (searchBy == "create")
        {
            searchBy = "insertdate";
        }
        else
        {
            searchBy = "installeddate";
        }
        var sqlQuery = $@"
                        select a.*, max(aa.insertdate)  as lasttouched,
                        (select firstname from agents where agentid=a.fieldrepid) as RepFirstName,
                        (select firstname from agents where agentid=a.techid) as TechFirstName,
                        (select lastname from agents where agentid=a.fieldrepid) as RepLastName,
                        (select lastname from agents where agentid=a.techid) as TechLastName
                        from account a
                        join accountaudit aa
                        on a.accountid = aa.accountid
                        where
                        convert_tz(aa.{searchBy}, '+00:00', '-07:00')
                        BETWEEN '{fromDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{toDate.ToString("yyyy-MM-dd")} 23:59:59'
                        group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
                        order by aa.insertdate desc;
                        ";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.SearchAccountByAccountStatusId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    public List<Account> GetAllAccountFromDate(DateTime fromDate)
    {
        var accounts = new List<Account>();
        //        var sqlQuery = $@"
        //select a.*, max(aa.insertdate)  as lasttouched from account a
        //join accountaudit aa
        //on a.accountid = aa.accountid
        //where aa.insertdate > '{fromDate.ToString("yyyy-MM-dd")}'
        //and aa.accountstatusId not in (199, 198)
        //group by a.accountid, a.leadid, a.accountstatusId, a.firstname, a.lastname, a.company, a.address, a.city, a.state, a.zip, a.county, a.baddress, a.bcity, a.bstate, a.bzip, a.email, a.hoverification, a.phone1, a.phone2, a.emername1, a.emerphone1, a.emername2, a.emerphone2, a.dob, a.area, a.signalsconf, a.onlineconf, a.preinstall, a.accountholder, a.installeddate, a.saledate, a.submitteddate, a.fundeddate, a.chargedbackdate, a.datecancelled, a.cancelreason, a.contractterm,a.creditgrade, a.creditscore, a.contractstartdate, a.mmr, a.buyoutamount, a.insertdate, a.verbalpasscode
        //order by aa.insertdate desc;";
        var sqlQuery = $@"select *,
                        (select firstname from agents where agentid=fieldrepid) as RepFirstName,
                        (select firstname from agents where agentid=techid) as TechFirstName,
                        (select lastname from agents where agentid=fieldrepid) as RepLastName,
                        (select lastname from agents where agentid=techid) as TechLastName
                        from account 
                        where accountstatusid not in (198, 199) 
                        and insertdate > '{fromDate.ToString("yyyy-MM-dd")}' order by insertdate desc";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.Query<Account>(sqlQuery).AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;

                    var InstalledDate = Util.ConvertToTZ(account.InstalledDate, "MST");
                    account.InstalledDate = InstalledDate;

                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccountFromDate", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accounts;
    }

    //Get account info by account status
    public async Task<AccountInfoList> GetAllAccountInfo()
    {
        var accounts = new List<AccountInfo>();
        var accountInfo = new AccountInfoList();
        var sqlQuery = $@"select a.accountid,a.firstname,a.lastname,a.address,a.city,a.state,a.insertdate,a.phone1,a.techid,a.fieldrepid,a.email
						 ,(select accountstatusId from accountaudit where accountid=a.accountid and accountstatusId in (56,120,110,100) order by accountauditid desc limit 1) accountstatusId
                         ,(select firstname from agents agt where agt.agentid=(select techid from account where accountid=a.accountid order by insertdate desc limit 1)) tech
                         ,(select firstname from agents agt where agt.agentid=(select fieldrepid from account where accountid=a.accountid  order by insertdate desc limit 1)) rep
                         ,(select leadstatustext from  leadstatus where leadstatusid =a.accountstatusId) leadstatustext
                         ,(select saledate from accountaudit where  accountid = a.accountid order by accountauditid desc limit 1) solddate
                         ,(select notetext from notes where accountid = a.accountid and notetext!='' order by noteid desc limit 1) notetext
                          from accountaudit a
                          where a.accountstatusId in (56,120,110,100)
                          group by accountid
                          order by a.insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accounts = conn.QueryAsync<AccountInfo>(sqlQuery).Result.AsList();

                foreach (var account in accounts)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var soldDate = Util.ConvertToTZ(account.SoldDate, "MST");
                    account.SoldDate = soldDate;
                }
                DateTime currentDate = DateTime.Now;

                DateTime startDayOfWeek = currentDate.Date.AddDays(-1 * ((int)(currentDate.Date.DayOfWeek) - 1));
                DateTime endDayOfWeek = currentDate.Date.AddDays(6 - ((int)currentDate.Date.DayOfWeek - 1));

                accountInfo.ThisWeek = accounts.FindAll(a => a.InsertDate.Date >= startDayOfWeek.Date.AddDays(1) && a.InsertDate.Date <= endDayOfWeek).ToList();

                DateTime LastWeekEndDay = startDayOfWeek.AddDays(-1);
                DateTime LastWeekStartDay = currentDate.Date.AddDays(-6 - (int)currentDate.Date.DayOfWeek);

                accountInfo.LastWeek = accounts.FindAll(a => a.InsertDate.Date >= LastWeekStartDay.Date.AddDays(1) && a.InsertDate.Date <= LastWeekEndDay).ToList();

                accountInfo.Oldest = accounts.FindAll(a => a.InsertDate.Date <= LastWeekStartDay.Date.AddDays(-1)).ToList();

            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccount", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accounts = null;
        }
        return accountInfo;
    }

    public async Task<AccountDashboard> GetAccountDashboard(DateTime fromDate, DateTime toDate)
    {
        var accountDashboard = new AccountDashboard();
        var sqlQuery = $@"Select Sold, PartInstall, Installed
                        from ( 	select
		                        sum(case accountstatusid when 56 then rcount else null end) as Sold
		                        , sum(case accountstatusid when 57 then rcount else null end) as Installed
		                        , sum(case accountstatusid when 120 then rcount else null end) as PartInstall from
		                        (select accountstatusid, count(distinct accountid) rcount
		                        from accountaudit
		                        where accountstatusid in (56, 120, 57) and 
                                convert_tz(insertdate, '+00:00', '-07:00')
                                BETWEEN '{fromDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{toDate.ToString("yyyy-MM-dd")} 23:59:59'
                        group by CONCAT(accountid, '-',accountstatusid )) t ) t1";

        //var sqlQueryAccount = $@"select * from 
        //                        (select accountauditid,accountid,accountstatusid, firstname,lastname,address,city,state,email,phone1, count(distinct accountid) rcount,insertdate,max(insertdate)  as lasttouched
        //                      from accountaudit
        //                      where accountstatusid in (56, 120, 57) and
        //                            convert_tz(insertdate, '+00:00', '-07:00')
        //                            BETWEEN '{fromDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{toDate.ToString("yyyy-MM-dd")} 23:59:59'
        //                            group by CONCAT(accountid)
        //                        )tbl 
        //                        order by insertdate desc";
        var sqlQueryAccount = $@"select * from (
			SELECT 				            
				aa.accountauditid,aa.accountid,a.accountstatusid,aa. firstname,aa.lastname,aa.address,aa.city,aa.state,aa.email,aa.phone1, count(distinct aa.accountid) rcount,aa.insertdate,max(aa.insertdate)  as lasttouched		
			FROM
				accountaudit aa join account a on a.accountid=aa.accountid                 
			where aa.accountstatusid  in (56, 120, 57) and
			convert_tz(aa.insertdate, '+00:00', '-07:00')
			 BETWEEN '{fromDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{toDate.ToString("yyyy-MM-dd")} 23:59:59'
            group by CONCAT(aa.accountid)		
			 )tbl 
             order by insertdate desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                accountDashboard = conn.QueryAsync<AccountDashboard>(sqlQuery).Result.SingleOrDefault();
                accountDashboard.Account = conn.QueryAsync<Account>(sqlQueryAccount).Result.AsList();

                foreach (var account in accountDashboard.Account)
                {
                    var insertDate = Util.ConvertToTZ(account.InsertDate, "MST");
                    account.InsertDate = insertDate;
                    var LastTouched = Util.ConvertToTZ(account.LastTouched, "MST");
                    account.LastTouched = LastTouched;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAccountDashboard", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            accountDashboard = null;
        }
        return accountDashboard;
    }

    public async Task<List<Account>> GetAllState()
    {
        var account = new List<Account>();
        var sqlQuery = $"select distinct state from account where state!=''";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                account = conn.Query<Account>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAccount", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            account = null;
        }
        return account;
    }
    #endregion

    #region Docusign File
    public int InserDocuSignFile(DocusignDocuments docusignDocuments)
    {
        int lastInsertRowId = 0;
        //var runSuccessful = true;        
        var insertDocuSignDocument =
            $@"INSERT INTO docusigndocuments (accountid, filename, agentid) 
                VALUES ('{docusignDocuments.AccountId}', '{docusignDocuments.FileName}', '{docusignDocuments.AgentId}');
                  SELECT LAST_INSERT_ID();";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {

                lastInsertRowId = docusignDocuments.Id = Convert.ToInt32(conn.ExecuteScalar(insertDocuSignDocument));
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.UploadDocuSignFile", $"Query: {insertDocuSignDocument}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lastInsertRowId = 0;
        }

        return lastInsertRowId;
    }

    public DocusignDocuments GetDosumentsById(int documentid)
    {
        var docusignDocuments = new DocusignDocuments();
        var sqlQuery = $"select * from docusigndocuments where id={documentid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                docusignDocuments = conn.QuerySingleOrDefault<DocusignDocuments>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetDosumentsById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            docusignDocuments = null;
        }
        return docusignDocuments;
    }

    public List<DocusignDocuments> GetAllDocuSignDosuments()
    {
        var docusignDocuments = new List<DocusignDocuments>();
        var sqlQuery = $"select *, (select firstname from account where accountid=d.accountid) as firstname,(select lastname from account where accountid=d.accountid) as lastname, (select noteid from notes where accountid=d.accountid and accountid!=0 limit 1) as isassigned from docusigndocuments d";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                docusignDocuments = conn.Query<DocusignDocuments>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetDosumentsById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            docusignDocuments = null;
        }
        return docusignDocuments;
    }

    public bool UpdateDocuSignFile(DocusignDocuments docusignDocuments)
    {
        var runSuccessful = true;
        var insertDocuSignDocument =
            $@"Update docusigndocuments set accountid= {docusignDocuments.AccountId}, filename='{docusignDocuments.FileName}', agentid={docusignDocuments.AgentId}
            WHERE id={docusignDocuments.Id}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertDocuSignDocument);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.UploadDocuSignFile", $"Query: {insertDocuSignDocument}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool DeleteDocuSignFileByID(int DocusignID)
    {
        var runSuccessful = true;
        var deleteDocuSignDocument =
            $@"delete from docusigndocuments 
            WHERE id={DocusignID}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(deleteDocuSignDocument);
                runSuccessful = true;
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.DeleteDocuSignFileByID", $"Query: {deleteDocuSignDocument }, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }
        return runSuccessful;
    }
    #endregion

    #region Notes
    public List<Note> GetNotes(Note noteRequest)
    {
        var notes = new List<Note>();
        var sqlQuery = $@"select n.*, CONCAT(a.firstname, ' ', a.lastname) AgentName, leadstatusid
from notes n JOIN agents a ON a.agentid = n.agentid 
where leadId = {noteRequest.LeadId} ORDER BY insertdate DESC";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                notes = conn.Query<Note>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetNotes", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            notes = null;
        }
        return notes;
    }

    public bool InsertNote(Note noteToSave)
    {
        var runSuccessful = true;
        noteToSave.NoteText = noteToSave.NoteText.Replace("'", "");
        var insertNote =
            $@"INSERT INTO notes (leadid, accountid, notetext, filename, agentid, insertdate, leadstatusid) 
                VALUES ('{noteToSave.LeadId}', '{noteToSave.AccountId}', '{noteToSave.NoteText}', '{noteToSave.FileName}', '{noteToSave.AgentId}','{Util.ConvertToTZ(DateTime.Now, "MST").ToString("yyyy-MM-dd HH:mm:ss")}', {noteToSave.LeadStatusId})";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertNote);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.InsertNote", $"Query: {insertNote}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public bool AddAccountIdToNotes(int leadId, int accountId)
    {
        var runSuccessful = true;
        var updateNote =
            $@"update notes set accountid={accountId} where leadid={leadId}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(updateNote);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.AddAccountIdToNotes", $"Query: {updateNote}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public List<Note> GetNoteByAccountId(int accountId)
    {
        var notes = new List<Note>();
        var sqlQuery = $@"select *,(select firstname from agents where agentid=notes.agentid) as agentName from notes where accountid = {accountId} and accountid!=0 ORDER BY insertdate DESC";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                notes = conn.Query<Note>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetNoteByAccountId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            notes = null;
        }
        return notes;
    }

    public Note GetNotesByOldAccountId(int AccountId)
    {
        var note = new Note();
        var sqlQuery = $"select accountid from notes where notetext like '%CustomerAccountId:</b> {AccountId}%'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                note = conn.QuerySingleOrDefault<Note>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetNotesByOldAccountId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            note = null;
        }
        return note;
    }
    #endregion

    #region Agents
    public bool LoginAgent(Agent agnt)
    {
        bool IsLoggedIn = false;
        var agent = new Agent();
        var sqlQuery = $"select * from agents where agentid = {agnt.AgentId} And password = '{agnt.AgentPassword}' And email ='{agnt.Email}'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agent = conn.QuerySingleOrDefault<Agent>(sqlQuery);
                if (agent != null)
                {
                    IsLoggedIn = true;
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAgentById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            IsLoggedIn = false;
        }
        return IsLoggedIn;
    }

    public List<Agent> GetAllAgents()
    {
        var agents = new List<Agent>();
        var sqlQuery = $@"select * from timeclock t
join agents a
on t.agentid = a.agentid
where t.ClockedinAt > previousweek(1,'MST')
group by a.agentid";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agents = conn.Query<Agent>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgents", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agents = null;
        }
        return agents;
    }

    public List<Agent> GetAllActiveAgents()
    {
        var agents = new List<Agent>();
        //only want to get Agents that are allowed to sign into CRM ...
        var sqlQuery = $@"
select * from agents where status = 1
and agenttypeid in (10, 11, 30, 31, 40, 60, 70)
order by firstname asc";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agents = conn.Query<Agent>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgents", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agents = null;
        }
        return agents;
    }

    public Agent GetAgentById(int AgentId)
    {
        var agent = new Agent();
        var sqlQuery = $"select *,password as agentpassword from agents where agentid = {AgentId}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agent = conn.QuerySingleOrDefault<Agent>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAgentById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agent = null;
        }
        return agent;
    }

    public List<Agent> GetAgentByType(string agentType)
    {
        var agents = new List<Agent>();
        string sqlQuery = "";
        if (agentType == "IS")
        {
            sqlQuery = $"select * from agents where agenttypeid in (40,60) and status = 1 order by firstname;";
        }
        if (agentType == "Manager")
        {
            sqlQuery = $"select * from agents where agenttypeid in (60,70) and status = 1 order by firstname";
        }
        if (agentType == "20")
        {
            sqlQuery = $"select * from agents where agenttypeid = {int.Parse(agentType)} and status = 1 order by firstname";
        }
        if (agentType == "50")
        {
            sqlQuery = $"select * from agents where agenttypeid = {int.Parse(agentType)} and status = 1 order by firstname";
        }
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agents = conn.Query<Agent>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgentsByAgentTypeId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agents = null;
        }
        return agents;
    }

    public List<Agent> GetAllAgentsByAgentTypeId(int agentTypeId)
    {
        var agents = new List<Agent>();
        var sqlQuery = $"select * from agents where agenttypeid = {agentTypeId} and status = 1";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agents = conn.Query<Agent>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgentsByAgentTypeId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agents = null;
        }
        return agents;
    }

    public List<TimeclockAction> GetClockedInOutHistory(TimeclockAction timeclockAction)
    {
        var agentString = (timeclockAction.AgentId == 0)
            ? string.Empty
            : $"AND c.AgentId = {timeclockAction.AgentId}";

        string sqlQuery =
            $@"SELECT a.agentid, c.timeclockid, a.Firstname,c.ClockedinAt, c.ClockedOutAt, c.HrsWorked
                      From agents a join timeclock c on c.agentid = a.agentid where
                      c.ClockedinAt between '{timeclockAction.StartDate.ToString("yyyy-MM-dd 00:00:00")}' and '{timeclockAction.EndDate.ToString("yyyy-MM-dd 23:59:59")}'
                      {agentString} group by c.ClockedinAt order by c.ClockedinAt desc;";
        var history = new List<TimeclockAction>();
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                history = conn.Query<TimeclockAction>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetTimeclockHistory", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            history = null;
        }
        return history;
    }

    public List<TimeclockAction> GetTimeclockHistory(int agentId)
    {
        var history = new List<TimeclockAction>();
        var sqlQuery = $"select DATE_FORMAT(ClockedInAt, '%m-%d-%Y %r')ClockedInAt, DATE_FORMAT(ClockedOutAt, '%m-%d-%Y %r')ClockedOutAt,timeclockid,AgentId,HrsWorked  from timeclock where agentid = {agentId} ORDER BY actiontime DESC LIMIT 5";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                history = conn.Query<TimeclockAction>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetTimeclockHistory", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            history = null;
        }
        return history;
    }

    public TimeclockAction IsUserClockedIn(TimeclockAction timeclockAction)
    {
        var timeClock = new TimeclockAction();
        var sqlQuery = $@"SELECT timeclockid, ClockedInAt, ClockedOutAt, AgentId FROM timeclock WHERE date_format(convert_tz(ClockedInAt,'+00:00','-07:00'), '%y-%m-%d') " +
            $"> date_format(convert_tz('{timeclockAction.ClockedInAt.ToString("yyyy-MM-dd")}','+00:00','-07:00'), '%y-%m-%d')" +
            $"AND agentId = {timeclockAction.AgentId} And ClockedOutAt IS NULL";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                timeClock = conn.QuerySingleOrDefault<TimeclockAction>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAgentById", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            timeClock = null;
        }
        return timeClock;
    }

    public TimeclockAction UpdateTimeclockAction(TimeclockAction action)
    {
        if (action.ClockedOutAt == DateTime.MinValue)
        {
            action.ClockedOutAt = DateTime.Now;
        }
        var result = action;
        var insertAction = $@"UPDATE timeclock SET ClockedOutAt = '{Util.ConvertToTZ(action.ClockedOutAt, "MST").ToString("yyyy-MM-dd HH:mm")}' WHERE timeclockid = {action.TimeclockId}";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertAction);
                result.Status = "Clocked Out";
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.UpdateTimeclockAction error", $"Query: {insertAction}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            result.Status = "Error";
        }

        return result;
    }

    public TimeclockAction InsertTimeclockAction(TimeclockAction action)
    {

        TimeclockAction result = action;

        var insertAction = $@"INSERT INTO timeclock (agentid, clockedInat)
                VALUES ({action.AgentId}, '{Util.ConvertToTZ(action.ClockedInAt, "MST").ToString("yyyy-MM-dd HH:mm")}')";
        Logger.Log("MySqlDataAgent.InsertTimeclockAction", $"Query: {insertAction}");
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertAction);
                result.Status = "Clocked In";
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertTimeclockAction error", $"Query: {insertAction}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            result.Status = "Error";
        }

        return result;
    }

    public List<TimeclockAction> GetAllMissingClockOut()
    {
        var timeClockList = new List<TimeclockAction>();
        var sqlQuery = $@"select * from timeclock where ClockedOutAt is null";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                timeClockList = conn.Query<TimeclockAction>(sqlQuery).ToList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllMissingClockOut", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return timeClockList;
    }

    //i.e. grid #1
    public List<LeadGenStats> GetLeadGenStats(DateTime fromDate, DateTime toDate)
    {
        var stats = new List<LeadGenStats>();
        var sqlQuery = $@"
select TotalProcessed, TotalNewLeads, TotalNewLeads / TotalProcessed as RawVsNewRatio, TotalRawUploaded, a.agentid, a.firstname as AgentName,
count(distinct leadid) LookUps, sum(personfound) PersonFound, round(sum(personfound)/count(distinct leadid),2) PersonFoundRatio 
FROM
    (select
 sum(case when leadstatusid < 10 then 1 else 0 end) TotalProcessed,
 sum(case when leadstatusid = 1 then 1 else 0 end) TotalNewLeads,
 sum(case when leadstatusid = 0 then 1 else 0 end) TotalRawUploaded,
 agentid
from leadaudit where leadstatusid < 10
AND CONVERT_TZ(insertdate, '+00:00', '-07:00') Between '{fromDate.ToString("yyyy-MM-dd 00:00:00")}' and '{toDate.ToString("yyyy-MM-dd 23:59:59")}' 
group by agentid) sub
left join agents a join lookupaudit la on la.agentid = a.agentid
on sub.agentid = a.agentid where agenttypeid in (10,30) and convert_tz(la.insertdate, '+00:00', '-07:00') between '{fromDate.ToString("yyyy-MM-dd 00:00:00")}' and '{toDate.ToString("yyyy-MM-dd 23:59:59")}' group by a.agentid;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<LeadGenStats>(sqlQuery).AsList();
                sqlQuery = $@"select count(1) from leads where CONVERT_TZ(insertdate, '+00:00', '-07:00')
Between '{fromDate.ToString("yyyy-MM-dd 00:00:00")}' and '{toDate.ToString("yyyy-MM-dd 23:59:59")}' ";
                var count = conn.ExecuteScalar(sqlQuery);
                stats.Add(new LeadGenStats()
                {
                    AgentId = 0,
                    AgentName = "Upload",
                    TotalRawUploaded = Convert.ToInt32(count),
                    TotalProcessed = 0,
                    TotalNewLeads = 0,
                    LookUps = 0,
                    PersonFound = 0,
                    RawVsNewRatio = 0,
                    PersonFoundRatio = 0
                });
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadGenStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }
        return stats;
    }

    //i.e. grid #2
    public List<DialAgentStats> GetDialAgentStats()
    {
        var stats = new List<DialAgentStats>();
        var sqlQuery = @"call dialagentstats";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                stats = conn.Query<DialAgentStats>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetDialAgentStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            stats = null;
        }
        return stats;
    }

    //i.e. grid #3
    public List<DialRatio> GetDialRatioStats(DateTime FromDate, DateTime ToDate)
    {
        var report = new List<DialRatio>();
        var sqlQuery = $"call dialratio(fromDate('{FromDate.ToString("yyyy-MM-dd")}'), toDate('{ToDate.ToString("yyyy-MM-dd")}'))";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                report = conn.Query<DialRatio>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetDialRatioStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            report = null;
        }
        return report;
    }

    //i.e. grid #4
    public List<LeadRatio> GetLeadRatioStats(DateTime FromDate, DateTime ToDate)
    {
        var report = new List<LeadRatio>();
        var sqlQuery = $"call leadratio(fromDate('{FromDate.ToString("yyyy-MM-dd")}'), toDate('{ToDate.ToString("yyyy-MM-dd")}'))";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                report = conn.Query<LeadRatio>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetLeadRatioStats", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            report = null;
        }
        return report;
    }

    public List<SAToInstall> GetSAToInstalls(DateTime FromDate, DateTime ToDate)
    {
        var report = new List<SAToInstall>();
        var sqlQuery = $@"select a.agentid AgentId,
concat(a.firstname,' ', a.lastname) Name,
SchedAppt, Confirmed, FRDispatched, UFS, NoShow, Sold, Installed
from (
select agentid
, sum(case leadstatusid when 53 then rcount else null end) as SchedAppt
, sum(case leadstatusid when 54 then rcount else null end) as `Confirmed`
, sum(case  when leadstatusid= 55 and agenttypeid = 20 then rcount else null end) as FRDispatched
, sum(case leadstatusid when 58 then rcount else null end) as UFS
, sum(case leadstatusid when 59 then rcount else null end) as NoShow
, sum(case leadstatusid when 56 then rcount else null end) as Sold
, sum(case leadstatusid when 57 then rcount else null end) as Installed from
(select l.agentid, a.agenttypeid, leadstatusid, count(distinct leadid) rcount
from leadaudit l join agents a on a.agentid = l.agentid
where leadstatusid >= 53 and convert_tz(l.insertdate, '+00:00', '-07:00') between '{FromDate.ToString("yyyy-MM-dd 00:00:00")}' and '{ToDate.ToString("yyyy-MM-dd 23:59:59")}'
group by l.agentid, a.agenttypeid, CONCAT(l.leadid, '-',l.leadstatusid )) t group by  agentid) t1 join agents a on a.agentid = t1.agentid order by SchedAppt desc, Installed desc;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                report = conn.Query<SAToInstall>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetSAToInstalls", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            report = null;
        }
        return report;
    }

    public List<DialReport> GetDaysDailReport(bool today, int agentId)
    {
        var dialreport = new List<DialReport>();

        var sqlQuery = (today)
            ? $@"select la.leadid, ls.leadstatustext as Marked, convert_tz(la.insertdate, '+00:00','-07:00') as DialedTime from leadaudit la
            join leadstatus ls
            on la.leadstatusid = ls.leadstatusid
            where la.agentid = {agentId}
            and convert_tz(la.insertdate, '+00:00','-07:00') between today(1, '-07:00') and today(2, '-07:00') order by DialedTime desc;"
            : $@"select la.leadid, ls.leadstatustext as Marked, convert_tz(la.insertdate, '+00:00','-07:00') as DialedTime from leadaudit la
            join leadstatus ls
            on la.leadstatusid = ls.leadstatusid
            where la.agentid = {agentId}
            and convert_tz(la.insertdate, '+00:00','-07:00') between yesterday(1, '-07:00') and yesterday(2, '-07:00') order by DialedTime desc;";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                dialreport = conn.Query<DialReport>(sqlQuery).ToList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetDaysDailReport", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return dialreport;
    }

    public List<Lead> GetPayReportList(TimeclockAction timeclockAction)
    {
        var leads = new List<Lead>();
        string sqlQuery = $@"select  la.leadauditid,la.agentid,la.leadstatusid,la.insertdate,la.leadtypeid,la.leadid, ls.leadstatustext statustext,
                    lt.longtext leadtypeText,
                    l.firstname FirstName,
                    l.lastname LastName
                    from leadaudit la join leads l on l.leadid = la.leadid
                    join leadstatus ls on ls.leadstatusid = la.leadstatusid
                    left join leadtype lt on lt.leadtypeid = la.leadtypeid
                    where la.leadstatusid in (54, 57, 59)
                    AND la.agentid={timeclockAction.AgentId}
                    AND CONVERT_TZ(la.insertdate,'+00:00','-07:00')
                    BETWEEN '{timeclockAction.StartDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{timeclockAction.EndDate.ToString("yyyy-MM-dd")} 23:59:59'
                    GROUP BY CONCAT(la.leadid, '-',la.leadstatusid )
                    order by leadstatusid;";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                leads = conn.Query<Lead>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetPayReportList", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            leads = null;
        }
        return leads;
    }

    public List<TimeclockAction> GetCheckinCheckOutReport(TimeclockAction timeclockAction)
    {
        List<TimeclockAction> lstTimeclockAction = new List<TimeclockAction>();

        var sqlQuery = $@"select Distinct Date(t.ClockedinAt) as ClockedinAt,  
        (select firstname from agents lt where agentid={timeclockAction.AgentId})FirstName,
        (select lastname from agents lt where agentid={timeclockAction.AgentId})LastName,
        (select SEC_TO_TIME(SUM(TIME_TO_SEC(HrsWorked))) from timeclock where agentid={timeclockAction.AgentId} and DATE(ClockedinAt) =DATE( ClockedOutAt) and date(ClockedinAt)=date(t.ClockedinAt)) as HrsWorked
        from timeclock t where agentid={timeclockAction.AgentId}
        AND  t.ClockedinAt BETWEEN '{timeclockAction.StartDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{timeclockAction.EndDate.ToString("yyyy-MM-dd")} 23:59:59'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lstTimeclockAction = conn.Query<TimeclockAction>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetCheckinCheckOutReport", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            lstTimeclockAction = null;
        }
        return lstTimeclockAction;
    }

    public int InsertAgent(Agent agent)
    {
        int lastInsertedRowId = -1;
        int Status = 0;
        if (agent.Status == true)
        {
            Status = 1;
        }
        else
        {
            Status = 0;
        }
        var insertAgent =
            $@"INSERT INTO agents (firstname, lastname, phone, email,password,agenttypeid,status) 
                VALUES ('{agent.FirstName}', '{agent.LastName}', '{agent.Phone}', '{agent.Email}', '{agent.AgentPassword}','{agent.AgentTypeId}','{Status}');
                SELECT LAST_INSERT_ID();";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                lastInsertedRowId = conn.Execute(insertAgent);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.InsertAgent", $"Query: {insertAgent}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            return lastInsertedRowId;
        }

        return lastInsertedRowId;
    }

    public bool UpdateAgent(Agent agent)
    {
        bool IsSuccess = false;
        int Status = 0;
        if (agent.Status == true)
        {
            Status = 1;
        }
        else
        {
            Status = 0;
        }
        var updateAgent =
            $@"UPDATE agents SET firstname='{agent.FirstName}',lastname='{agent.LastName}',phone='{agent.Phone}',email='{agent.Email}',password='{agent.AgentPassword}',agenttypeid ={agent.AgentTypeId}, status={Status} 
            WHERE agentid={agent.AgentId}";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                conn.Execute(updateAgent);
                IsSuccess = true;
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.UpdateAgent", $"Query: {updateAgent }, Message: {err.Message}, StackTrace: {err.StackTrace}");

            return false;
        }

        return IsSuccess;
    }

    public List<Agent> GetAllAgentList()
    {
        var agents = new List<Agent>();
        var sqlQuery = $@"select a.*,
(select agenttypename from agenttype at where at.agentid=a.agenttypeid) agenttypename 
from agents a
order by a.status desc, firstname asc";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                agents = conn.Query<Agent>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgents", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            agents = null;
        }
        return agents;
    }
    #endregion

    #region Agent Type
    public List<AgentType> GetAllAgentType()
    {
        var AgentType = new List<AgentType>();
        var sqlQuery = $@"select * from agenttype ";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                AgentType = conn.Query<AgentType>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgentType", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            AgentType = null;
        }
        return AgentType;
    }
    #endregion

    #region SMS
    public bool InsertSMS(SMSMessage message)
    {
        var runSuccessful = true;

        var fromPhone = message.FromPhone.Trim().Replace("+1", "");
        fromPhone = (fromPhone.StartsWith("1")) ? fromPhone.Substring(1) : fromPhone;
        var toPhone = message.ToPhone.Trim().Replace("+1", "");
        toPhone = (toPhone.StartsWith("1")) ? toPhone.Substring(1) : toPhone;
        var cleansedBody = message.Body.Replace("'", "");

        var insertSMS = $@"INSERT INTO sms (leadid, accountid, agentsentby, fromphone, tophone, message, messagestatus, isread,fileurl)
                VALUES ('{message.LeadId}', '{message.AccountId}', {message.AgentSentBy}, '{fromPhone}', '{toPhone}', '{cleansedBody}', '{message.MessageStatus}',{false},'{message.FileUrl}')";
        Logger.Log("MySqlDataAgent.InsertSMS", $"Query: {insertSMS}");
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertSMS);
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InsertSMS error", $"Query: {insertSMS}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            runSuccessful = false;
        }

        return runSuccessful;
    }

    public List<SMSMessage> GetSMSLHS(int agentTypeId, string agentPhone)
    {
        List<SMSMessage> sMSLHS = new List<SMSMessage>();
        agentPhone = agentPhone.Replace("+1", "");
        var sqlQuery = "";
        if (agentTypeId == 10)
        {
            //sqlQuery = $@"select s.leadid as agentid ,s.id id, s.agentsentby AgentSentBy, l.firstname FirstName, l.lastname LastName, s.tophone ToPhone, s.fromphone FromPhone,
            //            (select isread from sms where  (fromphone = (select phone1 from leads where leadid = s.leadid) or fromphone =(select phone2 from leads where leadid = s.leadid) or
            //            fromphone = (select phone3 from leads where leadid = s.leadid) or fromphone =(select phone4 from leads where leadid =s.leadid) or
            //            fromphone =(select phone5 from leads where leadid =s.leadid)) AND isread=0 Limit 1) isread
            //            from sms s 
            //            join leads l on l.leadid = s.leadid
            //            group by s.leadid";

            //sqlQuery = $@"
            //  select * from (
            //        select s.leadid as agentid ,s.id id, s.agentsentby AgentSentBy, l.firstname FirstName, l.lastname LastName, s.tophone ToPhone, s.fromphone FromPhone,convert_tz(s.datesent, '+00:00','-07:00')as datesent,
            //              (select if(fromphone = 4804000271, tophone, fromphone)  from sms where id=s.id limit 1)ChatAgentPhone,
            //        (select isread from sms where  
            //        (fromphone = (select phone1 from leads where leadid = s.leadid and phone1status=3) or fromphone =(select phone2 from leads where leadid = s.leadid and phone2status=3) or
            //        fromphone = (select phone3 from leads where leadid = s.leadid and phone3status=3) or fromphone =(select phone4 from leads where leadid =s.leadid and phone4status=3) or
            //        fromphone =(select phone5 from leads where leadid =s.leadid and phone5status=3)) AND isread=0 Limit 1) isread
            //        from sms s 
            //        join leads l on (l.phone1=s.fromphone or l.phone2=s.fromphone or l.phone3=s.fromphone or l.phone4=s.fromphone or l.phone5=s.fromphone 
            //        or l.phone1=s.tophone or l.phone2=s.tophone or l.phone3=s.tophone or l.phone4=s.tophone or l.phone5=s.tophone)                       

            //        where  ((fromphone in (select phone1 from leads ) or tophone in (select phone1 from leads ))OR
            //          (fromphone in (select phone2 from leads ) or tophone in (select phone2 from leads ))OR
            //          (fromphone in (select phone3 from leads ) or tophone in (select phone3 from leads ))OR
            //          (fromphone in (select phone4 from leads ) or tophone in (select phone4 from leads ))OR
            //          (fromphone in (select phone5 from leads ) or tophone in (select phone5 from leads )) 
            //          )
            //        AND convert_tz(s.datesent, '+00:00', '-07:00') >= ( CURDATE() - INTERVAL 2 week )
            //        AND (fromphone=4804000271 or tophone=4804000271)	
            //                  AND s.leadid>0
            //                  AND (l.leadstatusid!=19 and lower( s.message) != lower('stop'))
            //            order by datesent desc 
            //        )tmp
            //              where fromphone  not in (select fromphone from sms where lower(message) = lower('stop'))  or  tophone  not in (select tophone from sms where lower(message) = lower('stop'))
            //group by agentid;";
            sqlQuery = "GetSMSLHSForAgent10";
        }
        else
        {
            sqlQuery = $@"select s.id id, a.agentid, s.agentsentby AgentSentBy, a.firstname FirstName, a.lastname LastName, s.tophone ToPhone, s.fromphone FromPhone, convert_tz(s.datesent, '+00:00','-07:00')as datesent,
                        (select isread from sms where fromphone=(select phone from agents where agentid=a.agentid) AND isread=0 Limit 1) isread,
                        (select phone from agents where (agentid=s.agentsentby or phone=s.fromphone or phone=s.tophone) and agenttypeid={agentTypeId}  limit 1 ) ChatAgentPhone                       
                        from (
                        SELECT a.agentid, max(id) id from sms s join agents a on s.tophone = a.phone or s.fromphone = a.phone where a.agenttypeid = {agentTypeId} AND datesent >  previousweek(1, '-07:00') group by a.agentid) v 
                        join sms s on s.id = v.id join agents a on a.agentid = v.agentid 
                         where fromphone={agentPhone} or tophone={agentPhone}
                        order by s.datesent desc;";
        }
        try
        {
            if (agentTypeId == 10)
            {
                using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
                {
                   var sMSLHSTemp = conn.Query<SMSMessage>(sqlQuery, null, null, true, null, CommandType.StoredProcedure).AsList();
                    var ids = string.Join(",", sMSLHSTemp.Select(x => x.agentId).ToList());
                    
                    var leadList = new List<LeadTempClass>();
                    var smsList = new List<SMSMessage>();
                    var leadDetailList = new List<LeadTempClass>();

                    var param = new DynamicParameters();
                    param.Add("p_ids", ids, DbType.String, ParameterDirection.Input);
                    using (var questionList = conn.QueryMultiple("GetSMSLHSForAgent10Sub", param, commandType: CommandType.StoredProcedure))
                    {
                        leadList = questionList.Read<LeadTempClass>().ToList();
                        smsList = questionList.Read<SMSMessage>().ToList();
                        leadDetailList = questionList.Read<LeadTempClass>().ToList();
                    }
                    sMSLHSTemp.ForEach(x =>
                    {

                        var leadObj = leadDetailList.Where(y => y.leadid.Equals(x.agentId)).FirstOrDefault();
                        if (leadObj != null && leadObj.Leadstatusid != 19)
                        {
                            x.FirstName = leadObj.Firstname;
                            x.LastName = leadObj.Lastname;
                            x.isread = GetIsReadValue(x.agentId, leadList, smsList);
                            sMSLHS.Add(x);
                        }

                    });
                    sMSLHS = sMSLHS.OrderByDescending(a => a.DateSent).ToList();
                }
            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
                {
                    sMSLHS = conn.Query<SMSMessage>(sqlQuery).AsList();
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllSmsMessagesByAgentId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            sMSLHS = null;
        }

        return sMSLHS;
    }
    public bool? GetIsReadValue(int agentId,List<LeadTempClass> leadList, List<SMSMessage> smsList)
    {
        var leadObj = leadList.Where(x => x.leadid.Equals(agentId)).FirstOrDefault();
        if (leadObj != null) {
            var smsObj = smsList.Where(x => x.LeadId.Equals(agentId) && x.FromPhone.Equals(leadObj.Phone)).FirstOrDefault();
            if (smsObj != null) {
                return smsObj.isread;
            }
            return null;
        }
        else {
            return null;
        }
    }
    public List<SMSMessage> GetAllSmsMessagesByAgentId(int agentId, int loginagentId, string agentPhone, int? messageId = null, int? agentTypeIds = null)
    {
        var messages = new List<SMSMessage>();
        var updateSql = "";
        if (agentTypeIds != 10)
        {
            updateSql = $@"update sms set isread={true} where  (fromphone = (select phone from agents where agentID = {agentId}))"; // AND fromphone={agentPhone}) OR ((fromphone = (select phone from agents where agentID = {agentId})) AND tophone={agentPhone})";
        }
        else
        {
            updateSql = $@"update sms set isread={true} where  
            (fromphone = (select phone1 from leads where leadid = {agentId}) or fromphone =(select phone2 from leads where leadid = {agentId}) or
				   fromphone = (select phone3 from leads where leadid = {agentId}) or fromphone =(select phone4 from leads where leadid = {agentId}) or
                   fromphone =(select phone5 from leads where leadid = {agentId}) 
            )";
        }
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                bool IsRead = conn.Query(updateSql).Any();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllSmsMessagesByAgentId", $"Query: {updateSql}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        agentPhone = agentPhone.Replace("+1", "");
        var sqlQuery = "";
        if (agentTypeIds != 10)
        {
            if (messageId != null)
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from agents where agentid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE id>{messageId} AND
            (fromphone = (select phone from agents where agentID =  {agentId} AND tophone={agentPhone}) OR (tophone=(select phone from agents where agentID = {agentId} AND fromphone={agentPhone})))
            AND datesent >  previousweek(1, '-07:00')            
            ORDER BY datesent asc;";
                //            
            }
            else
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from agents where agentid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE (fromphone = (select phone from agents where agentID =  {agentId} AND tophone={agentPhone}) OR (tophone=(select phone from agents where agentID = {agentId} AND fromphone={agentPhone})))            
            AND datesent >  previousweek(1, '-07:00')
            ORDER BY datesent asc;";
                //
            }
        }
        else
        {
            if (messageId != null)
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from agents where agentid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE id>{messageId} AND
             (fromphone = (select phone1 from leads where leadid = {agentId} AND tophone={agentPhone}) OR (tophone=(select phone1 from leads where leadid = {agentId} AND fromphone={agentPhone})))
            AND convert_tz(datesent, '+00:00', '-07:00') >= ( CURDATE() - INTERVAL 2 week )
            ORDER BY datesent asc;";
            }
            else
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from leads where leadid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE ((fromphone = (select phone1 from leads where leadid = {agentId}) or fromphone =(select phone2 from leads where leadid = {agentId}) or
				   fromphone = (select phone3 from leads where leadid = {agentId}) or fromphone =(select phone4 from leads where leadid = {agentId}) or
                   fromphone =(select phone5 from leads where leadid = {agentId}) 
            ) AND tophone={agentPhone}) 
            OR (tophone = (select phone1 from leads where leadid = {agentId}) OR tophone=(select phone2 from leads where leadid = {agentId}) or
            (tophone = (select phone3 from leads where leadid = {agentId}) OR tophone=(select phone4 from leads where leadid = {agentId}) or
            tophone = (select phone5 from leads where leadid = {agentId})
            ) AND fromphone={agentPhone})
            AND convert_tz(datesent, '+00:00', '-07:00') >= ( CURDATE() - INTERVAL 2 week )
            ORDER BY datesent asc;";
            }
        }
        //convert_tz(datesent, '+00:00','-07:00')as
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                messages = conn.Query<SMSMessage>(sqlQuery).AsList();

                foreach (var item in messages)
                {
                    if (!string.IsNullOrEmpty(item.FileUrl))
                    {
                        var fileUrl = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Util.filefolderpath + "SMSMedia\\" + item.FileUrl + ".jpg" : Util.filefolderpath + "SMSMedia/" + item.FileUrl;
                        //var fileUrl = Util.filefolderpath + "SMSMedia//" + item.FileUrl;  // "c://code//plwebfiles//" + "SMSMedia//" + item.FileUrl;
                        if (File.Exists(fileUrl))
                        {
                            FileStream fileStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
                            byte[] data = new byte[(int)fileStream.Length];
                            fileStream.Read(data, 0, data.Length);
                            item.FileUrl = "data:image/jpg;base64," + Convert.ToBase64String(data);
                        }
                        else
                        {
                            item.FileUrl = "";
                        }
                    }
                    else
                    {
                        item.FileUrl = "";
                    }
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllSmsMessagesByAgentId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            messages = null;
        }

        return messages;
    }

    public List<SMSMessage> GetAllMessages()
    {
        List<SMSMessage> sMSMessage = new List<SMSMessage>();
        var sqlGetAllSMS = $@"select *,message as body from sms ORDER BY datesent asc;";
        try
        {
            string agentPhone = string.Empty;

            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                sMSMessage = conn.Query<SMSMessage>(sqlGetAllSMS).ToList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllMessages", $"Query: {sqlGetAllSMS}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            sMSMessage = null;
        }

        return sMSMessage;
    }

    public List<SMSMessage> GetSMSNotification()
    {
        List<SMSMessage> sMSLHS = new List<SMSMessage>();
        var sqlQuery = $@"                        	                    	                    
	                    select s.id id, a.agentid, s.agentsentby AgentSentBy, a.firstname FirstName, a.lastname LastName, s.tophone ToPhone, s.fromphone FromPhone, 
                            (select isread from sms where fromphone=(select phone from agents where agentid=a.agentid limit 1) AND isread=0 Limit 1) isread
                            from (
                            SELECT a.agentid, max(id) id from sms s join agents a on s.tophone = a.phone or s.fromphone = a.phone where a.agenttypeid = 50 or a.agenttypeid = 20 group by a.agentid) v 
                            join sms s on s.id = v.id join agents a on a.agentid = v.agentid 
                        UNION ALL

                        select s.id id,s.leadid as agentid , s.agentsentby AgentSentBy, l.firstname FirstName, l.lastname LastName, s.tophone ToPhone, s.fromphone FromPhone,
                            (select isread from sms where  
                            ((fromphone in (select phone1 from leads where phone1status=3)) or
                            (fromphone in (select phone2 from leads where phone2status=3)) or
                            (fromphone in (select phone3 from leads where phone3status=3)) or 
                            (fromphone in (select phone4 from leads where phone4status=3)) or
                            (fromphone in (select phone5 from leads where phone5status=3)) AND isread=0 AND (l.leadstatusid!=19 and lower( s.message) != lower('stop'))) limit 1) isread
                        from sms s 
                        join leads l on (l.phone1=fromphone or l.phone1 = tophone or l.phone2=fromphone or l.phone2 = tophone or l.phone3=fromphone or l.phone3 = tophone )
                        
                        where   ((fromphone in (select phone1 from leads where phone1status=3) or tophone in (select phone1 from leads where  phone1status=3))OR
								(fromphone in (select phone2 from leads where phone2status=3) or tophone in (select phone2 from leads where phone1status=3 ))OR
								(fromphone in (select phone3 from leads where phone3status=3) or tophone in (select phone3 from leads where phone1status=3 ))OR
								(fromphone in (select phone4 from leads where phone4status=3) or tophone in (select phone4 from leads where phone1status=3 ))OR
								(fromphone in (select phone5 from leads where phone5status=3) or tophone in (select phone5 from leads where phone1status=3 )))								
                                AND (l.leadstatusid!=19 and lower( s.message) != lower('stop'));";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                sMSLHS = conn.Query<SMSMessage>(sqlQuery).AsList();

            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllSmsMessagesByAgentId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            sMSLHS = null;
        }

        return sMSLHS;
    }

    public List<SMSMessage> GetAllSmsHistorybyAgent(int agentId, int loginagentId, string agentPhone, int? messageId = null, int? agentTypeIds = null)
    {
        var messages = new List<SMSMessage>();

        agentPhone = agentPhone.Replace("+1", "");
        var sqlQuery = "";
        if (agentTypeIds != 10)
        {
            if (messageId == null)
            {

                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from agents where agentid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE 
            (fromphone = (select phone from agents where agentID =  {agentId} AND tophone={agentPhone}) OR (tophone=(select phone from agents where agentID = {agentId} AND fromphone={agentPhone})))            
            ORDER BY datesent desc limit 10;";
            }
            else
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from agents where agentid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE id<{messageId} AND
            (fromphone = (select phone from agents where agentID =  {agentId} AND tophone={agentPhone}) OR (tophone=(select phone from agents where agentID = {agentId} AND fromphone={agentPhone})))            
            ORDER BY datesent desc limit 10;";
            }

        }
        else
        {
            if (messageId == null)
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from leads where leadid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE ((fromphone = (select phone1 from leads where leadid = {agentId}) or fromphone =(select phone2 from leads where leadid = {agentId}) or
				   fromphone = (select phone3 from leads where leadid = {agentId}) or fromphone =(select phone4 from leads where leadid = {agentId}) or
                   fromphone =(select phone5 from leads where leadid = {agentId}) 
            ) AND tophone={agentPhone}) 
            OR (tophone = (select phone1 from leads where leadid = {agentId}) OR tophone=(select phone2 from leads where leadid = {agentId}) or
            (tophone = (select phone3 from leads where leadid = {agentId}) OR tophone=(select phone4 from leads where leadid = {agentId}) or
            tophone = (select phone5 from leads where leadid = {agentId})
            ) AND fromphone={agentPhone})            
            ORDER BY datesent desc;";
            }
            else
            {
                sqlQuery = $@"select id,agentSentBy,{agentPhone}  as agentPhone, fromphone, tophone, isread, message as body,(select firstname from leads where leadid = {agentId}) as firstname ,convert_tz(datesent, '+00:00','-07:00')as datesent, (select firstname from agents where agentid=agentSentBy)as AgentName,fileurl
            from sms 
            WHERE id<{messageId} AND
            ((fromphone = (select phone1 from leads where leadid = {agentId}) or fromphone =(select phone2 from leads where leadid = {agentId}) or
				   fromphone = (select phone3 from leads where leadid = {agentId}) or fromphone =(select phone4 from leads where leadid = {agentId}) or
                   fromphone =(select phone5 from leads where leadid = {agentId}) 
            ) AND tophone={agentPhone}) 
            OR (tophone = (select phone1 from leads where leadid = {agentId}) OR tophone=(select phone2 from leads where leadid = {agentId}) or
            (tophone = (select phone3 from leads where leadid = {agentId}) OR tophone=(select phone4 from leads where leadid = {agentId}) or
            tophone = (select phone5 from leads where leadid = {agentId})
            ) AND fromphone={agentPhone})            
            ORDER BY datesent desc;";

            }

        }
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                messages = conn.Query<SMSMessage>(sqlQuery).AsList();

                foreach (var item in messages)
                {
                    if (!string.IsNullOrEmpty(item.FileUrl))
                    {
                        var fileUrl = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Util.filefolderpath + "SMSMedia\\" + item.FileUrl + ".jpg" : Util.filefolderpath + "SMSMedia/" + item.FileUrl;
                        //var fileUrl = Util.filefolderpath + "SMSMedia//" + item.FileUrl;  // "c://code//plwebfiles//" + "SMSMedia//" + item.FileUrl;
                        if (File.Exists(fileUrl))
                        {
                            FileStream fileStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
                            byte[] data = new byte[(int)fileStream.Length];
                            fileStream.Read(data, 0, data.Length);
                            item.FileUrl = "data:image/jpg;base64," + Convert.ToBase64String(data);
                        }
                        else
                        {
                            item.FileUrl = "";
                        }
                    }
                    else
                    {
                        item.FileUrl = "";
                    }
                }
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllSmsMessagesByAgentId", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            messages = null;
        }

        return messages;
    }

    public List<string> GetAllPhonesOnLeads()
    {
        var phones = new List<string>();
        var sqlquery = $@"select phone1 from leads
        where phone1 <> 0
        and phone1status > 0  and phone1status< 4
        and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60);";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                phones = conn.Query<string>(sqlquery).ToList();
            }

            var tempphones = new List<string>();
            sqlquery = $@"select phone2 from leads
                where phone2 <> 0
                and phone2status > 0  and phone2status< 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone3 from leads
                where phone3 <> 0
                and phone3status > 0  and phone3status< 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone4 from leads
                where phone4 <> 0
                and phone4status > 0  and phone4status< 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60);";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone5 from leads
                where phone5 <> 0
                and phone5status > 0  and phone5status< 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllMissingClockOut", $"Query: {sqlquery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return phones;
    }

    public List<string> GetAllBadPhonesOnLeads()
    {
        var phones = new List<string>();
        var sqlquery = $@"select phone1 from leads
        where phone1 <> 0
        and phone1status = 4
        and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60);";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                phones = conn.Query<string>(sqlquery).ToList();
            }

            var tempphones = new List<string>();
            sqlquery = $@"select phone2 from leads
                where phone2 <> 0
                and phone2status = 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone3 from leads
                where phone3 <> 0
                and phone3status = 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone4 from leads
                where phone4 <> 0
                and phone4status = 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60);";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);

            sqlquery = $@"select phone5 from leads
                where phone5 <> 0
                and phone5status = 4
                and leadstatusid in (1, 9, 50, 51, 52, 53, 54, 55, 56, 58, 59, 60)";
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                tempphones = conn.Query<string>(sqlquery).ToList();
            }
            phones.AddRange(tempphones);
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllMissingClockOut", $"Query: {sqlquery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return phones;
    }
    #endregion

    #region lookup
    public bool InsertLookupAudit(LookupAudit lookupAudit)
    {
        var runSuccessful = true;

        int personfound = (lookupAudit.PersonFound) ? 1 : 0;
        var insertLookupAudut = $@"INSERT INTO lookupaudit (agentid, leadid, personfound, jsonlookupdata, jsonsubmittedleadinfo)
                VALUES ({lookupAudit.AgentId}, {lookupAudit.LeadId}, {personfound}, '{lookupAudit.JsonLookupData}', '{lookupAudit.JsonSubmittedLeadInfo}')";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                int affectedRows = conn.Execute(insertLookupAudut);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.InsertLookupAudit error", $"Message: {err.Message}, StackTrace: {err.StackTrace}");
            Logger.Log("MySqlDataAgent.InsertLookupAudit error query", $"Query: {insertLookupAudut}");
            runSuccessful = false;
        }

        return runSuccessful;
    }
    #endregion

    #region Inventory
    public List<Manufactur> GetManufacturers()
    {
        var manufacturers = new List<Manufactur>();
        string sqlQuery = @"SELECT manufacturerId, manufacturer FROM manufacturer";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                manufacturers = conn.Query<Manufactur>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetManufacturers", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            manufacturers = null;
        }

        return manufacturers;
    }

    public List<Inventory> GetInventories(string manufacturerId)
    {
        var products = new List<Inventory>();

        string sqlQuery = $"SELECT * FROM products where manufacturerId = '{manufacturerId}'";
        if (manufacturerId == "0")
        {
            sqlQuery = "SELECT * FROM products";
        }

        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                products = conn.Query<Inventory>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetInventories", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            products = null;
        }

        return products;
    }

    public Inventory GetInventoryByProductNumber(string ProductNumber)
    {
        var products = new Inventory();
        string sqlQuery = $"SELECT * FROM products where productnumber = '{ProductNumber}'";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                products = conn.QuerySingleOrDefault<Inventory>(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetInventories", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            products = null;
        }

        return products;
    }

    public bool SaveInventories(IEnumerable<Inventory> inventories)
    {
        var runSuccessful = true;

        foreach (var inventory in inventories)
        {
            if (inventory.ProductNumber != null)
            {
                var insertInventory =
            $@"INSERT INTO invetorytrack (agentId, inOrout, productnumber, serialnumber, manutext, producttext) 
                VALUES ({inventory.AgentId}, '{inventory.InOrOut}', '{inventory.ProductNumber}', 
                '{inventory.SerialNumber}',
                '{inventory.ManuText}', '{inventory.ProductText}')";
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
                    {
                        int affectedRows = conn.Execute(insertInventory);
                    }
                }
                catch (Exception err)
                {
                    Logger.Log("PLWEB.ERROR MySqlDataAgent.SaveInventories", $"Query: {insertInventory}, Message: {err.Message}, StackTrace: {err.StackTrace}");
                    runSuccessful = false;
                }
            }
        }
        return runSuccessful;
    }

    public List<Inventory> InventoryReport(DateTime FromDate, DateTime ToDate)
    {
        var products = new List<Inventory>();

        string sqlQuery =
        $@"SELECT *, DATE_FORMAT(insertdate,'%Y%m%d%H%i')Invoice, (SELECT firstname FROM agents WHERE agents.agentid = invetorytrack.agentid limit 1)Agent 
        FROM invetorytrack WHERE CONVERT_TZ(insertdate,'+00:00','-07:00')  
        BETWEEN '{FromDate.ToString("yyyy-MM-dd")} 00:00:00' AND '{ToDate.ToString("yyyy-MM-dd")} 23:59:59';";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                products = conn.Query<Inventory>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR MySqlDataAgent.InventoryReport", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            products = null;
        }

        return products;
    }
    #endregion

    #region TEST
    public string Getpipleresponse(int lookupauditid)
    {
        //var piplesponse = string.Empty;
        var sqlQuery = $"select jsonlookupdata from lookupaudit where lookupauditid = {lookupauditid}";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                var piplesponse = conn.ExecuteScalar(sqlQuery);
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAgents", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
        return string.Empty;
    }

    #endregion

    #region Announcement

    public List<Announce> GetAllAnnouncement()
    {
        var announce = new List<Announce>();
        var sqlQuery = $@"select * from announce where active=true;";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(MySqlConnStr))
            {
                announce = conn.Query<Announce>(sqlQuery).AsList();
            }
        }
        catch (Exception err)
        {
            Logger.Log("MySqlDataAgent.GetAllAnnouncement", $"Query: {sqlQuery}, Message: {err.Message}, StackTrace: {err.StackTrace}");
            announce = null;
        }
        return announce;
    }
    #endregion

}
