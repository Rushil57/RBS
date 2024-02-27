using plweb.Controllers;
using System;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;
using Xunit;

namespace plweb.xunit
{
    public class SMSAgent_Test
    {
        [Theory]
        [InlineData("4805122033")]
        //[InlineData("4805260027")]
        public void GetNewLeadTest(string to)
        {
            var from = "+14804000754";
            var message = new SMSMessage()
            {
                Body = "Prolink CRM SMS Messaging is alive!",
                ToPhone = to,
                FromPhone = from,
                MessageStatus = MessageResource.StatusEnum.Sending.ToString()
            };

            var smsAgent = new SMSAgent();
            Assert.True(smsAgent.SendSMS(message));
        }

        [Fact]
        public void TestSMSInsert()
        {
            var smsAgent = new SMSAgent();
            //var twil = smsAgent.RecieveSMS("body:ToCountry=US&MediaContentType0=image%2Fjpeg&ToState=AZ&SmsMessageSid=MMccbc193f1ae18801e7a9db3a5d452667&NumMedia=1&ToCity=&FromZip=&SmsSid=MMccbc193f1ae18801e7a9db3a5d452667&FromState=CA&SmsStatus=received&FromCity=&Body=logo&FromCountry=US&To=%2B14804000754&ToZip=&NumSegments=1&MessageSid=MMccbc193f1ae18801e7a9db3a5d452667&AccountSid=AC00ccbda37c9620b827d19864caf9191f&From=%2B14805122033&MediaUrl0=https%3A%2F%2Fapi.twilio.com%2F2010-04-01%2FAccounts%2FAC00ccbda37c9620b827d19864caf9191f%2FMessages%2FMMccbc193f1ae18801e7a9db3a5d452667%2FMedia%2FME570ccee57af5b923b0f5435efb3cba7b&ApiVersion=2010-04-01");
            var twil = smsAgent.RecieveSMS("ToCountry=US&Body=Yes&To=%2B14804000271&From=%2B13235143123");
            Assert.True(twil != null);

            //Temp
            //Message message = new Message();
            //message.Body("ToCountry=US&Body=Test+13&ToPhone=%2B14804000754&FromPhone=%2B14805122033&MediaUrl=https://demo.twilio.com/owl.png");
            //message.Media(new Uri("https://demo.twilio.com/owl.png"));
            //var twil = smsAgent.RecieveSMS(message.ToString());

        }

        [Theory]
        [InlineData("4805122033")]
        //[InlineData("4805260027")] //Danny's 
        public void Dispatch_Test(string to)
        {
            DispatchAgentRequest dispatch = new DispatchAgentRequest()
            {
                SubmittingAgent = 1,
                AgentBeingDispatched = 13,
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

            Assert.True(new AgentAgent().DispatchAgent(dispatch));

        }

        [Fact]
        public void SMSSpam()
        {
            int count = 0;
            try
            {
                var success = true;
                MySqlDataAgent dataAgent = new MySqlDataAgent();
                //var phonelist = dataAgent.GetAllPhonesOnLeads();
                var phonelist = dataAgent.GetAllBadPhonesOnLeads();

                var from = "+14804000271";
                var message = new SMSMessage()
                {
                    Body = $@"Have you gotten a FREE Prolink Doorbell Camera to see your house when you’re not there?
Grab yours today! https://prolinkprotection.com/doorbellcamera/

For more info, reply 1. Reply STOP to remove.",
                    ToPhone = "4805122033",
                    FromPhone = from,
                    MessageStatus = MessageResource.StatusEnum.Sending.ToString()
                };

                var smsAgent = new SMSAgent();
                foreach (var phone in phonelist)
                {
                    if (count < 0)
                    {
                        var s = "do nothing";
                    }
                    //else if (count > 299)
                    //{
                    //    break;
                    //}
                    else
                    {
                        message.ToPhone = phone;
                        success = smsAgent.TwilioSendSMS(message);
                    }
                    count++;
                }

                //success = smsAgent.TwilioSendSMS(message);
                //message.ToPhone = "4805260027";
                //success = smsAgent.TwilioSendSMS(message);

                Assert.True(success);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
