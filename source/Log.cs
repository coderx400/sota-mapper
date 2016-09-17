using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SotAMapper
{
    public static class Log
    {
        public static bool Enabled { get; set; } = false;

        public static void WriteLine(string s)
        {
            if (!Enabled)
                return;

            var le = new LogEntry
            {
                LogTime = DateTime.Now,
                LogText = s
            };

            LogWorker.AddLogEntry(le);
        }
    }

    class LogEntry
    {
        public DateTime LogTime { get; set; }
        public string LogText { get; set; }

        public override string ToString()
        {
            return LogTime.ToString("yyyy.MM.dd@HH:mm:ss") + " " + LogText;
        }
    }

    static class LogWorker
    {
        private static ConcurrentQueue<LogEntry> _logEntryQueue;
        private static Thread _logWorkerThread;

        private static readonly int _checkDelayMS = 250;

        static LogWorker()
        {
            _logEntryQueue = new ConcurrentQueue<LogEntry>();
            _logWorkerThread = new Thread(WorkerThread) { IsBackground = true };
            _logWorkerThread.Start();
        }

        public static void AddLogEntry(LogEntry le)
        {
            _logEntryQueue.Enqueue(le);
        }

        private static void WorkerThread()
        {
            var entries = new List<LogEntry>();
            var lines = new List<string>();

            while (true)
            {
                LogEntry nextEntry;
                while (_logEntryQueue.TryDequeue(out nextEntry))
                    entries.Add(nextEntry);

                foreach (var entry in entries)
                    lines.Add(entry.ToString());

                File.AppendAllLines(Globals.AppLogFile,lines);

                entries.Clear();
                lines.Clear();

                Thread.Sleep(_checkDelayMS);
            }
        }
    }
}
