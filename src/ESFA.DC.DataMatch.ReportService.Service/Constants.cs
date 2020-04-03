using System;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public static class Constants
    {
        public const string ZPROG001 = "ZPROG001";

        public const string ILR = "ILR";

        public const string YEAR_1819 = "1819";

        public const string YEAR_1920 = "1920";

        public const int ApprenticeshipsFundModel = 36;

        public const string LearnDelFAMCode = "1";

        public const string LearnDelFAMType_ACT = "ACT";

        public const string AppFinRecordType_TNP = "TNP";

        public const string PeriodEnd = "DAS_PE";

        public const int SubmissionInMonth = 1;

        public const int SubmissionPeriodEnd = 2;

        public const string DLockErrorRuleNamePrefix = "DLOCK_";

        /// <summary>
        /// Message Key.
        /// </summary>
        public const string ILRPeriods = "ILRPeriods";

        public static DateTime PriceEpisodeStartDateStart1920 = new DateTime(2019, 08, 01);

        public static DateTime PriceEpisodeStartDateEnd1920 = new DateTime(2020, 07, 31);
    }
}
