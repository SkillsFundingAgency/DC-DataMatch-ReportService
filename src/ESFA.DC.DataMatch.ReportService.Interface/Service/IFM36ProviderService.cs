using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IFM36ProviderService
    {
        Task<DataMatchRulebaseInfo> GetFM36DataForDataMatchReport(int ukPrn, CancellationToken cancellationToken);
    }
}
