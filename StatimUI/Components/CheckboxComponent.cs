using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("checkbox")]
    public class CheckboxComponent : Component
    {
        public Property<bool> value = new ValueProperty<bool>(false);

        public event EventHandler<bool>? change;

        public override void Start(IList<Component> slots)
        {
        }

        protected void OnChange(bool value)
        {
            change?.Invoke(this, value);
        }

        override public bool Update()
        {
            return false;
        }

        protected override void OnRender(Vector2 offset)
        {
            bool temp = value;
            //if (ImGuiNET.ImGui.Checkbox($"##{this.GetHashCode()}", ref temp))
            //{
            //    value.Value = temp;
            //    OnChange(temp);
            //}
        }
    }
}
