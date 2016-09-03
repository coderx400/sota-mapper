using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotAMapper
{
    public static class Globals
    {
        public static string ExeDir { get; private set; }
        public static string DataDir { get; private set; }
        public static string MapDir { get; private set; }
        public static string IconDir { get; private set; }
        public static string LogDir { get; private set; }

        public static string LogWatcherTempFileName { get; private set; } = "_SotAMapper.tmp";
        public static string LogFilePattern { get; private set; } = "SotAChatLog_*.txt";

        static Globals()
        {
            var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            ExeDir = System.IO.Path.GetDirectoryName(exePath);
            DataDir = System.IO.Path.Combine(ExeDir, "data");
            MapDir = System.IO.Path.Combine(DataDir, "maps");
            IconDir = System.IO.Path.Combine(DataDir, "icons");
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LogDir = Path.Combine(appDataDir, "Portalarium", "Shroud of the Avatar", "ChatLogs");
        }
    }
}

