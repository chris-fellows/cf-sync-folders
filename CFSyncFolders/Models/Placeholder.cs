using System;
using System.Collections.Generic;

namespace CFSyncFolders.Models
{
    /// <summary>
    /// Placeholder that can appear in a string. E.g. "{machine}", "{date:yyyy-MM-dd}"
    /// </summary>
    public class Placeholder
    {
        /// <summary>
        /// Name. E.g. "{machine}", "{date:yyyy-MM-dd}". Anything after : is a parameter that will be used
        /// by GetValue.
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Description of placeholder
        /// </summary>
        public string Description { get; set; } = String.Empty;

        /// <summary>
        /// Whether instance can handle replacing this placeholder
        /// </summary>
        public Func<string, bool> CanGetValue { get; set; }

        /// <summary>
        /// Function that returns placeholder value
        /// </summary>
        public Func<string, Dictionary<string, object>, string> GetValue { get; set; }
    }
}
