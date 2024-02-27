var agentData;
var aId = 0;
var Lid;
var agentType;
var showLinks = false;
var CurrentDays = null;
window.onload = function () {

    $("#spanClockinout").load("/clockinout.html");
    $("#loader").show();

    checkAgent();
    setAgentType();

    if (agentType === "70" || agentType === "60" || agentType === "30") {
        showLinks = true;
    } else {
        showLinks = false;
    }

    GetCalendarLeads();
};

function GetCalendarLeads() {
    var url = "/api/Lead/GetCalendarLeads/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            console.log(JSON.stringify(result));
            //load the tables here
            BuildCalendarTables(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildCalendarTables(d) {
    if (showLinks) {
        BuildTable("#spanUFSResults", d.ufs);
        BuildTable("#spanYesterdayResults", d.yesterday);
        BuildTable("#spanLastWeekResults", d.lastWeek);
        BuildTable("#spanRescheduleResults", d.reschedule);
        BuildLastWeekToCurrentData(d.lastWeekThisWeekLeadData);
    } else {
        $("#ufsHeader").hide();
        $("#AllSchedHeader").hide();
        $("#yesterdayHeader").hide();
        $("#rescheduleHeader").hide();
        $("#lastWeekHeader").hide();
    }
    BuildTable("#spanTodayResults", d.today);
    BuildTable("#spanThisWeekResults", d.thisWeek);

}

function BuildTable(id, data) {


    var table = document.createElement("table");
    table.appendChild(BuildHeader());
    CurrentDays = null;
    for (var i = 0; i < data.length; i++) {
        if (id != "#spanUFSResults" && id != "#spanRescheduleResults") {
            var dates = new Date(data[i].schedApptDate);
            var days = dates.getDay();
            if (days != CurrentDays || CurrentDays == null) {
                var table1 = document.createElement("table");
                var tr1 = document.createElement("tr");
                tr1.className = "dayTableBorder";
                var td0 = document.createElement("td"); //Days
                var d = new Date();
                var weekday = new Array(7);
                weekday[0] = "Sunday";
                weekday[1] = "Monday";
                weekday[2] = "Tuesday";
                weekday[3] = "Wednesday";
                weekday[4] = "Thursday";
                weekday[5] = "Friday";
                weekday[6] = "Saturday";

                var dayName = weekday[days];
                CurrentDays = dates.getDay();
                td0.colSpan = 7;
                td0.innerHTML = dayName;
                td0.className = "textAlignCenter";
                td0.style.fontWeight = "600";
                td0.style.fontSize = "17px";
                tr1.appendChild(td0);
                table.appendChild(tr1)
            }
        }
        table.appendChild(BuildRow(data[i]));
    }
    if (data.length == 0) {       
        if (id == "#spanUFSResults") {
            $("#ufsHeader").hide();
            $("#spanUFSResults").hide();
        }
        if (id == "#spanLastWeekThisWeekData") {
            $("#spanLastWeekThisWeekData").hide();
            $("#AllSchedHeader").hide();
        }
        if (id == "#spanYesterdayResults") {
            $("#spanYesterdayResults").hide();
            $("#yesterdayHeader").hide();
        }
        if (id == "#spanLastWeekResults") {
            $("#spanLastWeekResults").hide();
            $("#lastWeekHeader").hide();
        }
        if (id == "#spanRescheduleResults") {
            $("#spanRescheduleResults").hide();
            $("#rescheduleHeader").hide();
        }
        if (id == "#spanTodayResults") {
            $("#spanTodayResults").hide();
            $("#todayHeader").hide();
        }
        if (id == "#spanThisWeekResults") {
            $("#spanThisWeekResults").hide();
            $("#thisWeekHeader").hide();
        }
    }
    if (id != "#spanUFSResults" && id != "#spanRescheduleResults") {
        table.className = "tableBorder";
    }
    $(id).html(table);
    $("#loader").hide();
}

function BuildRow(rowData) {
    console.log(rowData);
    var tr = document.createElement("tr");

    var td1 = document.createElement("td"); //leadid
    var td2 = document.createElement("td"); //status
    var td3 = document.createElement("td"); //misc
    var td4 = document.createElement("td"); //name
    var td5 = document.createElement("td"); //address
    var td6 = document.createElement("td"); //Phone
    var td7 = document.createElement("td"); //sa date

    var _leadfullname = rowData.firstName + " " + rowData.lastName;

    var _leadIdLink = '';
    var _statusText = "Follow-Up";
    var _status = "Follow-Up";
    var _otherText = "";
    if (rowData.leadStatusId == 52 || rowData.leadStatusId == 53 || rowData.leadStatusId == 60) {
        _leadIdLink = '<a href="/sadispatch.html?lid=' + rowData.leadId + '">' + rowData.leadId + "</a>";

        if (rowData.leadStatusId == 52) {
            var shortDate = DateFormat(rowData.followUpDate);
            _otherText = shortDate === null ? "" : shortDate;

            //var shortFUDate = rowData.followUpDate.split("T");
            //_otherText = shortFUDate[0] === "0001-01-01" ? "" : shortFUDate[0] + ' ' + timeConvert(rowData.followUpDate.split("T")[1].split(":")[0] + ":" + rowData.followUpDate.split("T")[1].split(":")[1]);
            //_otherText = _otherText.replace("2019-0", "");
        }
        else if (rowData.leadStatusId == 53) {
            _status = "Needs Confirm";
            _statusText = "<td style='background-color:red'><span style='color:#FFF' > Needs Confirm </span></td>";
        }
        else if (rowData.leadStatusId == 60) {
            _status = "Reschedule";
            _statusText = "<td style='background-color:pink'><span style='color:#FFF' > Reschedule </span></td>";
        }
    }
    else if (rowData.leadStatusId >= 54 || rowData.leadStatusId <= 59) {
        _leadIdLink = '<a href="/repdispatch.html?lid=' + rowData.leadId + '">' + rowData.leadId + "</a>";
        if (rowData.leadStatusId == 54) {
            _status = "Confirmed, needs Dispatch";
            _statusText = "<td>Confirmed, needs Dispatch</td>";

        }
        else if (rowData.leadStatusId == 55) {
            _otherText = rowData.dispatchedRepsName;
            _status = "Dispatched";
            _statusText = "<td style=background-color:blue><span style='color:#FFF'>Dispatched</span></td>";

        }
        else if (rowData.leadStatusId == 56) {
            _otherText = rowData.dispatchedRepsName;
            _status = "Sold";
            _statusText = "<td style=background-color:#32CD32><span style='color:#FFF'>Sold</span></td>";

        }
        else if (rowData.leadStatusId == 57) {
            _status = "Installed";
            _statusText = "<td style=background-color:#008000><span style='color:#FFF'>Installed</span> </td>";
        }
        else if (rowData.leadStatusId == 58) {
            _otherText = rowData.dispatchedRepsName;
            _status = "UFS";
            _statusText = "<td style=background-color:orange><span style='color:#FFF'>UFS</span> </td>";
        }
        else if (rowData.leadStatusId == 59) {
            _otherText = rowData.dispatchedRepsName;
            _status = "No Show";
            _statusText = "<td>No Show</td>";
        }
    }

    if (!showLinks) {
        _leadIdLink = rowData.leadId;
    }

    var _address = rowData.address + ", " + rowData.city + ", " + rowData.state;
    var googleMapsLink = '<a href="' + GetGoogleMapsLink(_address) + '" target="_blank">' + _address + "</a>";
    var otherTextNode = document.createTextNode(_otherText);
    var text3 = document.createTextNode(_leadfullname);
    var shortDate1 = rowData.schedApptDate.split("T");

    var shortDate = DateFormat(rowData.schedApptDate);
    //var _text5 = shortDate[0] === "0001-01-01" ? "" : shortDate[0] + ' ' + timeConvert(rowData.schedApptDate.split("T")[1].split(":")[0] + ":" + rowData.schedApptDate.split("T")[1].split(":")[1]);
    //_text5 = _text5.replace("2019-0", "");

    var _text5 = shortDate;
    var text5 = document.createTextNode(_text5);
    var text6 = document.createTextNode(rowData.phone1);

    td1.innerHTML = _leadIdLink;

    td2.innerHTML = _statusText;

    td2.textAlign = "left";
    if (_status == "Needs Confirm") {
        td2.bgColor = "#ed0000";
    }
    else if (_status == "Dispatched") {
        td2.bgColor = "blue";
    }
    else if (_status == "Sold") {
        td2.bgColor = "#32CD32";
    }
    else if (_status == "Installed") {
        td2.bgColor = "#008000";
    }
    else if (_status == "UFS") {
        td2.bgColor = "orange";
    }
    else if (_status == "Reschedule") {
        td2.bgColor = "#FF33FF";
    }
    td3.appendChild(otherTextNode);
    td4.appendChild(text3);
    td5.innerHTML = googleMapsLink;
    td6.appendChild(text6);
    td7.appendChild(text5);
    if (window.innerWidth > 767) {
        td2.className = "textAlignCenter";
        td1.width = "50px";
        td2.width = "60px";
        td3.width = "110px";
        td5.width = "350px";
        td7.width = "200px";
    }

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");

    var text1 = document.createTextNode("LeadId");
    var text2 = document.createTextNode("Status");
    var text3 = document.createTextNode("");
    var text4 = document.createTextNode("Lead Name");
    var text5 = document.createTextNode("Address");
    var text6 = document.createTextNode("Phone");
    var text7 = document.createTextNode("Sched Appt");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}

function BuildLastWeekToCurrentData(data) {
    var table = document.createElement("table");

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildDataRow(data[i]));
    }

    $("#spanLastWeekThisWeekData").html(table);
}

