using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RemoteMovie
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] Files = Directory.GetFiles(@"C:\Dehydrated\cct001\201101\20\cct001_20110120_152935\library");

            int cc = 0;
            for (int i=0;i<Files.Length;i++)
            {
                pictureBox1.Image = new Bitmap(Files[i]);
                pictureBox1.Invalidate();
                Application.DoEvents();
            }
        }
    }
}
