using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Service.Reports.External;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests.Builders
{
    /// <summary>
    /// DataMatchModelBuilder tests
    /// </summary>
    public class TestDataMatchModelBuilder
    {
        [Theory]
        [InlineData(11111, 1111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 4, 1, "11111", "11111")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 4, 2, "9900000111", "")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 0, 2, 2, 3, 3, 4, 4, 3, "1", "0")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 0, 3, 3, 4, 4, 4, "2", "0")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 0, 4, 4, 5, "3", "0")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 0, 6, "4", "0")]
        public void VerifyDataMatchModelBuilder(
            int ilrukPrn,
            int dasUkPrn,
            string learnRefNumber,
            long ilrUln,
            long dasUln,
            int ilrStdCode,
            int dasStdCode,
            int ilrFworkCode,
            int dasFworkCode,
            int ilrProgType,
            int dasProgType,
            int ilrPwayCode,
            int dasPwayCode,
            int ruleId,
            string expectedIlrValue = "",
            string expectedApprenticeshipValue = "")
        {
            var dataMatchModelBuilder = new ExternalDataMatchMonthEndModelBuilder(Mock.Of<IDataLockValidationMessageService>(), Mock.Of<ILogger>());
            var dataMatchILRInfo = BuildILRModelForDataMatchReportBuilderTests(ilrukPrn, learnRefNumber, ilrUln, "50117889", 1, ilrFworkCode, ilrProgType, ilrPwayCode, ilrStdCode, "ACT", "1", new DateTime(2019, 10, 10));
            var dataMatchRulebaseInfo = BuildILRRulebaseModelForDataMatchReportBuilderTests(ilrukPrn, learnRefNumber, 1);
            var dataLockValidationErrorInfo =
                BuildDataLockValidationErrorInfoForDataMatchReportBuildTests(dasUkPrn, learnRefNumber, 1, dasUln, ruleId, 12345);
            var dataMatchDasApprenticeshiPriceInfo =
                BuildDasApprenticeshipInfoForDataMatchReportBuilderTests(ilrukPrn, 9900000111, null, null, dasFworkCode, dasProgType, dasPwayCode, dasStdCode, 100, "TestLegalEntityName");

            var result = dataMatchModelBuilder.BuildExternalModels(dataMatchILRInfo, dataMatchRulebaseInfo, dataLockValidationErrorInfo, dataMatchDasApprenticeshiPriceInfo, -1);

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            result.First().RuleName.Should().Be("DLOCK_" + ruleId.ToString("00"));
            result.First().ILRValue.Should().Be(expectedIlrValue);
            result.First().ApprenticeshipServiceValue.Should().Be(expectedApprenticeshipValue);
        }

        private DataMatchDasApprenticeshipInfo BuildDasApprenticeshipInfoForDataMatchReportBuilderTests(
            int ukPrn,
            long uln,
            DateTime? pausedOnDate,
            DateTime? withdrawnOnDate,
            int frameworkCode,
            int programmeType,
            int pathwayCode,
            int standardCode,
            decimal cost,
            string legalEntityName)
        {
            return new DataMatchDasApprenticeshipInfo()
            {
                UkPrn = ukPrn,
                DasApprenticeshipInfos = new List<DasApprenticeshipInfo>()
                {
                    new DasApprenticeshipInfo()
                    {
                        LearnerUln = uln,
                        PausedOnDate = pausedOnDate,
                        WithdrawnOnDate = withdrawnOnDate,
                        LegalEntityName = legalEntityName,
                        Cost = cost,
                        FrameworkCode = frameworkCode,
                        ProgrammeType = programmeType,
                        PathwayCode = pathwayCode,
                        StandardCode = standardCode,
                        UkPrn = ukPrn,
                    },
                },
            };
        }

        private DataMatchDataLockValidationErrorInfo BuildDataLockValidationErrorInfoForDataMatchReportBuildTests(
            int ukPrn,
            string learnerReferenceNumber,
            int aimSeqNumber,
            long uln,
            int ruleId,
            long priceEpisodeMatchAppId)
        {
            return new DataMatchDataLockValidationErrorInfo()
            {
                DataLockValidationErrors = new List<DataLockValidationError>()
                {
                    new DataLockValidationError()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = learnerReferenceNumber,
                        AimSeqNumber = aimSeqNumber,
                        LearnerUln = uln,
                        RuleId = ruleId,
                        PriceEpisodeMatchAppId = priceEpisodeMatchAppId,
                    },
                },
            };
        }

        private ICollection<DataMatchLearner> BuildILRModelForDataMatchReportBuilderTests(
            int ukPrn,
            string learnerReferenceNumber,
            long uln,
            string learnAimRef,
            int aimSeqNumber,
            int frameworkCode,
            int programmeType,
            int pathwayCode,
            int standardCode,
            string learnDelFAMType,
            string learnDelFAMCode,
            DateTime learnStartDate)
        {
            return new List<DataMatchLearner>()
            {
                new DataMatchLearner()
                {
                    UkPrn = ukPrn,
                    LearnRefNumber = learnerReferenceNumber,
                    Uln = uln,
                    DataMatchLearningDeliveries = new List<DataMatchLearningDelivery>()
                    {
                        new DataMatchLearningDelivery()
                        {
                            LearnAimRef = learnAimRef,
                            AimSeqNumber = aimSeqNumber,
                            ProgType = programmeType,
                            StdCode = standardCode,
                            FworkCode = frameworkCode,
                            PwayCode = pathwayCode,
                            DataMatchLearningDeliveryFams = GetLearningDeliveryFAMs(learnDelFAMType, learnDelFAMCode),
                            LearnStartDate = learnStartDate,
                            AppFinRecords = GetAppFinRecords(learnerReferenceNumber, aimSeqNumber, 100, 1, "TNP", new DateTime(2017, 07, 30)),
                        },
                    },
                },
            };
        }

        private List<DataMatchLearningDeliveryFAM> GetLearningDeliveryFAMs(string learnDelFAMType, string learnDelFAMCode)
        {
            return new List<DataMatchLearningDeliveryFAM>()
            {
                new DataMatchLearningDeliveryFAM()
                {
                    LearnDelFAMType = learnDelFAMType,
                    LearnDelFAMCode = learnDelFAMCode,
                },
            };
        }

        private List<AppFinRecordInfo> GetAppFinRecords(string learnerReferenceNumber, int aimSeqNumber, int aFinAmount, int aFinCode, string aFinType, DateTime aFinDate)
        {
            return new List<AppFinRecordInfo>
            {
                new AppFinRecordInfo()
                {
                    AFinAmount = aFinAmount,
                    AFinCode = aFinCode,
                    AFinType = aFinType,
                    AFinDate = aFinDate,
                },
            };
        }

        private DataMatchRulebaseInfo BuildILRRulebaseModelForDataMatchReportBuilderTests(int ukPrn, string learnRefNumber, int aimSeqNumber)
        {
            return new DataMatchRulebaseInfo()
            {
                UkPrn = ukPrn,
                LearnRefNumber = learnRefNumber,
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
                {
                    new AECApprenticeshipPriceEpisodeInfo()
                    {
                        LearnRefNumber = learnRefNumber,
                        PriceEpisodeAgreeId = "YZ2V7Y",
                        EpisodeStartDate = new DateTime(2019, 06, 28),
                        PriceEpisodeActualEndDate = new DateTime(2020, 06, 28),
                        UkPrn = ukPrn,
                        AimSequenceNumber = aimSeqNumber,
                    },
                },
            };
        }
    }
}