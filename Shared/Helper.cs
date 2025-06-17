using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class StringOperations
    {
        public static string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return string.Concat(Path.GetFileNameWithoutExtension(fileName)
                                , "_"
                                , Guid.NewGuid().ToString().AsSpan(0, 4)
                                , Path.GetExtension(fileName));
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0], new CultureInfo("tr-TR", false)) + str.Substring(1);

            return str.ToUpper();
        }

        public static string ToTitleCase(string str)
        {
            if (str == null)
                return null;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower().Trim());
        }
    }

    public static class TextReplace
    {
        public static string StringReplace(string text)
        {
            if (text == null || text == "")
                return text;
            text = text
                .Replace("&#252;", "ü")
                .Replace("&Uuml;", "Ü")
                .Replace("&Ccedil;", "Ç")
                .Replace("&ccedil;", "ç")
                .Replace("&#xC7;", "Ç")
                .Replace("&uuml;", "ü")
                .Replace("&ouml;", "ö")
                .Replace("&#246;", "ö")
                .Replace("&#231;", "ç")
                .Replace("&#220;", "Ü")
                .Replace("&#199;", "Ç")
                .Replace("&#214;", "Ö")
                .Replace("&#351;", "ş")
                .Replace("&#350;", "Ş")
                .Replace("&#304;", "İ")
                .Replace("&#305;", "i")
                .Replace("&#287;", "ğ")
                .Replace("&#286;", "Ğ")
                .Replace("&amp;", "&")
                .Replace("&#xF6;", "ö")
                .Replace("&#x15F;", "ş")
                .Replace("&#x15E;", "Ş")
                .Replace("&#x11F;", "ğ")
                .Replace("&#x130;", "İ")
                .Replace("&#xD6;", "Ö")
                .Replace("&#xFC;", "ü")
                .Replace("&#x131;", "ı");
            return text;
        }
    }
}