var IsList = true;
var accountId = null;
var _account = new Object();
var leadid = 0;
var accountStatusId = 0;
$(document).ready(function () {
    ChcekCookie();
    $("#spanClockinout").load("/clockinout.html");
    $("#spanACC").load("/acc.html?v=" + Math.random());
    $("#txtDob,#txtinstalleddate,#txtsaledate,#txtsubmitteddate,#txtfundeddate,#txtcontractstartdate").datetimepicker({
        timepicker: false,
        format: 'm/d/Y',
        maxDate: 0
    });


    var currentDate = new Date();
    var date = new Date(currentDate);
    var newdate = new Date(date);

    newdate.setDate(newdate.getDate() - 1);

    var dd = newdate.getDate();
    var mm = newdate.getMonth() + 1;
    var y = newdate.getFullYear();

    var _date = mm + '/' + dd + '/' + y;

    $("#txtStartDate").datetimepicker({
        timepicker: false,
        format: 'm/d/y',
        step: 30
    });

    //$("#txtStartDate").val(_date);

    $("#txtEndDate").datetimepicker({
        timepicker: false,
        format: 'm/d/y',
        step: 30
    });
    //$("#txtEndDate").val(_date);


    $("#btnSubmit").click(function () {
        CreateAccount();
    });


    accountId = getParameterByName("aid");
    var isNew = getParameterByName("data");
    if (isNew != "0") {
        LoadAccountData("", "");
    }

    $("#btnSearch").click(function () {
        $("#btnSearch").hide();
        $("#loader").show();
        var _searchfield = $("#ddlSearchField").find(":selected").val();
        var _searchterm = $("#txtSearchAccount").val();
        if (_searchfield == "name" || _searchfield == "accountid" || _searchfield == "address" || _searchfield == "city" || _searchfield == "phone") {
            _searchterm = $("#txtSearchAccount").val();
        }
        else {
            _searchterm = $("#ddlSearchValue").find(":selected").val();
        }

        LoadAccountData(_searchfield, _searchterm);
    });

    if (accountId == "") {
        $("#divDetails").show();
    }
});

function SearchOnEnter(e) {
    var keycode = (e.keyCode ? e.keyCode : e.which);
    if (keycode == '13') {
        $("#btnSearch").hide();
        $("#loader").show();
        var _searchfield = $("#ddlSearchField").find(":selected").val();
        var _searchterm = $("#txtSearchAccount").val();
        LoadAccountData(_searchfield, _searchterm);
        return false;
    }
}

