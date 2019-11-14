using CsvHelper.Configuration;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Mapper
{
    public sealed class InternalDataMatchMapper : ClassMap<InternalDataMatchModel>
    {
        public InternalDataMatchMapper()
        {
            int i = 0;
            Map(m => m.Collection).Index(i++).Name("Collection");
            Map(m => m.Ukprn).Index(i++).Name("UKPRN");
            Map(m => m.LearnRefNumber).Index(i++).Name("LearnRefNumber");
            Map(m => m.Uln).Index(i++).Name("ULN");
            Map(m => m.AimSeqNumber).Index(i++).Name("AimSeqNumber");
            Map(m => m.RuleName).Index(i++).Name("RuleId");
            Map(m => m.CollectionPeriodName).Index(i++).Name("CollectionPeriodName");
            Map(m => m.CollectionPeriodMonth).Index(i++).Name("CollectionPeriodMonth");
            Map(m => m.CollectionPeriodYear).Index(i++).Name("CollectionPeriodYear");
            Map(m => m.LastSubmission).Index(i++).Name("LastSubmission");
            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}
