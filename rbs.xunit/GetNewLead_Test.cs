namespace plweb.xunit
{
    using System;
    using Xunit;

    public class GetNewLead_Test
    {
        [Fact]
        public void GetNewLeadTest()
        {
            var fromDate = new DateTime();
            var toDate = new DateTime();
            var newLead = new StatsAgent();
            var leads = newLead.GetLeadGenStats(fromDate, toDate);
        }

        [Theory]
        [InlineData(1247)]
        public void GetLookupFromDB(int lookupauditid)
        {
            var personRequest = new Lead()
            {
                FirstName = "Spencer",
                MiddleName = "L",
                LastName = "Thomason",
                City = "Gilbert",
                State = "AZ"
            };
            var pipleresponse = new MySqlDataAgent().Getpipleresponse(lookupauditid);

            var phoneListPipl = new PiplAgent().LookupPipl(personRequest,
                testResponse: pipleresponse);
        }

        [Fact]
        public void Test_LookupPiplWithLib()
        {
            var personRequest = new Lead()
            {
                FirstName = "Spencer",
                MiddleName = "L",
                LastName = "Thomason",
                City = "Gilbert",
                State = "AZ"
            };
            var testresult = new PiplAgent().LookupPiplWithLib(personRequest);
        }

        [Fact]
        public void Test_LookupPipl()
        {
            var personRequest = new Lead()
            {
                FirstName = "Spencer",
                MiddleName = "L",
                LastName = "Thomason",
                City = "Gilbert",
                State = "AZ"
            };
            var testresult = new PiplAgent().LookupPipl(personRequest);
        }
    }
}
