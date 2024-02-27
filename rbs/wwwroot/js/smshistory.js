var agentType;
var FromPhone;
var strChat = "";
var tid = 0;
var msgId = null;
var chatObj = [];
var CurrentChatId = null;
var isLoadData = true;
var hdnMasgId = null;
var agentTypeIds = null;
var userName = null;
var mchatId = 0;

function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}
$(document).ready(function () {
    ChcekCookie();
    setAgentType();
    var url = window.location.href;
    agentTypeIds = null;

    agentTypeIds = getParameterByName("type");

    $("#spanClockinout").load("/clockinout.html");

    $("#loader").show();

    $("#loading").show();
    $("#msgs").hide();

    if (agentType === "70" || agentType === "60" || agentType === "30") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }
    GetLetestSMSForAgent();
    var windowHeight = $(window).height() - 55;
    $("#bottom-component1").css('height', windowHeight);

    if (window.innerWidth <= 767) {
        $("#msgHistory").removeClass("msgHistory")
    }
    else {
        $("#msgHistory").addClass("msgHistory")
    }
});

$(window).scroll(function () {
    if ($(window).scrollTop() ==
        $(document).height() - $(window).height()) {

        var lastmid = $(".getMSG").last().find(".time_date").attr("data-id");
        //$("#divTop")
        GetLetestSMSForAgent(lastmid);
    }
});
//techId= current chat user id
function GetLetestSMSForAgent(messageId = null) {  
 
    var techId = getParameterByName("aid");
    var mid = 0;
    if (messageId == null || messageId == 0) {
        mid = 0;
    }
    else {
        mid = $(".getMSG").last().find(".time_date").attr("data-id");
    }
    var agentTypeId = getParameterByName("type");
    var agentId = getCookie("agentId");
    var url = "/api/SMS/GetAllSmsHistorybyAgent/" + techId + "/" + agentId + "/" + agentTypeId + "/" + mid;

    //var url = "/api/SMS/GetAllSmsHistorybyAgent/" + 111 + "/" + 103 + "/" + 20 + "/" + 0 + "/" + 20;
   
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {          
            if (result.length > 0) {
                for (var i = 0; i < result.length; i++) {
                    strChat = "";
                    if (i === 0) {
                        FromPhone = result[0].agentPhone;
                    }
                    if (result[i].id < mid || mid==0) {
                        appendMessage(result[i]);
                        msgId = result[i].id;
                        mid = result[i].id;
                    }
                    
                }
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();
                $("#loadingHistory").hide();
            }
            else {

                if (messageId == null) {
                    $("#mess").html("<h1> No messages to load for this agent.");
                }
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();
                $("#loadingHistory").hide();
            }
        },
        Error: function (error) {
            $("#loading").hide();
            $("#msgs").show();
            $("#loader").hide();
            $("#loadingHistory").hide();
        }
    });
}

