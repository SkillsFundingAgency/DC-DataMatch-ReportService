using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ESFA.DC.DataMatch.ReportService.Model.ReportModels
{
    public sealed class DataMatchModel
    {
        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public long? AimSeqNumber { get; set; }

        public string RuleName { get; set; }

        public string Description { get; set; }

        public string ILRValue { get; set; }

        public string ApprenticeshipServiceValue { get; set; }

        public DateTime? PriceEpisodeStartDate { get; set; }

        public DateTime? PriceEpisodeActualEndDate { get; set; }

        public string OfficialSensitive { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string LegalEntityName { get; set; }

        public DataMatchErrorValues DataMatchErrorValues { get; set; }
    }
}