function BuildDataRow(rowData) {
    console.log(rowData);
    var tr = document.createElement("tr");

    var td1 = document.createElement("td"); //leadid
    var td2 = document.createElement("td"); //firstname + lastname
    var td3 = document.createElement("td"); //address + city
    var td4 = document.createElement("td"); //current status
    var td5 = document.createElement("td"); //Phone
    var td6 = document.createElement("td"); //last touched

    var _leadfullname = rowData.firstName + " " + rowData.lastName;
    var _address = rowData.address + " " + rowData.city
    var _leadIdLink = '';
    if (rowData.leadStatusId == 52 || rowData.leadStatusId == 53) {
        _leadIdLink = '<a href="/sadispatch.html?lid=' + rowData.leadId + '">' + rowData.leadId + "</a>";
    } else {
        _leadIdLink = '<a href="/repdispatch.html?lid=' + rowData.leadId + '">' + rowData.leadId + "</a>";
    }
    if (!showLinks) {
        _leadIdLink = rowData.leadId;
    }

    var text2 = document.createTextNode(_leadfullname);
    var text3 = document.createTextNode(_address);
    var text4 = document.createTextNode(rowData.statusText);
    var shortDate = DateFormat(rowData.insertDate);
    var text5 = document.createTextNode(rowData.phone1);
    var text6 = document.createTextNode(shortDate);

    td1.innerHTML = _leadIdLink;
    td2.appendChild(text2);
    td2.className = "textAlign";
    td3.appendChild(text3);
    td3.className = "textAlign";
    td4.appendChild(text4);
    td4.className = "textAlign";
    td5.appendChild(text5);
    td6.appendChild(text6);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);

    return tr;
}