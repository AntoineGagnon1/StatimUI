using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Components;
using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Color = StatimUI.Rendering.Color;

namespace StatimUI
{
    public abstract class Component
    {
        public abstract bool Focusable { get; }

        public bool Focused => FocusManager.FocusedComponent == this;
        public bool Focus()
        {
            if (Focusable)
            {
                FocusManager.FocusedComponent = this;
                return true;
            }

            return false;
        }

        public bool Visible { get; set; } = true;

        public ChildList Children { get; } = new();

        public List<Component> VisibleChildren
        {
            get
            {
                var result = new List<Component>();
                foreach (var child in Children)
                {
                    if (child.Visible)
                        result.Add(child);
                }
                return result;
            }
        }

        public Component? Parent { get; set; }

        #region Width
        public Property<Dimension> Width { get; set; } = new ValueProperty<Dimension>(new Dimension(0, DimensionUnit.Auto));
        public Property<Dimension> MinWidth { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MinValue, DimensionUnit.Pixel));
        public Property<Dimension> MaxWidth { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MaxValue, DimensionUnit.Pixel));
        // Size (excluding padding)
        public float InnerPixelWidth => Math.Max(Math.Min(Width.Value.GetPixelSize(Parent), MaxWidth.Value.GetPixelSize(Parent)), MinWidth.Value.GetPixelSize(Parent));
        // Size (excluding margins)
        public float PixelWidth => InnerPixelWidth + Padding.Value.Horizontal;
        // Size (including margins)
        public float TotalPixelWidth => PixelWidth + Margin.Value.Horizontal;
        #endregion // Width 

        #region Height
        public Property<Dimension> Height { get; set; } = new ValueProperty<Dimension>(new Dimension(0, DimensionUnit.Auto));
        public Property<Dimension> MinHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MinValue, DimensionUnit.Pixel));
        public Property<Dimension> MaxHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MaxValue, DimensionUnit.Pixel));
        public float InnerPixelHeight => Math.Max(Math.Min(Height.Value.GetPixelSize(Parent), MaxHeight.Value.GetPixelSize(Parent)), MinHeight.Value.GetPixelSize(Parent));
        public float PixelHeight => InnerPixelHeight + Padding.Value.Vertical;
        public float TotalPixelHeight => PixelHeight + Margin.Value.Vertical;
        #endregion // Height

        #region Position
        public Property<Vector2> Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 DrawPosition => Position.Value + Margin.Value.TopLeft + Padding.Value.TopLeft;
        #endregion // Position

        public Property<Thickness> Padding { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);
        public Property<Thickness> Margin { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);


        public Property<Color> BackgroundColor { get; set; } = new ValueProperty<Color>(Color.Transparent);
        public Property<int> BorderRadius { get; set; } = new ValueProperty<int>(0);


        private float oldWidth = 0, oldHeight = 0; // Used by HasSizeChanged()

        public abstract void Start(IList<Component> slots);

        public static ForEach<T> CreateForEach<T>(IEnumerable<T> _) => new ForEach<T>();

        public Property<OutlineStyle> OutlineStyle { get; set; } = new ValueProperty<OutlineStyle>(StatimUI.OutlineStyle.Solid);

        private void RenderOutline(Vector2 drawPosition)
        {
            if (!Focused)
                return;
            
            if (OutlineStyle == StatimUI.OutlineStyle.Solid)
            {
                var topLeft = drawPosition - Padding.Value.TopLeft;
                Renderer.CurrentLayer.AddRectangle(topLeft, topLeft + new Vector2(PixelWidth, PixelHeight), Color.FromHex(0x3b82f6), 4f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Whether the component has updated its layout</returns>
        abstract public bool Update();
        public void Render(Vector2 offset)
        {
            if (Visible)
            {
                RenderOutline(offset + DrawPosition);
                OnRender(offset + DrawPosition);
            }
        }
        virtual protected void OnRender(Vector2 drawPosition)
        {
            if (BackgroundColor.Value.A != 0)
            {
                var topLeft = drawPosition - Padding.Value.TopLeft;
                Renderer.CurrentLayer.AddRectangleFilled(topLeft, topLeft + new Vector2(PixelWidth, PixelHeight), BackgroundColor.Value, BorderRadius.Value);
            }
        }

        protected bool HasSizeChanged()
        {
            bool changed = oldWidth != TotalPixelWidth || oldHeight != TotalPixelHeight;
            oldWidth = TotalPixelWidth;
            oldHeight = TotalPixelHeight;
            return changed;
        }


        [MemberNotNull(nameof(Parent))]
        protected void AssertParent()
        {
            if (Parent == null)
                throw new NullReferenceException($"A {GetType().Name} component has to have a parent component.");
        }

        public Component()
        {
            Children.OnChildAdded += (sender, child) => { child.Parent = this; };
        }
    }
}
