using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
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
using DevExpress.XtraReports.UI;
using SpecialJobs.ViewModels.ForReport;

namespace SpecialJobs.Views.ForReport
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
       // public event Action<string> SelectedDescriptionChanged;
        private ReportWindowViewModel rwvm;
        
        public ReportWindow()
        {
            InitializeComponent();                  
        }

        public void CloseWindow()
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (rwvm == null)
                rwvm = (ReportWindowViewModel)this.DataContext;
            //вставка в отчёт выбраного значения поля
            var v = rwvm.xr.AllControls<XRLabel>().Where(p => p.Name == rwvm.selectedLabel);//.Trim()
            if (v.Any())
                v.FirstOrDefault().Text = rwvm.selectedText;
            if (rwvm.selectedRow.RD_DEFAULT == rwvm.isDefault && rwvm.selectedRow.RD_TEXT == rwvm.selectedText)
                return; //ничего не изменили, выбрали готовое
            if (rwvm.selectedRow.RD_DEFAULT != rwvm.isDefault)
            {
                rwvm.selectedRow.RD_DEFAULT = rwvm.isDefault;
                if (rwvm.isDefault) //текущая запись помечена, по умолчанию. Снимем этот признак для других строк
                {
                    foreach (REPORT_DATA rd in rwvm.methodsEntities.REPORT_DATA.Where(p => p.RD_ANL_ID == rwvm.anl_id && p.RD_LABEL == rwvm.selectedLabel && p.RD_ID != rwvm.selectedRow.RD_ID))
                    {
                        rd.RD_DEFAULT = false;
                    }
                }
            }
            if (rwvm.selectedRow.RD_TEXT != rwvm.selectedText)//изменённый текст сохраним в БД
            {
                rwvm.selectedRow.RD_TEXT = rwvm.selectedText;
            }
            rwvm.methodsEntities.SaveChanges(); 
        }

       
        //запрет редактирования
        private void cbLabel_EditValueChanging(object sender, DevExpress.Xpf.Editors.EditValueChangingEventArgs e)
        {
           if (e.OldValue != null)
               e.IsCancel = true;
        }

       


    }
}
