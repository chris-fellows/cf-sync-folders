using CFSyncFolders.Models;
using System;
using System.Collections.Generic;

namespace CFSyncFolders.Interfaces
{
    /// <summary>
    /// Sync configuration service for managing SyncConfiguration instances
    /// </summary>
    public interface ISyncConfigurationService
    {
        SyncConfiguration GetByDescription(string description);

        SyncConfiguration GetById(Guid id);

        List<SyncConfiguration> GetAll();

        void Update(SyncConfiguration syncConfiguration);

        void Add(SyncConfiguration syncConfiguration);
    }
}
