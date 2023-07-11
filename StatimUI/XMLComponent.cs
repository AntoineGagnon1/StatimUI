using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StatimUI
{

    public class XMLComponent : Component
    {
        public List<Component> Children { get; private set; }

        public override void Update()
        {
        }

        public static IReadOnlyCollection<string> Names { get; }

        public override bool HasChanged()
        {
            throw new NotImplementedException();
        }

        public XMLComponent(string name)
        {
            var document = new XmlDocument();
            document.LoadXml("<text></text>");
            foreach (XmlNode xmlNode in document.ChildNodes)
            {
                Parse(xmlNode);
            }
        }

        private void Parse(XmlNode node)
        {
            if (ComponentByName.TryGetValue(node.Name, out var componentType))
            {
                var component = Activator.CreateInstance(componentType) as Component;
                if (component != null)
                {
                    component.GetType().GetProperty("Content")!.SetValue(component, node.InnerText);
                    Children.Add(component);
                }
            }
        }

        static XMLComponent()
        {
            List<string> componentNames = new();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var names = assembly.GetManifestResourceNames();
                foreach (string name in names)
                {
                    string? extension = Path.GetExtension(name);
                    if (extension != null && extension == Statim.FileExtension)
                    {
                        componentNames.Add(name);
                    }
                }
            }
            Names = componentNames.AsReadOnly();
        }
    }
}
