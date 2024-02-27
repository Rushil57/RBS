var address;
var city;
var _phone1status = -1;
var _phone2status = -1;
var _phone3status = -1;
var _phone4status = -1;
var _phone5status = -1;
var _formerLeadStatus = -1;
var IsRequest = false;
var GetResponseTracker;
$(document).ready(function () {
    $("#spanClockinout").load("/clockinout.html");
    $("#btnNowanswer").hide();
    $("#btnNotinterseted").hide();
    $("#btnFollowup").hide();
    $("#btnSchedappt").hide();
    $("#loadernoanswer").show();

    checkAgent();
    setAgentType();
    ShowPhoneStatus();

    String.prototype.splice = function (idx, rem, str) {
        return this.slice(0, idx) + str + this.slice(idx + Math.abs(rem));
    };
    var leadId = getParameterByName("lid");

    LoadNotes();
    $("#divNotesHeader").hide();
    LoadLead(leadId);

    $("#followupdate").datetimepicker({
        format: 'm/d/y h:i A',
        step: 30,
        validateOnBlur: false,
        minDate: 0
    });

    $("#schedApptupdate").datetimepicker({
        format: 'm/d/y h:i A',
        step: 30,
        validateOnBlur: false,
        minDate: 0,
        maxDate: MaximumSADate()
    });

    $("#btnFollowup").click(function () {
        FollowUp();
        $("#IsConfirmed").css('display', 'none');
    });

    $("#btnSchedappt").click(function () {
        $("#btnSendSMS").hide();
        if (IsRequest == true) {
            $("#IsConfirmed").css('display', 'inline-block');
            $("#btnSendSMS").hide();
            $("#btnSched").prop('disabled', false);
        }
        SchedAppt();
    });

    $("#btnNowanswer").click(function () {
        NoAnswer();
        $("#IsConfirmed").css('display', 'none');
    });

    $("#btnNotinterseted").click(function () {
        NotInterested();
        $("#IsConfirmed").css('display', 'none');
    });

    $("#btnFollowUp").click(function () {
        FollowUpDoIt();
    });

    $("#btnSched").click(function () {
        SchedApptDoIt();
    });

    $("#copyPhone1").click(function () {
        CopyToClipboard("phone1");
    });
    $("#copyPhone2").click(function () {
        CopyToClipboard("phone2");
    });
    $("#copyPhone3").click(function () {
        CopyToClipboard("phone3");
    });
    $("#copyPhone4").click(function () {
        CopyToClipboard("phone4");
    });
    $("#copyPhone5").click(function () {
        CopyToClipboard("phone5");
    });
    $("#phone1").mask("999-999-9999");
    $("#phone2").mask("999-999-9999");
    $("#phone3").mask("999-999-9999");
    $("#phone4").mask("999-999-9999");
    $("#phone5").mask("999-999-9999");


    $("#btnSendSMS").click(function () {
        SendLeadSMS();
    });

    $("#btnSaveNote").click(function () {
        SaveNote();
    });

    if (IsRequest == false) {
        GetResponse();
    }

    GetResponseTracker = window.setInterval(function () {
        if (IsRequest == false) {   
            if ($('#schedAppt:visible').length != 0) {
                GetResponse();
            }

        } else {
            clearInterval(GetResponseTracker);
        }
    }, 10000);

});

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

function FollowUp() {
    $("#msg").html("");
    $("#errorMessage").hide();
    $("#btnSaveNote").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if (phStatus1 !== "" && phStatus1 === "3"
        || (phStatus2 !== "" && phStatus2 === "3")
        || (phStatus3 !== "" && phStatus3 === "3")
        || (phStatus4 !== "" && phStatus4 === "3")
        || (phStatus5 !== "" && phStatus5 === "3")) {

        $("#followUp").show();
        $("#schedAppt").hide();
        $("#btnFollowUp").show();
        $("#btnSched").hide();
        $("#btnSendSMS").hide();
    }
    else {
        $("#msg").html("At least 1 phone must be marked Answered.");
        $("#errorMessage").show();
        $("#followUp").hide();
    }
}

