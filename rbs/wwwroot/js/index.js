var agents = [];

$(document).ready(function () {
    $('#msg').html("");
    $("#add").hide();
    $("#loader").show();

    $("#spanClockinout").load("/clockinout.html?v=1.1");
    var agent = getCookie("agentId");
    var loggedin = getCookie("loggedIn");

    if (agent == null && loggedin != 1) {
        LoadAllAgents();
    } else {
        if (agent != null) {
            window.location.replace("/internalsales.html?v=" + Math.random());
        }
        else {
            if (document.cookie.indexOf("lastchecked=") >= 0) {
                $(".row").hide();
                window.location.replace("/internalsales.html?v=" + Math.random());
            }
            else {
                LoadAllAgents();
            }
        }
    }

    $("#add").click(function (e) {
        e.preventDefault();
        $("#loader").show();
        $("#add").hide();
        LoginAgent();
    });
});

function GetAgentInfo(agentId) {
    for (var i = 0; i < agents.length; i++) {
        if (agentId == agents[i].agentId) {
            return JSON.stringify(agents[i]);
        }
    }
}

function LoginAgent() {
    $("#msg").html("");

    var loginInfo = new Object();
    loginInfo.Email = $("#txtID").val();
    loginInfo.AgentPassword = $("#txtPW").val();
    loginInfo.AgentId = $("#DDLAgents").val().split('|')[0];

    console.log(JSON.stringify(loginInfo));

    $.ajax({
        url: "/api/Login/",
        contentType: "application/json",
        type: "POST",
        data: JSON.stringify(loginInfo),
        success: function (data) {
            if (data === true) {

                if (data.status === false) {
                    $("#msg").html("Your account is suspended, please contact administrator.");
                    $('#successMsg').hide();
                    $('#errorMessage').show();
                }
                else {
                    var agentId = $("#DDLAgents").val().split('|')[0];
                    var agentTypeId = $("#DDLAgents").val().split('|')[1];

                    var agent = new Object();
                    agent.AgentId = agentId;
                    agent.AgentTypeId = agentTypeId;

                    setCookie("agentId", agentId, 365 * 2);
                    setCookie("agent", JSON.stringify(agent), 365 * 2);
                    setCookie("loggedIn", 1, 365 * 2);

                    var expireTime = new Date();
                    //expireTime.setTime(expireTime.getTime() + (1 * 60 * 1000)); // 1 minute
                    expireTime.setTime(expireTime.getTime() + (1 * 60 * 60 * 1000));//1 hours
                    var expires = "expires=" + expireTime.toUTCString();
                    document.cookie = "lastchecked" + "=" + 1 + ";" + expires + ";path=/";

                    $('#successMsg').show();
                    $('#errorMessage').hide();
                    window.location.replace("/internalsales.html?v=" + Math.random());
                    var ChcekCookie = window.setInterval(ChcekCookie, 3600000);
                }
            }
            else {
                $("#msg").html("Email and Password not correct.");
                $('#successMsg').hide();
                $('#errorMessage').show();
            }

            $("#loader").hide();
            $("#add").show();

        },

        error: function (xhr, textStatus, errorThrown) {
            console.log(xhr);
            console.log(textStatus);
            console.log(errorThrown);
            console.log("Error in Operation");
        }
    });
}

function LoadAllAgents() {

    $.ajax({
        url: "/api/Agent/AgentForIndex/",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            $("#add").hide();
            $("#loader").show();
            agents = data;
            var DDLAgents = document.getElementById("DDLAgents");
            $("#DDLAgents option").remove();
            for (var i = 0; i < data.length; i++) {
                DDLAgents.innerHTML = DDLAgents.innerHTML +
                    '<option value="' + data[i].agentId + '|' + data[i].agentTypeId + '">' + data[i].firstName + '</option>';
            }

            $("#add").show();
            $("#loader").hide();

            var agent = getCookie("agentId");

            if (agent != null) {
                if (document.cookie.indexOf("lastchecked=") >= 0) {
                    console.log("Agent Id =" + agent);
                    $("#DDLAgents").val(agent);
                    $("#add").hide();
                }
                else {
                    $("#add").show();
                    $("#loader").hide();
                }
            } else {
                $("#add").show();
            }

            console.log("dropdown loaded.");
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });

}