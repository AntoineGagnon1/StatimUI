using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using StatimUI.Components;

namespace StatimUI
{
    public enum SizeUnit { Pixel, Percent, FixedPixel }

    public abstract class Component
    {
        public bool Visible { get; set; } = true;

        public ChildList Children { get; } = new();

        public Component? Parent { get; set; }

        public Property<float> Width { get; set; } = new ValueProperty<float>(0);
        public SizeUnit WidthUnit { get; set; } = SizeUnit.Pixel;
        public float TotalPixelWidth => Width.Value;

        public Property<float> Height { get; set; } = new ValueProperty<float>(0);
        public SizeUnit HeightUnit { get; set; } = SizeUnit.Pixel;
        public float TotalPixelHeight => Height.Value;

        public Property<PointF> Position { get; set; } = new ValueProperty<PointF>(new PointF(0, 0));
        public PointF InsideTopLeft => Position;


        public abstract void Start(IList<Component> slots);

        // Return true if the component changed the layout
        abstract public bool Update();

        private float oldWidth = 0, oldHeight = 0;

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
