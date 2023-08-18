﻿using Microsoft.CodeAnalysis;
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
using Microsoft.CodeAnalysis.Text;
using System.Globalization;
using System.IO;

namespace StatimCodeGenerator
{
    public class CodeGenerator
    {
        public Dictionary<string, ComponentDefinition> ComponentDefinitions = new();

        public CodeGenerator(Dictionary<string, ComponentDefinition> componentDefinitions)
        {
            ComponentDefinitions = componentDefinitions;
        }

        public SyntaxTree GenerateTree(string name, string statimSyntax)
        {
            var tree = CSharpSyntaxTree.ParseText(GenerateClass(name, statimSyntax));

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
                        field.Declaration
                            .WithType(CreateGenericType("Property", field.Declaration.Type))
                            .WithVariables(variablesSeparated)
                    );
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
      
        private string GenerateClass(string name, string statimSyntax)
        {
            ScriptBuilder startContent = new();
            var parsingResult = Parser.Parse(statimSyntax);
            if (parsingResult.Root != null)
            {
                startContent.Indent(3);

                var startMethods = new List<string>();
                InitComponent(startContent, startMethods, parsingResult.Root, "__child", "this");
                AddStartMethods(startContent, startMethods);

                startContent.AppendLine("Children.Add(__child);");

                startContent.Unindent(3);
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

            Width.Value.Scalar = Children[0].TotalPixelWidth + Padding.Value.Horizontal;
            Height.Value.Scalar = Children[0].TotalPixelHeight + Padding.Value.Vertical;

            return HasSizeChanged();
        }}

        protected override void OnRender(System.Numerics.Vector2 offset) => Children[0].Render(offset);
        
        public override void Start(IList<Component> slots)
        {{
{startContent}
        }}

{parsingResult.Script?.Code}
    }}
}}";
        }

        private void InitComponent(ScriptBuilder content, List<string> startMethods, ComponentSyntax syntax, string variableName, string parentName)
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

            var definition = GetComponentDefinition(syntax.Name);
            content.AppendLine($"{definition.TypeName} {variableName} = new {definition.TypeName}();");

            foreach (var property in syntax.Properties)
            {
                InitProperty(
                    content, variableName,
                    definition.DashCase ? StringUtilities.DashToPascalCase(property.Name) : property.Name,
                    property.Value, property.Type);
            }

            startMethods.Add($"{variableName}.Start(new List<Component> {{ {string.Join(",", childNames)} }});");
        }

        private ComponentDefinition GetComponentDefinition(string name)
        {
            if (ComponentDefinitions.TryGetValue(name, out var definition))
                return definition;

            return new (name, false);
        }

        private void InitForeach(ScriptBuilder content, List<string> startMethods, ForEachSyntax foreachSyntax, string variableName, string parentName)
        {
            var foreachContent = new ScriptBuilder();
            var foreachStartMethods = new List<string>();
            
            content.AppendLine($"var {variableName} = CreateForEach({foreachSyntax.Items});");
            InitProperty(content, variableName, "Items", foreachSyntax.Items, PropertyType.Binding);

            foreachContent.AppendLineNoIndent($"{variableName}.ComponentsCreator = {foreachSyntax.Item} => {{");
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

            foreachContent.AppendLine($"{variableName}.Start(new List<Component> {{ }});");
            startMethods.Add(foreachContent.ToString());
        }

        private static void InitProperty(ScriptBuilder content, string variableName, string name, string value, PropertyType propertyType)
        {
            if (propertyType == PropertyType.Binding)
            {
                content.AppendLine($"{variableName}.{name} = {variableName}.{name}.ToBinding(() => {value});");
            }
            else
            {
                content.AppendLine($"{variableName}.{name} = {variableName}.{name}.ToValueProperty(\"{value}\");");
            }
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
