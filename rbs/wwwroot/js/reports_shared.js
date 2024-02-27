function BuildDialStatsTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildDialStatsHeader());

    var totals = new Object();
    totals.Today = 0;
    totals.ApptToday = 0;
    totals.ApptYesterday = 0;
    totals.ApptThisWeek = 0;
    totals.ApptLastWeek = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i].firstName != null
            && data[i].agentId != 14
            && data[i].status) {
            table.appendChild(BuildDialStatsRow(data[i]));
        }
        totals.Today += data[i].today;
        totals.ApptToday += data[i].apptToday;
        totals.ApptYesterday += data[i].apptYesterday;
        totals.ApptThisWeek += data[i].apptThisWeek;
        totals.ApptLastWeek += data[i].apptLastWeek;
    }

    table.appendChild(BuildDialStatsTotals(totals));

    return table;
}

function BuildDialStatsRow(rowData) {
    var tr = document.createElement("tr");

    if (rowData.lastDialed >= 10 && rowData.isClockedIn) {
        console.log("Last Dail : " + rowData.lastDialed);
        $(tr).css("background-color", "#ffec80");
    }
    if (rowData.lastDialed >= 30 && rowData.isClockedIn) {
        console.log("Last Dail : " + rowData.lastDialed);
        $(tr).css("background-color", "#f7bbbb");
    }

    var td1 = document.createElement("th");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");

    var text1 = document.createTextNode(rowData.firstName);
    var text2 = document.createTextNode(" - ");
    if (rowData.isClockedIn) {
        text2 = document.createTextNode(rowData.lastDialed);
    }
    var _today = (rowData.today == null) ? " - " : rowData.today;
    var text3 = document.createTextNode(_today);
    var _apptToday = (rowData.apptToday == null) ? " - " : rowData.apptToday;
    var text4 = document.createTextNode(_apptToday);
    var _apptYesterday = (rowData.apptYesterday == null) ? " - " : rowData.apptYesterday;
    var text5 = document.createTextNode(_apptYesterday);
    var _apptthisWeek = (rowData.apptThisWeek == null) ? " - " : rowData.apptThisWeek;
    var text6 = document.createTextNode(_apptthisWeek);
    var _apptlastWeek = (rowData.apptLastWeek == null) ? " - " : rowData.apptLastWeek;
    var text7 = document.createTextNode(_apptlastWeek);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}

function BuildDialStatsHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");

    var text1 = document.createTextNode("");
    var text2 = "Min Since<br/>Last Lead";
    var text3 = "Leads<br/>Contacted";
    var text4 = "Appt<br/>Today";
    var text5 = "Appt<br/>Yesterday";
    var text6 = "Appt<br/>This Week";
    var text7 = "Appt<br/>Last Week";

    td1.appendChild(text1);
    td2.innerHTML = text2;
    td3.innerHTML = text3;
    td4.innerHTML = text4;
    td5.innerHTML = text5;
    td6.innerHTML = text6;
    td7.innerHTML = text7;

    td1.className = "tableHeader";
    td2.className = "tableHeader";
    td3.className = "tableHeader";
    td4.className = "tableHeader";
    td5.className = "tableHeader";
    td6.className = "tableHeader";
    td7.className = "tableHeader";
    
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}

function BuildDialStatsTotals(totals) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");

    td1.className = "tabledata";

    var text1 = document.createTextNode("Total");
    var text2 = document.createTextNode("");
    var text3 = document.createTextNode(totals.Today);
    var text4 = document.createTextNode(totals.ApptToday);
    var text5 = document.createTextNode(totals.ApptYesterday);
    var text6 = document.createTextNode(totals.ApptThisWeek);
    var text7 = document.createTextNode(totals.ApptLastWeek);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);

    return tr;
}

function BuildLeadGenStatsTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildLeadGenStatsHeader());

    var totals = new Object();
    totals.TotalProcessed = 0;
    totals.TotalNewLeads = 0;
    totals.TotalRawUploaded = 0;
    totals.TotalLookups = 0;
    totals.TotalPersonFound = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i].firstName !== null) {
            table.appendChild(BuildLeadGenStatsRow(data[i]));
            totals.TotalProcessed += data[i].totalProcessed;
            totals.TotalNewLeads += data[i].totalNewLeads;
            totals.TotalLookups += data[i].lookUps;
            totals.TotalPersonFound += data[i].personFound;
            totals.TotalRawUploaded += data[i].totalRawUploaded;
        }
    }
    //TotalNewLeads / TotalProcessed
    totals.RawVsNewRatio = Math.round((totals.TotalNewLeads / totals.TotalProcessed) * 100) / 100;
    totals.PersonsFoundRatio = Math.round((totals.TotalPersonFound / totals.TotalLookups) * 100) / 100;

    table.appendChild(BuildLeadGenStatsTotals(totals));

    return table;
}

function BuildLeadGenStatsRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");
    var td8 = document.createElement("td");

    var text1 = document.createTextNode(rowData.agentName);
    var text2 = document.createTextNode(rowData.totalProcessed);
    var text3 = document.createTextNode(rowData.totalNewLeads);
    var text4 = document.createTextNode(rowData.rawVsNewRatio);
    var text5 = document.createTextNode(rowData.lookUps);
    var text6 = document.createTextNode(rowData.personFound);
    var text7 = document.createTextNode(rowData.personFoundRatio);
    var text8 = document.createTextNode(rowData.totalRawUploaded);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}

function BuildLeadGenStatsHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Agent");
    var text2 = document.createTextNode("Processed");
    var text3 = document.createTextNode("New Leads");
    var text4 = document.createTextNode("Raw Vs New Ratio");
    var text5 = document.createTextNode("Lookups");
    var text6 = document.createTextNode("PersonsFound");
    var text7 = document.createTextNode("PersonsFoundRatio");
    var text8 = document.createTextNode("Raw Uploaded");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}

function BuildLeadGenStatsTotals(totals) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Total");
    var text2 = document.createTextNode(totals.TotalProcessed);
    var text3 = document.createTextNode(totals.TotalNewLeads);
    var text4 = document.createTextNode(totals.RawVsNewRatio);
    var text5 = document.createTextNode(totals.TotalLookups);
    var text6 = document.createTextNode(totals.TotalPersonFound);
    var text7 = document.createTextNode(totals.PersonsFoundRatio);
    var text8 = document.createTextNode(totals.TotalRawUploaded);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}

function BuildDialRatioTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildDialRatioHeader());

    var totals = new Object();
    totals.Dials = 0;
    totals.NoAnswer = 0;
    totals.NoAnswerRatio = 0;
    totals.Bad = 0;
    totals.BadRatio = 0;
    totals.Answered = 0;
    totals.AnsweredRatio = 0;
    totals.DialsPerLead = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i].name != null) {
            table.appendChild(BuildDialRatioRow(data[i]));
            totals.Dials += data[i].dials;
            totals.NoAnswer += data[i].noAnswer;
            totals.Bad += data[i].bad;
            totals.Answered += data[i].answered;
            totals.DialsPerLead += data[i].dialsPerLead;
        }
    }

    totals.NoAnswerRatio = Math.round((totals.NoAnswer / totals.Dials) * 100) / 100;
    totals.BadRatio = Math.round((totals.Bad / totals.Dials) * 100) / 100;
    totals.AnsweredRatio = Math.round((totals.Answered / totals.Dials) * 100) / 100;
    totals.DialsPerLead = Math.round((totals.DialsPerLead / data.length) * 100) / 100;

    table.appendChild(BuildDialRatioTotals(totals));

    return table;
}

function BuildDialRatioHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");
    var td9 = document.createElement("th");

    var text1 = document.createTextNode("");
    var text2 = document.createTextNode("Dials");
    var text3 = document.createTextNode("No Ans");
    var text4 = "No Ans<br/>Ratio";
    var text5 = document.createTextNode("Bad");
    var text6 = "Bad<br/>Ratio";
    var text7 = document.createTextNode("Answered");
    var text8 = "Answered<br/>Ratio";
    var text9 = document.createTextNode("Dials/Lead");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.innerHTML = text4;
    td5.appendChild(text5);
    td6.innerHTML = text6;
    td7.appendChild(text7);
    td8.innerHTML = text8;
    td9.appendChild(text9);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);

    return tr;
}

function BuildDialRatioRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");
    var td8 = document.createElement("td");
    var td9 = document.createElement("td");

    var text1 = document.createTextNode(rowData.name);
    var text2 = document.createTextNode(rowData.dials === null ? " - " : rowData.dials);
    var text3 = document.createTextNode(rowData.noAnswer === null ? " - " : rowData.noAnswer);
    var text4 = document.createTextNode(rowData.noAnswerRatio === null ? " - " : rowData.noAnswerRatio);
    var text5 = document.createTextNode(rowData.bad === null ? " - " : rowData.bad);
    var text6 = document.createTextNode(rowData.badRatio === null ? " - " : rowData.badRatio);
    var text7 = document.createTextNode(rowData.answered === null ? " - " : rowData.answered);
    var text8 = document.createTextNode(rowData.answeredRatio === null ? " - " : rowData.answeredRatio);
    var text9 = document.createTextNode(rowData.dialsPerLead === null ? " - " : rowData.dialsPerLead);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);
    td9.appendChild(text9);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);

    return tr;
}

function BuildDialRatioTotals(totals) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");
    var td9 = document.createElement("th");

    var text1 = document.createTextNode("Total");
    var text2 = document.createTextNode(totals.Dials);
    var text3 = document.createTextNode(totals.NoAnswer);
    var text4 = document.createTextNode(totals.NoAnswerRatio);
    var text5 = document.createTextNode(totals.Bad);
    var text6 = document.createTextNode(totals.BadRatio);
    var text7 = document.createTextNode(totals.Answered);
    var text8 = document.createTextNode(totals.AnsweredRatio);
    var text9 = document.createTextNode(totals.DialsPerLead);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);
    td9.appendChild(text9);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);

    return tr;
}

function BuildLeadRatioTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildLeadRatioHeader());

    var totals = new Object();
    totals.Leads = 0;
    totals.Retry = 0;
    totals.RetryRatio = 0;
    totals.Bad = 0;
    totals.BadRatio = 0;
    totals.NotInterested = 0;
    totals.NotInterestedRatio = 0;
    totals.Followup = 0;
    totals.FollowupRatio = 0;
    totals.ApptSet = 0;
    totals.ApptSetRatio = 0;
    totals.Connects = 0;
    totals.ConnectRatio = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i].name != null) {
            table.appendChild(BuildLeadRatioRow(data[i]));
            totals.Leads += data[i].leads;
            totals.Retry += data[i].retry;
            totals.Bad += data[i].bad;
            totals.NotInterested += data[i].notInterested;
            totals.Followup += data[i].followup;
            totals.ApptSet += data[i].apptSet;
        }
    }

    totals.BadRatio = Math.round((totals.Bad / totals.Leads) * 100) / 100;
    totals.RetryRatio = Math.round((totals.Retry / totals.Leads) * 100) / 100;
    totals.NotInterestedRatio = Math.round((totals.NotInterested / totals.Leads) * 100) / 100;
    totals.FollowupRatio = Math.round((totals.Followup / totals.Leads) * 100) / 100;
    totals.ApptSetRatio = Math.round((totals.ApptSet / totals.Leads) * 100) / 100;
    totals.Connects += totals.NotInterested + totals.Followup + totals.ApptSet;
    totals.ConnectRatio = Math.round((totals.Connects / totals.Leads) * 100) / 100;

    table.appendChild(BuildLeadRatioTotals(totals));

    return table;
}

function BuildLeadRatioHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");
    var td9 = document.createElement("th");
    var td10 = document.createElement("th");
    var td11 = document.createElement("th");
    var td12 = document.createElement("th");
    var td13 = document.createElement("th");
    var td14 = document.createElement("th");

    var text1 = document.createTextNode("");
    var text2 = document.createTextNode("Leads");
    var text3 = document.createTextNode("Connects");
    var text4 = "Connect<br/>Ratio";
    var text5 = document.createTextNode("No Ans");
    var text6 = document.createTextNode("Ratio");
    var text7 = document.createTextNode("All Bad");
    var text8 = document.createTextNode("Ratio");
    var text9 = document.createTextNode("Not Int");
    var text10 = document.createTextNode("Ratio");
    var text11 = document.createTextNode("Follow Up");
    var text12 = document.createTextNode("Ratio");
    var text13 = document.createTextNode("Appt Set");
    var text14 = document.createTextNode("Ratio");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.innerHTML = text4;
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);
    td9.appendChild(text9);
    td10.appendChild(text10);
    td11.appendChild(text11);
    td12.appendChild(text12);
    td13.appendChild(text13);
    td14.appendChild(text14);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);
    tr.appendChild(td10);
    tr.appendChild(td11);
    tr.appendChild(td12);
    tr.appendChild(td13);
    tr.appendChild(td14);

    return tr;
}

function BuildLeadRatioRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");
    var td8 = document.createElement("td");
    var td9 = document.createElement("td");
    var td10 = document.createElement("td");
    var td11 = document.createElement("td");
    var td12 = document.createElement("td");
    var td13 = document.createElement("td");
    var td14 = document.createElement("td");

    var text1 = document.createTextNode(rowData.name);
    var text2 = document.createTextNode(rowData.leads === null ? " - " : rowData.leads);
    var connects = 0;
    var ni = rowData.notInterested === null ? 0 : rowData.notInterested;
    var fu = rowData.followup === null ? 0 : rowData.followup;
    var as = rowData.apptSet === null ? 0 : rowData.apptSet;
    connects = ni + fu + as;
    var text3 = document.createTextNode(rowData.connects === 0 ? " - " : connects);
    var connectRatio = 0;
    connectRatio = Math.round((connects / rowData.leads) * 100) / 100;
    var text4 = document.createTextNode(rowData.connectRatio === 0 ? " - " : connectRatio);
    var text5 = document.createTextNode(rowData.retry === null ? " - " : rowData.retry);
    var text6 = document.createTextNode(rowData.retryRatio === null ? " - " : rowData.retryRatio);
    var text7 = document.createTextNode(rowData.bad === null ? " - " : rowData.bad);
    var text8 = document.createTextNode(rowData.badRatio === null ? " - " : rowData.badRatio);
    var text9 = document.createTextNode(rowData.notInterested === null ? " - " : rowData.notInterested);
    var text10 = document.createTextNode(rowData.notInterestedRatio === null ? " - " : rowData.notInterestedRatio);
    var text11 = document.createTextNode(rowData.followup === null ? " - " : rowData.followup);
    var text12 = document.createTextNode(rowData.followupRatio === null ? " - " : rowData.followupRatio);
    var text13 = document.createTextNode(rowData.apptSet === null ? " - " : rowData.apptSet);
    var text14 = document.createTextNode(rowData.apptSetRatio === null ? " - " : rowData.apptSetRatio);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);
    td9.appendChild(text9);
    td10.appendChild(text10);
    td11.appendChild(text11);
    td12.appendChild(text12);
    td13.appendChild(text13);
    td14.appendChild(text14);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);
    tr.appendChild(td10);
    tr.appendChild(td11);
    tr.appendChild(td12);
    tr.appendChild(td13);
    tr.appendChild(td14);

    return tr;
}

