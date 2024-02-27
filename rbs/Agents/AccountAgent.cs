using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

public class AccountAgent
{
    public Account CreateAccountFromLead(Lead lead)
    {

        var notesAgent = new NotesAgent();
        var newAccount = MapLeadToAccount(lead);

        var _accountId = DoesTheAccountExist(newAccount);
        if (_accountId == -1)
        {
            newAccount.AccountStatusId = 55;
            //create a new account from the lead and insert into the account table
            _accountId = newAccount.AccountId = new MySqlDataAgent().CreateAccount(newAccount);
        }
        else
        {
            // if account already exist and create lead than insert data in accountaudit table 
            newAccount.AccountId = _accountId;
            var accountdata = new MySqlDataAgent().GetAccountById(_accountId);
            accountdata.AccountStatusId = lead.LeadStatusId;
            accountdata.LeadId = lead.LeadId;
            accountdata.AccountStatusId = lead.LeadStatusId;
            accountdata.AgentId = lead.AgentIdSubmitting;
            accountdata.FieldRepId = lead.FieldRepId;
            //accountdata.TechId = lead.TechId;

            var account = new MySqlDataAgent().UpdateAccount(accountdata);
            //var account = new MySqlDataAgent().CreateAccountAudit(newAccount);
        }
        //either way -- attach a note that an Account was created
        //build note
        var note = new Note()
        {
            LeadId = lead.LeadId,
            LeadStatusId = lead.LeadStatusId,
            AccountId = _accountId,
            AgentId = 0,
            NoteText = $"New AccountId {_accountId} Create from Lead {lead.LeadId}"
        };
        //submit note
        notesAgent.SaveNote(note);

        //go back and mark all notes?
        notesAgent.AddAccountIdToNotes(lead.LeadId, _accountId);

        return newAccount;
    }

    /// <summary>
    /// Checks against the email and then against the address to see
    /// if the account exists
    /// </summary>
    /// <param name="account">Account you are checking if exists</param>
    /// <returns>-1 if the account does not exist,
    /// the accountid if the account does exist</returns>
    public int DoesTheAccountExist(Account account)
    {
        var accountId = -1;
        try
        {
            var sqlDataAgent = new MySqlDataAgent();
            var accountList = sqlDataAgent.GetAllAccountByEmail(account.Email).ToList();
            if (accountList != null && accountList.Count > 0)
            {
                // verify account is exist or note using email id
                var AccountExist = accountList.Where(a => a.Email == account.Email).FirstOrDefault();
                if (AccountExist != null)
                {
                    accountId = AccountExist.AccountId;
                }
                //else will be -1
            }
            //else will be -1

            if (accountId == -1)
            {
                accountList = sqlDataAgent.SearchAccountByAddress(account.Address);
                if (accountList != null && accountList.Count > 0)
                {
                    var existingAccount = accountList.FirstOrDefault();
                    accountId = existingAccount.AccountId;
                }
                //else will be -1
            }
            //else will be -1

            //todo - in the future, we can query all with the zip code + street numbers
            //and do some more intelligent searching against that.
        }
        catch (Exception err)
        {
            Logger.Log("PLWEB.ERROR AccountAgent.DoesTheAccountExist", err.Message + err.StackTrace);
        }

        return accountId;
    }

    public Account MapLeadToAccount(Lead lead)
    {
        //na = New Account
        var na = new Account();
        na.LeadId = lead.LeadId;
        na.AccountStatusId = lead.LeadStatusId;
        na.AgentId = lead.AgentIdSubmitting;
        na.FieldRepId = lead.FieldRepId;
        na.TechId = lead.TechId;
        if (lead.FirstName != null)
        {
            na.FirstName = lead.FirstName.Trim();
        }
        if (lead.MiddleName != null)
        {
            na.MiddleName = lead.MiddleName.Trim();
        }
        if (lead.LastName != null)
        {
            na.LastName = lead.LastName.Trim();
        }
        if (lead.Address != null)
        {
            na.Address = lead.Address.Trim();
        }
        if (lead.City != null)
        {
            na.City = lead.City.Trim();
        }
        if (lead.State != null)
        {
            na.State = lead.State.Trim();
        }
        if (lead.Postal != null)
        {
            na.Zip = lead.Postal.Trim();
        }
        if (lead.Email != null)
        {
            na.Email = lead.Email.Trim();
        }
        if (lead.County != null)
        {
            na.County = lead.County.Trim();
        }

        var phones = GetAnsweredPhones(lead);
        if (phones.Count == 1)
        {
            na.Phone1 = phones[0].Trim();
        }
        else if (phones.Count == 2)
        {
            na.Phone1 = phones[0].Trim();
            na.Phone2 = phones[1].Trim();
        }
        else if (phones.Count > 2)
        {
            na.Phone1 = phones[0].Trim();
            na.Phone2 = phones[1].Trim();

            var strPhones = phones.Join(", ");
            var note = new Note()
            {
                LeadId = lead.LeadId,
                AccountId = -1,
                AgentId = 0,
                NoteText = $"Answered phone numbers: {strPhones}"
            };
            //submit note
            new NotesAgent().SaveNote(note);
        }

        //todo --> figure out the "area" based off zip ....

        return na;
    }

