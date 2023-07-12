using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public static class CSParser
    {
        public static void ParseScript(string source)
        {
            var script = CSharpScript.Create(source);
            var root = script.GetCompilation().SyntaxTrees.Single().GetCompilationUnitRoot();
            foreach (var member in root.Members.OfType<FieldDeclarationSyntax>())
            {
                Console.WriteLine(member.Declaration.Type);
                foreach (var variable in member.Declaration.Variables)
                {
                    Console.WriteLine(variable.Identifier.ValueText);
                }
                Console.WriteLine(member);
            }
        }
    }
}
