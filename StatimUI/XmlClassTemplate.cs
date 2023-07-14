using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
