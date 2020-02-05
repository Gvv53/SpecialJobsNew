using System;
using System.Windows;
using SpecialJobs.Helpers;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Interaction logic for WindowMode_1.xaml
    /// </summary>
    /// 
    public partial class WindowTypeARMMode_1 : Window
    {
        
        public WindowTypeARMMode_1()
        {
            InitializeComponent();
        }
        public void WindowClose()
        {
            this.Close();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
        //выделение содержимого ячейки, получившей фокус

    }
}
