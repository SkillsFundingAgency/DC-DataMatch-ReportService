using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IDASPaymentsProviderService
    {
        Task<ICollection<DataLockValidationError>> GetDataLockValidationErrorInfoForDataMatchReport(int collectionPeriod, int ukPrn, string collectionYear, CancellationToken cancellationToken);

        Task<ICollection<DasApprenticeshipInfo>> GetDasApprenticeshipInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken);
    }
}
