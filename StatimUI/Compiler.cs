using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using StatimCodeGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace StatimUI
{
    public class Compiler
    {
        public Assembly? Assembly { get; private set; }
        private CodeGenerator? codeGenerator;

        public static IEnumerable<MetadataReference> GetGlobalReferences()
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
            yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"));
        }

        public void LoadEmbedded()
        {
            List<SyntaxTree> trees = new List<SyntaxTree>();
            codeGenerator = new CodeGenerator(GetComponentDefinitions());
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
                    var parts = name.Split('.');
                    var tree = codeGenerator.GenerateTree(parts[parts.Length - 2], stream);
                    trees.Add(tree);
                }
            }

            foreach (var tree in trees)
                Console.WriteLine(tree);

            Assembly =  Compile(trees, GetGlobalReferences());
        }

        public Assembly Compile(List<SyntaxTree> trees, IEnumerable<MetadataReference> references)
        {
            using MemoryStream dllStream = new MemoryStream();
            using MemoryStream pdbStream = new MemoryStream();
            var res = CSharpCompilation.Create("StatimUIXmlComponents", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees)
                .AddReferences(references)
                .Emit(dllStream, pdbStream);

            foreach (var error in res.Diagnostics)
            {
                Console.WriteLine(error);
            }

            return Assembly.Load(dllStream.ToArray(), pdbStream.ToArray());
        }

        public Component? CreateComponent(string name)
        {
            if (codeGenerator.ComponentDefinitions.TryGetValue(name, out var definition))
                name = definition.TypeName;

            var type  = Assembly?.GetType("StatimUIXmlComponents." + name);

            if (type == null)
                return null;

            var instance = Activator.CreateInstance(type) as Component;
            instance?.Start(new List<Component>());
            return instance;
        }

        private static Dictionary<string, ComponentDefinition> GetComponentDefinitions()
        {
            var definitions = new Dictionary<string, ComponentDefinition>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Component)))
                    {
                        var nameAttr = type.GetCustomAttribute<ComponentAttribute>();
                        if (nameAttr != null)
                            definitions.Add(nameAttr.TagName, new(type.Name, nameAttr.DashCase));
                    }
                }
            }
            return definitions;
        }
    }
}
