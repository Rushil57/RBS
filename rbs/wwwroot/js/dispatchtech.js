/// <reference path="util.js" />
var agentType;
function setAgentType() {
    ChcekCookie();
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;

}

$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");

    if (agentType === "70" || agentType === "60") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }

    LoadTechs();

    $("#txtInstallTime").datetimepicker({
        format: 'm/d/y g:i A',
        step: 30,
        validateOnBlur: false,
        minDate: 0
    });

    $("#btnDispatchTech").click(function () {
        DispatchTech();
    });

    var accountId = getParameterByName("aid");
    if (accountId != null && accountId != "") {
        LoadAccountDetails(accountId);
    }
});

function DispatchTech() {
    $("#successMsg").hide();
    $("#errorMessage").hide();
    
    var techId = $("#DDLTechs").val();
    if (techId === -1) {
        $("#msg").html("Please select a tech from the DDL above!");
        $("#successMsg").hide();
        $("#errorMessage").show();
    } else {
        var lead = new Object();
        lead.LeadId = -99999; //this is a fake leadId for now
        lead.FirstName = $("#txtCustName").val();
        lead.Address = $("#txtAddress").val();
        lead.City = $("#txtCity").val();
        lead.State = $("#txtState").val();
        lead.Phone1 = $("#txtPhone").val();
        lead.SchedApptDate = $("#txtInstallTime").val();
        lead.TechId = techId;


        var account = new Object();
        account.AccountId = getParameterByName("aid");
        if (account.AccountId == "") {
            account.AccountId = -1;
        }
        account.TechId = techId;
        var dispatch = new Object();
        dispatch.SubmittingAgent = getCookie("agentId");
        dispatch.AgentBeingDispatched = techId;
        dispatch.LeadToDispatch = lead;
        dispatch.EquipmentOrder = $("#txtEquipment").val();
        dispatch.AccountToDispatch = account;

        $.ajax({
            method: "POST",
            contentType: "application/json",
            url: "/api/Dispatch/",
            data: JSON.stringify(dispatch),
            success: function (result) {
                console.log(result);
                if (!result) {
                    $("#msg").html("Uh, oh.  Something didn't work.  Better call Spencer");
                    $("#successMsg").hide();
                    $("#errorMessage").show();
                }
                else {
                    if (techId == -99999) { //Tech Schedule ....
                        window.location.replace("/account.html");
                    } else {
                        //redirect to SMS page for that tech
                        window.location.replace("/sms.html?aid=" + techId);
                    }
                }
            },
            Error: function (error) {
                alert(JSON.stringify(error));
                console.log(JSON.stringify(error));
            }
        });
    }
}

function ResetDispatchForm() {
    LoadTechs();
    $("#txtCustName").val("");
    $("#txtAddress").val("");
    $("#txtCity").val("");
    $("#txtState").val("");    
    $("#txtPhone").val("");
    $("#txtInstallTime").val("");
    $("#txtEquipment").val("");
}

function LoadTechs() {
    $.ajax({
        url: "/api/Agent/50", //tech is agentType50
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            agents = data;
            var DDLTechs = document.getElementById("DDLTechs");
            $("#DDLTechs option").remove();
            DDLTechs.innerHTML = DDLTechs.innerHTML +
                '<option value="-1">--Select Tech---</option>';
            var accountId = getParameterByName("aid");
            if (accountId != null && accountId != "") {
                DDLTechs.innerHTML = DDLTechs.innerHTML +
                    '<option value="-99999">***********SCHEDULED************</option>';
            }
            for (var i = 0; i < data.length; i++) {
                DDLTechs.innerHTML = DDLTechs.innerHTML +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
            }
            console.log("dropdown loaded.");
            $("#btnDispatchTech").show();
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function LoadAccountDetails(accountId) {
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/Account/GetAccountById/" + accountId,
        success: function(result) {
            if (result) {
                $("#txtCustName").val(result.firstName + " " + result.lastName);
                $("#txtAddress").val(result.address);
                $("#txtCity").val(result.city);
                $("#txtState").val(result.state);
                $("#txtbState").val(result.bState);
                $("#txtbZip").val(result.bZip);
                $("#txtPhone").val(result.phone1);
            } else {
                $("#loader").hide();
                $("#divDetails").show();
            }
        },
        Error: function(error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#loader").hide();
            $("#divDetails").show();
        }
    });
}