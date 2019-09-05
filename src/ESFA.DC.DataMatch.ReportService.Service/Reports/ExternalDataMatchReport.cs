using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Context;
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

namespace ESFA.DC.DataMatch.ReportService.Service.Reports
{
    public sealed class ExternalDataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IILRProviderService _ilrProviderService;
        private readonly IDataMatchModelBuilder _dataMatchModelBuilder;
        private readonly ExternalDataMatchModelComparer _dataMatchModelComparer;

        public ExternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IFM36ProviderService fm36ProviderService,
            IILRProviderService iIlrProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            ExternalDataMatchModelComparer dataMatchModelComparer,
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

        public override async Task<bool> GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Generate Data Match Report started", jobIdOverride: reportServiceContext.JobId);
            Task<DataMatchILRInfo> dataMatchILRInfoTask = _ilrProviderService.GetILRInfoForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);
            Task<DataMatchRulebaseInfo> dataMatchRulebaseInfoTask = _fm36ProviderService.GetFM36DataForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            Task<DataMatchDataLockValidationErrorInfo> dataLockValidationErrorInfoTask = _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(reportServiceContext.ReturnPeriod, reportServiceContext.Ukprn, reportServiceContext.CollectionName, cancellationToken);
            Task<DataMatchDasApprenticeshipInfo> dasApprenticeshipPriceInfoTask = _dasPaymentsProviderService.GetDasApprenticeshipInfoForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(dataMatchILRInfoTask, dataMatchRulebaseInfoTask, dataLockValidationErrorInfoTask, dasApprenticeshipPriceInfoTask);

            DataMatchILRInfo dataMatchILRInfo = dataMatchILRInfoTask.Result;
            DataMatchRulebaseInfo dataMatchRulebaseInfo = dataMatchRulebaseInfoTask.Result;
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo = dataLockValidationErrorInfoTask.Result;
            DataMatchDasApprenticeshipInfo dasApprenticeshipPriceInfo = dasApprenticeshipPriceInfoTask.Result;

            _logger.LogInfo($"dataMatchILRInfo (learners with ACT1 and FM36 in ILR) count {dataMatchILRInfo.DataMatchLearners.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dataMatchRulebaseInfo (AEC_ApprenticeshipPriceEpisodes) count {dataMatchRulebaseInfo.AECApprenticeshipPriceEpisodes.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dataLockValidationErrorInfo (DataLockEvents + joins) count {dataLockValidationErrorInfo.DataLockValidationErrors.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dasApprenticeshipPriceInfo (Payments.ApprenticeshipPriceEpisodes) count {dasApprenticeshipPriceInfo.DasApprenticeshipInfos.Count}", jobIdOverride: reportServiceContext.JobId);

            _logger.LogInfo("using the above to build the model...", jobIdOverride: reportServiceContext.JobId);
            List<DataMatchModel> dataMatchModels = _dataMatchModelBuilder.BuildExternalModels(dataMatchILRInfo, dataMatchRulebaseInfo, dataLockValidationErrorInfo, dasApprenticeshipPriceInfo, reportServiceContext.JobId).ToList();
            _logger.LogInfo($"dataMatchModels count (lines to go in the report) {dataMatchModels.Count}", jobIdOverride: reportServiceContext.JobId);
            dataMatchModels.Sort(_dataMatchModelComparer);

            var externalFileName = GetFilename(reportServiceContext);
            var fileName = GetZipFilename(reportServiceContext);

            string csv = WriteResults<ExternalDataMatchMapper, DataMatchModel>(dataMatchModels);

            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
            return true;
        }
    }
}
