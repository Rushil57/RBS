/// <reference path="util.js" />


$(document).ready(function () {
    var isClockedIn = getCookie("IsClockedIn");

    if (isClockedIn === null) {
        $("#btnClockIn").show();
        $("#btnClockOut").hide();
    }
    else {
        $("#btnClockOut").show();
        $("#btnClockIn").hide();
    }

    $("#btnClockIn").click(function (e) {
        ClockInOut(); //IN
    });

    $("#btnClockOut").click(function (e) {
        ClockInOut(); //OUT
    });
});

function ClockInOut() {
    var action = new Object();
    action.AgentId = getCookie("agentId");


    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: "/api/Timeclock/Timeclock/",
        dataType: "json",
        data: JSON.stringify(action),
        success: function (result) {
            console.log(result);
            
            if (result.status === "Clocked Out") {
                $("#btnClockOut").hide();
                $("#btnClockIn").show();
                eraseCookie("IsClockedIn");
            }
            else {
                $("#btnClockIn").hide();
                $("#btnClockOut").show();
                setCookie("IsClockedIn", "1", 365 * 2);
            }

            
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}