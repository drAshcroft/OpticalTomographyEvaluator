using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASU_Evaluation2
{
    /// <summary>
    /// Interaction logic for OrthogonalView.xaml
    /// </summary>
    public partial class OrthogonalView : UserControl
    {
        public OrthogonalView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //threeDViewer.Child = new Three_D_View();
        }
    }
}
