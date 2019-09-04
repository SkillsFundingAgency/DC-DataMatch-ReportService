using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public sealed class EntryPoint
    {
        private readonly ILogger _logger;

        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        private readonly IList<IReport> _reports;

        public EntryPoint(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IList<IReport> reports)
        {
            _logger = logger;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _reports = reports;
        }

        public async Task<bool> Callback(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Reporting callback invoked");

            var reportZipFileKey = $"{reportServiceContext.Ukprn}_{reportServiceContext.JobId}_Reports.zip";
            cancellationToken.ThrowIfCancellationRequested();

            MemoryStream memoryStream = new MemoryStream();
            var zipFileExists = await _streamableKeyValuePersistenceService.ContainsAsync(reportZipFileKey, cancellationToken);
            if (zipFileExists)
            {
                await _streamableKeyValuePersistenceService.GetAsync(reportZipFileKey, memoryStream, cancellationToken);
            }

            using (memoryStream)
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, true))
                {
                    await ExecuteTasks(reportServiceContext, archive, cancellationToken);
                }

                await _streamableKeyValuePersistenceService.SaveAsync(reportZipFileKey, memoryStream, cancellationToken);
            }

            return true;
        }

        private async Task ExecuteTasks(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            foreach (string taskItem in reportServiceContext.Tasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await GenerateReportAsync(taskItem, reportServiceContext, archive, cancellationToken);
            }
        }

        private async Task GenerateReportAsync(string task, IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            var foundReport = false;
            foreach (var report in _reports)
            {
                if (!report.IsMatch(task))
                {
                    continue;
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.LogDebug($"Attempting to generate {report.GetType().Name}");
                await report.GenerateReport(reportServiceContext, archive, cancellationToken);
                stopWatch.Stop();
                _logger.LogDebug($"Persisted {report.GetType().Name} to csv/json in: {stopWatch.ElapsedMilliseconds}");

                foundReport = true;
                break;
            }

            if (!foundReport)
            {
                _logger.LogDebug($"Unable to find report '{task}'");
            }
        }
    }
}
