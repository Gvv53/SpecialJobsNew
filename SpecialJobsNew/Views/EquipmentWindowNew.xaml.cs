using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Логика взаимодействия для EquipmentWindowNew.xaml
    /// </summary>
    public partial class EquipmentWindowNew : Window
    {
        public EquipmentWindowNew()
        {
            InitializeComponent();
        }
        public void RefreshData()
        {
            tgc.RefreshData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((EquipmentViewModelNew)this.DataContext).methodsEntities.SaveChanges();
        }

        private void TreeListView1_CellValueChanged(object sender, DevExpress.Xpf.Grid.TreeList.TreeListCellValueChangedEventArgs e)
        {
            var state = ((EquipmentViewModelNew)this.DataContext).methodsEntities.Entry(e.Row).State;
            if (state != System.Data.Entity.EntityState.Added)
                ((EquipmentViewModelNew)this.DataContext).methodsEntities.Entry(e.Row).State = System.Data.Entity.EntityState.Modified;
        }
    }
}