function LoadAccountData(searchfield, searchterm) {

    $("#btnSearch").hide();
    $("#loader").show();
    $("#msg").hide();
    $("#errorMessage").hide();
    $("#spanAccountResults").html("");
    var fromdate = new Date();
    var toDate = new Date();
    var SearchDateFiled = $('#ddlSearchDate option:selected').val();
    if (SearchDateFiled != "nothing") {
        fromdate = $("#txtStartDate").val();
        todate = $("#txtEndDate").val();
    }
    var searchRequest = {
        "searchterm": searchterm,
        "searchfield": searchfield,
        "fromDate": fromdate,
        "toDate": toDate,
        "searchfield1": SearchDateFiled
    };

    var url = "/api/Account/GetAllAccounts";
    $.ajax({
        method: "PUT",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(searchRequest),
        success: function (result) {
            if (result != undefined) {
                if (result.length > 0) {
                    $("#divHeader").show();
                    BuildAccountTable(result);
                    $("#spanAccountResults").show();
                    $("#errorMessage").hide();
                }
                else {

                    $("#errorMessage").show();
                    $("#msg").html("No Data Found!!!");
                    $("#msg").show();
                    $("#spanAccountResults").hide();
                    $("#loader").hide();
                    $("#btnSearch").show();
                    $("#divHeader").hide();
                }
            }
            else {

                if (accountId == null || accountId == "") {
                    $("#errorMessage").show();
                    $("#msg").html("No Data Found!!!");
                    $("#msg").show();
                    $("#spanAccountResults").hide();
                    $("#loader").hide();
                    $("#btnSearch").show();
                    $("#divHeader").hide();
                }
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function CreateAccount() {

    $("#successMsg").hide();
    $("#errorMessage").hide();
    var strmsg = "";
    if ($("#txtFirstName").val() == null || $("#txtFirstName").val() == "") {
        strmsg = "Enter First Name.</br>"
    }
    if ($("#txtLastName").val() == null || $("#txtLastName").val() == "") {
        strmsg += "Enter Last Name.</br>"
    }
    if ($("#txtEmail").val() == null || $("#txtEmail").val() == "") {
        strmsg += "Enter Email."
    }
    if (strmsg != "") {
        $("#msg").html(strmsg);
        $("#errorMessage").show();
        $("#btnSubmit").show();
        return false;
    }
    var account = _account;
    if (accountId != null && accountId != '') {
        account.AccountId = accountId;
        account.PreviousAccountStatusId = account.AccountStatusId;
    } else {
        account.AccountId = 0;
    }
    if ($("#txtFirstName").val() != null && $("#txtFirstName").val() != '') {
        account.FirstName = $("#txtFirstName").val().trim();
    }
    if ($("#txtLastName").val() != null && $("#txtLastName").val() != '') {
        account.LastName = $("#txtLastName").val().trim();
    }
    if ($("#txtCompany").val() != null && $("#txtCompany").val() != '') {
        account.Company = $("#txtCompany").val().trim();
    }
    if ($("#txtCity").val() != null && $("#txtCity").val() != '') {
        account.City = $("#txtCity").val().trim();
    }
    if ($("#txtState").val() != null && $("#txtState").val() != '') {
        account.State = $("#txtState").val().trim();
    }
    if ($("#txtAddress").val() != null && $("#txtAddress").val() != '') {
        account.Address = $("#txtAddress").val().trim();
    }
    if ($("#txtZip").val() != null && $("#txtZip").val() != '') {
        account.Zip = $("#txtZip").val().trim();
    }
    if ($("#txtbAddress").val() != null && $("#txtbAddress").val() != '') {
        account.bAddress = $("#txtbAddress").val().trim();
    }
    if ($("#txtbCity").val() != null && $("#txtbCity").val() != '') {
        account.bCity = $("#txtbCity").val().trim();
    }
    if ($("#txtbState").val() != null && $("#txtbState").val() != '') {
        account.bState = $("#txtbState").val().trim();
    }
    if ($("#txtbZip").val() != null && $("#txtbZip").val() != '') {
        account.bZip = $("#txtbZip").val().trim();
    }
    if ($("#txtEmail").val() != null && $("#txtEmail").val() != '') {
        account.Email = $("#txtEmail").val().trim();
    }

    var hoverification = $("input[name='rdbhoverification']:checked").val();
    if (hoverification) {
        account.HoVerification = parseInt(hoverification);
    }
    if ($("#txtphone1").val() != null && $("#txtphone1").val() != '') {
        account.Phone1 = $("#txtphone1").val().trim();
    }
    if ($("#txtphone2").val() != null && $("#txtphone2").val() != '') {
        account.Phone2 = $("#txtphone2").val().trim();
    }
    if ($("#txtEmername1").val() != null && $("#txtEmername1").val() != '') {
        account.EmerName1 = $("#txtEmername1").val().trim();
    }
    if ($("#txtEmerphone1").val() != null && $("#txtEmerphone1").val() != '') {
        account.EmerPhone1 = $("#txtEmerphone1").val().trim();
    }
    if ($("#txtEmername2").val() != null && $("#txtEmername2").val() != '') {
        account.EmerName2 = $("#txtEmername2").val().trim();
    }
    if ($("#txtEmerphone2").val() != null && $("#txtEmerphone2").val() != '') {
        account.EmerPhone2 = $("#txtEmerphone2").val().trim();
    }
    if ($("#txtDob").val() != null && $("#txtDob").val() != '') {
        account.DOB = $("#txtDob").val().trim();
    }
    if ($("#txtarea").val() != null && $("#txtarea").val() != '') {
        account.Area = $("#txtarea").val().trim();
    }
    account.Signalsconf = $("#txtsignalsconf").val();
    account.OnlineConf = $("#txtonlineconf").val();

    var Preinstall = $("input[name='rdbPreinstall']:checked").val();
    if (Preinstall) {
        account.Preinstall = parseInt(Preinstall);
    }

    account.Postinstall = $('#ddlPostInstall option:selected').val();
    account.AccountHolder = $('#ddlAccountHolder option:selected').val();
    account.Monitoring = $('#ddlMonitoring option:selected').val();

    if ($("#txtinstalleddate").val() != null && $("#txtinstalleddate").val() != '') {
        account.InstalledDate = $("#txtinstalleddate").val();
    }
    if ($("#txtsaledate").val() != null && $("#txtsaledate").val() != '') {
        account.SaleDate = $("#txtsaledate").val();
    }
    if ($("#txtsubmitteddate").val() != null && $("#txtsubmitteddate").val() != '') {
        account.SubmittedDate = $("#txtsubmitteddate").val();
    }
    if ($("#txtfundeddate").val() != null && $("#txtfundeddate").val() != '') {
        account.FundedDate = $("#txtfundeddate").val();
    }
    if ($("#txtcontractterm").val() != null && $("#txtcontractterm").val() != '') {
        account.ContractTerm = parseInt($("#txtcontractterm").val());
    }
    if ($("#txtcreditgrade").val() != null && $("#txtcreditgrade").val() != '') {
        account.CreditGrade = $("#txtcreditgrade").val().trim();
    }
    if ($("#txtcreditscore").val() != null && $("#txtcreditscore").val() != '') {
        account.CreditScore = parseInt($("#txtcreditscore").val());
    }
    if ($("#txtcontractstartdate").val() != null && $("#txtcontractstartdate").val() != '') {
        account.ContractStartDate = $("#txtcontractstartdate").val();
    }
    if ($("#txtmmr").val() != null && $("#txtmmr").val() != '') {
        account.MMR = parseFloat($("#txtmmr").val());
    }
    if ($("#txtbuyoutamount").val() != null && $("#txtbuyoutamount").val() != '') {
        account.BuyoutAmount = parseFloat($("#txtbuyoutamount").val());
    }

    if ($("#txtverbalpasscode").val() != null && $("#txtverbalpasscode").val() != '') {
        account.VerbalPasscode = $("#txtverbalpasscode").val().trim();
    }
    account.FieldRepId = $('#ddlFieldRep option:selected').val();
    account.TechId = $('#ddlTech option:selected').val();


    account.Rep = $('#ddlFieldRep option:selected').text();
    account.Tech = $('#ddlTech option:selected').text();

    account.leadId = leadid;
    account.accountStatusId = accountStatusId;
    var _agentId = getCookie("agentId");
    account.agentid = _agentId;
    $("#btnSubmit").hide();
    var url = "/api/Account/CreateAccount";


    var noteToSave = new Object();
    noteToSave.NoteText = $("#notes").val().trim();
    noteToSave.AgentId = _agentId;

    var AccountNotes = new Object();
    AccountNotes.Account = account;
    AccountNotes.NoteToSave = noteToSave;
    $.ajax({
        method: "POST",
        contentType: "application/json",
        url: url,
        data: JSON.stringify(AccountNotes),
        success: function (result) {
            window.location.replace("/account.html");
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#btnSubmit").show();
        }
    });
}

function Clear() {
    $("#txtFirstName").val("");
    $("#txtLastName").val("");
    $("#txtCompany").val("");
    $("#txtCity").val("");
    $("#txtState").val("");
    $("#txtAddress").val("");
    $("#txtZip").val("");
    $("#txtbAddress").val("");
    $("#txtbCity").val("");
    $("#txtbState").val("");
    $("#txtbZip").val("");
    $("#txtEmail").val("");
    $("#txthoverification").val("");
    $("#txtphone1").val("");
    $("#txtphone2").val("");
    $("#txtEmername1").val("");
    $("#txtEmerphone1").val("");
    $("#txtEmername2").val("");
    $("#txtEmerphone2").val("");
    $("#txtDob").val("");
    $("#txtarea").val("");
    $("#txtsignalsconf").val("");
    $("#txtonlineconf").val("");
    $("#txtPreinstall").val("");
    $("#txtPostinstall").val("");
    $("#ddlAccountHolder").val("");
    $("#ddlMonitoring").val("");
    $("#txtinstalleddate").val("");
    $("#txtsaledate").val("");
    $("#txtsubmitteddate").val("");
    $("#txtfundeddate").val("");
    $("#txtcontractterm").val("");
    $("#txtcreditgrade").val("");
    $("#txtcreditscore").val("");
    $("#txtcontractstartdate").val("");
    $("#txtmmr").val("");
    $("#txtbuyoutamount").val("");
    $("#notes").val("");
}

function GetAccountData(accountId) {

    $("#loader").show();
    $("#divDetails").hide();
    //var IsEdit = getParameterByName("isedit");

    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/Account/GetAccountById/" + accountId,
        success: function (result) {

            if (result) {
                _account = result;
                $("#lblaccountstatus").html(GetAccountStatusIdText(result.accountStatusId));
                accountStatusId = result.accountStatusId;
                $("#lblAccountId").html(result.accountId);
                $("#txtFirstName").val(result.firstName);
                $("#txtLastName").val(result.lastName);
                $("#txtCompany").val(result.company);
                $("#txtCity").val(result.city);
                $("#txtState").val(result.state);
                $("#txtAddress").val(result.address);
                $("#txtZip").val(result.zip);
                $("#txtbAddress").val(result.bAddress);
                $("#txtbCity").val(result.bCity);
                $("#txtbState").val(result.bState);
                $("#txtbZip").val(result.bZip);
                $("#txtEmail").val(result.email);
                if (result.hoVerification == true) {
                    $("#rdbhoVerification_Yes").prop("checked", true);
                }
                else {
                    $("#rdbhoVerification_No").prop("checked", true);
                }
                $("#txtphone1").val(result.phone1);
                $("#txtphone2").val(result.phone2);
                $("#txtEmername1").val(result.emerName1);
                $("#txtEmerphone1").val(result.emerPhone1);
                $("#txtEmername2").val(result.emerName2);
                $("#txtEmerphone2").val(result.emerPhone2);

                var dob1 = result.dob.split("T");
                if (dob1[0] === "0001-01-01") {
                    $("#txtDob").html("");
                }
                else {
                    var dob = new Date(result.dob);
                    var dd = dob.getDate();
                    var mm = dob.getMonth() + 1;
                    var y = dob.getFullYear();
                    var dateOfBirth = mm + '/' + dd + '/' + y;
                    $("#txtDob").val(dateOfBirth);
                }
                $("#txtarea").val(result.area);
                $("#txtsignalsconf").val(result.signalsconf);
                $("#txtonlineconf").val(result.onlineConf);
                if (result.preinstall == true) {
                    $("#rdbPreinstall_Yes").prop("checked", true);
                }
                else {
                    $("#rdbPreinstall_No").prop("checked", true);
                }

                $("#ddlPostInstall").val(result.postinstall);
                $("#ddlAccountHolder").val(result.accountHolder);
                $("#ddlMonitoring").val(result.monitoring);
                var installedDate1 = result.installedDate.split("T");
                if (installedDate1[0] === "0001-01-01") {
                    $("#txtinstalleddate").val("");
                }
                else {
                    var installedDate = new Date(result.installedDate);
                    var dd = installedDate.getDate();
                    var mm = installedDate.getMonth() + 1;
                    var y = installedDate.getFullYear();
                    installedDate = mm + '/' + dd + '/' + y;
                    $("#txtinstalleddate").val(installedDate);
                }

                var saleDate1 = result.saleDate.split("T");
                if (saleDate1[0] === "0001-01-01") {
                    $("#txtsaledate").val("");
                }
                else {
                    var saleDate = new Date(result.saleDate);
                    var dd = saleDate.getDate();
                    var mm = saleDate.getMonth() + 1;
                    var y = saleDate.getFullYear();
                    saleDate = mm + '/' + dd + '/' + y;
                    $("#txtsaledate").val(saleDate);
                }
                var submittedDate1 = result.submittedDate.split("T");
                if (submittedDate1[0] === "0001-01-01") {
                    $("#txtsubmitteddate").val("");
                }
                else {
                    var submittedDate = new Date(result.submittedDate);
                    var dd = submittedDate.getDate();
                    var mm = submittedDate.getMonth() + 1;
                    var y = submittedDate.getFullYear();
                    submittedDate = mm + '/' + dd + '/' + y;
                    $("#txtsubmitteddate").val(submittedDate);
                }

                var fundedDate1 = result.fundedDate.split("T");
                if (fundedDate1[0] === "0001-01-01") {
                    $("#txtfundeddate").val("");
                }
                else {
                    var fundedDate = new Date(result.fundedDate);
                    var dd = fundedDate.getDate();
                    var mm = fundedDate.getMonth() + 1;
                    var y = fundedDate.getFullYear();
                    fundedDate = mm + '/' + dd + '/' + y;
                    $("#txtfundeddate").val(fundedDate);
                }

                $("#txtcontractterm").val(result.contractTerm);
                $("#txtcreditgrade").val(result.creditGrade);
                $("#txtcreditscore").val(result.creditScore);

                var contractStartDate1 = result.contractStartDate.split("T");
                if (contractStartDate1[0] === "0001-01-01") {
                    $("#txtcontractstartdate").val("");
                }
                else {
                    var contractStartDate = new Date(result.contractStartDate);
                    var dd = contractStartDate.getDate();
                    var mm = contractStartDate.getMonth() + 1;
                    var y = contractStartDate.getFullYear();
                    contractStartDate = mm + '/' + dd + '/' + y;
                    $("#txtcontractstartdate").val(contractStartDate);
                }
                $("#txtmmr").val(result.mmr);
                $("#txtbuyoutamount").val(result.buyoutAmount);

                $("#txtverbalpasscode").val(result.verbalPasscode);

                leadid = result.leadId;
                $("#ddlFieldRep").val(result.fieldRepId)
                $("#ddlTech").val(result.techId)


                $("#btnSubmit").val("Update");
                $("#loader").hide();
                $("#divDetails").show();
                $("#addnote").show();
                $("#fileAttachmentupload").show();

                LoadAccountNotes();
            } else {
                $("#loader").hide();
                $("#divDetails").show();
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#loader").hide();
            $("#divDetails").show();
        }
    })

}

function ViewAccountData(accountId) {
    $("#loader").show();
    $("#divdetails").hide();
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: "/api/Account/GetAccountById/" + accountId,
        success: function (result) {
            if (result) {
                _account = result;
                $("#lblaccountstatus").html(GetAccountStatusIdText(result.accountStatusId));
                $("#lblFirstName").html(result.firstName);
                $("#lblMiddleName").html(result.middleName);
                $("#lblLastName").html(result.lastName);
                $("#lblCompany").html(result.company);
                $("#lblCity").html(result.city);
                $("#lblState").html(result.state);
                $("#lblAddress").html(result.address);
                $("#lblZip").html(result.zip);
                $("#lblbAddress").html(result.bAddress);
                $("#lblbCity").html(result.bCity);
                $("#lblbState").html(result.bState);
                $("#lblbZip").html(result.bZip);
                $("#lblAccountId").html(accountId);

                var _email = "";

                var email = result.email.split(";");
                var stremail = "";
                for (var i = 0; i < email.length; i++) {
                    _email += email[i] + "<br />"
                }

                $("#lblEmail").html(_email);
                if (result.hoVerification == true) {
                    $("#lblhoverification").html("Yes");
                }
                else {
                    $("#lblhoverification").html("No");
                }
                $("#lblphone1").html(result.phone1);
                $("#lblphone2").html(result.phone2);
                $("#lblEmername1").html(result.emerName1);
                $("#lblEmerphone1").html(result.emerPhone1);
                $("#lblEmername2").html(result.emerName2);
                $("#lblEmerphone2").html(result.emerPhone2);
                var dob1 = result.dob.split("T");
                if (dob1[0] === "0001-01-01") {
                    $("#lblDob").html("");
                }
                else {
                    var dob = new Date(result.dob);
                    var dd = dob.getDate();
                    var mm = dob.getMonth() + 1;
                    var y = dob.getFullYear();
                    var dateOfBirth = mm + '/' + dd + '/' + y;
                    $("#lblDob").html(dateOfBirth);
                }
                $("#lblarea").html(result.area);
                $("#lblsignalsconf").html(result.signalsconf);
                $("#lblonlineconf").html(result.onlineConf);

                if (result.preinstall == true) {
                    $("#lblPreinstall").html("Yes");
                }
                else {
                    $("#lblPreinstall").html("No");
                }

                if (result.postinstall == 3) {
                    $("#lblPostinstall").html("Completed");
                }
                else if (result.postinstall == 2) {
                    $("#lblPostinstall").html("Partial (fundable)");
                }
                else if (result.postinstall == 1) {
                    $("#lblPostinstall").html("Partial (non-fundable)");
                }
                else {
                    $("#lblPostinstall").html("No");
                }

                $("#lblaccountholder").html(result.accountHolder);

                var installedDate1 = result.installedDate.split("T");
                if (installedDate1[0] === "0001-01-01") {
                    $("#lblinstalleddate").html("");
                }
                else {
                    var installedDate = new Date(result.installedDate);
                    var dd = installedDate.getDate();
                    var mm = installedDate.getMonth() + 1;
                    var y = installedDate.getFullYear();
                    installedDate = mm + '/' + dd + '/' + y;
                    $("#lblinstalleddate").html(installedDate);
                }

                var saleDate1 = result.saleDate.split("T");
                if (saleDate1[0] === "0001-01-01") {
                    $("#lblsaledate").html("");
                }
                else {
                    var saleDate = new Date(result.saleDate);
                    var dd = saleDate.getDate();
                    var mm = saleDate.getMonth() + 1;
                    var y = saleDate.getFullYear();
                    saleDate = mm + '/' + dd + '/' + y;
                    $("#lblsaledate").html(saleDate);
                }
                var submittedDate1 = result.submittedDate.split("T");
                if (submittedDate1[0] === "0001-01-01") {
                    $("#lblsubmitteddate").html("");
                }
                else {
                    var submittedDate = new Date(result.submittedDate);
                    var dd = submittedDate.getDate();
                    var mm = submittedDate.getMonth() + 1;
                    var y = submittedDate.getFullYear();
                    submittedDate = mm + '/' + dd + '/' + y;
                    $("#lblsubmitteddate").html(submittedDate);
                }

                var fundedDate1 = result.fundedDate.split("T");
                if (fundedDate1[0] === "0001-01-01") {
                    $("#lblfundeddate").html("");
                }
                else {
                    var fundedDate = new Date(result.fundedDate);
                    var dd = fundedDate.getDate();
                    var mm = fundedDate.getMonth() + 1;
                    var y = fundedDate.getFullYear();
                    fundedDate = mm + '/' + dd + '/' + y;
                    $("#lblfundeddate").html(fundedDate);
                }

                $("#lblcontractterm").html(result.contractTerm);
                $("#lblcreditgrade").html(result.creditGrade);
                $("#lblcreditscore").html(result.creditScore);

                var contractStartDate1 = result.contractStartDate.split("T");
                if (contractStartDate1[0] === "0001-01-01") {
                    $("#lblcontractstartdate").html("");
                }
                else {
                    var contractStartDate = new Date(result.contractStartDate);
                    var dd = contractStartDate.getDate();
                    var mm = contractStartDate.getMonth() + 1;
                    var y = contractStartDate.getFullYear();
                    contractStartDate = mm + '/' + dd + '/' + y;
                    $("#lblcontractstartdate").html(contractStartDate);
                }
                $("#lblmmr").html(result.mmr);
                $("#lblbuyoutamount").html(result.buyoutAmount);
                $("#lblVerbalPasscoe").html(result.verbalPasscode);
                if (result.repFirstName != "" && result.repFirstName != null) {
                    $("#lblFiledRep").html(result.repFirstName + " " + result.repLastName)
                }
                if (result.techFirstName != "" && result.techFirstName != null) {
                    $("#lblTech").html(result.techFirstName + " " + result.techLastName);
                }



                $("#btnSubmit").html("Update");
                $("#errorMessage").css('display', 'none');
                $("#divdetails").css('display', 'block');
            } else {
                $("#errorMessage").css('display', 'block');
                $("#divdetails").css('display', 'none');
            }
            $("#addnote").show();
            $("#fileAttachmentupload").show();
            $("#divNotesList").show();
            LoadAccountNotes();
            $("#loader").hide();
            $("#divdetails").show();
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
            $("#msg").html(JSON.stringify(error));
            $("#errorMessage").show();
            $("#loader").hide();
            $("#divdetails").show();
        }
    })

}

function CancelAccount() {
    window.location.replace("/account.html");
}

function isNumber(evt) {
    var iKeyCode = (evt.which) ? evt.which : evt.keyCode
    if (iKeyCode != 46 && iKeyCode > 31 && (iKeyCode < 48 || iKeyCode > 57))
        return false;
    else
        return true;
}

function LoadAccountNotes() {
    var accountId = getParameterByName("aid");
    var url = "/api/Account/GetNoteByAccountId/" + accountId;

    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (result) {

            $("#divNotesList").show();
            BuildNotesTable(result);

        },
        Error: function (error) {
            alert(JSON.stringify(error));
            console.log(JSON.stringify(error));
        }
    });
}

