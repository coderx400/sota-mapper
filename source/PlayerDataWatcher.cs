using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace SotAMapper
{
    public delegate void PlayerDataChangedEventHandler(PlayerData newPlayerData);

    /// <summary>
    /// Watches the SotA log files for updated player data generated with the /loc command.
    /// </summary>
    public class PlayerDataWatcher
    {
        /// <summary>
        /// how often to check the log files in milliseconds
        /// </summary>
        private const int _checkLogIntervalMS = 1000;

        public event PlayerDataChangedEventHandler PlayerDataChanged;

        /// <summary>
        /// Notify listeners that player data has changed
        /// </summary>
        protected virtual void OnPlayerDataChanged(PlayerData newPlayerData)
        {
            if (PlayerDataChanged != null)
                PlayerDataChanged(newPlayerData);
        }

        public void Start()
        {
            var t = new Thread(Worker) { IsBackground = true };
            t.Start();
        }

        private void Worker()
        {
            var tmpFile = Path.Combine(Globals.LogDir, Globals.LogWatcherTempFileName);

            // regex used to match /loc output lines in the log file
            var locRE = new Regex(@"^\[.+\] +Area: +(.+) +\((.+)\) +Loc: +\((.+), *(.+), *(.+)\)");

            string lastLoadedLogFile = null;
            DateTime lastLoadedLogFileTime = default(DateTime);

            PlayerData lastReportedPlayerData = null;

            while (true)
            {
                try
                {
                    string latestModifiedLogFile = null;
                    DateTime latestModifiedLogFileTime = default(DateTime);

                    // find most recently modified log file
                    var logFiles = Directory.GetFiles(Globals.LogDir, Globals.LogFilePattern);
                    foreach (var logFile in logFiles)
                    {
                        var lastModTime = File.GetLastWriteTime(logFile);

                        if ((latestModifiedLogFile == null) ||
                            (lastModTime > latestModifiedLogFileTime))
                        {
                            latestModifiedLogFile = logFile;
                            latestModifiedLogFileTime = lastModTime;
                        }
                    }

                    if (latestModifiedLogFile != null)
                    {
                        // has the most recently modified log file changed since we last checked it?
                        if ((lastLoadedLogFile == null) ||
                            (latestModifiedLogFileTime > lastLoadedLogFileTime))
                        {
                            lastLoadedLogFile = latestModifiedLogFile;
                            lastLoadedLogFileTime = latestModifiedLogFileTime;

                            // copy to temp file
                            File.Copy(latestModifiedLogFile,tmpFile,true);

                            // load temp file
                            var lines = File.ReadAllLines(tmpFile);
                            if (lines?.Length > 0)
                            {
                                // read backwards through lines in log
                                for (int i = lines.Length - 1; i >= 0; i--)
                                {
                                    var line = lines[i];

                                    // if this is a /loc output line
                                    var m = locRE.Match(line);
                                    if (m?.Success ?? false)
                                    {
                                        var areaName = m.Groups[1].Value;
                                        var mapName = m.Groups[2].Value;
                                        var xStr = m.Groups[3].Value;
                                        var yStr = m.Groups[4].Value;
                                        var zStr = m.Groups[5].Value;

                                        float x, y, z;
                                        if (float.TryParse(xStr, out x) &&
                                            float.TryParse(yStr, out y) &&
                                            float.TryParse(zStr, out z))
                                        {
                                            var playerData = new PlayerData(areaName, mapName, new MapCoord(x, y, z));

                                            // if player data has never been reported or if it has changed since we last
                                            // reported it, then notify listeners of the change
                                            if ((lastReportedPlayerData == null) ||
                                                !lastReportedPlayerData.Equals(playerData))
                                            {
                                                lastReportedPlayerData = playerData;

                                                OnPlayerDataChanged(playerData);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }

                            // delete temp file
                            File.Delete(tmpFile);
                        }
                    }
                }
                catch (Exception)
                {
                }

                // wait a bit before trying again
                Thread.Sleep(_checkLogIntervalMS);
            }
        }
    }
}
