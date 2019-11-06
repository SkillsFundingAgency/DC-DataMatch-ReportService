using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Builders;
using ESFA.DC.DataMatch.ReportService.Service.Comparer;
using ESFA.DC.DataMatch.ReportService.Service.Mapper;
using ESFA.DC.DataMatch.ReportService.Service.Reports;
using ESFA.DC.DataMatch.ReportService.Service.Tests.Helpers;
using ESFA.DC.DataMatch.ReportService.Tests.Models;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests.Reports
{
    public sealed class TestDataMatchReport
    {
        [Fact]
        public async Task TestDataMatchReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Apprenticeship Data Match Report {dateTime:yyyyMMdd-HHmmss}";
            int ukPrn = 10033670;

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.IsIlrSubmission).Returns(true);

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IFM36ProviderService> fm36ProviderServiceMock = new Mock<IFM36ProviderService>();
            Mock<IILRProviderService> iIlrProviderService = new Mock<IILRProviderService>();
            IDataMatchModelBuilder dataMatchModelBuilder = new DataMatchMonthEndModelBuilder(logger.Object);

            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            var ilrModelForDataMatchReport = BuildILRModelForDataMatchReport(ukPrn);
            var dataMatchRulebaseInfo = BuildFm36Model(ukPrn);

            var dasApprenticeshipInfoForDataMatchReport = GetDasApprenticeshipInfoForDataMatchReport(ukPrn);
            var dataLockValidationErrorInfoForDataMatchReport = GetDataLockValidationErrorInfoForDataMatchReport(ukPrn);

            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);

            fm36ProviderServiceMock
                .Setup(x => x.GetFM36DataForDataMatchReport(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataMatchRulebaseInfo);
            iIlrProviderService
                .Setup(x => x.GetILRInfoForDataMatchReport(It.IsAny<int>(), It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ilrModelForDataMatchReport);
            dasPaymentProviderMock.Setup(x =>
                    x.GetDasApprenticeshipInfoForDataMatchReport(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dasApprenticeshipInfoForDataMatchReport);

            dasPaymentProviderMock.Setup(x => x.GetDataLockValidationErrorInfoForDataMatchReport(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataLockValidationErrorInfoForDataMatchReport);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var report = new ExternalDataMatchReport(
                dasPaymentProviderMock.Object,
                fm36ProviderServiceMock.Object,
                iIlrProviderService.Object,
                storage.Object,
                dataMatchModelBuilder,
                dateTimeProviderMock.Object,
                new ExternalDataMatchModelComparer(),
                logger.Object);

            await report.GenerateReport(reportServiceContextMock.Object, null, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            IEnumerable<DataMatchModel> result;
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new ExternalDataMatchMapper(), 1));
            using (var reader = new StreamReader($"{filename}.csv"))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.RegisterClassMap<ExternalDataMatchMapper>();
                    result = csvReader.GetRecords<DataMatchModel>().ToList();
                }
            }

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
        }

        private DataMatchDataLockValidationErrorInfo GetDataLockValidationErrorInfoForDataMatchReport(int ukPrn)
        {
            return new DataMatchDataLockValidationErrorInfo()
            {
                DataLockValidationErrors = new List<DataLockValidationError>()
                {
                    new DataLockValidationError()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = "9900000306",
                        AimSeqNumber = 1,
                        LearnerUln = 9900000111,
                        RuleId = 1,
                        PriceEpisodeMatchAppId = 12345,
                    },
                },
            };
        }

        private DataMatchDasApprenticeshipInfo GetDasApprenticeshipInfoForDataMatchReport(int ukPrn)
        {
            return new DataMatchDasApprenticeshipInfo()
            {
                UkPrn = ukPrn,
                DasApprenticeshipInfos = new List<DasApprenticeshipInfo>()
                {
                    new DasApprenticeshipInfo()
                    {
                        LearnerUln = 9900000111,
                        PausedOnDate = null,
                        WithdrawnOnDate = null,
                        LegalEntityName = "LegalEntityName",
                        Cost = 100,
                        FrameworkCode = 1,
                        ProgrammeType = 2,
                        PathwayCode = 3,
                        StandardCode = 4,
                    },
                },
            };
        }

        private DataMatchRulebaseInfo BuildFm36Model(int ukPrn)
        {
            return new DataMatchRulebaseInfo()
            {
                UkPrn = ukPrn,
                LearnRefNumber = "9900000306",
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
                {
                    new AECApprenticeshipPriceEpisodeInfo()
                    {
                        LearnRefNumber = "9900000306",
                        PriceEpisodeAgreeId = "YZ2V7Y",
                        EpisodeStartDate = new DateTime(2019, 06, 28),
                        PriceEpisodeActualEndDate = new DateTime(2020, 06, 28),
                        UkPrn = ukPrn,
                        AimSequenceNumber = 1,
                        EffectiveTnpStartDate = new DateTime(2017, 07, 30),
                    },
                },
            };
        }

        private DataMatchILRInfo BuildILRModelForDataMatchReport(int ukPrn)
        {
            return new DataMatchILRInfo()
            {
                UkPrn = ukPrn,
                DataMatchLearners = new List<DataMatchLearner>()
                {
                    new DataMatchLearner()
                    {
                        UkPrn = ukPrn,
                        LearnRefNumber = "9900000306",
                        Uln = 9900000111,
                        DataMatchLearningDeliveries = new List<DataMatchLearningDelivery>()
                        {
                            new DataMatchLearningDelivery()
                            {
                                LearnRefNumber = "9900000306",
                                LearnAimRef = "50117889",
                                AimSeqNumber = 1,
                                ProgType = 3,
                                StdCode = 0,
                                FworkCode = 421,
                                PwayCode = 2,
                                DataMatchLearningDeliveryFams = new List<DataMatchLearningDeliveryFAM>()
                                {
                                    new DataMatchLearningDeliveryFAM()
                                    {
                                        LearnDelFAMType = "ACT",
                                        LearnDelFAMCode = "1",
                                        UKPRN = ukPrn,
                                    },
                                },
                                UkPrn = ukPrn,
                                LearnStartDate = new DateTime(2017, 06, 30),
                                AppFinRecords = new List<AppFinRecordInfo>
                                {
                                    new AppFinRecordInfo()
                                    {
                                        LearnRefNumber = "9900000306",
                                        AimSeqNumber = 1,
                                        AFinAmount = 100,
                                        AFinCode = 1,
                                        AFinType = "TNP",
                                        AFinDate = new DateTime(2017, 07, 30),
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }
    }
}