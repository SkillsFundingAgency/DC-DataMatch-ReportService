using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class DataMatchILRInfo
    {
        public int UkPrn { get; set; }

        public List<DataMatchLearner> DataMatchLearners { get; set; }
    }
}
