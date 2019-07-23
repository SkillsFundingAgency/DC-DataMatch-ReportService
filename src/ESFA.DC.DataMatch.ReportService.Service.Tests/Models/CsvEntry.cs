using CsvHelper.Configuration;

namespace ESFA.DC.DataMatch.ReportService.Tests.Models
{
    public sealed class CsvEntry
    {
        public CsvEntry(ClassMap mapper, int dataRows, string title = "", int blankRowsBefore = 0)
        {
            Mapper = mapper;
            DataRows = dataRows;
            Title = title;
            BlankRowsBefore = blankRowsBefore;
        }

        public ClassMap Mapper { get; }

        public int DataRows { get; }

        public string Title { get; }

        public int BlankRowsBefore { get; }
    }
}
