using System.Collections.Generic;
using System.Linq;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Extensions;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.Internal
{
    public class InternalDataMatchMonthEndModelBuilder : IInternalDataMatchModelBuilder
    {
        public IEnumerable<InternalDataMatchModel> BuildInternalModels(
            DataMatchILRInfo dataMatchILRInfo,
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo,
            List<ReturnPeriod> returnPeriods,
            long jobId)
        {
            List<InternalDataMatchModel> dataMatchModels = new List<InternalDataMatchModel>();
            foreach (var dataLockValidationError in dataLockValidationErrorInfo.DataLockValidationErrors)
            {
                DataMatchLearner learner = dataMatchILRInfo.DataMatchLearners.SingleOrDefault(
                    x => x.LearnRefNumber.CaseInsensitiveEquals(dataLockValidationError.LearnerReferenceNumber) &&
                         x.DataMatchLearningDeliveries.Any(ld => ld.AimSeqNumber == dataLockValidationError.AimSeqNumber));

                if (learner == null)
                {
                    continue;
                }

                string ruleName = PopulateRuleName(dataLockValidationError.RuleId);

                ReturnPeriod period = returnPeriods.Single(x => x.PeriodNumber == dataLockValidationError.CollectionPeriod);

                InternalDataMatchModel dataMatchModel = new InternalDataMatchModel
                {
                    Collection = dataLockValidationError.Collection,
                    Ukprn = (int)dataLockValidationError.UkPrn,
                    LearnRefNumber = dataLockValidationError.LearnerReferenceNumber,
                    Uln = learner.Uln,
                    AimSeqNumber = dataLockValidationError.AimSeqNumber,
                    RuleName = ruleName,
                    CollectionPeriodName = $"{period.CollectionName}-R{dataLockValidationError.CollectionPeriod:D2}",
                    CollectionPeriodMonth = period.CalendarMonth,
                    CollectionPeriodYear = period.CalendarYear,
                    LastSubmission = dataLockValidationError.LastSubmission
                };

                dataMatchModels.Add(dataMatchModel);
            }

            return dataMatchModels;
        }

        private string PopulateRuleName(int ruleId)
        {
            return Constants.DLockErrorRuleNamePrefix + ruleId.ToString("00");
        }
    }
}