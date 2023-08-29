using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    public static class EventManager
    {
        public static Component? Hovered { get; private set; } = null;
        public static Component? Focused { get; private set; } = null;
        public static Component? TabNavigation { get; private set; } = null;

        // These functions must be called by the adapter to forward the user inputs to Statim :

        public static Dockspace? FocusedDockspace { get; private set; } = null;
        public static Panel? HoveredPanel { get; private set; } = null;
        public static Panel? FocusedPanel { get; private set; } = null;

        public static void SetMousePos(Vector2 pos, Dockspace dockspace)
        {
            HoveredPanel = dockspace.FindPanelAt(pos, out var offset);
            var component = HoveredPanel?.Children.FirstOrDefault()?.FindComponentAt(pos - offset - HoveredPanel.Padding.Value.TopLeft);
            if (Hovered != component)
            {
                Hovered?.OnMouseExit();
                component?.OnMouseEnter();
                Hovered = component;
            }
        }

        public static void MouseClicked()
        {
            TabNavigation = null;

            if (Hovered?.Focusable ?? false)
                Focused = Hovered;

            Hovered?.OnMouseClick();

            FocusedPanel = HoveredPanel;
        }

        public static void DockspaceFocused(Dockspace dockspace)
        {
            if(FocusedDockspace != dockspace)
            {
                FocusedDockspace = dockspace;
                FocusedPanel = FocusedDockspace?.Panels.FirstOrDefault();
            }

            TabNavigation = null;
        }

        public static void TabNavigationNext()
        {
            if(TabNavigation == null)
            { // Start from the root
                TabNavigation = TabNavigationFindNext(FocusedPanel?.Children.FirstOrDefault());
            }
            else
            {
                TabNavigation = TabNavigationFindNext(TabNavigation);
            }
        }

        private static Component? TabNavigationFindNext(Component? component)
        {
            if (component == null)
                return null;

            if (component.Parent == null) // this is the root
                return TabNavigationFindComponent(component);

            var index = component.Parent.Children.IndexOf(component);
            if (index != component.Parent.Children.Count - 1)
            {
                var next = component.Parent.Children[index + 1];
                if (next.Focusable)
                    return next;

                return TabNavigationFindComponent(component.Parent.Children[index + 1]);
            }

            return TabNavigationFindNext(component.Parent);
        }

        private static Component? TabNavigationFindComponent(Component component)
        {
            if (component.Children.Count > 0)
            {
                var child = component.Children[0];
                if (child.Focusable)
                    return child;

                return TabNavigationFindComponent(child);
            }

            return TabNavigationFindNext(component);
        }
    }
}
