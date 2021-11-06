# NPR and HS2 GTFS Creation Tool
A .NET6 (C#) console application to create public timetables in GTFS format for Northern Powerhouse Rail and High Speed Two. Open the .sln file with Visual Studio 2022 (or newer) to get started.

Outputs are included at the top level for immediate use in journey planning software like Open Trip Planner, GraphHopper, or Navitia.

New services could be added quite easily following the patterns in the examples. Current timetables use existing GB rail stations to ensure that passengers can link with existing public transport and the street network.

## Testing
Both timetables have been tested for journeys on 2019-09-10 using Open Trip Planner 2. They are valid GTFS and should work in all software. Because they stop at existing GB rail stations (Leeds, Liverpool Lime Street, Birmingham International, London Euston etc,...) they will work as part of the existing GB public transport network.
#### NPR
![NPR timetable in Open Trip Planner 2](NPR_ExampleInOTP2.jpg?raw=true "NPR timetable in Open Trip Planner 2")
#### HS2
![HS2 timetable in Open Trip Planner 2](HS2_ExampleInOTP2.jpg?raw=true "HS2 timetable in Open Trip Planner 2")
