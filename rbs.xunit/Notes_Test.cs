using System;
using System.IO;
using System.Text;
using Xunit;

namespace plweb.xunit
{
    public class Notes_Test
    {
        [Fact]
        public void GetFilesandUpload()
        {
            try
            {
                var FolderPath = @"D:\Images";
                DirectoryInfo dirInfo = new DirectoryInfo(FolderPath);
                FileInfo[] Files = dirInfo.GetFiles();

                foreach (FileInfo file in Files)
                {
                    if (!string.IsNullOrEmpty(file.Name) && file.Name.Contains("_"))
                    {
                        var FileName = file.Name.Replace(" ", "");
                        var strOldAccountId = FileName.Split('_')[0];

                        int intOldAccountId = 0;
                        int.TryParse(strOldAccountId, out intOldAccountId);
                        if (intOldAccountId > 0)
                        {
                            var newAccount = new MySqlDataAgent().GetNotesByOldAccountId(intOldAccountId);
                            var NewFileName = string.Format("{0}_{1}_{2}", newAccount.AccountId, Guid.NewGuid().ToString("N").Substring(0, 5), FileName);

                            try
                            {
                                string filePath = Path.Combine(Util.filefolderpath);
                                if (!Directory.Exists(filePath))
                                {
                                    Directory.CreateDirectory(filePath);
                                }
                                var fullpath = filePath + NewFileName;
                                if (file.Length > 0)
                                {
                                    file.CopyTo(fullpath, true);
                                }
                            }
                            catch (Exception err)
                            {
                                Logger.Log("DocuSignController.SaveAccountNote calling api ", err.Message + err.StackTrace);
                            }
                            var note = new Note()
                            {
                                LeadId = -1,
                                AccountId = newAccount.AccountId,
                                AgentId = 0,
                                NoteText = NewFileName,
                                FileName = NewFileName,
                                LeadStatusId = 0
                            };
                            new NotesAgent().SaveNote(note);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("DocuSignController.SaveAccountNote calling api ", ex.Message + ex.StackTrace);
            }
        }

        [Fact]
        public void ImportNoteFile()
        {
            string filePath = @"C:\Users\spenc\Downloads\notes_2014.tsv";
            string Pending = "";
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
                                        var note = new Note()
                                        {
                                            LeadId = -1,
                                            AccountId = newAccount.AccountId,
                                            AgentId = 0,
                                            NoteText = $"***Orion Import**** <br/>{rtn}",
                                            LeadStatusId = 0
                                        };
                                        //submit note
                                        new NotesAgent().SaveNote(note);

                                        counter++;

                                    }
                                    catch (Exception e)
                                    {
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
                }
            }
            catch (Exception ex)
            {
                Logger.Log("ImportNoteFile", $"ERROR! - {ex.Message} {ex.StackTrace}");
            }
        }        
    }
}
