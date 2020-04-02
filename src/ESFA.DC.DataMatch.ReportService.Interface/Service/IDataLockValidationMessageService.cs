using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IDataLockValidationMessageService
    {
        string ErrorMessageForRule(string ruleName);

        string RuleNameForRuleId(int ruleId);
    }
}
