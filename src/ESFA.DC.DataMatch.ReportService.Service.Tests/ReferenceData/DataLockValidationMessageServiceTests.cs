using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.DataMatch.ReportService.Service.ReferenceData;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.DataMatch.ReportService.Service.Tests.ReferenceData
{
    public class DataLockValidationMessageServiceTests
    {
        [Fact]
        public void ErrorMessage_KeyNotFound()
        {
            Action action = () => NewService().ErrorMessageForRule("Not Real");

            action.Should().Throw<KeyNotFoundException>().WithMessage("The Requested Rule Id Not Real was not found in the Validation Error Lookup.");
        }

        [Theory]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_01, "No matching record found in an employer digital account for the UKPRN")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_02, "No matching record found in the employer digital account for the ULN")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_03, "No matching record found in the employer digital account for the standard code")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_04, "No matching record found in the employer digital account for the framework code")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_05, "No matching record found in the employer digital account for the programme type")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_06, "No matching record found in the employer digital account for the pathway code")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_07, "No matching record found in the employer digital account for the negotiated cost of training")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_08, "Multiple matching records found in the employer digital account")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_09, "The start date for this negotiated price is before the corresponding price start date in the employer digital account")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_10, "The employer has stopped payments for this apprentice")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_11, "The employer is not currently a levy payer")]
        [InlineData(DataLockValidationErrorIdConstants.DLOCK_12, "The employer has paused payments for this apprentice")]
        public void ErrorMessageLookups(string ruleId, string errorMessage)
        {
            NewService().ErrorMessageForRule(ruleId).Should().Be(errorMessage);
        }

        [Fact]
        public void RuleName_KeyNotFound()
        {
            Action action = () => NewService().RuleNameForRuleId(0);

            action.Should().Throw<KeyNotFoundException>().WithMessage("The Requested Rule Id 0 was not found in the Validation Error Name Lookup.");
        }

        [Theory]
        [InlineData(1, DataLockValidationErrorIdConstants.DLOCK_01)]
        [InlineData(2, DataLockValidationErrorIdConstants.DLOCK_02)]
        [InlineData(3, DataLockValidationErrorIdConstants.DLOCK_03)]
        [InlineData(4, DataLockValidationErrorIdConstants.DLOCK_04)]
        [InlineData(5, DataLockValidationErrorIdConstants.DLOCK_05)]
        [InlineData(6, DataLockValidationErrorIdConstants.DLOCK_06)]
        [InlineData(7, DataLockValidationErrorIdConstants.DLOCK_07)]
        [InlineData(8, DataLockValidationErrorIdConstants.DLOCK_08)]
        [InlineData(9, DataLockValidationErrorIdConstants.DLOCK_09)]
        [InlineData(10, DataLockValidationErrorIdConstants.DLOCK_10)]
        [InlineData(11, DataLockValidationErrorIdConstants.DLOCK_11)]
        [InlineData(12, DataLockValidationErrorIdConstants.DLOCK_12)]
        public void RuleNameLookups(int ruleId, string ruleName)
        {
            NewService().RuleNameForRuleId(ruleId).Should().Be(ruleName);
        }

        private DataLockValidationMessageService NewService()
        {
            return new DataLockValidationMessageService();
        }
    }
}
