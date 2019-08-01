using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface.Provider;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Service;
using ESFA.DC.DataMatch.ReportService.Service.Builders;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using ESFA.DC.DataMatch.ReportService.Service.Service;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
using ESFA.DC.DataMatch.ReportService.Stateless.Handlers;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.FileService;
using ESFA.DC.FileService.Config;
using ESFA.DC.FileService.Config.Interface;
using ESFA.DC.FileService.Interface;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.ILR1920.DataStore.EF.Interface;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.ILR1920.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.ServiceFabric.Common.Modules;
using Microsoft.EntityFrameworkCore;
using VersionInfo = ESFA.DC.DataMatch.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.DataMatch.ReportService.Stateless
{
    public static class DIComposition
    {
        private static readonly string YEAR_1819 = "1819";
        private static readonly string YEAR_1920 = "1920";

        public static ContainerBuilder BuildContainer(ServiceFabric.Common.Config.Interface.IServiceFabricConfigurationService serviceFabricConfigurationService)
        {
            var containerBuilder = new ContainerBuilder();

            var statelessServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAsStatelessServiceConfiguration();

            var reportServiceConfiguration = serviceFabricConfigurationService.GetConfigSectionAs<ReportServiceConfiguration>("ReportServiceConfiguration");
            containerBuilder.RegisterInstance(reportServiceConfiguration).As<IReportServiceConfiguration>();

            // register azure blob storage service
            var azureBlobStorageOptions = serviceFabricConfigurationService.GetConfigSectionAs<AzureStorageOptions>("AzureStorageSection");
            containerBuilder.RegisterInstance(azureBlobStorageOptions).As<IAzureStorageOptions>();
            containerBuilder.Register(c =>
                    new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        azureBlobStorageOptions.AzureBlobContainerName))
                .As<IAzureStorageKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<AzureStorageKeyValuePersistenceService>()
                .As<IStreamableKeyValuePersistenceService>()
                .InstancePerLifetimeScope();

            var azureStorageFileServiceConfiguration = new AzureStorageFileServiceConfiguration()
            {
                ConnectionString = azureBlobStorageOptions.AzureBlobConnectionString,
            };

            containerBuilder.RegisterInstance(azureStorageFileServiceConfiguration).As<IAzureStorageFileServiceConfiguration>();
            containerBuilder.RegisterType<AzureStorageFileService>().As<IFileService>();

            containerBuilder.RegisterModule(new StatelessServiceModule(statelessServiceConfiguration));
            containerBuilder.RegisterModule<SerializationModule>();

            var versionInfo = serviceFabricConfigurationService.GetConfigSectionAs<VersionInfo>("VersionSection");
            containerBuilder.RegisterInstance(versionInfo).As<IVersionInfo>().SingleInstance();

            // register message mapper
            containerBuilder.RegisterType<DefaultJobContextMessageMapper<JobContextMessage>>().As<IMapper<JobContextMessage, JobContextMessage>>();

            // register MessageHandler
            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler<JobContextMessage>>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextManager<JobContextMessage>>().As<IJobContextManager<JobContextMessage>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextMessage>().As<IJobContextMessage>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<EntryPoint>().WithAttributeFiltering().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ZipService>().As<IZipService>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ReportsProvider>().As<IReportsProvider>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<ILR1819_DataStoreEntitiesValid>().As<IIlr1819ValidContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILR1819DataStoreValidConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR1819_DataStoreEntitiesValid>>()
                .SingleInstance();

            containerBuilder.RegisterType<ILR1920_DataStoreEntitiesValid>().As<IIlr1920ValidContext>();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR1920_DataStoreEntitiesValid>();
                    optionsBuilder.UseSqlServer(
                        reportServiceConfiguration.ILR1920DataStoreValidConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<ILR1920_DataStoreEntitiesValid>>()
                .SingleInstance();

            containerBuilder.RegisterType<ILR1819_DataStoreEntities>().As<IIlr1819RulebaseContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntities>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.ILR1819DataStoreConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<ILR1819_DataStoreEntities>>()
                .SingleInstance();

            containerBuilder.RegisterType<ILR1920_DataStoreEntities>().As<IIlr1920RulebaseContext>();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR1920_DataStoreEntities>();
                    optionsBuilder.UseSqlServer(
                        reportServiceConfiguration.ILR1920DataStoreConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<ILR1920_DataStoreEntities>>()
                .SingleInstance();

            containerBuilder.RegisterType<DASPaymentsContext>().As<IDASPaymentsContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DASPaymentsContext>();
                optionsBuilder.UseSqlServer(
                    reportServiceConfiguration.DASPaymentsConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions<DASPaymentsContext>>()
                .SingleInstance();

            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();

            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterReports(containerBuilder);

            return containerBuilder;
        }

        public static void RegisterServicesByYear(string year, ContainerBuilder containerBuilder)
        {
            if (year.CaseInsensitiveEquals(YEAR_1819))
            {
                containerBuilder.RegisterType<ILR1819ProviderService>().As<IILRProviderService>()
                    .WithAttributeFiltering()
                    .InstancePerLifetimeScope();
                containerBuilder.RegisterType<FM361819ProviderService>().As<IFM36ProviderService>()
                    .WithAttributeFiltering()
                    .InstancePerLifetimeScope();
            }
            else if (year.CaseInsensitiveEquals(YEAR_1920))
            {
                containerBuilder.RegisterType<ILR1920ProviderService>().As<IILRProviderService>()
                    .WithAttributeFiltering()
                    .InstancePerLifetimeScope();
                containerBuilder.RegisterType<FM361920ProviderService>().As<IFM36ProviderService>()
                    .WithAttributeFiltering()
                    .InstancePerLifetimeScope();
            }
        }

        private static void RegisterReports(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DataMatchReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ValidLearnerService>().As<IValidLearnersService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<DASPaymentsProviderService>().As<IDASPaymentsProviderService>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DataMatchMonthEndModelBuilder>().As<IDataMatchModelBuilder>()
                .InstancePerLifetimeScope();
        }
    }
}
