var CurrentDays = null;
$(document).ready(function () {
    //$("#spanClockinout").load("/clockinout.html");
    $("#loader").show();
    loadstats();
    GetCalendarSalesClosedLeads();
    GetAnnouncements();
    //this will load the stats every 60 seconds
    var intervalID_loadstats = window.setInterval(loadstats, 60000);
    var intervalID_GetCalendarSalesClosedLeads = window.setInterval(GetCalendarSalesClosedLeads, 60000);
    var intervalID_GetAnnouncements = window.setInterval(GetAnnouncements, 60000);

    //this will get set on every page load, and will reload the page in 60 mins
    var intervalID_loadentirepage = window.setInterval(loadentirepage, 3600000);
});

function loadentirepage() {
    window.location.reload(true);
}

function loadstats() {
    $.ajax({
        url: "/api/DialAgentStats/",
        type: "GET",
        success: function (result) {
            $("#loader").hide();
            $("#spanDialStats").html(BuildDialStatsTable(result));
        },
        error: function (jqXHR) {
            $("#loader").hide();
            console.log(JSON.stringify(jqXHR));
        },
        complete: function (jqXHR, status) {
            $("#loader").hide();
            console.log(JSON.stringify(jqXHR));
        }
    });

}

function GetCalendarSalesClosedLeads() {
    var url = "/api/Lead/GetCalendarSalesClosedLeads/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            if (result.thisWeek.length > 0) {
                console.log(JSON.stringify(result));
                BuildSaleClosedTable(result.thisWeek);
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildSaleClosedTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        //var dates = new Date(data[i].schedApptDate);
        //var days = dates.getDay();
        //if (days != CurrentDays || CurrentDays == null) {
        //    var table1 = document.createElement("table");
        //    var tr1 = document.createElement("tr");
        //    var td0 = document.createElement("td"); //Days
        //    var d = new Date();
        //    var weekday = new Array(7);
        //    weekday[0] = "Sunday";
        //    weekday[1] = "Monday";
        //    weekday[2] = "Tuesday";
        //    weekday[3] = "Wednesday";
        //    weekday[4] = "Thursday";
        //    weekday[5] = "Friday";
        //    weekday[6] = "Saturday";

        //    var dayName = weekday[days];
        //    CurrentDays = dates.getDay();
        //    td0.colSpan = 5;
        //    td0.innerHTML = dayName;
        //    td0.className = "textAlignCenter";
        //    td0.style.fontWeight = "600";
        //    tr1.appendChild(td0);
        //    table.appendChild(tr1)
        //}       
        table.appendChild(BuildRow(data[i]));
    }
    $("#spanThisWeekResults").html(table);
    $("#loader").hide();
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");

    var text1 = document.createTextNode("LeadId");
    var text2 = document.createTextNode("Rep Name");
    var text3 = document.createTextNode("Lead Name");
    var text4 = document.createTextNode("Phone");
    var text5 = document.createTextNode("Status");


    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    tr.className = "tableHeader2";
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);

    return tr;
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");
    var td1 = document.createElement("td"); //leadid
    var td2 = document.createElement("td"); //RepName
    var td3 = document.createElement("td"); //Lead Name
    var td4 = document.createElement("td"); //Phone
    var td5 = document.createElement("td"); //Status

    var _leadfullname = rowData.firstName + " " + rowData.lastName;
    var text1 = document.createTextNode(rowData.leadId);
    var text2 = document.createTextNode(rowData.dispatchedRepsName);
    var text3 = document.createTextNode(_leadfullname);

    var status = "";
    if (rowData.leadStatusId == 55) {
        status = "Dispatched";
    }
    else if (rowData.leadStatusId == 56) {
        status = "Sold";
    }
    else if (rowData.leadStatusId == 57) {
        status = "Installed";
    }
    else if (rowData.leadStatusId == 58) {
        status = "UFS";
    }
    else if (rowData.leadStatusId == 59) {
        status = "NoShow";
    }
    else if (rowData.leadStatusId == 60) {
        status = "Resched";
    }
    var text4 = document.createTextNode(rowData.phone1);
    var text5 = document.createTextNode(status);

    td1.appendChild(text1);
    td1.width = "50px";

    td2.appendChild(text2);
    td3.appendChild(text3);

    td4.appendChild(text4);
    td4.width = "100px";
    td5.appendChild(text5);
    td5.width = "60px";
    td5.className = "textAlignCenter";

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    tr.appendChild(td4);
    tr.appendChild(td5);

    return tr;
}


function GetAnnouncements() {
    var url = "/api/Announcement/GetAllAnnouncement/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {

            if (result.length > 0) {
                console.log(JSON.stringify(result));
                BuildAnnouncementsTable(result);
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildAnnouncementsTable(data) {

    var table = document.createElement("table");
    //table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        var classname = "";
        if (i % 2 == 0) {
            classname = "tableBodyEven";
        }
        else {
            classname = "tableBodyOdd";
        }
        table.appendChild(BuildAnnouncementsRow(data[i], classname));
    }
    $("#spanAnnouncements").html(table);
    $("#loader").hide();
}

function BuildAnnouncementsRow(rowData, classname) {

    var tr = document.createElement("tr");
    tr.className = classname;
    var td1 = document.createElement("td");

    var dateFrom = DateFormat(rowData.dateFrom);
    var dateTo = DateFormat(rowData.dateTo);

    var str = "<div class='col-md-12'>" + rowData.announceText + "</div>";

    td1.innerHTML = str;
    td1.width = "100%";

    tr.appendChild(td1);

    return tr;
}

var IsSetInterval = false;
var intervalID_ResumeSlideShow = null;
$('.carousel').carousel({
    pause: "false",
});
$('.carousel').on("click", function () {
    pushSlideShow();
});


function pushSlideShow() {
    if (IsSetInterval == false) {
        intervalID_ResumeSlideShow = window.setInterval(ResumeSlideShow, 900000);
    }
    IsSetInterval = true;
    $("#myCarousel").carousel("pause");

}
function ResumeSlideShow() {
    IsSetInterval = false;
    $('#myCarousel').carousel('cycle');
    clearInterval(intervalID_ResumeSlideShow);
}