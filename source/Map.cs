using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    // used to indicate which coord sys is in use for the map
    public enum CoordSysType
    {
        //
        //       X
        //       |
        //       |
        //  Z----*
        //
        XZ_NorthWest,

        //
        //  Z
        //  |
        //  |
        //  *----X
        //
        ZX_NorthEast
    }

    /// <summary>
    /// Represents a single map loaded from a map data .csv file in "data/maps".
    /// Map data files have name,x,y,z
    /// </summary>
    public class Map
    {
        public string MapFilePath { get; private set; }

        /// <summary>
        /// Name of the map, will be the base filename of the map data file
        /// and what is reported by the /loc command
        /// </summary>
        public string Name { get; private set; }

        public CoordSysType MapCoordSys { get; private set; }

        private List<MapItem> _items;

        /// <summary>
        /// Set of items loaded from the map data file
        /// </summary>
        public IEnumerable<MapItem> Items { get { return _items ?? new List<MapItem>(); } } 

        // extremes of the map data

        public MapCoord MinLoc { get; private set; }
        public MapCoord MaxLoc { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Name=" + (Name ?? "null") + "\n");

            sb.Append("   MinLoc: " + (MinLoc?.ToString() ?? "null") + "\n");
            sb.Append("   MaxLoc: " + (MaxLoc?.ToString() ?? "null") + "\n");

            foreach (var mapItem in Items)
            {
                sb.Append("   ");
                sb.Append(mapItem);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Load up items from map data file
        /// </summary>
        public bool Load(string mapFile)
        {
            try
            {
                Log.WriteLine($"LOAD: {mapFile}");

                MapFilePath = mapFile;
                Name = Path.GetFileNameWithoutExtension(mapFile);
                MapCoordSys = CoordSysType.XZ_NorthWest;
                _items = new List<MapItem>();
                MinLoc = null;
                MaxLoc = null;

                var lines = File.ReadLines(mapFile);

                const int maxNumItemsToLog = 3;

                foreach (var line in lines)
                {
                    var toks = line.Split(',');

                    if (toks.Length == 2)
                    {
                        if (string.Compare(toks[0].Trim(), "MapCoordSys", true) == 0)
                        {
                            CoordSysType tmp;
                            if (CoordSysType.TryParse(toks[1].Trim(), true, out tmp))
                            {
                                MapCoordSys = tmp;
                                Log.WriteLine("coord sys specified: " + MapCoordSys);
                            }
                        }
                    }

                    else if (toks.Length == 4)
                    {
                        float x, y, z;
                        if (!float.TryParse(toks[1], out x) ||
                            !float.TryParse(toks[2], out y) ||
                            !float.TryParse(toks[3], out z))
                        {
                            continue;
                        }

                        var itemName = toks[0].Trim();
                        if (itemName?.Length == 0)
                            continue;

                        var mapItem = new MapItem(itemName, new MapCoord(x, y, z));
                        _items.Add(mapItem);

                        if (_items.Count <= maxNumItemsToLog)
                        {
                            Log.WriteLine($"loaded map item: {mapItem}");
                        }

                        if (_items.Count == (maxNumItemsToLog + 1))
                        {
                            Log.WriteLine("... remaining items not logged");
                        }
                    }
                }

                Log.WriteLine($"loaded {_items.Count} map items");

                ComputeMinMaxValues();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"EXCEPTION: {ex.Message}");
                return false;
            }

            return true;
        }

        private void ComputeMinMaxValues()
        {
            MinLoc = null;
            MaxLoc = null;

            float?
                minX = null, maxX = null,
                minY = null, maxY = null,
                minZ = null, maxZ = null;

            foreach (var item in Items)
            {
                Utils.CheckAndSetMin(ref minX, item.Coord.X);
                Utils.CheckAndSetMin(ref minY, item.Coord.Y);
                Utils.CheckAndSetMin(ref minZ, item.Coord.Z);

                Utils.CheckAndSetMax(ref maxX, item.Coord.X);
                Utils.CheckAndSetMax(ref maxY, item.Coord.Y);
                Utils.CheckAndSetMax(ref maxZ, item.Coord.Z);
            }

            if ((minX != null) && (maxX != null) &&
                (minY != null) && (maxY != null) &&
                (minZ != null) && (maxZ != null))
            {
                MinLoc = new MapCoord(minX.GetValueOrDefault(), minY.GetValueOrDefault(), minZ.GetValueOrDefault());
                MaxLoc = new MapCoord(maxX.GetValueOrDefault(), maxY.GetValueOrDefault(), maxZ.GetValueOrDefault());
                Log.WriteLine($"MinLoc={MinLoc}, MaxLoc={MaxLoc}");
            }
            else
            {
                Log.WriteLine("unable to determine min/max map loc for map");
            }
        }

        public void AddMapItem(MapItem itm)
        {
            try
            {
                Log.WriteLine($"Add map item, {itm}");

                using (var sw = File.AppendText(MapFilePath))
                {
                    sw.WriteLine(itm.Name + ", " + itm.Coord);
                }

                if (_items == null)
                    _items = new List<MapItem>();

                _items.Add(itm);

                ComputeMinMaxValues();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
