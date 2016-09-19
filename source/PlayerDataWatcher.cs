using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Documents;

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
            // regex used to match /loc output lines in the log file
            var locRE = new Regex(@"^\[(.+)\] +Area: +(.+) +\((.+)\) +Loc: +\((.+), *(.+), *(.+)\)");

            // regex used to match current player data content
            var cpdRE = new Regex(@"^PlayerLoc: (.+), (.+), (.+).*");

            // regex used to match area transitions
            var areaChangeRE = new Regex(@"^\[(.+)\] Entering (.+) from (.+)");

            string lastLoadedLogFile = null;
            DateTime lastLoadedLogFileTime = default(DateTime);

            PlayerData playerData = null;
            DateTime? playerDataAreaNameTime = null;
            DateTime? playerDataMapNameTime = null;
            DateTime? playerDataLocTime = null;

            PlayerData lastReportedPlayerData = null;

            string lastLoadedCPDFile = null;
            DateTime lastLoadedCPDFileTime = default(DateTime);

            while (true)
            {
                Log.WriteLine(">>> check player data - BEGIN");

                try
                {
                    bool playerDataChanged = false;

                    //
                    // check SotA log files
                    //

                    string latestModifiedLogFile = null;
                    DateTime latestModifiedLogFileTime = default(DateTime);

                    // find most recently modified log file
                    var logFiles = Directory.GetFiles(Globals.LogDir, Globals.LogFilePattern);
                    var numLogFiles = logFiles?.Length ?? 0;
                    Log.WriteLine($"found {numLogFiles} log files");
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

                            Log.WriteLine("loading log file, " + lastLoadedLogFile);

                            // copy to temp file
                            var tempFilePath = Path.GetTempFileName();
                            File.Copy(latestModifiedLogFile, tempFilePath, true);

                            // load temp file
                            var lines = File.ReadAllLines(tempFilePath);
                            if (lines?.Length > 0)
                            {
                                // read backwards through lines in log
                                for (int i = lines.Length - 1; i >= 0; i--)
                                {
                                    var line = lines[i];

                                    // if this is an area change output line
                                    var mAC = areaChangeRE.Match(line);
                                    if (mAC?.Success ?? false)
                                    {
                                        Log.WriteLine("found area change, " + line);

                                        var dateTimeStr = mAC.Groups[1].Value;
                                        var newAreaName = mAC.Groups[2].Value;
                                        var oldAreaName = mAC.Groups[3].Value;

                                        DateTime dateTime;
                                        if (DateTime.TryParse(dateTimeStr, out dateTime))
                                        {
                                            // area change updates the area name and nulls out the map name
                                            // since we know map has changed, but don't know the new map name

                                            if (playerData == null)
                                            {
                                                playerData = new PlayerData(newAreaName, null, null);
                                                playerDataAreaNameTime = dateTime;
                                                playerDataMapNameTime = dateTime;
                                                playerDataLocTime = dateTime;
                                                playerDataChanged = true;
                                            }
                                            else
                                            {
                                                if ((playerDataAreaNameTime == null) ||
                                                    (dateTime > playerDataAreaNameTime))
                                                {
                                                    playerData = new PlayerData(newAreaName, playerData.MapName,
                                                        playerData.Loc);
                                                    playerDataAreaNameTime = dateTime;
                                                    playerDataChanged = true;
                                                }

                                                if ((playerDataMapNameTime == null) ||
                                                    (dateTime > playerDataMapNameTime))
                                                {
                                                    playerData = new PlayerData(playerData.AreaName, null,
                                                        playerData.Loc);
                                                    playerDataMapNameTime = dateTime;
                                                    playerDataChanged = true;
                                                }
                                            }
                                        }
                                    }

                                    // if this is a /loc output line
                                    var m = locRE.Match(line);
                                    if (m?.Success ?? false)
                                    {
                                        Log.WriteLine("found loc, " + line);

                                        var dateTimeStr = m.Groups[1].Value;
                                        var areaName = m.Groups[2].Value;
                                        var mapName = m.Groups[3].Value;
                                        var xStr = m.Groups[4].Value;
                                        var yStr = m.Groups[5].Value;
                                        var zStr = m.Groups[6].Value;

                                        DateTime dateTime;
                                        float x, y, z;
                                        if (DateTime.TryParse(dateTimeStr, out dateTime) &&
                                            float.TryParse(xStr, out x) &&
                                            float.TryParse(yStr, out y) &&
                                            float.TryParse(zStr, out z))
                                        {
                                            Log.WriteLine("parsed loc line OK");

                                            // /loc output line replaces all fields (name and cood)

                                            if (playerData == null)
                                            {
                                                playerData = new PlayerData(areaName, mapName, new MapCoord(x, y, z));
                                                playerDataAreaNameTime = dateTime;
                                                playerDataMapNameTime = dateTime;
                                                playerDataLocTime = dateTime;
                                                playerDataChanged = true;
                                            }
                                            else
                                            {
                                                if ((playerDataAreaNameTime == null) ||
                                                    (dateTime > playerDataAreaNameTime))
                                                {
                                                    playerData = new PlayerData(areaName, playerData.MapName,
                                                        playerData.Loc);
                                                    playerDataAreaNameTime = dateTime;
                                                    playerDataChanged = true;
                                                }

                                                if ((playerDataMapNameTime == null) ||
                                                    (dateTime > playerDataMapNameTime))
                                                {
                                                    playerData = new PlayerData(playerData.AreaName, mapName,
                                                        playerData.Loc);
                                                    playerDataMapNameTime = dateTime;
                                                    playerDataChanged = true;
                                                }

                                                if ((playerDataLocTime == null) ||
                                                    (dateTime > playerDataLocTime))
                                                {
                                                    playerData = new PlayerData(playerData.AreaName, playerData.MapName,
                                                        new MapCoord(x, y, z));
                                                    playerDataLocTime = dateTime;
                                                    playerDataChanged = true;
                                                }
                                            }

                                            // since /loc output includes all fields in player data, no need to
                                            // keep searching backwards through the log file as this would invalidate
                                            // any prior data
                                            break;
                                        }
                                    }
                                }
                            }

                            // delete temp file
                            File.Delete(tempFilePath);
                        }
                        else
                            Log.WriteLine("no changes to log file since last check");
                    }
                    else
                        Log.WriteLine("no log files to consider");

                    //
                    // check CurrentPlayerData file in SotA install dir
                    //

                    string latestModifiedCPDFile = null;
                    DateTime latestModifiedCPDFileTime = default(DateTime);

                    foreach (var instDir in Globals.SotAInstallDirs)
                    {
                        var cpdFile = Path.Combine(instDir, Globals.CPDFileName);
                        if (File.Exists(cpdFile))
                        {
                            var lastModTime = File.GetLastWriteTime(cpdFile);

                            if ((latestModifiedCPDFile == null) ||
                                (lastModTime > latestModifiedCPDFileTime))
                            {
                                latestModifiedCPDFile = cpdFile;
                                latestModifiedCPDFileTime = lastModTime;
                            }
                        }
                    }

                    if (latestModifiedCPDFile != null)
                    {
                        if ((lastLoadedCPDFile == null) ||
                            (latestModifiedCPDFileTime > lastLoadedCPDFileTime))
                        {
                            lastLoadedCPDFile = latestModifiedCPDFile;
                            lastLoadedCPDFileTime = latestModifiedCPDFileTime;

                            Log.WriteLine("loading CPD file, " + lastLoadedCPDFile);

                            // copy to temp file
                            var tempFilePath = Path.GetTempFileName();
                            File.Copy(latestModifiedCPDFile, tempFilePath);

                            // load temp file
                            var cpdContent = File.ReadAllText(tempFilePath);

                            var m = cpdRE.Match(cpdContent);
                            if (m?.Success ?? false)
                            {
                                var xStr = m.Groups[1].Value;
                                var yStr = m.Groups[2].Value;
                                var zStr = m.Groups[3].Value;

                                float x, y, z;
                                if (float.TryParse(xStr, out x) &&
                                    float.TryParse(yStr, out y) &&
                                    float.TryParse(zStr, out z))
                                {
                                    Log.WriteLine("parsed CPD file OK");

                                    // current player data file updates only location

                                    if (playerData == null)
                                    {
                                        playerData = new PlayerData(null, null, new MapCoord(x,y,z));
                                        playerDataAreaNameTime = latestModifiedCPDFileTime;
                                        playerDataMapNameTime = latestModifiedCPDFileTime;
                                        playerDataLocTime = latestModifiedCPDFileTime;
                                        playerDataChanged = true;
                                    }
                                    else
                                    {
                                        if ((playerDataLocTime == null) ||
                                            (latestModifiedCPDFileTime > playerDataLocTime))
                                        {
                                            playerData = new PlayerData(playerData.AreaName, playerData.MapName, new MapCoord(x, y, z));
                                            playerDataLocTime = latestModifiedCPDFileTime;
                                            playerDataChanged = true;
                                        }
                                    }
                                }
                            }

                            // delete temp file
                            File.Delete(tempFilePath);
                        }
                        else
                            Log.WriteLine("no changes to CPD file since last check");
                    }
                    else
                        Log.WriteLine("no CPD file to consider");

                    // if player data has changed, notify
                    if (playerDataChanged && (playerData != null))
                    {
                        if ((lastReportedPlayerData == null) ||
                            !lastReportedPlayerData.Equals(playerData))
                        {
                            lastReportedPlayerData = new PlayerData(playerData);

                            OnPlayerDataChanged(playerData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine("EXCEPTION: " + ex.Message);
                }

                Log.WriteLine("<<< check player data - END");

                // wait a bit before trying again
                Thread.Sleep(_checkLogIntervalMS);
            }
        }
    }
}
