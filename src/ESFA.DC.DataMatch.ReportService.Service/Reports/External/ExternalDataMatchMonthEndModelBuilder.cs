using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.External
{
    public sealed class ExternalDataMatchMonthEndModelBuilder : IExternalDataMatchModelBuilder
    {
        private readonly ILogger _logger;

        private readonly string[] _rulesWithBlankILRValues =
        {
            DataLockValidationMessages.DLOCK_08,
            DataLockValidationMessages.DLOCK_10,
            DataLockValidationMessages.DLOCK_11,
            DataLockValidationMessages.DLOCK_12,
        };

        private readonly string[] _rulesWithBlankApprenticeshipValues =
        {
            DataLockValidationMessages.DLOCK_02,
            DataLockValidationMessages.DLOCK_08,
            DataLockValidationMessages.DLOCK_09,
            DataLockValidationMessages.DLOCK_11,
        };

        private readonly string[] _rulesWithBlankLegalEntityValues =
        {
            DataLockValidationMessages.DLOCK_01,
            DataLockValidationMessages.DLOCK_02
        };

        public ExternalDataMatchMonthEndModelBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<DataMatchModel> BuildExternalModels(
            DataMatchILRInfo dataMatchILRInfo,
            DataMatchRulebaseInfo dataMatchRulebaseInfo,
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo,
            DataMatchDasApprenticeshipInfo dasApprenticeshipInfo,
            long jobId)
        {
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>();
            foreach (var dataLockValidationError in dataLockValidationErrorInfo.DataLockValidationErrors)
            {
                DataMatchLearner learner = dataMatchILRInfo.DataMatchLearners.SingleOrDefault(
                    x => x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber) &&
                         x.DataMatchLearningDeliveries.Any(ld => ld.AimSeqNumber == dataLockValidationError.AimSeqNumber));

                if (learner == null)
                {
                    continue;
                }

                AECApprenticeshipPriceEpisodeInfo matchedRulebaseInfo = dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.LastOrDefault(x =>
                    x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber));

                DasApprenticeshipInfo matchedDasPriceInfo = dasApprenticeshipInfo.DasApprenticeshipInfos.FirstOrDefault(x => x.LearnerUln == dataLockValidationError.LearnerUln);

                string ruleName = PopulateRuleName(dataLockValidationError.RuleId);

                DataMatchModel dataMatchModel = new DataMatchModel
                {
                    LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                    Uln = learner.Uln,
                    AimSeqNumber = dataLockValidationError.AimSeqNumber,
                    RuleName = ruleName,
                    Description = PopulateRuleDescription(ruleName),
                    ILRValue = GetILRValue(ruleName, learner, dataLockValidationError.AimSeqNumber, jobId),
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

        private string GetILRValue(string ruleName, DataMatchLearner learner, long? dasAimSeqNumber, long jobId)
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

            var validLearningDeliveries = learner.DataMatchLearningDeliveries.Where(x => x.AimSeqNumber == dasAimSeqNumber).ToList();

            if (validLearningDeliveries.Count > 1)
            {
                _logger.LogInfo($"Multiple matching learning deliveries found for leaner {learner.LearnRefNumber}", jobIdOverride: jobId);
                foreach (var ld in validLearningDeliveries)
                {
                    _logger.LogInfo(
                        $"AimSeq-{ld.AimSeqNumber}_FworkCode-{ld.FworkCode}_StdCode-{ld.StdCode}_PwayCode-{ld.PwayCode}_ProgType{ld.ProgType}_LearnStartDate-{ld.LearnStartDate}", jobIdOverride: jobId);
                }
            }

            DataMatchLearningDelivery validLearningDelivery = validLearningDeliveries.FirstOrDefault();

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_03))
            {
                return validLearningDelivery?.StdCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return validLearningDelivery?.FworkCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return validLearningDelivery?.ProgType?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return validLearningDelivery?.PwayCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                var appFinRecords = validLearningDelivery?.AppFinRecords;
                if (appFinRecords == null || !appFinRecords.Any())
                {
                    _logger.LogInfo("DLOCK_07 - Empty ILR Value(Negotiated Cost) due to no appfinrecords", jobIdOverride: jobId);
                    return string.Empty;
                }

                var tnp1 = appFinRecords.Where(x =>
                    string.Equals(x.AFinType, Constants.AppFinRecordType_TNP, StringComparison.OrdinalIgnoreCase) &&
                    x.AFinCode == 1).OrderByDescending(x => x.AFinDate).ToList();

                if (tnp1.Count > 1)
                {
                    _logger.LogInfo($"Multiple tnp1 AppFinRecords found for leaner {learner.LearnRefNumber}", jobIdOverride: jobId);
                    foreach (var tnp in tnp1)
                    {
                        _logger.LogInfo(
                            $"TNP1_AFinAmount-{tnp.AFinAmount}_AFinDate-{tnp.AFinDate}_AimSeqNumber-{tnp.AimSeqNumber}", jobIdOverride: jobId);
                    }
                }

                var tnp1Value = tnp1.FirstOrDefault();

                var tnp2 = appFinRecords.Where(x => string.Equals(x.AFinType, Constants.AppFinRecordType_TNP, StringComparison.OrdinalIgnoreCase) &&
                                                    x.AFinCode == 2).OrderByDescending(x => x.AFinDate).ToList();

                if (tnp2.Count > 1)
                {
                    _logger.LogInfo($"Multiple tnp2 AppFinRecords found for leaner {learner.LearnRefNumber}", jobIdOverride: jobId);
                    foreach (var tnp in tnp2)
                    {
                        _logger.LogInfo(
                            $"TNP2_AFinAmount-{tnp.AFinAmount}_AFinDate-{tnp.AFinDate}_AimSeqNumber-{tnp.AimSeqNumber}", jobIdOverride: jobId);
                    }
                }

                var tnp2Value = tnp2.FirstOrDefault();

                var negotiatedCostOfTraining = (tnp1Value?.AFinAmount ?? 0) + (tnp2Value?.AFinAmount ?? 0);

                return negotiatedCostOfTraining.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_09))
            {
                return validLearningDelivery?.LearnStartDate.ToString("dd/MM/yyyy");
            }

            return string.Empty;
        }

        private string GetLegalEntityName(string ruleName, DasApprenticeshipInfo dasApprenticeshipInfo)
        {
            if (_rulesWithBlankLegalEntityValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            return dasApprenticeshipInfo?.LegalEntityName ?? string.Empty;
        }

        private string PopulateRuleName(int ruleId)
        {
            return Constants.DLockErrorRuleNamePrefix + ruleId.ToString("00");
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
                return dasApprenticeshipInfo?.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_03))
            {
                return dasApprenticeshipInfo?.StandardCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_04))
            {
                return dasApprenticeshipInfo?.FrameworkCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_05))
            {
                return dasApprenticeshipInfo?.ProgrammeType?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_06))
            {
                return dasApprenticeshipInfo?.PathwayCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationMessages.DLOCK_07))
            {
                return dasApprenticeshipInfo?.Cost.ToString(CultureInfo.InvariantCulture);
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