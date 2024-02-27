var agentid = null;
var IsList = true
$(document).ready(function () {
    ChcekCookie();    
    $("#spanClockinout").load("/clockinout.html");
    $("#divDetails").hide();
    $("#btnSubmit").click(function () {
        CreateUser();
    });
  
    if (IsList == true) {
        LoadAgentData("", "");
    }
    agentid = getParameterByName("aid");      
    if (IsList == false) {
        
    }
    LoadAllAgentType();
    $("#btnSearch").click(function () {
        $("#btnSearch").hide();
        $("#loader").show();
        var _searchfield = $("#ddlSearchField").find(":selected").val();
        var _searchterm = $("#txtSearchAgent").val();
        LoadAgentData(_searchfield, _searchterm);
    });
});

function LoadAllAgentType() {

    $.ajax({
        url: "/api/Agent/GetAllAgentType",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            agents = data;
            
            var DDLAgentType = document.getElementById("DDLAgentType");
            if (DDLAgentType != null) {
                $("#DDLAgentType option").remove();

                DDLAgentType.innerHTML = '<option value="0">- SELECT -</option>';
                for (var i = 0; i < data.length; i++) {
                    DDLAgentType.innerHTML = DDLAgentType.innerHTML +
                        '<option value="' + data[i].agentid + '">' + data[i].agenttypename + '</option>';
                }
                if (agentid != null) {
                    $("#divDetails").hide();
                    GetAgentData(agentid);
                }
            }
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });

}

function CreateUser() {

    $("#successMsg").hide();
    $("#errorMessage").hide();
    var strmsg = "";
    if ($("#txtFirstName").val() == null || $("#txtFirstName").val() == "") {
        strmsg = "Enter First Name.</br>"
    }
    if ($("#txtLastName").val() == null || $("#txtLastName").val() == "") {
        strmsg += "Enter Last Name.</br>"
    }
    if ($("#txtEmail").val() == null || $("#txtEmail").val() == "") {
        strmsg += "Enter Email."
    }
    if (strmsg != "") {
        $("#msg").html(strmsg);
        $("#errorMessage").show();
        $("#btnSubmit").show();
        return false;
    }
    var user = new Object();
    if (agentid != null && agentid != '') {
        user.agentid = agentid;
    } else {
        user.agentid = 0;
    }
    user.FirstName = $("#txtFirstName").val();
    user.LastName = $("#txtLastName").val();
    user.Phone = $("#txtphone").val();
    user.AgentPassword = $("#txtpassword").val();
    user.AgentTypeId = $("#DDLAgentType").val();
    if ($('#chkIsEnabled').is(":checked")) {
        user.Status = parseInt(1);
    }
    else {
        user.Status = parseInt(0);
    }
    user.Email = $("#txtEmail").val();
    var url = "/api/Agent/CreateAgent";

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(user),
        success: function (result) {
            if (result) {
                console.log(JSON.stringify(result));
                $("#btnSubmit").show();
                $("#successMsg").show();
                $("#msg").html("Saved Successfully");
                $("#updateMsg").hide();
                
                Clear();
            }
            else {
                $("#msg").html("Something is wrong! please try again later");
                $("#errorMessage").show();
                $("#btnSubmit").show();
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#btnSubmit").show();
        }
    });
}

function GetAgentData(agentid) {
    
    var IsEdit = getParameterByName("isedit");
    $("#loader").show();  
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/Agent/GetAgentById/" + agentid,
        success: function (result) {

            if (result) {
                if (IsEdit == "false") {
                    $(".form-control").prop('disabled', true);
                    $("#btnSubmit").hide();
                    $("#btnCancel").prop('disabled', false);  
                    $("#btnCancel").val("Back");  
                    document.getElementById("chkIsEnabled").disabled = true;
                }
                else {
                    $(".form-control").prop('disabled', false);                    
                    document.getElementById("chkIsEnabled").disabled = false;
                    $("#btnSubmit").show()
                }

                $("#txtFirstName").val(result.firstName);
                $("#txtLastName").val(result.lastName);
                $("#txtEmail").val(result.email);
                $("#txtpassword").val(result.agentPassword);
                if (result.status == true) {
                    $("#chkIsEnabled").prop("checked", true);
                }
                else {
                    $("#chkIsEnabled").prop("checked", false);
                }
                $("#txtphone").val(result.phone);                       
                $('[name=DDLAgentType]').val(result.agentTypeId);                
                $("#btnSubmit").val("Update");
                $("#loader").hide();    
                $("#divDetails").show();
            } else {
                $("#loader").hide();
                $("#divDetails").show();
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#loader").hide();
            $("#divDetails").show();
        }
    })

}

