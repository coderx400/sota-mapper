using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SotAMapper
{
    public static class Globals
    {
        public static string ExeDir { get; private set; }
        public static string DataDir { get; private set; }
        public static string MapDir { get; private set; }
        public static string IconDir { get; private set; }
        public static string LogDir { get; private set; }

        public static string SettingsFilePath { get; private set; }

        public static readonly string SettingsFileName = "settings.ini";
        public static readonly string TempFileName = "_SotAMapper.tmp";
        public static readonly string LogFilePattern = "SotAChatLog_*.txt";
        public static readonly string CPDFileName = "CurrentPlayerData.txt";

        private static List<string> _sotaInstallDirs; 
        public static IEnumerable<string> SotAInstallDirs => _sotaInstallDirs;

        static Globals()
        {
            var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            ExeDir = System.IO.Path.GetDirectoryName(exePath);
            DataDir = System.IO.Path.Combine(ExeDir, "data");
            MapDir = System.IO.Path.Combine(DataDir, "maps");
            IconDir = System.IO.Path.Combine(DataDir, "icons");
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LogDir = Path.Combine(appDataDir, "Portalarium", "Shroud of the Avatar", "ChatLogs");

            SettingsFilePath = System.IO.Path.Combine(ExeDir, SettingsFileName);

            _sotaInstallDirs = FindSotAInstallDirs();
        }

        private static List<string> FindSotAInstallDirs()
        {
            var result = new List<string>();

            var baseKeys = new List<RegistryKey>
            {
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32),
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
            };
            try
            {
                foreach (var baseKey in baseKeys)
                {
                    const string uninstKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                    using (var uninstKey = baseKey.OpenSubKey(uninstKeyPath))
                    {
                        if (uninstKey != null)
                        {
                            var appKeyNames = uninstKey.GetSubKeyNames();
                            if (appKeyNames != null)
                            {
                                foreach (var appKeyName in appKeyNames)
                                {
                                    using (var appKey = uninstKey.OpenSubKey(appKeyName))
                                    {
                                        if (appKey != null)
                                        {
                                            var displayName = appKey.GetValue("DisplayName") as string;
                                            var installDir = appKey.GetValue("InstallLocation") as string;

                                            if ((displayName != null) &&
                                                (installDir != null))
                                            {
                                                if ((string.Compare(displayName, "Shroud of the Avatar", true) == 0) ||
                                                    (string.Compare(displayName, "Shroud of the Avatar: Forsaken Virtues") == 0))
                                                {
                                                    result.Add(installDir);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                foreach (var baseKey in baseKeys)
                {
                    baseKey?.Dispose();
                }
            }

            return result;
        }
    }
}

