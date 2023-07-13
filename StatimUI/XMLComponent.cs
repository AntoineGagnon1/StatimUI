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
        public List<string> Bindings { get; private set; }

        public XmlClassTemplate(Type? classType, string? xmlContent, List<string> bindings)
        {
            ClassType = classType;
            XmlContent = xmlContent;
            Bindings = bindings;
        }
    }


    public class XMLComponent : Component
    {
        public Component? Child { get; private set; }

        public object? ClassInstance { get; private set; }

        private Dictionary<string, (object get, object set)> Bindings = new();

        public override void Update()
        {
            Child?.Update();
        }

        public void GetBinding(string name, out object? getter, out object? setter)
        {
            if (Bindings.TryGetValue(name, out (object get, object set) binding))
            {
                getter = binding.get;
                setter = binding.set;
            }
            else
            {
                getter = null;
                setter = null;
            }
        }

        private XMLComponent(XmlClassTemplate template)
        {
            // Create the class instance
            if (template.ClassType is not null)
            {
                ClassInstance = Activator.CreateInstance(template.ClassType);
                
                // Get the bindings
                foreach(string binding in template.Bindings)
                {
                    MethodInfo? get = template.ClassType.GetMethod(BindingGetterName(binding));
                    MethodInfo? set = template.ClassType.GetMethod(BindingSetterName(binding));

                    if (get is not null && set is not null)
                    {
                        Bindings.Add(binding, (
                           (Func<object>)(() => get.Invoke(ClassInstance, null)), 
                           (Action<object>)((object value) => set.Invoke(ClassInstance, new object[]{ value })
                        )));
                    }
                    else
                    {
                        // TODO : error something
                    }
                }
            }


            // Load the child
            // TODO : Cache this
            if (template.XmlContent is not null)
            {
                Child = XMLParser.ParseElement(this, XElement.Parse(template.XmlContent));
            }
        }

        public static XMLComponent? Create(string name)
        {
            if (XMLComponentByName.TryGetValue(name, out var template))
            {
                return new XMLComponent(template);
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, XmlClassTemplate> XMLComponentByName { get; } = new();

        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

        static XMLComponent()
        {
            List<(string name, string? xmlContent, List<string> bindings)> components = new();
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

                List<string> bindings = new();
                if (scriptContent != null && !string.IsNullOrWhiteSpace(scriptContent))
                {
                    StringBuilder script = new StringBuilder();
                    script.Append($"namespace StatimUIXmlComponents {{\n public class {name} {{\n");
                    script.Append(scriptContent); // Add the content of the <script> tag

                    // Generate the binding functions
                    if (!string.IsNullOrWhiteSpace(xmlContent))
                    {
                        var element = XElement.Parse(xmlContent);
                        foreach (var attr in element.Attributes())
                        {
                            if(XMLParser.IsBinding(attr.Value))
                            {
                                bindings.Add(attr.Name.LocalName);

                                // TODO : change string to the actual type :
                                script.Append($"public string {BindingGetterName(attr.Name.LocalName)}() => {XMLParser.GetBindingContent(attr.Value)};\n");
                                script.Append($"public void {BindingSetterName(attr.Name.LocalName)}(string __value) => {XMLParser.GetBindingContent(attr.Value)} = __value;\n");
                            }
                        }
                    }

                    script.Append("\n}\n}"); // Close the class and the namespace
                    Console.Write(script.ToString());
                    trees.Add(CSharpSyntaxTree.ParseText(script.ToString())); // TODO : catch compilation errors
                }

                components.Add((name, xmlContent, bindings));
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
            foreach((string name, string? content, var bindings) in components)
            {
                XMLComponentByName.Add(name, new XmlClassTemplate(assembly.GetType("StatimUIXmlComponents." + name), content, bindings));
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

        private static string BindingGetterName(string attributeName) => $"__{attributeName}Get";
        private static string BindingSetterName(string attributeName) => $"__{attributeName}Set";
    }
}
