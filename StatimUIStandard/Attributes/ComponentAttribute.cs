using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ComponentAttribute : Attribute
    {
        public string TagName { get; private set; }

        public ComponentAttribute(string tagName)
        {
            TagName = tagName;
        }
    }
}
