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

            if (updateLayout || lastChildrenCount != Children.Count)
            {
                lastChildrenCount = Children.Count;
                return UpdateLayout();
            }

            return false;
        }
        private bool UpdateLayout()
        {
            var layout = GetLayout();

            foreach (var child in layout.Children)
            {
                child.Position.Value = GetChildPos(child, layout.Pos, layout.SubstractWidth, layout.SubstractHeight);
                layout.Pos = GetNewLayoutPos(child, layout.Direction, layout.Pos, layout.Spacing);

                ResizeCrossAxis(child, layout.Direction);
            }

            ResizedMainAxis(layout.Pos, layout.Direction);

            return HasSizeChanged();
        }

        private void ResizedMainAxis(Vector2 pos, StackDirection direction)
        {
            if (direction == StackDirection.Vertical || direction == StackDirection.VerticalReverse)
                Height.Value = Height.Value.WithScalar(pos.Y + Padding.Value.Vertical);
            else
                Width.Value = Width.Value.WithScalar(pos.X + Padding.Value.Horizontal);
        }

        private void ResizeCrossAxis(Component child, StackDirection direction)
        {
            if (direction == StackDirection.Vertical || direction == StackDirection.VerticalReverse)
                Width.Value = Width.Value.WithScalar(Math.Max(child.TotalPixelWidth + Padding.Value.Horizontal, Width.Value.Scalar));
            else
                Height.Value = Height.Value.WithScalar(Math.Max(child.TotalPixelHeight + Padding.Value.Vertical, Height.Value.Scalar));
        }

        private Vector2 GetNewLayoutPos(Component child, StackDirection direction, Vector2 pos, float spacing)
        {
            if (direction == StackDirection.Vertical)
                pos.Y += child.TotalPixelHeight + spacing;
            else if (direction == StackDirection.VerticalReverse)
                pos.Y -= child.TotalPixelHeight + spacing;
            else if (direction == StackDirection.Horizontal)
                pos.X += child.TotalPixelWidth + spacing;
            else if (direction == StackDirection.HorizontalReverse)
                pos.X -= child.TotalPixelWidth + spacing;

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
            public float Spacing;

            public Layout(Vector2 pos, IEnumerable<Component> children, StackDirection direction, SubstractSize substractWidth = SubstractSize.None, SubstractSize substractHeight = SubstractSize.None, float spacing = 0)
            {
                Pos = pos;
                Children = children;
                Direction = direction;
                SubstractWidth = substractWidth;
                SubstractHeight = substractHeight;
                Spacing = 0;
            }
        }
        private Layout GetLayout()
        {
            var align = Align.Value;
            var crossAlign = CrossAlign.Value;
            var direction = Direction.Value;
            var layout = new Layout(new Vector2(0f), Children, StackDirection.Horizontal);

            float innerWidth = PixelWidth - Padding.Value.Horizontal;
            float innerHeight = PixelHeight - Padding.Value.Vertical;
            if (direction == StackDirection.Vertical)
            {
                if (align == StackAlign.Start)
                {
                    layout.Direction = StackDirection.Vertical;
                }
                else if (align == StackAlign.Center)
                {
                    layout.Direction = StackDirection.Vertical;
                    layout.Pos.Y = innerHeight * 0.5f - GetTotalChildrenHeight() * 0.5f;
                }
                else if (align == StackAlign.End)
                {
                    layout.Direction = StackDirection.VerticalReverse;
                    layout.Pos.Y = innerHeight;
                    layout.SubstractHeight = SubstractSize.Full;
                    layout.Children = Children.ReverseList();
                }
                else if (align == StackAlign.SpaceBetween)
                {
                    layout.Direction = StackDirection.Vertical;
                    layout.Spacing = (innerHeight - GetTotalChildrenHeight()) / (float)(Children.Count - 1);
                }
                else if (align == StackAlign.SpaceAround)
                {
                    layout.Direction = StackDirection.Vertical;
                    layout.Spacing = (innerHeight - GetTotalChildrenHeight()) / (float)Children.Count;
                    layout.Pos.Y = layout.Spacing / 2f;
                }
                else if (align == StackAlign.SpaceEvenly)
                {
                    layout.Direction = StackDirection.Vertical;
                    layout.Spacing = (innerHeight - GetTotalChildrenHeight()) / (float)(Children.Count + 1);
                    layout.Pos.Y = layout.Spacing;
                }

                // nothing to do with StackCrossAlign.Start
                if (crossAlign == StackCrossAlign.Center)
                {
                    layout.Pos.X = innerWidth * 0.5f;
                    layout.SubstractWidth = SubstractSize.Half;
                }
                else if (crossAlign == StackCrossAlign.End)
                {
                    layout.Pos.X = innerWidth;
                    layout.SubstractWidth = SubstractSize.Full;
                }
            }
            else if (direction == StackDirection.Horizontal)
            {
                if (align == StackAlign.Start)
                {
                    layout.Direction = StackDirection.Horizontal;
                }
                else if (align == StackAlign.Center)
                {
                    layout.Direction = StackDirection.Horizontal;
                    layout.Pos.X = innerWidth * 0.5f - GetTotalChildrenWidth() * 0.5f;
                }
                else if (align == StackAlign.End)
                {
                    layout.Direction = StackDirection.HorizontalReverse;
                    layout.Pos.X = innerWidth;
                    layout.SubstractWidth = SubstractSize.Full;
                    layout.Children = Children.ReverseList();
                }
                else if (align == StackAlign.SpaceBetween)
                {
                    layout.Direction = StackDirection.Horizontal;
                    layout.Spacing = (innerWidth - GetTotalChildrenWidth()) / (float)(Children.Count - 1);
                }
                else if (align == StackAlign.SpaceAround)
                {
                    layout.Direction = StackDirection.Horizontal;
                    layout.Spacing = (innerWidth - GetTotalChildrenWidth()) / (float)Children.Count;
                    layout.Pos.X = layout.Spacing / 2f;
                }
                else if (align == StackAlign.SpaceEvenly)
                {
                    layout.Direction = StackDirection.Horizontal;
                    layout.Spacing = (innerWidth - GetTotalChildrenWidth()) / (float)(Children.Count + 1);
                    layout.Pos.X = layout.Spacing;
                }

                if (crossAlign == StackCrossAlign.Center)
                {
                    layout.Pos.Y = innerHeight * 0.5f;
                    layout.SubstractHeight = SubstractSize.Half;
                }
                else if (crossAlign == StackCrossAlign.End)
                {
                    layout.Pos.Y = innerHeight;
                    layout.SubstractHeight = SubstractSize.Full;
                }
            }

            return layout;
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
