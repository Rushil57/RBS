using System.Collections.Generic;

public class LatLong
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string accuracy { get; set; }
}

public class FoundAtAddress
{
    public string id { get; set; }
    public string location_type { get; set; }
    public string street_line_1 { get; set; }
    public object street_line_2 { get; set; }
    public string city { get; set; }
    public string postal_code { get; set; }
    public string zip4 { get; set; }
    public string state_code { get; set; }
    public string country_code { get; set; }
    public LatLong lat_long { get; set; }
    public bool is_active { get; set; }
    public string delivery_point { get; set; }
    public string link_to_person_start_date { get; set; }
    public object link_to_person_end_date { get; set; }
}

public class LatLong2
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string accuracy { get; set; }
}

public class CurrentAddress
{
    public string id { get; set; }
    public string location_type { get; set; }
    public string street_line_1 { get; set; }
    public object street_line_2 { get; set; }
    public string city { get; set; }
    public string postal_code { get; set; }
    public string zip4 { get; set; }
    public string state_code { get; set; }
    public string country_code { get; set; }
    public LatLong2 lat_long { get; set; }
    public bool is_active { get; set; }
    public string delivery_point { get; set; }
    public string link_to_person_start_date { get; set; }
}

public class LatLong3
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string accuracy { get; set; }
}

public class HistoricalAddress
{
    public string id { get; set; }
    public string location_type { get; set; }
    public string street_line_1 { get; set; }
    public object street_line_2 { get; set; }
    public string city { get; set; }
    public string postal_code { get; set; }
    public string zip4 { get; set; }
    public string state_code { get; set; }
    public string country_code { get; set; }
    public LatLong3 lat_long { get; set; }
    public bool? is_active { get; set; }
    public string delivery_point { get; set; }
    public string link_to_person_start_date { get; set; }
    public string link_to_person_end_date { get; set; }
}

public class AssociatedPeople
{
    public string id { get; set; }
    public string name { get; set; }
    public string firstname { get; set; }
    public string middlename { get; set; }
    public string lastname { get; set; }
    public string relation { get; set; }
}

public class WPPerson
{
    public string id { get; set; }
    public string name { get; set; }
    public string firstname { get; set; }
    public string middlename { get; set; }
    public string lastname { get; set; }
    public List<object> alternate_names { get; set; }
    public string age_range { get; set; }
    public string gender { get; set; }
    public FoundAtAddress found_at_address { get; set; }
    public List<CurrentAddress> current_addresses { get; set; }
    public List<HistoricalAddress> historical_addresses { get; set; }
    public List<object> phones { get; set; }
    public List<AssociatedPeople> associated_people { get; set; }
}

public class WPResponse
{
    public int count_person { get; set; }
    public List<WPPerson> person { get; set; }
    public List<object> warnings { get; set; }
    public object error { get; set; }
}