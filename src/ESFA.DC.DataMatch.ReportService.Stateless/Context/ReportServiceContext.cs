using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DataMatch.ReportService.Interface;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model;

namespace ESFA.DC.DataMatch.ReportService.Stateless.Context
{
    public sealed class ReportServiceContext : IReportServiceContext
    {
        private readonly JobContextMessage _jobContextMessage;

        public ReportServiceContext(JobContextMessage jobContextMessage)
        {
            _jobContextMessage = jobContextMessage;
        }

        public int Ukprn => int.Parse(_jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString());

        public string Container => _jobContextMessage.KeyValuePairs[JobContextMessageKey.Container].ToString();

        public IEnumerable<string> Tasks => _jobContextMessage.Topics[_jobContextMessage.TopicPointer].Tasks.SelectMany(x => x.Tasks);

        public int ReturnPeriod => int.Parse(_jobContextMessage.KeyValuePairs["ReturnPeriod"].ToString());

        public long JobId => _jobContextMessage.JobId;

        public DateTime SubmissionDateTimeUtc => _jobContextMessage.SubmissionDateTimeUtc;
    }
}
