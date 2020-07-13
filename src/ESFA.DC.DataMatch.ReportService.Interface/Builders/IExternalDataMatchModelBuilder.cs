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
            ICollection<AECApprenticeshipPriceEpisodeInfo> priceEpisodes,
            ICollection<DataLockValidationError> dataLockValidationErrors,
            ICollection<DasApprenticeshipInfo> dasApprenticeshipInfos,
            long jobId);
    }
}