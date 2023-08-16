using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI.Rendering
{
    public class RenderLayer
    {
        public List<RenderCommand> Commands { get; set; } = new ();

        internal void Clear()
        {
            Commands.Clear();
        }
    }
}
