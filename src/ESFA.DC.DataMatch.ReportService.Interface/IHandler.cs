using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Context;

namespace ESFA.DC.DataMatch.ReportService.Interface
{
    public interface IHandler
    {
        Task<bool> HandleAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}