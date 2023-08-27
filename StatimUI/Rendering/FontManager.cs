using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

namespace StatimUI.Rendering
{
    public static class FontManager
    {
        struct FontInfo
        {
            public string Path;
            public uint SizeX;
            public uint SizeY;

            public FontInfo(string path, uint sizeX, uint sizeY)
            {
                Path = path;
                SizeX = sizeX;
                SizeY = sizeY;
            }
        }

        private static Dictionary<FontInfo, Font> s_fonts = new();

        private static FreeTypeSharp.FreeTypeLibrary s_freeType = new ();

        public static Font DefaultFont { get; set; } = FontManager.GetFont("arial.ttf", 14);

        // Will load the font if needed
        // if fontSizeX is 0 it will be determined from the y size
        public static Font GetFont(string path, uint fontSizeY, uint fontSizeX = 0)
        {
            var info = new FontInfo(path, fontSizeX, fontSizeY);
            if (s_fonts.TryGetValue(info, out Font found))
            {
                return found;
            }

            var res = FT_New_Face(s_freeType.Native, path, 0, out var face);
            if(res == FT_Error.FT_Err_Ok)
                res = FT_Set_Pixel_Sizes(face, fontSizeX, fontSizeY);

            if (res != FT_Error.FT_Err_Ok)
            {
                throw new Exception($"Failed to load font at {path} : {res}");
            }

            var font = new Font(new FreeTypeSharp.FreeTypeFaceFacade(s_freeType, face));
            FT_Done_Face(face);

            s_fonts.Add(info, font);
            return font;
        }

        // Is this texture for a font ?
        public static bool IsTextureFont(IntPtr texture)
        {
            return s_fonts.Any(kvp => kvp.Value.Texture == texture);
        }
    }
}
