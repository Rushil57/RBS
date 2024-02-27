using System.Collections.Generic;

public class LookupResponse
{
    public bool PersonFound { get; set; }
    public List<ProlinkPerson> PossiblePeople { get; set; } = new List<ProlinkPerson>();
}

public class ProlinkPerson
{
    public List<string> PossibleSpouse { get; set; } = new List<string>();
    public List<string> Emails { get; set; } = new List<string>();
    public List<string> Addresses { get; set; } = new List<string>();
    public int Age { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public List<Pipl.APIs.Data.Fields.Phone> Phones { get; set; } = new List<Pipl.APIs.Data.Fields.Phone>();
}

