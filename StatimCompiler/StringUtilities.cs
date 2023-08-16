using System;
using System.Collections.Generic;
using System.Text;

namespace StatimCodeGenerator
{
    public static class StringUtilities
    {
        public static string DashToPascalCase(this string text)
        {
            StringBuilder builder = new();

            bool nextUpperCase = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (i == 0)
                {
                    builder.Append(char.ToUpper(c));
                    continue;
                }

                if (c == '-')
                {
                    nextUpperCase = true;
                    continue;
                }

                builder.Append(nextUpperCase ? char.ToUpper(c) : c);
                nextUpperCase = false;
            }

            return builder.ToString();
        }
    }
}
