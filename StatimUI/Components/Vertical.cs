﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("vertical")]
    public class Vertical : Component
    {
        private int lastChildrenCount = 0;

        public override void Render(Vector2 offset)
        {
            foreach(var child in Children)
            {
                child.Render(offset + Padding.Value.TopLeft);
            }
        }

        public override void Start(IList<Component> slots)
        {
            Console.WriteLine(Parent != null);
            Children.AddRange(slots);
        }

        public override bool Update()
        {
            bool updateLayout = false;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child.Update())
                    updateLayout = true;
            }

            if (updateLayout || lastChildrenCount != Children.Count)
            {
                lastChildrenCount = Children.Count;
                return UpdateLayout();
            }
            else
                return false;
        }

        private bool UpdateLayout()
        {
            float currHeight = 0.0f;
            foreach(var child in Children)
            {
                child.Position.Value = new (child.Position.Value.X, currHeight);
                currHeight += child.TotalPixelHeight;

                Width.Value.Scalar = Math.Max(child.TotalPixelWidth + Padding.Value.Left + Padding.Value.Right, Width.Value.Scalar);
            }

            Height.Value.Scalar = currHeight + Padding.Value.Top + Padding.Value.Bottom;

            return HasSizeChanged();
        }
    }
}
