using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for OrganizationControl.xaml
    /// </summary>
    public partial class MDeviceWindow : Window
    {
        public MDeviceWindow()
        {
            InitializeComponent();
        }
        public void Cancel()
        {
            this.Close();            
        }
        public void RefreshGcMDevice()
        {
            //для обновления данных из сязанной таблицы
            var itemSource = gcMDevice.ItemsSource;
            gcMDevice.ItemsSource = null;
            gcMDevice.ItemsSource = itemSource;
        }
        public void Focus()
        {
            teName.Focus();
        }
    }
}
