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
using System.Windows.Shapes;
using System.Windows.Threading;
namespace ASU_Evaluation2
{
    /// <summary>
    /// Interaction logic for CellSelection.xaml
    /// </summary>
    public partial class CellSelection : Window
    {
        private static Dispatcher dispatcher;
        public static Dispatcher GetDispatcher()
        {
            return dispatcher;

        }

        private MainWindow _MainWindow;

        public CellSelection(MainWindow mw )
        {
            InitializeComponent();
            _MainWindow = mw;
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcher = this.Dispatcher;
            var data = TreeviewSource.GetTreeview(DataStore.DataFolder);
            DataTree.ItemsSource = (System.Collections.IEnumerable)data;
            
           // PopulateDates();

            tbSqlStatement.Text = "SELECT datasetName FROM CellQuality WHERE cellType='EPC2' AND cellGood=1 AND reconGood=1 AND interesting=0 AND datasetDate>='2013-05-20 00:00:00.000' AND datasetDate<='2013-06-27 23:59:59:00.000';";
            lCellTypes.ItemsSource = Fixes.CellTypes();

        }


        private void PopulateDates()
        {

            string sql = "Select * from CellQuality";

            string[] folders = DataStore.GetDirectories(sql);
            var data = TreeviewSource.GetTreeview(folders, TreeviewSource.ViewTypes.ListView);
            DataTree.ItemsSource = (System.Collections.IEnumerable)data;
            TabControl1.SelectedIndex = 0;
        }
        private void DataTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Day)
            {
                var t=((Day)e.NewValue).DataSets;
                System.Diagnostics.Debug.Print(t.Count.ToString());
            }
            if (e.NewValue is Dataset)
            {
                _MainWindow.SelectDataset((Dataset) e.NewValue);
            }
        }

        private void bSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = "";
                if (tabItem2.IsSelected)
                {
                    sql = tbSqlStatement.Text;

                }
                else if (tabItem3.IsSelected)
                {
                    sql = "Select * from CellQuality WHERE datasetDate>='" + DataStore.DateTimeToSQL((DateTime)dSearchFrom.SelectedDate) + "' AND datasetDate<='" + DataStore.DateTimeToSQL((DateTime)bSearchTo.SelectedDate) + "';";

                }
                else if (tabItem4.IsSelected)
                {
                    sql = "SELECT datasetName FROM CellQuality WHERE cellType='" + lCellTypes.SelectedItem.ToString() + "';";

                }
                string[] folders = DataStore.GetDirectories(sql);
                var data = TreeviewSource.GetTreeview(folders, TreeviewSource.ViewTypes.ListView);
                DataTree.ItemsSource = (System.Collections.IEnumerable)data;
                TabControl1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                if (tabItem2.IsSelected)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        
    }
}