function BuildAgents(data, isLoads) {

    //$("#spanChatList").html("");
    var chat = "";
    chatObj = [];
    for (var i = 0; i < data.length; i++) {
        var str;

        var sentDate = DateFormat(data[i].dateSent);
        var uname = data[i].firstName != null ? data[i].firstName + ' ' + data[i].lastName : data[i].fromPhone;
        uname = uname.match(/[^&,]+/g).join('@');
        uname = uname.match(/[^ ,]+/g).join('$');

        if (data[i].isread == null && data[i].toPhone === FromPhone) {
            if (window.innerWidth <= 767) {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetChatDataForMobile('" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            else {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }

            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt=''> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                str += "<h5 style=font-weight:300>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
                str += "<h6 style=font-weight:300;color:#464646;>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h6>";
            }
            else {
                str += "<h5 style=font-weight:300>" + data[i].fromPhone + "<span class='chat_date'></span></h5>";
            }
            str += "<span class='time_date'>" + sentDate + " </span>";
            str += "</div></div></div></a>";
        }
        else if (data[i].isread == null && data[i].toPhone !== FromPhone) {
            if (window.innerWidth <= 767) {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetChatDataForMobile(" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            else {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt=''> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                str += "<h5 style=font-weight:300>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
                str += "<h6 style=font-weight:300;color:#464646;>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h6>";
            }
            else {
                str += "<h5 style=font-weight:300>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h5>";
            }
            str += "<span class='time_date'>" + sentDate + " </span>";
            str += "</div></div></div></a>";
        }
        else {

            if (window.innerWidth <= 767) {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetChatDataForMobile(" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            else {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + "); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            str += "<div class='chat_people' >";
            str += "<div class='chat_img'> <img src='https://ptetutorials.com/images/user-profile.png' alt=''> </div>";
            str += "<div class='chat_ib'>";
            if (data[i].firstName != null) {
                if (data[i].agentId != CurrentChatId) {
                    str += "<h5 style=font-weight:600>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
                    str += "<h6 style=font-weight:300;color:#464646;>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h6>";
                }
                else {
                    str += "<h5 style=font-weight:300>" + data[i].firstName + ' ' + data[i].lastName + "<span class='chat_date'></span></h5>";
                    str += "<h6 style=font-weight:300;color:#464646;>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h6>";
                }
            }
            else {
                if (data[i].agentId != CurrentChatId) {
                    str += "<h5 style=font-weight:600>" + data[i].fromPhone + "<span class='chat_date'></span></h5>";
                    str += "<h6 style=font-weight:300;color:#464646;>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h6>";
                }
                else {
                    str += "<h5 style=font-weight:300>" + data[i].chatAgentPhone + "<span class='chat_date'></span></h5>";
                }
            }
            str += "<span class='time_date'>" + sentDate + " </span>";
            str += "</div></div></div></a>";
        }
        chat += str;

        var chatObject = new Object();
        chatObject.agentSentBy = data[i].agentSentBy;

        chatObject.id = data[i].id;
        chatObject.agentId = data[i].agentId;

        chatObj.push(chatObject);

    }
    $("#spanChatList").html("");
    $("#spanChatList").html(chat);

    if (window.innerWidth <= 767) {
        ///////
        tid = getParameterByName("tid");

    } else {
        tid = chatObj[0].agentId;
    }

    if (CurrentChatId === null) {
        CurrentChatId = chatObj[0].agentId;
    }
    if (isLoads === true) {
        msgId = 0;
        isLoadData = false;
    }
    else {
        msgId = chatObj[0].id;
    }
    hdnMasgId = "hdn_" + msgId;

    if (CurrentChatId != null && CurrentChatId != undefined && CurrentChatId > 0) {
        $("#div_" + CurrentChatId).css("background", "#ebebeb");
    }

    GetLetestSMSForAgent(tid, msgId);
}


function appendMessage(data) {
    // append message here
    var body_replace;
    if (typeof (data.body) !== "undefined") {

        body_replace = data.body.replace(/(?:\r\n|\r|\n)/g, "<br>");

        body_replace = body_replace.replace(/2%2/g, "'");
        var toName = "";
        if (data.agentName != null) {
            toName = data.agentName;
        }
        else {
            toName = data.fromPhone;
        }
        var content = body_replace + "<br/>";
        var divId = $(".getMSG").last().find(".time_date").attr("data-id");

        if (divId == "undefined" || divId == null) {
            divId = 0;
        }

        var dateSent = new Date(data.dateSent);
        //dateSent = dateSent.toLocaleTimeString();
        var SMSDate = dateSent.toDateString();
        var SMSTime = dateSent.toLocaleTimeString();

        var currentdate = new Date();
        var currentday = currentdate.toLocaleDateString('en-US', { weekday: 'long' });
        var CurrentDate = currentdate.toDateString();
        var DateDay = dateSent.toLocaleDateString('en-US', { weekday: 'long' });

        if (currentday == DateDay && CurrentDate == SMSDate) {
            dateSent = "Today " + SMSTime;
        }
        else {
            dateSent = SMSDate + " " + SMSTime;
        }

        //var imgUrl = data.fileUrl.replace("data:image/jpg;base64,", "");
        if (data.id < divId || divId==0) {
            var _cont = "";
            if (data.fromPhone === FromPhone) {
                strChat += "<div class='getMSG'> <div class='outgoing_msg'><div class='sent_msg'>";

                if (data.fileUrl != null && data.fileUrl != "") {
                    strChat += "<div class='col-md-12'>";
                    strChat += "<a href='javascript:openImage(\"" + data.fileUrl + "\")' targer='_blank'><img src=" + data.fileUrl + " style='height:200px;width:200px;float:right;margin-left:10px'/></a>";
                    strChat += "</div>";
                }
                strChat += "<div class='col-md-12'>";
                strChat += "<div class='sent_msg'>";
                _cont = (content != null && content != "") ? unescape(content) : "";
                strChat += "<p>" + _cont + "<p style=\"text-align:right;border-radius:0px 3px 0px 3px !important;margin-top:-2px\"> -- " + data.agentName + " | " + dateSent + "<span  class='time_date' data-id=" + data.id + "></span></p> </div></div></div>";
                strChat += "</div></div>";
            }
            else {
                if (data.fromPhone === "+1" + FromPhone) {
                    strChat += "<div class='getMSG'><div class='outgoing_msg'><div class='sent_msg'>";
                    _cont = (content != null && content != "") ? unescape(content) : "";
                    strChat += "<p>" + _cont + "<p style=\"text-align:right;border-radius:0px 3px 3px 0px !important;margin-top:-2px \"> -- " + data.agentName + " | " + dateSent + "<span  class='time_date' data-id=" + data.id + "></span></p> </div></div></div>";
                } else {
                    strChat += "<div class='getMSG' style=\"padding-bottom:20px\">";


                    if (data.fileUrl != null && data.fileUrl != "") {
                        strChat += "<div class='col-md-12'>";
                        strChat += "<a href='javascript:openImage(\"" + data.fileUrl + "\")' targer='_blank'><img src=" + data.fileUrl + " style='height:200px;width:200px;float:left;margin-left:10px'/></a>";
                        strChat += "</div>";
                    }
                    strChat += "<div class='col-md-12' style='padding-bottom:25px;'>";
                    strChat += "<div class='received_msg'>";
                    strChat += "<div class='received_withd_msg'>";
                    _cont = (content != null && content != "") ? unescape(content) : "";
                    strChat += "<p>" + _cont + "</p><p style=\"text-align:right;border-radius:0px 3px 3px 0px !important;margin-top:-2px\"> -- " + data.firstName + " | " + dateSent + "<span style=\"color:#fff\" class='time_date' data-id=" + data.id + "></span></p> </div></div></div></div>";
                }
            }

        }

        $("#mess").append(strChat);
        var d = $('#msgs');
        d.scrollTop(d.prop("scrollHeight") + 200);
        msgId = data.id;

    }
};