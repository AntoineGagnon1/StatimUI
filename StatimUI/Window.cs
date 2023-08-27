using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class Panel
    {
        public Component Root { get; set; }
        public Size Size { get; set; } = new();

        public void TryClose()
        {
            // TODO : chose if the window should close or not
            Renderer.Adapter!.DestroySubWindow(this);
        }

        public void Update()
        {
            Root.Update();

            Root.Render(Vector2.Zero);
#if DEBUG
            DebugTools.DebugSettings.RenderDebugInfo(Root, Vector2.Zero);
#endif
        }

        static Panel()
        {
            // TODO: find a better place to put this
            ValueProperty<Thickness>.StringConverter = new ThicknessConverter();
            ValueProperty<Dimension>.StringConverter = new DimensionConverter();
            ValueProperty<Vector2>.StringConverter = new Vector2Converter();
            ValueProperty<StatimUI.Rendering.Color>.StringConverter = new ColorConverter();
        }
    }
}
