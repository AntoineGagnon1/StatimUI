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

        protected static void InitVariableProperty(object instance, string name, object value)
        {
            var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);

            // TODO: might wanna throw
            if (property == null && field == null)
                return;

            Type genericType = property?.PropertyType?.GenericTypeArguments[0] ?? field!.FieldType.GenericTypeArguments[0];

            var type = typeof(VariableProperty<>).MakeGenericType(genericType);
            var variableProperty = Activator.CreateInstance(type) as Property;
            if (variableProperty == null)
                throw new Exception("Todo");

            variableProperty.SetValue(value);
            if (property != null)
                property.SetValue(instance, variableProperty);
            else if (field != null)
                field.SetValue(instance, variableProperty);
        }
        public virtual void InitVariableProperty(string name, object value)
        {
            InitVariableProperty(this, name, value);
        }

        protected static void InitBindingProperty(object instance, string name, Binding binding)
        {
            var property = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);

            // TODO: might wanna throw
            if (property == null && field == null)
                return;

            Type genericType = property?.PropertyType?.GenericTypeArguments[0] ?? field!.FieldType.GenericTypeArguments[0];

            var type = typeof(Property<>).MakeGenericType(genericType);
            var value = type
                .GetMethod("FromBinding", BindingFlags.Static | BindingFlags.Public)
                ?.Invoke(null, new object[] { binding });

            if (property != null)
                property.SetValue(instance, value);
            else if (field != null)
                field.SetValue(instance, value);
        }
        public virtual void InitBindingProperty(string name, Binding binding)
        {
            InitBindingProperty(this, name, binding);
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
