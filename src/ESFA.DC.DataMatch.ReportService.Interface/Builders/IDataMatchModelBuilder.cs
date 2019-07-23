﻿using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Model.DASPayments;
using ESFA.DC.ILR1819.DataStore.EF.Valid;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IDataMatchModelBuilder
    {
        DataMatchModel BuildModel(
            DasApprenticeshipInfo dasApprenticeshipInfo,
            AECApprenticeshipPriceEpisodeInfo dataMatchRulebaseInfo,
            LearningDelivery learningDelivery);
    }
}