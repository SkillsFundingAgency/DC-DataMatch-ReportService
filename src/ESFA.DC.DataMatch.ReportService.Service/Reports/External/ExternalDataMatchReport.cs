﻿using System;
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

namespace ESFA.DC.DataMatch.ReportService.Service.Reports.External
{
    public sealed class ExternalDataMatchReport : AbstractReport, IReport
    {
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;
        private readonly IILRProviderService _ilrProviderService;
        private readonly IExternalDataMatchModelBuilder _dataMatchModelBuilder;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ExternalDataMatchModelComparer _dataMatchModelComparer;

        public ExternalDataMatchReport(
            IDASPaymentsProviderService dasPaymentsProviderService,
            IILRProviderService iIlrProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IExternalDataMatchModelBuilder dataMatchModelBuilder,
            IDateTimeProvider dateTimeProvider,
            ExternalDataMatchModelComparer dataMatchModelComparer,
            ILogger logger)
            : base(dateTimeProvider, streamableKeyValuePersistenceService, logger)
        {
            _dasPaymentsProviderService = dasPaymentsProviderService;
            _ilrProviderService = iIlrProviderService;
            _dataMatchModelBuilder = dataMatchModelBuilder;
            _dateTimeProvider = dateTimeProvider;
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
            var dataMatchRulebaseInfoTask = _ilrProviderService.GetFM36DataForDataMatchReportAsync(reportServiceContext.Ukprn, cancellationToken);

            var dataLockValidationErrorInfoTask = _dasPaymentsProviderService.GetDataLockValidationErrorInfoForUkprnAsync(reportServiceContext.ReturnPeriod, reportServiceContext.Ukprn, reportServiceContext.CollectionYear, cancellationToken);
            var dasApprenticeshipPriceInfoTask = _dasPaymentsProviderService.GetDasApprenticeshipInfoForDataMatchReport(reportServiceContext.Ukprn, cancellationToken);

            await Task.WhenAll(dataMatchRulebaseInfoTask, dataLockValidationErrorInfoTask, dasApprenticeshipPriceInfoTask);

            ICollection<AECApprenticeshipPriceEpisodeInfo> priceEpisodes = dataMatchRulebaseInfoTask.Result;
            var dataLockValidationErrorInfo = dataLockValidationErrorInfoTask.Result;
            var dasApprenticeshipPriceInfo = dasApprenticeshipPriceInfoTask.Result;

            List<long> learners = dataLockValidationErrorInfo.Select(x => x.LearnerUln).Distinct().ToList();
            var dataMatchILRInfo = await _ilrProviderService.GetILRInfoForDataMatchReportAsync(reportServiceContext.Ukprn, learners, cancellationToken);

            _logger.LogInfo($"dataMatchILRInfo (learners with ACT1 and FM36 in ILR) count {dataMatchILRInfo.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dataMatchRulebaseInfo (AEC_ApprenticeshipPriceEpisodes) count {priceEpisodes.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dataLockValidationErrorInfo (DataLockEvents + joins) count {dataLockValidationErrorInfo.Count}", jobIdOverride: reportServiceContext.JobId);
            _logger.LogInfo($"dasApprenticeshipPriceInfo (Payments.ApprenticeshipPriceEpisodes) count {dasApprenticeshipPriceInfo.Count}", jobIdOverride: reportServiceContext.JobId);

            _logger.LogInfo("using the above to build the model...", jobIdOverride: reportServiceContext.JobId);
            List<DataMatchModel> dataMatchModels = _dataMatchModelBuilder.BuildExternalModels(dataMatchILRInfo, priceEpisodes, dataLockValidationErrorInfo, dasApprenticeshipPriceInfo, reportServiceContext.JobId).ToList();
            _logger.LogInfo($"dataMatchModels count (lines to go in the report) {dataMatchModels.Count}", jobIdOverride: reportServiceContext.JobId);
            dataMatchModels.Sort(_dataMatchModelComparer);

            var fileName = GetFilename(reportServiceContext);

            var zipFileName = reportServiceContext.IsIlrSubmission
                ? $"{GetZipFilename(reportServiceContext)}.csv"
                : $"{reportServiceContext.Ukprn} {GetZipFilename(reportServiceContext)}.csv";

            string csv = WriteResults<ExternalDataMatchMapper, DataMatchModel>(dataMatchModels);

            if (reportServiceContext.IsIlrSubmission)
            {
                await _streamableKeyValuePersistenceService.SaveAsync(fileName, csv, cancellationToken);
            }

            await WriteZipEntry(archive, zipFileName, csv);
            return true;
        }

        private string GetFilename(IReportServiceContext reportServiceContext)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(reportServiceContext.SubmissionDateTimeUtc);

            return $"{GetFilenamePrefix(reportServiceContext)}_{ReportFileName} {dateTime:yyyyMMdd-HHmmss}.csv";
        }

        private string GetFilenamePrefix(IReportServiceContext reportServiceContext)
        {
            return reportServiceContext.IsIlrSubmission
                ? $"{reportServiceContext.Ukprn}_{reportServiceContext.JobId}"
                : $"{reportServiceContext.Ukprn}_R{reportServiceContext.ReturnPeriod:00}";
        }
    }
}
