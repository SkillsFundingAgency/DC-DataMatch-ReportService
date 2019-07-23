using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1819.DataStore.EF.Valid;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IValidLearnersService
    {
        Task<List<Learner>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
