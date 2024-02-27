using System;
using System.Collections.Generic;

public class TimeclockRequest
{
    public List<TimeclockAction> History { get; set; }
    /// <summary>
    /// 0 for ClockOut.  1 for ClockIn.
    /// </summary>
    public int LastActionType { get; set; }
}

public class TimeclockAction
{
    public int TimeclockId { get; set; }
    public int AgentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ClockedInAt { get; set; }
    public DateTime ClockedOutAt { get; set; }
    public object HrsWorked { get; set; }
    public string Status { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool MaxDollarPerHour { get; set; }
    public DateTime? PayDate { get; set; }
}