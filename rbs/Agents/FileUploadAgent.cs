using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class FileUploadAgent
{
    public string ImportLeadsViaFile(IFormFile file)
    {
        int totalcounter = 0;
        int newcounter = 0;
        int dupcounter = 0;
        int notcitycounter = 0;
        int notcountycounter = 0;
        var output = string.Empty;
        try
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                if (reader != null)
                {
                    //skip the header row
                    var s = reader.ReadLine();
                }

                //import into DB here
                while (!reader.EndOfStream)
                {
                    var fields = reader.ReadLine().Split(',');
                    int price = 0;
                    if (!string.IsNullOrEmpty(fields[7]) && int.TryParse(fields[7], out price))
                    {
                        if (price > 100000 && price < 500000) //shouldn't be loosing any here ...
                        {
                            var leads = new MySqlDataAgent().SearchLeadByAddress(fields[3].ToString());

                            if (leads != null)
                            {
                                if (leads.Count == 0)
                                {
                                    if (!string.IsNullOrEmpty(fields[1]) &&
                                        !string.IsNullOrEmpty(fields[3]))
                                    {
                                        if (DoWeWantThisCity(fields[4]))
                                        {
                                            Lead lead = new Lead();

                                            if (IsPinalCounty(fields[6]))
                                            {
                                                lead.County = "Pinal";
                                                lead.State = "AZ";
                                                lead.TimeZone = "MST";
                                            }
                                            else if ((IsMarcopiaCounty(fields[6])))
                                            {
                                                lead.County = "Maricopa";
                                                lead.State = "AZ";
                                                lead.TimeZone = "MST";
                                            }
                                            else if ((IsPimaCounty(fields[6])))
                                            {
                                                lead.County = "Pima";
                                                lead.State = "AZ";
                                                lead.TimeZone = "MST";
                                            }
                                            else if ((IsDallasCounty(fields[6])))
                                            {
                                                lead.County = "Dallas";
                                                lead.State = "TX";
                                                lead.TimeZone = "CST";
                                            }
                                            else if ((IsCollinCounty(fields[6])))
                                            {
                                                lead.County = "Collin";
                                                lead.State = "TX";
                                                lead.TimeZone = "CST";
                                            }

                                            if (lead.County != string.Empty)
                                            {
                                                //insert
                                                lead.Address = fields[3];
                                                lead.City = fields[4];
                                                lead.Postal = fields[6];
                                                lead.InsertDate = DateTime.Now;
                                                lead.HomeValue = price;
                                                if (fields[3].Contains("#"))
                                                {
                                                    lead.LeadStatusId = 5;
                                                }
                                                else
                                                {
                                                    lead.LeadStatusId = 0;
                                                }

                                                //raw (not complete)
                                                lead.LeadTypeId = 1; //NHO
                                                lead.SoldDate = Convert.ToDateTime(fields[1]); //sold date
                                                if (lead.SoldDate < DateTime.Now.AddMonths(-3))
                                                {
                                                    lead.SoldDate = DateTime.Now;
                                                }
                                                lead.Phone1Status = (lead.Phone1Status == null) ? 0 : lead.Phone1Status;
                                                lead.Phone2Status = (lead.Phone2Status == null) ? 0 : lead.Phone2Status;
                                                lead.Phone3Status = (lead.Phone3Status == null) ? 0 : lead.Phone3Status;
                                                lead.Phone4Status = (lead.Phone4Status == null) ? 0 : lead.Phone4Status;
                                                lead.Phone5Status = (lead.Phone5Status == null) ? 0 : lead.Phone5Status;
                                                new MySqlDataAgent().InsertLead(lead);
                                                newcounter++;
                                            }
                                            else
                                            {
                                                notcountycounter++;
                                            }
                                        }
                                        else
                                        {
                                            notcitycounter++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    dupcounter++;
                                }
                            }
                            else
                            {
                                dupcounter++;
                            }
                        }
                    }
                }
            }
            output =
                $"Total: {totalcounter}, New:{newcounter}, Dup:{dupcounter}, Not_City:{notcitycounter}, Not_County_Zip:{notcountycounter}";
        }
        catch (Exception e)
        {
            Logger.Log("PLWEB.ERROR FileUploadAgent.ImportLeadsViaFile", e.Message + e.StackTrace);
            output = "PLWEB.ERROR FileUploadAgent.ImportLeadsViaFile";
        }

        Logger.Log("FileUploadAgent.ImportLeadsViaFile COUNTS", 
            output);

        return output;
    }

    public bool DoWeWantThisCity(string city)
    {
        city = city.ToLower();
        if (
//Dallas cities
city == "addison" ||
city == "balch springs" ||
city == "carrollton" ||
city == "cedar hill" ||
city == "cockrell hill" ||
city == "combine" ||
city == "coppell" ||
city == "dallas" ||
city == "desoto" ||
city == "duncanville" ||
city == "farmers branch" ||
city == "ferris" ||
city == "garland" ||
city == "glenn heights" ||
city == "grand prairie" ||
city == "grapevine" ||
city == "hutchins" ||
city == "irving" ||
city == "lancaster" ||
city == "mesquite" ||
city == "ovilla" ||
city == "richardson" ||
city == "rowlett" ||
city == "sachse" ||
city == "seagoville" ||
city == "university park" ||
city == "wilmer" ||
city == "wylie" ||
//Collin county cities
city == "allen" ||
city == "anna" ||
city == "blue ridge" ||
city == "farmersville" ||
city == "fairview" ||
city == "lavon" ||
city == "lowry Crossing" ||
city == "lucas" ||
city == "mckinney" ||
city == "melissa" ||
city == "murphy" ||
city == "nevada" ||
city == "parker" ||
city == "weston" ||
city == "frisco" ||
city == "plano" ||
city == "princeton" ||
city == "prosper" ||
city == "royse city" ||
city == "celina" ||
city == "josephine" ||
city == "sachse" ||
city == "van alstyne" ||
//AZ cities
city == "avondale" ||
city == "buckeye" ||
city == "chandler" ||
city == "el mirage" ||
city == "glendale" ||
city == "goodyear" ||
city == "litchfield park" ||
city == "mesa" ||
city == "peoria" ||
city == "phoenix" ||
city == "scottsdale" ||
city == "surprise" ||
city == "tempe" ||
city == "tolleson" ||
city == "carefree" ||
city == "cave creek" ||
city == "fountain hills" ||
city == "gila bend" ||
city == "gilbert" ||
city == "guadalupe" ||
city == "paradise valley" ||
city == "queen creek" ||
city == "wickenburg" ||
city == "youngtown" ||
city == "apache junction" ||
city == "casa grande" ||
city == "coolidge" ||
city == "eloy" ||
city == "maricopa" ||
city == "florence" ||
city == "marana" ||
city == "queen creek" ||
city == "south tucson" ||
city == "tucson" ||
city == "oro valley" ||
city == "sahuarita"
)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsMarcopiaCounty(string PostCode)
    {
        bool isFound = false;
        string[] Codes = {"85005",
                            "85004",
                            "85007",
                            "85545",
                            "85006",
                            "85009",
                            "85008",
                            "85011",
                            "85013",
                            "85012",
                            "85015",
                            "85014",
                            "85017",
                            "85016",
                            "85019",
                            "85018",
                            "85021",
                            "85020",
                            "85023",
                            "85022",
                            "85024",
                            "85027",
                            "85026",
                            "85029",
                            "85028",
                            "85031",
                            "85033",
                            "85032",
                            "85035",
                            "85034",
                            "85037",
                            "85286",
                            "85036",
                            "85083",
                            "85038",
                            "85041",
                            "85040",
                            "85043",
                            "85042",
                            "85045",
                            "85044",
                            "85048",
                            "85392",
                            "85395",
                            "85295",
                            "85051",
                            "85298",
                            "85050",
                            "85054",
                            "85053",
                            "85064",
                            "85085",
                            "85082",
                            "85087",
                            "85086",
                            "85098",
                            "85202",
                            "85201",
                            "85204",
                            "85203",
                            "85206",
                            "85205",
                            "85208",
                            "85207",
                            "85210",
                            "85209",
                            "85212",
                            "85213",
                            "85215",
                            "85219",
                            "85224",
                            "85226",
                            "85225",
                            "85233",
                            "85234",
                            "85236",
                            "85244",
                            "85249",
                            "85248",
                            "85251",
                            "85250",
                            "85253",
                            "85252",
                            "85255",
                            "85254",
                            "85257",
                            "85256",
                            "85259",
                            "85258",
                            "85261",
                            "85260",
                            "85263",
                            "85262",
                            "85266",
                            "85264",
                            "85268",
                            "85274",
                            "85282",
                            "85281",
                            "85284",
                            "85283",
                            "85287",
                            "85118",
                            "85297",
                            "85296",
                            "85142",
                            "85301",
                            "85303",
                            "85139",
                            "85302",
                            "85305",
                            "85304",
                            "85307",
                            "85306",
                            "85308",
                            "85310",
                            "85320",
                            "85322",
                            "85323",
                            "85326",
                            "85327",
                            "85331",
                            "85329",
                            "85333",
                            "85335",
                            "85337",
                            "85339",
                            "85338",
                            "85340",
                            "85343",
                            "85342",
                            "85345",
                            "85351",
                            "85353",
                            "85355",
                            "85354",
                            "85358",
                            "85361",
                            "85363",
                            "85373",
                            "85375",
                            "85374",
                            "85377",
                            "85379",
                            "85378",
                            "85381",
                            "85383",
                            "85382",
                            "85387",
                            "85385",
                            "85390",
                            "85388",
                            "85396",
                            "85001",
                            "85003",
                            "85002"
                             };
        foreach (var postcode in Codes)
        {
            if (postcode == PostCode)
            {
                isFound = true;
                break;
            }
        }
        return isFound;
    }

    public bool IsPinalCounty(string PostCode)
    {
        bool isFound = false;
        string[] Codes = {"85618",
                            "85623",
                            "85140",
                            "85631",
                            "85218",
                            "85220",
                            "85130",
                            "85143",
                            "85242",
                            "85128",
                            "85131",
                            "85120",
                            "85122",
                            "85119",
                            "85172",
                            "85173",
                            "85145",
                            "85138",
                            "85132",
                            "85137",
                            "85193",
                            "85194",
                            "85191",
                            "85192",
                            "85141",
                            "85123",
                            "85539"};
        foreach (var postcode in Codes)
        {
            if (postcode == PostCode)
            {
                isFound = true;
                break;
            }
        }
        return isFound;
    }

    public bool IsPimaCounty(string PostCode)
    {
        bool isFound = false;
        string[] Codes = {
            "85601",
            "85602",
            "85611",
            "85614",
            "85619",
            "85622",
            "85629",
            "85633",
            "85658",
            "85637",
            "85641",
            "85321",
            "85645",
            "85653",
            "85652",
            "85701",
            "85702",
            "85705",
            "85704",
            "85707",
            "85341",
            "85706",
            "85709",
            "85708",
            "85711",
            "85710",
            "85713",
            "85712",
            "85715",
            "85714",
            "85716",
            "85719",
            "85718",
            "85721",
            "85723",
            "85725",
            "85730",
            "85735",
            "85737",
            "85736",
            "85739",
            "85738",
            "85741",
            "85743",
            "85742",
            "85745",
            "85744",
            "85747",
            "85746",
            "85749",
            "85748",
            "85750",
            "85757",
            "85755",
            "85756"
        };
        foreach (var postcode in Codes)
        {
            if (postcode == PostCode)
            {
                isFound = true;
                break;
            }
        }
        return isFound;
    }

    public bool IsDallasCounty(string PostCode)
    {
        bool isFound = false;
        string[] Codes =
        {
            "75019",
            "75039",
            "75038",
            "75041",
            "75040",
            "75043",
            "75042",
            "75044",
            "75047",
            "75048",
            "75051",
            "75050",
            "75052",
            "75054",
            "75061",
            "75060",
            "75063",
            "75062",
            "75080",
            "75082",
            "75081",
            "75089",
            "75088",
            "75099",
            "75104",
            "75106",
            "75115",
            "75116",
            "75123",
            "75125",
            "75134",
            "75137",
            "75141",
            "75146",
            "75150",
            "75149",
            "75154",
            "75159",
            "75172",
            "75181",
            "75180",
            "75182",
            "75202",
            "75201",
            "75204",
            "75203",
            "75206",
            "75205",
            "75208",
            "75207",
            "75210",
            "75209",
            "75212",
            "75211",
            "75215",
            "75214",
            "75217",
            "75216",
            "75219",
            "75218",
            "75220",
            "75223",
            "75222",
            "75225",
            "75224",
            "75227",
            "75226",
            "75229",
            "75228",
            "75231",
            "75230",
            "75233",
            "75232",
            "75235",
            "75234",
            "75237",
            "75236",
            "75238",
            "75241",
            "75240",
            "75243",
            "75242",
            "75244",
            "75247",
            "75246",
            "75249",
            "75248",
            "75251",
            "75253",
            "75254",
            "75275",
            "75283",
            "75284",
            "75326",
            "75381",
            "75001",
            "75390",
            "75006",
            "75007",
            "75397",
            "75015"
        };
        foreach (var postcode in Codes)
        {
            if (postcode == PostCode)
            {
                isFound = true;
                break;
            }
        }
        return isFound;
    }

    public bool IsCollinCounty(string PostCode)
    {
        bool isFound = false;
        string[] Codes =
        {
            "75189",
            "75409",
            "75024",
            "75023",
            "75025",
            "75034",
            "75072",
            "75035",
            "75424",
            "75442",
            "75454",
            "75069",
            "75071",
            "75070",
            "75075",
            "75074",
            "75078",
            "75252",
            "75093",
            "75097",
            "75094",
            "75485",
            "75098",
            "75287",
            "75033",
            "75164",
            "75002",
            "75166",
            "75009",
            "75173",
            "75013",
            "75407"
        };
        foreach (var postcode in Codes)
        {
            if (postcode == PostCode)
            {
                isFound = true;
                break;
            }
        }
        return isFound;
    }
}

