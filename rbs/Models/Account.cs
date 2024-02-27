using System;
using System.Collections.Generic;

public class Account
{
    public int AccountId { get; set; } = -1;
    public int LeadId { get; set; }
    public int PreviousAccountStatusId { get; set; }
    public int AccountStatusId { get; set; }
    /// <summary>
    /// AgentIdSubmitting
    /// </summary>
    public int AgentId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Company { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string County { get; set; }
    public string bAddress { get; set; }
    public string bCity { get; set; }
    public string bState { get; set; }
    public string bZip { get; set; }
    public string Email { get; set; }
    public bool HoVerification { get; set; }
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }
    public string EmerName1 { get; set; }
    public string EmerPhone1 { get; set; }
    public string EmerName2 { get; set; }
    public string EmerPhone2 { get; set; }
    public DateTime DOB { get; set; }
    public string Area { get; set; }
    public string Signalsconf { get; set; }
    public string OnlineConf { get; set; }
    public bool Preinstall { get; set; }
    /// <summary>
    /// 0 = not set, 1 = partial (non-fund), 2 = partial (fundable), 3 = complete 
    /// </summary>
    public int Postinstall { get; set; } = 0;
    public string AccountHolder { get; set; }
    public string Monitoring { get; set; }
    public DateTime LastTouched { get; set; }
    public DateTime InstalledDate { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime TechSchedDate { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime FundedDate { get; set; }
    public DateTime ChargedbackDate { get; set; }
    public DateTime DateCancelled { get; set; }
    public string CancelReason { get; set; }
    public int ContractTerm { get; set; }
    public string CreditGrade { get; set; }
    public int CreditScore { get; set; }
    public DateTime ContractStartDate { get; set; }
    public double MMR { get; set; }
    public double BuyoutAmount { get; set; }
    public DateTime InsertDate { get; set; }
    public string LeadSetter { get; set; }
    public string Rep { get; set; }
    public string Tech { get; set; }
    public int FieldRepId { get; set; }
    public int TechId { get; set; }
    public string VerbalPasscode { get; set; }
    public string LeadStatusText { get; set; }
    public int AccountAuditId { get; set; }

    public string TechFirstName { get; set; }
    public string TechLastName { get; set; }
    public string RepFirstName { get; set; }
    public string RepLastName { get; set; }
}
public class AccountNotes
{
    public Account Account { get; set; }
    public Note NoteToSave { get; set; }
}

public class TechCalendar
{
    public List<Account> LastWeek { get; set; }
    public List<Account> ThisWeek { get; set; }
}

public class AccountInfo
{
    public int AccountId { get; set; }
    public int AccountStatus { get; set; }
    public int AccountStatusId { get; set; }
    public string CustomerName { get; set; }
    public string NextAction { get; set; }
    public string NoteText { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone1 { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public DateTime SoldDate { get; set; }
    public string LeadStatusText { get; set; }
    public string Tech { get; set; }    
    public string Rep { get; set; }    
    public DateTime InsertDate { get; set; }
}
public class AccountInfoList
{
    public List<AccountInfo> LastWeek { get; set; }
    public List<AccountInfo> ThisWeek { get; set; }
    public List<AccountInfo> Oldest { get; set; }
}

public class AccountDashboard
{
    public int Sold { get; set; }
    public int Installed { get; set; }
    public int PartInstall { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Account> Account { get; set; }
}