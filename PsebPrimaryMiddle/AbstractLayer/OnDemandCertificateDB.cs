using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using PSEBONLINE.Models;
using System.IO;
using System.Web.Mvc;
using Ionic.Zip;
using System.Web.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Globalization;
using PsebJunior.Models;
using PsebJunior.AbstractLayer;

namespace PSEBONLINE.AbstractLayer
{
    public class OnDemandCertificateDB
    {
        private DBContext context;
        public OnDemandCertificateDB()
        {
            context = new DBContext();
        }
        public static List<OnDemandCertificateSearchModel> GetOnDemandCertificateStudentList(string type, string RP, string cls, string schl, string search, out DataSet dsOut)
        {
            List<OnDemandCertificateSearchModel> registrationSearchModels = new List<OnDemandCertificateSearchModel>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetOnDemandCertificateStudentListSP";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@RP", RP);
            cmd.Parameters.AddWithValue("@Class", cls);
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@search", search);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {
                var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new OnDemandCertificateSearchModel
                {
                    Roll = dataRow.Field<string>("Roll"),
                    Std_id = dataRow.Field<long>("Std_id"),
                    form = dataRow.Field<string>("form"),
                    SCHL = dataRow.Field<string>("SCHL"),
                    AdmDate = dataRow.Field<string>("AdmDate"),
                    name = dataRow.Field<string>("name"),
                    fname = dataRow.Field<string>("fname"),
                    mname = dataRow.Field<string>("mname"),
                    DOB = dataRow.Field<string>("DOB"),
                    Aadhar = dataRow.Field<string>("Aadhar"),
                    //
                    regno = dataRow.Field<string>("regno"),
                    Dist = dataRow.Field<string>("Dist"),
                    EXAM = dataRow.Field<string>("EXAM"),
                    phy_chal = dataRow.Field<string>("phy_chal"),
                    IsExistsInOnDemandCertificates = dataRow.Field<int>("IsExistsInOnDemandCertificates"),
                    DemandId = dataRow.Field<long>("DemandId"),
                    IsChallanCancel = dataRow.Field<int>("IsChallanCancel"),
                    IsHardCopyCertificate = dataRow.Field<string>("IsHardCopyCertificate"),
                    OnDemandCertificatesStatus = dataRow.Field<string>("OnDemandCertificatesStatus"),

                }).ToList();

                registrationSearchModels = eList.ToList();
            }
            dsOut = ds;
            return registrationSearchModels;

        }

        public static List<OnDemandCertificate_ChallanDetailsViews> OnDemandCertificate_ChallanListMiddlePrimary(string schl, out DataSet dsOut)
        {
            List<OnDemandCertificate_ChallanDetailsViews> registrationSearchModels = new List<OnDemandCertificate_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OnDemandCertificate_ChallanListMiddlePrimarySP";          
            cmd.Parameters.AddWithValue("@schl", schl);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {
                var eList = StaticDB.DataTableToList<OnDemandCertificate_ChallanDetailsViews>(ds.Tables[0]);              
                registrationSearchModels = eList.ToList();
            }
            dsOut = ds;
            return registrationSearchModels;

        }

        public int InsertOnDemandCertificateStudentList(List<OnDemandCertificates> list)
        {
            int result = 0;
            if (list.Count() > 0)
            {
                context.OnDemandCertificates.AddRange(list);
                result = context.SaveChanges();
            }
            return result;
        }

        public int RemoveRangeOnDemandCertificateStudentList(List<OnDemandCertificates> list)
        {
            int result = 0;
            if (list.Count() > 0)
            {
                //context.OnDemandCertificates.RemoveRange(list);  
                int i = 0;
                foreach (OnDemandCertificates onDemandCertificates in list)
                {
                    context.OnDemandCertificates.Attach(onDemandCertificates);
                    context.OnDemandCertificates.Remove(onDemandCertificates);
                    context.SaveChanges();
                    i++;
                }
                result = i;
            }
            return result;
        }


        public static DataSet OnDemandCertificatesCountRecordsClassWise(string search, string schl)
        {
            List<OnDemandCertificate_ChallanDetailsViews> registrationSearchModels = new List<OnDemandCertificate_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OnDemandCertificatesCountRecordsClassWise_SP";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@Schl", schl);
            ds = db.ExecuteDataSet(cmd);          
          
            return ds;

        }


        public static DataSet OnDemandCertificateCalculateFee(string cls,string date, string search, string schl)
        {
            List<OnDemandCertificate_ChallanDetailsViews> registrationSearchModels = new List<OnDemandCertificate_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OnDemandCertificateCalculateFeeSP";
            cmd.Parameters.AddWithValue("@cls", cls);
            cmd.Parameters.AddWithValue("@Schl", schl);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@search", search);
           
            ds = db.ExecuteDataSet(cmd);

            return ds;

        }


     

    }
}