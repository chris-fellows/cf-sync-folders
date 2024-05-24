using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;
using CFUtilities.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFSyncFolders.Services
{
    public class SyncConfigurationService : ISyncConfigurationService
    {
        private IItemRepository _syncConfigurationRepository;

        public SyncConfigurationService(string folder)
        {
            _syncConfigurationRepository = new CFUtilities.XML.XmlItemRepository(folder);
        }

        public SyncConfiguration GetByDescription(string description)
        {
            return _syncConfigurationRepository.GetAll<SyncConfiguration>()
                    .FirstOrDefault(sc => sc.Description.Equals(description, StringComparison.InvariantCultureIgnoreCase));
        }

        public SyncConfiguration GetById(Guid id)
        {
            return _syncConfigurationRepository.GetAll<SyncConfiguration>().FirstOrDefault(sc => sc.ID == id);
        }

        public List<SyncConfiguration> GetAll()
        {
            return _syncConfigurationRepository.GetAll<SyncConfiguration>().OrderBy(sc => sc.Description).ToList();
        }

        public void Update(SyncConfiguration syncConfiguration)
        {
            _syncConfigurationRepository.Update(syncConfiguration.ID.ToString(), syncConfiguration);
        }

        public void Add(SyncConfiguration syncConfiguration)
        {
            _syncConfigurationRepository.Insert(syncConfiguration.ID.ToString(), syncConfiguration);
        }
    }
}
