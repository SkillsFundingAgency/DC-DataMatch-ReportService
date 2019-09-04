using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IILRProviderService
    {
        Task<DataMatchILRInfo> GetILRInfoForDataMatchReport(int ukPrn, CancellationToken cancellationToken);
    }
}
