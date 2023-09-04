using System;
using System.Collections.Generic;
using System.Text;

namespace StatimCodeGenerator
{
    public struct ComponentDefinition
    {
        public string TypeName;
        public bool DashCase;

        public ComponentDefinition(string typeName, bool dashCase)
        {
            TypeName = typeName;
            DashCase = dashCase;
        }
    }
}
