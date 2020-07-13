namespace ESFA.DC.DataMatch.ReportService.Core.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }
        
        string ILR1920DataStoreConnectionString { get; set; }

        string ILR2021DataStoreConnectionString { get; set; }
    }
}
