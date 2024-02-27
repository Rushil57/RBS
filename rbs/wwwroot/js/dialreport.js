$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");
    checkAgent();

    LoadAgents();

    $("#btnSearch").click(function () {
        GenerateDialReport();
    });
});

function GenerateDialReport() {
    $("#loader").show();

    $("#spanDialReport").html("");

    var agentid = $('#DDLAgents option:selected').val();

    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/RawCount/" + agentid,
        dataType: "json",
        success: function (result) {
            console.log(JSON.stringify(result));
            $("#spanDialReportToday").html(BuildDialReportTable(result.todaysDials));
            $("#spanDialReportYest").html(BuildDialReportTable(result.yesterdaysDials));
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

function LoadAgents() {
    $.ajax({
        url: "/api/Agent",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            var DDLAgents = document.getElementById("DDLAgents");
            $("#DDLAgents option").remove();
            for (var i = 0; i < data.length; i++) {
                if (data[i].agentTypeId == 30 || data[i].agentTypeId == 40) {
                    DDLAgents.innerHTML = DDLAgents.innerHTML +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
                }
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

function BuildDialReportTable(data) {
    var table = document.createElement("table");

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildDialReportRow(data[i]));
    }

    return table;
}

function BuildDialReportRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    var leadLink = "<a href='/leaddial.html?lid=" + rowData.leadId + "'>" + rowData.leadId + "</a>";
    var text2 = document.createTextNode(rowData.marked);
    
    var text3 = "";
    if (rowData.dialedTime.split("T")[1] !== "00:00:00") {
        var shortDate = DateFormat(rowData.dialedTime);

        //var text3 = document.createTextNode(FormatShortDateTime(rowData.dialedTime.split("T")[0]).split(' ')[0] === "1/1/1"
        //    ? "Clocked In" : rowData.dialedTime.split("T")[1] === "00:00:00"
        //    ? "" : FormatShortDateTime(rowData.dialedTime.split("T")[0]).split(' ')[0] + ' ' + timeConvert(rowData.dialedTime.split("T")[1].split(":")[0] + ":" + rowData.dialedTime.split("T")[1].split(":")[1]));
        text3 = document.createTextNode(shortDate);
    }
    td1.innerHTML = leadLink;
    td2.appendChild(text2);
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}