function SaveAccountNote() {
    if ($("#notes").val() === "") {
        $("#msgNote").html("Enter Note First.");
        $("#successSschednotes").hide();
        $("#errorMessageschednotes").show();
    }
    else {
        $("#addnote").hide();
        $("#fileAttachmentupload").hide();
        $("#loadernote").show();

        var url = "/api/Account/SaveAccountNote/";

        var _agentId = getCookie("agentId");
        var accountId = getParameterByName("aid");
        var formData = new FormData();
        formData.append("file", $("#fileid")[0].files[0]);
        formData.append("agentid", _agentId);
        formData.append("accountid", accountId);
        formData.append("leadid", leadid);
        formData.append("noteText", $("#notes").val());

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result == "Success") {
                    $("#notes").val("");
                    $("#successSschednotes").show();
                    $("#errorMessageschednotes").hide();
                    LoadAccountNotes();
                    $("#loadernote").hide();
                    $("#addnote").show();
                    $("#fileAttachmentupload").show();
                    $("#lblAttchmentFileName").html('');
                    $("#errorMessageschednotes").hide();
                    $("#msgNote").html('');
                }
                else {
                    $("#errorMessageschednotes").show();
                    $("#msgNote").html("Note Save Fail!!");
                }

            },
            error: function (result) {
                $("#errorMessageschednotes").show();
                $("#msgNote").html("Note Save Fail!!");
                console.log(JSON.stringify(error));
            }
        });

    }

}

