using System;

namespace ESFA.DC.DataMatch.ReportService.Model.ReportModels
{
    public sealed class InternalDataMatchModel
    {
        public string Collection { get; set; }

        public int Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public long? AimSeqNumber { get; set; }

        public string RuleName { get; set; }

        public string CollectionPeriodName { get; set; }

        public int CollectionPeriodMonth { get; set; }

        public int CollectionPeriodYear { get; set; }

        public DateTime LastSubmission { get; set; }

        public string Tnp { get; set; }

        public string OfficialSensitive { get; set; }
    }
}
