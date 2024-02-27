using System;
using System.Collections.Generic;

public class Lead
{
    public int LeadId { get; set; }
    public int LeadTypeId { get; set; }
    public int LeadStatusId { get; set; }
    public int FormerLeadStatusId { get; set; }
    public int AgentIdSubmitting { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Postal { get; set; }
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }
    public string Phone3 { get; set; }
    public string Phone4 { get; set; }
    public string Phone5 { get; set; }
    public int? Phone1Status { get; set; } 
    public int? Phone1Dial { get; set; }
    public int? Phone2Status { get; set; }
    public int? Phone2Dial { get; set; }
    public int? Phone3Status { get; set; }
    public int? Phone3Dial { get; set; }
    public int? Phone4Status { get; set; }
    public int? Phone4Dial { get; set; }
    public int? Phone5Status { get; set; }
    public int? Phone5Dial { get; set; }
    public string County { get; set; }
    public string FormerCity { get; set; }
    public string FormerState { get; set; }
    public int NoteCount { get; set; }
    public DateTime SoldDate { get; set; }
    public DateTime InsertDate { get; set; }
    public DateTime FollowUpDate { get; set; }
    public DateTime SchedApptDate { get; set; }
    public string StatusText { get; set; }
    public string SANote { get; set; }
    public string TimeZone { get; set; } = "MST";
    public int HomeValue { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string LeadTypeText { get; set; }
    public string DispatchedRepsName { get; set; }
    public bool IsCountState { get; set; }
    public int FieldRepId { get; set; }
    public int TechId { get; set; }
}

public class LeadUpdateRequest
{
    public Lead LeadToUpdate { get; set; }
    public Note NoteToSave { get; set; }
    public string PageRequestingUpdate { get; set; }
    public int AccountId { get; set; } = -1;
}

public class CalendarLeads
{
    public List<Lead> Today { get; set; }
    public List<Lead> Yesterday { get; set; }
    public List<Lead> Future { get; set; }
    public List<Lead> LastWeek { get; set; }
    public List<Lead> ThisWeek { get; set; }
    public List<Lead> UFS { get; set; }
    public List<Lead> Reschedule { get; set; }
    public List<Lead> LastWeekThisWeekLeadData { get; set; }
}

public class RawLeadCount
{
    public int AZ { get; set; }
    public int TX { get; set; }
}