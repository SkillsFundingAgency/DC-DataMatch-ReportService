using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Context;

namespace ESFA.DC.DataMatch.ReportService.Interface.Reports
{
    public interface IReport
    {
        string ReportTaskName { get; }

        string ReportFileName { get; }

        string GetFilename(IReportServiceContext reportServiceContext);

        string GetZipFilename(IReportServiceContext reportServiceContext);

        Task<bool> GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken);

        bool IsMatch(string reportTaskName);
    }
}
