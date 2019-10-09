using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
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
            _logger.LogInfo("Data Match Reporting callback invoked", jobIdOverride: reportServiceContext.JobId);

            cancellationToken.ThrowIfCancellationRequested();

            string reportZipFileKey;
            if (reportServiceContext.CollectionName.StartsWith("ILR", StringComparison.OrdinalIgnoreCase))
            {
                reportZipFileKey = $"{reportServiceContext.Ukprn}_{reportServiceContext.JobId}_Reports.zip";
            }
            else
            {
                reportZipFileKey = $"R{reportServiceContext.ReturnPeriod:00}_{reportServiceContext.Ukprn}_Reports.zip";
            }

            MemoryStream memoryStream = new MemoryStream();
            var zipFileExists = await _streamableKeyValuePersistenceService.ContainsAsync(reportZipFileKey, cancellationToken);
            if (zipFileExists)
            {
                await _streamableKeyValuePersistenceService.GetAsync(reportZipFileKey, memoryStream, cancellationToken);
            }

            using (memoryStream)
            {
                bool needZip;
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, true))
                {
                    needZip = await ExecuteTasks(reportServiceContext, archive, cancellationToken);
                }

                if (needZip)
                {
                    await _streamableKeyValuePersistenceService.SaveAsync(reportZipFileKey, memoryStream, cancellationToken);
                }
            }

            return true;
        }

        private async Task<bool> ExecuteTasks(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            bool needZip = false;
            foreach (string taskItem in reportServiceContext.Tasks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (await GenerateReportAsync(taskItem, reportServiceContext, archive, cancellationToken))
                {
                    needZip = true;
                }
            }

            return needZip;
        }

        private async Task<bool> GenerateReportAsync(string task, IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            bool needZip = false;
            bool foundReport = false;

            foreach (var report in _reports)
            {
                if (!report.IsMatch(task))
                {
                    continue;
                }

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.LogDebug($"Attempting to generate {report.GetType().Name}", jobIdOverride: reportServiceContext.JobId);
                needZip = await report.GenerateReport(reportServiceContext, archive, cancellationToken);
                stopWatch.Stop();
                _logger.LogDebug($"Persisted {report.GetType().Name} to csv/json in: {stopWatch.ElapsedMilliseconds}", jobIdOverride: reportServiceContext.JobId);

                foundReport = true;
                break;
            }

            if (!foundReport)
            {
                _logger.LogDebug($"Unable to find Data Match report '{task}'", jobIdOverride: reportServiceContext.JobId);
            }

            return needZip;
        }
    }
}
