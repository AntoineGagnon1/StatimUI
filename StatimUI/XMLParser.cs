using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static Component ParseElement(XElement element)
        {
            Component? result = null;
            if (Component.ComponentByName.TryGetValue(element.Name.LocalName, out var componentType))
            {
                result = Activator.CreateInstance(componentType) as Component;
            }
            else
            {
                result = XMLComponent.Create(element.Name.LocalName);
            }

            if (result == null)
                throw new Exception("TODO - Element not found");

            foreach (XAttribute attribute in element.Attributes())
            {
                InitAttribute(result,  attribute.Name.LocalName, attribute.Value);
            }
            InitAttribute(result, "Content", element.Value);
            return result;
        }

        private static void InitAttribute(Component component, string name, string value)
        {
            if (IsBinding(value))
            {
                // TODO: THIS
                component.InitBindedProperty("Content", () => "123", val => { });
            }
            else
            {
                component.InitVariableProperty(name, value);
            }
        }

        private static bool IsBinding(string value) => value.StartsWith('{') && value.EndsWith('}');
    }
}
