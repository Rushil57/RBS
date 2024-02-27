using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePayLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using plweb.Internals;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    public class BluePayController : Controller
    {
        private readonly IOptions<BluePayData> _bluePayData;


        public BluePayController(IOptions<BluePayData> bluePayData)
        {
            this._bluePayData = bluePayData;
        }

        [HttpGet("CreateRecurringPaymentACH")]
        public string CreateRecurringPaymentACH()
        {
            string message = string.Empty;
            try
            {
                string accountID = _bluePayData.Value.AccountID;
                string secretKey = _bluePayData.Value.SecretKey;
                string mode = _bluePayData.Value.Mode;

                accountID = "100686682792";
                secretKey = "ULE3/8PWSQRTVWRGBTAIUHYJEHLURUSW";
                mode = "TEST";

                BluePay rebill = new BluePay
                (
                    accountID,
                    secretKey,
                    mode
                );

                rebill.SetCustomerInformation
                (
                    firstName: "Bob",
                    lastName: "Tester",
                    address1: "1234 Test St.",
                    address2: "Apt #500",
                    city: "Testville",
                    state: "IL",
                    zip: "54321",
                    country: "USA",
                    phone: "123-123-12345",
                    email: "test@bluepay.com"
                );

                rebill.SetACHInformation
                (
                    routingNum: "123123123",
                    accountNum: "123456789",
                    accountType: "C",
                    docType: "WEB"
                );

                // Sets recurring payment
                rebill.SetRebillingInformation
                (
                    rebFirstDate: "2019-01-01", // Rebill Start Date: Jan. 1, 2015
                    rebExpr: "1 MONTH", // Rebill Frequency: 1 MONTH
                    rebCycles: "12", // Rebill # of Cycles: 12
                    rebAmount: "15.00" // Rebill Amount: $15.00
                );

                // Sets a Card Authorization at $0.00
                rebill.Auth(amount: "0.00");

                // Makes the API Request
                rebill.Process();

                // If transaction was successful reads the responses from BluePay
                if (rebill.IsSuccessfulTransaction())
                {
                    Console.WriteLine("Transaction ID: " + rebill.GetTransID());
                    Console.WriteLine("Rebill ID: " + rebill.GetRebillID());
                    Console.WriteLine("Transaction Status: " + rebill.GetStatus());
                    Console.WriteLine("Transaction Message: " + rebill.GetMessage());
                    Console.WriteLine("Masked Payment Account: " + rebill.GetMaskedPaymentAccount());
                    Console.WriteLine("Customer Bank Name: " + rebill.GetBank());
                }
                else
                {
                    message = "Error: " + rebill.GetMessage();
                    Console.WriteLine("Error: " + rebill.GetMessage());
                }
            }
            catch (Exception err)
            {

            }

            return message;
        }

        [HttpPost("CreateRecurringPaymentCC")]
        public IActionResult CreateRecurringPaymentCC([FromBody] Payments payments)
        {
            bool IsPayment = false;
            string message = string.Empty;
            string accountID = _bluePayData.Value.AccountID;
            string secretKey = _bluePayData.Value.SecretKey;
            string mode = _bluePayData.Value.Mode;
            
            BluePay rebill = new BluePay
            (
                accountID,
                secretKey,
                mode
            );

            string firstName = payments.FirstName;
            string lastName = payments.LastName;
            string address1 = "1234 Test St.";
            string address2 = "Apt #500";
            string city = "Testville";
            string state = "IL";
            string zip = "54321";
            string country = "USA";
            string phone = "123-123-12345";
            string email = "test@bluepay.com";
            
            string merchantName = firstName + " " + lastName;
            string returnURL = _bluePayData.Value.PaymentsRedirectURL; 
            string transactionType = "AUTH";
            string acceptDiscover = "No";
            string acceptAmex = "No";
            string amount = payments.Amount.ToString();
            string protectAmount = "No";
            string rebilling = "No";
            string rebProtect = "Yes";
            string rebAmount = null;
            string rebCycles = null;
            string rebStartDate = null;
            string rebFrequency = null;
            string customID1 = null;
            string protectCustomID1 = "No";
            string customID2 = null;
            string protectCustomID2 = "No";
            string paymentTemplate = "mobileform01";
            string receiptTemplate = "mobileresult01";
            string receiptTempRemoteURL = null;

            string Result = rebill.GenerateURL(merchantName,returnURL,transactionType,acceptDiscover,acceptAmex,
                amount,protectAmount,rebilling,rebProtect,rebAmount,rebCycles,rebStartDate,rebFrequency,customID1,
                protectCustomID1,customID2,protectCustomID2,paymentTemplate,receiptTemplate,receiptTempRemoteURL);

            #region comments
            //rebill.SetCustomerInformation
            //(
            //    firstName: "Bob",
            //    lastName: "Tester",
            //    address1: "1234 Test St.",
            //    address2: "Apt #500",
            //    city: "Testville",
            //    state: "IL",
            //    zip: "54321",
            //    country: "USA",
            //    phone: "123-123-12345",
            //    email: "test@bluepay.com"
            //);

            //rebill.SetCCInformation
            //(
            //    ccNumber: "4111111111111111",
            //    ccExpiration: "1225",
            //    cvv2: "123"
            //);

            // Sets recurring payment
            //rebill.SetRebillingInformation
            //(
            //    rebFirstDate: "2019-01-01", // Rebill Start Date: Jan. 1, 2015
            //    rebExpr: "1 MONTH", // Rebill Frequency: 1 MONTH
            //    rebCycles: "12", // Rebill # of Cycles: 12
            //    rebAmount: "15.00" // Rebill Amount: $15.00
            //);
            // Sets a Card Authorization at $0.00
            //rebill.Auth(amount: "1.00");

            //// Makes the API Request
            //rebill.Process();

            // If transaction was successful reads the responses from BluePay
            //if (rebill.IsSuccessfulTransaction())
            //{
            //    IsPayment = true;
            //    Console.WriteLine("Transaction ID: " + rebill.GetTransID());
            //    Console.WriteLine("Rebill ID: " + rebill.GetRebillID());
            //    Console.WriteLine("Transaction Status: " + rebill.GetStatus());
            //    Console.WriteLine("Transaction Message: " + rebill.GetMessage());
            //    Console.WriteLine("AVS Response: " + rebill.GetAVS());
            //    Console.WriteLine("CVV2 Response: " + rebill.GetCVV2());
            //    Console.WriteLine("Masked Payment Account: " + rebill.GetMaskedPaymentAccount());
            //    Console.WriteLine("Card Type: " + rebill.GetCardType());
            //    Console.WriteLine("Authorization Code: " + rebill.GetAuthCode());

            //    string str = "Transaction ID: " + rebill.GetTransID() + " Rebill ID: " + rebill.GetRebillID() +
            //        " Transaction Status: " + rebill.GetStatus() + " Transaction Message: " + rebill.GetMessage() +
            //        " AVS Response: " + rebill.GetAVS() + " CVV2 Response: " + rebill.GetCVV2() +
            //        " Masked Payment Account: " + rebill.GetMaskedPaymentAccount() + " Card Type: " + rebill.GetCardType() +
            //        " Authorization Code: " + rebill.GetAuthCode();
            //    message = str;
            //}
            //else
            //{
            //    IsPayment = false;
            //    message = "Error: " + rebill.GetMessage();
            //    Console.WriteLine("Error: " + rebill.GetMessage());
            //}
            #endregion

            return Ok(Result);
        }

        public string Generatetocken()
        {
            string message = string.Empty;
            string accountID = "100686682792";
            string secretKey = "ULE3/8PWSQRTVWRGBTAIUHYJEHLURUSW";
            string mode = "TEST";

            BluePay token = new BluePay
            (
                accountID,
                secretKey,
                mode
            );

            token.SetCustomerInformation
            (
                firstName: "Bob",
                lastName: "Tester",
                address1: "1234 Test St.",
                address2: "Apt #500",
                city: "Testville",
                state: "IL",
                zip: "54321",
                country: "USA",
                phone: "123-123-12345",
                email: "test@bluepay.com"
            );

            token.SetCCInformation
            (
                ccNumber: "4111111111111111",
                ccExpiration: "1225",
                cvv2: "123"
            );

            // Card Authorization Amount: $0.00
            token.Auth(
                amount: "1.00",
                newCustomerToken: "029384230984" // "true" generates random string. Other values will be used literally
            );

            // Makes the API Request with BluePay
            token.Process();

            // Try again if we accidentally create a non-unique token
            if (token.GetMessage().Contains("Customer%20Tokens%20must%20be%20unique"))
            {
                token.Auth(
                    amount: "0.00",
                    newCustomerToken: "true"
                );
                token.Process();
            }

            // If transaction was successful reads the responses from BluePay
            if (token.IsSuccessfulTransaction())
            {
                BluePay payment = new BluePay
                (
                    accountID,
                    secretKey,
                    mode
                );

                Console.WriteLine(token.GetCustomerToken());
                payment.Sale(
                    amount: "3.99",
                    customerToken: token.GetCustomerToken()
                );

                payment.Process();

                if (payment.IsSuccessfulTransaction())
                {
                    Console.WriteLine("Transaction Status: " + payment.GetStatus());
                    Console.WriteLine("Transaction ID: " + payment.GetTransID());
                    Console.WriteLine("Transaction Message: " + payment.GetMessage());
                    Console.WriteLine("AVS Response: " + payment.GetAVS());
                    Console.WriteLine("CVV2 Response: " + payment.GetCVV2());
                    Console.WriteLine("Masked Payment Account: " + payment.GetMaskedPaymentAccount());
                    Console.WriteLine("Card Type: " + payment.GetCardType());
                    Console.WriteLine("Authorization Code: " + payment.GetAuthCode());
                    Console.WriteLine("Customer Token: " + payment.GetCustomerToken());
                }
                else
                {
                    message = "Error: " + payment.GetMessage();
                    Console.WriteLine("Error: " + payment.GetMessage());
                }
            }
            else
            {
                message = "Error: " + token.GetMessage();
                Console.WriteLine("Error: " + token.GetMessage());
            }
            return message;
        }
    }
}
