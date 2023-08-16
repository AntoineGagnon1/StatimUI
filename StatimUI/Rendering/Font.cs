using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using FreeTypeSharp.Native;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FreeTypeSharp.Native.FT;

namespace StatimUI.Rendering
{
    // TODO : reduce size ?
    internal struct FontGlyph
    {
        public Vector2 UVTopLeft;
        public Vector2 UVSize;

        public int Width;
        public int Height;
        public int BearingX;
        public int BearingY;
        public int Advance;
    }

    public class Font
    {
        public IntPtr Texture { get; private set; }

        private FontGlyph[] Glyphs = new FontGlyph[128];

        private int MaxBearingY;

        // TODO : Destroy the texture in the destructor, but on the main thread

        internal Font(FreeTypeSharp.FreeTypeFaceFacade face)
        {
            // TODO : find a better way to get the max size
            uint maxX = 0;
            uint maxY = 0;
            MaxBearingY = 0;
            for (uint i = 0; i < 128; i++)
            {
                var res = FT_Load_Char(face.Face, i, FT_LOAD_NO_BITMAP);
                if (res != FT_Error.FT_Err_Ok)
                {
                    throw new Exception($"Failed to load the {(char)i} glyph : {res}");
                }

                maxX = (uint)Math.Max(maxX, face.GlyphMetricWidth);
                maxY = (uint)Math.Max(maxY, face.GlyphMetricHeight);
                MaxBearingY = Math.Max(MaxBearingY, face.GlyphBitmapTop);
            }

            Texture = Renderer.Adapter!.MakeTexture();
            var pixelData = new byte[maxX * maxY * 4 * 128];
            for (uint y = 0; y < 8; y++)
            {
                for (uint x = 0; x < 16; x++)
                {
                    var res = FT_Load_Char(face.Face, y * 16 + x, FT_LOAD_RENDER);
                    if (res != FT_Error.FT_Err_Ok)
                    {
                        throw new Exception($"Failed to load the {(char)x * y} glyph : {res}");
                    }

                    int width = (int)face.GlyphBitmap.width;
                    int height = (int)face.GlyphBitmap.rows;
                    Glyphs[y * 16 + x] = new FontGlyph() {
                        Width = width,
                        Height = height,
                        BearingX = face.GlyphBitmapLeft,
                        BearingY = face.GlyphBitmapTop,
                        Advance = face.GlyphMetricHorizontalAdvance,
                        UVTopLeft = new Vector2((x * maxX) / (maxX * 16f), (y * maxY) / (maxY * 8f)),
                        UVSize = new Vector2((1f / 16f) * ((float)width / maxX), (1f / 8f) * ((float)height / maxY))
                    };

                    Size bitmapSize = new Size((int)face.GlyphBitmap.width, (int)face.GlyphBitmap.rows);
                    for (uint glyphY = 0; glyphY < bitmapSize.Height; glyphY++)
                    {
                        for(uint glyphX = 0; glyphX < bitmapSize.Width; glyphX++)
                        {
                            uint offset = ((y * maxX * 16 * maxY) + (glyphY * maxX * 16) + (x * maxX) + glyphX) * 4;
                            pixelData[offset] = 0xFF;
                            pixelData[offset + 1] = 0xFF;
                            pixelData[offset + 2] = 0xFF;
                            unsafe
                            {
                                byte* ptr = (byte*)face.GlyphBitmap.buffer.ToPointer();
                                pixelData[offset + 3] = ptr[glyphY * bitmapSize.Width + glyphX];
                            }
                        }
                    }
                }
            }

            Renderer.Adapter!.SetTextureData(Texture, new Size((int)maxX * 16, (int)maxY * 8), pixelData);
        }

        public RenderCommand MakeText(string text, Color color)
        {
            RenderCommand cmd = new RenderCommand() { Texture = Texture };

            if (text.Length == 0)
                return cmd;

            Vector2 cursor = new Vector2(-Glyphs[GetGlyphIndex(text.First())].BearingX, MaxBearingY); // align the top-left of the text at 0,0
            foreach (char letter in text)
            {
                int glyphIndex = GetGlyphIndex(letter);

                Vector2 topLeft = cursor + new Vector2(Glyphs[glyphIndex].BearingX, -Glyphs[glyphIndex].BearingY);
                Vector2 topRight = topLeft + new Vector2(Glyphs[glyphIndex].Width, 0);
                Vector2 bottomLeft = topLeft + new Vector2(0, Glyphs[glyphIndex].Height);
                Vector2 bottomRight = bottomLeft + new Vector2(Glyphs[glyphIndex].Width, 0);

                Vector2 uvStart = Glyphs[glyphIndex].UVTopLeft;
                Vector2 uvSize = Glyphs[glyphIndex].UVSize;

                cmd.AddTriangle(
                    new(bottomLeft, uvStart + new Vector2(0, uvSize.Y), color),
                    new(topLeft, uvStart, color),
                    new(topRight, uvStart + new Vector2(uvSize.X, 0), color)
                    );
                cmd.AddTriangle(
                    new(topRight, uvStart + new Vector2(uvSize.X, 0), color),
                    new(bottomRight, uvStart + uvSize, color),
                    new(bottomLeft, uvStart + new Vector2(0, uvSize.Y), color)
                    );

                cursor.X += Glyphs[glyphIndex].Advance;
            }

            return cmd;
        }

        private static int GetGlyphIndex(char letter)
        {
            return letter <= 127 ? letter : 0;
        }
    }
}
