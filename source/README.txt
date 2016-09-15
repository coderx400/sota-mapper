################################################################################
# SotAMapper
################################################################################

--- OVERVIEW -------------------------------------------------------------------

SotAMapper is a tool for SotA which uses a list of "items of interest" in a .csv
file and renders them graphically along with the current player position (which
is obtained from the SotA log files).

It is necessary to manually use the /loc command in game once each time the
player enters a map for which there is a map data .csv file to sync.

Initially the set of map data files is pretty limited.  The intent is that over
time people will create maps and contribute them (send them to 
coder1024@gmail.com) and they will be included in a future release.  So to see
this in action without creating any additional data, you'll need to go into a
map for which there is a map data file and type /loc while running the app.

The idea also is that it provides an easy way for you to record locations
while playing on a map and see them graphically.

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

The map data .csv file must be plain text format, make sure you're text editor
isn't saving it as RTF or some other format.

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

You can also click the Add button in SotAMapper to add an item to the map at
the current player location.  With this feature, there's no need to type
in coordinate values!

If you have Excel installed, you can double-click on the .csv files and edit
them in that, just make sure to save them out as .csv if prompted.

--- MAP ICONS ------------------------------------------------------------------

By default, map items are rendered as text labels with dots at the map location.
If a .PNG file is present in the "data/icons" directory which matches the name
of the item in the map data .csv file, then the image will be shown instead.

################################################################################
# Contributors
################################################################################

Project Creator, Programmer

    coder1024 (coder1024@gmail.com)

Data Files (Maps, Icons, etc.)

    LiquidSky
    lollie

Testing Feedback, Suggestions, Bug Reports

    Berek
    Bom
    Bushmaster
    cartodude
    Frostll
    Kabalyero
    LiquidSky
    lollie
    moko
    Umuri
                                        
################################################################################
# Known Issues
################################################################################

- some maps seem to have reversed coordinate systems, Berek confirmed that this
  is to be expected for some of the maps  

################################################################################
# Feature Requests
################################################################################

- ability to create a new map file using the name reported by SotA, saving the
  need to manually create it with a matching filename
- add a background image which could represent the local area better than
  just the black background
- zoom function
- pre-canned Add functionality to save the need to type out common names for
  resource types, etc.
- edit item, change name
- remove item for accidentally added items

################################################################################
# History
################################################################################

NEXT VERSION
- data updates
- when using the Add button to add a new map item, the min/max extents of the
  data are re-computed so the rendering doesn't get wonky as you add items when
  building a new map
- empty map file is now tolerated and handled correctly, this allows running
  the app with an empty map file and adding all items using the Add button and
  now (for real this time!) never having to manually enter any coordinates

2016.09.11, v1.3
- added TopMost checkbox, checking it will attempt to keep the window on top
  of other windows
- added settings.ini file which is created when run (if it doesn't exist) and
  which will persist settings (like TopMost) between runs.  The settings.ini
  file is stored in "%APPDATA%\SotAMapper".  there is NO NEED to hand edit
  this file, there is only 1 setting so far (the TopMost setting) and its
  set from the UI.
- fixed string formatting bug with MapCoord and spaced out values
  for readability
- there is now an Add button which allows adding a map item at the current
  player location to the current map file.  no more need to type in coordinate
  values!
- when loading map .csv files, ignore lines with empty item names

2016.09.08, v1.2
- added status bar which shows name of map item on mouse over for items which
  render as images (due to the presence of a .PNG with matching name in the
  data/icons folder)
- added link to forum discussion thread in status bar
- data updates (maps, icons), added map for Highvale
- fixed upper/lower case bug when case of map file didn't match what was
  reported by SotA
- fixed crash when map data .csv file was empty (no items)
- added descriptive error messages which render in red when the "NO DATA"
  condition occurs to help in identifying why a map is not showing

2016.09.04, v1.1
- player position updates automatically on map while in a map for which there is
  a map data .csv file
- when first entering a map, it is necessary to do 1 manual /loc to sync

2016.09.03, v1.0
- initial release
