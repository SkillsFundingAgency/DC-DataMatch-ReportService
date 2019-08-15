using ESFA.DC.DataMatch.ReportService.Interface.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Configuration
{
    public class ReportServiceConfiguration : IReportServiceConfiguration
    {
        public string DASPaymentsConnectionString { get; set; }

        public string ILR1819DataStoreConnectionString { get; set; }

        public string ILR1920DataStoreConnectionString { get; set; }
    }
}
