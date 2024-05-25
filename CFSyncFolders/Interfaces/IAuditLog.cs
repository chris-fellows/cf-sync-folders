using System;

namespace CFSyncFolders.Interfaces
{
    /// <summary>
    /// Interface for audit logging
    /// </summary>
    public interface IAuditLog
    {
        /// <summary>
        /// Logs action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="item1"></param>
        /// <param name="item1Data"></param>
        /// <param name="item2"></param>
        /// <param name="item2Data"></param>
        /// <param name="exeption"></param>
        void LogAction(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption);

        /// <summary>
        /// Deletes logs before the date
        /// </summary>
        /// <param name="dateTime"></param>
        void DeleteBefore(DateTime dateTime);
    }
}
