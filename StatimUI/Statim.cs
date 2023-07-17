using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public static class Statim
    {
        public const string FileExtension = ".statim";

        public static void LoadEmbedded()
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

                    Load(name.Split('.')[^2], stream);
                }
            }
        }

        public static void Load(string name, Stream stream)
        {
            var preParse = XMLPreParse(stream);
        }

        record struct PreParseResult(string Script, string Child) { }

        private static PreParseResult XMLPreParse(Stream stream)
        {
            const string scriptStartTag = "<script>";
            const string scriptEndTag = "</script>";

            StringBuilder scriptContent = new ();
            StringBuilder content = new ();
            bool readingScript = false;

            using (StreamReader reader = new (stream))
            {
                string? line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(scriptStartTag))
                    {
                        readingScript = true;
                        line = line.Substring(line.IndexOf(scriptStartTag) + scriptStartTag.Length);
                    }
                    if (line.StartsWith(scriptEndTag)) // No else because <script> and </script> might be on the same line 
                    {
                        readingScript = false;
                        scriptContent.AppendLine(line.Substring(0, line.IndexOf(scriptEndTag)));
                        line = line.Substring(line.IndexOf(scriptEndTag) + scriptEndTag.Length);
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue; // Skip empty lines

                    if (readingScript)
                        scriptContent.AppendLine(line);
                    else
                        content.AppendLine(line);
                }
            }

            return new PreParseResult(scriptContent.ToString(), content.ToString());
        }
    }
}
