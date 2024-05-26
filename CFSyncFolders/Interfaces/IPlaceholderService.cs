using System.Collections.Generic;
using CFSyncFolders.Models;

namespace CFSyncFolders.Interfaces
{
    /// <summary>
    /// Service for placeholders in string. E.g. Replaces "{machine}" with Environment.MachineName
    /// </summary>
    public interface IPlaceholderService
    {
        /// <summary>
        /// Returns all supported placeholders
        /// </summary>
        /// <returns></returns>
        List<Placeholder> GetAll();

        /// <summary>
        /// Replaces all placeholders in input
        /// </summary>
        /// <param name="input">String to process</param>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>String with all placeholders replaced</returns>
        string GetWithPlaceholdersReplaced(string input, Dictionary<string, object> parameters);
    }
}
