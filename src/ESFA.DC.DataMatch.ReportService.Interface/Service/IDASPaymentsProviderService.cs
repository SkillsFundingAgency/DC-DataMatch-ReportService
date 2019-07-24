using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.ILR.ReportService.Model.DASPayments;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IDASPaymentsProviderService
    {
        Task<IEnumerable<DasApprenticeshipInfo>> GetApprenticeshipsInfoAsync(long ukPrn, CancellationToken cancellationToken);
    }
}
