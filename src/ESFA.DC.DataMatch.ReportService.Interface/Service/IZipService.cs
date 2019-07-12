using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IZipService
    {
        Task AddReportToArchiveAsync(ZipArchive zipArchive, IReport report, IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
