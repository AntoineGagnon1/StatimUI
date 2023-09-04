using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatimCodeGenerator
{
    internal class DotValueSyntaxRewriter : CSharpSyntaxRewriter
    {
        private ComponentProperties componentProperties;
        private string name;

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is MemberAccessExpressionSyntax parent)
            {
                if (node == parent.Expression && componentProperties.ContainsProperty(name, node.Identifier.Text))
                    return AddDotValue(node);

                // node == parent.Name
                if (parent.Expression is IdentifierNameSyntax leftName && componentProperties.ContainsProperty(leftName.Identifier.Text, node.Identifier.Text))
                    return AddDotValue(node);

                return node;
            }

            if (HasForbiddenParent(node) || !componentProperties.ContainsProperty(name, node.Identifier.Text))
                return node;

            return AddDotValue(node);
        }

        private static MemberAccessExpressionSyntax AddDotValue(ExpressionSyntax expression) => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, SyntaxFactory.IdentifierName("Value"));

        private static bool HasForbiddenParent(IdentifierNameSyntax node)
        {
            var parent = node.Parent;
            Type type = parent!.GetType();

           return type.IsSubclassOf(typeof(TypeSyntax)) || type.IsSubclassOf(typeof(BaseTypeSyntax)) ||
                  type.IsSubclassOf(typeof(BaseTypeDeclarationSyntax)) || type == typeof(TypeOfExpressionSyntax) ||
                  type == typeof(MethodDeclarationSyntax) || type == typeof(ObjectCreationExpressionSyntax);

        }

        private static bool IsFirstInMemberAccess(MemberAccessExpressionSyntax parent, SyntaxNode node, string parentName)
        {
            var children = parent.ChildNodes().ToList();
            if (children[0] == node)
                return true;

            if (children[0] is IdentifierNameSyntax identifier)
            {
                if (identifier.Identifier.Text == parentName)
                {
                    if (parent.Parent is MemberAccessExpressionSyntax memberAccess)
                    {
                        return IsFirstInMemberAccess(memberAccess, parent, parentName);
                    }
                }

                return false;
            }
            
            return false;
        }

        public DotValueSyntaxRewriter(ComponentProperties _componentProperties, string _name)
        {
            componentProperties = _componentProperties;
            name = _name;
        }
    }
}
