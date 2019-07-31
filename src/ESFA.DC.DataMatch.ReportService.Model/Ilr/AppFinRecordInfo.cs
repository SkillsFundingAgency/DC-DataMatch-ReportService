using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class AppFinRecordInfo
    {
        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public string AFinType { get; set; }

        public int AFinCode { get; set; }

        public DateTime AFinDate { get; set; }

        public int AFinAmount { get; set; }

    }
}
