using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomControls
{
    public partial class ContrastPicturebox : UserControl
    {
        public ContrastPicturebox()
        {
            InitializeComponent();
        }

        private Bitmap mImage=null;
        private bool mWeber=false ;

        private float Brightness = 1;
        private float Contrast = 1;
        private void DrawImage()
        {
            pictureBox1.Image = mImage;
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public Bitmap Image
        {
            get { return mImage; }
            set { mImage = value; }

        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public bool WeberVisible
        {
            set { rbWeber.Visible = value; }

        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public string Label
        {
            set { label1.Text = value; }

        }

        private void rbWeber_CheckedChanged(object sender, EventArgs e)
        {
            mWeber = rbWeber.Checked;

            if (mWeber)
            {

            }
            DrawImage();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DrawImage();
            }
        }
    }
}
