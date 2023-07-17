using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Text.RegularExpressions;

namespace StatimUI
{
    internal static class Parser
    {
        internal static SyntaxTree Parse(string name, Stream stream)
        {
            var preParse = XMLPreParse(stream);

            var tree = CSharpSyntaxTree.ParseText(CreateClassString(name, preParse.Script, preParse.Child));
            var visiter = new PropertySyntaxRewriter();
            var newRoot = visiter.Visit(tree.GetRoot());
            return CSharpSyntaxTree.Create(newRoot as CSharpSyntaxNode);
        }

        private record struct PreParseResult(string Script, string Child) { }
        private static PreParseResult XMLPreParse(Stream stream)
        {
            const string scriptStartTag = "<script>";
            const string scriptEndTag = "</script>";

            StringBuilder scriptContent = new();
            StringBuilder content = new();
            bool readingScript = false;

            using (StreamReader reader = new(stream))
            {
                string? line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(scriptStartTag))
                    {
                        readingScript = true;
                        line = line.Substring(line.IndexOf(scriptStartTag) + scriptStartTag.Length);
                    }
                    if (line.StartsWith(scriptEndTag)) // No else because <script> and </script> might be on the same line 
                    {
                        readingScript = false;
                        scriptContent.AppendLine(line.Substring(0, line.IndexOf(scriptEndTag)));
                        line = line.Substring(line.IndexOf(scriptEndTag) + scriptEndTag.Length);
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue; // Skip empty lines

                    if (readingScript)
                        scriptContent.AppendLine(line);
                    else
                        content.AppendLine(line);
                }
            }

            return new PreParseResult(scriptContent.ToString(), content.ToString());
        }

        private static string CreateClassString(string name, string content, string childXML)
        {
            ChildInfo? childInfo = ParseChildXML(childXML);

            StringBuilder constructorContent = new();

            if (childInfo != null)
            {
                constructorContent.AppendLine($"Child = new {childInfo?.Name}();");

                // Bindings
                foreach ((string Name, string Value, BindingType Type) in childInfo?.Bindings)
                {
                    if (Type == BindingType.Value)
                        constructorContent.AppendLine($"Child.InitValueProperty({Name}, {Value});");
                    else if (Type == BindingType.OneWay)
                        constructorContent.AppendLine($"Child.InitBindingProperty({Name}, new Binding(() => {Value}));");
                    else
                        constructorContent.AppendLine($"Child.InitBindingProperty({Name}, new Binding(() => {Value}, (dynamic value) => {{{Name} = {Value};}}));");
                }
            }

            return @$"
            using System;
            using StatimUI;
            using StatimUIXmlComponents;
            namespace StatimUIXmlComponents
            {{ 

                public class {name} : Component
                {{
                    public Component? Child {{ get; private set; }}

                    public override void Update() => Child?.Update();
                    
                    public {name}()
                    {{
                        {constructorContent}
                    }}

                    {content}
                }}
            }}";
        }

        private enum BindingType { OneWay, TwoWay, Value }
        private record struct ChildInfo(string Name, List<(string Name, string Value, BindingType Type)> Bindings) { }
        private static ChildInfo? ParseChildXML(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var node = XElement.Parse(xml);

            List<(string, string, BindingType)> bindings = new();
            foreach (var attr in node.Attributes())
            {
                var type = IsBinding(attr.Value) ? IsTwoWayBinding(attr.Value) ? BindingType.TwoWay : BindingType.OneWay : BindingType.Value;
                bindings.Add((attr.Name.LocalName, GetBindingContent(attr.Value), type));
            }

            return new ChildInfo(node.Name.LocalName, bindings);
        }

        private static bool IsBinding(string value) => value.StartsWith('{') && value.EndsWith('}');
        private static bool IsTwoWayBinding(string value) => value.StartsWith("{bind ") && value.EndsWith('}');
        private static string GetBindingContent(string value)
        {
            if(IsBinding(value))
            {
                if (IsTwoWayBinding(value))
                    return value.Substring("{bind ".Length, value.Length - (1 + "{bind ".Length));
                else
                    return value.Substring(1, value.Length - 2);
            }
            else
            {
                return value;
            }
        }
    }
}
