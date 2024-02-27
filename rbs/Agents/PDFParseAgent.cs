using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using HtmlAgilityPack;
using iTextSharp.text.pdf.parser;
using Microsoft.Extensions.Options;
using plweb.Internals;
using SautinSoft;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class PDFParseAgent
{
    private readonly IOptions<DocuSignSDK> docuSignSetting;

    public PDFParseAgent(IOptions<DocuSignSDK> docuSignSetting = null)
    {
        if (docuSignSetting != null)
        {
            this.docuSignSetting = docuSignSetting;
        }
    }

    public EnvelopeDocumentsResult ListEnvelopeDocuments(string accountId, EnvelopesInformation envelopes)
    {
        EnvelopeDocumentsResult docsList = null;
        try
        {
            EnvelopesApi envelopesApi = new EnvelopesApi();
            for (int i = 0; i < envelopes.Envelopes.Count; i++)
            {
                // get Document list by envelopId
                docsList = envelopesApi.ListDocuments(accountId, envelopes.Envelopes[i].EnvelopeId);
                DownloadEnvelopeDocuments(accountId, docsList);
            }

            // print the JSON response
            // Console.WriteLine("EnvelopeDocumentsResult:\n{0}", JsonConvert.SerializeObject(docsList));
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("PDFParseAgent.ListEnvelopeDocuments PLWEB.ERROR", error);
            return null;
        }
        return docsList;
    }

    public bool DownloadEnvelopeDocuments(string accountId, EnvelopeDocumentsResult docsList)
    {
        var success = false;
        try
        {
            EnvelopesApi envelopesApi = new EnvelopesApi();
            string filePath = string.Empty;
            FileStream fs = null;
            var accountAgent = new AccountAgent();
            if (docsList != null)
            {
                for (int i = 0; i < docsList.EnvelopeDocuments.Count; i++)
                {
                    // GetDocument() API call returns a MemoryStream
                    MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, docsList.EnvelopeId, docsList.EnvelopeDocuments[i].DocumentId);
                    // let's save the document to local file system
                    string fileName = System.IO.Path.GetRandomFileName() + ".pdf";
                    filePath = System.IO.Path.GetTempPath() + fileName;
                    fs = new FileStream(filePath, FileMode.Create);
                    docStream.Seek(0, SeekOrigin.Begin);
                    docStream.CopyTo(fs);
                    fs.Close();
                    PDFParseAgent PDFParseAgent = new PDFParseAgent(docuSignSetting);
                    DocusignDocuments docusignDocuments = new DocusignDocuments();

                    docusignDocuments.AgentId = 0;
                    docusignDocuments.FileName = fileName;
                    var resultData = PDFParseAgent.InserDocuSignFile(docusignDocuments);

                    var account = PDFParseAgent.DownloadPDF(resultData);
                    var _accountId = account.AccountId = accountAgent.DoesTheAccountExist(account);
                    if (_accountId == -1)
                    {
                        //create account
                        var accountNote = new AccountNotes()
                        {
                            Account = account
                        };
                        accountAgent.CreateAccount(accountNote);
                    }
                    //either way
                    //attach file-note
                    var fileNote = new Note()
                    {
                        LeadId = (account != null) ? account.LeadId : -1,
                        AccountId = _accountId,
                        AgentId = 0,
                        NoteText = $"{fileName} saved!",
                        FileName = fileName
                    };
                    new NotesAgent().SaveNote(fileNote);
                }
            }
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("PDFParseAgent.DownloadEnvelopeDocuments PLWEB.ERROR", error);
        }

        return success;
    }

    public Account DownloadPDF(int DocumentId)
    {
        try
        {
            var account = new Account();

            var xmlString = CreatXMLString(DocumentId).Result;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(xmlString.ToString());

            HtmlNode table = document.DocumentNode.SelectSingleNode("//table");
            HtmlNodeCollection rows = table.SelectNodes("//row");

            for (int i = 0; i < rows.Count - 1; i++)
            {
                if (i == 22)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");
                    try
                    {
                        account.FirstName = rowdata[0].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.FirstName PLWEB.ERROR", error);
                    }
                    try
                    {
                        account.LastName = rowdata[2].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.LastName PLWEB.ERROR", error);
                    }
                }
                if (i == 34)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");
                    try
                    {
                        account.Address = rowdata[0].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.Address PLWEB.ERROR", error);
                    }
                    try
                    {
                        account.City = rowdata[2].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.City PLWEB.ERROR", error);
                    }

                    try
                    {
                        account.State = rowdata[4].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.State PLWEB.ERROR", error);
                    }

                    try
                    {
                        account.Zip = rowdata[6].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.Zip PLWEB.ERROR", error);
                    }

                }
                if (i == 38)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");
                    try
                    {
                        account.Phone1 = rowdata[0].InnerHtml.Replace("-", "").Trim();
                        account.Email = rowdata[2].InnerHtml.Trim();
                    }
                    catch (Exception err)
                    {
                        var error = err.Message + err.StackTrace;
                        Logger.Log("PDFParseAgent.DownloadPDF:account.Email PLWEB.ERROR", error);
                    }

                }
                if (i == 42)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");
                    var phone2 = rowdata[0].InnerHtml;
                    if (rowdata[0].InnerHtml == "")
                    {
                        try
                        {
                            account.Email = rowdata[1].InnerHtml.Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.Email[1] PLWEB.ERROR", error);
                        }
                    }
                    else
                    {
                        try
                        {
                            account.Email = rowdata[2].InnerHtml.Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.Email[2] PLWEB.ERROR", error);
                        }

                    }
                }
                if (i == 52)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");

                    if (rowdata.Count > 4)
                    {
                        try
                        {
                            account.EmerName1 = rowdata[0].InnerHtml.Trim();
                            account.EmerPhone1 = rowdata[4].InnerHtml.Replace("-", "").Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.EmerPhone1[1] PLWEB.ERROR", error);
                        }
                    }
                    if (rowdata.Count > 6)
                    {
                        try
                        {
                            account.EmerPhone1 = rowdata[6].InnerHtml.Replace("-", "").Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.EmerPhone1[2] PLWEB.ERROR", error);
                        }

                    }
                }
                if (i == 56)
                {
                    HtmlNodeCollection rowdata = rows[i].SelectNodes("cell");
                    if (rowdata.Count > 4)
                    {
                        try
                        {
                            account.EmerName2 = rowdata[0].InnerHtml.Trim();
                            account.EmerPhone2 = rowdata[4].InnerHtml.Replace("-", "").Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.EmerPhone2[2] PLWEB.ERROR", error);
                        }
                    }
                    if (rowdata.Count > 6)
                    {
                        try
                        {
                            account.EmerPhone2 = rowdata[6].InnerHtml.Replace("-", "").Trim();
                        }
                        catch (Exception err)
                        {
                            var error = err.Message + err.StackTrace;
                            Logger.Log("PDFParseAgent.DownloadPDF:account.EmerPhone2[3] PLWEB.ERROR", error);
                        }

                    }
                    break;
                }
            }

            return account;
        }
        catch (Exception err)
        {
            var error = err.Message + err.StackTrace;
            Logger.Log("PDFParseAgent.DownloadPDF PLWEB.ERROR", error);
            return null;
        }
    }

    public int InserDocuSignFile(DocusignDocuments docusignDocuments)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.InserDocuSignFile(docusignDocuments);
    }

    public DocusignDocuments GetDosumentsById(int documentid)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetDosumentsById(documentid);
    }

    public bool UpdateDocuSignFile(DocusignDocuments docusignDocuments)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.UpdateDocuSignFile(docusignDocuments);
    }

    public async Task<string> CreatXMLString(int DocumentId)
    {
        string XMLString = "";
        using (var client = new HttpClient())
        {
            string url = docuSignSetting.Value.SiteUrl;
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            DocusignDocuments docusignDocuments = new DocusignDocuments();
            docusignDocuments.Id = DocumentId;
            HttpResponseMessage response = await client.PostAsJsonAsync<DocusignDocuments>("UploadAccountFile", docusignDocuments);
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<string>().Result;
                if (result != null)
                    XMLString = result;
            }
        }
        return XMLString;
    }

    public string PDFTOXML(string fullpath)
    {
        string xmlString = string.Empty;
        try
        {
            PdfFocus ff = new PdfFocus();
            string docxFile = System.IO.Path.ChangeExtension(fullpath, ".xml");
            using (FileStream pdfStream = new FileStream(fullpath, FileMode.Open))
            {
                ff.XmlOptions.ConvertNonTabularDataToSpreadsheet = true;
                //ff.RenderPages = new int[][] { new int[] { 0, 1 }, new int[] { 0, 1 } };
                ff.OpenPdf(pdfStream);
                if (ff.PageCount > 0)
                {
                    using (MemoryStream docxStream = new MemoryStream(ff.ToWord()))
                    {
                        //int result1 = ff.ToXml(docxFile);
                        string result = ff.ToXml(1, 1);
                        xmlString = result;
                    }
                }
            }
            return xmlString;
        }
        catch (Exception err)
        {
            string error = err.Message + err.StackTrace;
            return error;
        }
    }

    public string UploadAccountFile(IFormFile file, string agentId, string noteText)
    {
        var rtnString = string.Empty;
        var fullpath = string.Empty;
        var fileNameNoSpace = file.FileName.Replace(" ", "");
        string filename = Guid.NewGuid().ToString("N").Substring(0, 5) + "_" + fileNameNoSpace;
        try
        {
            string filePath = System.IO.Path.Combine(Util.filefolderpath);
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
            PDFParseAgent PDFParseAgent = new PDFParseAgent(docuSignSetting);
            DocusignDocuments docusignDocuments = new DocusignDocuments();

            var agentid = Convert.ToInt32(agentId);

            docusignDocuments.AgentId = agentid;
            docusignDocuments.FileName = filename;
            var resultData = PDFParseAgent.InserDocuSignFile(docusignDocuments);

            var account = PDFParseAgent.DownloadPDF(resultData);
            if (account != null)
            {
                try
                {
                    var accountAgent = new AccountAgent();
                    account.AgentId = agentid;
                    account.AccountId = accountAgent.DoesTheAccountExist(account);
                    if (account.AccountId > 1)
                    {

                        var noteagents = new NotesAgent();
                        Note note = new Note();
                        note.AccountId = Convert.ToInt32(account.AccountId);
                        note.NoteText = noteText;
                        note.AgentId = agentid;
                        note.LeadId = 0;
                        note.FileName = filename;
                        noteagents.SaveNote(note);
                        DocusignDocuments docusignDocument = new DocusignDocuments();
                        var docData = PDFParseAgent.GetDosumentsById(resultData);
                        docusignDocument = docData;
                        docusignDocument.AccountId = Convert.ToInt32(account.AccountId);

                        PDFParseAgent.UpdateDocuSignFile(docusignDocument);
                        rtnString = "Success";
                        return rtnString;
                    }
                    rtnString = "Cannot match the account";
                }
                catch (Exception err)
                {
                    Logger.Log("DocuSignController.UploadAccountFile:note calling api ", err.Message + err.StackTrace);
                    rtnString = "ERROR!";
                }
            }
            rtnString = "Cannot match the account";
        }
        catch (Exception err)
        {
            Logger.Log("DocuSignController.UploadAccountFile calling api ", err.Message + err.StackTrace);
            rtnString = "ERROR!";
        }

        return rtnString;
    }
}

