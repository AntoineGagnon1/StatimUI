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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Components;

namespace StatimUI
{
    internal static class Parser
    {
        internal static SyntaxTree Parse(string name, Stream stream)
        {
            var preParse = XMLPreParse(stream);

            var tree = CSharpSyntaxTree.ParseText(CreateClassString(name, preParse.Script, preParse.Child));
            return AddProperties(tree);
        }

        private static SyntaxTree AddProperties(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var classRoot = root
                .ChildNodes().OfType<NamespaceDeclarationSyntax>().Single()
                .ChildNodes().OfType<ClassDeclarationSyntax>().Single();

            HashSet<string> propertyNames = new();
            SyntaxNode newClassRoot = classRoot.ReplaceNodes(classRoot.ChildNodes(), (node, n2) =>
            {
                if (node is FieldDeclarationSyntax field)
                {
                    var oldNode = node;
                    if (!field.Modifiers.Any(modif => modif.Text == "public"))
                        return node;

                    TypeSyntax variablePropertyType = CreateGenericType("ValueProperty", field.Declaration.Type);
                    var variables = new List<VariableDeclaratorSyntax>();

                    foreach (var variable in field.Declaration.Variables)
                    {
                        propertyNames.Add(variable.Identifier.Text);

                        if (variable.Initializer != null)
                        {
                            var arguments = new List<ArgumentSyntax>()
                            {
                                SyntaxFactory.Argument(variable.Initializer.Value)
                            };

                            var argumentsSeparated = SyntaxFactory.SeparatedList(arguments);
                            var objectCreationExpression = SyntaxFactory.ObjectCreationExpression(variablePropertyType, SyntaxFactory.ArgumentList(argumentsSeparated), null);
                            var equalsValueClause = SyntaxFactory.EqualsValueClause(objectCreationExpression);

                            variables.Add(variable.WithInitializer(equalsValueClause).NormalizeWhitespace());
                        }
                        else
                            variables.Add(variable);
                    }

                    var variablesSeparated = SyntaxFactory.SeparatedList(variables);
                    var newNode = field.WithDeclaration(
                        field.Declaration.WithType(
                            CreateGenericType("Property", field.Declaration.Type)
                        ).WithVariables(variablesSeparated));
                    return newNode;
                }

                return node;
            });
            var dotValueRewriter = new DotValueSyntaxRewriter(propertyNames);
            newClassRoot = dotValueRewriter.Visit(newClassRoot);

            return CSharpSyntaxTree.Create(root.ReplaceNode(classRoot, newClassRoot) as CSharpSyntaxNode);
        }

        private static GenericNameSyntax CreateGenericType(string name, TypeSyntax genericType)
            => SyntaxFactory.GenericName(SyntaxFactory.Identifier(name), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(new TypeSyntax[] { genericType })));
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
            StringBuilder constructorContent = new();
            if (!string.IsNullOrWhiteSpace(childXML))
            {
                XElement element = XElement.Parse(childXML);

                InitComponent(constructorContent, element, "__child");

                constructorContent.AppendLine("Child = __child;");
            }

            return @$"
            using System.Collections.Generic;
            using System.Collections;
            using System.Linq;
            using System;
            using StatimUI;
            using StatimUI.Components;

            namespace StatimUIXmlComponents
            {{ 

                public class {name} : Component
                {{
                    public Component? Child {{ get; private set; }}

                    public override void Update() => Child?.Update();
                    
                    public {name}(List<Component> slots)
                    {{
                        {constructorContent}
                    }}

                    {content}
                }}
            }}";
        }

        private static void InitComponent(StringBuilder content, XElement element, string variableName)
        {
            List<string> childNames = new();
            if (element.HasElements)
            {
                int i = 0;
                foreach (var child in element.Elements())
                {
                    var childName = $"{variableName}_{i}";
                    childNames.Add(childName);
                    InitComponent(content, child, $"{variableName}_{i}");
                    i++;
                }
            }

            var slots = $"new List<Component>() {{ {string.Join(',', childNames)} }}";
            content.AppendLine($"Component {variableName} = new {GetComponentName(element.Name.LocalName)}({slots});");

            // Bindings
            foreach (var attribute in element.Attributes())
            {
                InitProperty(content, variableName, attribute.Name.LocalName, attribute.Value);
            }

        }

        private static void InitProperty(StringBuilder content, string variableName, string name, string value)
        {
            var bindingValue = GetBindingContent(value);
            if (IsTwoWayBinding(value))
            {
                content.AppendLine($"{variableName}.InitBindingProperty(\"{name}\", new Binding(() => {bindingValue}, (dynamic value) => {{{name} = value;}}));");
            }
            else if (IsBinding(value))
            {
                content.AppendLine($"{variableName}.InitBindingProperty(\"{name}\", new Binding(() => {bindingValue}));");
            }
            else
            {
                content.AppendLine($"{variableName}.InitValueProperty(\"{name}\", \"{bindingValue}\");");
            }
        }

        private static string GetComponentName(string typeName)
        {
            if (Component.ComponentByName.TryGetValue(typeName, out var type))
                return type.Name;

            return typeName;
        }

        private enum BindingType { OneWay, TwoWay, Value }
        private record struct ChildInfo(string Name, List<(string Name, string Value, BindingType Type)> Bindings) { }
        private static ChildInfo? ParseChildXML(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            var node = XElement.Parse(xml);

            if (node.HasElements)
            {
                foreach (var element in node.Elements())
                {

                }
            }

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
