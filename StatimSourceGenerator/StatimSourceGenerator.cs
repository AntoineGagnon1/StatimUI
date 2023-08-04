using Microsoft.CodeAnalysis;
using StatimUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace StatimSourceGenerator
{
    [Generator]
    public class StatimSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var file in context.AdditionalFiles)
            {
                var name = Path.GetFileNameWithoutExtension(file.Path);
                context.AddSource($"{name}.g.cs", CodeGenerator.Parse(name, new MemoryStream(Encoding.UTF8.GetBytes(file.GetText().ToString()))).ToString());
            }
            context.AddSource("test.g.cs", "/* allo */");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Debugger.Launch();
        }
    }
}
