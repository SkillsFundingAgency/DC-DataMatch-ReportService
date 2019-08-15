using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Model.DasPaymenets;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using Microsoft.EntityFrameworkCore.Internal;

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
            DataMatchDasApprenticeshipInfo dasApprenticeshipPriceInfo)
        {
            var dataMatchModels = new List<DataMatchModel>();
            foreach (var dataLockValidationError in dataLockValidationErrorInfo.DataLockValidationErrors)
            {
                var learner = dataMatchILRInfo.DataMatchLearners.SingleOrDefault(
                    x => x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber.ToString())); // ||
                //x.Uln == dataLockValidationError.LearnerUln);

                if (learner != null)
                {
                    var matchedRulebaseInfo = dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.LastOrDefault(x =>
                        x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber));

                    var matchedDasPriceInfo = dasApprenticeshipPriceInfo.DasApprenticeshipPriceInfos.FirstOrDefault(x => x.LearnerUln == dataLockValidationError.LearnerUln);

                    if (matchedDasPriceInfo != null)
                    {
                        var ruleName = PopulateRuleName(dataLockValidationError.RuleId);

                        var dataMatchModel = new DataMatchModel()
                        {
                            LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                            Uln = learner.Uln,
                            AimSeqNumber = dataLockValidationError.AimSeqNumber,
                            RuleName = ruleName,
                            Description = PopulateRuleDescription(ruleName),
                            ILRValue = GetILRValue(ruleName, learner),
                            ApprenticeshipServiceValue = GetApprenticeshipServiceValue(ruleName, dataLockValidationError, matchedDasPriceInfo),
                            PriceEpisodeStartDate = matchedRulebaseInfo?.EpisodeStartDate,
                            PriceEpisodeActualEndDate = matchedRulebaseInfo?.PriceEpisodeActualEndDate,
                            PriceEpisodeIdentifier = matchedRulebaseInfo?.PriceEpisodeAgreeId,
                            LegalEntityName = GetLegalEntityName(ruleName, matchedDasPriceInfo),
                        };

                        dataMatchModels.Add(dataMatchModel);
                    }
                }
            }

            return dataMatchModels;
        }

        private string GetILRValue(string ruleName, DataMatchLearner learner)
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
                return learner.DataMatchLearningDeliveries.FirstOrDefault()?.StdCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault()?.FworkCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault()?.ProgType.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault()?.PwayCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                var negotiatedCostOfTraining = learner.DataMatchLearningDeliveries.FirstOrDefault()?.AppFinRecords?
                                                   .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                                               x.AFinCode == 1).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount +
                                               learner.DataMatchLearningDeliveries.FirstOrDefault()?.AppFinRecords?
                                                   .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                                               x.AFinCode == 2).OrderByDescending(x => x.AFinDate).FirstOrDefault()?.AFinAmount;

                return negotiatedCostOfTraining.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_09))
            {
                return learner.DataMatchLearningDeliveries.FirstOrDefault()?.LearnStartDate.ToString();
            }

            return string.Empty;
        }

        private string GetLegalEntityName(string ruleName, DasApprenticeshipPriceInfo dasApprenticeshipPriceInfo)
        {
            var rulesWithBlankLegalEntityValues = new[] { DataLockValidationMessages.DLOCK_01, DataLockValidationMessages.DLOCK_02 };
            if (rulesWithBlankLegalEntityValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            return dasApprenticeshipPriceInfo.LegalEntityName ?? string.Empty;
        }

        private string PopulateRuleName(int ruleId)
        {
            return _dLockErrorRuleNamePrefix + ruleId.ToString("00");
        }

        private string PopulateRuleDescription(string ruleName)
        {
            return DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(ruleName))?.ErrorMessage;
        }

        private string GetApprenticeshipServiceValue(string ruleName, DataLockValidationError dataLockValidationError, DasApprenticeshipPriceInfo dasApprenticeshipPriceInfo)
        {
            if (_rulesWithBlankApprenticeshipValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_01))
            {
                return dataLockValidationError.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_03))
            {
                return dataLockValidationError.StandardCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return dataLockValidationError.FrameworkCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return dataLockValidationError.ProgrammeType.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return dataLockValidationError.PathwayCode.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                return dasApprenticeshipPriceInfo.Cost.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_10))
            {
                return dasApprenticeshipPriceInfo.WithdrawnOnDate?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_12))
            {
                return dasApprenticeshipPriceInfo.PausedOnDate?.ToString();
            }

            return string.Empty;
        }
    }
}