function BuildLeadRatioTotals(totals) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");
    var td9 = document.createElement("th");
    var td10 = document.createElement("th");
    var td11 = document.createElement("th");
    var td12 = document.createElement("th");
    var td13 = document.createElement("th");
    var td14 = document.createElement("th");

    var text1 = document.createTextNode("Total");
    var text2 = document.createTextNode(totals.Leads);
    var text3 = document.createTextNode(totals.Connects);
    var text4 = document.createTextNode(totals.ConnectRatio);
    var text5 = document.createTextNode(totals.Retry);
    var text6 = document.createTextNode(totals.RetryRatio);
    var text7 = document.createTextNode(totals.Bad);
    var text8 = document.createTextNode(totals.BadRatio);
    var text9 = document.createTextNode(totals.NotInterested);
    var text10 = document.createTextNode(totals.NotInterestedRatio);
    var text11 = document.createTextNode(totals.Followup);
    var text12 = document.createTextNode(totals.FollowupRatio);
    var text13 = document.createTextNode(totals.ApptSet);
    var text14 = document.createTextNode(totals.ApptSetRatio);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);
    td9.appendChild(text9);
    td10.appendChild(text10);
    td11.appendChild(text11);
    td12.appendChild(text12);
    td13.appendChild(text13);
    td14.appendChild(text14);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);
    tr.appendChild(td10);
    tr.appendChild(td11);
    tr.appendChild(td12);
    tr.appendChild(td13);
    tr.appendChild(td14);

    return tr;
}

function BuildSaToInstallTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildSaToInstallHeader());

    var totals = new Object();
    totals.SchedAppt = 0;
    totals.Confirmed = 0;
    totals.Dispatched = 0;
    totals.UFS = 0;
    totals.NoShow = 0;
    totals.Sold = 0;
    totals.Installed = 0;

    for (var i = 0; i < data.length; i++) {
        if (data[i].name != null) {
            table.appendChild(BuildSaToInstallRow(data[i]));
            totals.SchedAppt += data[i].schedAppt;
            totals.Confirmed += data[i].confirmed;
            totals.Dispatched += data[i].frDispatched;
            totals.UFS += data[i].ufs;
            totals.NoShow += data[i].noShow;
            totals.Sold += data[i].sold;
            totals.Installed += data[i].installed;
        }
    }
    table.appendChild(BuildSaToInstallTotal(totals));

    return table;
}

function BuildSaToInstallHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Agent");
    var text2 = document.createTextNode("SchedAppt");
    var text3 = document.createTextNode("Confirmed");
    var text4 = document.createTextNode("Dispatched");
    var text5 = document.createTextNode("UFS");
    var text6 = document.createTextNode("No Show");
    var text7 = document.createTextNode("Sold");
    var text8 = document.createTextNode("Installed");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}

function BuildSaToInstallRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");
    var td4 = document.createElement("td");
    var td5 = document.createElement("td");
    var td6 = document.createElement("td");
    var td7 = document.createElement("td");
    var td8 = document.createElement("td");

    var text1 = document.createTextNode(rowData.name);
    var text2 = document.createTextNode(rowData.schedAppt === null ? " - " : rowData.schedAppt);
    var text3 = document.createTextNode(rowData.confirmed === null ? " - " : rowData.confirmed);
    var text4 = document.createTextNode(rowData.frDispatched === null ? " - " : rowData.frDispatched);
    var text5 = document.createTextNode(rowData.ufs === null ? " - " : rowData.ufs);
    var text6 = document.createTextNode(rowData.noShow === null ? " - " : rowData.noShow);
    var text7 = document.createTextNode(rowData.sold === null ? " - " : rowData.sold);
    var text8 = document.createTextNode(rowData.installed === null ? " - " : rowData.installed);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}

function BuildSaToInstallTotal(totals) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");

    var text1 = document.createTextNode("Total");
    var text2 = document.createTextNode(totals.SchedAppt);
    var text3 = document.createTextNode(totals.Confirmed);
    var text4 = document.createTextNode(totals.Dispatched);
    var text5 = document.createTextNode(totals.UFS);
    var text6 = document.createTextNode(totals.NoShow);
    var text7 = document.createTextNode(totals.Sold);
    var text8 = document.createTextNode(totals.Installed);

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);
    td4.appendChild(text4);
    td5.appendChild(text5);
    td6.appendChild(text6);
    td7.appendChild(text7);
    td8.appendChild(text8);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);

    return tr;
}
