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

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Interaction logic for WindowOrgAnalysis.xaml
    /// </summary>
    public partial class WindowOrgAnalysis_1 : Window
    {
        public WindowOrgAnalysis_1()
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
       
    }
}
