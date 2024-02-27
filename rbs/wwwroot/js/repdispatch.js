//store the javascript object to send back.
var _leadToDispatch = new Object();

$(document).ready(function () {
    $("#spanClockinout").load("/clockinout.html");

    checkAgent();
    var leadId = getParameterByName("lid");
    LoadLead(leadId);

    $("#txtSchedAppt").datetimepicker({
        format: 'm/d/y h:i A',
        step: 30,
        minDate: 0,
        validateOnBlur: false
    });

    $("#btnSendIt").click(function () {
        DispatchFieldRep();
    });

    $("#btnMarkSold").click(function () {
        UpdateLead(56);
    });

    $("#btnMarkInst").click(function () {
        UpdateLead(57);
    });

    $("#btnMarkUFS").click(function () {
        UpdateLead(58);
    });

    $("#btnMarkNoShow").click(function () {
        UpdateLead(59);
    });

    $("#btnSendToResched").click(function () {
        UpdateLead(60);
    });

    $("#btnSaveSchedNote").click(function () {
        if ($("#txtSANote").val() === "") {
            $("#msg").html("Enter Note First.");
            $("#successSschednotes").hide();
            $("#errorMessageschednotes").show();
        }
        else { SaveSchedNote(leadId); }
    });

    $("#btnMoreControls").click(function () {
        var _lid = getParameterByName("lid");
        window.location.replace("/sadispatch.html?lid=" + _lid);
    });
});

function DispatchFieldRep() {
    var dispatch = new Object();
    dispatch.SubmittingAgent = getCookie("agentId");
    var _agent = $('#DDLAgents option:selected').val();
    if (_agent == -1) {
        alert("Please pick an agent to dispatch!");
    } else {

        dispatch.AgentBeingDispatched = _agent;
        dispatch.LeadToDispatch = _leadToDispatch;
        dispatch.PageRequestingDispatch = "InternalSales";
        dispatch.LeadToDispatch.FieldRepId = $('#DDLAgents option:selected').val();        
        var notes = $("#txtSANote").val();

        if (notes !== null) {
            dispatch.equipmentOrder = notes;
        }
        var schedAppt = $("#txtSchedAppt").val();
        if (schedAppt !== null) {
            dispatch.LeadToDispatch.SchedApptDate = schedAppt;
        }

        $.ajax({
            method: "POST",
            contentType: "application/json",
            url: "/api/Dispatch/",
            data: JSON.stringify(dispatch),
            beforeSend: function () {
                $("#btnSendIt").hide();
                $("#loaderSchedAppt").show();
            },
            success: function (data, textStatus, xhr) {
                console.log("leadDial save: " + data);
                if (data === false) {
                    $("#msg").html("An error occured while saving.");
                    $("#errorMessage").show();
                }
                else {
                    window.location.replace("/internalsales.html");
                }
            },

            error: function (xhr, textStatus, errorThrown) {
                console.log("Error in Operation");
            }
        });
    }
}

