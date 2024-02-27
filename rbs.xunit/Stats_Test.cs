using System.IO;

namespace plweb.xunit
{
    using System;
    using Xunit;

    public class Stats_Test
    {
        [Fact]
        public void GetNewDialStatsTest()
        {
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var statsAgent = new StatsAgent();
            var stats = statsAgent.GetDialAgentStats();
        }

        [Fact]
        public void GetLookupFromDB()
        {
            try
            {
                using (var reader = new StreamReader(@"C:\Users\spenc\Downloads\nho.csv"))
                {
                    //import into DB here
                    while (!reader.EndOfStream)
                    {
                        var fields = reader.ReadLine().Split(',');
                        var leads = new MySqlDataAgent().SearchLeadByAddress(fields[2]);

                        if (leads != null && leads.Count == 0)
                        {
                            var lead = new Lead();
                            //insert
                            lead.FirstName = fields[0];
                            lead.LastName = fields[1];
                            lead.Address = fields[2];
                            lead.City = fields[3];
                            lead.State = fields[4];
                            lead.Postal = fields[5];
                            lead.County = fields[7];
                            lead.InsertDate = DateTime.Now;

                            lead.LeadTypeId = 1; //NHO=1, TO=3
                            lead.SoldDate = Convert.ToDateTime(fields[8]);
                            lead.LeadStatusId = 1;
                            lead.Phone1 = fields[6].Replace(" ", "").Replace("-", "").Replace("(", "")
                                .Replace(")", "");
                            lead.Phone1Status = (lead.Phone1Status == null) ? 0 : lead.Phone1Status;
                            lead.Phone2Status = (lead.Phone2Status == null) ? 0 : lead.Phone2Status;
                            lead.Phone3Status = (lead.Phone3Status == null) ? 0 : lead.Phone3Status;
                            lead.Phone4Status = (lead.Phone4Status == null) ? 0 : lead.Phone4Status;
                            lead.Phone5Status = (lead.Phone5Status == null) ? 0 : lead.Phone5Status;
                            int newLeadId = new MySqlDataAgent().InsertLead(lead);

                            var note = new Note()
                            {
                                LeadId = newLeadId,
                                AccountId = -1,
                                AgentId = 1,
                                NoteText = $"NHO 180 TS {fields[8]}",
                                LeadStatusId = 52
                            };
                            //submit note
                            new NotesAgent().SaveNote(note);
                        }
                        else
                        {
                            Console.WriteLine("Dup!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
