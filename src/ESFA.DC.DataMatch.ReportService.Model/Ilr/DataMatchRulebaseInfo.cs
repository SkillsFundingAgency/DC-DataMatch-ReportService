using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class DataMatchRulebaseInfo
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public IEnumerable<AECApprenticeshipPriceEpisodeInfo> AECApprenticeshipPriceEpisodes { get; set; }
    }
}
