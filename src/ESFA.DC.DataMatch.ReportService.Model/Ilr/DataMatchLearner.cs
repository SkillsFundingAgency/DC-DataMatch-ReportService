using System.Collections.Generic;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public sealed class DataMatchLearner
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public IEnumerable<DataMatchLearningDelivery> DataMatchLearningDeliveries { get; set; }

        public long Uln { get; set; }
    }
}
