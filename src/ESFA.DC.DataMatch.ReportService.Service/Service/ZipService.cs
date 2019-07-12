using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.FileService.Interface;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class ZipService : IZipService
    {
        private readonly IFileService _fileService;

        public ZipService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task AddReportToArchiveAsync(ZipArchive zipArchive, IReport report, IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            var zipFileName = report.GetZipFilename(reportServiceContext);
            var fileReference = report.GetFilename(reportServiceContext);

            ZipArchiveEntry archiveEntry = zipArchive.GetEntry(zipFileName);
            archiveEntry?.Delete();

            archiveEntry = zipArchive.CreateEntry(zipFileName, CompressionLevel.Optimal);

            using (var fileStream = await _fileService.OpenReadStreamAsync(fileReference, reportServiceContext.Container, cancellationToken))
            using (var stream = archiveEntry.Open())
            {
                await fileStream.CopyToAsync(stream, 81920, cancellationToken);
            }
        }
    }
}
