using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.DataMatch.ReportService.Interface.Provider;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.DataMatch.ReportService.Service.Service
{
    public class ReportsProvider : IReportsProvider
    {
        private readonly IList<IReport> _reports;
        private readonly ILogger _logger;

        public ReportsProvider(IList<IReport> reports, ILogger logger)
        {
            _reports = reports;
            _logger = logger;
        }

        public IEnumerable<IReport> ProvideReportsForContext(IReportServiceContext reportServiceContext)
        {
            var missingReportTasks =
                reportServiceContext
                    .Tasks
                    .Where(t => !_reports.Select(r => r.ReportTaskName).Contains(t, StringComparer.OrdinalIgnoreCase));

            foreach (var missingReportTask in missingReportTasks)
            {
                _logger.LogWarning($"Missing Report Task - {missingReportTask}");
            }

            return _reports.Where(r => reportServiceContext.Tasks.Contains(r.ReportTaskName, StringComparer.OrdinalIgnoreCase));
        }
    }
}
