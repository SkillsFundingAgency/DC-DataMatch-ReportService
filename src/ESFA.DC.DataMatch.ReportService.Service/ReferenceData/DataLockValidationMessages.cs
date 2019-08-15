using System.Collections.Generic;

namespace ESFA.DC.DataMatch.ReportService.Service.ReferenceData
{
    public static class DataLockValidationMessages
    {
        public const string DLOCK_01 = "DLOCK_01";

        public const string DLOCK_02 = "DLOCK_02";

        public const string DLOCK_03 = "DLOCK_03";

        public const string DLOCK_04 = "DLOCK_04";

        public const string DLOCK_05 = "DLOCK_05";

        public const string DLOCK_06 = "DLOCK_06";

        public const string DLOCK_07 = "DLOCK_07";

        public const string DLOCK_08 = "DLOCK_08";

        public const string DLOCK_09 = "DLOCK_09";

        public const string DLOCK_10 = "DLOCK_10";

        public const string DLOCK_11 = "DLOCK_11";

        public const string DLOCK_12 = "DLOCK_12";

        public static List<DataLockValidationMessage> Validations { get; } = new List<DataLockValidationMessage>
        {
            new DataLockValidationMessage(
                DLOCK_01,
                'E',
                "No matching record found in an employer digital account for the UKPRN"),
            new DataLockValidationMessage(
                DLOCK_02,
                'E',
                "No matching record found in the employer digital account for the ULN"),
            new DataLockValidationMessage(
                DLOCK_03,
                'E',
                "No matching record found in the employer digital account for the standard code"),
            new DataLockValidationMessage(
                DLOCK_04,
                'E',
                "No matching record found in the employer digital account for the framework code"),
            new DataLockValidationMessage(
                DLOCK_05,
                'E',
                "No matching record found in the employer digital account for the programme type"),
            new DataLockValidationMessage(
                DLOCK_06,
                'E',
                "No matching record found in the employer digital account for the pathway code"),
            new DataLockValidationMessage(
                DLOCK_07,
                'E',
                "No matching record found in the employer digital account for the negotiated cost of training"),
            new DataLockValidationMessage(
                DLOCK_08,
                'E',
                "Multiple matching records found in the employer digital account"),
            new DataLockValidationMessage(
                DLOCK_09,
                'E',
                "The start date for this negotiated price is before the corresponding price start date in the employer digital account"),
            new DataLockValidationMessage(
                DLOCK_10,
                'E',
                "The employer has stopped payments for this apprentice"),
            new DataLockValidationMessage(
                DLOCK_11,
                'E',
                "The employer is not currently a levy payer"),
            new DataLockValidationMessage(
                DLOCK_12,
                'W',
                "The employer has paused payments for this apprentice")
        };
    }
}