function SendToDispatch() {
    window.location.replace("/dispatchtech.html?v=" + Math.random() + "&aid=" + accountId);
}

function validateFileType() {
    $("#msgNote").html("Allowed only .pdf,.jpg,.png file.");
    $("#errorMessageschednotes").hide();
    var fileName = document.getElementById("fileid").value;
    var idxDot = fileName.lastIndexOf(".") + 1;
    var extFile = fileName.substr(idxDot, fileName.length).toLowerCase();
    if (extFile == "pdf" || extFile == "png" || extFile == "jpg") {
        $("#lblAttchmentFileName").html(fileName);
        return true;
    } else {
        $("#msgNote").html("Allowed only .pdf,.jpg,.png file.");
        $("#errorMessageschednotes").show();
        return false;
    }
}

//Building tables
function BuildAccountTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildRow(data[i]));
    }

    $("#spanAccountResults").html(table);
    $("#btnSearch").show();
    $("#loader").hide();
}

function BuildHeader() {
    var tr = document.createElement("tr");

    var td0 = document.createElement("th");
    var td1 = document.createElement("th");
    var td2 = document.createElement("th");
    var td3 = document.createElement("th");
    var td4 = document.createElement("th");
    var td5 = document.createElement("th");
    //var td12 = document.createElement("th");
    var td6 = document.createElement("th");
    var td7 = document.createElement("th");
    var td8 = document.createElement("th");
    var td9 = document.createElement("th");
    var td10 = document.createElement("th");
    var td11 = document.createElement("th");

    //var text0 = "<span class='spanHidetitle'>Account Id</span>";
    //var text1 = "<span class='spanHidetitle'>Account Status</span>";
    //var text2 = document.createTextNode("Create Date");
    //var text3 = document.createTextNode("Customer");
    //var text4 = document.createTextNode("Address");
    //var text5 = document.createTextNode("Email");
    //var text6 = document.createTextNode("Phone");
    //var text9 = document.createTextNode("MMR");
    //var text10 = document.createTextNode("Action");

    var text0 = "<span class='spanHidetitle'>Account Id</span>";
    var text1 = document.createTextNode("Name");
    var text2 = document.createTextNode("Area");
    var text3 = document.createTextNode("Status");
    var text4 = document.createTextNode("Holder");
    var text5 = document.createTextNode("Address"); //Address,city,state
    var text6 = document.createTextNode("Create Date");
    var text7 = document.createTextNode("Install Date");
    var text8 = document.createTextNode("MMR");
    var text9 = document.createTextNode("Tech");
    var text10 = document.createTextNode("Rep");
    var text11 = document.createTextNode("Action");
    //var text12 = document.createTextNode("Phone");

    td0.innerHTML = text0;
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
    //td12.appendChild(text12);

    tr.appendChild(td0);
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    //tr.appendChild(td12);

    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);
    tr.appendChild(td10);
    tr.appendChild(td11);

    return tr;
}

