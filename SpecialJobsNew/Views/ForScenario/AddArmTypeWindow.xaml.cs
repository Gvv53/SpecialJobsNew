using System;
using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for AddArmWindow.xaml
    /// </summary>
    public partial class AddArmTypeWindow : Window
    {
        public event Action<string> TextUpdated;
        public AddArmTypeWindow(ARM_TYPE armType)
        {
            InitializeComponent();
            DataContext = new AddArmTypeWindowViewModel(armType);
            TextUpdated += ((AddArmTypeWindowViewModel)DataContext).TextUpdated;
            ((AddArmTypeWindowViewModel)DataContext).closeWindow += WindowClose;
        }

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
