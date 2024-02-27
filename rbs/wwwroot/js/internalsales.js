var agentData;
var aId = 0;
var Lid;
var agentType;

$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");
    $("#btnSearch").hide();
    $("#loader").show();
});

function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}

window.onload = function () {
    checkAgent();
    setAgentType();

    if (agentType === "70" || agentType === "60" || agentType === "30" || agentType === "11") {
        LoadLeads();
    } else {
        $("#loader").hide();
        $("#btnSearch").hide();
        $("#ddlSearchField").hide();
        $("#txtSearchTerm").hide();
        $("#lblFilterBy").hide();
        $("#lblSearch").hide();
        DoSearch("agentfollowup", getCookie("agentId"));
    }

    $("#btnSearch").click(function () {
        $("#btnSearch").hide();
        $("#loader").show();
        var _searchfield = $("#ddlSearchField").find(":selected").val();
        var _searchterm = $("#txtSearchTerm").val();
        DoSearch(_searchfield, _searchterm);
    });

    $("#btnlucky").click(function () {
        GetRandomLead();
    });

    $("#btnluckyTO").click(function () {
        GetRandomTOLead();
    });

    $("#txtSearchTerm").keypress(function (event) {
        if (event.keyCode === 13) {
            $("#btnSearch").click();
            event.preventDefault();
            return false;
        }
    });
}

function GetRandomLead() {
    var url = "/api/Lead/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            console.log("random leadId:" + JSON.stringify(result));
            window.location.replace("/leaddial.html?lid=" + result.leadId);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function GetRandomTOLead() {
    var url = "/api/Lead/GetRandomTOLead/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            console.log("random TO leadId:" + JSON.stringify(result));
            window.location.replace("/leaddial.html?lid=" + result.leadId);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function LoadLeads() {
    if (agentType === "11") {
        $("#btnSearch").show();
        $("#btnlucky").hide();
        $("#ddlSearchField").val("address");
        $("#ddlSearchField").prop('disabled', true);
    }
    else {
        $("#btnSearch").show();
        $("#ddlSearchField").show();
        $("#txtSearchTerm").show();
    }
    DoSearch("", "");
}

function DoSearch(searchfield, searchterm) {
    $("#spanLeadsResults").html("");
    var url = "/api/Lead/";
    var searchRequest = {
        "searchterm": searchterm,
        "searchfield": searchfield
    };

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(searchRequest),
        success: function (result) {
            if (result != null && result != "") {
                BuildLeadsTable(result);
            } else {
                $("#loader").hide();
            }
            console.log("sss:" + JSON.stringify(result));
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildLeadsTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spanLeadsResults").html(table);

    $("#btnSearch").show();
    $("#loader").hide();

    if (agentType === "70" || agentType === "30" || agentType === "60") {
        GetStats();
    }
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");

    var _address = rowData.address + ", " + rowData.city;
    var _leadfullname = rowData.firstName + " " + rowData.lastName;

    var _leadIdLink = "";
    if (agentType === "11") {
        _leadIdLink = rowData.leadId;
    } else {
        _leadIdLink = '<a href="/leaddial.html?lid=' + rowData.leadId + '">' + rowData.leadId + "</a>";
    }

    // var shortDate = rowData.followUpDate.split("T");
    var text3 = document.createTextNode(_leadfullname);
    var text4 = document.createTextNode(_address);
    var _status = (rowData.statusText === "Follow Up") ? "Follow-up" : "Call";
    _status = (rowData.leadStatusId != null && rowData.leadStatusId == 56) ? "Installed" : _status;
    var text5 = document.createTextNode(_status);

    //var shortDate = DateFormat(rowData.followUpDate);
    //var text6 = document.createTextNode(rowData.statusText === "Follow Up" ? shortDate[0] === "0001-01-01" ? "-" : timeConvert(shortDate[0]) + " " + timeConvert(shortDate[1].split('Z')[0]): "-");
    var text6 = document.createTextNode("-");
    if (rowData.followUpDate.split('T')[0] !== "0001-01-01") {
        var shortDate = DateFormat(rowData.followUpDate);
        text6 = document.createTextNode(rowData.statusText === "Follow Up" ? shortDate : "-");
    }

    td1.innerHTML = _leadIdLink;
    td2.appendChild(text3);
    td3.appendChild(text4);
    td4.appendChild(text5);
    td4.className = "textAlignCenter";
    td5.appendChild(text6);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);

    return tr;
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");

    var text1 = document.createTextNode("LeadId");
    var text3 = document.createTextNode("Name");
    var text4 = document.createTextNode("Address");
    var text5 = document.createTextNode("Status");
    var text6 = document.createTextNode("Followup");

    td1.appendChild(text1);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);

    tr.appendChild(td1);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);

    return tr;
}

function GetStats() {
    $.ajax({
        url: "/api/InternalSalesStats/",
        type: "GET",
        dataType: "json",

        success: function (data, textStatus, xhr) {
            $("#totalfollowup").html("Follow Ups : " + data.totalFollowup);
            $("#totalretry").html("Retrys : " + data.totalRetry);
            $("#totalnewleads").html("New Leads : " + data.totalNewLead);
            $("#toready").html("TO : " + data.toReady);
            $("#divStats").show();
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}
