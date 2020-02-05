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
using DevExpress.Xpf.Editors;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for EquipmentWindow.xaml
    /// </summary>
    public partial class EquipmentWindow : Window
    {
        public EquipmentWindow()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            tgc.RefreshData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((EquipmentViewModel) this.DataContext).methodsEntities.SaveChanges();         
        }

        private void TreeListView1_CellValueChanged(object sender, DevExpress.Xpf.Grid.TreeList.TreeListCellValueChangedEventArgs e)
        {
            var state = ((EquipmentViewModel)this.DataContext).methodsEntities.Entry(e.Row).State;
            if (state != System.Data.Entity.EntityState.Added)
                ((EquipmentViewModel)this.DataContext).methodsEntities.Entry(e.Row).State = System.Data.Entity.EntityState.Modified;
        }
    }
}
