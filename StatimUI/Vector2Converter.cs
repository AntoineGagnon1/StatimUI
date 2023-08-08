using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    internal class Vector2Converter : IStringConverter<Vector2>
    {
        public Vector2 ToValue(string input)
        {
            if (input.Contains(" "))
                input = input.Replace(" ", "");

            var nums = input.Split(',');
            if (nums.Length != 2)
                throw new FormatException("A padding or a margin declartion must either follow this syntax: \"n, n\"");

            return new Vector2(float.Parse(nums[0]), float.Parse(nums[1]));
        }
    }
}
