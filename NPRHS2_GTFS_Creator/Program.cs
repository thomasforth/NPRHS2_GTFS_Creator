using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

// This script creates a GTFS format timetable for an imaginary Northern Powerhouse rail service and three imaginary HS2 routes.
// Both run in Autumn 2019 and with a frequency in both directions of one train every twenty minutes.
// Other imaginary could be added using a similar pattern.

List<NewService> services = new List<NewService>();

NewService NPR = new NewService()
{
    agency_id = "NPR",
    agency_name = "Northern Powerhouse Railways",
    route_long_name = "Northern Powerhouse Rail",
    service_id = "NPR_service",
    route_type = "2",
    agency_url = "https://www.tomforth.co.uk/",
    agency_timezone = "Europe/London",
};
NPR.ListOfStops = new List<string>() { "9100YORK", "9100LEEDS", "9100BRADIN", "9100MNCRPIC", "9100MNCRIAP", "9100LVRPLSH" };
NPR.TimeOffsetsInMinutes = new List<int>() { 0, 15, 20, 35, 40, 55 };
services.Add(NPR);

NewService HS2A = new NewService()
{
    agency_id = "HS2",
    agency_name = "High Speed Two",
    route_long_name = "High Speed Two Brum",
    service_id = "HS2_Brum",
    route_type = "2",
    agency_url = "https://www.tomforth.co.uk/",
    agency_timezone = "Europe/London",
};
HS2A.ListOfStops = new List<string>() { "9100EUSTON", "9100BHAMINT", "9100BHAMNWS"};
HS2A.TimeOffsetsInMinutes = new List<int>() { 0, 40, 50};
services.Add(HS2A);

NewService HS2B = new NewService()
{
    agency_id = "HS2",
    agency_name = "High Speed Two",
    route_long_name = "High Speed Two Manc",
    service_id = "HS2_Manc",
    route_type = "2",
    agency_url = "https://www.tomforth.co.uk/",
    agency_timezone = "Europe/London",
};
HS2B.ListOfStops = new List<string>() { "9100EUSTON", "9100BHAMINT", "9100CREWE", "9100MNCRIAP", "9100MNCRPIC" };
HS2B.TimeOffsetsInMinutes = new List<int>() { 0, 40, 60, 70, 75 };
services.Add(HS2B);

NewService HS2C = new NewService()
{
    agency_id = "HS2",
    agency_name = "High Speed Two",
    route_long_name = "High Speed Two Leeds",
    service_id = "HS2_Leeds",
    route_type = "2",
    agency_url = "https://www.tomforth.co.uk/",
    agency_timezone = "Europe/London",
};
HS2C.ListOfStops = new List<string>() { "9100EUSTON", "9100BHAMINT", "9100EMPKWAY", "9100LEEDS" };
HS2C.TimeOffsetsInMinutes = new List<int>() { 0, 40, 60, 90 };
services.Add(HS2C);


Console.WriteLine("Reading Autumn 2019 stops from stops.zip which contains NaPTAN.");
Dictionary<string, NaptanStop> NaPTANStopsDictionary = new Dictionary<string, NaptanStop>();
using (ZipArchive archive = new ZipArchive(File.OpenRead(@"stops.zip")))
{
    using (StreamReader streamReader = new StreamReader(archive.Entries.First().Open()))
    {
        CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
        NaPTANStopsDictionary = csvReader.GetRecords<NaptanStop>().ToDictionary(x => x.ATCOCode, x => x);
    }
}


