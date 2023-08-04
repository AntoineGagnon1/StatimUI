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
using System.Globalization;
using System.IO;

namespace StatimUI
{
    public static class CodeGenerator
    {
        public static SyntaxTree GenerateTree(string name, Stream stream)
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

        private struct PreParseResult
        {
            public string Script, Child;

            public PreParseResult(string script, string child)
            {
                Script = script;
                Child = child;
            }
        }
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

        private static string CreateClassString(string name, string content, string statimSyntax)
        {
            ScriptBuilder startContent = new();
            if (!string.IsNullOrWhiteSpace(statimSyntax))
            {
                var root = StatimParser.Parse(statimSyntax);
                if (root != null)
                {
                    startContent.Indent(3);

                    var startMethods = new List<string>();
                    InitComponent(startContent, startMethods, root, "__child", "this");
                    AddStartMethods(startContent, startMethods);

                    startContent.AppendLine("Children.Add(__child);");

                    startContent.Unindent(3);
                }
            }

            return @$"
// <auto-generated/>
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

            Width.Value.Scalar = Children[0].TotalPixelWidth + Padding.Value.Left + Padding.Value.Right;
            Height.Value.Scalar = Children[0].TotalPixelHeight + Padding.Value.Top + Padding.Value.Bottom;

            return HasSizeChanged();
        }}

        public override void Render(System.Numerics.Vector2 offset) => Children[0].Render(offset + Padding.Value.TopLeft);
        
        public override void Start(IList<Component> slots)
        {{
{startContent}
        }}

{content}
    }}
}}";
        }

        private static void InitComponent(ScriptBuilder content, List<string> startMethods, ComponentSyntax syntax, string variableName, string parentName)
        {
            if (syntax is ForEachSyntax foreachSyntax)
            {
                InitForeach(content, startMethods, foreachSyntax, variableName, parentName);
                return;
            }

            List<string> childNames = new();
            int i = 0;
            foreach (var child in syntax.Slots)
            {
                var childName = $"{variableName}_{i}";
                childNames.Add(childName);
                InitComponent(content, startMethods, child, childName, variableName);
                i++;
            }

            var type = GetComponentName(syntax.Name);
            content.AppendLine($"{type} {variableName} = new {type}();");

            foreach (var property in syntax.Properties)
            {
                InitProperty(content, variableName, property.Name, property.Value, property.Type);
            }

            startMethods.Add($"{variableName}.Start(new List<Component> {{ {string.Join(",", childNames)} }});");
        }

        private static void InitForeach(ScriptBuilder content, List<string> startMethods, ForEachSyntax foreachSyntax, string variableName, string parentName)
        {
            var foreachContent = new ScriptBuilder();
            var foreachStartMethods = new List<string>();

            content.AppendLine($"ForEach {variableName} = new {GetComponentName(foreachSyntax.Name)}();");

            foreachContent.AppendLineNoIndent($"Func<object, List<Component>> {variableName}_foreach = ({foreachSyntax.Item}) => {{");
            foreachContent.Indent();


            List<string> childNames = new();
            int i = 0;
            foreach (var child in foreachSyntax.Slots)
            {
                var childName = $"{variableName}_{i}";
                childNames.Add(childName);
                InitComponent(foreachContent, foreachStartMethods, child, $"{variableName}_{i}", variableName);
                i++;
            }

            foreach (var childName in childNames)
                foreachContent.AppendLine($"{childName}.Parent = {parentName};");

            AddStartMethods(foreachContent, foreachStartMethods);

            foreachContent.AppendLine($"return new List<Component> {{ {string.Join(",", childNames)} }};");
            foreachContent.Unindent();
            foreachContent.AppendLine("};");

            InitProperty(content, variableName, "Items", foreachSyntax.Items, PropertyType.OneWay);

            foreachContent.AppendLine($"{variableName}.Start(new List<Component> {{ }});");
            foreachContent.AppendLine($"{variableName}.ComponentsCreator = {variableName}_foreach;");
            startMethods.Add(foreachContent.ToString());
        }

        private static void InitProperty(ScriptBuilder content, string variableName, string name, string value, PropertyType propertyType)
        {
            if (propertyType == PropertyType.TwoWay)
            {
                content.AppendLine($"{variableName}.{name} = {variableName}.{name}.ToTwoWayProperty(() => {value}, __value => {value} = __value);");
            }
            else if (propertyType == PropertyType.OneWay)
            {
                content.AppendLine($"{variableName}.{name} = {variableName}.{name}.ToOneWayProperty(() => {value});");
            }
            else
            {
                content.AppendLine($"{variableName}.{name} = {variableName}.{name}.ToValueProperty(\"{value}\");");
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

        private static bool IsBinding(string value) => value.StartsWith("{") && value.EndsWith("}");
        private static bool IsTwoWayBinding(string value) => value.StartsWith("{bind ") && value.EndsWith("}");
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
