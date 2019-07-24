﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Abstract;
using ESFA.DC.DataMatch.ReportService.Service.Comparer;
using ESFA.DC.DataMatch.ReportService.Service.Mapper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public sealed class DataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IDataMatchModelBuilder _dataMatchModelBuilder;

        private static readonly DataMatchModelComparer DataMatchModelComparer = new DataMatchModelComparer();

        public DataMatchReport(
            ILogger logger,
            IDASPaymentsProviderService dasPaymentsProviderService,
            IValidLearnersService validLearnersService,
            IFM36ProviderService fm36ProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _validLearnersService = validLearnersService;
            _fm36ProviderService = fm36ProviderService;
            _dataMatchModelBuilder = dataMatchModelBuilder;
        }

        public override string ReportFileName => "Apprenticeship Data Match Report";

        public override string ReportTaskName => ReportTaskNameConstants.DataMatchReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            var validIlrLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
            var DasApprenticeshipInfoTask =
                _dasPaymentsProviderService.GetApprenticeshipsInfoAsync(reportServiceContext.Ukprn, cancellationToken);
            var dataMatchRulebaseInfoTask =
                _fm36ProviderService.GetFM36DataForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(validIlrLearnersTask, DasApprenticeshipInfoTask, dataMatchRulebaseInfoTask);

            var validIlrLearners = validIlrLearnersTask.Result;
            var dasApprenticeshipInfos = DasApprenticeshipInfoTask.Result;
            var dataMatchRulebaseInfo = dataMatchRulebaseInfoTask.Result;

            cancellationToken.ThrowIfCancellationRequested();
            var dataMatchModels = _dataMatchModelBuilder.BuildModels(validIlrLearners, dasApprenticeshipInfos, dataMatchRulebaseInfo)?.ToList();
            dataMatchModels?.Sort(DataMatchModelComparer);

            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);
            string csv = WriteResults(dataMatchModels);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private string WriteResults(IReadOnlyCollection<DataMatchModel> models)
        {
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<DataMatchMapper, DataMatchModel>(csvWriter, models);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
