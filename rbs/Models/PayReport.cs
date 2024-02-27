using System;
using System.Collections.Generic;

public class PayReport
{
    public List<Lead> ConfirmedAppointments { get; set; } = new List<Lead>();
    public List<Lead> CompletedInstalls { get; set; } = new List<Lead>();
    public List<Lead> NoShowLead { get; set; } = new List<Lead>();
    public int Hours { get; set; }
    public int HourlyRate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int AgentId { get; set; }

    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string  FileFullPath { get; set; }
    public double TotalMinutes { get; set; }
}

