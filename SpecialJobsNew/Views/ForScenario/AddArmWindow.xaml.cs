
using System;
using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for AddArmWindow.xaml
    /// </summary>
    public partial class AddArmWindow : Window
    {
        public event Action<string> TextUpdated;
        public AddArmWindow(ARM arm)
        {
            InitializeComponent();
            DataContext = new AddArmWindowViewModel(arm);
            TextUpdated += ((AddArmWindowViewModel)DataContext).TextUpdated;
            ((AddArmWindowViewModel) DataContext).closeWindow += WindowClose;
        }
        //public AddArmWindow(string armName)
        //{
        //    InitializeComponent();
        //    DataContext = new AddArmWindowViewModel(armName);
        //    TextUpdated += ((AddArmWindowViewModel)DataContext).TextUpdated;
        //    ((AddArmWindowViewModel)DataContext).closeWindow += WindowClose;
        //}
        private void WindowClose()
        {
            this.Close();
        }
        private void TextEdit_EditValueChanging(object sender, DevExpress.Xpf.Editors.EditValueChangingEventArgs e)
        {
            if (e.NewValue == null)
                TextUpdated("");
            else
                TextUpdated(e.NewValue.ToString());
        }
    }
}
