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
    internal class DotValueSyntaxRewriter : CSharpSyntaxRewriter
    {
        public HashSet<string> PropertyNames;
        private Property<string> Property;

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (!PropertyNames.Contains(node.Identifier.Text) || node.Parent == null)
                return node;
            var a = new String(String.Empty);
            Type type = node.Parent.GetType();

            if (IsForbiddenParent(type))
                return node;

            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node, SyntaxFactory.IdentifierName("Value"));
        }

        private static bool IsForbiddenParent(Type type)
        {
            return type.IsSubclassOf(typeof(TypeSyntax)) || type.IsSubclassOf(typeof(BaseTypeSyntax)) ||
                   type.IsSubclassOf(typeof(BaseTypeDeclarationSyntax)) || type == typeof(TypeOfExpressionSyntax) ||
                   type == typeof(MethodDeclarationSyntax) || type == typeof(ObjectCreationExpressionSyntax);
        }

        public DotValueSyntaxRewriter(HashSet<string> propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}
