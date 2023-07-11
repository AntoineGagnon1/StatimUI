using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public abstract class Component
    {
        public float Width { get; set; }
        public bool IsWidthFixed { get; set; }

        public float Height { get; set; }
        public bool IsHeightFixed { get; set; }

        public PointF Position { get; set; }

        abstract public void Render();

        virtual public bool HasChanged { get; protected set; }
    }
}
