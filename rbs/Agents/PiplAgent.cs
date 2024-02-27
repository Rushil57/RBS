using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pipl.APIs.Data.Fields;
using Pipl.APIs.Search;

public class PiplAgent
{
    public LookupResponse LookupPipl(Lead request, string testResponse = "")
    {
        //start building the obj to return
        var lookupResponse = new LookupResponse();
        var stuff = new SearchAPIResponse();
        try
        {
            if (testResponse == "")
            {
                stuff = LookupPiplWithLib(request);
            }
            else
            {
                stuff = JsonConvert.DeserializeObject<SearchAPIResponse>(testResponse);
            }

            if (stuff.Person != null ||
                (stuff.PossiblePersons != null && stuff.PossiblePersons.Count < 3))
            {
                lookupResponse.PersonFound = true;

                if (stuff.Person != null)
                {
                    lookupResponse.PossiblePeople.Add(BuildProlinkPerson(stuff.Person));
                }
                else
                {
                    foreach (var piplPerson in stuff.PossiblePersons)
                    {
                        lookupResponse.PossiblePeople.Add(BuildProlinkPerson(piplPerson));
                    }
                }
            }
            else
            {
                lookupResponse.PersonFound = false;
            }
        }
        catch (Exception e)
        {
            Logger.Log("PiplAgent.LookupPipl calling pipl or deserializing", e.Message + e.StackTrace);
            lookupResponse.PersonFound = false;
        }

        try
        {
            //insert the audit into the db
            new MySqlDataAgent().InsertLookupAudit(new LookupAudit()
            {
                PersonFound = lookupResponse.PersonFound,
                LeadId = request.LeadId,
                AgentId = request.AgentIdSubmitting,
                JsonLookupData = JsonConvert.SerializeObject(stuff),
                JsonSubmittedLeadInfo = JsonConvert.SerializeObject(request),
            });
        }
        catch (Exception e)
        {
            Logger.Log("PiplAgent.LookupPipl - submitting InsertLookupAudit", e.Message + e.StackTrace);
        }

        return lookupResponse;
    }

    private ProlinkPerson BuildProlinkPerson(Pipl.APIs.Data.Containers.Person piplPerson)
    {
        var pperson = new ProlinkPerson();
        if (piplPerson.Relationships != null)
        {
            foreach (var relation in piplPerson.Relationships)
            {
                if (relation.Names[0].Last.Contains(piplPerson.Names[0].Last))
                {
                    pperson.PossibleSpouse.Add($"{relation.Names[0].First} {relation.Names[0].Middle}");
                }
            }
        }

        var mostRecentName = piplPerson.Names?.OrderByDescending(x => x.LastSeen).FirstOrDefault();
        pperson.FirstName = mostRecentName.First;
        pperson.MiddleName = mostRecentName.Middle;
        pperson.LastName = mostRecentName.Last;

        var emaillist = piplPerson.Emails?.OrderByDescending(x => x.LastSeen).Take(5).ToList();
        if (emaillist != null)
        {
            foreach (var email in emaillist)
            {
                pperson.Emails.Add(email.Address);
            }
        }

        pperson.Phones = piplPerson.Phones?.OrderByDescending(x => x.ValidSince).Take(5).ToList();

        var addresslist = piplPerson.Addresses?.OrderByDescending(x => x.ValidSince).Take(3).ToList();
        if (addresslist != null)
        {
            foreach (var address in addresslist)
            {
                if (!(string.IsNullOrWhiteSpace(address.Street)
                    && string.IsNullOrWhiteSpace(address.City)
                    && string.IsNullOrWhiteSpace(address.State)))
                    pperson.Addresses.Add($"{address.Street}, {address.City}, {address.State}");
            }
        }

        //not sure which is coming back more frequent.  Start or end?
        if (piplPerson.DOB != null)
        {
            pperson.Age = DateTime.Now.Year - Convert.ToDateTime(piplPerson.DOB?.DateRange.End).Year;
        }

        return pperson;
    }

    public SearchAPIResponse LookupPiplWithLib(Lead request)
    {
        SearchConfiguration searchConfiguration = new SearchConfiguration(
            apiKey: "BUSINESS-PREMIUM-ugit65m8ymkbmve5kt1c5hsc",
            matchRequirements: "phone",
            minimumMatch: 0.66f
            );

        List<Field> fields = new List<Field>();

        //makes it a test.  Comment for prod, uncomment for test
        //fields.Add(new Pipl.APIs.Data.Fields.Email(
        //    address: "clark.kent@example.com"));

        fields.Add(new Pipl.APIs.Data.Fields.Name(
            first: request.FirstName,
            middle: request.MiddleName,
            last: request.LastName));
        fields.Add(new Pipl.APIs.Data.Fields.Address(
            country: "US",
            state: request.State,
            city: request.City));

        if (!string.IsNullOrWhiteSpace(request.FormerCity)
            || !string.IsNullOrWhiteSpace(request.FormerState)
            || !string.IsNullOrWhiteSpace(request.Address))
        {
            fields.Add(new Pipl.APIs.Data.Fields.Address(
                country: "US",
                state: request.FormerState,
                city: request.FormerCity,
                street: request.Address));
        }

        var person = new Pipl.APIs.Data.Containers.Person(fields);
        SearchAPIRequest apirequest = new SearchAPIRequest(
            person: person, 
            
            requestConfiguration: searchConfiguration);
        SearchAPIResponse response = apirequest.SendAsync().GetAwaiter().GetResult();
        return response;
    }
}

