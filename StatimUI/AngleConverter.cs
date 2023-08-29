using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StatimUI
{
    public class AngleConverter : IStringConverter<Angle>
    {
        private void SkipWhiteSpace(string input, ref int i)
        {
            while (char.IsWhiteSpace(input[i]))
                i++;
        }



        public Angle ToValue(string input)
        {
            var num = new StringBuilder();
            int i = 0;

            SkipWhiteSpace(input, ref i);

            for (; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]) || input[i] == '_' || input[i] == '.')
                    num.Append(input[i]);
                else
                    break;
            }

            SkipWhiteSpace(input, ref i);

            if (input.IndexOf("deg", i) != -1)
                return Angle.FromDegrees(float.Parse(num.ToString(), CultureInfo.InvariantCulture));

            if (input.IndexOf("rad", i) != -1)
                return Angle.FromRadians(float.Parse(num.ToString(), CultureInfo.InvariantCulture));

            if (input.IndexOf("turn", i) != -1)
                return Angle.FromTurns(float.Parse(num.ToString(), CultureInfo.InvariantCulture));

            throw new FormatException(input + " Is invalid. Please use the following format: [number] deg|rad|turn.");
        }
    }
}
