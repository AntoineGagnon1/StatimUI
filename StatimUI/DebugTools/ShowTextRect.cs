using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI.DebugTools
{
#if DEBUG
    public class ShowTextRect
    {
        public static void Render()
        {
            foreach (var layer in Renderer.Layers)
            {
                foreach(var renderCommand in layer.Commands)
                {
                    if (FontManager.IsTextureFont(renderCommand.Texture.Id))
                    {
                        for(int i = 0; i < renderCommand.Vertices.Count; i++)
                        {
                            renderCommand.Vertices[i] = new Vertex(renderCommand.Vertices[i].Position, Vector2.Zero, renderCommand.Vertices[i].Color);
                        }
                    }
                }
            }
        }
    }
#endif
}
