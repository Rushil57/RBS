using System;
using Xunit;

namespace plweb.xunit
{
    public class CalendarAgent_Test
    {
        [Theory]
        [InlineData("")]
        //[InlineData("4805260027")] //Danny's 
        public void InsertCalendar_Test(string to)
        {
            DispatchAgentRequest dispatch = new DispatchAgentRequest()
            {
                SubmittingAgent = 1,
                AgentBeingDispatched = 5,
                EquipmentOrder = "Test note",
                LeadToDispatch = new Lead()
                {
                    LeadId = -99999,
                    FirstName = "Spencer1",
                    Address = "address1",
                    City = "City1",
                    Phone1 = "phone1",
                    SchedApptDate = DateTime.Parse("11/28/18 8:30 AM")
                }
            };

            Assert.True(new CalendarAgent().SendCalendar(dispatch));

        }
    }
}
