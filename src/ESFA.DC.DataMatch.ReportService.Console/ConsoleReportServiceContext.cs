using System;
using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Interface.Context;

namespace ESFA.DC.DataMatch.ReportService.Console
{
    public sealed class ConsoleReportServiceContext : IReportServiceContext
    {
        public long JobId { get; set; }

        public int Ukprn { get; set; }

        public string Container { get; set; }

        public IEnumerable<string> Tasks { get; set; }

        public int ReturnPeriod { get; set; }

        public DateTime SubmissionDateTimeUtc { get; set; }

        public string CollectionName { get; set; }

        public string CollectionYear { get; set; }

        public IEnumerable<ReturnPeriod> ILRPeriods { get; set; }

        public bool IsIlrSubmission { get; set; }
    }
}
