using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StatimUI
{
    public static class XMLParser
    {
        public static IEnumerable<XNode> ParseFragment(XmlReader xr)
        {
            xr.MoveToContent();
            XNode node;
            while (!xr.EOF && (node = XNode.ReadFrom(xr)) != null)
            {
                yield return node;
            }
        }

        private static Component ParseElement(XElement element)
        {
            Component? result = null;
            if (Component.ComponentByName.TryGetValue(element.Name.LocalName, out var componentType))
            {
                var component = Activator.CreateInstance(componentType) as Component;
                if (component != null)
                    result = component;
            }
            else
            {

            }

            if (result == null)
                throw new Exception("TODO");

            foreach (XAttribute attribute in element.Attributes())
            {
                //var type = typeof(Property<>).MakeGenericType(GetTypeOfString(attribute.Value));
                result.SetProperty(attribute.Name.LocalName, attribute.Value);
            }
            //
            result.InitProperty("Content", new Property<string>(() => element.Value, val => { }));
            return result;
        }
    }
}
