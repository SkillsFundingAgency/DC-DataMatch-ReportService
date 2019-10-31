namespace ESFA.DC.DataMatch.ReportService.Core.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string ILR1819DataStoreConnectionString { get; set; }

        string ILR1920DataStoreConnectionString { get; set; }
    }
}
