using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Service.Builders;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.ILR.ReportService.Model.DASPayments;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests.Builders
{
    public class TestDataMatchModelBuilder
    {
        [Theory]
        [InlineData(11111, 1111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 4, DataLockValidationMessages.DLOCK_01)]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000112, 1, 1, 2, 2, 3, 3, 4, 4, DataLockValidationMessages.DLOCK_02)]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 0, 2, 2, 3, 3, 4, 4, DataLockValidationMessages.DLOCK_03)]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 0, 3, 3, 4, 4, DataLockValidationMessages.DLOCK_04)]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 0, 4, 4, DataLockValidationMessages.DLOCK_05)]
        [InlineData(11111, 11111, "9900000306", 9900000111, 9900000111, 1, 1, 2, 2, 3, 3, 4, 0, DataLockValidationMessages.DLOCK_06)]
        public void VerifyDataMatchModelBuilder(
            int ilrukPrn, int dasUkPrn,
            string learnRefNumber,
            long ilrUln, long dasUln,
            int ilrStdCode, int dasStdCode,
            int ilrFworkCode, int dasFworkCode,
            int ilrProgType, int dasProgType,
            int ilrPwayCode, int dasPwayCode,
            string expectedErrorCode)
        {
            var dataMatchModelBuilder = new DataMatchMonthEndModelBuilder();
            var learnersList = BuildBasicILRModelForTests(ilrukPrn, learnRefNumber, ilrUln, ilrProgType, ilrStdCode, ilrFworkCode, ilrPwayCode);
            var dasApprenticeshipInfoList = BuildBasicDasApprenticeshipInfo(dasUkPrn, learnRefNumber, dasUln, dasProgType, dasStdCode, dasFworkCode, dasPwayCode);
            var dataMatchRulebaseInfo = BuildBasicFmModelForTests(ilrukPrn, learnRefNumber);

            var result = dataMatchModelBuilder.BuildModels(learnersList, dasApprenticeshipInfoList, dataMatchRulebaseInfo);

            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            result.First().RuleName.Should().Be(expectedErrorCode);
        }

        private DataMatchRulebaseInfo BuildBasicFmModelForTests(int ukPrn, string learnRefNumber)
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
                        UkPrn = ukPrn
                    }
                }
            };
        }

        private List<Learner> BuildBasicILRModelForTests(int ukPrn, string learnRefNumber, long uln, int progType, int stdCode, int fworkCode, int pwayCode)
        {
            return new List<Learner>()
            {
                new Learner()
                {
                    UKPRN = ukPrn,
                    LearnRefNumber = learnRefNumber,
                    ULN = uln,
                    LearningDeliveries = new List<LearningDelivery>()
                    {
                        new LearningDelivery()
                        {
                            LearnRefNumber = learnRefNumber,
                            LearnAimRef = "50117889",
                            AimSeqNumber = 1,
                            FundModel = 36,
                            ProgType = progType,
                            StdCode = stdCode,
                            FworkCode = fworkCode,
                            PwayCode = pwayCode,
                            LearningDeliveryFAMs = new List<LearningDeliveryFAM>()
                            {
                                new LearningDeliveryFAM()
                                {
                                    LearnDelFAMType =  "ACT",
                                    LearnDelFAMCode = "1"
                                }
                            }
                        }
                    }
                }
            };
        }

        private List<DasApprenticeshipInfo> BuildBasicDasApprenticeshipInfo(int ukPrn, string learnRefNumber, long uln, int progType, int stdCode, int fworkCode, int pwayCode)
        {
            return new List<DasApprenticeshipInfo>()
            {
                new DasApprenticeshipInfo()
                {
                    UkPrn = ukPrn,
                    LearnerReferenceNumber = learnRefNumber,
                    Uln = uln,
                    ApprenticeshipId = 114656,
                    AgreementId = "YZ2V7Y",
                    AimSequenceNumber = 1,
                    AgreedOnDate = new DateTime(2017, 06, 28),
                    EstimatedStartDate = new DateTime(2017, 06, 30),
                    EstimatedEndDate = new DateTime(2018, 07, 30),
                    StandardCode = stdCode,
                    FrameworkCode = fworkCode,
                    PathwayCode = pwayCode,
                    ProgrammeType = progType,
                    Cost = 1.80M,
                    StopDate = new DateTime(2018, 05, 30),
                    RuleId = 3,
                    PauseDate = new DateTime(2018, 04, 30),
                    LegalEntityName = "LegalEntityName"
                }
            };
        }

    }
}
