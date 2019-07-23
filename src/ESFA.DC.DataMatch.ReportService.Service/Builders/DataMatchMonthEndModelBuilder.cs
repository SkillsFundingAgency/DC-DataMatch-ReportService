using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
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
                ILRValue = GetIlrValue(dasApprenticeshipInfo.RuleId.ToString(), learningDelivery, dasApprenticeshipInfo.Uln),
                ApprenticeshipServiceValue = dasApprenticeshipInfo.AppreticeshipServiceValue
            };

            return model;
        }

        private string GetIlrValue(string ruleId, LearningDelivery learningDelivery, long uln)
        {
            switch (ruleId)
            {
                case DataLockValidationMessages.DLOCK_01:
                    return learningDelivery.UKPRN.ToString();
                case DataLockValidationMessages.DLOCK_02:
                    return uln.ToString();
                case DataLockValidationMessages.DLOCK_03:
                    return learningDelivery.StdCode?.ToString();
                case DataLockValidationMessages.DLOCK_04:
                    return learningDelivery.FworkCode?.ToString();
                case DataLockValidationMessages.DLOCK_05:
                    return learningDelivery.ProgType?.ToString();
                case DataLockValidationMessages.DLOCK_06:
                    return learningDelivery.PwayCode?.ToString();
                case DataLockValidationMessages.DLOCK_09:
                    return learningDelivery.LearnStartDate.ToString("dd-MM-yyyy");
                case DataLockValidationMessages.DLOCK_07:
                    return CalculateIlrValueForDataLock07(learningDelivery);
                default:
                    return string.Empty;
            }
        }

        private string CalculateIlrValueForDataLock07(LearningDelivery learningDelivery)
        {
            var negotiatedCostOfTraining = (learningDelivery.AppFinRecords?
                .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                            x.AFinCode == 1).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount) + 
                   (learningDelivery.AppFinRecords?
                       .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                   x.AFinCode == 2).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount);

            return negotiatedCostOfTraining.ToString();
        }
    }
}