function BuildRow(rowData) {
    var tr = document.createElement("tr");

    var td0 = document.createElement("td"); //AccountId
    var td1 = document.createElement("td"); //Name
    var td2 = document.createElement("td"); //Area
    var td3 = document.createElement("td"); //Status
    var td4 = document.createElement("td"); //Holder
    var td5 = document.createElement("td"); //Address, City, State 
    //var td12 = document.createElement("td"); //Phone
    var td6 = document.createElement("td"); //Create Date
    var td7 = document.createElement("td"); //Install Date
    var td8 = document.createElement("td"); //MMR
    var td9 = document.createElement("td"); //Tech
    var td10 = document.createElement("td"); //Rep
    var td11 = document.createElement("td"); //Action

    var _editIdLink = '<div class="col-md-12"> <a href="/accountentry.html?v=' + Math.random() + '&aid=' + rowData.accountId + '" >Edit</a><a href="/accountdetails.html?v=' + Math.random() + '&aid=' + rowData.accountId + '" style="float:right">View</a></div>';
    var _viewIdLink = '<a href="/accountdetails.html?v=' + Math.random() + '&aid=' + rowData.accountId + '">' + rowData.accountId + '</a>';

    var _address = rowData.address + ", " + rowData.city + ", " + rowData.state;
    var _fullname = rowData.firstName + " " + rowData.lastName;
    var _email = "";

    var email = rowData.email.split(";");
    var stremail = "";
    for (var i = 0; i < email.length; i++) {
        _email += email[i] + "<br />"
    }
    var _phone1 = rowData.phone1;
    var accountStatus = GetAccountStatusIdText(rowData.accountStatusId);
    var insertDate = "";

    var dates = rowData.insertDate.split('T');
    if (dates[0] != "0001-01-01") {
        insertDate = DateFormat(rowData.insertDate);
    }
    var installedDate = "";
    var installedDateFormat = rowData.insertDate.split('T');
    if (installedDateFormat[0] != "0001-01-01") {
        installedDate = DateFormat(rowData.installedDate);
    }

    var Rep = "";
    if (rowData.fieldRepId > 0) {
        Rep = rowData.repFirstName + " " + rowData.repLastName;
    }

    var Tech = "";
    if (rowData.techId > 0) {
        Tech = rowData.techFirstName + " " + rowData.techLastName;
    }

    var text1 = "<span class='snapTitle' style ='display:none'><b>Name :</b></span> " + _fullname;
    var text2 = "<span class='snapTitle' style ='display:none'><b>Area:</b></span> " + rowData.area;
    var text3 = "<span class='snapTitle' style ='display:none'><b>Status:</b></span> " + accountStatus;
    var text4 = "<span class='snapTitle' style ='display:none'><b>Holder:</b></span> " + rowData.accountHolder;
    var text5 = "<span class='snapTitle' style ='display:none'><b>Address:</b></span> " + _address;
    //var text12 = "<span class='snapTitle' style ='display:none'><b>Phone:</b></span> " + rowData.phone1;
    var text6 = "<span class='snapTitle' style ='display:none'><b>Create Date:</b></span> " + insertDate;
    var text7 = "<span class='snapTitle' style ='display:none'><b>Install Date:</b></span> " + installedDate;
    var text8 = "<span class='snapTitle' style ='display:none'><b>MMR:</b></span> " + rowData.mmr;
    var text9 = "<span class='snapTitle' style ='display:none'><b>Tech:</b></span> " + Tech;
    var text10 = "<span class='snapTitle' style ='display:none'><b>Rep:</b></span> " + Rep;

    td0.innerHTML = _viewIdLink;
    td1.innerHTML = text1;
    td1.className = "textAlignleft";
    td2.innerHTML = text2;
    td2.className = "textAlignleft";
    td3.innerHTML = text3;
    td3.className = "textAlignleft";
    if (accountStatus == "Sold (1st QA)") {
        td3.bgColor = "#ffa500";
        td3.className = "font-color-white";
    }
    if (accountStatus == "Tech Dispatched") {
        td3.bgColor = "#0000ff";
        td3.className = "font-color-white";
    }
    if (accountStatus == "Partial Install") {
        td3.bgColor = "#ffff00";
    }
    if (accountStatus == "Installed (2nd QA)") {
        td3.bgColor = "#008000";
        td3.className = "font-color-white";
    }

    td4.innerHTML = text4;
    td4.className = "textAlignleft";
    td5.innerHTML = text5;
    td5.className = "textAlignleft";
    td6.innerHTML = text6;
    td6.className = "textAlignleft";
    td7.innerHTML = text7;
    td7.className = "textAlignleft";
    td8.innerHTML = text8;
    td8.className = "textAlignleft";

    td9.innerHTML = text9;
    td9.className = "textAlignleft";

    td10.innerHTML = text10;
    td10.className = "textAlignleft";


    td11.innerHTML = _editIdLink;
    //td12.innerHTML = text12;
    // td12.className = "textAlignleft";

    tr.appendChild(td0);
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    tr.appendChild(td4);
    tr.appendChild(td5);
    // tr.appendChild(td12);
    tr.appendChild(td6);
    tr.appendChild(td7);
    tr.appendChild(td8);
    tr.appendChild(td9);
    tr.appendChild(td10);
    tr.appendChild(td11);

    return tr;
}

