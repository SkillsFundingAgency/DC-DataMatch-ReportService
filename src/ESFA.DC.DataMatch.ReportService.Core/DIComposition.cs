using System;
using System.Collections.Generic;
using Autofac;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DataMatch.ReportService.Core.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Service;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.DataMatch.ReportService.Service.Reports;
using ESFA.DC.DataMatch.ReportService.Service.Reports.External;
using ESFA.DC.DataMatch.ReportService.Service.Reports.Internal;
using ESFA.DC.DataMatch.ReportService.Service.Service;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
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
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.DataMatch.ReportService.Core
{
    public static class DIComposition
    {
        public static ContainerBuilder BuildNewContainer()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            return containerBuilder;
        }

        public static void BuildStorageFileSystem(
            ContainerBuilder containerBuilder,
            AzureStorageOptions azureBlobStorageOptions,
            IStreamableKeyValuePersistenceService storagePersistenceService)
        {
            containerBuilder.RegisterInstance(azureBlobStorageOptions).As<IAzureStorageOptions>();
            containerBuilder.RegisterType<FileSystemFileService>().As<IFileService>();
            containerBuilder.RegisterInstance(storagePersistenceService)
                .As<IStreamableKeyValuePersistenceService>()
                .SingleInstance();
        }

        public static void BuildStorageContainerAzure(
            ContainerBuilder containerBuilder,
            AzureStorageOptions azureBlobStorageOptions)
        {
            // register azure blob storage service
            containerBuilder.RegisterInstance(azureBlobStorageOptions).As<IAzureStorageOptions>();
            containerBuilder.Register(c =>
                    new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        azureBlobStorageOptions.AzureBlobContainerName))
                .As<IAzureStorageKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<AzureStorageKeyValuePersistenceService>()
                .As<IStreamableKeyValuePersistenceService>()
                .InstancePerLifetimeScope();

            AzureStorageFileServiceConfiguration azureStorageFileServiceConfiguration = new AzureStorageFileServiceConfiguration()
            {
                ConnectionString = azureBlobStorageOptions.AzureBlobConnectionString,
            };

            containerBuilder.RegisterInstance(azureStorageFileServiceConfiguration).As<IAzureStorageFileServiceConfiguration>();
            containerBuilder.RegisterType<AzureStorageFileService>().As<IFileService>();
        }

        public static void BuildContainer(
            ContainerBuilder containerBuilder,
            ConfigurationRootModel configurationRoot)
        {
            containerBuilder.RegisterInstance(configurationRoot.reportServiceConfiguration).As<IReportServiceConfiguration>();
            
            containerBuilder.RegisterType<XmlSerializationService>().As<IXmlSerializationService>();
            containerBuilder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>().As<ISerializationService>();

            containerBuilder.RegisterInstance(configurationRoot.versionInfo).As<IVersionInfo>().SingleInstance();

            containerBuilder.RegisterType<ILR1819_DataStoreEntitiesValid>().As<IIlr1819ValidContext>();
            containerBuilder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>();
                optionsBuilder.UseSqlServer(
                    configurationRoot.reportServiceConfiguration.ILR1819DataStoreConnectionString,
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
                    configurationRoot.reportServiceConfiguration.ILR1920DataStoreConnectionString,
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
                    configurationRoot.reportServiceConfiguration.ILR1819DataStoreConnectionString,
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
                    configurationRoot.reportServiceConfiguration.ILR1920DataStoreConnectionString,
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
                    configurationRoot.reportServiceConfiguration.DASPaymentsConnectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.CommandTimeout(600);
                        sqlServerOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>());
                    });

                return optionsBuilder.Options;
            })
            .As<DbContextOptions<DASPaymentsContext>>()
            .SingleInstance();

            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<ExternalDataMatchModelComparer>();
            containerBuilder.RegisterType<InternalDataMatchModelComparer>();

            containerBuilder.RegisterType<Handler>().As<IHandler>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EntryPoint>().InstancePerLifetimeScope();

            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterReports(containerBuilder);
        }

        public static void RegisterServicesByYear(string year, ContainerBuilder containerBuilder)
        {
            if (year.CaseInsensitiveEquals(Constants.YEAR_1819))
            {
                containerBuilder.RegisterType<ILR1819ProviderService>().As<IILRProviderService>()
                    .InstancePerLifetimeScope();
                containerBuilder.RegisterType<FM361819ProviderService>().As<IFM36ProviderService>()
                    .InstancePerLifetimeScope();
            }
            else if (year.CaseInsensitiveEquals(Constants.YEAR_1920))
            {
                containerBuilder.RegisterType<ILR1920ProviderService>().As<IILRProviderService>()
                    .InstancePerLifetimeScope();
                containerBuilder.RegisterType<FM361920ProviderService>().As<IFM36ProviderService>()
                    .InstancePerLifetimeScope();
            }
        }

        private static void RegisterReports(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ExternalDataMatchReport>().As<IReport>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<InternalDataMatchReport>().As<IReport>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DASPaymentsProviderService>().As<IDASPaymentsProviderService>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ExternalDataMatchMonthEndModelBuilder>().As<IExternalDataMatchModelBuilder>();
            containerBuilder.RegisterType<InternalDataMatchMonthEndModelBuilder>().As<IInternalDataMatchModelBuilder>();

            containerBuilder.RegisterType<DataLockValidationMessageService>().As<IDataLockValidationMessageService>();
        }
    }
}
