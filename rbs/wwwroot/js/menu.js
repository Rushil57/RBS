/// <reference path="util.js" />
var agentType;
function setAgentType() {

    var agent = JSON.parse(getCookie("agent"));
    if (agent !== null) {
        agentType = agent.AgentTypeId;
    }
    else {
        // window.location = "/index.html";
        return false;
    }
}

$(function () {

    setAgentType();
    $("#menu").html("");
    var version = Math.random();

    var data;
    if (agentType === "10") { //LeadGen
        data = {
            menu: [
                {
                    name: 'New Lead',
                    link: '/newlead.html?v=' + version,
                    sub: null
                }, {
                    name: 'Lookup',
                    link: '/lookup.html?v=' + version,
                    sub: null
                },
                {
                    name: 'Upload Redfin',
                    link: '/leadupload.html?v=' + version,
                    sub: null
                }]
        };
    }
    else if (agentType === "11") { // LeadGenNewLead
        data = {
            menu: [{
                name: 'Internal Sale',
                link: '/internalsales.html?v=' + version,
                sub: null,
                class: "active"
            }, {
                name: 'New Lead',
                link: '/newlead.html?v=' + version,
                sub: null
            }]
        }
    }
    else if (agentType === "30") { //CSR
        data = {
            menu: [{
                name: 'Internal Sale',
                link: '/internalsales.html?v=' + version,
                sub: null
            }, {
                name: 'Calendar',
                link: '/calendar.html?v=' + version,
                sub: null
            }, {
                name: 'Timeclock',
                link: '/timeclockhistory.html?v=' + version,
                sub: null
            },
            {
                name: 'LeadGen',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Lookup',
                        link: '/lookup.html?v=' + version,
                        sub: null
                    }, {
                        name: 'New Lead',
                        link: '/newlead.html?v=' + version,
                        sub: null
                    },
                    ]
                }
            }, {
                name: 'SMS',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Tech',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=50' : '/msms.html?v=' + version + '&type=50',
                        sub: null
                    },
                    {
                        name: 'FieldRep',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=20' : '/msms.html?v=' + version + '&type=20',
                        sub: null
                    },
                    {
                        name: 'Lead',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=10' : '/msms.html?v=' + version + '&type=10',
                        sub: null
                    }]
                }
            },
            {
                name: 'Account',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Account',
                        link: '/account.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Account Info',
                        link: '/accountinfo.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Account Calendar',
                        link: '/accountcalendar.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Upload File',
                        link: '/uploadfile.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Un-Assigned Notes',
                        link: '/unassigned.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Serv Ticket Disp',
                        link: '/dispatchtech.html?v=' + version,
                        sub: null
                    }]
                }
            },
            {
                name: 'Admin',
                link: '#',
                sub: {
                    sub: [{
                        name: 'User',
                        link: '/user.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Inventory',
                        link: '/inventory.html?v=' + version,
                        sub: null
                    }]
                }
            }]
        };
    }
    else if (agentType === "31") { //CR-reps
        data = {
            menu: [{
                name: 'Account',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Account',
                        link: '/account.html?v=' + version,
                        sub: null
                    }]
                }
            }]
        };
    }
    else if (agentType === "40") { //IS
        data = {
            menu: [{
                name: 'Internal Sale',
                link: '/internalsales.html?v=' + version,
                sub: null
            }, {
                name: 'Calendar',
                link: '/calendar.html?v=' + version,
                sub: null
            }, {
                name: 'Timeclock',
                link: '/timeclockhistory.html?v=' + version,
                sub: null
            }]
        };
    }
    if (agentType === "60") { //Manager
        data = {
            menu: [{
                name: 'Internal Sale',
                link: '/internalsales.html?v=' + version,
                sub: null
            }, {
                name: 'Calendar',
                link: '/calendar.html?v=' + version,
                sub: null
            }, {
                name: 'New Lead',
                link: '/newlead.html?v=' + version,
                sub: null
            }, {
                name: 'SMS',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Tech',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=50' : '/msms.html?v=' + version + '&type=50',
                        sub: null
                    },
                    {
                        name: 'FieldRep',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=20' : '/msms.html?v=' + version + '&type=20',
                        sub: null
                    },
                    {
                        name: 'Lead',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=10' : '/msms.html?v=' + version + '&type=10',
                        sub: null
                    }]
                }
            }, {
                name: 'Manager',
                link: '#',
                sub: {
                    sub: [{
                        name: 'DialReport',
                        link: '/dialreport.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Timeclock',
                        link: '/timeclockhistory.html?v=' + version,
                        sub: null
                    }]
                }
            }, {
                name: 'Admin',
                link: '#',
                sub: {
                    sub: [{
                        name: 'User',
                        link: '/user.html?v=' + version,
                        sub: null
                    }]
                }
            }]
        };
    }
    if (agentType === "70") {
        data = {
            menu: [{
                name: 'Internal Sale',
                link: '/internalsales.html?v=' + version,
                sub: null,
                class: "active"
            }, {
                name: 'Calendar',
                link: '/calendar.html?v=' + version,
                sub: null
            }, {
                name: 'LeadGen',
                link: '#',
                sub: {
                    sub: [
                        {
                            name: 'Lookup',
                            link: '/lookup.html?v=' + version,
                            sub: null
                        }, {
                            name: 'New Lead',
                            link: '/newlead.html?v=' + version,
                            sub: null
                        }, {
                            name: 'Upload Redfin',
                            link: '/leadupload.html?v=' + version,
                            sub: null
                        }]
                }
            }, {
                name: 'SMS',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Tech',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=50' : '/msms.html?v=' + version + '&type=50',
                        sub: null
                    },
                    {
                        name: 'FieldRep',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=20' : '/msms.html?v=' + version + '&type=20',
                        sub: null
                    },
                    {
                        name: 'Lead',
                        link: window.innerWidth > 767 ? '/sms.html?v=' + version + '&type=10' : '/msms.html?v=' + version + '&type=10',
                        sub: null
                    }]
                }
            },
            {
                name: 'Manager',
                link: '#',
                sub: {
                    sub: [{
                        name: 'DialReport',
                        link: '/dialreport.html?v=' + version,
                        sub: null
                    },
                    {
                        name: 'Timeclock',
                        link: '/timeclockhistory.html?v=' + version,
                        sub: null
                    }]
                }
            },
            {
                name: 'Account',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Account',
                        link: '/account.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Account Info',
                        link: '/accountinfo.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Account Calendar',
                        link: '/accountcalendar.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Account Dashboard',
                        link: '/accountdashboard.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Upload File',
                        link: '/uploadfile.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Un-Assigned Notes',
                        link: '/unassigned.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Serv Ticket Disp',
                        link: '/dispatchtech.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Upload Accounts Notes',
                        link: '/uploadnotesfile.html?v=' + version,
                        sub: null
                    }]
                }
            },
            {
                name: 'Admin',
                link: '#',
                sub: {
                    sub: [{
                        name: 'Dashboard',
                        link: '/dashboard.html?v=' + version,
                        sub: null
                    }, {
                        name: 'User',
                        link: '/user.html?v=' + version,
                        sub: null
                    }, {
                        name: 'Inventory',
                        link: '/inventory.html?v=' + version,
                        sub: null
                    }]
                }
            }]
        };
    }

    var getMenuItem = function (itemData) {

        var item = $("<li>")
            .append(
                $("<a>", {
                    href: itemData.link,
                    html: itemData.name
                })
            );


        if (itemData.sub) {
            item = $("<li>")
                .append(itemData.name == "SMS" ? "<a href='" + itemData.link + "' class='dropdown-toggle' data-toggle='dropdown' aria-expanded='false'><span id='menuSMS'>" + itemData.name + "<i id='smsNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></span></a>" : "<a href='" + itemData.link + "' class='dropdown-toggle' data-toggle='dropdown' aria-expanded='false'>" + itemData.name + "</a>")
            var subList = $("<ul  class='dropdown-menu'>");

            $.each(itemData.sub, function (index, submenu) {
                if (itemData.name == "SMS") {
                    if (itemData.sub != null) {
                        if (itemData.sub.sub.length > 2) {
                            subList.append(itemData.sub.sub[0].name == "Tech" ? "<li><a href='" + itemData.sub.sub[0].link + "'>" + itemData.sub.sub[0].name + "<i id='TechNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></a>" : "<a href='" + itemData.sub.sub[0].link + "'>" + itemData.sub.sub[0].name + "</a></li>")
                                .append(itemData.sub.sub[1].name == "FieldRep" ? "<li><a href='" + itemData.sub.sub[1].link + "'>" + itemData.sub.sub[1].name + "<i id='FieldNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></a>" : "<a href='" + itemData.sub.sub[1].link + "'>" + itemData.sub.sub[1].name + "</a></li>")
                                .append(itemData.sub.sub[2].name == "Lead" ? "<li><a href='" + itemData.sub.sub[2].link + "'>" + itemData.sub.sub[2].name + "<i id='LeadNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></a>" : "<a href='" + itemData.sub.sub[2].link + "'>" + itemData.sub.sub[2].name + "</a></li>")
                        }
                        else {
                            subList.append(itemData.sub.sub[0].name == "Tech" ? "<li><a href='" + itemData.sub.sub[0].link + "'>" + itemData.sub.sub[0].name + "<i id='TechNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></a>" : "<a href='" + itemData.sub.sub[0].link + "'>" + itemData.sub.sub[0].name + "</a></li>")
                                .append(itemData.sub.sub[1].name == "FieldRep" ? "<li><a href='" + itemData.sub.sub[1].link + "'>" + itemData.sub.sub[1].name + "<i id='FieldNotify' class='fa fa-bell' style='color:red;display:none' aria-hidden='true'></i></a>" : "<a href='" + itemData.sub.sub[1].link + "'>" + itemData.sub.sub[1].name + "</a></li>");
                        }
                    }
                }
                else {
                    $.each(submenu, function () {
                        subList.append(getMenuItem(this));
                    });
                }
            });
            item.append(subList);

        }

        return item;
    };
    console.log(agentType);
    if (typeof (agentType) !== "undefined") {

        if (agentType === "70" || agentType === "60") {
            $("ul").empty();
        }
        var $menu = $("#menu").append("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css' />");
        $.each(data.menu, function () {
            $menu.append(
                getMenuItem(this)
            );
        });

        $.each($('.nav').find('li'), function () {
            $(this).toggleClass('active',
                $(this).find('a').attr('href') === window.location.pathname);
        });

    }
    if (agentType === "60" || agentType === "70" || agentType === "30") {
        GetSMSNotification();
        var loadTech = window.setInterval(GetSMSNotification, 60000);
    }

});

