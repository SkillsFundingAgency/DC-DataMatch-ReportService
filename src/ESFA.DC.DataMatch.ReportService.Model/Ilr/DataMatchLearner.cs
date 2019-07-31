using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class DataMatchLearner
    {
        public int UkPrn { get; set; }
        public string LearnRefNumber { get; set; }

        public IEnumerable<DataMatchLearningDelivery> DataMatchLearningDeliveries { get; set; }
        public long Uln { get; set; }
    }
}
