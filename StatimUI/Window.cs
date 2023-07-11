using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class Window
    {
        public Component Root { get; set; } = new TextComponent();


        public void Update()
        {
            Root.Update();
        }

    }
}
