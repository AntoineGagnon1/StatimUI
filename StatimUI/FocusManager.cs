using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Text;

namespace StatimUI
{
    public static class FocusManager
    {
        public static bool ShowFocus { get; set; } // true when tab is pressed until a mouse button is pressed
        public static Component? FocusedComponent { get; set; }

        private static Component FindNext(Component component)
        {
            if (component.Parent == null) // this is the root
                return FindComponent(component);

            var index = component.Parent.Children.IndexOf(component);
            if (index != component.Parent.Children.Count - 1)
            {
                var next = component.Parent.Children[index + 1];
                if (next.Focusable)
                    return next;

                return FindComponent(component.Parent.Children[index + 1]);
            }

            return FindNext(component.Parent);
        }

        private static Component FindComponent(Component component)
        {
            if (component.Children.Count > 0)
            {
                var child = component.Children[0];
                if (child.Focusable)
                    return child;

                return FindComponent(child);
            }

            return FindNext(component);
        }

        public static void ShiftFocus()
        {
            if (FocusedComponent == null)
                return; // TODO: make it the root

            FindComponent(FocusedComponent).Focus();
        }
    }
}