function BuildNotesTable(data) {
    var table = document.createElement("table");
    table.appendChild(BuildNoteHeader());

    for (var i = 0; i < data.length; i++) {
        table.appendChild(BuildNoteRow(data[i]));
    }

    $("#spanPastNotes").html(table);
    $("#loading").hide();
}

function BuildNoteRow(rowData) {
    var tr = document.createElement("tr");

    var td1 = document.createElement("td");
    var td2 = document.createElement("td");
    var td3 = document.createElement("td");

    var shortDate = DateFormat(rowData.insertDate);
    var text1 = document.createTextNode(shortDate);
    var noteText = rowData.noteText;
    noteText = noteText.replace(/\n/g, "<br />");
    if (rowData.fileName != null && rowData.fileName != "") {
        noteText = "<i class='fa fa-paperclip' style='font-size:20px' aria-hidden='true'></i>";
        noteText += rowData.noteText;
        noteText += "<a href='api/Notes/FetchFile/" + rowData.fileName + "'>" + rowData.fileName + "</a>";
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

function BuildNoteHeader() {
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

window.onload = function () {
    var accountId = getParameterByName("aid");
    if (accountId == null || accountId == "") {
        $("#loader").css('display', 'none');
        $("#spanAccountId").hide();
        $("#fileAttachmentupload").hide();
    }
    LoadFieldRep(1);
    LoadTech(1);
    if (accountId > 0) {

        var str = '<a href=/accountdetails.html?v=' + Math.random() + '&aid=' + accountId + ' class="btn btn-primary">View</a>';
        $("#spanbtnView").html(str);
        //GetAccountData(accountId);
        $("#spanAccountId").show();
    }

};

function decimaDigits(e) {
    $(e).on('input', function () {
        this.value = this.value
            .replace(/[^\d.]/g, '')             // numbers and decimals only
            .replace(/(^[\d]{7})[\d]/g, '$1')   // not more than 7 digits at the beginning
            .replace(/(\..*)\./g, '$1')         // decimal can't exist more than once
            .replace(/(\.[\d]{2})./g, '$1');    // not more than 2 digits after decimal
    });
}

if ((window.location.href.indexOf("accountentry") > -1) || (window.location.href.indexOf("accountdetail") > -1)) {
    document.getElementById('fileAttachmentupload').addEventListener('click', openDialog);
}

function openDialog() {
    document.getElementById('fileid').click();
}

function CreateAccountEntry() {
    window.location.href = "accountentry.html?v=" + Math.random() + "&data=0";
    IsList = false;
    $("#divDetails").show();
}

function SearchResult(searchBy) {

    if (searchBy == "name" || searchBy == "accountid" || searchBy == "address" || searchBy == "city" || searchBy == "phone") {
        $("#txtSearchAccount").css('display', 'inline-block');
        $("#ddlSearchValue").css('display', 'none');
    }
    else {
        $("#txtSearchAccount").css('display', 'none');
        $("#ddlSearchValue").css('display', 'inline-block');
        LoadSearchResult(searchBy);
    }
}

function LoadSearchResult(searchBy) {
    if (searchBy == "state") {
        LoadState();
    }
    else if (searchBy == "area") {
        LoadArea();
    }
    else if (searchBy == "status") {
        LoadStatus();
    }
    else if (searchBy == "fieldrep") {
        LoadFieldRep(0);
    }
    else if (searchBy == "tech") {
        LoadTech(0);
    }
}

function LoadState() {
    $.ajax({
        url: "/api/Account/GetAllState/",
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {
            var ddlSearchValue = document.getElementById("ddlSearchValue");
            if (ddlSearchValue != null) {
                $("#ddlSearchValue option").remove();
                var strState = "";
                strState = '<option value="0">- SELECT -</option>';
                for (var i = 0; i < data.length; i++) {
                    strState = strState +
                        '<option value="' + data[i].state + '">' + data[i].state + '</option>';
                }
                ddlSearchValue.innerHTML = strState;
            }
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

function LoadArea() {
    var ddlSearchValue = document.getElementById("ddlSearchValue");
    $("#ddlSearchValue option").remove();
    var strArea = "";
    strArea = '<option value="0">- SELECT -</option>';
    strArea = strArea + '<option value="MESA">MESA</option>';
    strArea = strArea + '<option value="VALOR">VALOR</option>';
    strArea = strArea + '<option value="DDG">DDG</option>';
    ddlSearchValue.innerHTML = strArea;
}

function LoadStatus() {
    var ddlSearchValue = document.getElementById("ddlSearchValue");
    $("#ddlSearchValue option").remove();
    var strStatus = "";
    strStatus = '<option value="0">- SELECT -</option>';
    strStatus = strStatus + '<option value="55">FieldRep</option>';
    strStatus = strStatus + '<option value="56">Sold</option>';
    strStatus = strStatus + '<option value="57">Installed (2nd QA)</option>';
    strStatus = strStatus + '<option value="58">UFS</option>';
    strStatus = strStatus + '<option value="59">No Show</option>';
    strStatus = strStatus + '<option value="100">Tech Sched</option>';
    strStatus = strStatus + '<option value="110">Tech Dispatched</option>';
    strStatus = strStatus + '<option value="120">Partial Install</option>';
    ddlSearchValue.innerHTML = strStatus;
}

function LoadFieldRep(isLoad) {
    var url = "/api/Account/GetFieldRep/20";
    console.log(url);
    $.ajax({
        method: "GET",
        contentType: "application/json",
        url: url,
        success: function (data) {
            var strFieldRep = "";
            strFieldRep = '<option value="0">- SELECT -</option>';
            for (var i = 0; i < data.length; i++) {
                strFieldRep = strFieldRep +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
            }
            if (isLoad == 0) {
                var ddlSearchValue = document.getElementById("ddlSearchValue");
                if (ddlSearchValue != null) {
                    $("#ddlSearchValue option").remove();

                    ddlSearchValue.innerHTML = strFieldRep;
                }
            }
            else {
                var ddlFieldRep = document.getElementById("ddlFieldRep");
                if (ddlFieldRep != null) {
                    $("#ddlSearchValue option").remove();

                    ddlFieldRep.innerHTML = strFieldRep;
                }
            }
        },
        Error: function (error) {
            console.log(JSON.stringify(error));
        }
    });
}

function LoadTech(isLoad) {

    $.ajax({
        url: "/api/Agent/50", //tech is agentType50
        type: "GET",
        dataType: "json",
        success: function (data, textStatus, xhr) {

            $("#ddlSearchValue option").remove();
            var strTech = "";
            strTech = strTech +
                '<option value="0">- SELECT -</option>';
            //var accountId = getParameterByName("aid");
            //if (accountId != null && accountId != "") {
            //    strTech = strTech +
            //        '<option value="-99999">***********SCHEDULED************</option>';
            //}
            for (var i = 0; i < data.length; i++) {
                strTech = strTech +
                    '<option value="' + data[i].agentId + '">' + data[i].firstName + ' ' + data[i].lastName + '</option>';
            }
            if (isLoad == 0) {
                var ddlSearchValue = document.getElementById("ddlSearchValue");
                ddlSearchValue.innerHTML = strTech;
            }
            else {
                var ddlTech = document.getElementById("ddlTech");
                if (ddlTech != null) {
                    $("#ddlTech option").remove();
                    ddlTech.innerHTML = strTech;
                }

                var accountId = getParameterByName("aid");
                if (accountId != null && accountId != "") {
                    $("#divDetails").hide();
                    GetAccountData(accountId);
                }
            }

            console.log("dropdown loaded.");
            $("#btnDispatchTech").show();
        },

        error: function (xhr, textStatus, errorThrown) {
            console.log("Error in Operation");
        }
    });
}

$(function () {
    $("#ddlSearchField").change(function () {
        var selectedText = $(this).find("option:selected").text();
        var selectedValue = $(this).val();
        SearchResult(selectedValue);
    });
});