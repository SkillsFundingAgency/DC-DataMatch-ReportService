using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.Internal
{
    public class InternalDataMatchMonthEndModelBuilder : IInternalDataMatchModelBuilder
    {
        private readonly IDataLockValidationMessageService _dataLockValidationMessageService;

        public InternalDataMatchMonthEndModelBuilder(IDataLockValidationMessageService dataLockValidationMessageService)
        {
            _dataLockValidationMessageService = dataLockValidationMessageService;
        }

        public IEnumerable<InternalDataMatchModel> BuildInternalModels(ICollection<DataMatchLearner> dataMatchLearners, DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo, ICollection<ReturnPeriod> returnPeriods)
        {
            IDictionary<string, DataMatchLearner> dataMatchLearnerLookup = dataMatchLearners.ToDictionary(l => l.LearnRefNumber, l => l, StringComparer.OrdinalIgnoreCase);
            IDictionary<int, ReturnPeriod> returnPeriodLookup = returnPeriods.ToDictionary(r => r.PeriodNumber, r => r);

            foreach (var dataLockValidationError in dataLockValidationErrorInfo.DataLockValidationErrors)
            {
                var learner = dataMatchLearnerLookup.GetValueOrDefault(dataLockValidationError.LearnerReferenceNumber);

                if (learner == null || learner.DataMatchLearningDeliveries.All(ld => ld.AimSeqNumber != dataLockValidationError.AimSeqNumber))
                {
                    continue;
                }

                string ruleName = _dataLockValidationMessageService.RuleNameForRuleId(dataLockValidationError.RuleId);

                ReturnPeriod period = returnPeriodLookup[dataLockValidationError.CollectionPeriod];

                yield return new InternalDataMatchModel
                {
                    Collection = dataLockValidationError.Collection,
                    Ukprn = dataLockValidationError.UkPrn,
                    LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                    Uln = learner.Uln,
                    AimSeqNumber = dataLockValidationError.AimSeqNumber,
                    RuleName = ruleName,
                    CollectionPeriodName = $"{period.CollectionName}-R{dataLockValidationError.CollectionPeriod:D2}",
                    CollectionPeriodMonth = period.CalendarMonth,
                    CollectionPeriodYear = period.CalendarYear,
                    LastSubmission = dataLockValidationError.LastSubmission
                };
            }
        }
    }
}