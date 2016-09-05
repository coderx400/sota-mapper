################################################################################
# SotAMapper
################################################################################

--- OVERVIEW -------------------------------------------------------------------

SotAMapper is a tool for SotA which uses a list of "items of interest" in a .csv
file and renders them graphically along with the current player position (which
is obtained from the SotA log files).

It is necessary to manually use the /loc command in game once each time the
player enters a map for which there is a map data .csv file to sync.

Initially there's only a single map data file for the "East Perennial Trail"
adventure map.  So to see this in action without creating any additional data,
you'll need to go into that zone and type /loc while running the app.  The idea
is that you can create your own map data .csv files with items of interest that
you discover on other maps.

If you create your own files and would like them included in a future update,
feel free to send them to coder1024@gmail.com and I will include them.

--- SYSTEM REQUIREMENTS --------------------------------------------------------

- Windows 7 or later
- .NET 4.5.2 Framework
  - can download this from the below or just search for .NET 4.5.2 runtime
    https://www.microsoft.com/en-us/download/details.aspx?id=42643
- SotAMapper is a C# .NET application created in Visual Studio 2015 (v14)
- Full source code for the application is included and is also posted on
  GitHub at https://github.com/coderx400/sota-mapper

--- MAP DATA FILES -------------------------------------------------------------

A separate .csv file is needed for each SotA map and should be in the
"data/maps" directory having a name matching the SotA map name as reported in
the /loc command.

For example, suppose the /loc command printed out the below.  In this case,
there should be a file "data/maps/Novia_R1_City_Soltown.csv" which contains
a list of items for that map.

```
Area: Soltown (Novia_R1_City_Soltown) Loc: (-15.7, 28.0, 23,2)
```

The map .csv file contains name and position for each item to be shown on the
map.  As you discover items on a map, you can add them to the .csv file and
they will be shown on the map (although you need to exit and restart the
application to show changes to the .csv file).  Here are some examples of items
in a map .csv file.  An example file is provided also as a starting point.

```
Name,X,Y,Z
Cotton,-93.2,19.1,21.8
Bear Cave,-88,18.8,-88.9
Mandrake,-53.8,18.5,3.5
```

Note that this tool only uses the X and Z values, but all (X, Y, and Z) are
expected in the file.  This is done to avoid confusion between going from what
is shown in game and what goes in the file.  The middle value (Y) represents
the elevation which, of course, is not used for a 2D map.  Having it here
for completeness may help if this data were to be used in other tools also.

When building a map .csv file, the /loctrack command can be helpful.  This
causes the location to be continually shown on screen.

If you have Excel installed, you can double-click on the .csv files and edit
them in that, just make sure to save them out as .csv if prompted.

--- MAP ICONS ------------------------------------------------------------------

By default, map items are rendered as text labels with dots at the map location.
If a .PNG file is present in the "data/icons" directory which matches the name
of the item in the map data .csv file, then the image will be shown instead.

################################################################################
# History
################################################################################

2016.09.03, v1.0
- initial release

2016.09.04, v1.1
- player position updates automatically on map while in a map for which there is
  a map data .csv file
- when first entering a map, it is necessary to do 1 manual /loc to sync
