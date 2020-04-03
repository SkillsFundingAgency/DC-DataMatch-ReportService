using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.Internal
{
    public class InternalDataMatchMonthEndModelBuilder : IInternalDataMatchModelBuilder
    {
        private readonly IDataLockValidationMessageService _dataLockValidationMessageService;

        public InternalDataMatchMonthEndModelBuilder(IDataLockValidationMessageService dataLockValidationMessageService)
        {
            _dataLockValidationMessageService = dataLockValidationMessageService;
        }

        public IEnumerable<InternalDataMatchModel> BuildInternalModels(ICollection<DataLockValidationError> dataLockValidationErrors, ICollection<ReturnPeriod> returnPeriods)
        {
            IDictionary<int, ReturnPeriod> returnPeriodLookup = returnPeriods.ToDictionary(r => r.PeriodNumber, r => r);

            return dataLockValidationErrors
                .Select(dataLockValidationError =>
                {
                    var period = returnPeriodLookup[dataLockValidationError.CollectionPeriod];

                    return new InternalDataMatchModel
                    {
                        Collection = dataLockValidationError.Collection,
                        Ukprn = dataLockValidationError.UkPrn,
                        LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                        Uln = dataLockValidationError.LearnerUln,
                        AimSeqNumber = dataLockValidationError.AimSeqNumber,
                        RuleName = _dataLockValidationMessageService.RuleNameForRuleId(dataLockValidationError.RuleId),
                        CollectionPeriodName = $"{period.CollectionName}-R{dataLockValidationError.CollectionPeriod:D2}",
                        CollectionPeriodMonth = period.CalendarMonth,
                        CollectionPeriodYear = period.CalendarYear,
                        LastSubmission = dataLockValidationError.LastSubmission
                    };
                })
                .OrderBy(m => m.Collection)
                .ThenBy(m => m.Ukprn)
                .ThenBy(m => m.LearnRefNumber);
        }
    }
}