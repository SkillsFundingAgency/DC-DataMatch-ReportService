using System;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public sealed class RawEarning
    {
        public string PriceEpisodeIdentifier { get; set; }

        public long Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public int AimSeqNumber { get; set; }

        public DateTime? EpisodeStartDate { get; set; }

        public DateTime? EpisodeEffectiveTnpStartDate { get; set; }

        public int Period { get; set; }

        public int? ProgrammeType { get; set; }

        public int? FrameworkCode { get; set; }

        public int? PathwayCode { get; set; }

        public int? StandardCode { get; set; }

        public decimal? AgreedPrice { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime LearningDeliveryStartDate { get; set; }

        public DateTime? FirstIncentiveCensusDate { get; set; }

        public DateTime? SecondIncentiveCensusDate { get; set; }

        public DateTime? LearnerAdditionalPaymentsDate { get; set; }
    }
}
