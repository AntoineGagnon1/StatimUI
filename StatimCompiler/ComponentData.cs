using System;
using System.Collections.Generic;
using System.Text;

namespace StatimCodeGenerator
{
    public struct ComponentData
    {
        public string Name;
        public string Code;

        public ComponentData(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
