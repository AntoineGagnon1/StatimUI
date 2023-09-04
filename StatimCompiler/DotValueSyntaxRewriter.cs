using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimCodeGenerator
{
    internal class DotValueSyntaxRewriter : CSharpSyntaxRewriter
    {
        public HashSet<string> PropertyNames;

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (!PropertyNames.Contains(node.Identifier.Text) || node.Parent == null)
                return node;

            if (HasForbiddenParent(node))
                return node;

            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node, SyntaxFactory.IdentifierName("Value"));
        }

        private static bool HasForbiddenParent(IdentifierNameSyntax node)
        {
            var parent = node.Parent;
            Type type = parent!.GetType();

            if (parent is MemberAccessExpressionSyntax memberAccess)
            {
                return !IsFirstInMemberAccess(memberAccess, node);
            }
            else
            {
                return type.IsSubclassOf(typeof(TypeSyntax)) || type.IsSubclassOf(typeof(BaseTypeSyntax)) ||
                       type.IsSubclassOf(typeof(BaseTypeDeclarationSyntax)) || type == typeof(TypeOfExpressionSyntax) ||
                       type == typeof(MethodDeclarationSyntax) || type == typeof(ObjectCreationExpressionSyntax);

            }
        }

        private static bool IsFirstInMemberAccess(MemberAccessExpressionSyntax parent, SyntaxNode node)
        {
            var children = parent.ChildNodes().ToList();
            if (node == children[0])
            {
                if (parent.Parent is MemberAccessExpressionSyntax memberAccess)
                    return IsFirstInMemberAccess(memberAccess, parent);

                return true;
            }

            return false;
        }

        public DotValueSyntaxRewriter(HashSet<string> propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}
