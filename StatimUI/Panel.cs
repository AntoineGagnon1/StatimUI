using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public abstract class Panel
    {
        public ChildList Children { get; } = new();

        public Property<Thickness> Padding { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);

        public Property<Color> BackgroundColor { get; set; } = new ValueProperty<Color>(Color.Transparent);
        public Property<int> BorderRadius { get; set; } = new ValueProperty<int>(0);
        
        public System.Drawing.Size Size { get; set; } = new();

        public Property<Vector2> Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 DrawPosition => Position.Value + Padding.Value.TopLeft;

        public bool CanClose()
        {
            // TODO : chose if the window should close or not
            return true;
        }

        public void Update(Vector2 offset)
        {
            if (Children.Count > 0)
                Children[0].Update();

            // Render
            if (BackgroundColor.Value.A != 0)
            {
                var topLeft = (offset + DrawPosition) - Padding.Value.TopLeft;
                Renderer.CurrentLayer.AddRectangleFilled(topLeft, topLeft + new Vector2(Size.Width, Size.Height), BackgroundColor.Value, BorderRadius.Value);
            }

            if (Children.Count > 0)
                Children[0].Render((offset + DrawPosition));
#if DEBUG
            if (Children.Count > 0)
                DebugTools.DebugSettings.RenderDebugInfo(Children[0], (offset + DrawPosition));
#endif
        }

        static Panel()
        {
            // TODO: find a better place to put this
            ValueProperty<Thickness>.StringConverter = new ThicknessConverter();
            ValueProperty<Dimension>.StringConverter = new DimensionConverter();
            ValueProperty<Vector2>.StringConverter = new Vector2Converter();
            ValueProperty<StatimUI.Rendering.Color>.StringConverter = new ColorConverter();
            ValueProperty<Angle>.StringConverter = new AngleConverter();
        }
    }
}
