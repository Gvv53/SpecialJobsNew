using System;
using System.Collections.ObjectModel;
using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for MdWindow.xaml
    /// </summary>
    /// 
   
    public partial class MdWindow : Window
    {
        private MdViewModel vm;
        public event Action saveData;
        public MdWindow()
        {
            InitializeComponent();
        }
        public void RefreshData()
        {
            gcMdAll.RefreshData();
            gcMdArm.RefreshData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveData();
        }
        //коллекция выбранных строк
        public void getSelectedRow(string gcName)
        {
            if (vm == null)
                vm = this.DataContext as MdViewModel;
            ObservableCollection<MEASURING_DEVICE> temp = new ObservableCollection<MEASURING_DEVICE>();
            ObservableCollection<ANTENNA> temp1 = new ObservableCollection<ANTENNA>();
            switch (gcName)
            {
                case "gcMdArm":
                    foreach (MEASURING_DEVICE md in gcMdArm.SelectedItems)
                        temp.Add(md);
                    vm.selectedItemsMdArm = temp;
                    break;
                case "gcMdAll":                    
                    foreach (MEASURING_DEVICE md in gcMdAll.SelectedItems)
                        temp.Add(md);
                    vm.selectedItemsMdAll = temp;
                    break;
                case "gcAntArm":
                    foreach (ANTENNA md in gcAntArm.SelectedItems)
                        temp1.Add(md);
                    vm.selectedItemsAntArm = temp1;
                    break;

                case "gcAntAll":
                    foreach (ANTENNA md in gcAntAll.SelectedItems)
                        temp1.Add(md);
                    vm.selectedItemsAntAll = temp1;
                    break;

            }

        }

    }
}
