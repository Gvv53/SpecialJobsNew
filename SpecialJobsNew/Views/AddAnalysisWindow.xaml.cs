
using System;
using System.Windows;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for AddAnalisisWindow.xaml
    /// </summary>
    public partial class AddAnalisisWindow : Window
    {
        public event Action<string> TextUpdated;
        public AddAnalisisWindow(ANALYSIS analysis)
        {
            InitializeComponent();
            DataContext = new AddAnalysisWindowViewModel(analysis);
            TextUpdated += ((AddAnalysisWindowViewModel)DataContext).TextUpdated;
            ((AddAnalysisWindowViewModel) DataContext).closeWindow += WindowClose;
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
