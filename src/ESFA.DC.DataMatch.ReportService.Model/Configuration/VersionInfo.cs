using ESFA.DC.DataMatch.ReportService.Interface.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Configuration
{
    public sealed class VersionInfo : IVersionInfo
    {
        public string ServiceReleaseVersion { get; set; }
    }
}
