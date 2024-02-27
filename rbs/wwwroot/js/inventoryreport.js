$(document).ready(function () {
    checkAgent();

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

    $("#btnSearch").click(function () {
        $("#msg").html("");
        $("#errorMessage").hide();

        if ($("#txtStartDate").val() === "" ||
            $("#txtEndDate").val() === "") {

            $("#msg").html("Select start and end date first.");
            $("#errorMessage").show();
        } else {
            InventoryReport();
        }
    });
});

function InventoryReport() {
    $("#btnSearch").hide();
    $("#loader").show();
    $("#spanResults").html("");
    var report = new Object();
    report.FromDate = $("#txtStartDate").val();
    report.ToDate = $("#txtEndDate").val();

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: "/api/InternalSalesStats/",
        data: JSON.stringify(report),
        success: function (result) {
            console.log("Report : " + JSON.stringify(result));
            BuildInventoryReportTable(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildInventoryReportTable(data) {
    var table = document.createElement("table");

    table.appendChild(BuildInventoryReportHeader());

    var prevInvoice = "";
    for (var i = 0; i < data.length; i++) {
        var newInvoice = (i == 0 || (prevInvoice != data[i].invoice));
        table.appendChild(BuildInventoryReportRow(newInvoice, data[i]));
        prevInvoice = data[i].invoice;
    }

    $("#spanResults").html(table);

    $("#btnSearch").show();
    $("#loader").hide();

}

function BuildInventoryReportHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Agent");
    var text2 = document.createTextNode("Invoice #");
    var text3 = document.createTextNode("Date");
    var text4 = document.createTextNode("In/Out");
    var text5 = document.createTextNode("Manu");
    var text6 = document.createTextNode("Prod");
    var text7 = document.createTextNode("Prod #");
    var text8 = document.createTextNode("Serial");
    //var text7 = document.createTextNode("Manu");
    

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

function BuildInventoryReportRow(newInvoice, rowData) {
    console.log(JSON.stringify(rowData));
    var tr = document.createElement("tr");
    if (newInvoice) {
        tr.style.cssText = "border-top: solid black;";
    }

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");
    var td8 = document.createElement("td");

    var _text1 = "";
    var _text2 = "";
    var _text3 = "";
    var _text4 = "";

    if (newInvoice) {
        _text1 = rowData.agent;
        _text2 = rowData.invoice;
        _text3 = rowData.insertDate.split("T")[0];
        _text4 = (rowData.inOrOut=="True") ? "IN" : "OUT";
    } //else, leave blank

    var text1 = document.createTextNode(_text1);
    var text2 = document.createTextNode(_text2);
    var text3 = document.createTextNode(_text3);
    var text4 = document.createTextNode(_text4);
    var text5 = document.createTextNode(rowData.manuText);
    var text6 = document.createTextNode(rowData.productText);
    var text7 = document.createTextNode(rowData.productNumber);
    var text8 = document.createTextNode(rowData.serialNumber);

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
