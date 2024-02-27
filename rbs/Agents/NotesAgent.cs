using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

public class NotesAgent
{
    public List<Note> GetNotes(Note NoteRequest)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetNotes(NoteRequest);
    }

    public bool SaveNote(Note NoteToSave)
    {
        var dataAgent = new MySqlDataAgent();

        //last ditch to make sure that there is a leadid and accountid
        if (NoteToSave.LeadId == -1
            && NoteToSave.AccountId > 0)
        {
            var leadid = dataAgent.GetLeadIdByAccountId(NoteToSave.AccountId.Value);
            if (leadid > 1)
            {
                NoteToSave.LeadId = leadid;
            }
        }
        else if (NoteToSave.AccountId == -1
            && NoteToSave.LeadId > 0)
        {
            var accountid = dataAgent.GetAccountIdByLeadId(NoteToSave.LeadId.Value);
            if (accountid > 1)
            {
                NoteToSave.AccountId = accountid;
            }
        }

        return dataAgent.InsertNote(NoteToSave);
    }

    public bool AddAccountIdToNotes(int leadId, int accountId)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.AddAccountIdToNotes(leadId, accountId);
    }

    public string SaveFileAsNote(IFormFile file, int leadId, int accountId, int agentId)
    {
        var fileNameNoSpace = file.FileName.Replace(" ", "");
        string filename = Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
        try
        {
            string filePath = Path.Combine(Util.filefolderpath);
            string fullpath = filePath + filename;
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(fullpath, FileMode.Create))
                {
                    file.CopyToAsync(fileStream).Wait();
                }
            }

            var fileNote = new Note()
            {
                LeadId = leadId,
                AccountId = accountId,
                AgentId = agentId,
                NoteText = $"{filename} saved!",
                FileName = filename
            };
            SaveNote(fileNote);
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            filename = string.Empty;
        }

        return filename;
    }

    public bool ImportNoteFile(string filePath, int agentid)
    {
        //string filePath = @"E:\import-data\Prolink Customer Notes 2014 - Sheet1.tsv";
        string Pending = "";
        bool result = false;
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                int counter = 0;
                int i = 0;
                StringBuilder str = new StringBuilder();
                int accountIdCheck = 0;
                Note NoteToCheck = null;
                while (!reader.EndOfStream)
                {
                    result = true;
                    try
                    {
                        var fields = reader.ReadLine().Split('\t');

                        var rtn = $@"<b>AccountId:</b> {fields[0]}<br />
                                    <b>NoteType:</b> {fields[1]}<br />
                                    <b>NoteText:</b> {fields[2]}<br />
                                    <b>Created By:</b> {fields[3]}<br />
                                    <b>Created By UserId:</b> {fields[4]}<br />
                                    <b>DateEntered:</b> {fields[5]}<br />";

                        if (i != 0)
                        {
                            int accountId = Convert.ToInt32(fields[0]);
                            Note newAccount = null;

                            if (accountIdCheck != accountId)
                            {
                                newAccount = new MySqlDataAgent().GetNotesByOldAccountId(accountId);
                            }
                            else
                            {
                                if (NoteToCheck != null)
                                {
                                    newAccount = NoteToCheck;
                                }
                            }

                            if (newAccount != null)
                            {
                                accountIdCheck = accountId;
                                NoteToCheck = newAccount;

                                try
                                {
                                    //str.Append(" AccountNo:" + newAccount.AccountId + " Note Text: " + fields[3].ToString());
                                    var note = new Note()
                                    {
                                        LeadId = -1,
                                        AccountId = newAccount.AccountId,
                                        AgentId = agentid,
                                        NoteText = $"***Orion Import**** <br/>{rtn}",
                                        LeadStatusId = 0
                                    };
                                    //submit note
                                    new NotesAgent().SaveNote(note);
                                    counter++;
                                }
                                catch (Exception e)
                                {
                                    result = false;
                                    Console.WriteLine($"ERROR! - Old AccountID {fields[0]} {e.Message} {e.StackTrace}");
                                    Logger.Log("ImportNoteFile", $"ERROR! - Old AccountID {fields[0]} {e.Message} {e.StackTrace}");
                                }
                            }
                            else
                            {
                                Pending += $"\n FileName:{filePath}, AccountNote! - Counter: {counter} - Old Account Id: {fields[0]} \n";
                                Logger.Log("ORIONIMPORT", $"\n FileName:{filePath}, AccountNote! - Counter: {counter} - Old Account Id: {fields[0]} \n");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        Pending += $"\n FileName:{filePath}, AccountNote! - Counter: {counter} - Error: {ex.Message} \n";
                        Logger.Log("ORIONIMPORT", $"\n FileName:{filePath}, AccountNote! - Counter: {counter} - Error: {ex.Message}  \n");
                    }                
                    i++;
                }
                if (!string.IsNullOrEmpty(Pending))
                {
                    string errorfilePath = Path.Combine(Util.filefolderpath) + "AccountNoteError.txt";
                    if (!File.Exists(errorfilePath))
                    {
                        File.Create(errorfilePath);
                    }
                    File.AppendAllText(errorfilePath, Pending + Environment.NewLine);
                }
                string data = str.ToString();
                result = true;
            }
        }
        catch (Exception ex)
        {
            result = false;
            Logger.Log("ImportNoteFile", $"ERROR! - {ex.Message} {ex.StackTrace}");
        }
        return result;
    }
}
