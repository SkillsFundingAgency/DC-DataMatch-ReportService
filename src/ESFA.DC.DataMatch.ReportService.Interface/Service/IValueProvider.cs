using System.Collections.Generic;
using CsvHelper.Configuration;
using ESFA.DC.DataMatch.ReportService.Model.Generation;

namespace ESFA.DC.DataMatch.ReportService.Interface.Service
{
    public interface IValueProvider
    {
        void GetFormattedValue(List<object> values, object value, ClassMap mapper, ModelProperty modelProperty);
    }
}
