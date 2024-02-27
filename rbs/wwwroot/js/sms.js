/// <reference path="util.js" />
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
var currentchatuserid = null;
var username = null;
function setAgentType() {
    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null)
        agentType = agent.AgentTypeId;
}

$(window).resize(function () {
    DeviceView();
});
function DeviceView() {
    agentTypeIds = getParameterByName("type");
    var version = Math.random();
    if (window.innerWidth > 767) {
        window.location.href = "sms.html?v=" + version + "&type=" + agentTypeIds;
    }
    else {
        window.location.href = "msms.html?v=" + version + "&type=" + agentTypeIds;
    }
}

$(document).ready(function () {

    ChcekCookie();
    setAgentType();
    var url = window.location.href;
    agentTypeIds = null;

    agentTypeIds = getParameterByName("type");

    $("#spanClockinout").load("/clockinout.html");

    $("#btnSendSMS").hide();
    $("#fileImageupload").hide();
    $("#loader").show();

    $("#loading").show();
    $("#msgs").hide();


    if (agentType === "70" || agentType === "60" || agentType === "30") {
        $("#menus").show();
    }
    else {
        $("#menus").hide();
    }

    LoadTechs();

    var loadTech = window.setInterval(LoadTechs, 30000);
    //var intervalID = window.setInterval(SMSText, 40000);

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
    $("#btnViewHistory").click(function () {
        ViewHistory();
    });
});
window.onload = function () {
    if (agentTypeIds == 50) {
        document.title = 'PROLINK CRM | Tech-SMS';
        //$("#menuSMS").html("Tech-SMS <i id='smsNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i>");
    }
    if (agentTypeIds == 20) {
        document.title = 'PROLINK CRM | Rep-SMS';        
    }
    if (agentTypeIds == 10) {
        document.title = 'PROLINK CRM | Lead-SMS';        
    }
}

