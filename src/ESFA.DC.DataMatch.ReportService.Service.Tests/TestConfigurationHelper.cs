using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
using ESFA.DC.ServiceFabric.Common.Config;
using ESFA.DC.ServiceFabric.Common.Config.Interface;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests
{
    public sealed class TestConfigurationHelper : IServiceFabricConfigurationService
    {
        private const string ConfigurationPackageObject = @"Config";

        private const string StatelessServiceConfiguration = @"StatelessServiceConfiguration";

        public IDictionary<string, string> GetConfigSectionAsDictionary(string sectionName)
        {
            throw new System.NotImplementedException();
        }

        public T GetConfigSectionAs<T>(string sectionName)
        {
            switch (sectionName)
            {
                case "StatelessServiceConfiguration":
                    return (T)(object)new StatelessServiceConfiguration()
                    {
                        ServiceBusConnectionString = "ServiceBusConnectionString",
                        TopicName = "TopicName",
                        SubscriptionName = "SubscriptionName",
                        TopicMaxConcurrentCalls = "1",
                        TopicMaxCallbackTimeSpanMinutes = "20",
                        JobStatusQueueName = "JobStatusQueueName",
                        JobStatusMaxConcurrentCalls = "1",
                        AuditQueueName = "AuditQueueName",
                        AuditMaxConcurrentCalls = "1",
                        LoggerConnectionString = "LoggerConnectionString"
                    };
                case "ReportServiceConfiguration":
                    return (T)(object)new ReportServiceConfiguration()
                    {
                        DASPaymentsConnectionString = "DASPaymentsConnectionString",
                        ILR1920DataStoreConnectionString = "ILR1920DataStoreConnectionString",
                        ILR2021DataStoreConnectionString = "ILR2021DataStoreConnectionString",
                    };
                case "AzureStorageSection":
                    return (T)(object)new AzureStorageOptions()
                    {
                        AzureBlobConnectionString = "AzureBlobConnectionString",
                        AzureBlobContainerName = "AzureBlobContainerName"
                    };
                case "SoftwareLicenceSection":
                //  <Parameter Name="AsposeLicence" Value="[AsposeLicence]" />
                return (T)(object)new SoftwareLicenceSection()
                {
                    AsposeLicence = "AsposeLicence"
                };
                case "VersionSection":
                    return (T)(object)new VersionInfo
                    {
                        ServiceReleaseVersion = "1.2.3"
                    };
            }

            return default(T);
        }

        public IStatelessServiceConfiguration GetConfigSectionAsStatelessServiceConfiguration()
        {
            return GetConfigSectionAs<StatelessServiceConfiguration>(StatelessServiceConfiguration);
        }
    }
}
