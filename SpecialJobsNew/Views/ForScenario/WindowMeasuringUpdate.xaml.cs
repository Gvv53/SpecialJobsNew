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
    /// Логика взаимодействия для WindowMeasuringUpdate.xaml
    /// </summary>
    public partial class WindowMeasuringUpdate : Window
    {
        public WindowMeasuringUpdate()
        {
            InitializeComponent();
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
