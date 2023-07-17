using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            Dictionary<string, ComponentData> components = GetComponentsData();
            List<SyntaxTree> trees = new();

            foreach (var component in components)
            {
                if (component.Value.ScriptTree != null)
                    trees.Add(component.Value.ScriptTree);

                var tagsTree = GetBindings(component.Key, component.Value, components);
                if (tagsTree != null)
                    trees.Add(tagsTree);
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
            foreach(var component in components)
            {
                XMLComponentByName.Add(component.Key, new XmlClassTemplate(assembly.GetType("StatimUIXmlComponents." + component.Key), component.Value?.XmlContent, component.Value.Bindings));
            }
        }

        private static SyntaxTree? GetBindings(string name, ComponentData data, Dictionary<string, ComponentData> components)
        {
            if (data.XmlElement != null)
            {
                StringBuilder tagsCode = new();
                foreach (var attr in data.XmlElement.Attributes())
                {
                    data.Bindings.Add(attr.Name.LocalName);

                    if (XMLParser.IsTwoWay(attr.Value))
                    {
                        string variableName = XMLParser.GetTwoWayVariableName(attr.Value);
                        string? childName = data.XmlElement.Name.LocalName;
                        string? variableType = null;
                        if (components.TryGetValue(childName, out var value))
                            variableType = value.ScriptTree!.FindVariableType(attr.Name.LocalName);
                        else
                            variableType = ComponentByName[childName].FindVariableType(attr.Name.LocalName);

                        if (variableType == null)
                            throw new Exception("Couldn't find variable type");

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

                if (data.Bindings.Count > 0)
                {
                    var tagsTree = CSharpSyntaxTree.ParseText(CSParser.CreateClassString(name, tagsCode.ToString()));
                    return tagsTree;
                }
            }
            return null;
        }

        private static Dictionary<string, ComponentData> GetComponentsData()
        {
            Dictionary<string, ComponentData> result = new();
            // Parse the xml data for each component type
            //foreach (var component in GetXmlComponents())
            //{
            //    ComponentData data = new();
            //    var fragments = XMLParser.ParseFragment(XmlReader.Create(component.Stream, xmlSettings));
            //    var root = new XElement("root", fragments);

            //    foreach (var element in root.Elements())
            //    {
            //        try
            //        {
            //            if (element.Name == "script")
            //            {
            //                var scriptContent = element.Value;
            //                if (scriptContent != null && !string.IsNullOrWhiteSpace(scriptContent))
            //                    data.ScriptTree = CSharpSyntaxTree.ParseText(CSParser.CreateClassString(component.Name, scriptContent));
            //            }
            //            else if (data.XmlElement == null)
            //            {
            //                data.XmlElement = element;
            //                data.XmlContent = element.ToString();
            //            }
            //            else
            //            { } // TODO : log error + abort this class ?    
            //        } catch (Exception e) { }
            //    }

            //    result.Add(component.Name, data);
            //}
            return result;
        }

        private static string BindingGetterName(string attributeName) => $"__{attributeName}Get";
        private static string BindingSetterName(string attributeName) => $"__{attributeName}Set";
    }

    public class ComponentData
    {
        public SyntaxTree? ScriptTree { get; set; }
        public string? XmlContent { get; set; }
        public XElement? XmlElement { get; set; }
        public List<string> Bindings { get; set; } = new();
    }
}
