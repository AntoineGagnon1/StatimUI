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
        public float Width { get; set; }
        public bool IsWidthFixed { get; set; }

        public float Height { get; set; }
        public bool IsHeightFixed { get; set; }

        public PointF Position { get; set; }

        abstract public void Update();

        abstract public bool HasChanged();

        private Dictionary<string, Action<object>> propertySetters = new();
        public virtual void SetProperty(string name, object value)
        {
            if (propertySetters.TryGetValue(name, out var setter))
            {
                setter.Invoke(value);
            }
        }

        public virtual void InitProperty(string name, Property customProperty)
        {
            var property = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

            if (property == null)
                return;

            property.SetValue(this, customProperty);

            propertySetters.Add(name, value => customProperty.SetValue(value));
        }

        public Component()
        {
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
