using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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

        public Component? Parent { get; set; }

        #region Width
        // TODO : Discusting
        public Property<Dimension> Width { get; set; } = new ValueProperty<Dimension>(new Dimension(0, DimensionUnit.Auto));
        public Property<Dimension> MinWidth { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MinValue, DimensionUnit.Pixel));
        public Property<Dimension> MaxWidth { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MaxValue, DimensionUnit.Pixel));
        public float PixelWidth => Math.Max(Math.Min(Width.Value.GetPixelSize(Parent), MaxWidth.Value.GetPixelSize(Parent)), MinWidth.Value.GetPixelSize(Parent));
        public float TotalPixelWidth => PixelWidth;
        #endregion // Width 

        #region Height
        // TODO : Discusting
        public Property<Dimension> Height { get; set; } = new ValueProperty<Dimension>(new Dimension(0, DimensionUnit.Auto));
        public Property<Dimension> MinHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MinValue, DimensionUnit.Pixel));
        public Property<Dimension> MaxHeight { get; set; } = new ValueProperty<Dimension>(new Dimension(float.MaxValue, DimensionUnit.Pixel));
        public float PixelHeight => Math.Max(Math.Min(Height.Value.GetPixelSize(Parent), MaxHeight.Value.GetPixelSize(Parent)), MinHeight.Value.GetPixelSize(Parent));
        public float TotalPixelHeight => PixelHeight;
        #endregion // Height

        #region Position
        public Property<Vector2> Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 InsideTopLeft => Position.Value;
        #endregion // Position

        // TODO : change to custom type with top/bottom/right/left
        public Property<Thickness> Padding { get; set; } = new ValueProperty<Thickness>(Thickness.Zero);
        public Vector2 TopLeftPadding => new Vector2(Padding.Value.Left, Padding.Value.Top);
        public Vector2 BottomRightPadding => new Vector2(Padding.Value.Right, Padding.Value.Bottom);

        private float oldWidth = 0, oldHeight = 0; // Used by HasSizeChanged()

        public abstract void Start(IList<Component> slots);


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Whether the component has updated its layout</returns>
        abstract public bool Update();
        abstract public void Render(Vector2 offset);

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


        public static Dictionary<string, Type> ComponentByName { get; private set; } = new();

        static Component()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Component)))
                    {
                        var nameAttr = type.GetCustomAttribute<ComponentAttribute>();
                        if (nameAttr != null)
                            ComponentByName.Add(nameAttr.TagName, type);
                    }
                }
            }
        }

        public static Component? FromName(string name) => Activator.CreateInstance(ComponentByName[name]) as Component;
    }
}
