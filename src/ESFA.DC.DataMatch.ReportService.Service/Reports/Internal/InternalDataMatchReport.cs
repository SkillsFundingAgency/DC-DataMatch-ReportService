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
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.Internal
{
    public sealed class InternalDataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IILRProviderService _ilrProviderService;
        private readonly IInternalDataMatchModelBuilder _dataMatchModelBuilder;
        private readonly InternalDataMatchModelComparer _dataMatchModelComparer;

        public InternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IILRProviderService iIlrProviderService,
            IInternalDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            InternalDataMatchModelComparer dataMatchModelComparer,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _ilrProviderService = iIlrProviderService;
            _dataMatchModelBuilder = dataMatchModelBuilder;
            _dataMatchModelComparer = dataMatchModelComparer;
        }

        public override string ReportTaskName => ReportTaskNameConstants.InternalDataMatchReport;

        public override string ReportFileName => "Apprenticeship Internal Data Match Report";

        public override async Task<bool> GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Generate Internal Data Match Report started", jobIdOverride: reportServiceContext.JobId);
            List<InternalDataMatchModel> dataMatchModels = new List<InternalDataMatchModel>();

            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo = await _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(reportServiceContext.ReturnPeriod, -1, reportServiceContext.CollectionYear, cancellationToken);
            List<int> ukPrns = dataLockValidationErrorInfo.DataLockValidationErrors.Select(x => (int)x.UkPrn).Distinct().ToList();

            _logger.LogInfo($"Ukprns count {ukPrns.Count}", jobIdOverride: reportServiceContext.JobId);

            foreach (int ukPrn in ukPrns)
            {
                List<long> learners = dataLockValidationErrorInfo.DataLockValidationErrors.Where(x => x.UkPrn == ukPrn).Select(x => x.LearnerUln).Distinct().ToList();
                _logger.LogInfo($"Processing UKPRN {ukPrn} with {learners.Count} learners");
                DataMatchILRInfo dataMatchILRInfo = await _ilrProviderService.GetILRInfoForDataMatchReport(ukPrn, learners, cancellationToken);
                dataMatchModels.AddRange(_dataMatchModelBuilder.BuildInternalModels(dataMatchILRInfo, dataLockValidationErrorInfo, reportServiceContext.ILRPeriods.ToList(), reportServiceContext.JobId).ToList());
            }

            _logger.LogInfo($"Sorting");
            dataMatchModels.Sort(_dataMatchModelComparer);

            string externalFileName = GetFilename(reportServiceContext);

            _logger.LogInfo($"Generating CSV");
            string csv = WriteResults<InternalDataMatchMapper, InternalDataMatchModel>(dataMatchModels);

            _logger.LogInfo($"Persisting");
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            return false;
        }

        private string GetFilename(IReportServiceContext reportServiceContext)
        {
            return $"R{reportServiceContext.ReturnPeriod:00}_{ReportFileName}";
        }
    }
}
