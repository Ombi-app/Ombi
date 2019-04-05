using System.Text.RegularExpressions;

namespace Ombi.Helpers
{
    public static class HtmlHelper
    {
        public static string RemoveHtml(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }
    }
}