using System;

namespace ESFA.DC.DataMatch.ReportService.Service.Extensions
{
    public static class StringExtensions
    {
        public static bool CaseInsensitiveEquals(this string source, string data)
        {
            if (source == null && data == null)
            {
                return true;
            }

            return source?.Equals(data, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}
