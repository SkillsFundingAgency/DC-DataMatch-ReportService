using System;

namespace ESFA.DC.DataMatch.ReportService.Model.Ilr
{
    public class AECApprenticeshipPriceEpisodeInfo
    {
        public int UkPrn { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSequenceNumber { get; set; }

        public DateTime? EpisodeStartDate { get; set; }

        public DateTime? PriceEpisodeActualEndDate { get; set; }

        public string PriceEpisodeAgreeId { get; set; }

        public DateTime? EffectiveTnpStartDate { get; set; }
        
    }
}