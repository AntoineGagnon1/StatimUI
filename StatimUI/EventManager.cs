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

        // These functions must be called by the adapter to forward the user inputs to Statim :

        public static void SetMousePos(Vector2 pos, Dockspace dockspace)
        {
            var component = dockspace.FindPanelAt(pos, out var offset)?.Children.FirstOrDefault()?.FindComponentAt(pos - offset);
            if (Hovered != component)
            {
                Hovered?.OnMouseExit();
                component?.OnMouseEnter();
                Hovered = component;
            }
        }

    }
}
