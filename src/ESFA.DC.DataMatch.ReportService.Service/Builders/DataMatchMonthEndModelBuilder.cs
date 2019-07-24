using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Model.DasPaymenets;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.ILR.ReportService.Model.DASPayments;
using ESFA.DC.ILR1819.DataStore.EF.Valid;

namespace ESFA.DC.DataMatch.ReportService.Service.Builders
{
    public class DataMatchMonthEndModelBuilder : IDataMatchModelBuilder
    {
        public DataMatchModel BuildModel(
            DasApprenticeshipInfo dasApprenticeshipInfo,
            AECApprenticeshipPriceEpisodeInfo aecApprenticeshipPriceEpisodeInfo,
            LearningDelivery learningDelivery)
        {
            var model = new DataMatchModel()
            {
                LearnRefNumber = dasApprenticeshipInfo.LearnerReferenceNumber,
                Uln = dasApprenticeshipInfo.Uln,
                AimSeqNumber = dasApprenticeshipInfo.AimSequenceNumber,
                RuleName = dasApprenticeshipInfo.RuleId.ToString(), //todo: this should be DATALOCK_01 etc, whereas in daspayments, it's defined as a tinyint
                Description = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId == dasApprenticeshipInfo.RuleId.ToString())?.ErrorMessage,
                PriceEpisodeStartDate = aecApprenticeshipPriceEpisodeInfo.EpisodeStartDate,
                PriceEpisodeActualEndDate = aecApprenticeshipPriceEpisodeInfo.PriceEpisodeActualEndDate,
                PriceEpisodeIdentifier = aecApprenticeshipPriceEpisodeInfo.PriceEpisodeAgreeId,
                LegalEntityName = dasApprenticeshipInfo.LegalEntityName,
                OfficialSensitive = "N/A",
                //ILRValue = GetIlrValue(dasApprenticeshipInfo.RuleId.ToString(), learningDelivery, dasApprenticeshipInfo.Uln),
                ApprenticeshipServiceValue = dasApprenticeshipInfo.AppreticeshipServiceValue
            };

            return model;
        }

