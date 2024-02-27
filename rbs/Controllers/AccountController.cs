using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace plweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        [HttpPut]
        [Route("GetAllAccounts")]
        public List<Account> GetAllAccounts([FromBody]SearchRequest search)
        {
            try
            {
                var accountAgent = new AccountAgent();
                return accountAgent.GetAllAccounts(search);
            }
            catch (Exception e)
            {
                Logger.Log("AccountController.GetAllAccounts calling api", e.Message + e.StackTrace);
                return null;
            }
        }

        [HttpPost]
        [Route("CreateAccount")]
        public IActionResult CreateAccount(AccountNotes accountNotes)
        {
            bool success = true;
            string result = "";
            try
            {
                AccountAgent accountAgent = new AccountAgent();
                if (accountNotes.Account.AccountId > 0)
                {
                    success = accountAgent.UpdateAccount(accountNotes);
                    result = "Update";
                }
                else
                {
                    success = (accountAgent.CreateAccount(accountNotes) > 0);
                    result = "Create";
                }
            }
            catch (Exception e)
            {
                Logger.Log("AccountController.CreateAccount calling api ", e.Message + e.StackTrace);
                success = false;
            }
            return Ok(result.ToString());
        }

        [HttpGet]
        [Route("GetAccountById/{accountid}")]
        public IActionResult GetAccountById(int accountid)
        {
            var accountAgent = new AccountAgent();
            Account accountData = accountAgent.GetAccountById(accountid);
            return Ok(accountData);
        }

        [HttpGet]
        [Route("GetNoteByAccountId/{accountid}")]
        public IActionResult GetNoteByAccountId(int accountid)
        {
            var dataAgent = new MySqlDataAgent();
            List<Note> note = dataAgent.GetNoteByAccountId(accountid).ToList();
            return Ok(note);
        }
        [HttpPost]
        [Route("SaveAccountNote")]
        public ActionResult SaveAccountNote(IFormFile file)
        {
            string result = "";
            string fullpath = string.Empty;

            string filename = string.Empty;
            var accountId = Convert.ToInt32(Request.Form["accountid"]);
            if (file != null)
            {
                var fileNameNoSpace = file.FileName.Replace(" ", "");
                filename = accountId + "_" + Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
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
                }
                catch (Exception err)
                {
                    Logger.Log("DocuSignController.SaveAccountNote calling api ", err.Message + err.StackTrace);
                    result = "";
                }
            }
            try
            {
                var agentid = Convert.ToInt32(Request.Form["agentid"]);
                var noteText = Convert.ToString(Request.Form["noteText"]);
                var leadid = Convert.ToInt64(Request.Form["leadid"]);

                var pDFParseAgent = new PDFParseAgent();

                var noteagents = new NotesAgent();
                Note note = new Note();
                note.AccountId = accountId;
                note.NoteText = noteText;
                note.AgentId = agentid;
                note.LeadId = leadid;
                note.FileName = filename;
                bool isSuccess = noteagents.SaveNote(note);
                if (isSuccess == true)
                {
                    result = "Success";
                }
            }
            catch (Exception err)
            {
                Logger.Log("DocuSignController.SaveAccountNote calling api ", err.Message + err.StackTrace);
                result = "";
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("GetTechCalendar/")]
        public IActionResult GetTechCalendar()
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(dataAgent.GetTechCalendar());
        }

        // Note: Don't change anything in bellow method becase this method call in file download api.
        [HttpGet]
        [Route("GetAccountDocumentById/{documentid}")]
        public FileResult GetAccountDocumentById(int documentid)
        {
            var dataAgent = new MySqlDataAgent();
            var docusignDocuments = dataAgent.GetDosumentsById(documentid);
            if (docusignDocuments != null)
            {
                var fileUrl = Path.Combine(Util.filefolderpath);
                var filename = docusignDocuments.FileName;
                docusignDocuments.FileName = filename;

                var path = Path.Combine(fileUrl, filename);

                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;
                return File(memory, Util.GetContentType(fileUrl + filename), filename);
            }
            return null;
        }

        [HttpGet]
        [Route("GetAllAccount/")]
        public IActionResult GetAllAccount()
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(dataAgent.GetAllAccount());
        }

        [HttpGet]
        [Route("GetDocumentFileById/{documentid}")]
        public IActionResult GetDocumentFileById(int documentid)
        {
            var dataAgent = new MySqlDataAgent();
            var docusignDocuments = dataAgent.GetDosumentsById(documentid);
            if (docusignDocuments != null)
            {
                return Ok(docusignDocuments);
            }
            return null;
        }

        [HttpGet]
        [Route("GetAllAccountsName/")]
        public IActionResult GetAllAccountsName()
        {
            try
            {
                var dataAgent = new MySqlDataAgent();
                return Ok(dataAgent.GetAllAccountsName());
            }
            catch (Exception e)
            {
                Logger.Log("AccountController.GetAllAccountsName calling api", e.Message + e.StackTrace);
                return null;
            }
        }


        [HttpGet]
        [Route("GetAllAccountInfo/")]
        public async Task<IActionResult> GetAllAccountInfo()
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(await dataAgent.GetAllAccountInfo());
        }

        [HttpPut("GetAccountDashboard")]
        public async Task<IActionResult> GetAccountDashboard(AccountDashboard accountDashboard)
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(await dataAgent.GetAccountDashboard(accountDashboard.StartDate, accountDashboard.EndDate));
        }

        [HttpGet]
        [Route("GetAllState/")]
        public async Task<IActionResult> GetAllState()
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(await dataAgent.GetAllState());
        }

        [HttpGet]
        [Route("GetFieldRep/{agentTypeId}")]
        public IActionResult GetFieldRep(string agentTypeId)
        {
            var dataAgent = new MySqlDataAgent();
            return Ok(dataAgent.GetAgentByType(agentTypeId));
        }        
    }
}
