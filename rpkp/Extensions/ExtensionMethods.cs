using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpkp.Extensions
{
    public static class ExtensionMethods
    {
        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (string.Compare(c.ToString(), "ł") == 0)
                {
                    stringBuilder.Append("l");
                }
                else if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ReplaceAWordOnSemicolonBasedOnParameters(this string text, string[] parameters)
        {
            foreach (string word in parameters)
            {
                text = text.Replace(word, ";");
            }

            return text;
        }
    }
}
