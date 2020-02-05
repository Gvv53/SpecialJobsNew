using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for AntennaWindow1.xaml
    /// </summary>
    public partial class AntennaWindow: Window
    {
        public event Action newCalibr;
        public AntennaWindow()
        {
            InitializeComponent();
        }
        public void Cancel()
        {
            this.Close();
        }

        private void TableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            newCalibr();
        }

        public void RefrashGcAntennas()
        {
            //var itemSource = gcAntennas.ItemsSource;
            //gcAntennas.ItemsSource = null;
            //gcAntennas.ItemsSource = itemSource;
            gcAntennas.RefreshData();
            gcCalibr.RefreshData();
        }

        //выделение поля, получившего фокус
        private void TextEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => ((TextEdit)sender).SelectAll()), DispatcherPriority.Input);
        }

        //выделение содержимого ячейки, получившей фокус
        private void TableView_ShownEditor(object sender, EditorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Editor is TextEdit && ((TextEdit)e.Editor).SelectAllOnGotFocus)
                    ((TextEdit)e.Editor).SelectAll();
            }), DispatcherPriority.Input);
        }
        public void FocusUI(string nameUI)
        {
            (FindName(nameUI) as FrameworkElement).Focus();            
        }
       
    }
}
