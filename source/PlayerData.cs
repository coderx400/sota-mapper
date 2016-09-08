using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    /// <summary>
    /// current player data
    /// </summary>
    public class PlayerData
    {
        public string AreaName { get; private set; }
        public string MapName { get; private set; }
        public MapCoord Loc { get; private set; }

        public PlayerData(PlayerData rhs)
        {
            AreaName = rhs.AreaName;
            MapName = rhs.MapName;
            Loc = rhs.Loc;
        }

        public PlayerData(string areaName, string mapName, MapCoord loc)
        {
            AreaName = areaName;
            MapName = mapName;
            Loc = loc;
        }

        public override string ToString()
        {
            return $"Area={AreaName ?? "null"}, Map={MapName ?? "null"}, Loc=" + (Loc?.ToString() ?? "null");
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as PlayerData;
            if (rhs == null)
                return false;

            if ((rhs.AreaName != AreaName) ||
                (rhs.MapName != MapName) ||
                !rhs.Loc.Equals(Loc))
            {
                return false;
            }

            return true;
        }
    }
}
