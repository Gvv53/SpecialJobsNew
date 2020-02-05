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

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for MdWindow.xaml
    /// </summary>
    public partial class MdWindow : Window
    {
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

    }
}
