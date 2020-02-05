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

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Interaction logic for WindowOrgAnalysis_2.xaml
    /// </summary>
    public partial class WindowOrgAnalysis_2 : Window
    {
        public WindowOrgAnalysis_2()
        {
            InitializeComponent();
            teName.Focus();               
        }
        public void WindowClose()
        {
            this.Close();
        }
        public void focusName()
        {
            teName.Focus();            
        }
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }

    }
}
