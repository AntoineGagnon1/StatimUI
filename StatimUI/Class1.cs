using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    public class Test : IAnimation<Component>
    {
        public void Update(Component component, float t)
        {
            Console.WriteLine("T: " + t);
            component.Scale.Value.X = t;
        }
    }
}
