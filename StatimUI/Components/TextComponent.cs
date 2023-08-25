using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("text")]
    public class TextComponent : Component
    {
        public Property<string> Content = new ValueProperty<string>("");
        public Property<Color> TextColor = new ValueProperty<Color>(Color.White);

        private string lastContent = "";
        private RenderCommand renderCommand = new RenderCommand();

        public override bool Focusable => true;

        protected override void OnRender(Vector2 drawPosition)
        {
            base.OnRender(drawPosition);
            //renderCommand.Transform = Matrix4x4.CreateTranslation(drawPosition.X, drawPosition.Y, 0);
            Renderer.CurrentLayer.AddText(Content.Value, drawPosition, TextColor.Value);
        }

        public override void Start(IList<Component> slots)
        {
        }
        
        override public bool Update()
        {
            var textSize = FontManager.DefaultFont.GetTextSize(Content.Value);
            Width.Value.Scalar = textSize.X;
            Height.Value.Scalar = textSize.Y;
            if ((new Random()).NextDouble() > 0.9995d)
               Content.Value += "a";

            return HasSizeChanged();
        }
    }
}
