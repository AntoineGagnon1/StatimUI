using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    public interface IRenderingAdapter
    {
        public abstract void Render();
        public abstract void WindowResized(Vector2 size);
    }
}
