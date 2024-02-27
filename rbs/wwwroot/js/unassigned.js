var agentType;
var filename = "";
var accountId = 0;
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;

}

$(document).ready(function () {
    accountId = getParameterByName("aid");

    ChcekCookie();
    setAgentType();
    $("#spanClockinout").load("/clockinout.html");
    LoadUnAssignedAccount();
    if (accountId > 0 && accountId != "") {
        LoadAccountNotes();
    }
    else {
        $("#btnSaveFile").hide();
    }

    $("#btnSaveNote").click(function () {
           
        var _DocId = getParameterByName("did");
        if (_DocId == "") {
            UploadFileManualy();
            SaveNoteFile();
            return true;
        }
        else {
            SaveNote();
            //var aid = getParameterByName("aid");
            //if (aid > 0) {
            //    SaveNote();
            //}            
            //UploadFileManualy();   
            //SaveNoteFile();
        }

    });

    $("#btnSaveFile").click(function () {
        SaveFile();
    });

});

function LoadUnAssignedAccount() {

    $("#btnSearch").hide();
    $("#loader").show();
    $("#spanAccountResults").html("");

    var url = "/api/DocuSign/GetDocuSignAccount/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            BuildAccountTable(result);
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function BuildAccountTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spanAccountResults").html(table);
    $("#btnSearch").show();
    $("#loader").hide();
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th"); //ID
    var td2 = document.createElement("th");//AccountName
    var td3 = document.createElement("th");//File Name
    var td4 = document.createElement("th");// Is Assigned
    var td5 = document.createElement("th");// Action


    var text1 = document.createTextNode("Id");
    var text2 = document.createTextNode("Name");
    var text3 = document.createTextNode("File Name");
    var text4 = document.createTextNode("Assigned");
    var text5 = document.createTextNode("Action");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);

    return tr;
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");

    var _fullname = rowData.firstName + " " + rowData.lastName;
    var _agentType = rowData.agentTypeName;
    var _ViewAccount = "";
    if (rowData.isAssigned > 0) {
        _ViewAccount = '<div class="col-md-12"> <a href="/accountdetails.html?v=' + Math.random() + '&aid=' + rowData.accountId + '">Assigned</a>';
    }
    else {
        _ViewAccount = '<div class="col-md-12">No</div>';
    }
    var _editIdLink = '<div class="col-md-12"> <a href="/assignednotes.html?v=' + Math.random() + '&aid=' + rowData.accountId + '&did=' + rowData.id + '">Edit</a>';

    var text1 = "<span class='snapTitle' style ='display:none'><b>Id:</b></span> " + rowData.id;
    var text2 = "<span class='snapTitle' style ='display:none'><b>Name:</b></span> " + _fullname;
    var text3 = "<span class='snapTitle' style ='display:none'><b>File Name:</b></span> " + rowData.fileName;
    var text4 = "<span class='snapTitle' style ='display:none'><b>Assigned:</b></span> " + _ViewAccount;
    var text5 = _editIdLink;

    td1.innerHTML = text1;
    td2.innerHTML = text2;
    td3.innerHTML = text3;
    td4.innerHTML = text4;
    td5.innerHTML = text5;

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);

    return tr;
}

function LoadAccountNotes() {

    var aid = getParameterByName("aid");
    if (aid == "0") {
        //accountId = $('#DDLAccount option:selected').val();
        accountId = $('#DDLAccount').val();
    }
    var url = "/api/Account/GetNoteByAccountId/" + accountId;

    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            BuildNotesTable(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildNoteHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildNoteRow(data[i]));
    }

    $("#spanAccountNotes").html(table);
    $("#loading").hide();

}

function BuildNoteRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    //var shortDate = rowData.insertDate.split("T");
    var shortDate = DateFormat(rowData.insertDate);
    //var text1 = document.createTextNode(shortDate[0] + " " + shortDate[1].replace('Z', ''));
    var text1 = document.createTextNode(shortDate);
    var noteText = rowData.noteText;
    if (rowData.fileName != null && rowData.fileName != "") {        
        noteText = "<i class='fa fa-paperclip' style='font-size:20px' aria-hidden='true'></i>";  
        noteText += rowData.noteText;
        noteText += "  <a href='api/Notes/FetchFile/" + rowData.fileName + "'>" + rowData.fileName + "</a>";
    }
    var text3 = document.createTextNode(rowData.agentName);

    td1.appendChild(text1);
    td2.innerHTML = noteText;
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function BuildNoteHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    var text1 = document.createTextNode("DateTime");
    var text2 = document.createTextNode("Note");
    var text3 = document.createTextNode("Agent");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function GetAccountDocument() {
    var documentid = getParameterByName("did");
    var url = "/api/Account/GetAccountDocumentById/" + documentid;
    $("#btnDownload").html("");
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (result, status, xhr) {

            if (result != null) {
                var str = "";

                //var fname = result.fileName;
                //$("#hidden").val(result.fileName);
                //var matches = fname.match(/\/([^\/?#]+)[^\/]*$/);
                //if (matches != null) {
                //    filename = matches[1];
                //    $("#hidden").val(result.fileName);
                //}
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');

                if (disposition) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches !== null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }
                var linkelem = document.createElement('a');
                try {
                    var blob = new Blob([result], { type: 'application/octet-stream' });

                    if (typeof window.navigator.msSaveBlob !== 'undefined') {
                        //   IE workaround for "HTML7007: One or more blob URLs were revoked by closing the blob for which they were created. These URLs will no longer resolve as the data backing the URL has been freed."
                        window.navigator.msSaveBlob(blob, filename);
                    } else {
                        var URL = window.URL || window.webkitURL;
                        var downloadUrl = URL.createObjectURL(blob);

                        // str = '<h4>' + filename + ' <a href="' + downloadUrl + '" class="btn btn-primary" target=_blank><i class="fa fa-eye" aria-hidden="true"></i> View </a></h4>';
                        if (filename) {
                            // use HTML5 a[download] attribute to specify filename
                            var a = document.createElement("a");

                            // safari doesn't support this yet
                            if (typeof a.download === 'undefined') {
                                window.location = downloadUrl;
                            } else {
                                a.href = downloadUrl;
                                a.download = filename;
                                document.body.appendChild(a);
                                a.target = "_blank";
                                a.click();
                            }
                        } else {
                            window.location = downloadUrl;
                        }
                    }

                } catch (ex) {
                    console.log(ex);

                }


                $("#btnDownload").html(str);
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function SaveNote() {
    $("#msgFileFail").hide();
    $("#msgFileSuccess").hide();
    SaveNoteFile();
    $("#loading").show();
    $("#btnSaveNote").hide();
    var _DocId = getParameterByName("did");
    //var _accountid = $('#DDLAccount option:selected').val();
    var _accountid = $('#DDLAccount').val();

    var _agentId = getCookie("agentId");

    var url = "/api/Notes/";
    var noteToSave = {
        "accountid": _accountid,
        "leadid": 0,
        "agentid": _agentId,
        "notetext": $("#txtNoteText").val()
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteToSave),
        success: function (result) {
            //accountId = $('#DDLAccount option:selected').val();
            accountId = $('#DDLAccount').val();
            var aid = getParameterByName("aid");
            //if (aid > 0) {
            //    LoadAccountNotes();
            //}
            LoadAccountNotes();
            $("#DDLAccount").prop('disabled', true);
            //$('#DDLAccount option[value=0]').attr("selected", "selected");
            $("#txtNoteText").val("");            
            $("#loading").hide();
            $("#btnSaveNote").show();
            $("#divHistory").show();            
            if (_DocId == "") {
                $("#btnSaveFile").hide();
            }
            else {
                $("#btnSaveFile").show();
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

//function LoadAccount() {
//    $("#divNotesHeader").hide();
//    $("#loading").show();
//    var url = "/api/Account/GetAllAccountsName";
//    console.log(url);
//    $.ajax({
//        method: "GET",
//        contentType: "application/json",
//        url: url,
//        success: function (result) {

//            var DDLAgents = document.getElementById("DDLAccount");
//            $("#DDLAgents option").remove();
//            DDLAgents.innerHTML = DDLAgents.innerHTML +
//                '<option value="-1">--Select Account---</option>';

//            var Accountdata = "";
//            for (var i = 0; i < result.length; i++) {
//                Accountdata = Accountdata + 
//                    '<option value="' + result[i].accountId + '">' + result[i].firstName + ' ' + result[i].lastName + '</option>';
//            }
//            DDLAgents.innerHTML += Accountdata;

//            var _aid = getParameterByName("aid");
//            if (_aid > 0) {
//                $('#DDLAccount option[value=' + _aid + ']').attr("selected", "selected");
//                $("#DDLAccount").prop('disabled', true);
//            }
//            $("#loading").hide();
//            $("#divNotesHeader").show();
//        },
//        Error: function (error) {
//            console.log(JSON.stringify(error));
//            $("#loading").hide();
//            $("#divNotesHeader").show();
//        }
//    });
//}

function SaveNoteFile() {
    var _DocId = getParameterByName("did");
    var filename = "";
  
    var filename = $("#hdnfilename").val();

    var _agentId = getCookie("agentId");
    var url = "/api/DocuSign/AssignedNote";

    //var _DocId = getParameterByName("did");
    //var _accountid = $('#DDLAccount option:selected').val();
    var _accountid = $('#DDLAccount').val();

    var docusignDocuments = {
        "accountid": _accountid,
        "filename": filename,
        "agentid": _agentId,
        "id": _DocId
    };
    $("#btnSaveFile").hide();
    //$("#saveFileLoading").show();
    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(docusignDocuments),
        success: function (result) {
            if (result == "Success") {
                $("#msgSuccess").show();
                $("#msgFail").hide();
                $("#uploadfile").val('');
            }
            else if (result == "Fail") {
                $("#msg").html("Fail To File Upload.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
            else {
                //error       
                $("#msg").html("Error! Something is wrong.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
            
            if (_DocId == "") {
                $("#btnSaveFile").hide();
            }
            else {
                $("#btnSaveFile").show();
            }

            //$("#saveFileLoading").hide();

        },
        error: function (result) {            
            if (_DocId == "") {
                $("#btnSaveFile").hide();
            }
            else {
                $("#btnSaveFile").show();
            }
        }
    });
}

function UploadFileManualy() {
    var _DocId = getParameterByName("did");
    var _agentId = getCookie("agentId");
    var url = "/api/DocuSign/AssignedNoteManuallly";
    //accountId = $('#DDLAccount option:selected').val();
    accountId = $('#DDLAccount').val();
    //var _accountid = $('#DDLAccount option:selected').val();
    var _accountid = $('#DDLAccount').val();
    var formData = new FormData();
    formData.append("file", $("#uploadfile")[0].files[0]);
    formData.append("agentid", _agentId);
    formData.append("accountid", _accountid);
    formData.append("noteText", $("#txtNoteText").val());


    $("#btnSaveFile").hide();
    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result == "Success") {
                $("#msgSuccess").show();
                $("#msgFail").hide();
                var aid = getParameterByName("aid");
                if (aid > 0) {
                    LoadAccountNotes();
                }
            }
            else if (result == "Fail") {
                $("#msg").html("Fail To File Upload.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
            else if (result == "Exist") {
                $("#msg").html("User Already Exist.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
            else {
                //error       
                $("#msg").html("Error! Something is wrong.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
           
            if (_DocId == "") {
                $("#btnSaveFile").hide();
            }
            else {
                $("#btnSaveFile").show();
            }
            
            //$("#saveFileLoading").hide();

        },
        error: function (result) {            
            if (_DocId == "") {
                $("#btnSaveFile").hide();
            }
            else {
                $("#btnSaveFile").show();
            }          
        }
    });
}

function GetDocumentFileById() {

    var documentid = getParameterByName("did");
    var url = "/api/Account/GetDocumentFileById/" + documentid;

    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {

            if (result != null) {
                var fname = result.fileName;
                $("#hidden").val(result.fileName);
                $("#lblfilename").html(result.fileName);
                $("#lblfilename").show();

            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function SaveFile() {
    
    var _DocId = getParameterByName("did");
    $("#msgFileFail").hide();
    $("#msgFileSuccess").hide();
    $("#msgFail").hide();
    $("#msgSuccess").hide();
    var furl = $("#uploadfile")[0].files[0];
    if (furl === undefined || furl == null || furl == "") {
        $("#msgFile").html("Please attached file first.");
        $("#msgFileFail").show();
        return false;
    }

    var url = "/api/Notes/NoteFileUpload/";
    var _leadid = -1;

    //var _accountid = $('#DDLAccount option:selected').val();
    var _accountid = $('#DDLAccount').val();
    if (_accountid == -1) {
        $("#msgFile").html("Please select Account.");
        $("#msgFileFail").show();
        return false;
    }
    //var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _leadid = (_leadid) ? _leadid : -1;
    _accountid = (_accountid) ? _accountid : -1;
    _agentId = (_agentId) ? _agentId : -1;

    var formData = new FormData();
    formData.append("file", $("#uploadfile")[0].files[0]);
    formData.append("leadid", _leadid);
    formData.append("accountid", _accountid);
    formData.append("agentid", _agentId);

    $("#btnSaveFile").hide();
    $("#saveFileLoading").show();

    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false,  // tell jQuery not to process the data
        contentType: false,  // tell jQuery not to set contentType
        success: function (result) {
            if (result === "") {

                $("#msgFileSuccess").hide();
                $("#msgFileFail").show();
                $("#msgFail").html("File Upload Fail");
                
                if (_DocId == "") {
                    $("#btnSaveFile").hide();
                }
                else {
                    $("#btnSaveFile").show();
                }
                $("#saveFileLoading").hide();
            }
            else {
                $("#uploadfile").val('');
                $("#msgFileSuccess").show();
                $("#msgFileFail").hide();                
                if (_DocId == "") {
                    $("#btnSaveFile").hide();
                }
                else {
                    $("#btnSaveFile").show();
                }
                $("#saveFileLoading").hide();
                var aid = getParameterByName("aid");
                if (aid > 0) {
                    LoadAccountNotes();
                }
            }
        },
        error: function (result) {
            $("#msgFileSuccess").hide();
            $("#msgFileFail").show();
            $("#msgFail").html("File Upload Fail");
        }
    });
}

function validateFileType() {
    $("#msgFile").html("Allowed only .pdf file.");
    $("#msgFileFail").hide();
    var fileName = document.getElementById("uploadfile").value;
    var idxDot = fileName.lastIndexOf(".") + 1;
    var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
    if (extFile == "pdf") {
        return true;
    } else {
        $("#msgFile").html("Allowed only .pdf file.");
        $("#msgFileFail").show();
        return false;
    }
}