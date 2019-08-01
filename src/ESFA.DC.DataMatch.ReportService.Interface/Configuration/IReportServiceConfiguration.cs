using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Interface.Configuration
{
    public interface IReportServiceConfiguration
    {
        string DASPaymentsConnectionString { get; set; }

        string ILR1819DataStoreConnectionString { get; set; }

        string ILR1920DataStoreConnectionString { get; set; }

        string ILR1819DataStoreValidConnectionString { get; set; }

        string ILR1920DataStoreValidConnectionString { get; set; }
    }
}
