using System;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public sealed class DasApprenticeshipInfo
    {
        public long LearnerUln { get; set; }

        public long UkPrn { get; set; }

        public long? StandardCode { get; set; }

        public int? ProgrammeType { get; set; }

        public int? FrameworkCode { get; set; }

        public int? PathwayCode { get; set; }

        public decimal Cost { get; set; }

        public DateTime? PausedOnDate { get; set; }

        public string LegalEntityName { get; set; }

        public DateTime? WithdrawnOnDate { get; set; }
    }
}
