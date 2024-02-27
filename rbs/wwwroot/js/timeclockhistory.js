var agentType;
var userId;

$(document).ready(function () {
    ChcekCookie();
    $("#btnSearch").hide();
    $("#loader").show();

    checkAgent();
    setAgentType();

    $("#spanClockinout").load("/clockinout.html");

    var currentDate = new Date();
    var date = new Date(currentDate);
    var newdate = new Date(date);

    newdate.setDate(newdate.getDate() - 1);

    var dd = newdate.getDate();
    var mm = newdate.getMonth() + 1;
    var y = newdate.getFullYear();

    var _date = mm + '/' + dd + '/' + y;

    $("#txtStartDate").datetimepicker({
        timepicker: false,
        format: 'm/d/y',
        step: 30
    });

    $("#txtStartDate").val(_date);

    $("#txtEndDate").datetimepicker({
        timepicker: false,
        format: 'm/d/y',
        step: 30
    });
    $("#txtEndDate").val(_date);


    var _paydate = mm + '/' + (dd + 1) + '/' + y;

    $("#txtPayDate").datetimepicker({
        timepicker: false,
        format: 'm/d/y',
        step: 30
    });
    $("#txtPayDate").val(_paydate);

    LoadAgents();

    $("#DDLAgents").change(function () {
        if ($("#DDLAgents").val() !== getCookie("agentId")) {
            $("#txtStartDate").val(_date);
            $("#txtStartDate").val(_date);
        }
    });

    $("#btnSearch").click(function () {
        $("#msg").html("");
        $("#errorMessage").hide();
        $("#spanTimeclockHistory").html("");

        if ($("#txtStartDate").val() === "" ||
            $("#txtEndDate").val() === "") {

            $("#msg").html("Select start and end date first.");
            $("#errorMessage").show();
        }
        else {
            GetHistory(0);
        }
    });


    //$("#btnPayReport").click(function () {

    //    $("#msg").html("");
    //    $("#errorMessage").hide();
    //    $("#spanTimeclockHistory").html("");

    //    if ($("#txtStartDate").val() === "" ||
    //        $("#txtEndDate").val() === "") {

    //        $("#msg").html("Select start and end date first.");
    //        $("#errorMessage").show();
    //    }
    //    else {
    //        GetPayReport(0);
    //    }
    //});

    if (agentType === "70") {
        $("#btnPayReport").show();
        $("#btnPayReport").click(function () {

        });
        $("#divMaxDollarPerHour").show();
    } else {
        $("#divMaxDollarPerHour").hide();
        $("#btnPayReport").hide();
    }

    CurrentStaus();
});

function CurrentStaus() {
    var isClockedIn = getCookie("IsClockedIn");

    if (isClockedIn === null) {
        $("#currentStatus").html("You are currently <b>CLOCKED OUT</b>");
    }
    else {
        $("#currentStatus").html("You are currently <b>CLOCKED IN</b>");
    }
}

