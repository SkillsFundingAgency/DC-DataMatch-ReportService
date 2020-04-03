using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ESFA.DC.DataMatch.ReportService.Interface.Service;

namespace ESFA.DC.DataMatch.ReportService.Service.ReferenceData
{
    public class DataLockValidationMessageService : IDataLockValidationMessageService
    {
        private readonly IDictionary<string, string> _validationErrorMessageDictionary = BuildErrorMessageDictionary();
        private readonly IDictionary<int, string> _validationErrorNameDictionary = BuildValidationErrorNameDictionary();

        public string ErrorMessageForRule(string ruleName)
        {
            var match = _validationErrorMessageDictionary.TryGetValue(ruleName, out var errorMessage);

            if (match)
            {
                return errorMessage;
            }

            throw new KeyNotFoundException($"The Requested Rule Id {ruleName} was not found in the Validation Error Lookup.");
        }

        public string RuleNameForRuleId(int ruleId)
        {
            var match = _validationErrorNameDictionary.TryGetValue(ruleId, out var errorMessage);

            if (match)
            {
                return errorMessage;
            }

            throw new KeyNotFoundException($"The Requested Rule Id {ruleId} was not found in the Validation Error Name Lookup.");
        }

        private static IDictionary<int, string> BuildValidationErrorNameDictionary()
        {
            return new Dictionary<int, string>()
            {
                { 1, DataLockValidationErrorIdConstants.DLOCK_01 },
                { 2, DataLockValidationErrorIdConstants.DLOCK_02 },
                { 3, DataLockValidationErrorIdConstants.DLOCK_03 },
                { 4, DataLockValidationErrorIdConstants.DLOCK_04 },
                { 5, DataLockValidationErrorIdConstants.DLOCK_05 },
                { 6, DataLockValidationErrorIdConstants.DLOCK_06 },
                { 7, DataLockValidationErrorIdConstants.DLOCK_07 },
                { 8, DataLockValidationErrorIdConstants.DLOCK_08 },
                { 9, DataLockValidationErrorIdConstants.DLOCK_09 },
                { 10, DataLockValidationErrorIdConstants.DLOCK_10 },
                { 11, DataLockValidationErrorIdConstants.DLOCK_11 },
                { 12, DataLockValidationErrorIdConstants.DLOCK_12 },
            };
        }

        private static IDictionary<string, string> BuildErrorMessageDictionary()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { DataLockValidationErrorIdConstants.DLOCK_01, "No matching record found in an employer digital account for the UKPRN" },
                { DataLockValidationErrorIdConstants.DLOCK_02, "No matching record found in the employer digital account for the ULN" },
                { DataLockValidationErrorIdConstants.DLOCK_03, "No matching record found in the employer digital account for the standard code" },
                { DataLockValidationErrorIdConstants.DLOCK_04, "No matching record found in the employer digital account for the framework code" },
                { DataLockValidationErrorIdConstants.DLOCK_05, "No matching record found in the employer digital account for the programme type" },
                { DataLockValidationErrorIdConstants.DLOCK_06, "No matching record found in the employer digital account for the pathway code" },
                { DataLockValidationErrorIdConstants.DLOCK_07, "No matching record found in the employer digital account for the negotiated cost of training" },
                { DataLockValidationErrorIdConstants.DLOCK_08, "Multiple matching records found in the employer digital account" },
                { DataLockValidationErrorIdConstants.DLOCK_09, "The start date for this negotiated price is before the corresponding price start date in the employer digital account" },
                { DataLockValidationErrorIdConstants.DLOCK_10, "The employer has stopped payments for this apprentice" },
                { DataLockValidationErrorIdConstants.DLOCK_11, "The employer is not currently a levy payer" },
                { DataLockValidationErrorIdConstants.DLOCK_12, "The employer has paused payments for this apprentice" }
            };
        }
    }
}
