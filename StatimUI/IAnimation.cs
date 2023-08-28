using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI
{
    public interface IAnimation<T> where T : Component
    {
        public void Update(T component, float t);
    }
}
