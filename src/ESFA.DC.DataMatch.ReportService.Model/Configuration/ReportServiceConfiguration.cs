using ESFA.DC.DataMatch.ReportService.Core.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Model.Configuration
{
    public class ReportServiceConfiguration : IReportServiceConfiguration
    {
        public string DASPaymentsConnectionString { get; set; }
        
        public string ILR1920DataStoreConnectionString { get; set; }

        public string ILR2021DataStoreConnectionString { get; set; }
    }
}
