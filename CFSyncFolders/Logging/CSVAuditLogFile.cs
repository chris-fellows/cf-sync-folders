using CFUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using CFSyncFolders.Interfaces;
using System.Linq;

namespace CFSyncFolders.Log
{
    /// <summary>
    ///  CSV log file
    /// </summary>
    public class CSVAuditLogFile : IAuditLog
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

        private readonly Char _delimiter;
        private readonly string _logFile = "";  // May contain placeholders
        private List<LogEntry> _logEntries = new List<LogEntry>();
        private DateTime _lastFlush = DateTime.MinValue;
        private readonly IPlaceholderService _placeholderService;

        public CSVAuditLogFile(Char delimiter, string logFile, IPlaceholderService placeholderService)
        {
            _delimiter = delimiter;
            _logFile = logFile;
            _placeholderService = placeholderService;
        }

        private string GetLogFile(DateTime dateTime)
        {
            return _placeholderService.GetWithPlaceholdersReplaced(_logFile,
                                                new Dictionary<string, object>() { { "date", dateTime } });          
        }
        
        private void WriteHeaders(string logFile)
        {
            if (!String.IsNullOrEmpty(logFile))
            {                
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", _delimiter,
                                        "Time", "Machine", "Action", "Item1", "Item1Data", "Item2", "Item2Data", "Exception"));
                    writer.Flush();
                    writer.Close();
                }
            }            
        }

        private void WriteInternal(IEnumerable<LogEntry> logEntries)
        {            
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
                                writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", _delimiter, logEntry.Time, Environment.MachineName,
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

        /// <summary>
        /// Flushes log if forced, too many queued or time overdue
        /// </summary>
        /// <param name="force"></param>
        private void FlushLogIfRequired(bool force)
        {
            TimeSpan flushFrequency = TimeSpan.FromSeconds(10);
            if (force || 
                _logEntries.Count >= 500 ||      // Limit number of log entries stored in memory
                _lastFlush.Add(flushFrequency) <= DateTime.UtcNow)  // Time overdue
            {
                _lastFlush = DateTime.UtcNow;

                if (_logEntries.Any())
                {
                    // Get log entries
                    var logEntries = new List<LogEntry>();
                    logEntries.AddRange(_logEntries);
                    _logEntries.Clear();

                    // Write to log
                    WriteInternal(logEntries);
                }
            }
        }

        public void LogAction(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption)
        {
            if (String.IsNullOrEmpty(action))   // Bit of a hack, force flush log
            {
                FlushLogIfRequired(true);
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

                FlushLogIfRequired(false);
            }
        }

        public void DeleteBefore(DateTime dateTime)
        {           
            // Delete old logs until we get N consecutive days with no logs            
            var countConsecutiveFailed = 0;
            do
            {
                var logFile = GetLogFile(dateTime);
                if (File.Exists(logFile))
                {
                    countConsecutiveFailed = 0;
                    File.Delete(logFile);
                }
                else
                {
                    countConsecutiveFailed++;
                }
                dateTime = dateTime.AddDays(-1);
            } while (countConsecutiveFailed < 30);
        }
    }
}
