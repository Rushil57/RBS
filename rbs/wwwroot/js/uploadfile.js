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

    $("#btnSaveFile").click(function () {
        SaveFile();
    });

    $("#btnTempSaveFile").click(function () {
        SaveTempFile();
    });
});

function SaveFile() {
    $("#msgSuccess").hide();
    $("#msgFail").hide();
    var _agentId = getCookie("agentId");
    var url = "/api/DocuSign/UploadAccountFile/";
    var furl = $("#uploadfile")[0].files[0];

    if (furl === undefined || furl == null || furl == "") {
        $("#msg").html("Please attached file first.");
        $("#msgFail").show();
        return false;
    }
    var formData = new FormData();
    formData.append("file", $("#uploadfile")[0].files[0]);
    formData.append("agentid", _agentId);
    formData.append("noteText", $("#txtNoteText").val());

    $("#btnSaveFile").hide();
    $("#saveFileLoading").show();
    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false,  // tell jQuery not to process the data
        contentType: false,  // tell jQuery not to set contentType
        success: function (result) {
            if (result == "Success") {
                $("#msgSuccess").show();
                $("#msgFail").hide();
                $("#txtNoteText").val('');
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
                $("#txtNoteText").val('');
            }
            else {
                //error       
                $("#msg").html("Error! Something is wrong.");
                $("#msgSuccess").hide();
                $("#msgFail").show();
            }
            $("#btnSaveFile").show();
            $("#saveFileLoading").hide();
            $("#uploadfile").val('');

        },
        error: function (result) {
            $("#btnSaveFile").show();
            $("#saveFileLoading").hide();
        }
    });
}

function validateFileType() {
    $("#msg").html("Allowed only .pdf file.");
    $("#msgFail").hide();
    var fileName = document.getElementById("uploadfile").value;
    var idxDot = fileName.lastIndexOf(".") + 1;
    var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
    if (extFile == "pdf") {
        return true;
    } else {
        $("#msg").html("Allowed only .pdf file.");
        $("#msgFail").show();
        return false;
    }
}


/////Temp

function SaveTempFile() {
    $("#msgSuccess").hide();
    $("#msgFail").hide();
    var _agentId = getCookie("agentId");
    var url = "/api/DocuSign/UploadAccountFileTemp/";
    var furl = $("#uploadfile1")[0].files[0];

    if (furl === undefined || furl == null || furl == "") {
        $("#msg").html("Please attached file first.");
        $("#msgFail").show();
        return false;
    }
    var formData = new FormData();
    formData.append("file", $("#uploadfile1")[0].files[0]);
    formData.append("agentid", _agentId);

    
    $("#saveFileLoading").show();
    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false,  // tell jQuery not to process the data
        contentType: false,  // tell jQuery not to set contentType
        success: function (result) {
          
            $("#msgFail").show();
            $("#msg").html("Success:" + result);
            //if (result == "Success") {
            //    $("#msgSuccess").show();
            //    $("#msgFail").hide();               
            //}
            //else if (result == "Fail") {
            //    $("#msg").html("Fail To File Upload.");
            //    $("#msgSuccess").hide();
            //    $("#msgFail").show();
            //}
            //else if (result == "Exist") {
            //    $("#msg").html("User Already Exist.");
            //    $("#msgSuccess").hide();
            //    $("#msgFail").show();

            //}
            //else {
            //    //error       
            //    $("#msg").html("Error! Something is wrong.");
            //    $("#msgSuccess").hide();
            //    $("#msgFail").show();
            //}
            $("#btnSaveFile").show();
            $("#saveFileLoading").hide();
            $("#uploadfile1").val('');

        },
        error: function (result) {
            $("#msgFail").show();
            $("#msg").html("Error:" + result);
            $("#btnSaveFile").show();
            $("#saveFileLoading").hide();
        }
    });
}