function GetSMSNotification() {
    //$("#smsNotify").css('display', 'none');
    $("#TechNotify").css('display', 'none');
    $("#FieldNotify").css('display', 'none');
    $("#LeadNotify").css('display', 'none');
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/SMS/GetSMSNotification",
        success: function (result) {
            if (result != null && result != "") {
                var str = result;
                var splitted = str.split(",");

                splitted.forEach(function (splitted) {
                    if (splitted == "Tech") {
                        $("#TechNotify").css('display', 'inline-block');
                        document.title = 'Tech New SMS';
                        //if (document.cookie.indexOf("teckNotification=") == -1) {
                        //    window.open("/sms.html?v=0.38582239552502307&type=50&notification=true", '_blank');
                        //    setCookie("teckNotification", 1, 1);
                        //}
                    }
                    if (splitted == "FieldRep") {
                        $("#FieldNotify").css('display', 'inline-block');
                        document.title = 'Rep New SMS';
                        //if (document.cookie.indexOf("repNotification=") == -1) {
                        //    window.open("/sms.html?v=0.38582239552502307&type=20&notification=true", '_blank');
                        //    setCookie("repNotification", 1, 1);
                        //}
                    }
                    if (splitted == "Lead") {
                        $("#LeadNotify").css('display', 'inline-block');
                        document.title = 'Lead New SMS';
                        //if (document.cookie.indexOf("leadNotification=") == -1) {
                        //    window.open("/sms.html?v=0.38582239552502307&type=10&notification=true", '_blank');
                        //    setCookie("leadNotification", 5);
                        //}
                    }
                    $("#smsNotify").css('display', 'inline-block');
                });

            }
            else {
                $("#smsNotify").css('display', 'none');
                $("#TechNotify").css('display', 'none');
                $("#FieldNotify").css('display', 'none');
                $("#LeadNotify").css('display', 'none');
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#smsNotify").css('display', 'none');
            $("#TechNotify").css('display', 'none');
            $("#FieldNotify").css('display', 'none');
            $("#LeadNotify").css('display', 'none');
        }
    });
}

//$(window).unload(function () {
//    $.cookies.del('teckNotification');
//    $.cookies.del('repNotification');
//    $.cookies.del('leadNotification');
//});
