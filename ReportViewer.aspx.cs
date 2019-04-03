using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public partial class ReportViewer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
            if (!IsPostBack)
            {
                long ReportId = 0;
                long UserId = 0;
            try
            {
                ReportId = Convert.ToInt64(Request.QueryString["reportid"]);
                UserId = Convert.ToInt64(Request.QueryString["userid"]);
            }
            catch
            {
                ReportId = 0;
                UserId = 0;
            }
            //ReportId = 636712298336242996;
            //UserId = 0;

            PrepareReport PR_obj = new PrepareReport();
                ReportViewerData RVD_obj = new ReportViewerData();
                mReportViewer RV_obj = new mReportViewer();
                ConvertDate CD_obj = new ConvertDate();

                RVD_obj = getReportData(ReportId, UserId);
                if (RVD_obj.CurrentCulture == 0)
                    Title = "Report Viewer";
                else
                    Title = "عرض التقرير";
                //UpdateReportData(ReportId, UserId);
                RV_obj = PR_obj.getReportData(RVD_obj.Employees, RVD_obj.ReportName, RVD_obj.From, RVD_obj.To, RVD_obj.CurrentCulture, RVD_obj.culture, RVD_obj.FromDate, RVD_obj.ToDate, RVD_obj.EmployeeLocation, RVD_obj.isHijri, RVD_obj.InterruptionPeriod);


                ReportParameter ReportDatetmp = new ReportParameter();
                ReportParameter ReportMonth = new ReportParameter();
                ReportParameter StartOfPeriod = new ReportParameter();
                ReportParameter EndOfPeriod = new ReportParameter();
                ReportParameter CurrentTime = new ReportParameter();
                ReportParameter InterruptionPeriodParameter = new ReportParameter(); // added by Lara Almomani
                ReportParameter currentUserParameter = new ReportParameter(); // added by Lara Almomani

            Microsoft.Reporting.WebForms.ReportDataSource rds = new ReportDataSource();

                if (RV_obj.DataSourceName != "Report_MonthlyAttendance")
                    rds = new Microsoft.Reporting.WebForms.ReportDataSource(RV_obj.DataSourceName, RV_obj.DataSource);
                else
                {

                    ReportMonth = new ReportParameter("ReportMonth", RVD_obj.FromDate.Split('/')[1]);
                    rds = new ReportDataSource(RV_obj.DataSourceName, RV_obj.MonthlyAttendanceReport);
                }
                string dt = DateTime.Now.ToString("dd/MM/yyyy");
                if (RVD_obj.CurrentCulture == 2)
                    dt = CD_obj.GregorianToHijri(dt);
                ReportDatetmp = new ReportParameter("ReportDate", dt);
                StartOfPeriod = new ReportParameter("From", RVD_obj.FromDate);
                EndOfPeriod = new ReportParameter("To", RVD_obj.ToDate);
                CurrentTime = new ReportParameter("Time", DateTime.Now.ToString("h:mm:ss tt"));

            #region InterruptionPeriod Report and MissingPunchReport
            // added by Lara Almomani
            if (RVD_obj.ReportName == "EmployeesWorkInterruptionReport")
            {
                InterruptionPeriodParameter = new ReportParameter("InterruptionPeriodParam", RVD_obj.InterruptionPeriod.ToString());
            }
            // added by Lara Almomani
            if (RVD_obj.ReportName == "MissingPunchReport" || RVD_obj.ReportName == "EmployeesWorkInterruptionReport")
            {
                currentUserParameter = new ReportParameter("currentUserParam", RVD_obj.userName);
            }
            #endregion


            reportViewer.LocalReport.ReportPath = RV_obj.ReportPath;
                reportViewer.LocalReport.DataSources.Clear();
                reportViewer.LocalReport.DataSources.Add(rds);
                reportViewer.LocalReport.SetParameters(ReportMonth);
                reportViewer.LocalReport.SetParameters(EndOfPeriod);
                reportViewer.LocalReport.SetParameters(StartOfPeriod);
                reportViewer.LocalReport.SetParameters(ReportDatetmp);
                reportViewer.LocalReport.SetParameters(CurrentTime);

            // added by Lara Almomani
            if (RVD_obj.ReportName == "EmployeesWorkInterruptionReport")
            {
                reportViewer.LocalReport.SetParameters(InterruptionPeriodParameter);
            }
            // added by Lara Almomani
            if (RVD_obj.ReportName == "MissingPunchReport" || RVD_obj.ReportName == "EmployeesWorkInterruptionReport")
            {
                reportViewer.LocalReport.SetParameters(currentUserParameter);
            }

            reportViewer.LocalReport.Refresh();

            }


        
        
    }

    public ReportViewerData getReportData(long ReportId, long UserId)
    {
        ReportViewerData RVD_obj = new ReportViewerData();
        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ATASSqlCon"].ConnectionString))
        {
            string cmd = "SELECT [ReportId] ,[UserId],[ReportName],[From],[To],[CurrentCulture],[culture],[FromDate],[ToDate],[EmployeeLocation],[isHijri],[InterruptionPeriod], [userName] FROM [dbo].[ReportData] where ReportId = '" + ReportId + "' and UserId = '" + UserId + "'";
            using (SqlCommand command = new SqlCommand(cmd, conn))
            {
                command.CommandType = CommandType.Text; ;
                SqlDataReader reader = null;
                conn.Open();
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        RVD_obj.ReportId = Convert.ToInt64(reader.GetValue(0));
                        RVD_obj.UserId = Convert.ToInt64(reader.GetValue(1));
                        RVD_obj.ReportName = Convert.ToString(reader.GetValue(2));
                        RVD_obj.From = Convert.ToInt32(reader.GetValue(3));
                        RVD_obj.To = Convert.ToInt32(reader.GetValue(4));
                        RVD_obj.CurrentCulture = Convert.ToInt16(reader.GetValue(5));
                        RVD_obj.culture = Convert.ToString(reader.GetValue(6));
                        RVD_obj.FromDate = Convert.ToString(reader.GetValue(7));
                        RVD_obj.ToDate = Convert.ToString(reader.GetValue(8));
                        RVD_obj.EmployeeLocation = Convert.ToInt16(reader.GetValue(9));
                        RVD_obj.isHijri = Convert.ToBoolean(reader.GetValue(10));
                        RVD_obj.InterruptionPeriod = Convert.ToInt32(reader.GetValue(11));
                        RVD_obj.userName = Convert.ToString(reader.GetValue(12));
                    }
                }
                command.Dispose();
                reader.Close();
            }

            // get Report Employees

            RVD_obj.Employees = new List<long>();
            cmd = "SELECT [EmployeeId] FROM [dbo].[ReportDataEmployees] where ReportId = '" + ReportId + "' and UserId = '" + UserId + "'";
            using (SqlCommand command = new SqlCommand(cmd, conn))
            {
                command.CommandType = CommandType.Text; ;
                SqlDataReader reader = null;
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        long e = Convert.ToInt64(reader.GetValue(0));
                        RVD_obj.Employees.Add(Convert.ToInt64(reader.GetValue(0)));
                    }
                }
                command.Dispose();
                reader.Close();
            }

        }
        return RVD_obj;
    }

    public void UpdateReportData(long ReportId, long UserId)
    {
        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ATASSqlCon"].ConnectionString))
        {
            string cmd = "delete FROM [dbo].[ReportDataEmployees] where ReportId = '" + ReportId + "' and UserId = '" + UserId + "'" + " delete FROM [dbo].[ReportData] where ReportId = '" + ReportId + "' and UserId = '" + UserId + "'";
            using (SqlCommand command = new SqlCommand(cmd, conn))
            {
                command.CommandType = CommandType.Text;
                conn.Open();
                command.ExecuteNonQuery();
            }
        }
    }

}