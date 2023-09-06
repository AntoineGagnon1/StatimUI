using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Components;
using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Color = StatimUI.Rendering.Color;

namespace StatimUI
{
    public abstract class Component
    {
        public abstract bool Focusable { get; }

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
        public Vector2 Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 DrawPosition => Position + Margin.Value.TopLeft + Padding.Value.TopLeft;
        #endregion // Position

        #region Transform
        public Property<Vector2> Origin { get; set; } = new ValueProperty<Vector2>();
        public Property<Vector2> Translation { get; set; } = new ValueProperty<Vector2>();
        public Property<Vector2> Scale { get; set; } = new ValueProperty<Vector2>(new Vector2(1f));
        public Property<Angle> Rotation { get; set; } = new ValueProperty<Angle>();
        #endregion

        public Property<Thickness> Padding { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);
        public Property<Thickness> Margin { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);


        public Property<Color> BackgroundColor { get; set; } = new ValueProperty<Color>(Color.Transparent);
        public Property<int> BorderRadius { get; set; } = new ValueProperty<int>(0);


        #region Events
        public event Action? Hovered;
        public event Action? HoverEnded;
        public bool IsHovered => EventManager.Hovered == this;
        internal void OnMouseEnter() => Hovered?.Invoke();
        internal void OnMouseExit() => HoverEnded?.Invoke();


        public event Action? Clicked;
        internal void OnMouseClick() => Clicked?.Invoke();


        public event Action? Focused;
        public event Action? LostFocus;
        public bool IsFocused => EventManager.Focused == this;


        public event Action? TabNavigation;
        public bool IsTabFocused => EventManager.TabNavigation == this;
        internal void OnTabNavigate() => TabNavigation?.Invoke();

        #endregion

        private float oldWidth = 0, oldHeight = 0; // Used by HasSizeChanged()

        public abstract void Start(IList<Component> slots);

        public static ForEach<T> CreateForEach<T>(IEnumerable<T> _) => new ForEach<T>();

        public Property<OutlineStyle> OutlineStyle { get; set; } = new ValueProperty<OutlineStyle>(StatimUI.OutlineStyle.Solid);

        public void RegisterStyleModifiers(Action styleModifiers)
        {
            Hovered += styleModifiers;
            HoverEnded += styleModifiers;
            Focused += styleModifiers;
            LostFocus += styleModifiers;
            // todo: tab
            styleModifiers();
        }


        private void RenderOutline(Vector2 drawPosition)
        {
            if (!IsTabFocused)
                return;
            
            if (OutlineStyle == StatimUI.OutlineStyle.Solid)
            {
                var topLeft = drawPosition - Padding.Value.TopLeft;
                Renderer.CurrentLayer.AddRectangle(topLeft, topLeft + new Vector2(PixelWidth, PixelHeight), Color.FromHex(0x3b82f6), 4f);
            }
        }

        public Component? FindComponentAt(Vector2 at, Vector2 offset)
        {
            var drawPos = offset + DrawPosition - Padding.Value.TopLeft;

            bool transform = Scale.Value != Vector2.One || Rotation.Value != Angle.Empty;
            if (transform)
            {
                TransformManager.PushTransform(Transform.FromComponent(Position, this));

            }

            Vector2 transformedPos = at;
            if (!TransformManager.IsEmpty)
            {
                Matrix3x2.Invert(TransformManager.Matrix, out var inverted);
                transformedPos = Vector2.Transform(at, inverted);
            }


            var topLeft = drawPos;
            var bottomRight = topLeft + new Vector2(PixelWidth, PixelHeight);

            if (transformedPos.X < topLeft.X || transformedPos.X > bottomRight.X || transformedPos.Y < topLeft.Y || transformedPos.Y > bottomRight.Y)
            {
                if (transform)
                    TransformManager.PopTransform();
                return null;
            }

            foreach (var child in Children)
            {
                var found = child.FindComponentAt(at, drawPos);
                if (found != null)
                {
                    TransformManager.PopTransform();
                    return found;
                }
            }

            if (transform)
                TransformManager.PopTransform();

            return this;
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
                var drawPos = offset + DrawPosition;

                bool transform = Scale.Value != Vector2.One || Rotation.Value != Angle.Empty;
                if (transform)
                    TransformManager.PushTransform(Transform.FromComponent(offset + Position, this));

                RenderOutline(drawPos);
                OnRender(drawPos);

                if (transform)
                    TransformManager.PopTransform();
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

            //Color oldColor = Color.Transparent;
            //Hovered += delegate { oldColor = BackgroundColor.Value; BackgroundColor = new ValueProperty<Color>(Color.FromRGBA(1, 0, 0)); };
            //HoverEnded += delegate { BackgroundColor = new ValueProperty<Color>(oldColor); };
        }
    }
}
