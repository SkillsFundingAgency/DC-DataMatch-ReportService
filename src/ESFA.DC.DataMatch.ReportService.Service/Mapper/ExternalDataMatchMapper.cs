using CsvHelper.Configuration;
using ESFA.DC.DataMatch.ReportService.Model.ReportModels;

namespace ESFA.DC.DataMatch.ReportService.Service.Mapper
{
    public sealed class ExternalDataMatchMapper : ClassMap<DataMatchModel>
    {
        public ExternalDataMatchMapper()
        {
            Map(m => m.LearnRefNumber).Index(0).Name("Learner reference number");
            Map(m => m.Uln).Index(1).Name("Unique learner number");
            Map(m => m.AimSeqNumber).Index(2).Name("Aim sequence number");
            Map(m => m.RuleName).Index(3).Name("Rule name");
            Map(m => m.Description).Index(4).Name("Description");
            Map(m => m.ILRValue).Index(5).Name("ILR value");
            Map(m => m.ApprenticeshipServiceValue).Index(6).Name("Apprenticeship service value");
            Map(m => m.PriceEpisodeStartDate).Index(7).Name("Price episode start date");
            Map(m => m.PriceEpisodeActualEndDate).Index(8).Name("Price episode actual end date");
            Map(m => m.LegalEntityName).Index(9).Name("Employer name from apprenticeship service");
            Map(m => m.OfficialSensitive).Index(10).Name("OFFICIAL - SENSITIVE");
        }
    }
}
