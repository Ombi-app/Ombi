using System;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Ombi.Core.Helpers
{
    public static class EmailValidator
    {
        static bool _invalid;

        public static bool IsValidEmail(string strIn)
        {
            _invalid = false;
            if (string.IsNullOrEmpty(strIn))
                return true;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, "(@)(.+)$", DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (_invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new MailAddress(strIn);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}