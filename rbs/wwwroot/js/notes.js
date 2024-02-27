var agentType;
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;

}
$(document).ready(function () {
    ChcekCookie();
    setAgentType();
    $("#spanClockinout").load("/clockinout.html");
    if (agentType === "70" || agentType === "60") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }
    LoadNotes();
    $("#btnSaveNote").click(function () {
        SaveNote();
    });

    $("#btnSaveFile").click(function () {
        SaveFile();
    });

    $("#btnUploadNoteFile").click(function () {
        UploadAccountsNotesFile();
    });
});

function SaveFile() {
    var url = "/api/Notes/NoteFileUpload/";

    var _leadid = getParameterByName("lid");
    var _accountid = getParameterByName("aid");
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
                alert("Error!");
            }
            else {
                $("#btnSaveFile").show();
                $("#saveFileLoading").hide();
                LoadNotes();
            }
        },
        error: function (result) {

        }
    });
}

function LoadNotes() {
    $("#loading").show();
    $("#btnSaveNote").hide();

    var _leadid = getParameterByName("lid");
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _leadid = (_leadid) ? _leadid : -1;
    _accountid = (_accountid) ? _accountid : -1;
    _agentId = (_agentId) ? _agentId : -1;
    $("#spanAgentId").text(_agentId);
    $("#spanLeadId").text(_leadid);
    $("#spanAccountId").text(_accountid);

    if (window.location.pathname === "/leaddial.html") {
        $("#menus").hide();
        $("#divNotesHeader").hide();

    }
    var url = "/api/Notes/";
    var noteRequest = {
        "leadid": _leadid,
        "accountid": _accountid,
        "agentid": getCookie("agentId")
    };

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteRequest),
        success: function (result) {
            BuildNotesTable(result);
            console.log(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function SaveNote() {
    $("#loading").show();
    $("#btnSaveNote").hide();

    var _leadid = getParameterByName("lid");
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _leadid = (_leadid) ? _leadid : -1;
    _accountid = (_accountid) ? _accountid : -1;
    _agentId = (_agentId) ? _agentId : -1;

    var url = "/api/Notes/";
    var noteToSave = {
        "accountid": _accountid,
        "leadid": _leadid,
        "agentid": _agentId,
        "notetext": $("#txtNoteText").val()
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteToSave),
        success: function (result) {
            console.log("note save" + result);
            LoadNotes();
            $("#txtNoteText").val("");
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        if (agentType === "40") {
            if (data[i].leadStatusId == 52 ||
                data[i].leadStatusId == 53) {
                table.appendChild(BuildRow(data[i]));
            }
        } else {
            table.appendChild(BuildRow(data[i]));
        }
    }

    $("#spanPastNotes").html(table);
    $("#loading").hide();
    $("#btnSaveNote").show();
}

function BuildRow(rowData) {
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

function BuildHeader() {
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

function UploadAccountsNotesFile() {
    
    var url = "/api/Notes/UploadAccountsNotesFile/";

    var _agentId = getCookie("agentId");

    var formData = new FormData();
    formData.append("file", $("#uploadfile")[0].files[0]);
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
            if (result === false) {
                $("#msgSuccess").hide();
                $("#msgFail").show();
                $("#msg").html('Fail to upload file!!')
                $("#saveFileLoading").hide();
            }
            else {
                $("#uploadfile").val('');
                $("#msgSuccess").show();
                $("#btnSaveFile").show();
                $("#saveFileLoading").hide();
            }
        },
        error: function (result) {
            $("#msgSuccess").hide();
            $("#msgFail").show();
            $("#msg").html('Something is wrong, try again later!!')
            $("#saveFileLoading").hide();
        }
    });
}

function validateFileType() {
    $("#msg").html("Allowed only .tsv file.");
    $("#msgFail").hide();
    var fileName = document.getElementById("uploadfile").value;
    var idxDot = fileName.lastIndexOf(".") + 1;
    var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
    if (extFile == "tsv") {
        return true;
    } else {
        $("#msg").html("Allowed only .tsv file.");
        $("#msgFail").show();
        return false;
    }
}