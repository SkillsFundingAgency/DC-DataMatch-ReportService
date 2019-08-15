using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Abstract
{
    public abstract class AbstractReport : IReport
    {
        protected readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        protected readonly ILogger _logger;

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

        public abstract Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken);

        public bool IsMatch(string reportTaskName)
        {
            return string.Equals(reportTaskName, ReportTaskName, StringComparison.OrdinalIgnoreCase);
        }

        protected Stream WriteModelsToCsv<TMapper, TModel>(Stream stream, IEnumerable<TModel> models)
            where TMapper : ClassMap
            where TModel : class
        {
            using (TextWriter textWriter = new StreamWriter(stream))
            {
                using (CsvWriter csvWriter = new CsvWriter(textWriter))
                {
                    WriteCsvRecords<TMapper, TModel>(csvWriter, models);
                }
            }

            return stream;
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

        protected void WriteCsvRecords<TMapper>(CsvWriter csvWriter, TMapper mapper)
            where TMapper : ClassMap
        {
            object[] names = mapper.MemberMaps.OrderBy(x => x.Data.Index).SelectMany(x => x.Data.Names.Names).Select(x => (object)x).ToArray();
            WriteCsvRecords(csvWriter, names);
        }

        /// <summary>
        /// Builds a CSV report using the specified mapper as the list of column names.
        /// </summary>
        /// <typeparam name="TMapper">The mapper.</typeparam>
        /// <typeparam name="TModel">The model.</typeparam>
        /// <param name="writer">The memory stream to write to.</param>
        /// <param name="record">The record to persist.</param>
        protected void WriteCsvRecords<TMapper, TModel>(CsvWriter writer, TModel record)
            where TMapper : ClassMap
            where TModel : class
        {
            WriteCsvRecords<TMapper, TModel>(writer, new[] { record });
        }

        /// <summary>
        /// Writes a blank row to the csv file.
        /// </summary>
        /// <param name="writer">The memory stream to write to.</param>
        /// <param name="numberOfBlankRows">The optional number of blank rows to create.</param>
        protected void WriteCsvRecords(CsvWriter writer, int numberOfBlankRows = 1)
        {
            for (int i = 0; i < numberOfBlankRows; i++)
            {
                writer.NextRecord();
            }
        }

        /// <summary>
        /// Writes the items as individual tokens to the CSV.
        /// </summary>
        /// <param name="writer">The writer target.</param>
        /// <param name="items">The strings to write.</param>
        protected void WriteCsvRecords(CsvWriter writer, params object[] items)
        {
            foreach (object item in items)
            {
                writer.WriteField(item);
            }

            writer.NextRecord();
        }

        /// <summary>
        /// Writes the stream to the zip file with the specified filename.
        /// </summary>
        /// <param name="archive">Archive to write to.</param>
        /// <param name="filename">Filename to use in zip file.</param>
        /// <param name="data">Data to write.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling copy operation.</param>
        /// <returns>Awaitable task.</returns>
        protected async Task WriteZipEntry(ZipArchive archive, string filename, Stream data, CancellationToken cancellationToken)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry entry = archive.GetEntry(filename);
            entry?.Delete();

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using (Stream sw = archivedFile.Open())
            {
                data.Position = 0;
                await data.CopyToAsync(sw, 81920, cancellationToken);
            }
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
