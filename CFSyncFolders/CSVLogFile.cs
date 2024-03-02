using CFUtilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace CFSyncFolders
{
    /// <summary>
    ///  CSV log file
    /// </summary>
    public class CSVLogFile : ILog
    {
        private class LogEntry
        {
            public DateTime Time { get; set; }

            public string Action { get; set; }

            public string Item1 { get; set; }

            public string ItemData1 { get; set; }

            public string Item2 { get; set; }

            public string ItemData2 { get; set; }

            public Exception Exception { get; set; }
        }

        private string _logFile = "";  // May contain placeholders
        private List<LogEntry> _logEntries = new List<LogEntry>();
        private DateTime _lastFlush = DateTime.MinValue;

        public CSVLogFile(string logFile)
        {
            _logFile = logFile;            
        }

        private string GetLogFile(DateTime dateTime)
        {
            string logFile = _logFile.Replace("{date}", string.Format("{0}-{1}", dateTime.Month, dateTime.Year))
                        .Replace("{user}", Environment.UserName)
                        .Replace("{machine}", Environment.MachineName);
            return logFile;  
        }

        //public string File
        //{
        //    get { return _logFile; }
        //}

        private void WriteHeaders(string logFile)
        {
            if (!String.IsNullOrEmpty(logFile))
            {
                Char delimiter = (Char)9;
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", delimiter, "Time", "Action", "Item1", "Item1Data", "Item2", "Item2Data", "Exception"));
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        private void WriteInternal(IEnumerable<LogEntry> logEntries)
        {
            Char delimiter = (Char)9;

            if (!String.IsNullOrEmpty(_logFile))
            {
                string logFile = GetLogFile(DateTime.Now);

                string folder = Path.GetDirectoryName(logFile);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                if (!System.IO.File.Exists(logFile))
                {
                    WriteHeaders(logFile);
                }

                DateTime timeout = DateTime.UtcNow.AddSeconds(30);
                bool success = false;
                do
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(logFile, true))
                        {
                            foreach (var logEntry in logEntries)
                            {
                                writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}", delimiter, logEntry.Time, 
                                            logEntry.Action, logEntry.Item1, logEntry.ItemData1,
                                            logEntry.Item2, logEntry.ItemData2,
                                            (logEntry.Exception == null ? "" : logEntry.Exception.Message)));
                            }
                            writer.Flush();
                            writer.Close();
                            success = true;
                        }
                    }
                    catch (System.Exception exception)
                    {
                        if (IOUtilities.IsFileInUseByAnotherProcess(exception) && DateTime.UtcNow < timeout)   // Locked by another process
                        {
                            System.Threading.Thread.Sleep(200);    // Wait before retry
                        }
                        else
                        {
                            throw;
                        }
                    }
                } while (!success);
            }
        }

        private void FlushLogIfRequired()
        {
            TimeSpan flushFrequency = TimeSpan.FromSeconds(10);
            if (_lastFlush.Add(flushFrequency) <= DateTime.UtcNow)
            {
                _lastFlush = DateTime.UtcNow;

                // Get log entries
                List<LogEntry> logEntries = new List<LogEntry>();
                logEntries.AddRange(_logEntries);
                _logEntries.Clear();

                // Write to log
                WriteInternal(logEntries);
            }
        }

        public void Write(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption)
        {
            if (String.IsNullOrEmpty(action))   // Bit of a hack, force flush log
            {
                FlushLogIfRequired();
            }
            else
            {
                // Queue log entry
                LogEntry logEntry = new LogEntry()
                {
                    Time = DateTime.UtcNow,
                    Action = action,
                    Item1 = item1,
                    ItemData1 = item1Data,
                    Item2 = item2,
                    ItemData2 = item2Data,
                    Exception = exeption
                };
                _logEntries.Add(logEntry);

                FlushLogIfRequired();
            }
        }

        public void DeleteBefore(DateTimeOffset beforeDate)
        {            
           
        }
    }
}
