using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IDASPaymentsProviderService
    {
        Task<DataMatchDataLockValidationErrorInfo> GetDataLockValidationErrorInfoForDataMatchReport(
            int collectionPeriod,
            int ukPrn,
            string[] learnRefNumbers,
            string collectionYear,
            long? jobId,
            CancellationToken cancellationToken);

        Task<DataMatchDasApprenticeshipInfo> GetDasApprenticeshipInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken);
    }
}
