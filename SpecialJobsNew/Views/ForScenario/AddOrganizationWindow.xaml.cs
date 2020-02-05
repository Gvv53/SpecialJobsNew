
using System;
using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for AddOrganizationWindow.xaml
    /// </summary>
    public partial class AddOrganizationWindow : Window
    {
        public event Action< string> TextUpdated;
        public AddOrganizationWindow(ORGANIZATION org)
        {
            InitializeComponent();
            DataContext = new AddOrganizationWindowViewModel(org);
            TextUpdated += ((AddOrganizationWindowViewModel) DataContext).TextUpdated;
            ((AddOrganizationWindowViewModel)DataContext).closeWindow += WindowClose;
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
