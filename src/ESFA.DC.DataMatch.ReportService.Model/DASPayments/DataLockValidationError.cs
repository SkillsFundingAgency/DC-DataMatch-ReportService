using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public class DataLockValidationError
    {
        public string LearnerReferenceNumber { get; set; }
        public long? AimSeqNumber { get; set; }
        public int RuleId { get; set; }

        public long PriceEpisodeMatchAppId { get; set; }

        public long LearnerUln { get; set; }

        public long UkPrn { get; set; }
    }
}
