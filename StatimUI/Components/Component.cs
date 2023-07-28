using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace StatimUI
{
    public enum SizeUnit { Pixel, Percent }
    public enum AutoSizeUnit { Pixel, Percent, Auto } // Must match SizeUnit for everyting except Auto which is the same as Pixel, but the component may change the value

    public abstract class Component
    {
        public bool Visible { get; set; } = true;

        public ChildList Children { get; } = new();

        public Component? Parent { get; set; }

        #region Width
        // TODO : Discusting
        public Property<float> Width { get; set; } = new ValueProperty<float>(0);
        public AutoSizeUnit WidthUnit { get; set; } = AutoSizeUnit.Auto;
        public Property<float> MinWidth { get; set; } = new ValueProperty<float>(float.MinValue);
        public SizeUnit MinWidthUnit { get; set; } = SizeUnit.Pixel;
        public Property<float> MaxWidth { get; set; } = new ValueProperty<float>(float.MaxValue);
        public SizeUnit MaxWidthUnit { get; set; } = SizeUnit.Pixel;
        public float PixelWidth => Math.Max(Math.Min(WidthUnit == AutoSizeUnit.Auto ? Width.Value : GetSizeAsPixels(Width.Value, (SizeUnit)WidthUnit), MaxWidth.Value), GetSizeAsPixels(MinWidth.Value, MinWidthUnit));
        public float TotalPixelWidth => PixelWidth;
        public bool CanSetWidth => WidthUnit == AutoSizeUnit.Auto;
        #endregion // Width 

        #region Height
        // TODO : Discusting
        public Property<float> Height { get; set; } = new ValueProperty<float>(0);
        public AutoSizeUnit HeightUnit { get; set; } = AutoSizeUnit.Auto;
        public Property<float> MinHeight { get; set; } = new ValueProperty<float>(float.MinValue);
        public SizeUnit MinHeightUnit { get; set; } = SizeUnit.Pixel;
        public Property<float> MaxHeight { get; set; } = new ValueProperty<float>(float.MaxValue);
        public SizeUnit MaxHeightUnit { get; set; } = SizeUnit.Pixel;
        public float PixelHeight => Math.Max(Math.Min(HeightUnit == AutoSizeUnit.Auto ? Height.Value : GetSizeAsPixels(Height.Value, (SizeUnit)HeightUnit), MaxHeight.Value), GetSizeAsPixels(MinHeight.Value, MinHeightUnit));
        public float TotalPixelHeight => PixelHeight;
        public bool CanSetHeight => HeightUnit == AutoSizeUnit.Auto;
        #endregion // Height

        #region Position
        public Property<Vector2> Position { get; set; } = new ValueProperty<Vector2>(new Vector2(0, 0));
        public Vector2 InsideTopLeft => Position;
        #endregion // Position

        // TODO : change to custom type with top/bottom/right/left
        public Property<Vector4> Padding { get; set; } = new ValueProperty<Vector4>(Vector4.Zero);
        public Vector2 TopLeftPadding => new Vector2(Padding.Value.X, Padding.Value.Y);
        public Vector2 BottomRightPadding => new Vector2(Padding.Value.Z, Padding.Value.W);

        private float oldWidth = 0, oldHeight = 0; // Used by HasSizeChanged()

        public abstract void Start(IList<Component> slots);

        // Return true if the component changed the layout
        abstract public bool Update();
        abstract public void Render(Vector2 offset);

        protected bool HasSizeChanged()
        {
            bool changed = oldWidth != TotalPixelWidth || oldHeight != TotalPixelHeight;
            oldWidth = TotalPixelWidth;
            oldHeight = TotalPixelHeight;
            return changed;
        }
        
        public void InitValueProperty(string name, object value)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var field = GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);

            if (property == null && field == null)
                return;

            Type genericType = property?.PropertyType?.GenericTypeArguments[0] ?? field!.FieldType.GenericTypeArguments[0];

            var type = typeof(ValueProperty<>).MakeGenericType(genericType);
            var variableProperty = Activator.CreateInstance(type) as Property;
            if (variableProperty == null)
                throw new TypeLoadException($"Could not create an instance of the type {type}");

            variableProperty.SetValue(value);
            if (property != null)
                property.SetValue(this, variableProperty);
            else if (field != null)
                field.SetValue(this, variableProperty);
        }

        public void InitBindingProperty(string name, Binding binding)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var field = GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);

            if (property == null && field == null)
                return;

            Type genericType = property?.PropertyType?.GenericTypeArguments[0] ?? field!.FieldType.GenericTypeArguments[0];

            var type = typeof(Property<>).MakeGenericType(genericType);
            var value = type
                .GetMethod("FromBinding", BindingFlags.Static | BindingFlags.Public)
                ?.Invoke(null, new object[] { binding });

            if (property != null)
                property.SetValue(this, value);
            else if (field != null)
                field.SetValue(this, value);
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


        private float GetSizeAsPixels(float value, SizeUnit unit)
        {
            switch (unit)
            {
                case SizeUnit.Pixel: return value;
                case SizeUnit.Percent: return (value / 100f) * (Parent?.PixelWidth ?? 0.0f);
            }
            throw new InvalidDataException($"Invalid SizeUnit value : {unit}({(int)unit})");
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
