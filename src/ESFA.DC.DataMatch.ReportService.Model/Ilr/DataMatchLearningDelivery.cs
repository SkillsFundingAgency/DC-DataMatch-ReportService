using System;
using System.Collections.Generic;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class DataMatchLearningDelivery
    {
        public string LearnAimRef { get; set; }

        public int AimSeqNumber { get; set; }

        public DateTime LearnStartDate { get; set; }

        public int? ProgType { get; set; }

        public int? FworkCode { get; set; }

        public int? PwayCode { get; set; }

        public int? StdCode { get; set; }

        public IEnumerable<DataMatchLearningDeliveryFAM> DataMatchLearningDeliveryFams { get; set; }

        public ICollection<AppFinRecordInfo> AppFinRecords { get; set; }
    }
}
