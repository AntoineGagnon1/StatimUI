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
using Microsoft.CodeAnalysis.Text;

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

                        var arguments = new List<ArgumentSyntax>();
                        if (variable.Initializer != null)
                            arguments.Add(SyntaxFactory.Argument(variable.Initializer.Value));
                        var argumentsSeparated = SyntaxFactory.SeparatedList(arguments);
                        var objectCreationExpression = SyntaxFactory.ObjectCreationExpression(variablePropertyType, SyntaxFactory.ArgumentList(argumentsSeparated), null);
                        var equalsValueClause = SyntaxFactory.EqualsValueClause(objectCreationExpression);

                        variables.Add(variable.WithInitializer(equalsValueClause).NormalizeWhitespace());
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

            return CSharpSyntaxTree.Create((CSharpSyntaxNode)root.ReplaceNode(classRoot, newClassRoot));
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
            ScriptBuilder startContent = new();
            if (!string.IsNullOrWhiteSpace(childXML))
            {
                XElement element = XElement.Parse(childXML);

                startContent.Indent(3);

                var startMethods = new List<string>();
                InitComponent(startContent, startMethods, element, "__child", "this");
                AddStartMethods(startContent, startMethods);

                startContent.AppendLine("Children.Add(__child);");

                startContent.Unindent(3);
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
        public override bool Update() 
        {{ 
            Children[0].Update(); 
            if(WidthUnit == AutoSizeUnit.Auto)
                Width = Children[0].TotalPixelWidth;
            if(HeightUnit == AutoSizeUnit.Auto)
                Height = Children[0].TotalPixelHeight;
            return HasSizeChanged();
        }}
        
        public override void Start(IList<Component> slots)
        {{
{startContent}
        }}

{content}
    }}
}}";
        }

        private static void InitComponent(ScriptBuilder content, List<string> startMethods, XElement element, string variableName, string parentName)
        {
            if (element.Name.LocalName == "foreach")
            {
                InitForeach(content, startMethods, element, variableName, parentName);
                return;
            }

            List<string> childNames = new();
            if (element.HasElements)
            {
                int i = 0;
                foreach (var child in element.Elements())
                {
                    var childName = $"{variableName}_{i}";
                    childNames.Add(childName);
                    InitComponent(content, startMethods, child, childName, variableName);
                    i++;
                }
            }

            content.AppendLine($"Component {variableName} = new {GetComponentName(element.Name.LocalName)}();");

            foreach (var attribute in element.Attributes())
                InitProperty(content, variableName, attribute.Name.LocalName, attribute.Value);

            startMethods.Add($"{variableName}.Start(new List<Component> {{ {string.Join(',', childNames)} }});");
        }

        private static void InitForeach(ScriptBuilder content, List<string> startMethods, XElement element, string variableName, string parentName)
        {
            var foreachContent = new ScriptBuilder();
            var foreachStartMethods = new List<string>();

            content.AppendLine($"ForEach {variableName} = new {GetComponentName(element.Name.LocalName)}();");

            var itemName = GetBindingContent(element.Attribute("item")!.Value);
            foreachContent.AppendLineNoIndent($"Func<object, List<Component>> {variableName}_foreach = ({itemName}) => {{");
            foreachContent.Indent();


            List<string> childNames = new();
            if (element.HasElements)
            {
                int i = 0;
                foreach (var child in element.Elements())
                {
                    var childName = $"{variableName}_{i}";
                    childNames.Add(childName);
                    InitComponent(foreachContent, foreachStartMethods, child, $"{variableName}_{i}", variableName);
                    i++;
                }
            }

            foreach (var childName in childNames)
                foreachContent.AppendLine($"{childName}.Parent = {parentName};");

            AddStartMethods(foreachContent, foreachStartMethods);

            foreachContent.AppendLine($"return new List<Component> {{ {string.Join(',', childNames)} }};");
            foreachContent.Unindent();
            foreachContent.AppendLine("};");

            var inAttribute = element.Attribute("In");
            InitProperty(content, variableName, inAttribute!.Name.LocalName, inAttribute.Value);

            foreachContent.AppendLine($"{variableName}.Start(new List<Component> {{ }});");
            foreachContent.AppendLine($"{variableName}.ComponentsCreator = {variableName}_foreach;");
            startMethods.Add(foreachContent.ToString());
        }

        private static void InitProperty(ScriptBuilder content, string variableName, string name, string value)
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
            {
                return type.Name;
            }

            return typeName;
        }

        private static void AddStartMethods(ScriptBuilder content, List<string> startMethods)
        {
            for (int i = startMethods.Count - 1; i >= 0; i--)
                content.AppendLine(startMethods[i]);
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
