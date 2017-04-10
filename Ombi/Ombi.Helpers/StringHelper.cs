using System.Globalization;

namespace Ombi.Helpers
{
    public static class StringHelper
    {
        public static bool Contains(this string paragraph, string word, CompareOptions opts)
        {
            return CultureInfo.CurrentUICulture.CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }
    }
}