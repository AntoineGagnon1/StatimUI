using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    public struct Color
    {
        // Packed color data, format 0xAABBGGRR
        private UInt32 Data;

        public static Color FromRGBA(float r, float g, float b, float a = 1.0f)
        {
            return FromRGBABytes(FloatToByte(r), FloatToByte(g), FloatToByte(b), FloatToByte(a));
        }

        public static Color FromRGBABytes(byte r, byte g, byte b, byte a = 255)
        {
            return new Color() { Data = (uint)((a << 24) | (b << 16) | (g << 8) | r) };
        }

        // Format : 0xRRGGBB
        public static Color FromHex(UInt32 color, byte opacity = 255)
        {
            return FromRGBABytes((byte)((color & 0xFF0000) >> 16), (byte)((color & 0xFF00) >> 8), (byte)(color & 0xFF), opacity);
        }

        private static byte FloatToByte(float value)
        {
            return (byte)Math.Max(Math.Min(value * 255, 255), 0);
        }

        public static readonly Color White = Color.FromHex(0xFFFFFF);
        public static readonly Color Black = Color.FromHex(0x000000);

        public static readonly Color Red = Color.FromHex(0xFF0000);
        public static readonly Color Green = Color.FromHex(0x00FF00);
        public static readonly Color Blue = Color.FromHex(0x0000FF);
    }

    internal class ColorConverter : IStringConverter<Color>
    {
        public Color ToValue(string input)
        {
            try
            {
                return Color.FromHex(Convert.ToUInt32(input, 16));
            }
            catch(Exception e)
            {
                throw new FormatException($"Invalid Color format : {input}, format must be 0xRRGGBB. Error : {e.Message}");
            }
        }
    }
}
