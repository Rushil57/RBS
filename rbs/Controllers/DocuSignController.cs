using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using plweb.Agents;
using plweb.Internals;
using plweb.Models;
using SautinSoft;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace plweb.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class DocuSignController : Controller
    {
        private static string AccountId { get; set; }
        // Point to DocuSign Demo (sandbox) environment for requests
        public const string RestApiUrl = "https://demo.docusign.net/restapi";

        TestConfig testConfig = new TestConfig();

        // This is an application-specific param that may be passed around during the OAuth
        // flow. It allows the app to track its flow, in addition to more security.
        public const string stateOptional = "testState";

        private readonly IOptions<DocuSignSDK> docuSignSetting;

        public DocuSignController(IOptions<DocuSignSDK> docuSignSetting)
        {
            this.docuSignSetting = docuSignSetting;
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            //Code example
            // These items are all registered at the DocuSign Admin console and are required 
            // to perform the OAuth flow.
            // Instantiating a client.
            ApiClient apiClient = new ApiClient(RestApiUrl);
            // Adding signature as out scope.
            List<string> scopes = new List<string>
            {
                OAuth.Scope_SIGNATURE
            };
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // STEP 1: Get the Auth URI          
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            Uri oauthLoginUrl = apiClient.GetAuthorizationUri(this.docuSignSetting.Value.ClientId, scopes, this.docuSignSetting.Value.RedirectUri, OAuth.CODE, stateOptional);

            return Redirect(oauthLoginUrl.AbsoluteUri);
        }

        [HttpGet]
        [Route("GetDocuSignCode")]
        public IActionResult GetDocuSignCode()
        {
            DocuSignViewModel docuSignViewModel = new DocuSignViewModel();
            ApiClient apiClient = new ApiClient(RestApiUrl);

            string code = HttpContext.Request.Query["code"].ToString();
            OAuth.OAuthToken oAuthToken = apiClient.GenerateAccessToken(this.docuSignSetting.Value.ClientId, this.docuSignSetting.Value.ClientSecret, code);
            if (oAuthToken != null)
            {
                OAuth.UserInfo userInfo = apiClient.GetUserInfo(oAuthToken.access_token);

                foreach (var item in userInfo.Accounts)
                {
                    if (item.IsDefault == "true")
                    {
                        AccountId = item.AccountId;
                        testConfig.AccountId = item.AccountId;
                        apiClient = new ApiClient(item.BaseUri + "/restapi");
                        break;
                    }
                }
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                //envDef.EmailSubject = "[DocuSign C# SDK] - Please sign this doc";
                //TemplateRole tRole = new TemplateRole();
                //tRole.Email = "givinidev@gmail.com";
                //tRole.Name = "Girish";
                //tRole.RoleName = "bob";
                //List<TemplateRole> rolesList = new List<TemplateRole>() { tRole };
                //envDef.TemplateRoles = rolesList;
                //envDef.TemplateId = "b92c9178-a7d3-4bae-ae67-18bbf09ec289";
                //envDef.Status = "sent";
                //EnvelopesApi envelopesApi = new EnvelopesApi(apiClient.Configuration);
                //EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(testConfig.AccountId, envDef);
                //docuSignViewModel.AuthToken = oAuthToken.access_token;
                //docuSignViewModel.UserInfo = userInfo;


                DateTime fromDate = DateTime.UtcNow;
                fromDate = fromDate.AddDays(-30);
                string fromDateStr = fromDate.ToString("o");

                // set a filter for the envelopes we want returned using the fromDate and count properties
                EnvelopesApi.ListStatusChangesOptions options = new EnvelopesApi.ListStatusChangesOptions()
                {
                    fromDate = fromDateStr
                };
                // Get all envelop list
                EnvelopesApi envelopesApi = new EnvelopesApi(apiClient.Configuration);
                EnvelopesInformation envelopes = envelopesApi.ListStatusChanges(AccountId, options);
                var pfdParseAgent = new PDFParseAgent(docuSignSetting);
                //var data = pfdParseAgent.ListEnvelopeDocuments(AccountId, "cdf59873-4d52-44b8-8969-9958a6e130c8");
                //var data = pfdParseAgent.ListEnvelopeDocuments("e6447790-d956-4405-bc2c-b1e896144d42", "1b8f8b6d-88bf-473e-9598-96b600a6aba6");

                var data = pfdParseAgent.ListEnvelopeDocuments(AccountId, envelopes);

            }
            else
            {
                docuSignViewModel.AuthToken = "Not Authorized";
            }

            return Ok(docuSignViewModel);
        }

        [HttpPost("UploadAccountFile/")]
        public ActionResult UploadAccountFile(IFormFile file)
        {
            if (file == null)
            {
                return Ok("");
            }

            return Ok(new PDFParseAgent(docuSignSetting).UploadAccountFile(file,
                Request.Form["agentid"].ToString(),
                Request.Form["noteText"].ToString()));
        }

        [HttpPost("AssignedNote")]
        public ActionResult AssignedNote(DocusignDocuments docusignDocuments)
        {
            string result = "";
            var pDFParseAgent = new PDFParseAgent(docuSignSetting);
            DocusignDocuments docusignDocument = new DocusignDocuments();
            var docData = pDFParseAgent.GetDosumentsById(docusignDocuments.Id);
            docusignDocument = docData;
            docusignDocument.AgentId = docusignDocuments.AgentId;
            docusignDocument.AccountId = docusignDocuments.AccountId;
            docusignDocument.FileName = docData.FileName;


            var resultData = pDFParseAgent.UpdateDocuSignFile(docusignDocument);
            if (resultData == true)
            {
                result = "Success";
            }
            return Ok(result);
        }

        [HttpPost("AssignedNoteManuallly")]
        public ActionResult AssignedNoteManuallly(IFormFile file)
        {
            string result = "";
            string fullpath = string.Empty;
            if (file == null)
            {
                return Ok(result);
            }
            var accountId = Convert.ToInt32(Request.Form["accountid"]);
            var fileNameNoSpace = file.FileName.Replace(" ", "");
            string filename = accountId + "_" + Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
            try
            {
                string filePath = Path.Combine(Util.filefolderpath);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fullpath = filePath + filename;

                var fileStream = new FileStream(fullpath, FileMode.Create);
                if (file.Length > 0)
                {
                    file.CopyTo(fileStream);
                }
                fileStream.Close();
                var agentid = Convert.ToInt32(Request.Form["agentid"]);

                var noteText = Convert.ToString(Request.Form["noteText"]);

                var pDFParseAgent = new PDFParseAgent(docuSignSetting);

                var noteagents = new NotesAgent();
                Note note = new Note();
                note.AccountId = accountId;
                note.NoteText = noteText;
                note.AgentId = agentid;
                note.LeadId = 0;
                note.FileName = filename;
                noteagents.SaveNote(note);

                DocusignDocuments docusignDocument = new DocusignDocuments();

                docusignDocument.AgentId = agentid;
                docusignDocument.AccountId = accountId;
                docusignDocument.FileName = filename;

                var resultData = pDFParseAgent.InserDocuSignFile(docusignDocument);
                if (resultData > 0)
                {
                    result = "Success";
                }
            }
            catch (Exception err)
            {
                Logger.Log("DocuSignController.AssignedNoteManuallly calling api ", err.Message + err.StackTrace);
                result = "";
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDocuSignAccount/")]
        public IActionResult GetDocuSignAccount()
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(dataAgent.GetAllDocuSignDosuments());
        }

        [HttpPost("UploadAccountFileTemp/")]
        public ActionResult UploadAccountFileTemp(IFormFile file)
        {
            string result = "new";
            string fullpath = string.Empty;
            if (file == null)
            {
                return Ok(result);
            }

            var fileNameNoSpace = file.FileName.Replace(" ", "");
            string filename = Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
            try
            {

                string filePath = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? Util.filefolderpath + "Account\\" : Util.filefolderpath + "Account/";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fullpath = filePath + filename;
                result = result + "File Name: " + fullpath;
                result = result + ": Before file upload";
                var fileStream = new FileStream(fullpath, FileMode.Create);
                if (file.Length > 0)
                {
                    file.CopyTo(fileStream);
                }
                result = result + ": After file upload";
                fileStream.Close();
                PDFParseAgent PDFParseAgent = new PDFParseAgent(docuSignSetting);
                //DocusignDocuments docusignDocuments = new DocusignDocuments();

                //var agentid = Convert.ToInt32(Request.Form["agentid"].ToString());                

                //docusignDocuments.AgentId = agentid;
                //docusignDocuments.FileName = filename;
                ////var resultData = PDFParseAgent.InserDocuSignFile(docusignDocuments);

                string result1 = PDFParseAgent.PDFTOXML(fullpath);
                result = result + ": XML Data:" + result1;
            }
            catch (Exception err)
            {
                string error = err.Message + err.StackTrace;
                result = "Error code:" + error;
            }
            return Ok(result);
        }
    }
}
