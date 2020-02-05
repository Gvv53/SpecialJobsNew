using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraReports.UI;
using SpecialJobs.ViewModels;
using System.Linq;
using System.Windows.Forms;

namespace SpecialJobs.Reports
{
    public partial class ReportResult_FSTEK : DevExpress.XtraReports.UI.XtraReport
    {
        private int nppTable3 = 0;

        public ReportResult_FSTEK()
        {
            InitializeComponent();
            parameter1.Value = DateTime.Now.ToShortDateString();
            parameter2.Value = DateTime.Now.ToShortDateString();
            xrLabelFIO.DataBindings.Add(new XRBinding(parameter3, "Text", ""));
            parameter3.Value = (object)xrLabelFIO.Text;
        }


        private void xrTableCell149_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //бордюры ячейки - реализация объединения ячеек таблицы
            if (!String.IsNullOrEmpty((((XRTableCell)sender).Text)))
                switch (((XRTableCell)sender).Text)
                {
                    case "begin-end":
                        ((XRTableCell)sender).Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left )
                                                                                                 | DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom))));
                        ((XRTableCell)sender).Text = "Поверка и калибровка метрологической службой не требуется";
                        break;
                    case "begin":
                        ((XRTableCell)sender).Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left )
                                                                                                 | DevExpress.XtraPrinting.BorderSide.Right))));
                        ((XRTableCell)sender).Text = "Поверка и калибровка метрологической службой не требуется";
                        break;
                    case "end":
                        ((XRTableCell)sender).Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)
                                                                                                 | DevExpress.XtraPrinting.BorderSide.Right))));
                        ((XRTableCell)sender).Text = "";
                        break;
                }
        }

        private void xrTableCell31_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((XRTableCell)sender).Text = (++nppTable3).ToString() + ".";
        }

      

        

        //private void xrTableCell92_93_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        //{
        //    //бордюры ячейки - реализация объединения ячеек таблицы
        //    if (!String.IsNullOrEmpty((((XRTableCell)sender).Text)) && ((XRTableCell)sender).Text.Contains("end"))
        //        ((XRTableCell)sender).Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)
        //                                                                                 | DevExpress.XtraPrinting.BorderSide.Right))));
        //    if (((XRTableCell)sender).Text.Contains("end"))
        //        if (((XRTableCell)sender).Text == "end")
        //            ((XRTableCell)sender).Text = String.Empty;
        //        else
        //            ((XRTableCell)sender).Text = ((XRTableCell)sender).Text.Substring(3);
        //}

        private void xrTableCell33_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRTableCell)sender).Text == "True")
                ((XRTableCell)sender).Text = "сплошной";
            else
                ((XRTableCell)sender).Text = "дискретный";
        }

        private void xrTableRow20_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if ((((XRTableRow)sender).Cells[3].Text == "0" || ((XRTableRow)sender).Cells[3].Text == "") &&
               (((XRTableRow)sender).Cells[4].Text == "0" || ((XRTableRow)sender).Cells[4].Text == "0"))
                ((XRTableRow)sender).Visible = false;
            else
            {
                ((XRTableRow)sender).Visible = true;
            }
        }

        private void ReportResult_FSTEK_ParametersRequestSubmit(object sender, DevExpress.XtraReports.Parameters.ParametersRequestEventArgs e)
        {
            labelBeginEnd.Text = parameter1.Value.Equals(parameter2.Value)
                ? parameter1.Value + "г."
                : "с  " + parameter1.Value + "г.  по " +
                  parameter2.Value + "г.";
        }

        private void xrTableCell_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRTableCell)sender).Text == "0")
                ((XRTableCell)sender).Text = "";
        }

        private void xrTableRow13_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

            if (((XRTableRow)sender).Cells[7].Text == "0" ||
               ((XRTableRow)sender).Cells[7].Text == "")
                ((XRTableRow)sender).Visible = false;
            else
            {
                ((XRTableRow)sender).Visible = true;
            }
        }

        private void xrTableRow23_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if ((((XRTableRow)sender).Cells[2].Text == "0" || ((XRTableRow)sender).Cells[2].Text == "") &&
              (((XRTableRow)sender).Cells[3].Text == "0" || ((XRTableRow)sender).Cells[3].Text == "0") &&
              (((XRTableRow)sender).Cells[4].Text == "0" || ((XRTableRow)sender).Cells[4].Text == "") &&
              (((XRTableRow)sender).Cells[5].Text == "0" || ((XRTableRow)sender).Cells[5].Text == "0"))
                ((XRTableRow)sender).Visible = false;
            else
            {
                ((XRTableRow)sender).Visible = true;
            }
        }

        private void GroupHeader_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ((GroupHeaderBand)sender).Visible = (bool)parameter4.Value;
        }

        private void xrTableCell55_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRTableCell)sender).Text == "Электрическая")
                ((XRTableCell)sender).Text = "Элек.";
            else
                ((XRTableCell)sender).Text = "Маг.";
        }
    }
}

