using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StatimUI
{

    public class XMLComponent : Component
    {
        public override void Render()
        {
            var document = new XmlDocument();
            document.LoadXml("<div>heyy</div>");
            foreach (XmlNode xmlNode in document.ChildNodes)
            {
                Console.WriteLine(xmlNode.Name);
            }
            /*foreach (var node in document)
            {
                Console.WriteLine(node.Name);
            }*/
        }
    }
}
