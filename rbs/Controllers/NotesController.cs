using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace plweb.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        // POST: api/Notes
        [HttpPost]
        public bool Post([FromBody] Note NoteToSave)
        {
            var notesAgent = new NotesAgent();
            return notesAgent.SaveNote(NoteToSave);
        }

        // PUT: api/Notes
        [HttpPut]
        public List<Note> Put([FromBody]Note NoteRequest)
        {
            var notesAgent = new NotesAgent();
            return notesAgent.GetNotes(NoteRequest);
        }

        [HttpPost("NoteFileUpload/")]
        public ActionResult UploadFile(IFormFile file)
        {
            var filename = string.Empty;
            if (file != null)
            {
                var nagent = new NotesAgent();
                var leadid = Convert.ToInt32(Request.Form["leadid"].ToString());
                var accountid = Convert.ToInt32(Request.Form["accountid"].ToString());
                var agentid = Convert.ToInt32(Request.Form["agentid"].ToString());

                filename = nagent.SaveFileAsNote(file, leadid, accountid, agentid);
            }
            return Ok(filename);
        }

        [HttpGet("FetchFile/{filename}")]
        public ActionResult FetchFile(string filename)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var memory = new MemoryStream();
                if (filename != null)
                {
                    var path = Path.Combine(Util.filefolderpath, filename);
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        stream.CopyTo(memory);
                    }
                    memory.Position = 0;
                    return File(memory, Util.GetContentType(path), filename);
                }
                return File(memory, "application/pdf", "");
            }
            catch (Exception ex)
            {
                Logger.Log("PLWEB.ERROR Timeclock.PayReport ", ex.Message + ex.StackTrace);
                return null;
            }
        }

        [HttpPost("UploadAccountsNotesFile/")]
        public ActionResult UploadAccountsNotesFile(IFormFile file)
        {
            bool result = false;
            if (file != null)
            {
                var fileNameNoSpace = file.FileName.Replace(" ", "");
                string filename = Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
                var agentid = Convert.ToInt32(Request.Form["agentid"].ToString());
                string filePath = Path.Combine(Util.filefolderpath);
                string fullpath = filePath + filename;
                if (file.Length > 0)
                {
                    using (var fileStream = new FileStream(fullpath, FileMode.Create))
                    {
                        file.CopyToAsync(fileStream).Wait();
                    }
                    var noteAgent = new NotesAgent();
                    result = noteAgent.ImportNoteFile(fullpath, agentid);
                }

            }
            return Ok(result);
        }
    }
}