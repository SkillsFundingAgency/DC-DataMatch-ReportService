using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IInternalDataMatchModelBuilder
    {
        IEnumerable<InternalDataMatchModel> BuildInternalModels(ICollection<DataMatchLearner> dataMatchLearners, ICollection<DataLockValidationError> dataLockValidationErrors, ICollection<ReturnPeriod> returnPeriods);
    }
}