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
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.Data.Odbc;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Threading.Tasks;
using System.Threading;
using PsebPrimaryMiddle.Repository;


namespace PsebJunior.AbstractLayer
{
    public class CenterHeadDB : ICenterHeadRepository
    {
        private Database db;
        public CenterHeadDB()
        {
            db = DatabaseFactory.CreateDatabase();
        }


        public int ChangePassword(string UserId, string CurrentPassword, string NewPassword)
        {
            int result;

            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CenterHeadChangePasswordSP";
                cmd.Parameters.AddWithValue("@UserId", UserId);
                cmd.Parameters.AddWithValue("@OldPwd", CurrentPassword);
                cmd.Parameters.AddWithValue("@NewPwd", NewPassword);
                result = db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                result = -1;
            }
            return result;

        }

        public Task<CenterHeadLoginSession> CheckCenterHeadLogin(LoginModel LM)  // Type 1=Regular, 2=Open
        {
            CenterHeadLoginSession loginSession = new CenterHeadLoginSession();
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "CenterHeadLoginSP";// LoginSP(old)
                cmd.Parameters.AddWithValue("@UserName", LM.UserName);
                cmd.Parameters.AddWithValue("@Pwd", LM.Password);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        loginSession.LoginStatus = DBNull.Value != reader["LoginStatus"] ? (int)reader["LoginStatus"] : default(int);
                        loginSession.CenterHeadId =  DBNull.Value != reader["CenterHeadId"] ? (int) reader["CenterHeadId"] : default(int);
                        loginSession.UserName = DBNull.Value != reader["UserName"] ? (string)reader["UserName"] : default(string);
                        loginSession.CenterHeadName = DBNull.Value != reader["CenterHeadName"] ? (string)reader["CenterHeadName"] : default(string);
                        loginSession.Mobile = DBNull.Value != reader["Mobile"] ? (string)reader["Mobile"] : default(string);
                        loginSession.EmailId = DBNull.Value != reader["EmailId"] ? (string) reader["EmailId"] : default(string);
                        loginSession.SchoolAllows = DBNull.Value != reader["SchoolAllows"] ? (string)reader["SchoolAllows"] : default(string);
                        loginSession.IsActive = DBNull.Value != reader["IsActive"] ? (bool) reader["IsActive"] : default(bool);
                        loginSession.ClusterName = DBNull.Value != reader["clusternm"] ? (string)reader["clusternm"] : default(string);
                        loginSession.ChtUdise = DBNull.Value != reader["chtudise"] ? (string)reader["chtudise"] : default(string);
                        loginSession.CenterInchargeName = DBNull.Value != reader["CenterInchargeName"] ? (string)reader["CenterInchargeName"] : default(string);
                    }
                }
                Thread.Sleep(2000);
            }
            catch (Exception)
            {
                loginSession = null;
            }
            return Task.FromResult(loginSession);
        }



        public static DataSet CheckSchlAllowToCenterHead(int type, string centerid, string schl, string Search)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CheckSchlAllowToCenterHeadSP"; //REGular
                //cmd.CommandText = "CheckSchlAllowToCenterHeadSP_PVT"; //PRivate Reappear Candidate
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@CenterId", centerid);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@Search", Search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }


        public DataSet CenterHeadMaster(int type, int id, string Search)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CenterHeadMasterSP"; //REGular
                //cmd.CommandText = "CenterHeadMasterSP_PVT"; //PRivate Reappear Candidate
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@Search", Search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }


        public List<CenterSchoolModel> SchoolListByCenterId(int type, int id, string Search)
        {
            List<CenterSchoolModel> centerSchoolModels = new List<CenterSchoolModel>();
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                 cmd.CommandText = "CenterHeadMasterSP"; //REGular
               // cmd.CommandText = "CenterHeadMasterSP_PVT"; //PRivate Reappear Candidate
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@Search", Search);
                ds = db.ExecuteDataSet(cmd);
                if (ds != null)
                {
                    var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new CenterSchoolModel
                    {
                        Schl = dataRow.Field<string>("Schl"),
                        SchlNME = dataRow.Field<string>("SchlNME"),
                        Dist = dataRow.Field<string>("Dist"),
                        DistNM = dataRow.Field<string>("DistNM"),
                        FifSet = dataRow.Field<string>("FifSet"),
                        Fifth = dataRow.Field<string>("Fifth"),
                        Middle = dataRow.Field<string>("Middle"),
                        MidSet = dataRow.Field<string>("MidSet"),                        
                        NOP = dataRow.Field<int>("NOP"),
                        Udisecode = dataRow.Field<string>("Udisecode"),
                        UserType = dataRow.Field<string>("UserType"),
                        IsMarkedFilled = dataRow.Field<int>("IsMarkedFilled"),
                        IsActive = dataRow.Field<int>("IsActive"),
                        LastDate = dataRow.Field<DateTime>("LastDate"),
                        FinalStatus = dataRow.Field<string>("FinalStatus"),
                        FinalSubmitBy = dataRow.Field<string>("FinalSubmitBy"),
                        FinalSubmitLot = dataRow.Field<string>("FinalSubmitLot"),
                        FinalSubmitOn = dataRow.Field<string>("FinalSubmitOn"),

                    }).ToList();

                    centerSchoolModels = eList.ToList();
                }
            }
            catch (Exception ex)
            {
                centerSchoolModels = null;
            }
            return centerSchoolModels;
        }


        public DataSet UpdateCenterHeadMaster(CenterHeadMasterModel centerHeadMasterModel, out int OutStatus)   // GetBankDataByBCODE
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdateCenterHeadMasterSP";
            cmd.Parameters.AddWithValue("@CenterHeadId", centerHeadMasterModel.CenterHeadId);
            cmd.Parameters.AddWithValue("@UserName", centerHeadMasterModel.UserName);
            cmd.Parameters.AddWithValue("@CenterHeadName", centerHeadMasterModel.CenterHeadName);
            cmd.Parameters.AddWithValue("@Mobile", centerHeadMasterModel.Mobile);
            cmd.Parameters.AddWithValue("@EmailId", centerHeadMasterModel.EmailId);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }


        #region GenerateTicket    

        public int InsertGenerateTicket(GenerateTicketModel generateTicketModel, out string outTicketNumber)
        {
            int result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertGenerateTicketSP";
                cmd.Parameters.AddWithValue("@Type", generateTicketModel.ActionType);
                cmd.Parameters.AddWithValue("@TicketId", generateTicketModel.TicketId);
                cmd.Parameters.AddWithValue("@ComplaintTypeId", generateTicketModel.ComplaintTypeId);
                cmd.Parameters.AddWithValue("@CenterHeadUserName", generateTicketModel.CenterHeadUserName);
                cmd.Parameters.AddWithValue("@SchoolCode", generateTicketModel.SchoolCode);
                cmd.Parameters.AddWithValue("@QueryMessage", generateTicketModel.QueryMessage);
                cmd.Parameters.AddWithValue("@Filepath", generateTicketModel.Filepath);
                cmd.Parameters.AddWithValue("@IsActive", generateTicketModel.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", generateTicketModel.CreatedBy);
                cmd.Parameters.AddWithValue("@UpdatedBy", generateTicketModel.UpdatedBy);
                cmd.Parameters.AddWithValue("@AdminId", generateTicketModel.AdminId);
                cmd.Parameters.AddWithValue("@TicketStatus", generateTicketModel.TicketStatus);
                cmd.Parameters.AddWithValue("@TicketReason", generateTicketModel.TicketReason);
                cmd.Parameters.AddWithValue("@TicketStatusBy", generateTicketModel.TicketStatusBy);
                cmd.Parameters.Add("@OutId", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@TicketNumber", SqlDbType.NVarChar, 30).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                int outuniqueid = (int)cmd.Parameters["@OutId"].Value;
                result = outuniqueid;
                outTicketNumber = (string)cmd.Parameters["@TicketNumber"].Value;
            }
            catch (Exception ex)
            {
                outTicketNumber = ex.Message;
                result = -1;
            }
            return result;          
        }


        public string ListingGenerateTicket(int type, int TicketId, out int OutStatus)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ListingGenerateTicketSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@TicketId", TicketId);
                cmd.Parameters.Add("@outstatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@outstatus"].Value;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                result = "";
            }
            return result;
        }



        public List<GenerateTicketModel> GetGenerateTicketData(int type, string centerUser, int id, string Search)
        {
            List<GenerateTicketModel> GenerateTicketModels = new List<GenerateTicketModel>();
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GenerateTicketSP"; //CUTLISTSP_AG
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@centerUser", centerUser);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@Search", Search);
                ds = db.ExecuteDataSet(cmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                       var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new GenerateTicketModel
                        {
                            TicketId = dataRow.Field<int>("TicketId"),
                            TicketNumber = dataRow.Field<string>("TicketNumber"),
                            ComplaintTypeId = dataRow.Field<int>("ComplaintTypeId"),
                            SchoolCode = dataRow.Field<string>("SchoolCode"),
                            QueryMessage = dataRow.Field<string>("QueryMessage"),
                            Filepath = dataRow.Field<string>("Filepath"),
                            IsActive = dataRow.Field<int>("IsActive"),
                            CreatedOn = dataRow.Field<DateTime>("CreatedOn"),
                            CreatedBy = dataRow.Field<int>("CreatedBy"),
                            UpdatedOn = dataRow.Field<DateTime?>("UpdatedOn"),
                            UpdatedBy = dataRow.Field<int>("UpdatedBy"),
                            AdminId = dataRow.Field<int>("AdminId"),
                            TicketStatus = dataRow.Field<int>("TicketStatus"),
                            TicketReason = dataRow.Field<string>("TicketReason"),
                            TicketStatusBy = dataRow.Field<string>("TicketStatusBy"),
                            CenterHeadUserName = dataRow.Field<string>("CenterHeadUserName"),
                            ComplaintTypeName = dataRow.Field<string>("ComplaintTypeName"),
                            FinalTicketStatus = dataRow.Field<string>("FinalTicketStatus"),
                        }).ToList();

                        GenerateTicketModels = eList.ToList();
                }
            }
            catch (Exception ex)
            {
                GenerateTicketModels = null;
            }
            return GenerateTicketModels;
        }

        #endregion

    }
}