using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    /// <summary>
    /// Represents a single map loaded from a map data .csv file in "data/maps".
    /// Map data files have name,x,y,z
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Name of the map, will be the base filename of the map data file
        /// and what is reported by the /loc command
        /// </summary>
        public string Name { get; private set; }

        private List<MapItem> _items;

        /// <summary>
        /// Set of items loaded from the map data file
        /// </summary>
        public IEnumerable<MapItem> Items { get { return _items; } } 

        // extremes of the map data

        public MapCoord MinLoc { get; private set; }
        public MapCoord MaxLoc { get; private set; }

        public override string ToString()
        {
            if ((Name == null) || (Items == null) || (MinLoc == null) || (MaxLoc == null))
            {
                return "no data";
            }

            var sb = new StringBuilder();

            sb.Append("Name=" + Name + "\n");

            sb.Append("   MinLoc: " + MinLoc + "\n");
            sb.Append("   MaxLoc: " + MaxLoc + "\n");

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
                Name = Path.GetFileNameWithoutExtension(mapFile);
                _items = new List<MapItem>();
                MinLoc = null;
                MaxLoc = null;

                var lines = File.ReadLines(mapFile);

                float? 
                    minX = null, maxX = null, 
                    minY = null, maxY = null,
                    minZ = null, maxZ = null;

                foreach (var line in lines)
                {
                    var toks = line.Split(',');

                    if (toks.Length != 4)
                        continue;

                    float x, y, z;
                    if (!float.TryParse(toks[1], out x) ||
                        !float.TryParse(toks[2], out y) ||
                        !float.TryParse(toks[3], out z))
                    {
                        continue;
                    }

                    Utils.CheckAndSetMin(ref minX, x);
                    Utils.CheckAndSetMin(ref minY, y);
                    Utils.CheckAndSetMin(ref minZ, z);

                    Utils.CheckAndSetMax(ref maxX, x);
                    Utils.CheckAndSetMax(ref maxY, y);
                    Utils.CheckAndSetMax(ref maxZ, z);

                    var mapItem = new MapItem(toks[0], new MapCoord(x, y, z));

                    _items.Add(mapItem);
                }

                if ((minX != null) && (maxX != null) &&
                    (minY != null) && (maxY != null) &&
                    (minZ != null) && (maxZ != null))
                {
                    MinLoc = new MapCoord(minX.GetValueOrDefault(), minY.GetValueOrDefault(), minZ.GetValueOrDefault());
                    MaxLoc = new MapCoord(maxX.GetValueOrDefault(), maxY.GetValueOrDefault(), maxZ.GetValueOrDefault());
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
