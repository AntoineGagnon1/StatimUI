using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using StatimUI.Components;
using System.Collections;
using System.Numerics;

namespace StatimUI
{
    public static class Statim
    {
        public const string FileExtension = ".statim";

        private static Assembly xmlAssembly;

        public static void LoadEmbedded()
        {
            List<SyntaxTree> trees = new List<SyntaxTree>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var names = assembly.GetManifestResourceNames();
                foreach (string name in names)
                {
                    string? extension = Path.GetExtension(name);
                    if (extension == null || extension != Statim.FileExtension)
                        continue;

                    Stream? stream = assembly.GetManifestResourceStream(name);
                    if (stream == null)
                        continue;

                    trees.Add(Parser.Parse(name.Split('.')[^2], stream));
                }
            }

            foreach (var tree in trees)
                Console.WriteLine(tree);

            Compile(trees);
        }

        public static Component? CreateComponent(string name)
        {
            if (!Component.ComponentByName.TryGetValue(name, out var type))
            {
                type = xmlAssembly.GetType("StatimUIXmlComponents." + name);
            }

            if (type == null)
                return null;
            
            var instance = Activator.CreateInstance(type) as Component;
            instance?.Start(new List<Component>());
            return instance;
        }

        private static void Compile(List<SyntaxTree> trees)
        {
            using MemoryStream dllStream = new MemoryStream();
            using MemoryStream pdbStream = new MemoryStream();
            var res = CSharpCompilation.Create("StatimUIXmlComponents", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees)
                .AddReferences(GetGlobalReferences())
                .Emit(dllStream, pdbStream);

            foreach (var error in res.Diagnostics)
            {
                Console.WriteLine(error);
            }

            xmlAssembly = Assembly.Load(dllStream.ToArray(), pdbStream.ToArray());
        }

        private static IEnumerable<MetadataReference> GetGlobalReferences()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            yield return MetadataReference.CreateFromFile(typeof(IList).Assembly.Location);
            yield return MetadataReference.CreateFromFile(typeof(Statim).Assembly.Location);
            yield return MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
            yield return MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location);
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll"));
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll"));
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll"));
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"));
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll"));
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Numerics.Vectors.dll"));
        }
    }
}
