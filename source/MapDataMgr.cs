using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SotAMapper
{
    /// <summary>
    /// Loads all avail. maps from "data/maps"
    /// </summary>
    public class MapDataMgr
    {
        private Dictionary<string, Map> _loadedMaps;

        public Map GetMap(string name)
        {
            if (name == null)
                return null;

            Map result = null;
            if (!_loadedMaps.TryGetValue(name.ToUpper(), out result))
                return null;
            return result;
        }

        public void Load()
        {
            try
            {
                Log.WriteLine("loading map .csv files");

                _loadedMaps = new Dictionary<string, Map>();

                var mapFiles = Directory.GetFiles(Globals.MapDir, "*.csv");
                if (mapFiles != null)
                {
                    Log.WriteLine($"found {mapFiles.Length} map .csv files");

                    foreach (var mapFile in mapFiles)
                    {
                        var map = new Map();
                        if (!map.Load(mapFile))
                            continue;

                        _loadedMaps[map.Name.ToUpper()] = map;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
