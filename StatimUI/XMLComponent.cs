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
        public Component Child { get; private set; }

        public override void Update()
        {
            Child?.Update();
        }

        public static Dictionary<string, Stream> XMLComponentByName { get; } = new();

        public override bool HasChanged()
        {
            throw new NotImplementedException();
        }

        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
        public XMLComponent(string name)
        {
            var fragments = XMLParser.ParseFragment(XmlReader.Create(XMLComponentByName[name], xmlSettings));
            XElement root = new XElement("root", fragments);

            foreach (XElement element in root.Elements())
            {
                if (element.Name == "script")
                {

                }
                else
                    Child = XMLParser.ParseElement(element);
            }
        }

        static XMLComponent()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var names = assembly.GetManifestResourceNames();
                foreach (string name in names)
                {
                    string? extension = Path.GetExtension(name);
                    if (extension == null || extension != Statim.FileExtension)
                        continue;

                    Stream? stream = assembly.GetManifestResourceStream(name);
                    if (stream == null)
                        continue;

                    var parts = name.Split('.');
                    XMLComponentByName.Add(parts[parts.Length - 2], stream);
                }
            }
        }
    }
}
