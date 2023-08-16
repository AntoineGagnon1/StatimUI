using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace StatimUI
{
    public abstract class Component
    {
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
        public float InnerPixelWidth => PixelWidth - Padding.Value.Horizontal;
        // Size (excluding margins)
        public float PixelWidth => Math.Max(Math.Min(Width.Value.GetPixelSize(Parent), MaxWidth.Value.GetPixelSize(Parent)), MinWidth.Value.GetPixelSize(Parent));
        // Size (including margins)
        public float TotalPixelWidth => PixelWidth + Margin.Value.Horizontal;
        #endregion // Width 

        #region Height
        public Property<Dimension> Height { get; set; } = new ValueProperty<Dimension>(new Dimension(0, DimensionUnit.Auto));
        public Property<Dimension> MinHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MinValue, DimensionUnit.Pixel));
        public Property<Dimension> MaxHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MaxValue, DimensionUnit.Pixel));
        public float InnerPixelHeight => PixelHeight - Padding.Value.Vertical;
        public float PixelHeight => Math.Max(Math.Min(Height.Value.GetPixelSize(Parent), MaxHeight.Value.GetPixelSize(Parent)), MinHeight.Value.GetPixelSize(Parent));
        public float TotalPixelHeight => PixelHeight + Margin.Value.Vertical;
        #endregion // Height

        #region Position
        public Property<Vector2> Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 DrawPosition => Position.Value + Margin.Value.TopLeft + Padding.Value.TopLeft;
        #endregion // Position

        public Property<Thickness> Padding { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);
        public Property<Thickness> Margin { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);


        private float oldWidth = 0, oldHeight = 0; // Used by HasSizeChanged()

        public abstract void Start(IList<Component> slots);

        protected static ForEach<T> CreateForEach<T>(IEnumerable<T> _) => new ForEach<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Whether the component has updated its layout</returns>
        abstract public bool Update();
        public void Render(Vector2 offset)
        {
            if (Visible)
                OnRender(offset + DrawPosition);
        }
        abstract protected void OnRender(Vector2 drawPosition);

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