function RunPayReport() {
    var payReportRequest = new Object();
    //build obj

    var jsonPayReportRequest = JSON.stringify(payReportRequest);
    $.ajax({
        url: "/api/Agent/GetPayReport",
        type: "PUT",
        dataType: "json",
        data: jsonPayReportRequest,
        success: function (data, textStatus, xhr) {

        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}

function LoadAgents() {
    $.ajax({
        url: "/api/Agent",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            var DDLAgents = document.getElementById("DDLAgents");
            $("#DDLAgents option").remove();
            DDLAgents.innerHTML = DDLAgents.innerHTML + '<option value="14" selected>Kaitlyn Collins</option>';
            DDLAgents.innerHTML = DDLAgents.innerHTML + '<option value="112" selected>Justin Sena</option>';
            for (var i = 0; i < data.length; i++) {
                if (data[i].agentTypeId == 30 || data[i].agentTypeId == 40 || data[i].agentTypeId == 60) {
                    DDLAgents.innerHTML = DDLAgents.innerHTML +
                        '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
                }
            }

            if (agentType === "60" || agentType === "70") {
                $("#DDLAgents").val(getCookie("agentId"));
                $("#DDLAgents").prop('disabled', false);

            } else {
                $("#DDLAgents").val(getCookie("agentId"));
                $("#DDLAgents").prop('disabled', true);
            }

            $("#divLoggedIn").show();
            $("#btnSearch").show();
            $("#loader").hide();
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function GetHistory(isDefault) {
    $("#divLoggedIn").hide();

    var history = new Object();
    if (isDefault === 1) {
        var date = new Date();
        var firstDay = (date.getMonth() + 1) + '/' + (date.getDate(), 1) + '/' + date.getFullYear();
        $("#txtStartDate").val(firstDay);
        $("#txtEndDate").val(date.getMonth() + 1 + '/' + date.getDate() + '/' + date.getFullYear());
    }

    history.StartDate = $("#txtStartDate").val();
    history.EndDate = $("#txtEndDate").val();
    history.AgentId = $("#DDLAgents").val();

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: "/api/Timeclock/",
        data: JSON.stringify(history),
        success: function (result) {
            console.log("Report : " + JSON.stringify(result));
            BuildHistoryTable(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildHistoryTable(data) {
    var table = document.createElement("table");

    table.appendChild(BuildHistoryHeader());
    var totalHoursWorked = 0;

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildHistoryRow(data[i]));
        if (data[i].hrsWorked != null && data[i].hrsWorked != "") {
            totalHoursWorked += TimeToDecimal(data[i].hrsWorked);
        }

    }

    table.appendChild(BuildHistoryTotalRow(totalHoursWorked));

    $("#spanTimeclockHistory").html(table);

    $("#divLoggedIn").show();
    $("#btnSearch").show();
    $("#loader").hide();

}

function BuildHistoryTotalRow(totalHoursWorked) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");

    var text1 = document.createTextNode("");
    var text2 = document.createTextNode("");
    var text3 = document.createTextNode("Total");
    var text4 = document.createTextNode(Math.round(totalHoursWorked * 100) / 100);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);

    return tr;
}

function BuildHistoryHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");

    var text1 = document.createTextNode("Agent");
    var text2 = document.createTextNode("Clocked In");
    var text3 = document.createTextNode("Clocked Out");
    var text4 = document.createTextNode("Hours Worked");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);

    return tr;
}

function BuildHistoryRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");

    if (rowData.clockedOutAt.split("T")[1] === "23:59:59") {
        $(td4).css("background-color", "#ffec80");
    }

    var text1 = document.createTextNode(rowData.firstName);

    var text2 = "";
    if (rowData.clockedInAt !== null) {
        var shortDate = DateFormat(rowData.clockedInAt);
        text2 = document.createTextNode(shortDate);

    }
    var text3 = ""
    if (rowData.clockedOutAt !== null) {
        var shortDate = DateFormat(rowData.clockedOutAt);
        text3 = document.createTextNode(shortDate);

    }

    //var text2 = document.createTextNode(rowData.clockedInAt === null ? "" : FormatShortDateTime(rowData.clockedInAt.split("T")[0]).split(' ')[0] + ' ' + timeConvert(rowData.clockedInAt.split("T")[1].split(":")[0] + ":" + rowData.clockedInAt.split("T")[1].split(":")[1]));
    //var text3 = document.createTextNode(FormatShortDateTime(rowData.clockedOutAt.split("T")[0]).split(' ')[0] === "1/1/1" ? "Clocked In" : rowData.clockedOutAt.split("T")[1] === "00:00:00" ? "" : FormatShortDateTime(rowData.clockedOutAt.split("T")[0]).split(' ')[0] + ' ' + timeConvert(rowData.clockedOutAt.split("T")[1].split(":")[0] + ":" + rowData.clockedOutAt.split("T")[1].split(":")[1]));
    var _hrsWorked = " - ";
    if (rowData.hrsWorked != null && rowData.hrsWorked != "") {
        _hrsWorked = TimeToDecimal(rowData.hrsWorked);
    }
    var text4 = document.createTextNode(_hrsWorked);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);

    return tr;
}

function GetPayReport(isDefault) {
    $("#divLoggedIn").hide();

    var history = new Object();
    if (isDefault === 1) {
        var date = new Date();
        var firstDay = (date.getMonth() + 1) + '/' + (date.getDate(), 1) + '/' + date.getFullYear();
        $("#txtStartDate").val(firstDay);
        $("#txtEndDate").val(date.getMonth() + 1 + '/' + date.getDate() + '/' + date.getFullYear());

        var Paydate = new Date();
        //var startDay = (Paydate.getMonth() + 1) + '/' + (Paydate.getDate(), 0) + '/' + Paydate.getFullYear();      
        $("#txtPayDate").val(Paydate.getMonth() + 1 + '/' + Paydate.getDate() + '/' + Paydate.getFullYear());
    }
    if ($("#txtPayDate").val() != "") {
        history.PayDate = $("#txtPayDate").val();
    }
    history.StartDate = $("#txtStartDate").val();
    history.EndDate = $("#txtEndDate").val();
    history.AgentId = $("#DDLAgents").val();
    history.MaxDollarPerHour = $('input[name=chkMaxDollarPerHour]').is(':checked');

    //history.AgentId = 1;

    $.ajax({
        method: "Post",
        contentType: "application/json",
        url: "/api/Timeclock/PayReport/",
        data: JSON.stringify(history),
        xhrFields: {
            responseType: 'blob'
        },
        success: function (response, status, xhr) {
            if (response.size === 0) {
                $("#btnPayReport").prop("disabled", false);
                $("#msg").html("No Data Found!");
                $("#errorMessage").show();
                return false;
            }
            $("#errorMessage").hide();
            var filename = "";
            var disposition = xhr.getResponseHeader('Content-Disposition');

            if (disposition) {
                var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                var matches = filenameRegex.exec(disposition);
                if (matches !== null && matches[1]) filename = matches[1].replace(/['"]/g, '');
            }
            var linkelem = document.createElement('a');
            try {
                var blob = new Blob([response], { type: 'application/octet-stream' });

                if (typeof window.navigator.msSaveBlob !== 'undefined') {
                    //   IE workaround for "HTML7007: One or more blob URLs were revoked by closing the blob for which they were created. These URLs will no longer resolve as the data backing the URL has been freed."
                    window.navigator.msSaveBlob(blob, filename);
                } else {
                    var URL = window.URL || window.webkitURL;
                    var downloadUrl = URL.createObjectURL(blob);

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
                $("#btnPayReport").prop("disabled", false);
            }
            $("#btnPayReport").prop("disabled", false);
        },
        Error: function (error) {
            //alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
            $("#btnPayReport").prop("disabled", false);
            $("#msg").html("No Data Found!");
            $("#errorMessage").show();
        }
    });
}