function LoadTechs() {

    $.ajax({
        url: "/api/SMS/GetSMSLHS/" + agentTypeIds,
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            
            if (data === "undefined") {
                $("#mess").html("<h1> No agent to load for Lead.")
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();                
                //$("#btnSendSMS").show();
                return;
            }
            else if (data.length == 0) {
                $("#mess").html("<h1> No any chat found.")
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();
                return;
            }

            agents = data;            
            if (data.length > 0) {
                if (FromPhone !== null) {
                    FromPhone = data[0].agentPhone;
                }
                BuildAgents(data, isLoadData);
            }
            
            $("#btnDispatchTech").show();
            $("#loading").hide();
            $("#msgs").show();
            $("#btnSendSMS").show();
            $("#fileImageupload").show();
            $("#loader").hide();
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
            $("#btnDispatchTech").show();
            $("#loading").hide();
            $("#msgs").show();
            $("#btnSendSMS").show();
            $("#fileImageupload").show();
            $("#loader").hide();
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
        
        var uname = data[i].firstName != null ? data[i].firstName.trim() + ' ' + data[i].lastName.trim() : data[i].fromPhone;
        uname = uname.match(/[^&,]+/g).join('@');
        uname = uname.match(/[^ ,]+/g).join('$');        
        if (data[i].isread == null && data[i].toPhone === FromPhone) {
            if (window.innerWidth <= 767) {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetChatDataForMobile('" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
            }
            else {
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
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
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
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
                str = "<a href='#' id=a_" + data[i].agentSentBy + "  onclick=GetAllSMSForAgent(" + data[i].agentId + ",\"" + uname + "\"); > <div id=div_" + data[i].agentId + " class='chat_list'>";
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
        var uname1 = data[0].firstName != null ? data[0].firstName.trim() + ' ' + data[0].lastName.trim() : data[0].fromPhone.trim();
        uname1 = uname1.match(/[^&,]+/g).join('@');
        username = uname1.match(/[^ ,]+/g).join('$');   
    }
    $("#spanChatList").html("");
    $("#spanChatList").html(chat);

    if (window.innerWidth <= 767) {        
        tid = getParameterByName("tid");
        currentchatuserid = getParameterByName("tid");
    } else {        
        tid = chatObj[0].agentId;
        currentchatuserid = chatObj[0].agentId;     
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

function GetLetestSMSForAgent(techId, messageId = null) {

    var mid = 0;
    if (messageId == null || messageId == 0) {
        mid = 0;
    }
    else {
        mid = $(".getMSG").last().find(".time_date").attr("data-id");
    }
    //var mid = messageId;
    if (CurrentChatId != null && window.innerWidth > 767) {
        techId = CurrentChatId;
        currentchatuserid = CurrentChatId;
    }
    //var url = window.location.href;

    var agentId = getCookie("agentId");
    var url = "/api/SMS/GetLetest/" + techId + "/" + agentId + "/" + agentTypeIds + "/" + mid + "/" + agentTypeIds;
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {

            if (result.length > 0) {

                //$("#mess").html("");
                //strChat = "";
                for (var i = 0; i < result.length; i++) {
                    strChat = "";
                    if (i === 0) {
                        FromPhone = result[0].agentPhone;
                    }
                    if (result[i].id > mid) {
                        appendMessage(result[i]);
                        msgId = result[i].id;
                    }
                    mid = result[i].id;
                }
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
                var d = $('#msgs');
                d.scrollTop(d.prop("scrollHeight"));
            }
            else {

                if (messageId == null) {
                    $("#mess").html("<h1> No messages to load for this agent.");
                }
                $("#loading").hide();
                $("#msgs").show();
                $("#loader").hide();
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
            }
            if (techId !== "") {
                for (var divid = 0; divid < chatObj.length; divid++) {
                    var id = chatObj[divid].agentSentBy;
                    //$("#div_" + id).css("background", "white");

                    //if (divid === 0) {
                    //    $("#div_" + techId).css("background", "#ebebeb");
                    //}
                }
                tid = techId;
                mchatId = techId;
            }
        },
        Error: function (error) {
            $("#loading").hide();
            $("#msgs").show();
            $("#loader").hide();
            $("#btnSendSMS").show();
            $("#fileImageupload").show();
            //alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }

    });


}

function GetAllSMSForAgent(techId, uname) {
        
    username = uname;
    $(".chat_list").css("background", "white");

    if (techId != null && techId != undefined && techId > 0) {
        $("#div_" + techId).css("background", "#ebebeb");
        $("#div_" + techId).find("h5").css("font-weight", "300");
    }

    $("#loading").show();
    $("#msgs").hide();
    $("#mess").html("");
    $("#errorMessage").hide();
    $("#spanimagevalidation").css('display', 'none');
    $("#txtSmsToSend").val('');
    CurrentChatId = techId;

    GetLetestSMSForAgent(techId, null);
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
        if (data.id > divId) {
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

function SendSMS() {

    $("#btnSendSMS").hide();
    $("#fileImageupload").hide();
    $("#loader").show();
    var agentId = getCookie("agentId");
    $("#errorMessage").hide();

    var furl = $("#fileid")[0].files[0];

    var msgBody = $("#txtSmsToSend").val();

    var message = $("#txtSmsToSend").val();


    if (furl === undefined || furl == null || furl == "") {
        sms = {
            "agentSentBy": agentId, //tech we are SMS
            "Body": $("#txtSmsToSend").val(), //SMS body
            "agentId": CurrentChatId,
            "SmsMessageSid": msgId,
            "AgentTypeId": agentTypeIds,
        };
        $.ajax({
            method: "PUT",
            contentType: "application/json",
            url: "/api/SMS/SendSMS/",
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
                    LoadTechs();
                    $("#txtSmsToSend").val('');
                }
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
                $("#loader").hide();

            },
            Error: function (error) {
                alert(JSON.stringify(error));
                console.log(JSON.stringify(error));
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
                $("#loader").hide();
            }
        });
    } else {
        var formData = new FormData();
        formData.append("agentSentBy", agentId);
        formData.append("Body", msgBody); //SMS body
        formData.append("SmsMessageSid", msgId);
        formData.append("agentId", CurrentChatId);
        formData.append("AgentTypeId", agentTypeIds);
        formData.append("file", furl);

        $.ajax({
            method: "PUT",
            //contentType: "application/json",
            url: "/api/SMS/SendSMSWithAttechment/",
            data: formData,
            processData: false,  // tell jQuery not to process the data
            contentType: false,  // tell jQuery not to set contentType
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

                    LoadTechs();
                    //GetLetestSMSForAgent(tid, msgId);
                    $("#txtSmsToSend").val('');
                    $("#fileid").val('');
                }
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
                $("#loader").hide();
                $("#spanimagevalidation").css('display', 'none');
                $("#spanimagevalidation").html('');

            },
            Error: function (error) {
                alert(JSON.stringify(error));
                console.log(JSON.stringify(error));
                $("#btnSendSMS").show();
                $("#fileImageupload").show();
                $("#loader").hide();
                $("#spanimagevalidation").html('');
            }
        });
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

function openImage(base64URL) {
    //window.open(base64URL);
    var win = window.open();
    win.document.write('<iframe src="' + base64URL + '" frameborder="0" style="border:0; top:0px; left:0px; bottom:0px; right:0px; width:100%; height:100%;" allowfullscreen></iframe>');
}

function GetChatDataForMobile(id, uname) {
    type = getParameterByName("type");
    username = uname;
    
    window.location.replace("/mchat.html?v="+Math.random()+"&type=" + type + "&uname=" + uname + "&tid=" + id);
    //GetAllSMSForAgent(id);    
    
}

function validateFileType() {

    var fileName = document.getElementById("fileid").value;
    var idxDot = fileName.lastIndexOf(".") + 1;
    var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
    if (extFile == "jpg" || extFile == "jpeg" || extFile == "png") {
        $("#spanimagevalidation").css('display', 'block');
        $("#spanimagevalidation").css('color', 'green');
        $("#spanimagevalidation").html(fileName)
    } else {
        $("#spanimagevalidation").css('display', 'block');
        $("#spanimagevalidation").css('color', '#ff0000');
        $("#spanimagevalidation").html("Allowed only .jpg,.jpeg,.png files.");
        $("#errorMessage").hide();
        return false;
    }
}
function SetPage() {
    $('.msg_history').css('height', '100%');
    $('#divBottom').css('height', '100%');
    var windowHeight = $(window).height() - 55;
    $("#split-pane-1").css('height', windowHeight);
    $("#splitPanel").css('height', windowHeight);
    $("#top-component").css('height', windowHeight);
    $("#bottom-component").css('height', windowHeight);
    $("#bottom-component2").css('height', '11.5em');

    windowHeight = $(window).height();

    var topmHeight = $('#divTop').height();
    var MsgSendBoxHeight = $('#divBottom').height();
    var Height = 100;

    $('#divTop').css('max-height', windowHeight - MsgSendBoxHeight);
    Height = windowHeight - MsgSendBoxHeight;
    if (Height > 475) {
        $('.inbox_chat').css('max-height', windowHeight - 60);
        $('.inbox_chat').css('height', windowHeight - 60);
    }
    else {
        $('.inbox_chat').css('max-height', windowHeight - 60);
        $('.inbox_chat').css('height', windowHeight - 60);
    }
}

function ViewHistory() {       
    
    if (window.innerWidth < 767) {
        var uname = getParameterByName("uname");
        uname = uname.match(/[^$]+/g).join(' ');
        username = uname.match(/[^@]+/g).join(' & ');
    }
    var agenttypeid =  getParameterByName("type");
    window.open("/smshistory.html?v=" + Math.random() +"&aid=" + currentchatuserid + "&type=" + agenttypeid + "&uname=" + username);
}