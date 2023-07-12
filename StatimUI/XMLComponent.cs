using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StatimUI
{
    public class XmlClassTemplate
    {
        public Type? ClassType { get; private set; }
        public string? XmlContent { get; private set; }

        public XmlClassTemplate(Type? classType, string? xmlContent)
        {
            ClassType = classType;
            XmlContent = xmlContent;
        }
    }


    public class XMLComponent : Component
    {
        public Component? Child { get; private set; }

        public object? ClassInstance { get; private set; }

        public override void Update()
        {
            Child?.Update();
        }

        public static Dictionary<string, XmlClassTemplate> XMLComponentByName { get; } = new();

        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
        public static XMLComponent? Create(string name)
        {
            if (XMLComponentByName.TryGetValue(name, out var template))
            {
                XMLComponent c = new XMLComponent();

                // Create the class instance
                if (template.ClassType is not null)
                {
                    c.ClassInstance = Activator.CreateInstance(template.ClassType);
                }

                // Load the child
                // TODO : Cache this
                if (template.XmlContent is not null)
                {
                    c.Child = XMLParser.ParseElement(XElement.Parse(template.XmlContent));
                }
                return c;
            }
            else
            {
                return null;
            }
        }

        static XMLComponent()
        {
            List<(string name, string? xmlContent)> components = new();
            List<SyntaxTree> trees = new();

            // Parse the xml data for each component type
            foreach ((string name, Stream stream) in GetXmlComponents())
            {
                var fragments = XMLParser.ParseFragment(XmlReader.Create(stream, xmlSettings));

                string? scriptContent = null;
                string? xmlContent = null;
                foreach(var fragment in fragments)
                {
                    try
                    {
                        var reader = XElement.Parse(fragment.ToString());

                        if (reader.Name == "script")
                            scriptContent = String.Concat(reader.Nodes()); // Get the inner text
                        else if (xmlContent == null)
                            xmlContent = fragment.ToString(); // Get the inner text
                        else
                        { } // TODO : log error + abort this class ?    
                    }
                    catch (Exception) { }
                }

                if (scriptContent != null && !string.IsNullOrWhiteSpace(scriptContent))
                {
                    scriptContent = $"namespace StatimUIXmlComponents {{ public class {name} {{\n" + scriptContent + "}}";
                    trees.Add(CSharpSyntaxTree.ParseText(scriptContent)); // TODO : catch compilation errors
                }

                components.Add((name, xmlContent));
            }

            using MemoryStream dllStream = new MemoryStream();
            using MemoryStream pdbStream = new MemoryStream();
            var res = CSharpCompilation.Create("StatimUIXmlComponents", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees.ToArray())
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location)) // add system dll
                .Emit(dllStream, pdbStream);
            // TODO : check res for errors
            var assembly = Assembly.Load(dllStream.ToArray(), pdbStream.ToArray());

            // Create all the XmlClassTemplates
            foreach((string name, string? content) in components)
            {
                XMLComponentByName.Add(name, new XmlClassTemplate(assembly.GetType("StatimUIXmlComponents." + name), content));
            }
        }

        private static IEnumerable<(string, Stream)> GetXmlComponents()
        {
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
                    yield return (parts[parts.Length - 2], stream);
                }
            }
        }
    }
}
