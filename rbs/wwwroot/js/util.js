function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.search);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function timeConvert(time) {    
    // Check correct time format and split into components
    time = time.toString().match(/^([01]\d|2[0-3])(:)([0-5]\d)(:[0-5]\d)?$/) || [time];

    if (time.length > 1) { // If time format correct
        time = time.slice(1);  // Remove full string match value
        time[5] = +time[0] < 12 ? ' AM' : ' PM'; // Set AM/PM
        time[0] = +time[0] % 12 || 12; // Adjust hours
    }
    return time.join(''); // return adjusted time or original string
}

function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=-99999999;';
}

function checkAgent() {
    var agentid = getCookie("agentId");
    var loggedin = getCookie("loggedIn");

    if (agentid === null && loggedin !== 1) {
        window.location.replace("/index.html");
    }
    else {
        var agent = new Object();
        agent.AgentId = agentid;
        $.ajax({
            url: "/api/Agent/",
            type: "Post",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(agent),
            success: function (data, textStatus, xhr) {
                console.log(data);
                if (data.status === false) {
                    eraseCookie("agentId");
                    eraseCookie("loggedIn");
                    eraseCookie("agent");
                    window.location.replace("/index.html");
                }
            },

            error: function (xhr, textStatus, errorThrown) {
                console.log("Error in Operation");
            }
        });
    }

}

function FormatShortDateTime(strDate) {
    var date = new Date(Date.parse(strDate));
    return date.getMonth() + 1 +
        "/" +
        //date.getDate() + 1 + 
        (date.getDate() + 1) +
        "/" +
        date.getFullYear() +
        " " +
        date.getHours() +
        ":" +
        date.getMinutes() +
        ":" +
        date.getSeconds();
}

function CopyToClipboard(CopyFromInput) {
    var copyText = document.getElementById(CopyFromInput);
    copyText.select();
    document.execCommand("copy");
}

function pasteText(pasteTo) {
    navigator.clipboard.readText()
        .then(text => {
            $(pasteTo).val(text);
            $(pasteTo).focus();
        })
        .catch(err => {
            console.error('Failed to read clipboard contents: ', err);
        });
}

function TimeToDecimal(t) {
    var arr = t.split(':');
    var dec = parseInt((arr[1] / 6) * 10, 10);

    return parseFloat(parseInt(arr[0], 10) + '.' + (dec < 10 ? '0' : '') + dec);
}

function MaximumSADate() {
    var date = new Date();
    var day = date.getDay();
    if (day === 5) {
        date = date.setDate(date.getDate() + 4);
    }
    else if (day === 6) {
        date = date.setDate(date.getDate() + 3);
    }
    else {
        date = date.setDate(date.getDate() + 2);
    }
    return date;
}

function GetGoogleMapsLink(address) {
    return "http://maps.google.com/?q=" + encodeURIComponent(address);
}

function DateFormat(date) {
    var newdate = new Date(date);
    newdate.setDate(newdate.getDate());

    var newtime = timeConvert(date.split("T")[1].split(":")[0] + ":" + date.split("T")[1].split(":")[1]);
    var newdateformat = (newdate.getMonth() + 1) + '-' + (newdate.getDate()) + ' ' + newtime;
    return newdateformat;
}


function ChcekCookie() {
    var agentid = getCookie("agentId");
    var loggedin = getCookie("loggedIn");

    if (agentid === null && loggedin !== 1) {
        window.location.replace("/index.html?v=1.1");
        return false;
    }

    if (document.cookie.indexOf("lastchecked=") >= 0) {
        return true;        
    }
    else {
        checkUserStatus(agentid);
        return false;
    }
}

function ClearCache(cacheBust) {
    //var cacheBust = ['js/util.js', 'js/account.js', 'js/menu.js'];
    for (i = 0; i < cacheBust.length; i++) {
        var el = document.createElement('script');
        el.src = cacheBust[i] + "?v=" + Math.random();
        document.getElementsByTagName('head')[0].appendChild(el);
    }
}

function checkUserStatus(agentId) {
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/Agent/GetAgentStatusById/" + agentId,
        success: function (result) {

            if (result == true) {
                var expireTime = new Date();
                //expireTime.setTime(expireTime.getTime() + (1 * 60 * 1000)); // 1 minute
                expireTime.setTime(expireTime.getTime() + (1 * 60 * 60 * 1000));//1 hours
                var expires = "expires=" + expireTime.toUTCString();
                document.cookie = "lastchecked" + "=" + 1 + ";" + expires + ";path=/";
                return true;
            } else {
                window.location.href = "/index.html?v=1.1";
                return false;
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function GetAccountStatusIdText(accountStatusId) {    
    var statusText = "";
    if (accountStatusId == 55) {
        statusText = "FieldRep Disp";
    }
    else if (accountStatusId == 56) {
        statusText = "Sold (1st QA)";
    }
    else if (accountStatusId == 57) {
        statusText = "Installed (2nd QA)";
    }
    else if (accountStatusId == 58) {
        statusText = "UFS";
    }
    else if (accountStatusId == 59) {
        statusText = "No Show";
    }
    else if (accountStatusId == 60) {
        statusText = "Reschedule";
    }
    else if (accountStatusId == 100) {
        statusText = "Tech Scheduled";
    }
    else if (accountStatusId == 110) {
        statusText = "Tech Dispatched";
    }
    else if (accountStatusId == 120) {
        statusText = "Partial Install";
    }

    return statusText;
}