List<GTFSNaptanStop> GTFSStopsList = new List<GTFSNaptanStop>();
foreach (NewService NS in services)
{
    foreach (string stopCode in NS.ListOfStops)
    {
        NaptanStop naptanstop = NaPTANStopsDictionary[stopCode];
        GTFSNaptanStop gTFSNaptanStop = new GTFSNaptanStop()
        {
            stop_id = naptanstop.ATCOCode,
            stop_name = naptanstop.CommonName,
            stop_code = naptanstop.ATCOCode,
            stop_lat = Math.Round(naptanstop.Latitude, 5),
            stop_lon = Math.Round(naptanstop.Longitude, 5)
        };
        if (GTFSStopsList.Exists(x => x.stop_id == gTFSNaptanStop.stop_id) == false)
        {
            GTFSStopsList.Add(gTFSNaptanStop);
        }
    }

    List<Agency> AgencyList = new List<Agency>();
    AgencyList.Add(new Agency()
    {
        agency_id = NS.agency_id,
        agency_name = NS.agency_name,
        agency_url = NS.agency_url,
        agency_timezone = NS.agency_timezone
    });

    List<Calendar> calendarList = new List<Calendar>();
    calendarList.Add(new Calendar()
    {
        service_id = NS.service_id,
        monday = 1,
        tuesday = 1,
        wednesday = 1,
        thursday = 1,
        friday = 1,
        saturday = 1,
        sunday = 1,
        start_date = "20190519",
        end_date = "20191208"
    });

    List<Route> RoutesList = new List<Route>();
    List<Trip> tripList = new List<Trip>();
    List<StopTime> stopTimesList = new List<StopTime>();

    // THIS IS THE HARD/IMPORTANT BIT OF CODE. I NEED TO CREATE BOTH trips.txt and stop_times.txt in one go with a nested loop
    TimeSpan departureTimeFromOrigin = new TimeSpan(6, 0, 0);
    List<string> ReverseListOfStops = NS.ListOfStops.Reverse<string>().ToList();
    List<int> ReverseTimeOffsets = new List<int>();
    foreach (int Offset in NS.TimeOffsetsInMinutes)
    {
        ReverseTimeOffsets.Add(Math.Abs(NS.TimeOffsetsInMinutes.Last() - Offset));

    }
    ReverseTimeOffsets.Reverse();

    int i = 0;
    while (departureTimeFromOrigin < new TimeSpan(23, 0, 0))
    {
        RoutesList.Add(new Route()
        {
            agency_id = NS.agency_id,
            route_type = NS.route_type,
            route_long_name = NS.route_long_name,
            route_id = $"{NS.service_id}_route_" + i,
        });

        tripList.Add(new Trip()
        {
            trip_id = $"{NS.service_id}_trip_O_" + i,
            route_id = $"{NS.service_id}_route_" + i,
            service_id = $"{NS.service_id}"
        });
        tripList.Add(new Trip()
        {
            trip_id = $"{NS.service_id}_trip_I_" + i,
            route_id = $"{NS.service_id}_route_" + i,
            service_id = $"{NS.service_id}"
        });

        // outbound loop
        for (int n = 0; n < NS.ListOfStops.Count; n++)
        {
            StopTime OutboundJourneyStopTime = new StopTime();
            OutboundJourneyStopTime.trip_id = $"{NS.service_id}_trip_O_" + i;
            OutboundJourneyStopTime.stop_id = NS.ListOfStops[n];
            OutboundJourneyStopTime.stop_sequence = n + 1;
            OutboundJourneyStopTime.arrival_time = departureTimeFromOrigin.Add(new TimeSpan(0, NS.TimeOffsetsInMinutes[n], 0)).ToString(@"hh\:mm\:ss");
            OutboundJourneyStopTime.departure_time = departureTimeFromOrigin.Add(new TimeSpan(0, NS.TimeOffsetsInMinutes[n], 0)).ToString(@"hh\:mm\:ss");
            stopTimesList.Add(OutboundJourneyStopTime);
        }

        // inbound loop                   
        for (int n = 0; n < ReverseListOfStops.Count; n++)
        {
            StopTime InboundJourneyStopTime = new StopTime();
            InboundJourneyStopTime.trip_id = $"{NS.service_id}_trip_I_" + i;
            InboundJourneyStopTime.stop_id = ReverseListOfStops[n];
            InboundJourneyStopTime.stop_sequence = n + 1;
            InboundJourneyStopTime.arrival_time = departureTimeFromOrigin.Add(new TimeSpan(0, NS.TimeOffsetsInMinutes[n], 0)).ToString(@"hh\:mm\:ss");
            InboundJourneyStopTime.departure_time = departureTimeFromOrigin.Add(new TimeSpan(0, NS.TimeOffsetsInMinutes[n], 0)).ToString(@"hh\:mm\:ss");
            stopTimesList.Add(InboundJourneyStopTime);
        }

        departureTimeFromOrigin = departureTimeFromOrigin.Add(new TimeSpan(0, 20, 0));
        i++;
    }

    Console.WriteLine("Writing agency.txt");
    // write GTFS txts.
    // agency.txt, calendar.txt, calendar_dates.txt, routes.txt, stop_times.txt, stops.txt, trips.txt
    string output = $"{NS.service_id}";
    if (Directory.Exists(output) == false)
    {
        Directory.CreateDirectory(output);
    }

    TextWriter TextWriter;
    CsvWriter CSVwriter;
    using (TextWriter = File.CreateText($"{output}/agency.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(AgencyList);
        }
    }

    Console.WriteLine("Writing stops.txt");
    using (TextWriter = File.CreateText($"{output}/stops.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(GTFSStopsList);
        }
    }

    Console.WriteLine("Writing routes.txt");
    using (TextWriter = File.CreateText($"{output}/routes.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(RoutesList);
        }
    }

    Console.WriteLine("Writing trips.txt");
    using (TextWriter = File.CreateText($"{output}/trips.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(tripList);
        }
    }

    Console.WriteLine("Writing calendar.txt");
    using (TextWriter = File.CreateText($"{output}/calendar.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(calendarList);
        }
    }

    Console.WriteLine("Writing stop_times.txt");
    using (TextWriter = File.CreateText($"{output}/stop_times.txt"))
    {
        using (CSVwriter = new CsvWriter(TextWriter, CultureInfo.InvariantCulture))
        {
            CSVwriter.WriteRecords(stopTimesList);
        }
    }

    Console.WriteLine("Creating a GTFS .zip file.");
    if (File.Exists($"{NS.service_id}_GTFS.zip"))
    {
        File.Delete($"{NS.service_id}_GTFS.zip");
    }
    ZipFile.CreateFromDirectory(output, $"{NS.service_id}_GTFS.zip", CompressionLevel.Optimal, false, Encoding.UTF8);
}

