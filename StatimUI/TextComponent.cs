using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class TextComponent : Component
    {
        private string _content = "";
        public string Content 
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    HasChanged = true;
                }
            }
        }

        override public void Render()
        {
            ImGuiNET.ImGui.Text(Content);
            
            HasChanged = false;
        }
    }
}
