using ESFA.DC.DataMatch.ReportService.Stateless.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Model.Configuration
{
    public sealed class ConfigurationRootModel
    {
        public ReportServiceConfiguration reportServiceConfiguration;

        public AzureStorageOptions azureBlobStorageOptions;

        public VersionInfo versionInfo;
    }
}
