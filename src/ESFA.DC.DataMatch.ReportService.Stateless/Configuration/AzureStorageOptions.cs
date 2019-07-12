using ESFA.DC.DataMatch.ReportService.Interface.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Configuration
{
    public class AzureStorageOptions : IAzureStorageOptions
    {
        public string AzureBlobConnectionString { get; set; }

        public string AzureBlobContainerName { get; set; }
    }
}
