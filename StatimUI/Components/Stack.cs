using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    public enum StackDirection
    {
        Vertical, Horizontal, VerticalReverse, HorizontalReverse
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
                child.Render(offset + DrawPosition);
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

            if (updateLayout || lastChildrenCount != Children.Count || true)
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
            foreach (var child in Children)
            {
                child.Position.Value = new(child.Position.Value.X, currHeight);
                currHeight += child.TotalPixelHeight;

                Width.Value = Width.Value.WithScalar(Math.Max(child.TotalPixelWidth + Padding.Value.Left + Padding.Value.Right, Width.Value.Scalar));
            }

            if (Height.Value.Unit == DimensionUnit.Auto)
                Height.Value = Height.Value.WithScalar(currHeight + Padding.Value.Top + Padding.Value.Bottom);

            return HasSizeChanged();
        }

        private Vector2 GetNewLayoutPos(Component child, StackDirection direction, Vector2 pos)
        {
            if (direction == StackDirection.Vertical)
                pos.Y += child.TotalPixelHeight;
            else if (direction == StackDirection.VerticalReverse)
                pos.Y -= child.TotalPixelHeight;
            else if (direction == StackDirection.Horizontal)
                pos.X += child.TotalPixelWidth;
            else if (direction == StackDirection.HorizontalReverse)
                pos.X -= child.TotalPixelWidth;

            return pos;
        }

        private Vector2 GetChildPos(Component child, Vector2 pos,SubstractSize subWidth, SubstractSize subHeight)
        {
            if (subWidth == SubstractSize.Full)
                pos.X -= child.TotalPixelWidth;
            else if (subWidth == SubstractSize.Half)
                pos.X -= child.TotalPixelWidth * 0.5f;

            if (subHeight == SubstractSize.Full)
                pos.Y -= child.TotalPixelHeight;
            else if (subHeight == SubstractSize.Half)
                pos.Y -= child.TotalPixelHeight * 0.5f;

            return pos;
        }

        private enum SubstractSize
        {
            None, Half, Full
        }
        private struct Layout
        {
            public Vector2 Pos;
            public IEnumerable<Component> Children;
            public SubstractSize SubstractWidth;
            public SubstractSize SubstractHeight;
            public StackDirection Direction;

            public Layout(Vector2 pos, IEnumerable<Component> children, StackDirection direction, SubstractSize substractWidth = SubstractSize.None, SubstractSize substractHeight = SubstractSize.None)
            {
                Pos = pos;
                Children = children;
                Direction = direction;
                SubstractWidth = substractWidth;
                SubstractHeight = substractHeight;

            }
        }
        private Layout GetLayout()
        {
            var align = Align.Value;
            var crossAlign = CrossAlign.Value;
            var direction = Direction.Value;
            var layout = new Layout(Position.Value + Padding.Value.TopLeft, Children, StackDirection.Horizontal);

            float innerWidth = PixelWidth - Padding.Value.Horizontal;
            float innerHeight = PixelHeight - Padding.Value.Vertical;
            if (direction == StackDirection.Vertical)
            {
                if (align == StackAlign.Start)
                {
                    layout.Direction = StackDirection.Vertical;
                }
                if (align == StackAlign.Center)
                {
                    layout.Pos.Y += innerHeight * 0.5f - GetTotalChildrenHeight() * 0.5f;
                    layout.SubstractHeight = SubstractSize.None;
                    layout.Direction = StackDirection.Vertical;
                }
                else if (align == StackAlign.End)
                {
                    layout.Pos.Y += innerHeight;
                    layout.Children = Children.ReverseList();
                    layout.Direction = StackDirection.VerticalReverse;
                    layout.SubstractHeight = SubstractSize.Full;
                }

                // nothing to do with StackCrossAlign.Start
                if (crossAlign == StackCrossAlign.Center)
                {
                    layout.Pos.X += innerWidth / 2f;
                    layout.SubstractWidth = SubstractSize.Half;
                }
                else if (crossAlign == StackCrossAlign.End)
                {
                    layout.Pos.X += innerWidth;
                    layout.SubstractWidth = SubstractSize.Full;
                }
                return layout;
            }
            throw new Exception("a");
        }

        private float GetTotalChildrenWidth()
        {
            var total = 0f;
            foreach (var child in Children)
                total += child.TotalPixelWidth;
            return total;
        }

        private float GetTotalChildrenHeight()
        {
            var total = 0f;
            foreach (var child in Children)
                total += child.TotalPixelHeight;
            return total;
        }
    }
}
