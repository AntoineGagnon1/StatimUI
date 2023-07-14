using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    public class XMLComponent : Component
    {
        public Component? Child { get; private set; }
        public object? ClassInstance { get; private set; }
        public ReadOnlyDictionary<string, Binding> Bindings { get; private set; }

        public override void Update()
        {
            Child?.Update();
        }

        public override void InitBindingProperty(string name, Binding binding)
        {
            base.InitBindingProperty(name, binding);

            if (ClassInstance == null)
                return;

            InitBindingProperty(ClassInstance, name, binding);
        }

        public override void InitVariableProperty(string name, object value)
        {
            base.InitVariableProperty(name, value);

            if (ClassInstance == null)
                return;

            InitVariableProperty(ClassInstance, name, value);
        }

        private XMLComponent(XmlClassTemplate template)
        {
            // Create the class instance
            var bindings = new Dictionary<string , Binding>();
            if (template.ClassType is not null)
            {
                ClassInstance = Activator.CreateInstance(template.ClassType);
                
                // Get the bindings
                foreach(string bindingName in template.Bindings)
                {
                    var get = template.ClassType.GetMethod(BindingGetterName(bindingName))?.CreateDelegate<Func<object>>(ClassInstance);
                    var set = template.ClassType.GetMethod(BindingSetterName(bindingName))?.CreateDelegate<Action<object>>(ClassInstance);

                    if (get == null)
                        throw new Exception("Getter is null");

                    bindings.Add(bindingName, new Binding(get, set));
                }
            }
            Bindings = new ReadOnlyDictionary<string, Binding>(bindings);

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
                return new XMLComponent(template);

            return null;
        }

        public static Dictionary<string, XmlClassTemplate> XMLComponentByName { get; } = new();


        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
        static XMLComponent()
        {
            List<(string name, string? xmlContent, List<string> bindings)> components = new();
            List<SyntaxTree> trees = new();

            // Parse the xml data for each component type
            foreach (var component in GetXmlComponents())
            {
                var fragments = XMLParser.ParseFragment(XmlReader.Create(component.Stream, xmlSettings));

                string? scriptContent = null;
                string? xmlContent = null;
                foreach(var fragment in fragments)
                {
                    try
                    {
                        var reader = XElement.Parse(fragment.ToString());

                        if (reader.Name == "script")
                            scriptContent = WebUtility.HtmlDecode(String.Concat(reader.Nodes())); // Get the inner text
                        else if (xmlContent == null)
                            xmlContent = fragment.ToString(); // Get the inner text
                        else
                        { } // TODO : log error + abort this class ?    
                    }
                    catch (Exception e)
                    {
                    }
                }
                Console.WriteLine(scriptContent);
                List<string> bindings = new();
                if (scriptContent != null && !string.IsNullOrWhiteSpace(scriptContent))
                {
                    var scriptTree = CSharpSyntaxTree.ParseText(CSParser.CreateClassString(component.Name, scriptContent));
                    trees.Add(scriptTree);

                    // Generate the binding functions
                    if (!string.IsNullOrWhiteSpace(xmlContent))
                    {
                        var element = XElement.Parse(xmlContent);
                        StringBuilder tagsCode = new();
                        foreach (var attr in element.Attributes())
                        {
                            bindings.Add(attr.Name.LocalName);

                            if (XMLParser.IsTwoWay(attr.Value))
                            {
                                string variableName = XMLParser.GetTwoWayVariableName(attr.Value);
                                // TODO: get type of attribute since the variable's type isn't always gonna be in the tree ( ex: layer.name ) the layer is in the tree but not the name
                                var variableType = scriptTree.FindVariableType(variableName);
                                tagsCode.Append($"public object {BindingGetterName(attr.Name.LocalName)}() => {variableName};\n");
                                tagsCode.Append($"public void {BindingSetterName(attr.Name.LocalName)}(object __value)" +
                                "{" +
                                    $"{variableName} = ({variableType})System.Convert.ChangeType(__value, typeof({variableType}));" +
                                "}");
                            }
                            else
                            {
                                tagsCode.Append($"public object {BindingGetterName(attr.Name.LocalName)}() => {XMLParser.GetOneWayContent(attr.Value)};\n");
                            }
                        }

                        if (bindings.Count > 0)
                        {
                            var tagsTree = CSharpSyntaxTree.ParseText(CSParser.CreateClassString(component.Name, tagsCode.ToString()));
                            trees.Add(tagsTree);
                        }
                    }

                }

                components.Add((component.Name, xmlContent, bindings));
            }

            using MemoryStream dllStream = new MemoryStream();
            using MemoryStream pdbStream = new MemoryStream();
            Stopwatch watch = Stopwatch.StartNew();
            var res = CSharpCompilation.Create("StatimUIXmlComponents", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(trees.ToArray())
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Property).Assembly.Location)
                ) // add system dll
                .Emit(dllStream, pdbStream);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            foreach (var error in res.Diagnostics)
            {
                Console.WriteLine(error);
            }

            // TODO : check res for errors
            var assembly = Assembly.Load(dllStream.ToArray(), pdbStream.ToArray());
            
            // Create all the XmlClassTemplates
            foreach((string name, string? content, var bindings) in components)
            {
                XMLComponentByName.Add(name, new XmlClassTemplate(assembly.GetType("StatimUIXmlComponents." + name), content, bindings));
            }
        }

        private static IEnumerable<(string Name, Stream Stream)> GetXmlComponents()
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
