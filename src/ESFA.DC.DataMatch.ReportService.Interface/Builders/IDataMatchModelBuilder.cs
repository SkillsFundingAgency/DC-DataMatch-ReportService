﻿using System.Collections.Generic;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IDataMatchModelBuilder
    {
        IEnumerable<DataMatchModel> BuildExternalModels(
            DataMatchILRInfo dataMatchILRInfo,
            DataMatchRulebaseInfo dataMatchRulebaseInfo,
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo,
            DataMatchDasApprenticeshipInfo dasApprenticeshipPriceInfo,
            long jobId);

        IEnumerable<InternalDataMatchModel> BuildInternalModels(
            DataMatchILRInfo dataMatchILRInfo,
            DataMatchDataLockValidationErrorInfo dataLockValidationErrorInfo,
            List<ReturnPeriod> returnPeriods,
            long jobId);
    }
}