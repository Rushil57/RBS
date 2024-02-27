using System;
using Microsoft.AspNetCore.Http;

public class Note
{
    public long NoteId { get; set; }
    public long? LeadId { get; set; }
    public long? AccountId { get; set; }
    public int? AgentId { get; set; }
    public int? LeadStatusId { get; set; } = 0;
    public string AgentName { get; set; }
    public string NoteText { get; set; }
    public string FileName { get; set; }
    public DateTime InsertDate { get; set; }
}
