/// <reference path="util.js" />
var agentType;
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;

}

$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");

    if (agentType === "70" || agentType === "60") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }

    if (window.location.pathname === "/newlead.html") {
        $("#menus").hide();
        $("#menubar").hide();

    }

    $("#btnLookup").click(function () {
        if ($("#txtLeadId").val() === "") {
            $("#msg").html("Enter LeadId.");
            $("#errorMessage").show();
        }
        else if ($('#txtFirstName').val() === "") {
            $("#msg").html("Enter First Name.");
            $("#errorMessage").show();
        }
        else if ($("#txtLastName").val() === "") {
            $("#msg").html("Enter Last Name.");
            $("#errorMessage").show();
        }
        else if ($("#txtCity").val() === "") {
            $("#msg").html("Enter City.");
            $("#errorMessage").show();
        }
        else if ($("#txtState").val() === "") {
            $("#msg").html("Enter State.");
            $("#errorMessage").show();
        }
        
        else {
            Lookup();
        }
        
    });

    $("#btnClear").click(function () {
        ResetLookupForm();
    });
});

function Lookup() {
    $("#successMsg").hide();
    $("#errorMessage").hide();

    var lead = new Object();
    lead.AgentIdSubmitting = getCookie("agentId");
    lead.LeadId = $("#txtLeadId").val();
    lead.FirstName = $("#txtFirstName").val();
    lead.MiddleName = $("#txtMiddleName").val();
    lead.LastName = $("#txtLastName").val();
    lead.City = $("#txtCity").val();
    lead.State = $("#txtState").val();
    lead.FormerCity = $("#txtFormerCity").val();
    lead.FormerState = $("#txtFormerState").val();
    lead.Address = $("#txtAddress").val();
    //have to clear the form or reload to get the button
    $("#btnLookup").hide();

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: "/api/Lookup/",
        data: JSON.stringify(lead),
        success: function (result) {
            if (result.personFound) {
                console.log(JSON.stringify(result));
                if (result.possiblePeople[0] != null) {
                    $("#spanResults0").html(BuildLookupTable(result.possiblePeople[0]));
                }
                if (result.possiblePeople[1] != null) {
                    $("#spanResults1").html(BuildLookupTable(result.possiblePeople[1]));
                }
                if (result.possiblePeople[2] != null) {
                    $("#spanResults2").html(BuildLookupTable(result.possiblePeople[2]));
                }
            } else {
                $("#msg").html("Not a good enough match.  Refine search, or else mark as \"Person Not Found\".");
                $("#errorMessage").show();
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
        }
    });
}

