using System;
using System.Collections.Generic;
using CFSyncFolders.Models;

namespace CFSyncFolders.Interfaces
{
    /// <summary>
    /// Service for placeholders in string. E.g. Replaces "{machine}" with Environment.MachineName
    /// </summary>
    public interface IPlaceholderService
    {
        List<Placeholder> GetAll();

        string GetWithPlaceholdersReplaced(string input, Dictionary<string, object> parameters);
    }
}
