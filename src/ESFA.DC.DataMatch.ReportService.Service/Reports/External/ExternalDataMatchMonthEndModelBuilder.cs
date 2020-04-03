using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
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
        private readonly IDataLockValidationMessageService _dataLockValidationMessageService;
        private readonly ILogger _logger;

        private readonly string[] _rulesWithBlankILRValues =
        {
            DataLockValidationErrorIdConstants.DLOCK_08,
            DataLockValidationErrorIdConstants.DLOCK_10,
            DataLockValidationErrorIdConstants.DLOCK_11,
            DataLockValidationErrorIdConstants.DLOCK_12,
        };

        private readonly string[] _rulesWithBlankApprenticeshipValues =
        {
            DataLockValidationErrorIdConstants.DLOCK_02,
            DataLockValidationErrorIdConstants.DLOCK_08,
            DataLockValidationErrorIdConstants.DLOCK_09,
            DataLockValidationErrorIdConstants.DLOCK_11,
        };

        private readonly string[] _rulesWithBlankLegalEntityValues =
        {
            DataLockValidationErrorIdConstants.DLOCK_01,
            DataLockValidationErrorIdConstants.DLOCK_02
        };

        public ExternalDataMatchMonthEndModelBuilder(IDataLockValidationMessageService dataLockValidationMessageService, ILogger logger)
        {
            _dataLockValidationMessageService = dataLockValidationMessageService;
            _logger = logger;
        }

        public IEnumerable<DataMatchModel> BuildExternalModels(
            ICollection<DataMatchLearner> dataMatchLearners,
            DataMatchRulebaseInfo dataMatchRulebaseInfo,
            ICollection<DataLockValidationError> dataLockValidationErrors,
            ICollection<DasApprenticeshipInfo> dasApprenticeshipInfos,
            long jobId)
        {
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>();
            foreach (var dataLockValidationError in dataLockValidationErrors)
            {
                DataMatchLearner learner = dataMatchLearners.SingleOrDefault(
                    x => x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber) &&
                         x.DataMatchLearningDeliveries.Any(ld => ld.AimSeqNumber == dataLockValidationError.AimSeqNumber));

                if (learner == null)
                {
                    continue;
                }

                AECApprenticeshipPriceEpisodeInfo matchedRulebaseInfo = dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.LastOrDefault(x =>
                    x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber));

                DasApprenticeshipInfo matchedDasPriceInfo = dasApprenticeshipInfos.FirstOrDefault(x => x.LearnerUln == dataLockValidationError.LearnerUln);

                string ruleName = PopulateRuleName(dataLockValidationError.RuleId);

                DataMatchModel dataMatchModel = new DataMatchModel
                {
                    LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                    Uln = learner.Uln,
                    AimSeqNumber = dataLockValidationError.AimSeqNumber,
                    RuleName = ruleName,
                    Description = _dataLockValidationMessageService.ErrorMessageForRule(ruleName),
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

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_01))
            {
                return learner.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_02))
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

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_03))
            {
                return validLearningDelivery?.StdCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_04))
            {
                return validLearningDelivery?.FworkCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_05))
            {
                return validLearningDelivery?.ProgType?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_06))
            {
                return validLearningDelivery?.PwayCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_07))
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
                            $"TNP1_AFinAmount-{tnp.AFinAmount}_AFinDate-{tnp.AFinDate}_AimSeqNumber-{validLearningDelivery.AimSeqNumber}", jobIdOverride: jobId);
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
                            $"TNP2_AFinAmount-{tnp.AFinAmount}_AFinDate-{tnp.AFinDate}_AimSeqNumber-{validLearningDelivery.AimSeqNumber}", jobIdOverride: jobId);
                    }
                }

                var tnp2Value = tnp2.FirstOrDefault();

                var negotiatedCostOfTraining = (tnp1Value?.AFinAmount ?? 0) + (tnp2Value?.AFinAmount ?? 0);

                return negotiatedCostOfTraining.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_09))
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

        private string GetApprenticeshipServiceValue(string ruleName, DasApprenticeshipInfo dasApprenticeshipInfo)
        {
            if (_rulesWithBlankApprenticeshipValues.Any(x => x.CaseInsensitiveEquals(ruleName)))
            {
                return string.Empty;
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_01))
            {
                return dasApprenticeshipInfo?.UkPrn.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_03))
            {
                return dasApprenticeshipInfo?.StandardCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_04))
            {
                return dasApprenticeshipInfo?.FrameworkCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_05))
            {
                return dasApprenticeshipInfo?.ProgrammeType?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_06))
            {
                return dasApprenticeshipInfo?.PathwayCode?.ToString();
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_07))
            {
                return dasApprenticeshipInfo?.Cost.ToString(CultureInfo.InvariantCulture);
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_10))
            {
                return dasApprenticeshipInfo?.WithdrawnOnDate?.ToString("dd/MM/yyyy");
            }

            if (ruleName.CaseInsensitiveEquals(DataLockValidationErrorIdConstants.DLOCK_12))
            {
                return dasApprenticeshipInfo?.PausedOnDate?.ToString("dd/MM/yyyy");
            }

            return string.Empty;
        }
    }
}