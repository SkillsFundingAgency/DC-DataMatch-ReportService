using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IILRProviderService
    {
        Task<ICollection<DataMatchLearner>> GetILRInfoForDataMatchReport(int ukPrn, List<long> learners, CancellationToken cancellationToken);
    }
}
