using System.Collections.Generic;
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
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
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
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IILRProviderService _ilrProviderService;
        private readonly IDataMatchModelBuilder _dataMatchModelBuilder;
        private readonly DataMatchModelComparer _dataMatchModelComparer;

        public DataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IFM36ProviderService fm36ProviderService,
            IILRProviderService iIlrProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            DataMatchModelComparer dataMatchModelComparer,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _fm36ProviderService = fm36ProviderService;
            _ilrProviderService = iIlrProviderService;
            _dataMatchModelBuilder = dataMatchModelBuilder;
            _dataMatchModelComparer = dataMatchModelComparer;
        }

        public override string ReportFileName => "Apprenticeship Data Match Report";

        public override string ReportTaskName => ReportTaskNameConstants.DataMatchReport;

        public override async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, CancellationToken cancellationToken)
        {
            _logger.LogInfo("Generate Data Match Report started");
            Task<DataMatchILRInfo> dataMatchILRInfoTask = _ilrProviderService.GetILRInfoForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);
            Task<DataMatchRulebaseInfo> dataMatchRulebaseInfoTask = _fm36ProviderService.GetFM36DataForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            Task<DataMatchDataLockValidationErrorInfo> dataLockValidationErrorInfoTask = _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(reportServiceContext.ReturnPeriod, reportServiceContext.Ukprn, reportServiceContext.CollectionName, cancellationToken);
            Task<DataMatchDasApprenticeshipInfo> dasApprenticeshipPriceInfoTask = _dasPaymentsProviderService.GetDasApprenticeshipInfoForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(dataMatchILRInfoTask, dataMatchRulebaseInfoTask, dataLockValidationErrorInfoTask, dasApprenticeshipPriceInfoTask);

            var dataMatchILRInfo = dataMatchILRInfoTask.Result;
            var dataMatchRulebaseInfo = dataMatchRulebaseInfoTask.Result;
            var dataLockValidationErrorInfo = dataLockValidationErrorInfoTask.Result;
            var dasApprenticeshipPriceInfo = dasApprenticeshipPriceInfoTask.Result;

            _logger.LogInfo($"dataMatchILRInfo (learners with ACT1 and FM36 in ILR) count {dataMatchILRInfo.DataMatchLearners.Count}");
            _logger.LogInfo($"dataMatchRulebaseInfo (AEC_ApprenticeshipPriceEpisodes) count {dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.Count}");
            _logger.LogInfo($"dataLockValidationErrorInfo (DataLockEvents + joins) count {dataLockValidationErrorInfo.DataLockValidationErrors.Count}");
            _logger.LogInfo($"dasApprenticeshipPriceInfo (Payments.ApprenticeshipPriceEpisodes) count {dasApprenticeshipPriceInfo.DasApprenticeshipInfos.Count}");

            _logger.LogInfo($"using the above to build the model...");
            List<DataMatchModel> dataMatchModels = _dataMatchModelBuilder.BuildModels(dataMatchILRInfo, dataMatchRulebaseInfo, dataLockValidationErrorInfo, dasApprenticeshipPriceInfo).ToList();
            _logger.LogInfo($"dataMatchModels count (lines to go in the report) {dataMatchModels.Count}");
            dataMatchModels.Sort(_dataMatchModelComparer);

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
