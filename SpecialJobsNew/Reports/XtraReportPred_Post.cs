using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace SpecialJobs.Reports
{
    public partial class XtraReportPred_Post : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportPred_Post()
        {
            InitializeComponent();
            //xrLabelDate.DataBindings.Add(new XRBinding(parameter1, "Text", ""));
            xrLabelFIO1.DataBindings.Add(new XRBinding(parameter2, "Text", ""));
            xrLabelFIO2.DataBindings.Add(new XRBinding(parameter3, "Text", ""));
            parameter2.Value = (object)xrLabelFIO1.Text;
            parameter3.Value = (object)xrLabelFIO2.Text;
        }

       
    }
}
