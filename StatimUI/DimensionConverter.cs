using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StatimUI
{
    internal class DimensionConverter : IStringConverter<Dimension>
    {
        public Dimension ToValue(string input)
        {// todo: configure float.parse to work with only a . decimal separator
            if (input.Contains(" "))
                input = input.Replace(" ", "");
            if (input == "auto")
                return new Dimension(0f, DimensionUnit.Auto);

            if (input.EndsWith("%"))
                return new Dimension(float.Parse(input.Substring(0, input.Length - 1)) / 100f, DimensionUnit.Decimal);

            if (input.EndsWith("px"))
                return new Dimension(float.Parse(input.Substring(0, input.Length - 2)), DimensionUnit.Pixel);

            return new Dimension(float.Parse(input), DimensionUnit.Pixel);
        }
    }
}
