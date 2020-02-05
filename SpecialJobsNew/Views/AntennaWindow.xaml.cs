using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using DevExpress.Xpf.Grid;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for OrganizationControl.xaml
    /// </summary>
    public partial class AntennaWindow : Window
    {        
        public AntennaWindow()
        {
            InitializeComponent();
        }
        public void Cancel()
        {
            this.Close();            
        }

        private void TableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            
        }
    }
}
