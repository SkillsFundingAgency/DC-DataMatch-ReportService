using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string ILRDataStoreConnectionString { get; set; }

        string ILRDataStoreValidConnectionString { get; set; }
    }
}
