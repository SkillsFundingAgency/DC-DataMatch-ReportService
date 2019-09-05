using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Builders;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.DataMatch.ReportService.Service.Abstract;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public sealed class InternalDataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IILRProviderService _ilrProviderService;
        private readonly IDataMatchModelBuilder _dataMatchModelBuilder;

        public InternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IFM36ProviderService fm36ProviderService,
            IILRProviderService iIlrProviderService,
            IDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _fm36ProviderService = fm36ProviderService;
            _ilrProviderService = iIlrProviderService;
            _dataMatchModelBuilder = dataMatchModelBuilder;
        }

        public override string ReportTaskName => ReportTaskNameConstants.InternalDataMatchReport;

        public override string ReportFileName => "Apprenticeship Internal Data Match Report";

        public override async Task<bool> GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Generate Internal Data Match Report started");
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>();

            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo = await _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(-1, -1, reportServiceContext.CollectionName, cancellationToken);
            IEnumerable<int> ukPrns = dataLockValidationErrorInfo.DataLockValidationErrors.Select(x => (int)x.UkPrn).Distinct();

            foreach (int ukPrn in ukPrns)
            {
                Task<DataMatchILRInfo> dataMatchILRInfoTask = _ilrProviderService.GetILRInfoForDataMatchReport(ukPrn, cancellationToken);
                Task<DataMatchRulebaseInfo> dataMatchRulebaseInfoTask = _fm36ProviderService.GetFM36DataForDataMatchReport(ukPrn, cancellationToken);
                Task<DataMatchDasApprenticeshipInfo> dasApprenticeshipPriceInfoTask = _dasPaymentsProviderService.GetDasApprenticeshipInfoForDataMatchReport(ukPrn, cancellationToken);

                await Task.WhenAll(dataMatchILRInfoTask, dataMatchRulebaseInfoTask, dasApprenticeshipPriceInfoTask);

                dataMatchModels.AddRange(_dataMatchModelBuilder.BuildModels(dataMatchILRInfoTask.Result, dataMatchRulebaseInfoTask.Result, dataLockValidationErrorInfo, dasApprenticeshipPriceInfoTask.Result).ToList());
            }

            var externalFileName = GetFilename(reportServiceContext);

            string csv = WriteResults(dataMatchModels);

            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            return false;
        }
    }
}
