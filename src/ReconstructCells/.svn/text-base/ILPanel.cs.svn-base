using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ILNumerics.Drawing;
using ILNumerics;

namespace ReconstructCells
{
    public partial class ILPanelForm : Form
    {

        public static ILPanelForm Panel;

        public ILPanelForm()
        {
            InitializeComponent();

            Panel = this;
        }


        // a panel as class attribute
        ILPanel m_panel; 

        private void ILPanel_Load(object sender, EventArgs e)
        {
            // create a new panel
            m_panel = ILPanel.Create();

            // add the panel to the forms Controls collection
            Controls.Add(m_panel);

           
        }


        public void SetSurface(ILArray<float> surf)
        {
            // add a sample surface graph to the panels Graphs collection
            m_panel.Graphs.AddSurfGraph(surf);
            

        }


        public void SetImageSC(ILArray<float> surf)
        {
            // add a sample surface graph to the panels Graphs collection
            m_panel.Graphs.AddImageSCGraph(surf);

        
        }
    }
}
