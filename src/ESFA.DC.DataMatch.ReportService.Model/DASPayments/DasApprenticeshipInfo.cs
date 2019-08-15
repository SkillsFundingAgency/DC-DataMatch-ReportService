using System;

namespace ESFA.DC.DataMatch.ReportService.Model.DASPayments
{
    public sealed class DasApprenticeshipInfo
    {
        public long ApprenticeshipId { get; set; }
        public string AgreementId { get; set; }
        public DateTime AgreedOnDate { get; set; }
        public long Uln { get; set; }
        public long UkPrn { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public long? StandardCode { get; set; }
        public int? ProgrammeType { get; set; }
        public int? FrameworkCode { get; set; }
        public int? PathwayCode { get; set; }
        public DateTime? StopDate { get; set; }
        public string LearnerReferenceNumber { get; set; }
        public long? AimSequenceNumber { get; set; }
        public int RuleId { get; set; }
        public decimal Cost { get; set; }
        public DateTime PauseDate { get; set; }
        public string LegalEntityName { get; set; }
        public string AppreticeshipServiceValue { get; set; }

        public DateTime? EffectiveFromDate { get; set; }
        public int PaymentStatus { get; set; }
    }
}
