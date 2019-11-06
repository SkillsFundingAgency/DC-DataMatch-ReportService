using Autofac;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Service;
using ESFA.DC.DataMatch.ReportService.Stateless.Handlers;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.ServiceFabric.Common.Config.Interface;
using ESFA.DC.ServiceFabric.Common.Modules;
using VersionInfo = ESFA.DC.DataMatch.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.DataMatch.ReportService.Stateless
{
    public static class DICompositionServiceFabric
    {
        public static ConfigurationRootModel BuildContainer(
            ContainerBuilder containerBuilder,
            IServiceFabricConfigurationService serviceFabricConfigurationService)
        {
            IStatelessServiceConfiguration statelessServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAsStatelessServiceConfiguration();
            containerBuilder.RegisterModule(new StatelessServiceModule(statelessServiceConfiguration));

            ReportServiceConfiguration reportServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAs<ReportServiceConfiguration>("ReportServiceConfiguration");
            AzureStorageOptions azureBlobStorageOptions = serviceFabricConfigurationService.GetConfigSectionAs<AzureStorageOptions>("AzureStorageSection");
            VersionInfo versionInfo = serviceFabricConfigurationService.GetConfigSectionAs<VersionInfo>("VersionSection");

            // register message mapper
            containerBuilder.RegisterType<DefaultJobContextMessageMapper<JobContextMessage>>().As<IMapper<JobContextMessage, JobContextMessage>>();

            // register MessageHandler
            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler<JobContextMessage>>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextManager<JobContextMessage>>().As<IJobContextManager<JobContextMessage>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextMessage>().As<IJobContextMessage>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterModule<SerializationModule>();

            return new ConfigurationRootModel
            {
                reportServiceConfiguration = reportServiceConfiguration,
                azureBlobStorageOptions = azureBlobStorageOptions,
                versionInfo = versionInfo
            };
        }
    }
}
