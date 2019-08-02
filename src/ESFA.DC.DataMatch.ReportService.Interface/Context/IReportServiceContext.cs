using System;
using System.Collections.Generic;

namespace ESFA.DC.DataMatch.ReportService.Interface
{
    public interface IReportServiceContext
    {
        long JobId { get; }

        int Ukprn { get; }

        string Container { get; }

        IEnumerable<string> Tasks { get; }

        int ReturnPeriod { get; }

        DateTime SubmissionDateTimeUtc { get; }

        string CollectionName { get; }

        string CollectionYear { get; }
    }
}
