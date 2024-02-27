/// <reference path="util.js" />
var agentType;
var FromPhone;
var strChat = "";
var tid = 0;
var msgId = null;
var chatObj = [];
var CurrentChatId = null;
var isLoadPage = true;
var isFirstCall = true;
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}

$(document).ready(function () {
    setAgentType();
    $("#spanClockinout").load("/clockinout.html");

    $("#btnSendSMS").hide();
    $("#loader").show();

    $("#loading").show();
    $("#msgs").hide();

    if (agentType === "70" || agentType === "60") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }

    LoadTechs();
   
    //Temp
    //var loadTech = window.setInterval(LoadTechs, 3000000);
    //var intervalID = window.setInterval(SMSText, 5000000);

    var loadTech = window.setInterval(LoadTechs, 10000);
    var intervalID = window.setInterval(SMSText, 10000);

    $("#btnSendSMS").click(function () {

        $("#msg").html("");
        $("#errorMessage").hide();

        if (tid === 0 || typeof (tid) === "undefined") {
            $("#msg").html("Select an agent first to view messages.");
            $("#bottom-component2").css('height', '16.2em !important');
            $("#errorMessage").show();
        }
        else if ($("#txtSmsToSend").val() === "") {
            $("#msg").html("Type a message to send.");
            $("#errorMessage").show();
            $("#bottom-component2").css('height', '16.2em !important');
        }
        else {
            $("#btnSendSMS").hide();
            $("#loader").show();

            SendSMS();
        }
    });

});

function LoadTechs() {

    var loginAgentId = getCookie("agentId");
    $.ajax({
        //url: "/api/Agent/50", //tech is agentType50
        url: "/api/SMS/GetSMSLHS/" + loginAgentId,
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {

            agents = data;
            //Temp             
            if (FromPhone !== null) {
                FromPhone = data[0].agentPhone;
            }
            BuildAgents(data, isLoadPage);

            $("#btnDispatchTech").show();
            $("#btnSendSMS").show();
            $("#loader").hide();
            $("#loading").hide();
            $("#msgs").show();


            //if (getParameterByName("aid") !== null) {
            //    //$("#DDLTechs").val(getParameterByName("aid"));
            //    tid = getParameterByName("aid");
            //    GetSMSForAgent(tid);
            //}
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }

    });

}

function GetAllSMSForAgent(techId) {
    isLoadPage = true;
    $("#loading").show();
    $("#msgs").hide();

    CurrentChatId = techId;
    GetLetestSMSForAgent(techId);
}


function GetSMSForAgent(techId, messageId = null) {
    var mid = null;
    if (messageId == null) {
        mid = 0;
    }
    else {
        mid = messageId;
    }
    if (CurrentChatId != null) {
        techId = CurrentChatId;
    }
    if (isLoadPage === true) {
        mid = 0;
    }

    var agentId = getCookie("agentId");
    var url = "/api/SMS/GetLetest/" + techId + "/" + agentId + "/" + mid;
    //var url = "/api/SMS/";    
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            if (result.length > 0) {
                //$("#mess").html("");
               
                for (var i = 0; i < result.length; i++) {
                    strChat = "";
                    if (i === 0) {
                        FromPhone = result[i].agentPhone;
                    }
                    if (result[i].id > mid ) {
                        BuildSMSChat(result[i]);
                    }
                    
                    isFirstCall = false;
                }
            }
            else {
                if (messageId == null) {
                    $("#mess").html("<h1> No messages to load for this agent.");
                }
            }
            if (techId !== "") {
                for (var divid = 0; divid < chatObj.length; divid++) {
                    var id = chatObj[divid].agentSentBy;
                    $("#div_" + id).css("background", "white");
                }
                tid = techId;
                $("#div_" + techId).css("background", "#ebebeb");
            }
            //var d = $('#msgs');
            //d.scrollTop(d.prop("scrollHeight"));
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });

}

