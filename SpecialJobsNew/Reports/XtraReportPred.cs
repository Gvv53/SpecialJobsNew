using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace SpecialJobs.Reports
{
    public partial class XtraReportPred : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportPred()
        {
            InitializeComponent();
            
        }

        private void XtraReportPred_ParametersRequestSubmit(object sender, DevExpress.XtraReports.Parameters.ParametersRequestEventArgs e)
        {
            //в зависимости от значений параметров и типа отчёта определяется видимость полей
            switch (sender.GetType().Name)
            {
                case "XtraReportPred":
                    switch (e.ParametersInformation[0].Parameter.Value.ToString())
                    {
                        case "Поставка":
                            label1.Visible = true;
                            break;
                    }
                    break;
                case "ReportResult":
                    break;
            }
            
        }

   

    }
}
