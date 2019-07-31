using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Service.Builders;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests.Builders
{
    /// <summary>
    /// DataMatchModelBuilder tests
    /// </summary>
    public class TestDataMatchModelBuilder
    {
        [Theory]
        [InlineData(11111, 1111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 4, 1, "11111", "1111")]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000112, 1, 1, 2, 2, 3, 3, 4, 4, 2, "9900000111", "")]
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
            var dataMatchModelBuilder = new DataMatchMonthEndModelBuilder();
            var dataMatchILRInfo = BuildILRModelForDataMatchReportBuilderTests(ilrukPrn, learnRefNumber, ilrUln, "50117889", 1, ilrFworkCode, ilrProgType, ilrPwayCode, ilrStdCode, "ACT", "1", new DateTime(2019, 10, 10));
            var dataMatchRulebaseInfo = BuildILRRulebaseModelForDataMatchReportBuilderTests(ilrukPrn, learnRefNumber, 1);
            var dataLockValidationErrorInfo =
                BuildDataLockValidationErrorInfoForDataMatchReportBuildTests(dasUkPrn, learnRefNumber, 1, dasUln, dasFworkCode, dasProgType, dasPwayCode, dasStdCode, ruleId, 12345);
            var dataMatchDasApprenticeshiPriceInfo =
                BuildDasApprenticeshipInfoForDataMatchReportBuilderTests(ilrukPrn, 9900000111, null, null, 100, "TestLegalEntityName");

            var result = dataMatchModelBuilder.BuildModels(dataMatchILRInfo, dataMatchRulebaseInfo, dataLockValidationErrorInfo, dataMatchDasApprenticeshiPriceInfo);

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
            decimal cost,
            string legalEntityName)
        {
            return new DataMatchDasApprenticeshipInfo()
            {
                UkPrn = ukPrn,
                DasApprenticeshipPriceInfos = new List<DasApprenticeshipPriceInfo>()
                {
                    new DasApprenticeshipPriceInfo()
                    {
                        LearnerUln = uln,
                        PausedOnDate = pausedOnDate,
                        WithdrawnOnDate = withdrawnOnDate,
                        LegalEntityName = legalEntityName,
                        Cost = cost,
                    },
                },
            };
        }

        private DataMatchDataLockValidationErrorInfo BuildDataLockValidationErrorInfoForDataMatchReportBuildTests(
            int ukPrn,
            string learnerReferenceNumber,
            int aimSeqNumber,
            long uln,
            int frameworkCode,
            int programmeType,
            int pathwayCode,
            int standardCode,
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
                        FrameworkCode = frameworkCode,
                        ProgrammeType = programmeType,
                        PathwayCode = pathwayCode,
                        StandardCode = standardCode,
                        RuleId = ruleId,
                        PriceEpisodeMatchAppId = priceEpisodeMatchAppId,
                    },
                },
            };
        }

        private DataMatchILRInfo BuildILRModelForDataMatchReportBuilderTests(
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
            return new DataMatchILRInfo()
            {
                UkPrn = ukPrn,
                DataMatchLearners = new List<DataMatchLearner>()
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
                                LearnRefNumber = learnerReferenceNumber,
                                LearnAimRef = learnAimRef,
                                AimSeqNumber = aimSeqNumber,
                                ProgType = programmeType,
                                StdCode = standardCode,
                                FworkCode = frameworkCode,
                                PwayCode = pathwayCode,
                                DataMatchLearningDeliveryFams = GetLearningDeliveryFAMs(ukPrn, learnDelFAMType, learnDelFAMCode),
                                UkPrn = ukPrn,
                                LearnStartDate = learnStartDate,
                                AppFinRecords = GetAppFinRecords(learnerReferenceNumber, aimSeqNumber, 100, 1, "TNP", new DateTime(2017, 07, 30)),
                            },
                        },
                    },
                },
            };
        }

        private List<DataMatchLearningDeliveryFAM> GetLearningDeliveryFAMs(int ukPrn, string learnDelFAMType, string learnDelFAMCode)
        {
            return new List<DataMatchLearningDeliveryFAM>()
            {
                new DataMatchLearningDeliveryFAM()
                {
                    LearnDelFAMType = learnDelFAMType,
                    LearnDelFAMCode = learnDelFAMCode,
                    UKPRN = ukPrn,
                },
            };
        }

        private List<AppFinRecordInfo> GetAppFinRecords(string learnerReferenceNumber, int aimSeqNumber, int aFinAmount, int aFinCode, string aFinType, DateTime aFinDate)
        {
            return new List<AppFinRecordInfo>
            {
                new AppFinRecordInfo()
                {
                    LearnRefNumber = learnerReferenceNumber,
                    AimSeqNumber = aimSeqNumber,
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