public class NewService
{
    public string agency_id { get; set; }
    public string service_id { get; set; }
    public string route_type { get; set; }
    public string route_long_name { get; set; }
    public string agency_name { get; set; }
    public string agency_url { get; set; }
    public string agency_timezone { get; set; }
    public List<string> ListOfStops { get; set; }
    public List<int> TimeOffsetsInMinutes { get; set; }
}

// Classes to hold the GTFS output
// A LIST OF THESE CALENDAR OBJECTS CREATE THE GTFS calendar.txt file

public class Calendar
{
    public string service_id { get; set; }
    public int monday { get; set; }
    public int tuesday { get; set; }
    public int wednesday { get; set; }
    public int thursday { get; set; }
    public int friday { get; set; }
    public int saturday { get; set; }
    public int sunday { get; set; }
    public string start_date { get; set; }
    public string end_date { get; set; }
}

// A LIST OF THESE CALENDAR EXCEPTIONS CREATES THE GTFS  calendar_dates.txt file
public class CalendarException
{
    public string service_id { get; set; }
    public string date { get; set; }
    public string exception_type { get; set; }
}

// A LIST OF THESE TRIPS CREATES THE GTFS trips.txt file.
public class Trip
{
    public string route_id { get; set; }
    public string service_id { get; set; }
    public string trip_id { get; set; }
    public string trip_headsign { get; set; }
    public string direction_id { get; set; }
    public string block_id { get; set; }
    public string shape_id { get; set; }
}

// A LIST OF THESE STOPTIMES CREATES THE GTFS stop_times.txt file
public class StopTime
{
    public string trip_id { get; set; }
    public string arrival_time { get; set; }
    public string departure_time { get; set; }
    public string stop_id { get; set; }
    public int stop_sequence { get; set; }
    public string stop_headsign { get; set; }
    public string pickup_type { get; set; }
    public string drop_off_type { get; set; }
    public string shape_dist_traveled { get; set; }
}

//A LIST OF THESE NAPTANSTOPS CREATES THE GTFS stops.txt file
public class GTFSNaptanStop
{
    public string stop_id { get; set; }
    public string stop_code { get; set; }
    public string stop_name { get; set; }
    public double stop_lat { get; set; }
    public double stop_lon { get; set; }
    public string stop_url { get; set; }
    //public string vehicle_type { get; set; }
}

// A LIST OF THESE ROUTES CREATES THE GTFS routes.txt file.
public class Route
{
    public string route_id { get; set; }
    public string agency_id { get; set; }
    public string route_short_name { get; set; }
    public string route_long_name { get; set; }
    public string route_desc { get; set; }
    public string route_type { get; set; }
    public string route_url { get; set; }
    public string route_color { get; set; }
    public string route_text_color { get; set; }
}

// A LIST OF THESE AGENCIES CREATES THE GTFS agencies.txt file.
public class Agency
{
    public string agency_id { get; set; }
    public string agency_name { get; set; }
    public string agency_url { get; set; }
    public string agency_timezone { get; set; }
}

public class NaptanStop
{
    public string ATCOCode { get; set; }
    public string NaptanCode { get; set; }
    public string CommonName { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string StopType { get; set; }
}