function UpdateLead(statusId) {
    $("#msg").html("");
    $("#errorMessage").hide();

    var leadUpdateRequest = new Object();

    var lead = _leadToDispatch;
    lead.LeadStatusId = statusId;

    lead.AgentIdSubmitting = getCookie("agentId");
    if (statusId === 56) {
        $("#btnMarkSold").hide();
        leadUpdateRequest.PageRequestingUpdate = "SetLeadToSold";
    }
    else if (statusId === 57) {
        $("#btnMarkInst").hide();
        leadUpdateRequest.PageRequestingUpdate = "SetLeadToInstalled";
    }
    else if (statusId === 58) {
        $("#btnMarkUFS").hide();
        lead.AgentIdSubmitting = $("#DDLAgents").val();
        leadUpdateRequest.PageRequestingUpdate = "SetLeadToUFS";
    }
    else if (statusId === 59) {
        $("#btnMarkNoShow").hide();
        lead.AgentIdSubmitting = $("#DDLAgents").val();
        leadUpdateRequest.PageRequestingUpdate = "SetLeadToNoShow";
        var schedAppt = $("#txtSchedAppt").val();
        if (schedAppt !== null) {
            lead.SchedApptDate = schedAppt;
        }
    }
    else if (statusId === 60) {
        $("#btnSendToResched").hide();
        leadUpdateRequest.PageRequestingUpdate = "SetLeadToResched";
    }
    var IsCountState = $('input[name=chkIsCountState]').is(':checked')
    if (IsCountState == true) {
        lead.IsCountState = 1;
    }
    else {
        lead.IsCountState = 0;
    }    
    lead.FieldRepId = $('#DDLAgents option:selected').val();    
    
    $("#loadernoanswer").show();
    leadUpdateRequest.LeadToUpdate = lead;


    var jsonleadUpdateRequest = JSON.stringify(leadUpdateRequest);
    console.log(jsonleadUpdateRequest);
    $.ajax({
        url: "/api/Dispatch/",
        type: "PUT",
        dataType: "json",
        contentType: "application/json",
        data: jsonleadUpdateRequest,
        success: function (data, textStatus, xhr) {
            console.log("leadDial save: " + data);
            if (data === false) {
                $("#msg").html("An error occured while saving Lead Dial.");
                $("#errorMessage").show();
            }
            else {
                window.location.replace("/calendar.html");
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('Error in Operation');
            alert(errorThrown);
        }
    });
}

function LoadLead(id) {
    var url = "/api/Dispatch/" + id;
    console.log(url);
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            console.log(result);
            _leadToDispatch = result.leadToDispatch;
            LoadLeadForm(result.leadToDispatch);
            LoadAgents(result.agents);
            BuildNotesTable(result.notes);
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function LoadLeadForm(lead) {
    $("#txtName").val(lead.firstName + " " + lead.lastName);
    $("#txtAddress").val(lead.address + ", " + lead.city);

    //always set at least the first phone ...
    var phoneToCall = lead.phone1;
    if (lead.phone2Status == 3) {
        phoneToCall = lead.phone2;
    }
    else if (lead.phone3Status == 3) {
        phoneToCall = lead.phone3;
    }
    else if (lead.phone4Status == 3) {
        phoneToCall = lead.phone4;
    }
    else if (lead.phone5Status == 3) {
        phoneToCall = lead.phone5;
    }
    $("#txtPhone").val(phoneToCall);
    $("#txtPhone").mask("999-999-9999");

    var shortDate = lead.schedApptDate.split("T");
    var schedDate = shortDate[0] + " " + shortDate[1].replace('Z', '');
    $("#txtSchedAppt").val(schedDate);
}

function LoadAgents(data) {
    var DDLAgents = document.getElementById("DDLAgents");
    $("#DDLAgents option").remove();
    DDLAgents.innerHTML = DDLAgents.innerHTML +
        '<option value="-1">--Select Tech---</option>';
    for (var i = 0; i < data.length; i++) {
        DDLAgents.innerHTML = DDLAgents.innerHTML +
            '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
    }
}

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    var sanote = true;

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
        //will get the first (most recent) only since it is most recent
        //if (sanote && data[i].leadStatusId == 53) {
        //    sanote = false;
        //    $("#txtSANote").val(data[i].noteText);
        //}
    }

    $("#spanNotes").html(table);
    $("#loading").hide();
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    //var shortDate = rowData.insertDate.split("T");
    var shortDate = DateFormat(rowData.insertDate);
    var text1 = document.createTextNode(shortDate);
    var text3 = document.createTextNode(rowData.agentName);

    var noteText = rowData.noteText;
    noteText = noteText.replace(/\n/g, "<br />");
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

function SaveSchedNote(id) {
    $("#loaderSaveNote").show();
    $("#btnSaveSchedNote").hide();

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
        "notetext": $("#txtSANote").val()
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteToSave),
        success: function (result) {
            console.log("note save" + result);
            $("#txtSANote").val("");
            $("#successSschednotes").show();
            $("#errorMessageschednotes").hide();
            $("#loaderSaveNote").hide();
            $("#btnSaveSchedNote").show();
            LoadLead(id);
        },
        Error: function (error) {
            $("#loaderSaveNote").hide();
            $("#btnSaveSchedNote").show();
            //alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}