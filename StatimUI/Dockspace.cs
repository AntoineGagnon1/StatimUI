using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    // No need for -X and -Y, because you can create them by inverting the parent and child
    public enum DockDirection { X, Y }
    public enum SplitDirection { X, NegativeX, Y, NegativeY}
    
    public class Dockspace
    {
        // Dockspaces can have one child dockspace (when it is split)
        public Dockspace? Child { get; set; } = null;
        public DockDirection Direction { get; set; } = DockDirection.X;
        public int SplitPosition { get; set; } = 0;

        public List<Panel> Panels { get; private set; } = new List<Panel>();

        public Size Size { get; private set; }

        public Dockspace(Panel panel)
        {
            Dockspaces.Add(this);
            Panels.Add(panel);
        }

        public Dockspace(Panel panel, Size size)
            : this(panel)
        {
            Resize(size);
        }

        public void Split(SplitDirection direction, Panel child)
        {
            if (Child != null)
                throw new Exception("Cannot split an already split Dockspace");

            switch (direction)
            {
                case SplitDirection.NegativeX:
                    // Like SplitDirection.X, but the current content goes into the child
                    Child = new Dockspace(child);
                    Child.Panels = Panels;
                    Panels = new();
                    goto case SplitDirection.X;
                case SplitDirection.X:
                    Child = new Dockspace(child);
                    SplitPosition = Size.Width / 2;
                    Direction = DockDirection.X;
                    break;

                case SplitDirection.NegativeY:
                    // Like SplitDirection.Y, but the current content goes into the child
                    Child = new Dockspace(child);
                    Child.Panels = Panels;
                    Panels = new();
                    goto case SplitDirection.Y;
                case SplitDirection.Y:
                    Child = new Dockspace(child);
                    SplitPosition = Size.Height / 2;
                    Direction = DockDirection.Y;
                    break;
            }

            Resize(Size);
        }

        public void Update(Vector2 offset)
        {
            foreach (var panel in Panels)
            {
                panel.Update(offset);
            }

            Child?.Update(offset + (Direction == DockDirection.X ? new Vector2(SplitPosition, 0) : new Vector2(0, SplitPosition)));
        }

        public void TryClose()
        {
            List<Panel> toRemove = new();
            foreach (var panel in Panels)
            {
                if(panel.CanClose())
                    toRemove.Add(panel);
            }

            foreach (var panel in toRemove)
                Panels.Remove(panel);

            if(Panels.Count == 0)
                Renderer.Adapter!.DestroySubWindow(this);
        }

        public void Resize(Size newSize)
        {
            Size = newSize;

            foreach (var panel in Panels)
                panel.Size = GetPanelsSize();

            Child?.Resize(GetChildSize());
        }


        private Size GetPanelsSize()
        {
            if (Child == null)
                return Size;

            if (Direction == DockDirection.X)
                return new Size(SplitPosition, Size.Height);
            else
                return new Size(Size.Width, SplitPosition);
        }

        private Size GetChildSize() => Size - (Direction == DockDirection.X ? new Size(SplitPosition, 0) : new Size(0, SplitPosition));

        // One Dockspace for each os window
        public static List<Dockspace> Dockspaces = new List<Dockspace>();
    }
}
