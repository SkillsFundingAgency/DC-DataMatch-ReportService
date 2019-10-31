using ESFA.DC.DataMatch.ReportService.Core.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Model.Configuration
{
    public class AzureStorageOptions : IAzureStorageOptions
    {
        public string AzureBlobConnectionString { get; set; }

        public string AzureBlobContainerName { get; set; }
    }
}
