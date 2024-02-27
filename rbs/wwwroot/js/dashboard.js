$(document).ready(function () {
    ChcekCookie();
    checkAgent();

    var currentDate = new Date();
    var date = new Date(currentDate);
    var newdate = new Date(date);

    newdate.setDate(newdate.getDate());

    var dd = newdate.getDate();
    var mm = newdate.getMonth() + 1;
    var y = newdate.getFullYear();

    $("#spanClockinout").load("/clockinout.html");

    var _date = mm + '/' + dd + '/' + y;

    $("#txtStartDate").datetimepicker({
        timepicker:false,
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

    GenerateDashboard();

    $("#btnDashboard").click(function () {
        $("#msg").html("");
        $("#errorMessage").hide();

        if ($("#txtStartDate").val() === "" ||
            $("#txtEndDate").val() === "") {

            $("#msg").html("Select start and end date first.");
            $("#errorMessage").show();
        }
        else {
            GenerateDashboard();
        }
    });

    GetStats();
});

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

function GenerateDashboard() {
    $("#btnDashboard").hide();
    $("#loader").show();

    $("#spanLeadGenStats").html("");
    $("#spanDialStats").html("");
    $("#spanDialRatio").html("");

    var request = new Object();
    var fromdate = $("#txtStartDate").val() + ' ' + '00:00:00';
    var todate = $("#txtEndDate").val() + ' ' + '23:59:59';

    request.FromDateTime = fromdate;
    request.ToDateTime = todate;
    request.Day = "yesterday";

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: "/api/AdminDashboard/",
        dataType: "json",
        data: JSON.stringify(request),
        success: function (result) {
            console.log("Dashboard : " + JSON.stringify(result));
            if (result.leadGenStatsList != null && result.leadGenStatsList.length > 0) {
                $("#spanLeadGenStats").html(BuildLeadGenStatsTable(result.leadGenStatsList));
            } else {
                $("#spanLeadGenStats").html("No data for these dates");
            }
            if (result.dialAgentStatsList != null && result.dialAgentStatsList.length > 0) {
                $("#spanDialStats").html(BuildDialStatsTable(result.dialAgentStatsList));
            } else {
                $("#spanDialStats").html("No data for these dates");
            }
            if (result.dialRatioList != null && result.dialRatioList.length > 0) {
                $("#spanDialRatio").html(BuildDialRatioTable(result.dialRatioList));
            } else {
                $("#spanDialRatio").html("No data for these dates");
            }
            if (result.leadRatioList != null && result.leadRatioList.length > 0) {
                $("#spanLeadRatio").html(BuildLeadRatioTable(result.leadRatioList));
            } else {
                $("#spanLeadRatio").html("No data for these dates");
            }
            if (result.saToInstall != null && result.saToInstall.length > 0) {
                $("#spanSaToInstall").html(BuildSaToInstallTable(result.saToInstall));
            } else {
                $("#spanSaToInstall").html("No data for these dates");
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        },
        complete: function (jqXHR, status) {
            $("#btnDashboard").show();
            $("#loader").hide();
        }
    });
}
