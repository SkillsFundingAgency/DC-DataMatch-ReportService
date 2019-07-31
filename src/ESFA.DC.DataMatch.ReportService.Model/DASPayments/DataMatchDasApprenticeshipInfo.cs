using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public class DataMatchDasApprenticeshipInfo
    {
        public int UkPrn { get; set; }

        public List<DasApprenticeshipPriceInfo> DasApprenticeshipPriceInfos { get; set; }
    }
}
