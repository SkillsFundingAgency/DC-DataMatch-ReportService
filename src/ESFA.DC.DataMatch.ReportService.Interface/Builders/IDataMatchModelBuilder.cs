using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Model.DASPayments;
using ESFA.DC.ILR1819.DataStore.EF.Valid;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IDataMatchModelBuilder
    {
        List<DataMatchModel> BuildModels(List<Learner> validIlrLearners, List<DasApprenticeshipInfo> dasApprenticeshipInfos, DataMatchRulebaseInfo dataMatchRulebaseInfo);
    }
}