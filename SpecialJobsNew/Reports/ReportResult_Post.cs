using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DevExpress.CodeParser;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraReports.UI;
using DevExpress.XtraRichEdit.Model.History;
using SpecialJobs.ViewModels;

namespace SpecialJobs.Reports
{
    public partial class ReportResult_Post : DevExpress.XtraReports.UI.XtraReport
    {
        int nppMode = 1;
        int npp = 1;
        int nppSP = 1;
        private double R2_Max_2, R2_Max_3,DeltaMax;//максимальные значения в режиме, формируются динамически, перед печатью R2 строки
        private bool keyVisible = false;

        public ReportResult_Post()
        {
            InitializeComponent();
            parameter1.Value = DateTime.Now.ToShortDateString();
            parameter2.Value = DateTime.Now.ToShortDateString();
            xrLabelFIO.DataBindings.Add(new XRBinding(parameter3, "Text", ""));
            parameter3.Value = (object)xrLabelFIO.Text;
          
        }

        private void xrLabel23_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //очередной режим
            ((XRLabel)sender).Text = "Таблица № 3." +  npp.ToString();
            labelSP.Text = "Таблица № 3." + npp.ToString() + ".1";
            npp++;
            R2_Max_2 = 0;
            R2_Max_3 = 0;
            DeltaMax = 0;
        }

        private void CellR2_Max_2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (R2_Max_2 != 0)
                ((XRTableCell) sender).Text = R2_Max_2.ToString();
            else
               ((XRTableCell) sender).Text = "";
        }

        private void cellR2_2_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                double currentR2 = Double.Parse(((XRTableCell)sender).Lines[0]);
                R2_Max_2 = Math.Max(R2_Max_2, currentR2);
            }
            catch (Exception)
            {}                                
           
        }

        private void cellR2_3_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                double currentR3 = Double.Parse(((XRTableCell) sender).Lines[0]);
                R2_Max_3 = Math.Max(R2_Max_3, currentR3);
            }           
            catch (Exception)
            {}                                

        }

        private void CellR2_Max_3_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (R2_Max_3 != 0)
                ((XRTableCell)sender).Text = R2_Max_3.ToString();
            else 
                ((XRTableCell)sender).Text = "";
        }

        private void CellDeltaMax_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (DeltaMax != 0)
               ((XRTableCell)sender).Text = DeltaMax.ToString();
            else  
                ((XRTableCell)sender).Text = "";
        }

        private void CellDelta_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                double currentDelta = Double.Parse(((XRTableCell)sender).Lines[0]);
                DeltaMax = Math.Max(DeltaMax, currentDelta);
            }
            catch (Exception)
            {}
        }
        //строка таблицы широкополосных сигналов
        private void xrTableRow18_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            
            
            if (((XRTableRow)sender).Cells[1].Text == "0" &&
                ((XRTableRow)sender).Cells[2].Text == "0")
                ((XRTableRow)sender).Visible = false;
            else
            {//если есть хоть одна ШП строка, показываем таблицу                
                ((XRTableRow)sender).Visible = true;               
            }        
        }

    
        private void xrTableCell54_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (((XRTableCell) sender).Text == "0")
                ((XRTableCell) sender).Text = "";
        }      

        private void DetailReport4_DataSourceRowChanged(object sender, DataSourceRowEventArgs e)
        {
            if (((MainWindowViewModel) ((BindingSource) ((DetailReportBand) sender).DataSource).DataSource)
                    .ModesForReport[e.CurrentRow].mode.MEASURING_FOR_REPORT.Where(p => p.MFR_F_BEGIN != 0 && p.MFR_F_END != 0)
                    .Count() > 0)
            {
                labelSP.Visible = true;
                xrTableSPHeader.Visible = true;
            }
            else
            {
                labelSP.Visible = false;
                xrTableSPHeader.Visible = false;
            }
        }

        private void ReportResult_Post_ParametersRequestSubmit(object sender, DevExpress.XtraReports.Parameters.ParametersRequestEventArgs e)
        {
            if (!(bool)parameter4.Value)
            {
                Header.Visible = true;
                HeaderGS.Visible = false;
                Data.Visible = true;
                DataGS.Visible = false;
                R2.Visible = true;
                R2GS.Visible = false;
                                //п.1.3. в зависимости от параметра "Без ГШ"        
                GroupHeader2.Visible = false;
                labelGS.Visible = false;
            }
            else
            {
                Header.Visible = false;
                HeaderGS.Visible = true;
                Data.Visible = false;
                DataGS.Visible = true;
                R2.Visible = false;
                R2GS.Visible = true;
                //п.1.3. в зависимости от параметра "Без ГШ"        
                GroupHeader2.Visible = true;
                labelGS.Visible = true;
            }
            labelBeginEnd.Text = parameter1.Value.Equals(parameter2.Value)
                ? parameter1.Value + "г."
                : "с  " + parameter1.Value + "г.  по " +
                  parameter2.Value + "г.";
        }

        private void xrTableCell149_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //бордюры ячейки - реализация объединения ячеек таблицы
            if (!String.IsNullOrEmpty((((XRTableCell) sender).Text)))
            switch (((XRTableCell) sender).Text)
            {
                case "begin-end":
                    ((XRTableCell)sender).Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left )
                                                                                             | DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom))));
                    ((XRTableCell)sender).Text = "Поверка и калибровка метрологической службой не требуется";
                    break;
                case "begin":
                    ((XRTableCell) sender).Borders  = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left )                        
                                                                                             | DevExpress.XtraPrinting.BorderSide.Right) )));
                    ((XRTableCell)sender).Text = "Поверка и калибровка метрологической службой не требуется";
                break;   
            case "end":
                    ((XRTableCell) sender).Borders  = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom) 
                                                                                             | DevExpress.XtraPrinting.BorderSide.Right) )));
                    ((XRTableCell)sender).Text = "";
                break;  
            }

        }
    }
}

