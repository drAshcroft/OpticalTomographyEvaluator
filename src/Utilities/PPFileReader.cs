using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Utilities
{
    public class xmlFileReader:IDisposable
    {

        XmlDocument xmlDoc = new XmlDocument();


        public void Dispose()
        {
           
         
        }
        //public Dictionary<string, string> Properties;


        public xmlFileReader(string Filename)
        {
            xmlDoc.Load(Filename);
        }

        public string[] GetNodes(string NodePath, int index, string[] attributes)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes(NodePath);
            XmlNode node = nodes[index];
            
            string[] values = new string[attributes.Length];
            for (int i=0;i<attributes.Length;i++)
                values [i]=(string)node.Attributes[attributes[i]].Value;
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
                        return  node.InnerText.ToString();
                    }
                    catch { }
                }
            }

            return "";
        }

       

    }
}
