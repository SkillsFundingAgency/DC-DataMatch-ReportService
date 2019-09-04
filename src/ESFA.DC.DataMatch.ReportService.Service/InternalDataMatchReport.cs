using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.DataMatch.ReportService.Interface.Service;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Service.Abstract;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public sealed class InternalDataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;

        public InternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IDateTimeProvider dateTimeProvider,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
        }

        public override string ReportTaskName => ReportTaskNameConstants.InternalDataMatchReport;

        public override string ReportFileName => "Apprenticeship Internal Data Match Report";

        public override async Task GenerateReport(
            IReportServiceContext reportServiceContext,
            ZipArchive archive,
            CancellationToken cancellationToken)
        {
            _logger.LogInfo("Generate Internal Data Match Report started");
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo = await _dasPaymentsProviderService.GetDataLockValidationErrorInfoForDataMatchReport(-1, -1, reportServiceContext.CollectionName, cancellationToken);
            IEnumerable<long> ukPrns = dataLockValidationErrorInfo.DataLockValidationErrors.Select(x => x.UkPrn).Distinct();

            foreach (long ukPrn in ukPrns)
            {
            }
        }
    }
}
