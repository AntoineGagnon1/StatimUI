using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public virtual void SetProperty(string name, object value)
        {
            if (namedProperties.TryGetValue(name, out var property))
            {
                property.SetValue(value);
            }
        }

        public virtual void InitProperty(string name, Property customProperty)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (property == null)
                return;

            property.SetValue(this, customProperty);

            namedProperties.Add(name, customProperty);
        }

        protected void OnValueChanged<T>(T value)
        {
            HasChanged = true;
        }

        public Component()
        {
            Width.ValueChanged += OnValueChanged;
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