function BuildLookupTable(rowData) {
    var table = document.createElement("table");

    //name(s) & age
    var trName = document.createElement("tr");
    var tdName1 = document.createElement("td");
    var textName1 = document.createTextNode("Name & Age:");
    tdName1.appendChild(textName1);
    trName.appendChild(tdName1);

    var tdName2 = document.createElement("td");
    var _cleanMiddle = (rowData.middleName == null) ? "" : rowData.middleName + " ";
    var _age = (rowData.age == null || rowData.age == 0) ? "" : ", " + rowData.age;
    var _name = rowData.firstName + " " + _cleanMiddle + rowData.lastName + _age;
    var textName2 = document.createTextNode(_name);
    tdName2.appendChild(textName2);
    trName.appendChild(tdName2);
    table.appendChild(trName);

    //address(es)
    if (rowData.addresses != null && rowData.addresses.length > 0) {
        var trAddress = document.createElement("tr");
        var tdAddress1 = document.createElement("td");
        var textAddress1 = document.createTextNode("Address(es):");
        tdAddress1.appendChild(textAddress1);
        trAddress.appendChild(tdAddress1);

        var tdAddress2 = document.createElement("td");
        var _addresses = "";

        for (var iaddress = 0; iaddress < rowData.addresses.length; iaddress++) {
            _addresses += rowData.addresses[iaddress] + "<br/>";
        }
        tdAddress2.innerHTML = _addresses;
        trAddress.appendChild(tdAddress2);
        table.appendChild(trAddress);
    }

    //possible spouse
    if (rowData.possibleSpouse != null && rowData.possibleSpouse.length > 0) {
        var spouse = "";
        for (var ispouse = 0; ispouse < rowData.possibleSpouse.length; ispouse++) {
            spouse += rowData.possibleSpouse[ispouse] + ", ";
        }
        var trSpouse = document.createElement("tr");
        var tdSpouse1 = document.createElement("td");
        tdSpouse1.appendChild(document.createTextNode("Possible spouses:"));
        trSpouse.appendChild(tdSpouse1);

        var tdSpouse2 = document.createElement("td");
        var textSpouse2 = document.createTextNode(spouse);
        tdSpouse2.appendChild(textSpouse2);
        trSpouse.appendChild(tdSpouse2);
        table.appendChild(trSpouse);
    }

    //phones
    if (rowData.phones != null && rowData.phones.length > 0) {
        for (var i = 0; i < rowData.phones.length; i++) {
            var tmptrphone = document.createElement("tr");
            //build phoneN row here
            var tdPhone1 = document.createElement("td");
            var textPhone1 = document.createTextNode("Phone:");
            tdPhone1.appendChild(textPhone1);
            tmptrphone.appendChild(tdPhone1);

            var tdPhone2 = document.createElement("td");
            var _phone = "<input type='text' id='phone" +
                rowData.phones[i].number +
                "' readonly='' style='background-color: rgb(235, 235, 228);' value='";
            _phone += rowData.phones[i].number + "' >";
            _phone += "<button type='button' onclick='CopyToClipboard(\"phone";
            _phone += rowData.phones[i].number;
            _phone += "\")' class='btn btn-default btn-sm'><i class='glyphicon glyphicon-copy'></i></button><br/>";
            if (rowData.phones[i].type != null) {
                _phone += "type: " + rowData.phones[i].type + "<br/>";
            }
            if (rowData.phones[i].last_seen != null) {
                _phone += "last seen: " + rowData.phones[i].last_seen + "<br/>";
            }
            if (rowData.phones[i].valid_since != null) {
                _phone += "valid since: " + rowData.phones[i].valid_since + "<br/>";
            }
            tdPhone2.innerHTML = _phone;
            tmptrphone.appendChild(tdPhone2);
            table.appendChild(tmptrphone);
        }
    }
    //emails
    if (rowData.emails != null && rowData.emails.length > 0) {
        var emails = "";
        for (var iemails = 0; iemails < rowData.emails.length; iemails++) {
            emails += rowData.emails[iemails] + ";";
        }
        var trEmail = document.createElement("tr");
        var tdEmail1 = document.createElement("td");
        var textEmail1 = document.createTextNode("Email:");
        tdEmail1.appendChild(textEmail1);
        trEmail.appendChild(tdEmail1);


        var tdEmail2 = document.createElement("td");
        var _email = "<button type='button' onclick='CopyToClipboard(\"emailrow\")' class='btn btn-default btn-sm'><i class='glyphicon glyphicon-copy'></i></button><br/>";
        _email += "<input type='text' id='emailrow' readonly='' style='background-color: rgb(235, 235, 228);' value='" + emails + "' >";

        tdEmail2.innerHTML = _email;
        trEmail.appendChild(tdEmail2);
        table.appendChild(trEmail);
    }

    return table;
}

function ResetLookupForm() {
    $("#txtLeadId").val("");
    $("#txtFirstName").val("");
    $("#txtMiddleName").val("");
    $("#txtLastName").val("");
    $("#txtAddress").val("");
    $("#txtCity").val("");
    $("#txtState").val("");
    $("#txtFormerCity").val("");
    $("#txtFormerState").val("");
    $("#spanResults0").html("");
    $("#spanResults1").html("");
    $("#spanResults2").html("");
    //have to clear the form or reload to get the button
    $("#btnLookup").show();
}

