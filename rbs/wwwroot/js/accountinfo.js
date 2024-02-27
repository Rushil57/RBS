$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");
    GetAllAccountInfo();

});

function GetAllAccountInfo() {

    $("#loader").show();
    var url = "/api/Account/GetAllAccountInfo/";
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {

            BuildAccountInfoTables(result);
            //BuildTable(result);
            $("#loader").hide();
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildAccountInfoTables(d) {

    BuildTable("#spanThishWeekResults", d.thisWeek);
    BuildTable("#spanLasthWeekResults", d.lastWeek);
    BuildTable("#spanOldestResult", d.oldest);
}

function BuildTable(id, data) {

    var table = document.createElement("table");
    table.appendChild(BuildHeader());
    CurrentDays = null;
    if (data != null) {
        for (var i = 0; i < data.length; i++) {
            if (id != "#spanOldestResult") {
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
                    td0.colSpan = 6;
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
    }
    table.className = "tableBorder";
    $(id).html(table);
    $("#loader").hide();
}


function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    //var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");


    var text1 = document.createTextNode("AccountId");
    var text2 = document.createTextNode("Status");
    var text3 = document.createTextNode("Rep/Tech");
    var text4 = document.createTextNode("Customer Info");
    //var text5 = document.createTextNode("Next Action");
    var text6 = document.createTextNode("Notes");
    var text7 = document.createTextNode("Created Date");


    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    //td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    if (window.innerWidth > 767) {
        //td2.className = "textAlignCenter";
        td1.width = "40px";
        td2.width = "50px";
        td3.width = "100px";
        td4.width = "250px";
        td6.width = "250px";
        td7.width = "100px";
    }
    tr.appendChild(td1);
    tr.appendChild(td4);
    tr.appendChild(td2);
    tr.appendChild(td3);
    //tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}


function BuildRow(rowData) {
    var tr = document.createElement("tr");
    var td1 = document.createElement("td");//AccountId
    var td4 = document.createElement("td");//Customer Info
    var td2 = document.createElement("td");//Status
    var td3 = document.createElement("td");//Rep/Tech    
    //var td5 = document.createElement("td");//Next Action
    var td5 = document.createElement("td");//Notes
    var td6 = document.createElement("td");//Insert Date


    var _leadfullname = rowData.firstName + " " + rowData.lastName;
    var shortDate = DateFormat(rowData.soldDate);
    var _status = GetAccountStatusIdText(rowData.accountStatusId);
    _status = _status + " </br > (" + shortDate + ")";
    var _accountIdLink = '<a href="/accountdetails.html?aid=' + rowData.accountId + '">' + rowData.accountId + "</a>";
    var nextAction = "";
    
    var repTech = "";

    if (rowData.rep != "" && rowData.rep != null) {
        repTech += "R: " + rowData.rep + "</br>";
    }
    if (rowData.tech != "" && rowData.tech != null) {
        repTech += "T: " + rowData.tech;
    }
    var _address = rowData.address + ", " + rowData.city + ", " + rowData.state;
    var googleMapsLink = '<p>' + _leadfullname + ' </p>';

    if (_address != null && _address != "" && _address != ", , ") {
        googleMapsLink = googleMapsLink + '<a href="' + GetGoogleMapsLink(_address) + '" target="_blank">' + _address + "</a>";
    }
    if (rowData.phone1 != null && rowData.phone1 != "") {
        googleMapsLink = googleMapsLink + "<br /> <b>Phone:</b> " + rowData.phone1;
    }
    if (rowData.email != null && rowData.email != "") {
        var _email = "";

        var email = rowData.email.split(";");
        var stremail = "";
        for (var i = 0; i < email.length; i++) {
            _email += email[i] + "<br />"
        }
        googleMapsLink = googleMapsLink + "<br /><b> Email: </b>" + _email;
    }
    //var text2 = document.createTextNode(_status);
    //var text3 = document.createTextNode(repTech);
    var text5 = document.createTextNode(nextAction);
    var noteText = rowData.noteText;
    if (noteText != null && noteText != "") {
        noteText = noteText.replace(/\n/g, "<br />");
    }
    var text6 = document.createTextNode(noteText);

    var lastTouchedshortDate = DateFormat(rowData.insertDate);
    var _text7 = lastTouchedshortDate;
    var text7 = document.createTextNode(_text7);

    td1.innerHTML = _accountIdLink;
    td2.innerHTML = _status;
    if (rowData.accountStatusId == 56) {
        td2.bgColor = "#ffa500";
        td2.className = "font-color-white";
    }
    else if (rowData.accountStatusId == 110) {
        td2.bgColor = "#0000ff";
        td2.className = "font-color-white";
    }
    else if (rowData.accountStatusId == 120) {
        td2.bgColor = "#ffff00";
    }
    if (_status == "Installed") {
        td2.bgColor = "#008000";
        td2.className = "font-color-white";
    }
    td3.innerHTML = repTech;

    td4.innerHTML = googleMapsLink;
    //td5.appendChild(text5);
    td5.innerHTML = noteText;

    td6.appendChild(text7);
    if (window.innerWidth > 767) {
        //td2.className = "textAlignCenter";
        td1.width = "40px";
        td2.width = "50px";
        td3.width = "100px";
        td4.width = "250px";
        td5.width = "300px";
        td6.width = "100px";
    }

    tr.appendChild(td1);
    tr.appendChild(td4);
    tr.appendChild(td2);
    tr.appendChild(td3);
    //tr.appendChild(td5);
    tr.appendChild(td5);
    tr.appendChild(td6);

    return tr;
}
