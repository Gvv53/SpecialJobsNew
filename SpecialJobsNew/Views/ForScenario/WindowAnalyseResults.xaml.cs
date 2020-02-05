using System;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace SpecialJobs.Views.ForScenario
{
    /// <summary>
    /// Логика взаимодействия для WindowAnalyseResults.xaml
    /// </summary>
    public partial class WindowAnalyseResults : Window
    {
        public event Action refreshAnalyse;
        public WindowAnalyseResults()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            tgc.View.UseLightweightTemplates = UseLightweightTemplates.None;
            tgc.Columns.GetColumnByFieldName("R2portable_1").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2portable_2").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2portable_3").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2drive_1").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2drive_2").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2drive_3").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2carry_1").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2carry_2").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R2carry_3").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R1sosr_1").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R1sosr_2").CellStyle = (Style)tgc.FindResource("customCellStyle");
            tgc.Columns.GetColumnByFieldName("R1sosr_3").CellStyle = (Style)tgc.FindResource("customCellStyle");          
        }

        private void CheckEdit_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshAnalyse();
            this.tgc.RefreshData();
        }

            private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                this.Top = (SystemParameters.WorkArea.Height - e.NewSize.Height) / 2;
                this.Left = (SystemParameters.WorkArea.Width - e.NewSize.Width) / 2;
            }

        private void treeListView1_CustomColumnDisplayText(object sender, DevExpress.Xpf.Grid.TreeList.TreeListCustomColumnDisplayTextEventArgs e)
        {
            var val = String.Format("{0}", e.Value);
            if (val == "0")
                e.DisplayText = "";
        }
    }
}
