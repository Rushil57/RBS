$(document).ready(function () {
    checkAgent();
    LoadAgents("IS");


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
        format: 'm/d/Y',
        step: 30
    });

    $("#txtStartDate").val(_date);

    $("#txtEndDate").datetimepicker({
        timepicker: false,
        format: 'm/d/Y',
        step: 30
    });
    $("#txtEndDate").val(_date);

    $("#DLLSearchType").change(function () {
        if ($("#DLLSearchType").val() === "0") {
            LoadAgents("IS");
        }

        if ($("#DLLSearchType").val() === "1" ||
            $("#DLLSearchType").val() === "2") {
            LoadAgents("Manager");
        }
    });

    $("#btnSearch").click(function () {
        $("#msg").html("");
        $("#errorMessage").hide();

        if ($("#txtStartDate").val() === "" ||
            $("#txtEndDate").val() === "") {

            $("#msg").html("Select start and end date first.");
            $("#errorMessage").show();
        }
        else {

            GenerateDailPerDayReport();
        }
    });


});

function GenerateDailPerDayReport() {
    $("#btnSearch").hide();
    $("#loader").show();
    $("#spanClockinout").load("/clockinout.html");

    $("#spanTotalDialPerDayResults").html("");
    $("#reportheader").hide();
    $("#header").html("");

    var fromdate = $("#txtStartDate").val() + ' ' + '00:00:00';
    var todate = $("#txtEndDate").val() + ' ' + '23:59:59';
    var report = new Object();
    report.FromDate = fromdate;
    report.ToDate = todate;
    report.AgentId = $("#DDLAgents").val();
    if ($("#DLLSearchType").val() === "0") {
        report.ReportType = "Dail";
    }
    if ($("#DLLSearchType").val() === "1") {
        report.ReportType = "AppointmentSet";
    }
    if ($("#DLLSearchType").val() === "2") {
        report.ReportType = "AppointmentDispatch";
    }
    var url = "/api/InternalSalesStats/";

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(report),
        success: function (result) {
            console.log("Report : " + JSON.stringify(result));
            $("#reportheader").show();


            if ($("#DLLSearchType").val() === "0") {
                $("#header").html("Total Dials Report");
            }
            if ($("#DLLSearchType").val() === "1") {
                $("#header").html("Total Appointment Set Report");
            }
            if ($("#DLLSearchType").val() === "2") {
                $("#header").html("Total Appointment Confirmed & Dispatched Report");
            }

            BuildTotalDialPerDayTable(result);

        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildTotalDialPerDayTable(data) {
    var table = document.createElement("table");

    table.appendChild(BuildTotalDialPerDayHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildTotalDialPerDayRow(data[i]));
    }

    $("#spanTotalDialPerDayResults").html(table);

    $("#btnSearch").show();
    $("#loader").hide();

}

function BuildTotalDialPerDayHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");

    var text1 = document.createTextNode("Date");
    var text2 = document.createTextNode("Agent");
    var text3 = document.createTextNode("Total");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function BuildTotalDialPerDayRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    //schedapptdate

    var text1 = document.createTextNode(FormatShortDateTime(rowData.insertDate.split("T")[0]).split(' ')[0] === "1/1/1" ? "" : FormatShortDateTime(rowData.insertDate.split("T")[0]).split(' ')[0]);
    if (rowData.FirstName === "Total") {
        text1 = "";
    }
    var text2 = document.createTextNode(rowData.firstName);
    var text3;

    if ($("#DLLSearchType").val() === "0") {
        text3 = document.createTextNode(rowData.totalDailPerDay);
    } else if ($("#DLLSearchType").val() === "1") {
        text3 = document.createTextNode(rowData.totalAppointmentSet);
    } else if ($("#DLLSearchType").val() === "2") {
        text3 = document.createTextNode(rowData.totalAppointmentsDispatch);
    }

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function LoadAgents(agentTypeIds) {
    $.ajax({
        url: "/api/Agent/" + agentTypeIds,
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            var DDLAgents = document.getElementById("DDLAgents");
            $("#DDLAgents option").remove();
            DDLAgents.innerHTML = DDLAgents.innerHTML +
                '<option value="0">All</option>';

            for (var i = 0; i < data.length; i++) {
                DDLAgents.innerHTML = DDLAgents.innerHTML +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + '</option>';
            }
            console.log("dropdown loaded.");
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}