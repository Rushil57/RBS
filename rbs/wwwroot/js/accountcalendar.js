var agentData;
var aId = 0;
var agentType;
var CurrentDays = null;
window.onload = function () {

    $("#spanClockinout").load("/clockinout.html");
    $("#loader").show();

    checkAgent();
    setAgentType();

    GetTechCalendar();
};


function GetTechCalendar() {
    var url = "/api/Account/GetTechCalendar/";
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

    BuildTable("#spanThisWeekResults", d.thisWeek);
    BuildTable("#spanLastWeekResults", d.lastWeek);

}

function BuildTable(id, data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());
    CurrentDays = null;
    for (var i = 0; i < data.length; i++) {

        //var dates = new Date(data[i].installedDate);
        var dates = new Date(data[i].insertDate);
        
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
            td0.colSpan = 8;
            td0.innerHTML = dayName;
            td0.className = "textAlignCenter";
            td0.style.fontWeight = "600";
            td0.style.fontSize = "17px";
            tr1.appendChild(td0);
            table.appendChild(tr1)
        }
        table.appendChild(BuildRow(data[i]));
    }
    table.className = "tableBorder";
    $(id).html(table);
    $("#loader").hide();
}

function BuildRow(rowData) {
    console.log(rowData);
    var tr = document.createElement("tr");

    var td1 = document.createElement("td"); //Account Id
    var td2 = document.createElement("td"); //status
    var td3 = document.createElement("td"); //Customer Name
    var td4 = document.createElement("td"); //Address
    var td5 = document.createElement("td"); //Email
    var td6 = document.createElement("td"); //Phone
    var td7 = document.createElement("td"); //Last Touch date
    var td8 = document.createElement("td"); //Date Created

    var _leadfullname = rowData.firstName + " " + rowData.lastName;

    var _accountIdLink = '<a href=/accountdetails.html?v=' + Math.random() + '&aid=' + rowData.accountId + '>' + rowData.accountId + '</a>';        
   
    var _statusText = GetAccountStatusIdText(rowData.accountStatusId);
    
    var _status = "";
    var _otherText = "";

   
    var _address = rowData.address + ", " + rowData.city + ", " + rowData.state;
    var googleMapsLink = '<a href="' + GetGoogleMapsLink(_address) + '" target="_blank">' + _address + "</a>";
    var text3 = document.createTextNode(_leadfullname);

    var _email = "";
    var email = rowData.email.split(";");
    var stremail = "";
    for (var i = 0; i < email.length; i++) {
        _email += email[i] + "<br />"
    }
    var text6 = document.createTextNode(rowData.phone1);

    var lastTouchedshortDate = DateFormat(rowData.lastTouched);
    var _text7 = lastTouchedshortDate;
    var text7 = document.createTextNode(_text7);

    var shortDate = DateFormat(rowData.insertDate);
    var _text8 = shortDate;
    var text8 = document.createTextNode(_text8);

    td1.innerHTML = _accountIdLink;
    td2.innerHTML = GetAccountStatusIdText(rowData.accountStatusId);
    td3.appendChild(text3);
    td4.innerHTML = googleMapsLink;
    td5.innerHTML = _email;
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    if (window.innerWidth > 767) {
        td2.className = "textAlignCenter";
        td1.width = "50px";
        td2.width = "60px";
        td3.width = "110px";
        td5.width = "300px";
        td7.width = "100px";
        td8.width = "100px";
    }

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

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
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Account Id");
    var text2 = document.createTextNode("Status");
    var text3 = document.createTextNode("Customer Name");
    var text4 = document.createTextNode("Address");
    var text5 = document.createTextNode("Email");
    var text6 = document.createTextNode("Phone");
    var text7 = document.createTextNode("Last Touched");
    var text8 = document.createTextNode("Date Created");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}