var agentData;
var aId = 0;
var Lid;
var agentType;
var showLinks = false;

$(document).ready(function () {
    $("#loader").show();

    var i = getParameterByName("i");      
    var q = getParameterByName("q");      

    // User Validation check in here
    if (i !== "" && q !== "") {
        //valid
        GetRepLeads(i, q);
    }
    else {
        console.log(i+q);
    }
});

function GetRepLeads(i, q) {
    var url = "/api/Lead/GetRepLeads/"+i+"/"+q;
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            console.log(JSON.stringify(result));
            //load the tables here
            BuildTable(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}
function BuildTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spantwoWeekResults").html(table);
    $("#loader").hide();
}


function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
  
    var text1 = document.createTextNode("Name");
    var text2 = document.createTextNode("Address");
    var text3 = document.createTextNode("Status");

    td1.appendChild(text1);
    td1.width = "120px";
    td2.appendChild(text2);
    td3.appendChild(text3);
    td3.width = "100px";


    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
  
    return tr;
}


function BuildRow(rowData) {
    console.log(rowData);
    var tr = document.createElement("tr");

    var td1 = document.createElement("td"); //name
    var td2 = document.createElement("td"); //address
    var td3 = document.createElement("td"); //status

    var _leadfullname =  rowData.lastName;

    var _status = "Follow-Up";
    if (rowData.leadStatusId == 52 || rowData.leadStatusId == 53) {
        if (rowData.leadStatusId == 53) {
            _status = "Needs Confirm";
        }
    }
    else if (rowData.leadStatusId >= 54 || rowData.leadStatusId <= 59) {
        if (rowData.leadStatusId == 54) {
            _status = "Confirmed, needs Dispatch";
        }
        else if (rowData.leadStatusId == 55) {
            _status = "Dispatched";
        }
        else if (rowData.leadStatusId == 56) {
            _status = "Sold";
        }
        else if (rowData.leadStatusId == 57) {
            _status = "Installed";
        }
        else if (rowData.leadStatusId == 58) {
            _status = "UFS";
        }
        else if (rowData.leadStatusId == 59) {
            _status = "No Show";
        }
    }

    var _address = rowData.address + ", " + rowData.city;
    var googleMapsLink = '<a href="' + GetGoogleMapsLink(_address) + '" target="_blank">' + _address + "</a>";
    var text3 = document.createTextNode(_status);
    var text1 = document.createTextNode(_leadfullname);
  
    td1.width = "200px";
    td1.appendChild(text1);
    td2.width = "220px";    
    td2.innerHTML = googleMapsLink;
    td3.appendChild(text3);
    td3.width = "100px";

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
 
    return tr;
}