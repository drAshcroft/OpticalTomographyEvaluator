using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MathLibrary;
using ImageProcessing;
using ImageProcessing._3D;

namespace ReconstructCells
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public void ShowCaption(string text)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowTextDelegate(ShowCaption), text);
            }
            else
            {
                Text = text;
                Application.DoEvents();
            }
        }


        private delegate void ShowTextDelegate(string text);
        public void ShowText(string text)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowTextDelegate(ShowText), text);
            }
            else
            {
                textBox1.Visible = true;
                textBox1.Text = text;
                tableLayoutPanel1.Visible = false;
                Application.DoEvents();
            }
        }

        private delegate void  ShowBitmapsDelegate(Bitmap [] b);
        public void ShowBitmaps(Bitmap[] b)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowBitmapsDelegate(ShowBitmaps), b);
            }
            else
            {
                pictureBox1.Image = b[0];
                pictureBox2.Image = b[1];
                pictureBox3.Image = b[2];

                if (b.Length == 4)
                    pictureBox4.Image = b[3];

                pictureBox1.Invalidate();
                pictureBox2.Invalidate();
                pictureBox3.Invalidate();
                pictureBox4.Invalidate();
                Application.DoEvents();
            }
        }

        private delegate void ShowBitmapsDelegateF(float[,,] b);
        public void ShowBitmaps(float[,,] b)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowBitmapsDelegateF(ShowBitmaps), b);
            }
            else
            {
                var b2 = b.ShowCross();
                pictureBox1.Image = b2[0];
                pictureBox2.Image = b2[1];
                pictureBox3.Image = b2[2];

                if (b.Length == 4)
                    pictureBox4.Image = b2[3];

                pictureBox1.Invalidate();
                pictureBox2.Invalidate();
                pictureBox3.Invalidate();
                pictureBox4.Invalidate();
                Application.DoEvents();
            }
        }

        private delegate void ShowBitmapsLibDelegate(OnDemandImageLibrary b);
        public void ShowBitmaps(OnDemandImageLibrary b)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowBitmapsLibDelegate(ShowBitmaps), b);
            }
            else
            {
                pictureBox1.Image = b[0].Bitmap;
                pictureBox2.Image = b[50].Bitmap;
                pictureBox3.Image = b[0].Bitmap;
                pictureBox4.Image = b[50].Bitmap;

                pictureBox1.Invalidate();
                pictureBox2.Invalidate();
                pictureBox3.Invalidate();
                pictureBox4.Invalidate();
                Application.DoEvents();
            }
        }
    }
}
