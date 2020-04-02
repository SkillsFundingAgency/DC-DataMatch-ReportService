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

        public InternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IILRProviderService iIlrProviderService,
            IInternalDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
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
            _logger.LogInfo("Generate Internal Data Match Report started", jobIdOverride: reportServiceContext.JobId);

            var ilrPeriods = reportServiceContext.ILRPeriods.ToList();

            var dataMatchModels = new List<InternalDataMatchModel>();

            var dataLockValidationErrors = await _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(reportServiceContext.ReturnPeriod, -1, reportServiceContext.CollectionYear, cancellationToken);

            var ukPrns = dataLockValidationErrors.Select(x => (int)x.UkPrn).Distinct().ToList();

            _logger.LogInfo($"Ukprns count {ukPrns.Count}", jobIdOverride: reportServiceContext.JobId);

            foreach (int ukPrn in ukPrns)
            {
                var learners = dataLockValidationErrors.Where(x => x.UkPrn == ukPrn).Select(x => x.LearnerUln).Distinct().ToList();

                _logger.LogInfo($"Processing UKPRN {ukPrn} with {learners.Count} learners");

                var dataMatchILRInfo = await _ilrProviderService.GetILRInfoForDataMatchReport(ukPrn, learners, cancellationToken);

                var reportModels = _dataMatchModelBuilder.BuildInternalModels(dataMatchILRInfo, dataLockValidationErrors, ilrPeriods);

                dataMatchModels.AddRange(reportModels);
            }

            _logger.LogInfo($"Sorting");

            dataMatchModels = dataMatchModels
                .OrderBy(m => m.Collection)
                .ThenBy(m => m.Ukprn)
                .ThenBy(m => m.LearnRefNumber)
                .ToList();

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
