using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using PsebJunior.Models;
using System.IO;
using System.Web.Mvc;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebPrimaryMiddle.Repository;

namespace PsebJunior.AbstractLayer
{
    public class ReportDB : IReportRepository
    {

       
        public  DataSet PrimaryMiddleSummaryReportsByType(int ReportType)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PrimaryMiddleSummaryReportsByType";
                cmd.Parameters.AddWithValue("@ReportType", ReportType);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
           
        }


        public  DataSet SubjectWiseReports(string ReportType, int cls, string dist)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "SubjectWiseReportsSP";
                cmd.Parameters.AddWithValue("@ReportType", ReportType);
                cmd.Parameters.AddWithValue("@Class", cls);
                cmd.Parameters.AddWithValue("@Dist", dist);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
           
        }



        public DataSet RegistrationReportSearch(string Search, string Session)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RegistrationReportSearchSP";
                cmd.Parameters.AddWithValue("@Session", Session);
                cmd.Parameters.AddWithValue("@Search", Search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }


        public DataSet ClassSummaryReportByType(int ReportType, int cls)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClassSummaryReportByTypeSP";
                cmd.Parameters.AddWithValue("@ReportType", ReportType);
                cmd.Parameters.AddWithValue("@cls", cls);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }


        public DataSet BankWiseFeeCollectionDetails(string search, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BankWiseFeeCollectionDetailsSP";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutError = "";
                return null;
            }

        }



        public DataSet CategoryWiseFeeCollectionDetails(string search, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CategoryWiseFeeCollectionDetailsSP";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutError = "";
                return  null;
            }

        }


        public DataSet DateWiseFeeCollectionDetails(string search, string type, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DateWiseFeeCollectionDetailsSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutError = "";
                return null;
            }

        }


        public DataSet StatusofCorrection()
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "StatusofCorrectionReportSP";               
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public DataSet MigrationCountReport(string search, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "MigrationCountReportSP";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;

                ds= db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "";
                return  null;
            }

        }


        public DataSet ClusterRegisterReport(string userId, string usertype, string search, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClusterRegisterReportSP";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@usertype", usertype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;

                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "";
                return null;
            }

        }


        public DataSet ClusterMarkingStatusReport(int ReportType, string userId, string usertype, string search, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "ClusterMarkingStatusReportSP";
                cmd.Parameters.AddWithValue("@reporttype", ReportType);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@usertype", usertype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "";
                return null;
            }

        }

        public ClusterReportModel BindAllListofClusterReport()
        {
            ClusterReportModel clusterReportModel = new ClusterReportModel();
            try
            {
                
                DataSet ds = new DataSet();

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BindAllListofClusterReport";
                ds= db.ExecuteDataSet(cmd);
                if (ds != null)
                {
                    var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new ClusterModel
                    {
                        dist = dataRow.Field<string>("dist"),
                        ccode = dataRow.Field<string>("ccode"),
                        clusternm = dataRow.Field<string>("clusterdetails"),                        
                    }).ToList();

                    var subList = ds.Tables[1].AsEnumerable().Select(dataRow => new SubjectModel
                    {
                        sub = dataRow.Field<string>("sub"),
                        subnm = dataRow.Field<string>("name_eng"),
                    }).ToList();

                    clusterReportModel.ClusterList = eList.ToList();
                    clusterReportModel.SubList = subList.ToList();
                }
                
            }
            catch (Exception ex)
            {
                //return null;
            }

            return clusterReportModel;
        }


        public DataSet TheoryMarksStatusReport(string ReportType, int cls)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "TheoryMarksStatusReportSP";
                cmd.Parameters.AddWithValue("@ReportType", ReportType);
                cmd.Parameters.AddWithValue("@Class", cls);
                
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }



    }
}
