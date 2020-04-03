using System.Collections.Generic;

namespace ESFA.DC.DataMatch.ReportService.Service.Extensions
{
    public static class IDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null || key == null || !dictionary.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}
