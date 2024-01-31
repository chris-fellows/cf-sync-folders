using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFSyncFolders
{
    /// <summary>
    /// Interface for logging
    /// </summary>
    public interface ILog
    {
        void Write(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption);
    }
}