function SchedAppt() {
    $("#msg").html("");
    $("#errorMessage").hide();
    $("#btnSaveNote").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if (phStatus1 !== "" && phStatus1 === "3"
        || (phStatus2 !== "" && phStatus2 === "3")
        || (phStatus3 !== "" && phStatus3 === "3")
        || (phStatus4 !== "" && phStatus4 === "3")
        || (phStatus5 !== "" && phStatus5 === "3")) {

        $("#followUp").hide();
        $("#schedAppt").show();
        $("#btnSched").show();
        if (IsRequest == false) {
            //$("#btnSched").prop("disabled", true);
            //$("#btnSendSMS").show();
        }
        $("#btnFollowUp").hide();
    }
    else {
        $("#msg").html("At least 1 phone must be marked Answered.");
        $("#errorMessage").show();
        $("#schedAppt").hide();
        $("#btnSched").hide();
        $("#btnSendSMS").hide();
    }
}

function FollowUpDoIt() {
    $("#msg").html("");
    $("#errorMessage").hide();

    $("#btnSaveNote").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if (phStatus1 !== "" && phStatus1 === "3"
        || (phStatus2 !== "" && phStatus2 === "3")
        || (phStatus3 !== "" && phStatus3 === "3")
        || (phStatus4 !== "" && phStatus4 === "3")
        || (phStatus5 !== "" && phStatus5 === "3")) {
        if ($("#txtNoteText").val() === "") {
            $("#msg").html("Enter Note First.");
            $("#errorMessage").show();
        }
        else { UpdateLead(52); }
    }
    else {
        $("#msg").html("At least 1 phone must be marked Answered.");
        $("#errorMessage").show();
    }
}

function SchedApptDoIt() {
    $("#msg").html("");
    $("#errorMessage").hide();

    $("#msg").html("");
    $("#errorMessage").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if (phStatus1 !== "" && phStatus1 === "3"
        || (phStatus2 !== "" && phStatus2 === "3")
        || (phStatus3 !== "" && phStatus3 === "3")
        || (phStatus4 !== "" && phStatus4 === "3")
        || (phStatus5 !== "" && phStatus5 === "3")) {

        if ($("#txtNoteText").val() === "") {
            $("#msg").html("Enter Note First.");
            $("#errorMessage").show();
        }
        else { UpdateLead(53); }
    }
    else {
        $("#msg").html("At least 1 phone must be marked Answered.");
        $("#errorMessage").show();
    }
}

function NotInterested() {
    $("#msg").html("");
    $("#errorMessage").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if (phStatus1 !== "" && phStatus1 === "3"
        || (phStatus2 !== "" && phStatus2 === "3")
        || (phStatus3 !== "" && phStatus3 === "3")
        || (phStatus4 !== "" && phStatus4 === "3")
        || (phStatus5 !== "" && phStatus5 === "3")) {

        UpdateLead(51);
    }
    else {
        $("#msg").html("At least 1 phone must be marked Answered.");
        $("#errorMessage").show();
    }
}

function NoAnswer() {
    $("#msg").html("");
    $("#errorMessage").hide();

    var phStatus1 = "";
    var phStatus2 = "";
    var phStatus3 = "";
    var phStatus4 = "";
    var phStatus5 = "";

    if ($("#divphone1").is(":visible") && $("#phone1").val() !== "") {
        phStatus1 = $("#phone1status").val();
    }
    if ($("#divphone2").is(":visible") && $("#phone2").val() !== "") {
        phStatus2 = $("#phone2status").val();
    }
    if ($("#divphone3").is(":visible") && $("#phone3").val() !== "") {
        phStatus3 = $("#phone3status").val();
    }
    if ($("#divphone4").is(":visible") && $("#phone4").val() !== "") {
        phStatus4 = $("#phone4status").val();
    }
    if ($("#divphone5").is(":visible") && $("#phone5").val() !== "") {
        phStatus5 = $("#phone5status").val();
    }

    console.log("ph1" + phStatus1);
    console.log("ph2" + phStatus2);
    console.log("ph3" + phStatus3);
    console.log("ph4" + phStatus4);
    console.log("ph5" + phStatus5);

    if ((phStatus1 === "" || phStatus1 === "4" || phStatus1 === "2")
        && (phStatus2 === "" || phStatus2 === "4" || phStatus2 === "2")
        && (phStatus3 === "" || phStatus3 === "4" || phStatus3 === "2")
        && (phStatus4 === "" || phStatus4 === "4" || phStatus4 === "2")
        && (phStatus5 === "" || phStatus5 === "4" || phStatus5 === "2")) {

        UpdateLead(50);
    }
    else {
        $("#msg").html("Select all phone status to No Answer OR Bad.");
        $("#errorMessage").show();
    }
}

