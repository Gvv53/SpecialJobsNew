using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SpecialJobs.ViewModels;
using DevExpress.Xpf.Grid;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for SelectWindow.xaml
    /// </summary>
    public partial class SelectWindow : Window
    {                       
        public SelectWindow(ObservableCollection<AllArmsView> allArmsView,AllArmsView aav)
        {
            InitializeComponent();            
            this.DataContext = new SelectWindowViewModel(allArmsView,aav) ;
            ((SelectWindowViewModel)this.DataContext).closeWindow += WindowClose;

        }
        private void WindowClose()
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gc.Columns[0].GroupIndex = 0;
            gc.Columns[1].GroupIndex = 1;
            gc.Columns[2].GroupIndex = 2;

        }

       
    }
}
