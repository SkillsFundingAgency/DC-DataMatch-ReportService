using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Aspose.Cells;
using Autofac;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Core;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
using ESFA.DC.DataMatch.ReportService.Model.Configuration;
using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Config;
using ESFA.DC.Logging.Config.Interfaces;
using ESFA.DC.Logging.Enums;
using ESFA.DC.Logging.Interfaces;
using Microsoft.Extensions.Configuration;
using ExecutionContext = ESFA.DC.Logging.ExecutionContext;

namespace ESFA.DC.DataMatch.ReportService.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = configurationBuilder.Build();

            // License Aspose.Cells
            string asposeLicense = configuration["AsposeLicence"];
            if (!string.IsNullOrEmpty(asposeLicense))
            {
                using (MemoryStream ms =
                    new MemoryStream(Encoding.UTF8.GetBytes(asposeLicense.Replace("&lt;", "<").Replace("&gt;", ">"))))
                {
                    new License().SetLicense(ms);
                }
            }

            // Setup Autofac
            ContainerBuilder builder = DIComposition.BuildNewContainer();
            ConfigurationRootModel configurationRoot = new ConfigurationRootModel
            {
                azureBlobStorageOptions = new AzureStorageOptions
                {
                    AzureBlobConnectionString = configuration["AzureBlobConnectionString"],
                    AzureBlobContainerName = configuration["AzureBlobContainerName"]
                },
                reportServiceConfiguration = new ReportServiceConfiguration
                {
                    DASPaymentsConnectionString = configuration["DASPaymentsConnectionString"],
                    ILR1920DataStoreConnectionString = configuration["ILR1920DataStoreConnectionString"],
                    ILR2021DataStoreConnectionString = configuration["ILR2021DataStoreConnectionString"],
                },
                versionInfo = new VersionInfo
                {
                    ServiceReleaseVersion = "1.0"
                }
            };

            DIComposition.BuildContainer(builder, configurationRoot);
            DIComposition.BuildStorageFileSystem(builder, configurationRoot.azureBlobStorageOptions,
                new FileSystemService(@"C:\Temp\"));

            IReportServiceContext reportServiceContext = new ConsoleReportServiceContext
            {
                JobId = -1,
                Container = configurationRoot.azureBlobStorageOptions.AzureBlobContainerName,
                CollectionYear = "1920",
                CollectionName = "PE-DAS-AppsInternalDataMatchMonthEndReport1920",
                ReturnPeriod = 3,
                ILRPeriods = GetIlrPeriods(),
                IsIlrSubmission = false,
                SubmissionDateTimeUtc = DateTime.UtcNow,
                Tasks = new List<string>
                {
                    ReportTaskNameConstants.InternalDataMatchReport
                },
                Ukprn = 0
            };

            builder.Register(c => new ApplicationLoggerSettings
            {
                ApplicationLoggerOutputSettingsCollection = new List<IApplicationLoggerOutputSettings>
                {
                    new ConsoleApplicationLoggerOutputSettings
                    {
                        MinimumLogLevel = LogLevel.Verbose
                    }
                } as IList<IApplicationLoggerOutputSettings>
            }).As<IApplicationLoggerSettings>().SingleInstance();
            builder.RegisterType<ExecutionContext>().As<IExecutionContext>().InstancePerLifetimeScope();
            builder.RegisterType<SerilogLoggerFactory>().As<ISerilogLoggerFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SeriLogger>().As<ILogger>().InstancePerLifetimeScope();

            using (var container = builder.Build())
            {
                IHandler handler = container.Resolve<IHandler>();
                bool res = handler.HandleAsync(reportServiceContext, CancellationToken.None).Result;
                System.Console.WriteLine($"Finished - {res}");
            }
        }

        private static IEnumerable<ReturnPeriod> GetIlrPeriods()
        {
            return new List<ReturnPeriod>
            {
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2019-08-22T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2019-09-05T17:05:00"),
                    PeriodNumber = 1,
                    CollectionName = "ILR1920",
                    CalendarMonth = 8,
                    CalendarYear = 2019
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2019-09-17T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2019-10-04T17:05:00"),
                    PeriodNumber = 2,
                    CollectionName = "ILR1920",
                    CalendarMonth = 9,
                    CalendarYear = 2019
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2019-10-16T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2019-11-06T18:05:00"),
                    PeriodNumber = 3,
                    CollectionName = "ILR1920",
                    CalendarMonth = 10,
                    CalendarYear = 2019
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2019-11-18T09:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2019-12-05T18:05:00"),
                    PeriodNumber = 4,
                    CollectionName = "ILR1920",
                    CalendarMonth = 11,
                    CalendarYear = 2019
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2019-12-17T09:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-01-07T18:05:00"),
                    PeriodNumber = 5,
                    CollectionName = "ILR1920",
                    CalendarMonth = 12,
                    CalendarYear = 2019
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-01-17T09:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-02-06T18:05:00"),
                    PeriodNumber = 6,
                    CollectionName = "ILR1920",
                    CalendarMonth = 1,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-02-18T09:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-03-05T18:05:00"),
                    PeriodNumber = 7,
                    CollectionName = "ILR1920",
                    CalendarMonth = 2,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-03-17T09:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-04-06T17:05:00"),
                    PeriodNumber = 8,
                    CollectionName = "ILR1920",
                    CalendarMonth = 3,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-04-20T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-05-06T17:05:00"),
                    PeriodNumber = 9,
                    CollectionName = "ILR1920",
                    CalendarMonth = 4,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-05-19T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-06-04T17:05:00"),
                    PeriodNumber = 10,
                    CollectionName = "ILR1920",
                    CalendarMonth = 5,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-06-16T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-07-06T17:05:00"),
                    PeriodNumber = 11,
                    CollectionName = "ILR1920",
                    CalendarMonth = 6,
                    CalendarYear = 2020
                },
                new ReturnPeriod
                {
                    StartDateTimeUtc = DateTime.Parse("2020-07-16T08:00:00"),
                    EndDateTimeUtc = DateTime.Parse("2020-08-06T17:05:00"),
                    PeriodNumber = 12,
                    CollectionName = "ILR1920",
                    CalendarMonth = 7,
                    CalendarYear = 2020
                }
            };
        }
    }
}