function Clear() { 
    $("#txtFirstName").val("");
    $("#txtLastName").val("");
    $("#txtphone").val("");
    $("#txtpassword").val("");
    $("#txtEmail").val("");
    $("#DDLAgentType").val("0");
    $('input[name="chkIsEnabled"]')[0].checked = false;
}

function LoadAgentData(searchfield, searchterm) {

    $("#btnSearch").hide();
    $("#loader").show();
    $("#spanAgentResults").html("");
    var searchRequest = {
        "searchterm": searchterm,
        "searchfield": searchfield
    };

    var url = "/api/Agent/GetAllAgentsList";
    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(searchRequest),
        success: function (result) {
            BuildAccountTable(result);
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function BuildAccountTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spanAgentResults").html(table);
    $("#btnSearch").show();
    $("#loader").hide();
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");

    var text1 = "<span class='spanHidetitle'>Name</span>";
    var text2 = document.createTextNode("Email");
    var text3 = document.createTextNode("Phone");
    var text4 = document.createTextNode("Agent Type");
    var text5 = document.createTextNode("Status");
    var text6 = document.createTextNode("Action");

    td1.innerHTML = text1;
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
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

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");

    var _editIdLink = '<div class="col-md-12"> <a href="/userdetails.html?aid=' + rowData.agentId + '&isedit=true" >Edit</a><a href="/userdetails.html?aid=' + rowData.agentId + '&isedit=false" style="float:right">View</a></div>';
    // var _viewIdLink = '<a href="/accountdetails.html?aid=' + rowData.accountId + '">View</a>';

    var _address = rowData.address ? rowData.address + ", " : "" + rowData.city;
    var _fullname = rowData.firstName + " " + rowData.lastName;
    var _agentType = rowData.agentTypeName;
    var _email = "";

    var email = rowData.email.split(";");
    var stremail = "";
    for (var i = 0; i < email.length; i++) {
        _email += email[i] + "<br />"
    }
    var _phone = rowData.phone;
    var _status = "De-Active";
    if (rowData.status == true) {
        _status = "Active";
    }
    else {
        _status = "De-Active";
    }
    var text1 = "<span class='snapTitle' style ='display:none'><b>Name:</b></span> " + _fullname;
    var text2 = "<span class='snapTitle' style ='display:none'><b>Email:</b></span> " + _email;
    var text3 = "<span class='snapTitle' style ='display:none'><b>Phone:</b></span> " + _phone;
    var text4 = "<span class='snapTitle' style ='display:none'><b>agent Type:</b></span> " + _agentType;
    var text5 = "<span class='snapTitle' style ='display:none'><b>Status:</b></span> " + _status;

    td1.innerHTML = text1;
    td2.innerHTML = text2;
    td3.innerHTML = text3;
    td4.innerHTML = text4;
    td5.innerHTML = text5;
    td6.innerHTML = _editIdLink;
    //td7.innerHTML = _viewIdLink;

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    //tr.appendChild(td7);

    return tr;
}

function CancelAgent() {
    window.location.replace("/user.html");
}

function validateEmail(emailField) {
    if (emailField.value == "") {
        strmsg = "Enter Email Address";
        $("#msg").html(strmsg);
        $("#errorMessage").show();
        $("#btnSubmit").show();
        return false;
    }
    var reg = /^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/;

    if (reg.test(emailField.value) == false) {
        strmsg = "Invalid Email Address";
        $("#msg").html(strmsg);
        $("#errorMessage").show();
        $("#btnSubmit").show();
        return false;
    }
    else {
        $("#errorMessage").hide();
        $("#btnSubmit").show();
    }
    return true;
}

function CreateUserEntry() {
    window.location.href = "userdetails.html";
    IsList = false;
}