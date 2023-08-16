using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatimUI.Rendering
{
    public static class Renderer
    {
        public static IRenderingAdapter? Adapter { get; set; }

        public static List<RenderLayer> Layers { get; } = new () { new RenderLayer() };
        static int _currentLayerIndex = 0;
        static int currentLayerIndex
        {
            get => _currentLayerIndex;
            set
            {
                _currentLayerIndex = value;

                if (Layers.Count <= _currentLayerIndex)
                {
                    for (int i = 0; i <= _currentLayerIndex - Layers.Count; i++)
                    {
                        Layers.Add(new RenderLayer() { });
                    }
                }
            }
        }

        public static RenderLayer CurrentLayer => Layers[currentLayerIndex];

        public static void PushLayer() => currentLayerIndex++;
        public static void PopLayer() => currentLayerIndex--;

        public static void ClearLayers()
        {
            foreach (var layer in Layers)
                layer.Clear();

            currentLayerIndex = 0;
        }

        public static int MaxVerticesCount() => Layers.Max(x => x.Commands.Max(y => y.Vertices.Count));
        public static int MaxIndicesCount() => Layers.Max(x => x.Commands.Max(y => y.Indices.Count));
    }
}
