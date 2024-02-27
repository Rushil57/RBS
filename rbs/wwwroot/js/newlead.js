var agentType;
var currentLeadId = 0;
var leadid;
var rawCount = -1;
var isRawLead;

$(document).ready(function () {
    ChcekCookie();

    String.prototype.splice = function (idx, rem, str) {
        return this.slice(0, idx) + str + this.slice(idx + Math.abs(rem));
    };

    HideButtons();
    $("#spanClockinout").load("/clockinout.html");
    checkAgent();
    setAgentType();

    if (agentType !== "10") {
        var dummyagent = getCookie("changeAgent");
        if (dummyagent !== null) {
            agentType = dummyagent;
        }
    }

    $("#copyAddress").click(function (e) {
        e.preventDefault();
        CopyToClipboard(("address"));
    });


    $("#pastePhone1").click(function (e) {
        e.preventDefault();
        pasteText("#phone1");
    });
    $("#pastePhone2").click(function (e) {
        e.preventDefault();
        pasteText("#phone2");
    });
    $("#pastePhone3").click(function (e) {
        e.preventDefault();
        pasteText("#phone3");
    });
    $("#pastePhone4").click(function (e) {
        e.preventDefault();
        pasteText("#phone4");
    });
    $("#pastePhone5").click(function (e) {
        e.preventDefault();
        pasteText("#phone5");
    });

    $("#phone1").mask("999-999-9999");
    $("#phone2").mask("999-999-9999");
    $("#phone3").mask("999-999-9999");
    $("#phone4").mask("999-999-9999");
    $("#phone5").mask("999-999-9999");

    leadid = getParameterByName("lid");
    $("#county").prop('disabled', true);

    if (leadid === "") {
        if (agentType === "10") {

            $("#save").hide();
            //if agenttype == 10 then this is always a leadtype = 1
            //so this doesn't need to be shown
            $("#divRowLeadType").hide();
            //$("#address").prop('disabled', true);
            $("#address").prop('readonly', true);
            $("#address").css('background-color', "#ebebe4");
            $("#copyAddress").show();
            $("#state").prop('disabled', true);

            loadLead(-99999);
            TotalRawCount();

        }
        else {
            ShowButtons();
            $("#changeAgent").show();
            $("#save").show();
            if (agentType === "11") {
                $("#divRowLeadType").hide();
            }
            else {
                $("#divRowLeadType").show();
            }
            $("#DDLLeadType").show();
            $("#savenext").hide();
            $("#addnote").hide();
            //$("#address").prop('disabled', false);
            $("#address").prop('readonly', false);
            $("#address").css('background-color', "white");
            $("#copyAddress").hide();
            $("#state").prop('disabled', false);

            $("#solddate").datetimepicker({
                timepicker: false,
                format: 'm/d/Y',
                maxDate: 0
            });
            if (agentType === "70") {
                TotalRawCount();
            }
        }
    }
    else {
        loadLead(leadid);
        $("#address").prop('readonly', false);
        $("#address").css('background-color', "white");
        $("#copyAddress").hide();


    }
    $("#save").click(function (e) {
        e.preventDefault();
        HideButtons();
        $("#msg").html("");
        leadid = getParameterByName("lid");
        if (leadid === "") {
            createNewlead();
        } else {
            updateLead();
        }
    });

    $("#addnote").click(function (e) {
        e.preventDefault();
        $("#addnote").hide();
        $("#loadernote").show();
        SaveAndLoadNote(currentLeadId);
    });

    $("#savenext").click(function (e) {
        e.preventDefault();
        HideButtons();
        updateLead();
    });

    $("#changeAgent").click(function (e) {
        e.preventDefault();
        setCookie("changeAgent", 10, 1);
        window.location.replace("/newlead.html");
    });
});
function paste() {
    var pasteText = document.querySelector("#phone1");
    pasteText.focus();
    document.execCommand("paste");
    console.log(pasteText.textContent);
}
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null) {
        agentType = agent.AgentTypeId;
    }
    else {
        window.location.replace("/index.html");
    }
}

