using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;
using CFUtilities.XML;
using System;

namespace CFSyncFolders.Services
{
    public class SyncConfigurationService : XmlItemRepository<SyncConfiguration, Guid>, ISyncConfigurationService
    {
        //private IItemRepository _syncConfigurationRepository;

        public SyncConfigurationService(string folder) : base(folder, (SyncConfiguration syncConfiguration) => syncConfiguration.ID)
        {
           
        }        
    }
}
