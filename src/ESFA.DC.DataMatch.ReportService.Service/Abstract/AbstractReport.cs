using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Abstract
{
    public abstract class AbstractReport : IReport
    {
        protected readonly ILogger _logger;

        protected readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        private readonly IDateTimeProvider _dateTimeProvider;

        protected AbstractReport(IDateTimeProvider dateTimeProvider, IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService, ILogger logger)
        {
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        public abstract string ReportFileName { get; }

        public abstract string ReportTaskName { get; }

        public string GetFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"{reportServiceContext.Ukprn}_{reportServiceContext.JobId}_{ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        public string GetZipFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);
            return $"{ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        public abstract Task<bool> GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken);

        public bool IsMatch(string reportTaskName)
        {
            return string.Equals(reportTaskName, ReportTaskName, StringComparison.OrdinalIgnoreCase);
        }

        protected string WriteResults<TMapper, TModel>(IReadOnlyCollection<TModel> models)
            where TMapper : ClassMap
            where TModel : class
        {
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<TMapper, TModel>(csvWriter, models);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        protected void WriteCsvRecords<TMapper, TModel>(CsvWriter csvWriter, IEnumerable<TModel> records)
            where TMapper : ClassMap
            where TModel : class
        {
            csvWriter.Configuration.RegisterClassMap<TMapper>();

            csvWriter.WriteHeader<TModel>();
            csvWriter.NextRecord();

            csvWriter.WriteRecords(records);

            csvWriter.Configuration.UnregisterClassMap();
        }

        /// <summary>
        /// Writes the data to the zip file with the specified filename.
        /// </summary>
        /// <param name="archive">Archive to write to.</param>
        /// <param name="filename">Filename to use in zip file.</param>
        /// <param name="data">Data to write.</param>
        /// <returns>Awaitable task.</returns>
        protected async Task WriteZipEntry(ZipArchive archive, string filename, string data)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry entry = archive.GetEntry(filename);
            entry?.Delete();

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using (StreamWriter sw = new StreamWriter(archivedFile.Open()))
            {
                await sw.WriteAsync(data);
            }
        }
    }
}
