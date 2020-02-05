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
using SpecialJobs.ViewModels;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowReports.xaml
    /// </summary>
    public partial class WindowReports : Window
    {
        MainWindowViewModel vm;
        public WindowReports()
        {
            InitializeComponent();
            buttonKind.Tag = "ProtFSTEK";
        }
        public void WindowClose()
        {
            this.Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (buttonKind != null)
                buttonKind.Tag = "PredPost";
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (buttonKind != null)
                buttonKind.Tag = "PredPostFSTEK";
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            if (buttonKind != null)
                buttonKind.Tag = "ProtPost";
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            if (buttonKind != null)
                buttonKind.Tag = "ProtFSTEK";
        }  
    }
}
