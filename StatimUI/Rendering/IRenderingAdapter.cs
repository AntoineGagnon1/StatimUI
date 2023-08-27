using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    public interface IRenderingAdapter
    {
        public abstract void CreateSubWindow(Panel window);
        public abstract void DestroySubWindow(Panel window);

        public abstract void BindTexture(IntPtr texture);
        public abstract IntPtr MakeTexture();
        public abstract void SetTextureData(IntPtr texture, Size size, byte[] data);
        public abstract void FreeTexture(IntPtr ptr);
    }
}
