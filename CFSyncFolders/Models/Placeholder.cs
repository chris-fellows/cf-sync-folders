using System;
using System.Collections.Generic;

namespace CFSyncFolders.Models
{
    /// <summary>
    /// Placeholder that can appear in a string. E.g. "{machine}" would be replaced with the
    /// current machine name.
    /// </summary>
    public class Placeholder
    {
        /// <summary>
        /// Name. E.g. "{machine}"
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Description of placeholder
        /// </summary>
        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// Function that returns placeholder value. Optional parameters passed in.
        /// </summary>
        public Func<Dictionary<string, object>, string> GetValue { get; set; }
    }
}
