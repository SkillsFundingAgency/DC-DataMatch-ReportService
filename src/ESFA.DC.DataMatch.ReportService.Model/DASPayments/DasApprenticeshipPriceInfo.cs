using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public class DasApprenticeshipPriceInfo
    {
        public long LearnerUln { get; set; }

        public decimal Cost { get; set; }

        public DateTime? PausedOnDate { get; set; }

        public DateTime? WithdrawnOnDate { get; set; }
        public string LegalEntityName { get; set; }

        public int StandardCode { get; set; }

        public int FrameworkCode { get; set; }

        public int ProgrammeType { get; set; }

        public int PathwayCode { get; set; }
    }
}
