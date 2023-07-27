using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("if")]
    public class If : Component
    {
        public Property<bool> cond;
        private IList<Component>? _slots;

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

            var visible = cond.Value;
            foreach (var slot in _slots)
            {
                Parent.Children.Single(component => component == slot).Visible = visible;
            }
            return false;
        }
    }
}