    public List<string> GetAnsweredPhones(Lead lead)
    {
        var phones = new List<string>();
        if (lead.Phone1Status == 3)
        {
            phones.Add(lead.Phone1);
        }
        else if (lead.Phone2Status == 3)
        {
            phones.Add(lead.Phone2);
        }
        else if (lead.Phone3Status == 3)
        {
            phones.Add(lead.Phone3);
        }
        else if (lead.Phone4Status == 3)
        {
            phones.Add(lead.Phone4);
        }
        else if (lead.Phone5Status == 3)
        {
            phones.Add(lead.Phone5);
        }

        return phones;
    }

    public List<Account> GetAllAccounts(SearchRequest search)
    {
        var dataAgent = new MySqlDataAgent();
        try
        {
            List<Account> accountData = new List<Account>();
            DateTime fromDate = DateTime.Parse(search.FromDate.ToString("yyyy-MM-dd 00:00:00"));
            DateTime toDate = DateTime.Parse(search.ToDate.ToString("yyyy-MM-dd 23:59:59"));
            if (search.SearchField == "" || string.IsNullOrEmpty(search.SearchField) || search.SearchTerm == "" || string.IsNullOrEmpty(search.SearchTerm))
            {
                DateTime CurrentDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd 00:00:00"));
                if (fromDate < CurrentDate)
                {
                    accountData = dataAgent.SearchAccountByDates(fromDate, toDate, search.SearchField1.ToLower());
                }
                else
                {
                    accountData = dataAgent.GetAllAccountFromDate(DateTime.Now.AddDays(-14));
                }
            }
            else if (search.SearchField == "name")
            {
                accountData = dataAgent.SearchAccountByName(search.SearchTerm.ToLower());

                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }

            }
            else if (search.SearchField == "address")
            {
                accountData = dataAgent.SearchAccountByAddress(search.SearchTerm.ToLower());

                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }

            }
            else if (search.SearchField == "city")
            {
                accountData = dataAgent.SearchAccountByCity(search.SearchTerm.ToLower());
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "accountid")
            {
                accountData = dataAgent.GetAccountListById(Convert.ToInt32(search.SearchTerm));
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "status")
            {
                accountData = dataAgent.SearchAccountByAccountStatusId(Convert.ToInt32(search.SearchTerm));
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "state")
            {
                accountData = dataAgent.SearchAccountByState(search.SearchTerm.ToLower());
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "area")
            {
                accountData = dataAgent.SearchAccountByArea(search.SearchTerm.ToLower());
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "phone")
            {
                accountData = dataAgent.SearchAccountByPhone(search.SearchTerm.ToLower());
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }

            else if (search.SearchField == "fieldrep")
            {
                accountData = dataAgent.SearchAccountByFieldRepId(Convert.ToInt32(search.SearchTerm));
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }
            else if (search.SearchField == "tech")
            {
                accountData = dataAgent.SearchAccountByTechId(Convert.ToInt32(search.SearchTerm));
                if (search.SearchField1 == "create")
                {
                    accountData = accountData.FindAll(a => a.InsertDate >= fromDate && a.InsertDate <= toDate);
                }
                if (search.SearchField1 == "install")
                {
                    accountData = accountData.FindAll(a => a.InstalledDate >= fromDate && a.InstalledDate <= toDate);
                }
            }

