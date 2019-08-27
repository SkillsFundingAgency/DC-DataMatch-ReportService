using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;

namespace ESFA.DC.DataMatch.ReportService.Service.Builders
{
    public class DataMatchMonthEndModelBuilder : IDataMatchModelBuilder
    {
        readonly string[] _rulesWithBlankILRValues =
        {
            DataLockValidationMessages.DLOCK_08,
            DataLockValidationMessages.DLOCK_10,
            DataLockValidationMessages.DLOCK_11,
            DataLockValidationMessages.DLOCK_12,
        };

        readonly string[] _rulesWithBlankApprenticeshipValues =
        {
            DataLockValidationMessages.DLOCK_02,
            DataLockValidationMessages.DLOCK_08,
            DataLockValidationMessages.DLOCK_09,
            DataLockValidationMessages.DLOCK_11,
        };

        readonly string _dLockErrorRuleNamePrefix = "DLOCK_";

        public IEnumerable<DataMatchModel> BuildModels(
            DataMatchILRInfo dataMatchILRInfo,
            DataMatchRulebaseInfo dataMatchRulebaseInfo,
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo,
            DataMatchDasApprenticeshipInfo dasApprenticeshipInfo)
        {
            var dataMatchModels = new List<DataMatchModel>();
            foreach (var dataLockValidationError in dataLockValidationErrorInfo.DataLockValidationErrors)
            {
                var learner = dataMatchILRInfo.DataMatchLearners.SingleOrDefault(
                    x => x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber.ToString()) &&
                         x.DataMatchLearningDeliveries.Any(ld => ld.AimSeqNumber == dataLockValidationError.AimSeqNumber));

                if (learner == null)
                {
                    continue;
                }

                var matchedRulebaseInfo = dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.LastOrDefault(x =>
                    x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber));

                var matchedDasPriceInfo = dasApprenticeshipInfo.DasApprenticeshipInfos.FirstOrDefault(x => x.LearnerUln == dataLockValidationError.LearnerUln);

                var ruleName = PopulateRuleName(dataLockValidationError.RuleId);

                var dataMatchModel = new DataMatchModel()
                {
                    LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                    Uln = learner.Uln,
                    AimSeqNumber = dataLockValidationError.AimSeqNumber,
                    RuleName = ruleName,
                    Description = PopulateRuleDescription(ruleName),
                    ILRValue = GetILRValue(ruleName, learner, dataLockValidationError.AimSeqNumber),
                    ApprenticeshipServiceValue = GetApprenticeshipServiceValue(ruleName, matchedDasPriceInfo),
                    PriceEpisodeStartDate = matchedRulebaseInfo?.EpisodeStartDate?.ToString("dd/MM/yyyy"),
                    PriceEpisodeActualEndDate = matchedRulebaseInfo?.PriceEpisodeActualEndDate?.ToString("dd/MM/yyyy"),
                    PriceEpisodeIdentifier = matchedRulebaseInfo?.PriceEpisodeAgreeId,
                    LegalEntityName = GetLegalEntityName(ruleName, matchedDasPriceInfo),
                };

                dataMatchModels.Add(dataMatchModel);
            }

            return dataMatchModels;
        }

        private string GetILRValue(string ruleName, DataMatchLearner learner, long? dasAimSeqNumber)
        {
            if (_rulesWithBlankILRValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_01))
            {
                return learner.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_02))
            {
                return learner.Uln.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_03))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.StdCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.FworkCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.ProgType.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.PwayCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                var negotiatedCostOfTraining = learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.AppFinRecords?
                                                   .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                                               x.AFinCode == 1).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount +
                                               learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.AppFinRecords?
                                                   .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                                               x.AFinCode == 2).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount;

                return negotiatedCostOfTraining.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_09))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault(x => x.AimSeqNumber == dasAimSeqNumber)?.LearnStartDate.ToString("dd/MM/yyyy");
            }

            return string.Empty;
        }

        private string GetLegalEntityName(string ruleName, DasApprenticeshipInfo dasApprenticeshipInfo)
        {
            var rulesWithBlankLegalEntityValues = new[] { DataLockValidationMessages.DLOCK_01, DataLockValidationMessages.DLOCK_02 };
            if (rulesWithBlankLegalEntityValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            return dasApprenticeshipInfo.LegalEntityName ?? string.Empty;
        }

        private string PopulateRuleName(int ruleId)
        {
            return _dLockErrorRuleNamePrefix + ruleId.ToString("00");
        }

        private string PopulateRuleDescription(string ruleName)
        {
            return DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(ruleName))?.ErrorMessage;
        }

        private string GetApprenticeshipServiceValue(string ruleName, DasApprenticeshipInfo dasApprenticeshipInfo)
        {
            if (_rulesWithBlankApprenticeshipValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_01))
            {
                return dasApprenticeshipInfo.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_03))
            {
                return dasApprenticeshipInfo.StandardCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return dasApprenticeshipInfo.FrameworkCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return dasApprenticeshipInfo.ProgrammeType.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return dasApprenticeshipInfo.PathwayCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                return dasApprenticeshipInfo?.Cost.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_10))
            {
                return dasApprenticeshipInfo?.WithdrawnOnDate?.ToString("dd/MM/yyyy");
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_12))
            {
                return dasApprenticeshipInfo?.PausedOnDate?.ToString("dd/MM/yyyy");
            }

            return string.Empty;
        }
    }
}