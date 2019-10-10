using System;
using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.DataMatch.ReportService.Interface.Context
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

        IEnumerable<ReturnPeriod> ILRPeriods { get; }

        bool IsIlrSubmission { get; }
    }
}