            return accountData;
        }
        catch (Exception e)
        {
            Logger.Log("AccountController.GetAllAccounts calling api", e.Message + e.StackTrace);
            return null;
        }
    }

    public Account GetAccountById(int accountid)
    {
        var dataAgent = new MySqlDataAgent();
        return dataAgent.GetAccountById(accountid);
    }

    public bool UpdateAccount(AccountNotes accountNotes)
    {
        var updatelead = false;
        var systemNoteText = string.Empty;
        var dataAgent = new MySqlDataAgent();
        if (accountNotes.Account.Postinstall == 3 && //Complete
            (accountNotes.Account.AccountStatusId == 110 //Tech Dispatched
            || accountNotes.Account.AccountStatusId == 120)) //Partial Install
        {
            accountNotes.Account.AccountStatusId = 57;
            updatelead = true;
            systemNoteText = $"SET TO INSTALLED (2nd QA COMPLETE)";
        }
        else if (accountNotes.Account.Postinstall == 2 && //Partial (fundable)
            (accountNotes.Account.AccountStatusId == 110 //Tech Dispatched
             || accountNotes.Account.AccountStatusId == 120)) //Partial Install
        {
            accountNotes.Account.AccountStatusId = 120;
            updatelead = true;
            systemNoteText = $"SET TO PARTIAL (FUNDABLE) INSTALL";
        }
        else if (accountNotes.Account.Postinstall == 1 && //Partial (non-fundable)
                 (accountNotes.Account.AccountStatusId == 110 //Tech Dispatched
                  || accountNotes.Account.AccountStatusId == 120)) //Partial Install
        {
            accountNotes.Account.AccountStatusId = 120;
            updatelead = true;
            systemNoteText = $"SET TO PARTIAL (NON-FUNDABLE) INSTALL";
        }
        else if (accountNotes.Account.Preinstall &&
            (accountNotes.Account.AccountStatusId == 55 //FieldRep Dispatched
            || accountNotes.Account.AccountStatusId == 58) //UFS
            )
        {
            accountNotes.Account.AccountStatusId = 56; //Sold
            updatelead = true;
            systemNoteText = $"SET TO SOLD (1st QA COMPLETE)";
        }
        else //if (accountNotes.Account.AccountStatusId != accountNotes.Account.PreviousAccountStatusId)
        {
            systemNoteText = $"SET TO {accountNotes.Account.AccountStatusId}";
        }

        //update the lead - if applicable
        if (updatelead && accountNotes.Account.LeadId > 0)
        {
            var leadAgent = new LeadAgent();
            var _lead = leadAgent.GetLeadById(accountNotes.Account.LeadId);
            _lead.LeadStatusId = accountNotes.Account.AccountStatusId;
            var leadUpdateRequest = new LeadUpdateRequest()
            {
                LeadToUpdate = _lead
            };
            var leadUpdate = new LeadAgent().UpdateLeadFromSADispatch(leadUpdateRequest);
        }

        var success = dataAgent.UpdateAccount(accountNotes.Account);

        //add "system" note
        
        if (accountNotes.Account.TechId>0)
        {
            systemNoteText = systemNoteText + $"\n Tech: {accountNotes.Account.Tech}";
        }
        if (accountNotes.Account.FieldRepId > 0)
        {
            systemNoteText = systemNoteText + $"\n FieldRep: {accountNotes.Account.Rep}";
        }


        accountNotes.NoteToSave.AccountId = accountNotes.Account.AccountId;
        accountNotes.NoteToSave.LeadId = accountNotes.Account.LeadId;
        accountNotes.NoteToSave.LeadStatusId = accountNotes.Account.AccountStatusId;
        accountNotes.NoteToSave.NoteText = systemNoteText;
        success = dataAgent.InsertNote(accountNotes.NoteToSave);

        if (!string.IsNullOrEmpty(accountNotes.NoteToSave.NoteText))
        {
            accountNotes.NoteToSave.AccountId = accountNotes.Account.AccountId;
            accountNotes.NoteToSave.LeadId = accountNotes.Account.LeadId;
            accountNotes.NoteToSave.LeadStatusId = accountNotes.Account.AccountStatusId;
            accountNotes.NoteToSave.NoteText = accountNotes.NoteToSave.NoteText;
            success = dataAgent.InsertNote(accountNotes.NoteToSave);
        }

        return success;
    }

    public int CreateAccount(AccountNotes accountNotes)
    {
        var dataAgent = new MySqlDataAgent();
        accountNotes.Account.AccountStatusId = 56;
        var _accountId = dataAgent.CreateAccount(accountNotes.Account);
        if (_accountId > 0)
        {
            string NoteText = $"New Created AccountId {_accountId}";
            if (accountNotes.Account.TechId > 0)
            {
                NoteText = NoteText + $"\n Tech: {accountNotes.Account.Tech}";
            }
            if (accountNotes.Account.FieldRepId > 0)
            {
                NoteText = NoteText + $"\n FieldRep: {accountNotes.Account.Rep}";
            }
            
            //add "system" note
            accountNotes.NoteToSave.AccountId = _accountId;
            accountNotes.NoteToSave.LeadId = -1;
            accountNotes.NoteToSave.LeadStatusId = accountNotes.Account.AccountStatusId;
            accountNotes.NoteToSave.NoteText = NoteText; // $"New Created AccountId {_accountId}";
            dataAgent.InsertNote(accountNotes.NoteToSave);

            if (!string.IsNullOrEmpty(accountNotes.NoteToSave.NoteText))
            {
                accountNotes.NoteToSave.AccountId = _accountId;
                accountNotes.NoteToSave.LeadId = -1;
                //various scenarios can be different.  This comes from the UI
                accountNotes.NoteToSave.LeadStatusId = accountNotes.Account.AccountStatusId;
                accountNotes.NoteToSave.NoteText = accountNotes.NoteToSave.NoteText;
                dataAgent.InsertNote(accountNotes.NoteToSave);
            }
        }

        return _accountId;
    }
}

