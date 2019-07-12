using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Interface.Reports;

namespace ESFA.DC.DataMatch.ReportService.Interface.Provider
{
    public interface IReportsProvider
    {
        IEnumerable<IReport> ProvideReportsForContext(IReportServiceContext reportServiceContext);
    }
}
