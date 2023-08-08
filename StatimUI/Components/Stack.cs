using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    public enum StackDirection
    {
        Vertical, VerticalReverse, Horizontal, HorizontalReverse
    }

    public enum StackAlign
    {
        Start, End, Center, SpaceBetween, SpaceAround, SpaceEvenly
    }

    public enum StackCrossAlign
    {
        Start, End, Center
    }

    [Component("stack")]
    public class Stack : Component
    {
        public Property<StackDirection> Direction = new ValueProperty<StackDirection>(StackDirection.Horizontal);
        public Property<StackAlign> Align = new ValueProperty<StackAlign>(StackAlign.Start);
        public Property<StackCrossAlign> CrossAlign = new ValueProperty<StackCrossAlign>(StackCrossAlign.Start);
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
            float total = 0.0f;
            var direction = Direction.Value;

            if (direction == StackDirection.Vertical)
            {
                foreach (var child in Children)
                {
                    child.Position.Value = new(child.Position.Value.X, total);
                    total += child.TotalPixelHeight;

                    var childSize = child.TotalPixelWidth + Padding.Value.Left + Padding.Value.Right;
                    if (childSize > Width.Value.Scalar && Width.Value.Unit == DimensionUnit.Auto)
                        Width.Value.Scalar = childSize;
                }
            }

            Height.Value.Scalar = total + Padding.Value.Top + Padding.Value.Bottom;

            return HasSizeChanged();
        }
    }
}
