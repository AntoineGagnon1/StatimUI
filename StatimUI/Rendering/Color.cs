using System;
using System.Collections.Generic;
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
        public static Color FromHex(UInt32 color)
        {
            return FromRGBABytes((byte)((color & 0xFF0000) >> 16), (byte)((color & 0xFF00) >> 8), (byte)(color & 0xFF));
        }

        private static byte FloatToByte(float value)
        {
            return (byte)Math.Max(Math.Min(value * 255, 255), 0);
        }
    }
}
