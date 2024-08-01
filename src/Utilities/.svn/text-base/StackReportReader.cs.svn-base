using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Threading;

namespace Utilities
{
    public class StackReportReader
    {
        XmlDocument xmlDoc = new XmlDocument();


        public void Dispose()
        {


        }
        //public Dictionary<string, string> Properties;

        
        public StackReportReader(string Filename)
        {
            xmlDoc.Load(Filename);
            
        }


        public Rectangle[] GetLocations()
        {


            string[] attributes = new string[] { "centerX", "centerY" };
            XmlNodeList nodes = xmlDoc.SelectNodes(@"DataSet/FixedFocalPlaneStack/FixedFocalPlaneImages/Image");
            Rectangle[] Locations = new Rectangle[nodes.Count];

            int cc = 0;
            foreach (XmlNode node in nodes)
            {
                string[] values = new string[attributes.Length];
                for (int i = 0; i < attributes.Length; i++)
                    values[i] = (string)node.Attributes[attributes[i]].Value;

                int cX = int.Parse(values[0]);
                int cY = int.Parse(values[1]);

                Locations[cc] = new Rectangle(cX - 120, cY - 120, 240, 240);
                cc++;
            }
            
           

            return Locations;
        }

        public string[] GetNodes(string NodePath, int index, string[] attributes)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes(NodePath);
            XmlNode node = nodes[index];

            string[] values = new string[attributes.Length];
            for (int i = 0; i < attributes.Length; i++)
                values[i] = (string)node.Attributes[attributes[i]].Value;
            return values;
        }

        public string GetNode(string NodePath)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes(NodePath);
            if (nodes.Count == 0)
                nodes = xmlDoc.GetElementsByTagName(NodePath);

            if (nodes.Count > 0)
            {
                XmlNode node = nodes[0];
                if (node.Value != null)
                {
                    Console.WriteLine(node.Name + ", " + node.Value.ToString());
                    return node.Value.ToString();
                }
                else
                {
                    try
                    {
                        Console.WriteLine(node.Name + ", " + node.InnerText.ToString());
                        return node.InnerText.ToString();
                    }
                    catch { }
                }
            }

            return "";
        }



    }
}