        public List<DataMatchModel> BuildModels(List<Learner> validIlrLearners,
            List<DasApprenticeshipInfo> dasApprenticeshipInfos, DataMatchRulebaseInfo dataMatchRulebaseInfo)
        {
            var populatedDataMatchModels = new List<DataMatchModel>();
            foreach (var learner in validIlrLearners)
            {
                foreach (var learningDelivery in learner.LearningDeliveries)
                {
                    if (!ValidLearningDelivery(learningDelivery))
                    {
                        continue;
                    }
                    
                    var matchedAecApprenticeshipPriceEpisodeInfo =
                        dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.Where(x => x.LearnRefNumber == learningDelivery.LearnRefNumber);

                    foreach (var aecApprenticeshipPriceEpisode in matchedAecApprenticeshipPriceEpisodeInfo)
                    {
                        var matchedDasApprenticeshipInfoAgainstAgreementId =
                            dasApprenticeshipInfos.FirstOrDefault(x =>
                                x.AgreementId == aecApprenticeshipPriceEpisode.PriceEpisodeAgreeId);

                        var dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.UkPrn == aecApprenticeshipPriceEpisode.UkPrn);

                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_01, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learner.UKPRN.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId.UkPrn.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.Uln == learner.ULN);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_02, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learner.ULN.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.StandardCode == learningDelivery.StdCode);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_03, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.StdCode.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId?.StandardCode.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.FrameworkCode == learningDelivery.FworkCode);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_04, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.FworkCode.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId?.FrameworkCode.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.ProgrammeType == learningDelivery.ProgType);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_05, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.ProgType.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId?.ProgrammeType.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.PathwayCode == learningDelivery.PwayCode);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_06, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.PwayCode.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId?.PathwayCode.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        var calculatedIlrCost = CalculateIlrValueForDataLock07(learningDelivery);
                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.Cost == calculatedIlrCost);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_07, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                calculatedIlrCost.ToString(),
                                matchedDasApprenticeshipInfoAgainstAgreementId?.Cost.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        //todo: DLOCK_08

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.EffectiveFromDate <= aecApprenticeshipPriceEpisode.EffectiveTnpStartDate);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_09, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.LearnStartDate.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        dasApprenticeshipInfoToMatch =
                            dasApprenticeshipInfos.Where(x => x.EffectiveFromDate <= aecApprenticeshipPriceEpisode.EffectiveTnpStartDate);
                        if (!dasApprenticeshipInfoToMatch.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_09, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                learningDelivery.LearnStartDate.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        var withdrawnApprenticeships = dasApprenticeshipInfos
                            .Where(x => x.PaymentStatus == (int)DasPaymentStatus.Withdrawn || x.StopDate.HasValue)
                            .ToList();
                        //var activeWithdrawnCommitments = withdrawnCommitments
                        //    .Where(x => x.StopDate >= censusDate)
                        //    .ToList();
                        if (withdrawnApprenticeships.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_10, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                "",
                                withdrawnApprenticeships.FirstOrDefault(x => x.LearnerReferenceNumber == learningDelivery.LearnRefNumber)?.StopDate.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }

                        var pausedApprenticeships = dasApprenticeshipInfos.Where(x => x.PaymentStatus == (int)DasPaymentStatus.Paused).ToList();
                        if (pausedApprenticeships.Any())
                        {
                            var model = PopuDataMatchModel(DataLockValidationMessages.DLOCK_12, aecApprenticeshipPriceEpisode,
                                matchedDasApprenticeshipInfoAgainstAgreementId,
                                "",
                                pausedApprenticeships.FirstOrDefault(x => x.LearnerReferenceNumber == learningDelivery.LearnRefNumber)?.StopDate.ToString());
                            populatedDataMatchModels.Add(model);
                            continue;
                        }
                    }
                }
            }

            return populatedDataMatchModels;
        }

        private DataMatchModel PopuDataMatchModel(
            string ruleName,
            AECApprenticeshipPriceEpisodeInfo aecApprenticeshipPriceEpisodeInfo,
            DasApprenticeshipInfo dasApprenticeshipInfo,
            string ilrValue = "",
            string apprenticeShipValue = "")
        {
            return new DataMatchModel()
            {
                LearnRefNumber = dasApprenticeshipInfo.LearnerReferenceNumber,
                Uln = dasApprenticeshipInfo.Uln,
                AimSeqNumber = dasApprenticeshipInfo.AimSequenceNumber,
                RuleName = ruleName, //todo: this should be DATALOCK_01 etc, whereas in daspayments, it's defined as a tinyint
                Description = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId == ruleName)?.ErrorMessage,
                PriceEpisodeStartDate = aecApprenticeshipPriceEpisodeInfo.EpisodeStartDate,
                PriceEpisodeActualEndDate = aecApprenticeshipPriceEpisodeInfo.PriceEpisodeActualEndDate,
                PriceEpisodeIdentifier = aecApprenticeshipPriceEpisodeInfo.PriceEpisodeAgreeId,
                LegalEntityName = dasApprenticeshipInfo.LegalEntityName,
                OfficialSensitive = "N/A",
                ILRValue = ilrValue,
                ApprenticeshipServiceValue = apprenticeShipValue
            };
        }


        

        private bool ValidLearningDelivery(LearningDelivery learningDelivery)
        {
            return learningDelivery.FundModel == 36 &&
                   learningDelivery.LearningDeliveryFAMs.Any(
                       x => x.LearnDelFAMType == "ACT" && x.LearnDelFAMCode == "1");
        }
        
        private long? CalculateIlrValueForDataLock07(LearningDelivery learningDelivery)
        {
            var negotiatedCostOfTraining = (learningDelivery.AppFinRecords?
                .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                            x.AFinCode == 1).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount) + 
                   (learningDelivery.AppFinRecords?
                       .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                   x.AFinCode == 2).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount);

            return negotiatedCostOfTraining;
        }
    }
}
