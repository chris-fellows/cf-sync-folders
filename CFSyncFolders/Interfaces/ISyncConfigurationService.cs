using CFSyncFolders.Models;
using System;
using CFUtilities.Repository;

namespace CFSyncFolders.Interfaces
{
    /// <summary>
    /// Sync configuration service for managing SyncConfiguration instances
    /// </summary>
    public interface ISyncConfigurationService : IItemRepository<SyncConfiguration, Guid>
    {

    }
}