function LoadLead(id) {
    var url = "/api/Lead/" + id;
    console.log(url);
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {
            ClearForm();
            console.log(result);
            $("#lblCurrentLeadId").html("Current LeadId : " + result.leadId);
            $('#address').val(result.address); // + ", " + result.city + ", " + result.state);
            $('#city').val(result.city);
            $('#state').val(result.state);
            address = result.address;
            city = result.city;
            $('#firstname').val(result.firstName);
            $('#lastname').val(result.lastName);
            $('#email').val(result.email);

            $("#phone1").val(result.phone1 !== "" ? result.phone1.splice(3, 0, "-").splice(7, 0, "-") : "");
            $("#phone2").val(result.phone2 !== "" ? result.phone2.splice(3, 0, "-").splice(7, 0, "-") : "");
            $("#phone3").val(result.phone3 !== "" ? result.phone3.splice(3, 0, "-").splice(7, 0, "-") : "");
            $("#phone4").val(result.phone4 !== "" ? result.phone4.splice(3, 0, "-").splice(7, 0, "-") : "");
            $("#phone5").val(result.phone5 !== "" ? result.phone5.splice(3, 0, "-").splice(7, 0, "-") : "");

            if (result.phone1 !== "") {
                $("#phone1").prop('readonly', true);
                $("#phone1").css('background-color', "#ebebe4");
                $("#copyPhone1").show();
            }
            if (result.phone2 !== "") {
                $("#phone2").prop('readonly', true);
                $("#phone2").css('background-color', "#ebebe4");
                $("#copyPhone2").show();
            }
            if (result.phone3 !== "") {
                $("#phone3").prop('readonly', true);
                $("#phone3").css('background-color', "#ebebe4");
                $("#copyPhone3").show();
            }
            if (result.phone4 !== "") {
                $("#phone4").prop('readonly', true);
                $("#phone4").css('background-color', "#ebebe4");
                $("#copyPhone4").show();
            }
            if (result.phone5 !== "") {
                $("#phone5").prop('readonly', true);
                $("#phone5").css('background-color', "#ebebe4");
                $("#copyPhone5").show();
            }

            _formerLeadStatus = result.leadStatusId;
            if (_formerLeadStatus == 52) {
                $("#lblStatus").html("FollowUp");
            }
            else if (_formerLeadStatus == 53) {
                $("#lblStatus").html("Scheduled Appt");
            }
            _phone1status = result.phone1Status == null ? 0 : result.phone1Status;
            _phone2status = result.phone2Status == null ? 0 : result.phone2Status;
            _phone3status = result.phone3Status == null ? 0 : result.phone3Status;
            _phone4status = result.phone4Status == null ? 0 : result.phone4Status;
            _phone5status = result.phone5Status == null ? 0 : result.phone5Status;

            console.log("p1 :" + _phone1status);
            console.log("p2 :" + _phone2status);
            console.log("p3 :" + _phone3status);
            console.log("p4 :" + _phone4status);
            console.log("p5 :" + _phone5status);

            result.phone1 !== "" ? $('#phone1status').show() : $('#phone1status').hide();
            result.phone2 !== "" ? $('#phone2status').show() : $('#phone2status').hide();
            result.phone3 !== "" ? $('#phone3status').show() : $('#phone3status').hide();
            result.phone4 !== "" ? $('#phone4status').show() : $('#phone4status').hide();
            result.phone5 !== "" ? $('#phone5status').show() : $('#phone5status').hide();
            result.phone6 !== "" ? $('#phone6status').show() : $('#phone6status').hide();

            //if the phone number is bad, then hide the whole row
            result.phone1Status == 4 ? $('#divphone1').hide() : $('#divphone1').show();
            result.phone2Status == 4 ? $('#divphone2').hide() : $('#divphone2').show();
            result.phone3Status == 4 ? $('#divphone3').hide() : $('#divphone3').show();
            result.phone4Status == 4 ? $('#divphone4').hide() : $('#divphone4').show();
            result.phone5Status == 4 ? $('#divphone5').hide() : $('#divphone5').show();

            if (result.leadTypeId == 3) {
                $("#lblTOLeadType").show();
            }
            else {
                $("#lblTOLeadType").hide();
            }

            $("#btnNowanswer").show();
            $("#btnNotinterseted").show();
            $("#btnFollowup").show();
            $("#btnSchedappt").show();
            $("#loadernoanswer").hide();

        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function UpdateLead(statusId) {

    $("#msg").html("");
    $("#errorMessage").hide();

    $("#btnNowanswer").hide();
    $("#btnNotinterseted").hide();
    $("#btnFollowup").hide();
    $("#btnSchedappt").hide();
    $("#btnSched").hide();
    $("#btnFollowUp").hide();
    $("#btnSendSMS").hide();
    $("#loadernoanswer").show();

    var leadUpdateRequest = new Object();
    leadUpdateRequest.PageRequestingUpdate = "updateleaddial";

    var newLead = new Object();
    newLead.LeadId = getParameterByName("lid");
    newLead.FirstName = $("#firstname").val();
    newLead.LastName = $("#lastname").val();
    newLead.Address = $("#address").val();
    newLead.City = $("#city").val();
    newLead.State = $("#state").val();
    newLead.Email = $("#email").val();
    newLead.Phone1 = $("#phone1").val() === null ? null : $("#phone1").val().replace("-", "");
    newLead.Phone2 = $("#phone2").val() === null ? null : $("#phone2").val().replace("-", "");
    newLead.Phone3 = $("#phone3").val() === null ? null : $("#phone3").val().replace("-", "");
    newLead.Phone4 = $("#phone4").val() === null ? null : $("#phone4").val().replace("-", "");
    newLead.Phone5 = $("#phone5").val() === null ? null : $("#phone5").val().replace("-", "");
    newLead.Postal = $("#postcode").val();

    newLead.Phone1Status = (newLead.Phone1 == "") ? 0 : ($("#phone1status").val() == -1) ? _phone1status : $("#phone1status").val();
    newLead.Phone2Status = (newLead.Phone2 == "") ? 0 : ($("#phone2status").val() == -1) ? _phone2status : $("#phone2status").val();
    newLead.Phone3Status = (newLead.Phone3 == "") ? 0 : ($("#phone3status").val() == -1) ? _phone3status : $("#phone3status").val();
    newLead.Phone4Status = (newLead.Phone4 == "") ? 0 : ($("#phone4status").val() == -1) ? _phone4status : $("#phone4status").val();
    newLead.Phone5Status = (newLead.Phone5 == "") ? 0 : ($("#phone5status").val() == -1) ? _phone5status : $("#phone5status").val();

    newLead.Phone1Dial = ($("#phone1status").val() == -1) ? 0 : 1;
    newLead.Phone2Dial = ($("#phone2status").val() == -1) ? 0 : 1;
    newLead.Phone3Dial = ($("#phone3status").val() == -1) ? 0 : 1;
    newLead.Phone4Dial = ($("#phone4status").val() == -1) ? 0 : 1;
    newLead.Phone5Dial = ($("#phone5status").val() == -1) ? 0 : 1;

    newLead.LeadStatusId = statusId;
    newLead.FormerLeadStatusId = _formerLeadStatus;
    newLead.AgentIdSubmitting = getCookie("agentId");

    if (statusId === 53) {
        if ($("#schedApptupdate").val() != null && $("#schedApptupdate").val() != "") {
            newLead.SchedApptDate = $("#schedApptupdate").val();
        }
    }

    if (statusId === 52) {
        newLead.FollowUpDate = $("#followupdate").val();
    }
    leadUpdateRequest.LeadToUpdate = newLead;

    var noteToSave = new Object();
    noteToSave.NoteText = $("#txtNoteText").val();
    leadUpdateRequest.NoteToSave = noteToSave;

    var jsonleadUpdateRequest = JSON.stringify(leadUpdateRequest);
    console.log(jsonleadUpdateRequest);
    $.ajax({
        url: "/api/Lead/UpdateLead",
        type: "POST",
        contentType: "application/json",
        data: jsonleadUpdateRequest,
        success: function (data, textStatus, xhr) {
            console.log("leadDial save: " + data);
            if (data === false) {
                $("#msg").html("An error occured while saving Lead Dial.");
                $("#errorMessage").show();
                $("#btnNowanswer").show();
                $("#btnNotinterseted").show();
                $("#btnFollowup").show();
                $("#btnSchedappt").show();
                $("#btnSched").hide();
                $("#btnFollowUp").hide();
                $("#btnSendSMS").hide();
                $("#loadernoanswer").hide();
            }
            else {
                window.location.replace("/internalsales.html");
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.log('Error in Operation');
            alert(errorThrown);
        }
    });
}

function ClearForm() {
    $('#firstname').val("");
    $('#lastname').val("");
    $('#email').val("");
    $('#phone1').val("");
    $('#phone2').val("");
    $('#phone3').val("");
    $('#phone4').val("");
    $('#phone5').val("");
    $("#phone1status").val(-1);
    $("#phone2status").val(-1);
    $("#phone3status").val(-1);
    $("#phone4status").val(-1);
    $("#phone5status").val(-1);
    $("#address").val("");
}

function ShowPhoneStatus() {

    $("#phone1").keyup(function () {
        $("#phone1").val() !== "___-___-____" ? $('#phone1status').show() : $('#phone1status').hide();
    });

    $("#phone2").keyup(function () {
        $("#phone2").val() !== "___-___-____" ? $('#phone2status').show() : $('#phone2status').hide();
    });

    $("#phone3").keyup(function () {
        $("#phone3").val() !== "___-___-____" ? $('#phone3status').show() : $('#phone3status').hide();
    });

    $("#phone4").keyup(function () {
        $("#phone4").val() !== "___-___-____" ? $('#phone4status').show() : $('#phone4status').hide();
    });

    $("#phone5").keyup(function () {
        $("#phone5").val() !== "___-___-____" ? $('#phone5status').show() : $('#phone5status').hide();
    });
}

function SendLeadSMS() {
    $("#btnSendSMS").show();
    $("#loader").show();
    var agentId = getCookie("agentId");
    var url = "/api/Lead/SendLeadSMS";
    var toPhone = "";
    if ($("#phone1").val() != null && $("#phone1status").val() == 3) {
        toPhone += $("#phone1").val() + ",";
    }
    if ($("#phone2").val() != null && $("#phone2status").val() == 3) {
        toPhone += $("#phone2").val() + ",";
    }
    if ($("#phone3").val() != null && $("#phone3status").val() == 3) {
        toPhone += $("#phone3").val() + ",";
    }
    if ($("#phone4").val() != null && $("#phone4status").val() == 3) {
        toPhone += $("#phone4").val() + ",";
    }
    if ($("#phone5").val() != null && $("#phone5status").val() == 3) {
        toPhone += $("#phone5").val() + ",";
    }

    var sms = {
        "agentSentBy": agentId, //tech we are SMS
        "Body": "Hi - this is Prolink Protection.  Please confirm that this is the correct number by replying YES or 1.", //SMS body        
        "toPhone": toPhone,
        "leadid": getParameterByName("lid")
    };
    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(sms),
        success: function (result) {
            console.log(result);
            if (!result) {
                $("#msg").html("Uh, oh. Something didn't work.");
                $("#errorMessage").show();
                $("#btnSendSMS").show();
            }
            else {
                $("#btnSched").prop('disabled', true);
                GetResponse();
                //IsRequest = true;
                $("#btnSendSMS").show();
            }
            $("#loader").hide();
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
            $("#btnSendSMS").show();
            $("#loader").hide();
        }
    });
}

function GetResponse() {

    if ($("#btnSched").is(":visible")) {
        if (IsRequest == true) {
            $("#IsConfirmed").css('display', 'inline-block');
            $("#IsWaiting").css('display', 'none');
        }
    }
    else {
        $("#IsConfirmed").css('display', 'none');
    }
    if (IsRequest == true) {
        //$("#IsConfirmed").css('display', 'inline-block')
        $("#btnSendSMS").hide();
        $("#btnSched").prop('disabled', false);
    }

    var toPhone = "";
    if ($("#phone1").val() != null) {
        toPhone += $("#phone1").val() + ",";
    }
    if ($("#phone2").val() != null) {
        toPhone += $("#phone2").val() + ",";
    }
    if ($("#phone3").val() != null) {
        toPhone += $("#phone3").val() + ",";
    }
    if ($("#phone4").val() != null) {
        toPhone += $("#phone4").val() + ",";
    }
    if ($("#phone5").val() != null) {
        toPhone += $("#phone5").val() + ",";
    }

    var agentId = getCookie("agentId");
    var url = "/api/Lead/GetResponse";
    var sms = {
        "agentSentBy": agentId, //tech we are SMS        
        "toPhone": toPhone,
        "leadid": getParameterByName("lid")
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(sms),
        success: function (result) {
            console.log(result);
            if (result == "confirmed") {
                if ($("#btnSched").is(":visible")) {
                    $("#IsConfirmed").css('display', 'inline-block');
                    $("#loader").hide();
                    $("#btnSendSMS").hide();
                    $("#btnSched").prop('disabled', false);
                    IsRequest = true;
                    clearInterval(GetResponseTracker);
                    $("#IsWaiting").css('display', 'none');
                }
                else {
                    $("#IsConfirmed").css('display', 'none');
                    $("#IsWaiting").css('display', 'none');
                }
            }
            else if (result == "Waiting") {
                if ($("#btnSched").is(":visible")) {
                    $("#IsConfirmed").css('display', 'none');
                    $("#btnSendSMS").show();
                    $("#btnSched").prop('disabled', false);
                    //IsRequest = true;
                    $("#IsWaiting").css('display', 'inline-block');
                    $("#loader").show();
                }
                else {
                    $("#IsConfirmed").css('display', 'none');
                    $("#IsWaiting").css('display', 'none');
                }
            }
            else {
                if ($("#btnSched").is(":visible")) {
                    $("#btnSendSMS").show();
                }
                $("#IsConfirmed").css('display', 'none');
                $("#IsWaiting").css('display', 'none');
            }
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
            $("#IsConfirmed").css('display', 'none');
            $("#IsWaiting").css('display', 'none');
            $("#btnSendSMS").show();
            $("#btnSched").show();
        }
    });
}

