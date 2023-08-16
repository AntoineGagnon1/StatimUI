using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    public interface IRenderingAdapter
    {
        public abstract void Render();
        public abstract void WindowResized(Vector2 size);

        public abstract void BindTexture(IntPtr texture);
        public abstract IntPtr MakeTexture();
        public abstract void SetTextureData(IntPtr texture, Size size, byte[] data);
        public abstract void FreeTexture(IntPtr ptr);
    }
}
