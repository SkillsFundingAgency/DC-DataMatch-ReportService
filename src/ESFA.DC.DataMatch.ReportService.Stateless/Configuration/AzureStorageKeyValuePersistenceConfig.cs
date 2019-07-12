using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Configuration
{
    public class AzureStorageKeyValuePersistenceConfig : IAzureStorageKeyValuePersistenceServiceConfig
    {
        public AzureStorageKeyValuePersistenceConfig(string connectionString, string containerName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
        }

        public string ConnectionString { get; }

        public string ContainerName { get; }
    }
}
