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

    [Component("stack", true)]
    public class Stack : Component
    {
        public Property<StackDirection> Direction = new ValueProperty<StackDirection>(StackDirection.Horizontal);
        public Property<StackAlign> Align = new ValueProperty<StackAlign>(StackAlign.Start);
        public Property<StackCrossAlign> CrossAlign = new ValueProperty<StackCrossAlign>(StackCrossAlign.Start);
        private int lastChildrenCount = 0;

        protected override void OnRender(Vector2 drawPosition)
        {
            base.OnRender(drawPosition);

            foreach (var child in Children)
            {
                child.Render(drawPosition);
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

            if (Direction.Value == StackDirection.Horizontal || Direction.Value == StackDirection.HorizontalReverse)
            {
                foreach (var child in layout.Children)
                {
                    child.Position.Value = GetChildPos(child, new Vector2(layout.MainPos, layout.CrossPos), layout.SubstractMain, layout.SubstractCross);
                    if (layout.DirectionReversed)
                        layout.MainPos -= child.TotalPixelWidth + layout.Spacing;
                    else
                        layout.MainPos += child.TotalPixelWidth + layout.Spacing;

                    Height.Value = Height.Value.WithScalar(Math.Max(child.TotalPixelHeight + Padding.Value.Vertical, Height.Value.Scalar));
                }

                Width.Value = Width.Value.WithScalar(layout.MainPos + Padding.Value.Horizontal);
            }
            else
            {
                foreach (var child in layout.Children)
                {
                    child.Position.Value = GetChildPos(child, new Vector2(layout.CrossPos, layout.MainPos), layout.SubstractCross, layout.SubstractMain);
                    if (layout.DirectionReversed)
                        layout.MainPos -= child.TotalPixelHeight + layout.Spacing;
                    else
                        layout.MainPos += child.TotalPixelHeight + layout.Spacing;

                    Width.Value = Width.Value.WithScalar(Math.Max(child.TotalPixelWidth + Padding.Value.Horizontal, Width.Value.Scalar));
                }

                Height.Value = Height.Value.WithScalar(layout.MainPos + Padding.Value.Vertical);
            }

            //ResizeMainAxis(layout.Pos, layout.Direction);

            return HasSizeChanged();
        }


        private static Vector2 GetChildPos(Component child, Vector2 pos,SubstractSize subWidth, SubstractSize subHeight)
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
            public float MainPos;
            public float CrossPos;
            public float Spacing;
            public IEnumerable<Component> Children;
            public bool DirectionReversed;
            public SubstractSize SubstractMain;
            public SubstractSize SubstractCross;

        }
        private Layout GetLayout()
        {
            var layout = new Layout();
            bool reverse = false;
            var visibleChildren = VisibleChildren;

            var align = Align.Value;
            if (IsReversed())
            {
                reverse = true;
                if (align == StackAlign.Start)
                    align = StackAlign.End;
                else if (align == StackAlign.End)
                    align = StackAlign.Start;
            }
            var crossAlign = CrossAlign.Value;

            if (align == StackAlign.Center)
            {
                layout.MainPos = MainInnerSize() * 0.5f - GetChildrenMainSize(visibleChildren) * 0.5f;
            }
            else if (align == StackAlign.End)
            {
                reverse = !reverse;
                layout.DirectionReversed = true;
                layout.MainPos = MainInnerSize();
                layout.SubstractMain = SubstractSize.Full;
            }
            else if (align == StackAlign.SpaceBetween)
            {
                layout.Spacing = (MainInnerSize() - GetChildrenMainSize(visibleChildren)) / (float)(visibleChildren.Count - 1);
            }
            else if (align == StackAlign.SpaceAround)
            {
                layout.Spacing = (MainInnerSize() - GetChildrenMainSize(visibleChildren)) / (float)visibleChildren.Count;
                layout.MainPos = layout.Spacing / 2f;
            }
            else if (align == StackAlign.SpaceEvenly)
            {
                layout.Spacing = (MainInnerSize() - GetChildrenMainSize(visibleChildren)) / (float)(visibleChildren.Count + 1);
                layout.MainPos = layout.Spacing;
            }

            if (crossAlign == StackCrossAlign.Center)
            {
                layout.CrossPos = CrossInnerSize() * 0.5f;
                layout.SubstractCross = SubstractSize.Half;
            }
            else if (crossAlign == StackCrossAlign.End)
            {
                layout.CrossPos = CrossInnerSize();
                layout.SubstractCross = SubstractSize.Full;
            }

            layout.Children = reverse ? visibleChildren.ReverseList() : visibleChildren;

            return layout;
        }

        private bool IsReversed() => Direction.Value == StackDirection.HorizontalReverse || Direction.Value == StackDirection.VerticalReverse;

        private float CrossInnerSize()
        {
            if (Direction.Value == StackDirection.Horizontal || Direction.Value == StackDirection.HorizontalReverse)
                return InnerPixelHeight;

            return InnerPixelWidth;
        }

        private float MainInnerSize()
        {
            if (Direction.Value == StackDirection.Horizontal || Direction.Value == StackDirection.HorizontalReverse)
                return InnerPixelWidth;

            return InnerPixelHeight;
        }

        private float GetChildrenMainSize(List<Component> children)
        {
            if (Direction.Value == StackDirection.Horizontal || Direction.Value == StackDirection.HorizontalReverse)
                return GetTotalChildrenWidth(children);
   
            return GetTotalChildrenHeight(children);
        }

        private float GetTotalChildrenWidth(List<Component> children)
        {
            var total = 0f;
            foreach (var child in children)
                total += child.TotalPixelWidth;
            return total;
        }

        private float GetTotalChildrenHeight(List<Component> children)
        {
            var total = 0f;
            foreach (var child in children)
                total += child.TotalPixelHeight;
            return total;
        }
    }
}
