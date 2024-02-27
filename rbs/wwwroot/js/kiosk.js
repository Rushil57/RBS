$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");    
    $("#btnSubmit").click(function () {
        Create();
    });    


    $("#btnNext1").click(function () {
        $("#collapse1").addClass("collapsed");
        $("#collapse3").addClass("collapsed");
        $("#collapse4").addClass("collapsed");
        $("#collapse2").removeClass("collapsed");
    });
    $("#btnNext2").click(function () {
        $("#collapse1").addClass("collapsed");
        $("#collapse2").addClass("collapsed");
        $("#collapse4").addClass("collapsed");
        $("#collapse3").removeClass("collapsed");
    });
    $("#btnNext3").click(function () {
        $("#collapse1").addClass("collapsed");
        $("#collapse2").addClass("collapsed");
        $("#collapse3").addClass("collapsed");
        $("#collapse4").removeClass("collapsed");
    });

    $("#btnPrev1").click(function () {

        $("#collapse2").addClass("collapsed");
        $("#collapse1").removeClass("collapsed");
    });
    $("#btnPrev2").click(function () {
        $("#collapse3").addClass("collapsed");
        $("#collapse2").removeClass("collapsed");
    });   
    LoadAllAgentTypre();
});
function Create() {

    $("#successMsg").hide();
    $("#errorMessage").hide();
    var strmsg = "";
    var agent = new Object();
    
    var otherInfo = new Object();
    otherInfo.EmergencyContact = $("#txtEmergencyContact").val();
    otherInfo.BankInfo = $("#txtBankInfo").val();
    otherInfo.TaxInfo = $("#txtTaxInfo").val();
    otherInfo.HatSize = $("#txtHatSize").val();
    otherInfo.WaistSize = $("#txtWaistSize").val();
    
    var OtherInfo = JSON.stringify(otherInfo);
    agent.OtherInfo = OtherInfo;
    agent.FirstName = $("#txtfirstname").val();
    agent.LastName = $("#txtlastname").val();
    agent.Phone = $("#txtphone").val();
    agent.Email = $("#txtemail").val();
    agent.AgentTypeId = $("#DDLAgentType").val();
    
    var url = "/api/Agent/InsertUserOtherInfo";
    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(agent),
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

function LoadAllAgentTypre() {
    
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
            }
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}