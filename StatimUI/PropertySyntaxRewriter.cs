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
    internal class PropertySyntaxRewriter : CSharpSyntaxRewriter
    {
        /*public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (!node.Modifiers.ToString().Contains("public"))
                return node;


            TypeSyntax variablePropertyType = SyntaxFactory.IdentifierName($"VariableProperty<{node.Type}>");

            if (node.Initializer != null)
            {
                var arguments = new List<ArgumentSyntax>()
                {
                    SyntaxFactory.Argument(node.Initializer.Value)
                };

                var argumentsSeparated = SyntaxFactory.SeparatedList(arguments);
                var objectCreationExpression = SyntaxFactory.ObjectCreationExpression(variablePropertyType, SyntaxFactory.ArgumentList(argumentsSeparated), null);
                var equalsValueClause = SyntaxFactory.EqualsValueClause(objectCreationExpression);
                node = node.WithInitializer(equalsValueClause);
            }

            return node.WithType(SyntaxFactory.IdentifierName($"Property<{node.Type}>"));
        }*/

        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // TODO: this is a hack
            if (!node.Modifiers.ToString().Contains("public"))
                return node;

            TypeSyntax variablePropertyType = SyntaxFactory.IdentifierName($"ValueProperty<{node.Declaration.Type}>");
            var variables = new List<VariableDeclaratorSyntax>();

            foreach (var variable in node.Declaration.Variables)
            {
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
            return node.WithDeclaration(node.Declaration.WithType(SyntaxFactory.IdentifierName($"Property<{node.Declaration.Type}>")).WithVariables(variablesSeparated));
        }
    }
}
