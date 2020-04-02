using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IExternalDataMatchModelBuilder
    {
        IEnumerable<DataMatchModel> BuildExternalModels(
            ICollection<DataMatchLearner> dataMatchlearners,
            DataMatchRulebaseInfo dataMatchRulebaseInfo,
            ICollection<DataLockValidationError> dataLockValidationErrors,
            DataMatchDasApprenticeshipInfo dasApprenticeshipPriceInfo,
            long jobId);
    }
}