function loadLead(leadid) {
    isRawLead = false;

    $('#notfound').html("");

    $('#save').hide();
    $.ajax({
        url: "/api/Lead/" + leadid,
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        success: function (data, textStatus, xhr) {
            console.log(JSON.stringify(data));
            clearForm();
            if (typeof (data) !== "undefined" && data !== null) {
                currentLeadId = data.leadId;
                $("#lblCurrentLeadId").html("Current LeadId : " + data.leadId);
                $("#solddate").val(data.soldDate.split('T')[0] === "0001-01-01" ? "" : data.soldDate.split('T')[0]);
                $("#firstname").val(data.firstName);
                $("#lastname").val(data.lastName);
                $("#address").val(data.address);
                $("#email").val(data.email);
                $("#city").val(data.city);
                $("#phone1").val(data.phone1 !== "" ? data.phone1.splice(3, 0, "-").splice(7, 0, "-") : "");
                $("#phone2").val(data.phone2 !== "" ? data.phone2.splice(3, 0, "-").splice(7, 0, "-") : "");
                $("#phone3").val(data.phone3 !== "" ? data.phone3.splice(3, 0, "-").splice(7, 0, "-") : "");
                $("#phone4").val(data.phone4 !== "" ? data.phone4.splice(3, 0, "-").splice(7, 0, "-") : "");
                $("#phone5").val(data.phone5 !== "" ? data.phone5.splice(3, 0, "-").splice(7, 0, "-") : "");
                $("#postcode").val(data.postal);
                $("#county").val(data.county);
                $("#state").val(data.state);
                $("#divCounty").show();
                $("#DDLLeadStatus").val(data.leadStatusId === 0 ? 1 : data.leadStatusId);
                console.log("data.leadtypeid :" + data.leadtypeid);
                $("#DDLLeadType").val(data.leadTypeId === 0 ? 1 : data.leadTypeId);

                if (data.leadStatusId === 0) {
                    $("#lookupdiv").show();
                }
                else {
                    $("#lookupdiv").hide();
                }
                // check if raw file in update mode.
                if (agentType === "10") {
                    if (data.leadStatusId === 0) {
                        isRawLead = true;
                    } else {
                        isRawLead = false;
                    }
                }
                LoadNotes(data.leadId);
                $("#divNotes").show();
                $('.row').show(); //What does this do?  I am not sure why we are calling this it undoes what we tried below
                //if agenttype == 10 then this is always a leadtype = 1
                //so this doesn't need to be shown
                $("#divRowLeadType").hide();
                ShowButtons();
            } else {
                if (leadid !== "") {
                    $('#msg').html("Lead record not found.");
                    $("#successMsg").hide();
                    $("#errorMessage").show();
                    $('.row').hide();
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
            alert(errorThrown);
        }
    });
}

function createNewlead() {

    if (agentType === "11") {
        $("#successMsg").hide();
        $("#errorMessage").hide();
        var strmsg = "";

        if ($("#solddate").val() == null || $("#solddate").val() == "") {
            strmsg = "Enter Sold date.</br>";
        }
        if ($("#address").val() == null || $("#address").val() == "") {
            strmsg += "Enter Address.</br>";
        }
        if ($("#state").val() == null || $("#state").val() == "") {
            strmsg += "Enter State.</br>";
        }
        if ($("#city").val() == null || $("#city").val() == "") {
            strmsg += "Enter City.</br>";
        }
        if ($("#postcode").val() == null || $("#postcode").val() == "") {
            strmsg += "Enter Pastcode.</br>";
        }
        if ($("#homevalue").val() == null || $("#homevalue").val() == "") {
            strmsg += "Enter Home value.";
        }
        if (strmsg != "") {
            $("#msg").html(strmsg);
            $("#errorMessage").show();
            $("#save").show();
            $("#loader").hide();
            return false;
        }
    }

    $("#msg").html("");
    $("#successMsg").hide();
    $("#errorMessage").hide();

    var newLead = new Object();
    var leadUpdateRequest = new Object();

    leadUpdateRequest.PageRequestingUpdate = "createlead";

    $("#msg").html("");
    $("#successMsg").hide();
    $("#errorMessage").hide();

    newLead.SoldDate = $("#solddate").val() === "" ? "0001/01/01" : $("#solddate").val();

    newLead.FirstName = $("#firstname").val().trim();
    newLead.LastName = $("#lastname").val().trim();
    newLead.Address = $("#address").val().trim();
    newLead.Email = $("#email").val().trim();
    newLead.City = $("#city").val().trim();
    newLead.State = $("#state").val().trim();
    newLead.Phone1 = $("#phone1").val() === null ? null : $("#phone1").val().replace("-", "");
    newLead.Phone2 = $("#phone2").val() === null ? null : $("#phone2").val().replace("-", "");
    newLead.Phone3 = $("#phone3").val() === null ? null : $("#phone3").val().replace("-", "");
    newLead.Phone4 = $("#phone4").val() === null ? null : $("#phone4").val().replace("-", "");
    newLead.Phone5 = $("#phone5").val() === null ? null : $("#phone5").val().replace("-", "");
    newLead.Postal = $("#postcode").val().trim();
    newLead.State = $("#state").val().trim();

    if (agentType === "11") {
        newLead.LeadStatusId = parseInt(0);
        newLead.LeadTypeId = parseInt(1);
        newLead.County = "Pima";
        newLead.HomeValue = $("#homevalue").val();
    }
    else {
        newLead.LeadStatusId = $("#DDLLeadStatus").val();
        newLead.LeadTypeId = parseInt($("#DDLLeadType").val());
    }
    newLead.AgentIdSubmitting = getCookie("agentId");
    leadUpdateRequest.LeadToUpdate = newLead;

    var noteToSave = new Object();
    noteToSave.NoteText = $("#notes").val().trim();
    leadUpdateRequest.NoteToSave = noteToSave;

    $.ajax({
        url: "/api/Lead/UpdateLead",
        type: "POST",
        dataType: "json",
        data: JSON.stringify(leadUpdateRequest),
        contentType: "application/json",
        success: function (data, textStatus, xhr) {
            console.log("Save process : " + data);
            if (data === false) {
                $("#msg").html("An error occured while saving Lead.");
                $("#successMsg").hide();
                $("#errorMessage").show();
                ShowButtons();
            }
            else {
                if (data === "IsExist") {
                    $("#msg").html("Address already exist!!!");
                    $("#errorMessage").show();
                    $("#loader").hide();
                    $("#save").show();
                    ShowButtons();
                }
                else {
                    $("#successMsg").show();
                    $("#errorMessage").hide();
                    $("#loader").hide();
                    $("#save").show();
                    clearForm();
                    ShowButtons();
                    console.log("Saved Successfully");
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#errorMessage").show();
            $("#loader").hide();
            $("#save").show();
            $("#msg").html("An error occured while saving Lead.");
            console.log("Error in Operation");
        }
    });
}

function updateLead() {
    $("#msg").html("");
    $("#successMsg").hide();
    $("#errorMessage").hide();

    var newLead = new Object();
    var leadUpdateRequest = new Object();
    if (isRawLead === true) {
        leadUpdateRequest.PageRequestingUpdate = "updaterawlead";
    } else {
        leadUpdateRequest.PageRequestingUpdate = "updatelead";
    }

    $("#msg").html("");

    newLead.LeadId = currentLeadId;
    newLead.SoldDate = $("#solddate").val() === "" ? "0001/01/01" : $("#solddate").val();
    newLead.FirstName = $("#firstname").val();
    newLead.LastName = $("#lastname").val();
    newLead.Address = $("#address").val();
    newLead.Email = $("#email").val();
    newLead.City = $("#city").val();
    newLead.State = $("#state").val();
    newLead.Phone1 = $("#phone1").val() === null ? null : $("#phone1").val().replace("-", "");
    newLead.Phone2 = $("#phone2").val() === null ? null : $("#phone2").val().replace("-", "");
    newLead.Phone3 = $("#phone3").val() === null ? null : $("#phone3").val().replace("-", "");
    newLead.Phone4 = $("#phone4").val() === null ? null : $("#phone4").val().replace("-", "");
    newLead.Phone5 = $("#phone5").val() === null ? null : $("#phone5").val().replace("-", "");
    newLead.Postal = $("#postcode").val();
    newLead.LeadStatusId = $("#DDLLeadStatus").val();
    newLead.AgentIdSubmitting = getCookie("agentId");
    leadUpdateRequest.LeadToUpdate = newLead;

    var noteToSave = new Object();
    noteToSave.NoteText = $("#notes").val();
    leadUpdateRequest.NoteToSave = noteToSave;

    console.log(JSON.stringify(leadUpdateRequest));
    $.ajax({
        url: "/api/Lead/UpdateLead",
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(leadUpdateRequest),
        success: function (data, textStatus, xhr) {
            console.log("Save process : " + data);
            if (data === false) {
                $("#msg").html("An error occured while saving Lead");
                $("#successMsg").hide();
                $("#errorMessage").show();
                ShowButtons();
            }
            else {
                if (isRawLead === true) {
                    loadLead(-99999);
                } else {
                    $("#successMsg").show();
                    $("#errorMessage").hide();
                    ShowButtons();
                    console.log("Saved Successfully");
                }
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('Error in Operation');
        }
    });

    if (agentType === "10") {
        if (isRawLead === true) {
            --rawCount;
            TotalRawCount();
        }
    }
}

function clearForm() {
    $("#firstname").val("");
    $("#lastname").val("");
    $("#address").val("");
    $("#email").val("");
    $("#city").val("");
    $("#phone1").val("");
    $("#phone2").val("");
    $("#phone3").val("");
    $("#phone4").val("");
    $("#phone5").val("");
    $("#postcode").val("");
    $("#notes").val("");
    if (agentType === "11") {
        $("#state").val("AZ");
    } else {
        $("#state").val("");
    }

    $("#solddate").val("");
    $("#DDLLeadStatus").val(1);
    $("#DDLLeadType").val(1);
    $("#homevalue").val("");
}

function LoadNotes(d) {
    var _leadid = d;
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _accountid = (_accountid) ? _accountid : -1;

    var url = "/api/Notes/";
    var noteRequest = {
        "leadid": _leadid,
        "accountid": _accountid,
        "agentid": _agentId
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

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spanPastNotes").html(table);

    $("#addnote").show();
    $("#loadernote").hide();
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    var shortDate = rowData.insertDate.split("T");
    var text1 = document.createTextNode(shortDate[0] + " " + shortDate[1].replace('Z', ''));
    var text3 = document.createTextNode(rowData.agentName);

    td1.appendChild(text1);
    td2.innerHTML = rowData.noteText;
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

function SaveAndLoadNote(_leadid) {
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _accountid = (_accountid) ? _accountid : -1;

    var noteToSave = {
        "accountid": _accountid,
        "leadid": _leadid,
        "agentid": _agentId,
        "notetext": $("#notes").val()
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: "/api/Notes/",
        data: JSON.stringify(noteToSave),
        success: function (result) {
            console.log("Save note :" + result);
            LoadNotes(_leadid);
            $("#notes").val("");
            console.log(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function TotalRawCount() {
    if (rawCount > 0) {
        $("#rawCount").html("Total Raw : " + rawCount);
    }
    else if (rawCount == 0) {
        $("#msg").html("There is no other record to update, but you can now ADD NEW LEAD.");
        $("#successMsg").hide();
        $("#errorMessage").show();
        $("#save").show();
        //if agenttype == 10 then this is always a leadtype = 1
        //so this doesn't need to be shown
        $("#divRowLeadType").hide();
        $("#savenext").hide();
    }
    else { //-1 means it just hasn't loaded yet

        $.ajax({
            url: "/api/RawCount/",
            type: "GET",
            dataType: "json",

            success: function (data, textStatus, xhr) {
                rawCount = data.tx + data.az;
                $("#rawCountAZ").html("Total Raw AZ : " + data.az);
                $("#rawCountTX").html("Total Raw TX : " + data.tx);
                ShowButtons();
            },

            error: function (xhr, textStatus, errorThrown) {
                console.log("Error in Operation");
            }
        });
    }
}

function ShowButtons() {
    $("#loader").hide();
    $("#loadernote").hide();
    $("#addnote").show();
    if (agentType === "10") {
        $("#save").hide();
        $("#savenext").show();
    }
    else if (agentType === "11") {
        $("#divNote").hide();
        $("#divFirstName").hide();
        $("#divLastName").hide();
        $("#divemail").hide();
        $("#divphone1Block").hide();
        $("#divphone2Block").hide();
        $("#divphone3Block").hide();
        $("#divphone4Block").hide();
        $("#divphone5Block").hide();
        $("#divRowLeadType").css('display', 'none !important');
        $("#divRowLeadStatus").hide();
        $("#DDLLeadType").hide();
        $("#changeToAgentType").hide();
        $("#address").css('width', '100%');
        $("#divHomeValue").show();
        $("#state").val("AZ");
    }
    else {
        $("#save").show();
        $("#savenext").hide();
        $("#addnote").hide();
    }
}

function HideButtons() {
    $("#loader").show();
    $("#loadernote").show();
    $("#addnote").hide();
    $("#save").hide();
    $("#savenext").hide();
}


//WIP?
function CopyPhoneNumber(phone) {
    $("$phone1").val(phone);
}

function isNumber(evt) {
    var iKeyCode = (evt.which) ? evt.which : evt.keyCode
    if (iKeyCode != 46 && iKeyCode > 31 && (iKeyCode < 48 || iKeyCode > 57))
        return false;
    else
        return true;
} 