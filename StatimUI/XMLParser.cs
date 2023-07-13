using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

        public static Component ParseElement(XMLComponent self, XElement element)
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
                InitAttribute(self, result,  attribute.Name.LocalName, attribute.Value);
            }
            
            if(!string.IsNullOrEmpty(element.Value))
            {
                InitAttribute(self, result, "Content", element.Value);
            }
            return result;
        }

        private static void InitAttribute(XMLComponent self, Component component, string name, string value)
        {
            if (IsOneWay(value))
            {
                if (IsTwoWay(value))
                {
                    self.GetBinding(name, out var get, out var set);
                    component.InitTwoWayProperty(name, (Func<object>)get, (Action<object>)set);
                }
                else
                {
                    self.GetBinding(name, out var get, out var set);
                    component.InitOneWayProperty(name, (Func<object>)get);
                }
            }
            else
            {
                component.InitVariableProperty(name, value);
            }
        }

        public static bool IsOneWay(string value) => value.StartsWith('{') && value.EndsWith('}');
        public static bool IsTwoWay(string value) => Regex.IsMatch(value, "{\\s*bind\\s+\\S+\\s*}");
        public static string GetOneWayContent(string value) => value.Substring(1, value.Length - 2);

        private static readonly int bindStartIndex = "{bind".Length;
        public static string GetTwoWayVariableName(string value)
        {
            string noSpace = value.Replace(" ", "");
            return noSpace.Substring(bindStartIndex, noSpace.Length - bindStartIndex - 1);
        }
    }
}
