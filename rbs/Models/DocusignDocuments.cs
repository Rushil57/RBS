using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DocusignDocuments
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string FileName { get; set; }
    public int AgentId { get; set; }
    public DateTime InsertDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int IsAssigned { get; set; }    
}
