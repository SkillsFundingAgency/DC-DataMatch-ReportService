using ESFA.DC.DataMatch.ReportService.Interface.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Configuration
{
    public class ReportServiceConfiguration : IReportServiceConfiguration
    {
        public string DASPaymentsConnectionString { get; set; }

        public string ILRDataStoreConnectionString { get; set; }

        public string ILRDataStoreValidConnectionString { get; set; }
    }
}
