using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StatimUI
{
    public static class CSParser
    {
        public static string? FindVariableType(this SyntaxTree tree, string variableName)
        {
            var nodes = tree.GetRoot().DescendantNodes();
            var fields = nodes.OfType<FieldDeclarationSyntax>();
            var properties = nodes.OfType<PropertyDeclarationSyntax>();

            foreach (var field in fields)
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    if (variable.Identifier.Text == variableName && field.Declaration.Type is GenericNameSyntax genericType)
                        return genericType.ToString();
                }
            }

            foreach (var property in properties)
            {
                if (property.Identifier.Text == variableName && property.Type is GenericNameSyntax genericType)
                    return genericType.ToString();
            }

            return null;
        }

        public static string? FindVariableType(this Type type, string variableName)
        {
            foreach (var field in type.GetFields())
            {
                if (field.Name == variableName && field.FieldType == typeof(Property<>))
                    return field.FieldType.GenericTypeArguments[0].FullName;
            }

            foreach (var property in type.GetProperties())
            {
                if (property.Name == variableName)
                    return property.PropertyType.GenericTypeArguments[0].FullName;
            }

            return null;
        }

        public static string CreateClassString(string name, string content)
        {
            return @$"
            namespace StatimUIXmlComponents
            {{ 
                public partial class {name}
                {{
                    {content}
                }}
            }}";
        }
    }
}