function GetLetestSMSForAgent(techId, messageId = null) {
    if (messageId == null) {
        messageId = 0;
    }
    if (CurrentChatId != null) {
        techId = CurrentChatId;
    }
    var agentId = getCookie("agentId");
    var url = "/api/SMS/GetLetest/" + techId + "/" + agentId + "/" + messageId;
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            if (result.length > 0) {

               
                strChat = "";
                for (var i = 0; i < result.length; i++) {

                    if (i === 0) {
                        FromPhone = result[i].agentPhone;
                    }
                    if (result[i].id > mid) {
                        BuildSMSChat(result[i]);
                    }
                    $("#loading").hide();
                    $("#msgs").show();
                    var d = $('#msgs');
                    d.scrollTop(d.prop("scrollHeight"));
                }
            }
            else {
                $("#loading").hide();
                $("#msgs").show();
                if (messageId == null) {
                    $("#mess").html("<h1> No messages to load for this agent.");
                }
            }
            if (techId !== "") {
                for (var divid = 0; divid < chatObj.length; divid++) {
                    var id = chatObj[divid].agentSentBy;
                    $("#div_" + id).css("background", "white");
                }
                tid = techId;
                $("#div_" + techId).css("background", "#ebebeb");
            }

            //var d = $('#msgs');
            //d.scrollTop(d.prop("scrollHeight"));
        },
        Error: function (error) {
            $("#loading").hide();
            $("#msgs").show();
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}



function SendSMS() {
    var agentId = getCookie("agentId");
    $("#errorMessage").hide();
    var url = "/api/SMS/";
    var sms = {
        "agentSentBy": tid, //tech we are SMS
        "Body": $("#txtSmsToSend").val() //SMS body
    };
    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(sms),
        success: function (result) {
            console.log(result);
            if (!result) {
                $("#msg").html("Uh, oh.  Something didn't work.  Better call Spencer");
                $("#errorMessage").show();
            }
            else {
                if (msgId == null) {
                    msgId = 0;
                }

                GetSMSForAgent(tid, msgId);
            }

            $("#txtSmsToSend").val('');
            $("#btnSendSMS").show();
            $("#loader").hide();
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });

}

function BuildSMSChat(data) {

    var body_replace;
    if (typeof (data.body) !== "undefined") {
        body_replace = data.body.replace(/(?:\r\n|\r|\n)/g, "<br>");

        var toName = "";
        if (data.agentName != null) {
            toName = data.agentName;
        }
        else {
            toName = data.fromPhone;
        }        
        var content = body_replace + "<br/>";

        if (data.fromPhone === FromPhone) {
            strChat += "<div class='outgoing_msg'><div class='sent_msg'>";
            strChat += "<p>" + decodeURI(content) + "<p style=\"text-align:right;border-radius:0px 3px 0px 3px !important;margin-top:-2px\"> --" + data.agentName + "</p><span class='time_date'></span></div></div>";
        }
        else {
            if (data.fromPhone === "+1" + FromPhone) {
                strChat += "<div class='outgoing_msg'><div class='sent_msg'>";
                strChat += "<p>" + decodeURI(content) + "<p style=\"text-align:right;border-radius:0px 3px 3px 0px !important;margin-top:-2px \"> --" + data.agentName + "</p><span class='time_date'></span></div></div>";
            } else {
                strChat += "<div class='incoming_msg'>";
                strChat += "<div class='received_msg'>";
                strChat += "<div class='received_withd_msg'>";
                strChat += "<p>" + decodeURI(content) + "</p><p style=\"text-align:right;border-radius:0px 3px 3px 0px !important;margin-top:-2px\"> --" + data.firstName + "</p> <span class='time_date'></span></div></div></div>";                
            }
        }        
        if (msgId == null) {
            $("#mess").html(strChat);
            var d = $('#msgs');
            d.scrollTop(d.prop("scrollHeight") + 200);
        }
        else {
            if (data.id > msgId) {
                $("#mess").append(strChat);
                msgId = null;
                var d = $('#msgs');
                d.scrollTop(d.prop("scrollHeight") + 200);
            }
        }
    }
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");
    var td1 = document.createElement("td");

    //https://codepen.io/Founts/pen/gmhcl
    var body_replace = rowData.body.replace(/(?:\r\n|\r|\n)/g, "<br>");
    _to = rowData.toPhone === FromPhone ? "" : "--" + rowData.firstName;
    var content = "body: " + body_replace + "<br/>" + _to;
    console.log("content : " + content);
    var bubble;
    if (rowData.fromPhone === FromPhone) {
        bubble = `<div class="talk-bubble tri-right round border left-top">
        <div class="talktext">
        <p>`+ decodeURI(content) + `</p></div></div>`;
    }
    else {
        if (rowData.fromPhone === "+1" + FromPhone) {
            bubble = `<div class="talk-bubble tri-right round border left-top">
        <div class="talktext">
        <p>`+ decodeURI(content) + `</p></div></div>`;
        }
        else {
            bubble = `<div class="talk-bubble tri-right round border right-top">
        <div class="talktext">
        <p>`+ decodeURI(content) + `</p></div></div>`;
            $(td1).css("text-align", "right");
        }
    }
    $(td1).css("border", "0px");
    $(td1).css("padding", "0");
    td1.innerHTML = bubble;
    tr.appendChild(td1);

    return tr;
}

