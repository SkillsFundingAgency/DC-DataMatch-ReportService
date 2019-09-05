using System;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public sealed class DataLockValidationError
    {
        public string LearnerReferenceNumber { get; set; }

        public long? AimSeqNumber { get; set; }

        public int RuleId { get; set; }

        public long PriceEpisodeMatchAppId { get; set; }

        public long LearnerUln { get; set; }

        public long UkPrn { get; set; }

        public string Collection { get; set; }

        public byte CollectionPeriod { get; set; }

        public DateTime LastSubmission { get; set; }
    }
}
