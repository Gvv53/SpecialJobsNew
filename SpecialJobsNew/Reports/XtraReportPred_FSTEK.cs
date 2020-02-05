using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace SpecialJobs.Reports
{
    public partial class XtraReportPred_FSTEK :XtraReport
    {
        public XtraReportPred_FSTEK()
        {
            InitializeComponent();
            xrLabelFIO1.DataBindings.Add(new XRBinding(parameter2, "Text", ""));
            xrLabelFIO2.DataBindings.Add(new XRBinding(parameter3, "Text", ""));
            parameter2.Value = (object)xrLabelFIO1.Text;
            parameter3.Value = (object)xrLabelFIO2.Text;
        }

        private void GroupHeader1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((GroupHeaderBand)sender).Visible = (bool)parameter1.Value;
        }

    }
}
