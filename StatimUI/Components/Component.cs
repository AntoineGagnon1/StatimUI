using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatimUI
{
    public abstract class Component
    {
        public Property<float> Width { get; set; }
        public bool IsWidthFixed { get; set; }

        public Property<float> Height { get; set; }
        public bool IsHeightFixed { get; set; }

        public Property<PointF> Position { get; set; }

        abstract public void Update();

        public bool HasChanged { get; protected set; }

        private Dictionary<string, Property> namedProperties = new();

        public void InitValueProperty(string name, object value)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (property is null)
                return;

            var type = typeof(ValueProperty<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0]);
            var variableProperty = Activator.CreateInstance(type) as Property;
            if (variableProperty == null)
                throw new Exception("Todo");

            variableProperty.SetValue(value);
            property.SetValue(this, variableProperty);
        }

        public void InitBindingProperty(string name, Binding binding)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (property is null)
                return;

            var type = typeof(Property<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0]);
            var value = type
                .GetMethod("FromBinding", BindingFlags.Static | BindingFlags.Public)
                ?.Invoke(null, new object[] { binding });

            property.SetValue(this, value);
        }


        protected void OnValueChanged<T>(T value)
        {
            HasChanged = true;
        }

        public Component()
        {
            //Width.ValueChanged += OnValueChanged;
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