function LoadNotes() {
    $("#loading").show();
    $("#btnSaveNote").hide();

    var _leadid = getParameterByName("lid");
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _leadid = (_leadid) ? _leadid : -1;
    _accountid = (_accountid) ? _accountid : -1;
    _agentId = (_agentId) ? _agentId : -1;
    $("#spanAgentId").text(_agentId);
    $("#spanLeadId").text(_leadid);
    $("#spanAccountId").text(_accountid);

    if (window.location.pathname === "/leaddial.html") {
        $("#menus").hide();
        $("#divNotesHeader").hide();

    }
    var url = "/api/Notes/";
    var noteRequest = {
        "leadid": _leadid,
        "accountid": _accountid,
        "agentid": getCookie("agentId")
    };

    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteRequest),
        success: function (result) {
            BuildNotesTable(result);
            console.log(result);
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        //if (agentType === "40") {
        //    if (data[i].leadStatusId == 52 ||
        //        data[i].leadStatusId == 53) {
        //        table.appendChild(BuildRow(data[i]));
        //    }
        //} else {
            table.appendChild(BuildRow(data[i]));
        //}
    }

    $("#divNotes").html(table);
    $("#loading").hide();
    $("#btnSaveNote").show();
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    //var shortDate = rowData.insertDate.split("T");
    var shortDate = DateFormat(rowData.insertDate);
    //var text1 = document.createTextNode(shortDate[0] + " " + shortDate[1].replace('Z', ''));
    var text1 = document.createTextNode(shortDate);
    var noteText = rowData.noteText;
    noteText = noteText.replace(/\n/g, "<br />");
    if (rowData.fileName != null && rowData.fileName != "") {
        noteText += "  <a href='api/Notes/FetchFile/" + rowData.fileName + "'>" + rowData.fileName + "</a>";
    }
    var text3 = document.createTextNode(rowData.agentName);

    td1.appendChild(text1);
    td2.innerHTML = noteText;
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    var text1 = document.createTextNode("DateTime");
    var text2 = document.createTextNode("Note");
    var text3 = document.createTextNode("Agent");

    td1.appendChild(text1);
    td2.appendChild(text2);
    td3.appendChild(text3);

    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return tr;
}

function SaveNote() {
    $("#loading").show();
    $("#btnSaveNote").hide();

    var _leadid = getParameterByName("lid");
    var _accountid = getParameterByName("aid");
    var _agentId = getCookie("agentId");
    _leadid = (_leadid) ? _leadid : -1;
    _accountid = (_accountid) ? _accountid : -1;
    _agentId = (_agentId) ? _agentId : -1;

    var url = "/api/Notes/";
    var noteToSave = {
        "accountid": _accountid,
        "leadid": _leadid,
        "agentid": _agentId,
        "notetext": $("#txtNoteText").val()
    };

    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(noteToSave),
        success: function (result) {
            console.log("note save" + result);
            LoadNotes();
            $("#txtNoteText").val("");
        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}