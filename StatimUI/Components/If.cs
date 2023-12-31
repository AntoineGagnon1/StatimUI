﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("if")]
    public class If : Component
    {
        public Property<bool> Condition = new ValueProperty<bool>(false);
        private IList<Component>? _slots;

        public If()
        {
            Visible = false;
        }

        public override bool Focusable => false;

        protected override void OnRender(Vector2 drawPosition)
        {
            
        }

        public override void Start(IList<Component> slots)
        {
            AssertParent();

            _slots = slots;
            Parent.Children.AddRange(slots);
        }

        public override bool Update()
        {
            AssertParent();
            if (_slots == null)
                return false;

            var visible = Condition.Value;
            foreach (var slot in _slots)
            {
                Parent.Children.Single(component => component == slot).Visible = visible;
            }
            return false;
        }
    }
}
