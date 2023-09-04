using System;
using System.Collections.Generic;
using System.Text;

namespace StatimCodeGenerator
{
    public struct ComponentProperties
    {
        private Dictionary<string, HashSet<string>> componentProperties = new();

        public ComponentProperties()
        {
        }

        public void SetProperties(string component, HashSet<string> properties)
        {
            componentProperties.Add(component, properties);
        }

        public void AddProperty(string component, string property)
        {
            if (componentProperties.TryGetValue(component, out var properties))
            {
                properties.Add(property);
                return;
            }

            componentProperties.Add(component, new HashSet<string> { property });
        }

        public bool ContainsComponent(string component) => componentProperties.ContainsKey(component);

        public bool ContainsProperty(string component, string property)
        {
            if (componentProperties.TryGetValue(component, out var properties))
                return properties.Contains(property);

            return false;
        }
    }
}