function GetLatestSMS() {
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/SMS",
        success: function (result) {
            console.log("messages : " + JSON.stringify(result));          
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildAgents(data, isLoads) {
    $("#spanChatList").html("");
    var chat = "";
    chatObj = [];
    for (var i = 0; i < data.length; i++) {
        var str;

        if (data[i].isread == null && data[i].toPhone === FromPhone) {
            str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentSentBy + " class='chat_list'>";
            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt='sunil'> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                str += "<h5 style=font-weight:300>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
            }
            else {
                str += "<h5 style=font-weight:300>" + data[i].fromPhone + "<span class='chat_date'></span></h5>";
            }
            str += "</div></div></div></a>";
            //str += "<p>To " + data[i].toPhone + "</p></div></div></div></a>";
        }
        else if (data[i].isread == null && data[i].toPhone !== FromPhone) {

            str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentSentBy + " class='chat_list'>";
            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt='sunil'> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                str += "<h5 style=font-weight:300>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
            }
            else {
                str += "<h5 style=font-weight:300>" + data[i].fromPhone + "<span class='chat_date'></span></h5>";
            }
            str += "</div></div></div></a>";
            //str += "<p>To " + data[i].toPhone + "</p></div></div></div></a>";
        }
        else {
            str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentSentBy + " class='chat_list'>";
            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt='sunil'> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                str += "<h5 style=font-weight:600>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
            }
            else {
                str += "<h5 style=font-weight:600>" + data[i].fromPhone + "<span class='chat_date'></span></h5>";
            }
            str += "</div></div></div></a>";
        }
        chat += str;

        var chatObject = new Object();
        chatObject.agentSentBy = data[i].agentSentBy;

        chatObject.id = data[i].id;
        chatObject.agentId = data[i].agentId;

        chatObj.push(chatObject);
    }

    $("#spanChatList").html(chat);
    //tid = chatObj[0].agentSentBy;
    tid = chatObj[0].agentId;
    //CurrentChatId = chatObj[0].agentId;
    if (isLoadPage === true) {
        msgId = 0;
        isLoadPage = false;
    }
    else {
        isLoadPage = false;
        msgId = chatObj[0].id;
    }

    GetSMSForAgent(tid, msgId);
    //msgId = null;
}

function SMSText() {
    if (tid !== "") {
        //GetLetestSMSForAgent(tid, msgId);

        GetSMSForAgent(tid, msgId);
    }
}