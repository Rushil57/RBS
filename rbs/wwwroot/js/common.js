//function ClearCache(cacheBust) {
//    //var cacheBust = ['js/util.js', 'js/account.js', 'js/menu.js'];
//    for (i = 0; i < cacheBust.length; i++) {
//        var el = document.createElement('script');
//        el.src = cacheBust[i] + "?v=" + Math.random();
//        document.getElementsByTagName('head')[0].appendChild(el);
//    }
//}
//function ChcekCookie() {
//    var agentid = getCookie("agentId");
//    var loggedin = getCookie("loggedIn");

//    if (agentid === null && loggedin !== 1) {
//        window.location.replace("/index.html");
//    }
//}

//function getParameterByName(name) {
//    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
//    var regexS = "[\\?&]" + name + "=([^&#]*)";
//    var regex = new RegExp(regexS);
//    var results = regex.exec(window.location.search);
//    if (results == null)
//        return "";
//    else
//        return decodeURIComponent(results[1].replace(/\+/g, " "));
//}


//function getCookie(name) {
//    var nameEQ = name + "=";
//    var ca = document.cookie.split(';');
//    for (var i = 0; i < ca.length; i++) {
//        var c = ca[i];
//        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
//        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
//    }
//    return null;
//}