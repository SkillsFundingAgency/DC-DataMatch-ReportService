using System.Collections.Generic;
using ESFA.DC.DataMatch.ReportService.Model.DASPayments;
using ESFA.DC.DataMatch.ReportService.Model.Ilr;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;
using ESFA.DC.ILR1819.DataStore.EF.Valid;

namespace ESFA.DC.DataMatch.ReportService.Interface.Builders
{
    public interface IDataMatchModelBuilder
    {
        IEnumerable<DataMatchModel> BuildModels(IEnumerable<Learner> validIlrLearners, IEnumerable<DasApprenticeshipInfo> dasApprenticeshipInfos, DataMatchRulebaseInfo dataMatchRulebaseInfo);
    }
}