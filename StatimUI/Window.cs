using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class Window
    {
        public Component Root { get; set; }

        public void Update()
        {
            Root.Update();

            Root.Render(Vector2.Zero);
#if DEBUG
            DebugTools.DebugSettings.RenderDebugInfo(Root, Vector2.Zero);
#endif
        }

        static Window()
        {
            // TODO: find a better place to put this
            ValueProperty<Thickness>.StringConverter = new ThicknessConverter();
            ValueProperty<Dimension>.StringConverter = new DimensionConverter();
            ValueProperty<Vector2>.StringConverter = new Vector2Converter();
            ValueProperty<Color>.StringConverter = new ColorConverter();
        }
    }
}
