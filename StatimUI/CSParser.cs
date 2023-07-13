using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public static class CSParser
    {
        public static TypeSyntax? FindVariableType(this SyntaxTree tree, string variableName)
        {
            var nodes = tree.GetRoot().DescendantNodes();
            var fields = nodes.OfType<FieldDeclarationSyntax>();
            var properties = nodes.OfType<PropertyDeclarationSyntax>();

            foreach (var field in fields)
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    if (variable.Identifier.Text == variableName)
                        return field.Declaration.Type;
                }
            }

            foreach (var property in properties)
            {
                if (property.Identifier.Text == variableName)
                    return property.Type;
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
