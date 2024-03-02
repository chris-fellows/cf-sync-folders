using System;

namespace CFSyncFolders
{
    /// <summary>
    /// Interface for logging
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Writes log entry
        /// </summary>
        /// <param name="action"></param>
        /// <param name="item1"></param>
        /// <param name="item1Data"></param>
        /// <param name="item2"></param>
        /// <param name="item2Data"></param>
        /// <param name="exeption"></param>
        void Write(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption);

        /// <summary>
        /// Deletes log entries before date
        /// </summary>
        /// <param name="beforeDate"></param>
        void DeleteBefore(DateTimeOffset beforeDate);
    }
}
