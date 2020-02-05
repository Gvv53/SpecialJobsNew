using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace SpecialJobs.Reports
{
    public partial class reportMeasurementData : DevExpress.XtraReports.UI.XtraReport
    {
        private TopMarginBand topMarginBand1;
        private DetailBand detailBand1;
        private BottomMarginBand bottomMarginBand1;
    
        public reportMeasurementData()
        {
            InitializeComponent();
        }

        //private void InitializeComponent()
        //{
        //    this.topMarginBand1 = new DevExpress.XtraReports.UI.TopMarginBand();
        //    this.detailBand1 = new DevExpress.XtraReports.UI.DetailBand();
        //    this.bottomMarginBand1 = new DevExpress.XtraReports.UI.BottomMarginBand();
        //    ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
        //    // 
        //    // topMarginBand1
        //    // 
        //    this.topMarginBand1.Name = "topMarginBand1";
        //    // 
        //    // detailBand1
        //    // 
        //    this.detailBand1.Name = "detailBand1";
        //    // 
        //    // bottomMarginBand1
        //    // 
        //    this.bottomMarginBand1.Name = "bottomMarginBand1";
        //    // 
        //    // reportMeasurementData
        //    // 
        //    this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
        //    this.topMarginBand1,
        //    this.detailBand1,
        //    this.bottomMarginBand1});
        //    this.Version = "13.2";
        //    ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        //}

    }
}
