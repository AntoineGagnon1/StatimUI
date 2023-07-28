﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("foreach")]
    public class ForEach : Component
    {
        public Func<object, List<Component>> ComponentsCreator;

        public Property<IEnumerable<object>> In;
        public List<object> lastItems = new();
        public int StartIndex, Count;

        public override void Start(IList<Component> slots)
        {
            AssertParent();

            StartIndex = Parent.Children.IndexOf(this) + 1;
        }

        public ForEach()
        {
            Visible = false;
        }

        public override bool Update()
        {
            AssertParent();
            var num = (new Random()).NextDouble();
            if (num > 0.9995f)
                ((List<string>)@In.Value).Add(num.ToString());

            int i = 0;
            foreach (var newItem in @In.Value)
            {
                var lastItem = lastItems.ElementAtOrDefault(i);
                if (!newItem.Equals(lastItem))
                {
                    if (lastItem == null)
                        lastItems.Add(newItem);
                    else
                        lastItems[i] = newItem;

                    var slots = ComponentsCreator(newItem);
                    var slotIndex = StartIndex + i * slots.Count;
                    if (i < Count)
                    {
                        for (int j = 0; j  < slots.Count; j++)
                        {
                            Parent.Children[slotIndex + j] = slots[j];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < slots.Count; j++)
                        {
                            Parent.Children.Insert(slotIndex + j, slots[j]);
                        }
                    }
                }
                i++;
            }

            Count = i;
            return false;
        }

        public override void Render(Vector2 offset)
        {
            
        }
    }
}
