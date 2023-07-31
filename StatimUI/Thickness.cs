using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    /// <summary>
    /// Describes the thickness of a frame around or inside a rectangle in pixels.
    /// </summary>
    public struct Thickness
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness(float all)
        {
            Left = all;
            Top = all;
            Right = all;
            Bottom = all;
        }
    }
}
