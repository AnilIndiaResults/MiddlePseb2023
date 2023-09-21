using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace PsebPrimaryMiddle.Repository
{
    public interface IReportRepository
    {
        DataSet PrimaryMiddleSummaryReportsByType(int ReportType);
        DataSet SubjectWiseReports(string ReportType, int cls, string dist);
        DataSet RegistrationReportSearch(string Search, string Session);
        DataSet ClassSummaryReportByType(int ReportType, int cls);

        DataSet CategoryWiseFeeCollectionDetails(string search, out string OutError);
        DataSet BankWiseFeeCollectionDetails(string search, out string OutError);
        DataSet DateWiseFeeCollectionDetails(string search, string type, out string OutError);

        DataSet StatusofCorrection();
        DataSet MigrationCountReport(string search, out string OutError);

        DataSet TheoryMarksStatusReport(string ReportType, int cls);


        #region Cluster/CenterHead Reports 

        ClusterReportModel BindAllListofClusterReport();
        DataSet ClusterRegisterReport(string userId, string usertype,string search, out string OutError);
        DataSet ClusterMarkingStatusReport(int ReportType,string userId, string usertype, string search, out string OutError);

        #endregion Cluster/CenterHead Reports 
    }
}