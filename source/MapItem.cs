using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    /// <summary>
    /// A single item on a map represented as one line in a map data .csv file
    /// </summary>
    public class MapItem
    {
        public string Name { get; private set; }
        public MapCoord Coord { get; private set; }

        public MapItem(string name, MapCoord coord)
        {
            Name = name;
            Coord = coord;
        }

        public override string ToString()
        {
            return $"Name={Name}, Coord={Coord}";
        }
    }
}
