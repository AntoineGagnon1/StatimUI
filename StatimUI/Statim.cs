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

namespace StatimUI
{
    public static class Statim
    {
        public const string FileExtension = ".statim";

        public static void LoadEmbedded()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine(assemblyFolder);
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

            Compile(trees);
        }

        public static Component? CreateComponent(string name)
        {
            // TODO : Cache ?
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(name);
                if (type != null)
                {
                    return (Component)Activator.CreateInstance(type);
                }
            }

            return null;
        }

        private static void Compile(List<SyntaxTree> trees)
        {
            using MemoryStream dllStream = new MemoryStream();
            using MemoryStream pdbStream = new MemoryStream();
            Stopwatch watch = Stopwatch.StartNew();
            var res = CSharpCompilation.Create("StatimUIXmlComponents", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Statim).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)
                ).AddReferences(GetGlobalReferences()).Emit(dllStream, pdbStream);

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            foreach (var error in res.Diagnostics)
            {
                Console.WriteLine(error);
            }

            // TODO : check res for errors
            var assembly = Assembly.Load(dllStream.ToArray(), pdbStream.ToArray());
        }

        private static IEnumerable<MetadataReference> GetGlobalReferences()
        {
            var returnList = new List<MetadataReference>();

            //The location of the .NET assemblies
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            /* 
                * Adding some necessary .NET assemblies
                * These assemblies couldn't be loaded correctly via the same construction as above,
                * in specific the System.Runtime.
                */
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            return returnList;
        }
    }
}
