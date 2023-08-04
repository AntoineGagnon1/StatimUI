using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI
{
    public class ThicknessConverter : IStringConverter<Thickness>
    {
        public Thickness ToValue(string input)
        {
            var nums = input.Replace(" ", "").Split(',');

            if (nums.Length == 1)
                return new Thickness(float.Parse(nums[0]));

            if (nums.Length == 4)
            {
                return new Thickness(
                    float.Parse(nums[0]),
                    float.Parse(nums[1]),
                    float.Parse(nums[2]),
                    float.Parse(nums[3])
                );
            }
                
            throw new FormatException("A padding or a margin declartion must either follow this syntax: \"n\" or this one: \"n, n, n, n\"");
        }
    }
}
