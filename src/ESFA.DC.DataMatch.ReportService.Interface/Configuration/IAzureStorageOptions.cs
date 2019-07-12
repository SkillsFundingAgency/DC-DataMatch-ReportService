using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Interface.Configuration
{
    public interface IAzureStorageOptions
    {
        string AzureBlobConnectionString { get; set; }

        string AzureBlobContainerName { get; set; }
    }
}
