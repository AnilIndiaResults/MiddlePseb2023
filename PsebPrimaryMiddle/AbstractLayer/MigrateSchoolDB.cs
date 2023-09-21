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
//using Ionic.Zip;
using System.Web.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebPrimaryMiddle.Repository;

namespace PsebJunior.AbstractLayer
{
    public class MigrateSchoolDB : IMigrateSchoolRepository
    {


        public DataSet ViewAllCandidatesOfSchool(string schl, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ViewAllCandidatesOfSchoolSP";
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet ChekResultCompairSubjects(string schl, string chkSub)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ChekResultCompairSubjects_sp";           
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@stud_sub", chkSub);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet GetMigrationDataByStudentId(int type, string StudentId)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetMigrationDataByStudentId";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@StudentId", StudentId);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetMigrationDataByIdandSearch(int type,string MigrationId, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetMigrationDataByIdandSearch";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@Id", MigrationId);
            cmd.Parameters.AddWithValue("@Search", search);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet Insert_MigrationForm(MigrateSchoolModels MS)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Insert_MigrationForm_SP";
                cmd.Parameters.AddWithValue("@StdId", MS.StdId);
                cmd.Parameters.AddWithValue("@SchlCode", MS.SchlCode);
                cmd.Parameters.AddWithValue("@RegNo", MS.RegNo);
                cmd.Parameters.AddWithValue("@groupNM", MS.Std_Sub);
                cmd.Parameters.AddWithValue("@Candi_Name", MS.Candi_Name);
                cmd.Parameters.AddWithValue("@Father_Name", MS.Father_Name);

                cmd.Parameters.AddWithValue("@Mother_Name", MS.Mother_Name);
                cmd.Parameters.AddWithValue("@SchlCodeNew", MS.SchlCodeNew);
                cmd.Parameters.AddWithValue("@DISTNM", MS.DistName);               
                cmd.Parameters.AddWithValue("@DDRcptNo", MS.DDRcptNo);
                cmd.Parameters.AddWithValue("@Amount", MS.Amount);
                cmd.Parameters.AddWithValue("@DepositDt", MS.DepositDt);
                cmd.Parameters.AddWithValue("@BankName", MS.BankName);
                cmd.Parameters.AddWithValue("@DiryOrderNo", MS.DiryOrderNo);
                cmd.Parameters.AddWithValue("@OrderDt", MS.OrderDt);
                cmd.Parameters.AddWithValue("@OrderBy", MS.OrderBy);
                cmd.Parameters.AddWithValue("@Remark", MS.Remark);
                cmd.Parameters.AddWithValue("@userip", AbstractLayer.StaticDB.GetFullIPAddress());
                cmd.Parameters.AddWithValue("@userName", MS.UserName);
                cmd.Parameters.Add("@GetCorrectionNo", SqlDbType.Int).Direction = ParameterDirection.Output;
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {

                return null;
            }
        }



        public string DeleteMigrationData(string stdid)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteMigrationData";
                cmd.Parameters.AddWithValue("@Std_id", stdid);
                cmd.Parameters.AddWithValue("@MyIP", AbstractLayer.StaticDB.GetFullIPAddress());               
                result = db.ExecuteNonQuery(cmd).ToString();               
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;           
        }



    }
}