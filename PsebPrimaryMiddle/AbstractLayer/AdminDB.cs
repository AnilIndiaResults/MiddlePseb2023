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
    public class AdminDB : IAdminRepository
    {
        public List<SelectListItem> getAdminDistAllowList(string usertype, string adminid)
        {
            List<SelectListItem> itemDist = new List<SelectListItem>();
            if (usertype.ToLower() == "admin")
            {
                DataSet ds = new AbstractLayer.AdminDB().GetAllAdminUser(Convert.ToInt32(adminid), "");
                foreach (System.Data.DataRow dr in ds.Tables[1].Rows)
                {
                    itemDist.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() });
                }
            }
            else if (usertype.ToLower() == "center")
            {     
                DataSet Dresult = new AbstractLayer.CenterHeadDB().CenterHeadMaster(1, Convert.ToInt32(adminid), ""); // passing Value to DBClass from model            
                foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                {
                    itemDist.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() });
                }
            }
            return itemDist;
        }

         public int ChangePassword(int UserId, string CurrentPassword, string NewPassword)
        {
            int result;
            
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AdminChangePasswordSP";
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


         public AdminLoginSession CheckAdminLogin(LoginModel LM)  // Type 1=Regular, 2=Open
        {
            AdminLoginSession adminLoginSession = new AdminLoginSession();
            try
            {                
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AdminLoginSP";// LoginSP(old)
                cmd.Parameters.AddWithValue("@UserName", LM.UserName);
                cmd.Parameters.AddWithValue("@Password", LM.Password);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        adminLoginSession.AdminId = DBNull.Value != reader["id"] ? (int)reader["id"] : default(int);
                        adminLoginSession.USER = DBNull.Value != reader["USER"] ? (string)reader["USER"] : default(string);
                        adminLoginSession.AdminType = DBNull.Value != reader["Usertype"] ? (string)reader["Usertype"] : default(string);
                        adminLoginSession.USERNAME = DBNull.Value != reader["User_fullnm"] ? (string)reader["User_fullnm"] : default(string);
                        adminLoginSession.PAccessRight = DBNull.Value != reader["PAccessRight"] ? (string)reader["PAccessRight"] : default(string);
                        adminLoginSession.Dist_Allow = DBNull.Value != reader["Dist_Allow"] ? (string)reader["Dist_Allow"] : default(string);
                        adminLoginSession.ActionRight = DBNull.Value != reader["ActionRight"] ? (string)reader["ActionRight"] : default(string);
                        adminLoginSession.LoginStatus = DBNull.Value != reader["Status"] ? (int)reader["Status"] : default(int);
                    }
                }
            }
            catch (Exception)
            {
                adminLoginSession.LoginStatus=0;
            }
            return adminLoginSession;
        }         

        #region  Inbox Master

         public DataTable MailReplyMaster(InboxModel im, out string OutError)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "MailReplyMasterSP";
            cmd.Parameters.AddWithValue("@MId", im.id);
            cmd.Parameters.AddWithValue("@Reply", im.Reply);
            cmd.Parameters.AddWithValue("@ReplyFile", im.Attachments);
            cmd.Parameters.AddWithValue("@ReplyTo", im.To);
            cmd.Parameters.AddWithValue("@ReplyBy", im.From);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            OutError = (string)cmd.Parameters["@OutError"].Value;
            return ds.Tables[0];
        }

         public DataTable AddInboxMaster(InboxModel im, out string OutError)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AddInboxMaster";
            cmd.Parameters.AddWithValue("@SentTo", im.To);
            cmd.Parameters.AddWithValue("@SentFrom", im.From);
            cmd.Parameters.AddWithValue("@Subject", im.Subject);
            cmd.Parameters.AddWithValue("@Body", im.Body);
            cmd.Parameters.AddWithValue("@Attachment", im.Attachments);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            OutError = (string)cmd.Parameters["@OutError"].Value;
            return ds.Tables[0];
        }


         public DataSet ViewInboxMaster(int MailId, int Type, int adminid, string search, int pageNumber, int PageSize)
        {            
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ViewInboxMasterNew";
            cmd.Parameters.AddWithValue("@MailId", MailId);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@adminid", adminid);
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", PageSize);
            return db.ExecuteDataSet(cmd); 
        }

         public string ReadInbox(int Id, int AdminId, int type1, out string OutError)
        {
            string result;
        
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ReadInboxSP";
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.Parameters.AddWithValue("@AdminId", AdminId);
                cmd.Parameters.AddWithValue("@type1", type1);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutError = (string)cmd.Parameters["@OutError"].Value;
            }
            catch (Exception ex)
            {
                OutError = "0";
                result = "";
            }
            return result;
        }
     
        #endregion  Inbox Master

        #region Menu Master

         public DataSet GetAllMenu(int id)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "MenuSP";
            cmd.Parameters.AddWithValue("@id", id);
            return db.ExecuteDataSet(cmd);
        }

         public DataSet UpdateMenuJunior(SiteMenu model, int ParentMenuId, out int OutStatus, string AssignYear)
        {
            DataSet result = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdateMenuJuniorSP";
            cmd.Parameters.AddWithValue("@MenuId", model.MenuID);
            cmd.Parameters.AddWithValue("@MenuName", model.MenuName);
            cmd.Parameters.AddWithValue("@MenuUrl", model.MenuUrl);
            cmd.Parameters.AddWithValue("@ParentMenuId", ParentMenuId);
            cmd.Parameters.AddWithValue("@AssignYear", AssignYear);
            cmd.Parameters.Add("@Outstatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            result = db.ExecuteDataSet(cmd);
            OutStatus = (int)cmd.Parameters["@outstatus"].Value;
            return result;

        }

         public DataSet ListingMenuJunior(int type, int menuid, out int OutStatus)
        {
            DataSet result = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ListingMenuJuniorSP";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@menuid", menuid);
            cmd.Parameters.Add("@outstatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            result = db.ExecuteDataSet(cmd);
            OutStatus = (int)cmd.Parameters["@outstatus"].Value;
            return result;

        }


         public string CreateMenuJunior(SiteMenu model, int IsParent, int ParentMenuId, int IsMenu, string AssignYear)
        {
            string result;           
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CreateMenuJuniorSP";
                cmd.Parameters.AddWithValue("@MenuName", model.MenuName);
                cmd.Parameters.AddWithValue("@MenuUrl", model.MenuUrl);
                cmd.Parameters.AddWithValue("@IsParent", IsParent);
                cmd.Parameters.AddWithValue("@ParentMenuId", ParentMenuId);
                cmd.Parameters.AddWithValue("@IsMenu", IsMenu);
                cmd.Parameters.AddWithValue("@AssignYear", AssignYear);
                result = db.ExecuteNonQuery(cmd).ToString();                
            }
            catch (Exception ex)
            {               
                result = "";
            }
            return result;
        }


     

        #endregion Menu Master

        #region Admin User Master      

          public string ListingUser(int type, int menuid, out int OutStatus)
        {
            string result;
            ;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ListingUserSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@id", menuid);
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

     

         public string CreateAdminUser(AdminUserModel model, out int OutStatus)
        {
            string result;
            ;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CreateAdminUserSP";
                cmd.Parameters.AddWithValue("@id", model.id);
                cmd.Parameters.AddWithValue("@user", model.user);
                cmd.Parameters.AddWithValue("@pass", model.pass);
                cmd.Parameters.AddWithValue("@PAccessRight", model.PAccessRight);
                cmd.Parameters.AddWithValue("@Usertype", model.Usertype);
                cmd.Parameters.AddWithValue("@Dist_Allow", model.Dist_Allow);
                cmd.Parameters.AddWithValue("@User_fullnm", model.User_fullnm);
                cmd.Parameters.AddWithValue("@Designation", model.Designation);
                cmd.Parameters.AddWithValue("@Branch", model.Branch);
                cmd.Parameters.AddWithValue("@Mobno", model.Mobno);
                cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
                cmd.Parameters.AddWithValue("@STATUS", model.STATUS);
                cmd.Parameters.AddWithValue("@EmailID", model.EmailID);
                cmd.Parameters.AddWithValue("@SAccessRight", model.SAccessRight);
                cmd.Parameters.AddWithValue("@ActionRight", model.ActionRight);
                cmd.Parameters.AddWithValue("@utype", model.utype);
                cmd.Parameters.AddWithValue("@Set_Allow", model.Set_Allow);
                cmd.Parameters.AddWithValue("@Bank_Allow", model.Bank_Allow);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                result = "";
            }
            return result;
        }


         public string AssignMenuToUser(string adminid, string adminlist, string pagelist, out string OutError)
        {
            string result;
            ;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AssignMenuToUserSP";
                cmd.Parameters.AddWithValue("@adminlist", adminlist);
                cmd.Parameters.AddWithValue("@pagelist", pagelist);
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutError = (string)cmd.Parameters["@OutError"].Value;
            }
            catch (Exception ex)
            {
                OutError = "";
                result = "";
            }
            return result;
        }


         public DataSet GetAllAdminUser(int id, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllAdminUserPrimaryMiddle"; //for primary middle
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

         public DataSet GetAdminIdWithMenuId(int MenuId)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAdminIdWithMenuIdPrimaryMiddleSP";
            cmd.Parameters.AddWithValue("@MenuId", MenuId);
            return db.ExecuteDataSet(cmd);
        }

    

        //------------------------Delete Data------------------
 
         public string DeleteAdminUser(string id)
        {
            string result;
            ;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "DeleteAdminUser";
                cmd.Parameters.AddWithValue("@id", id);
                result = db.ExecuteNonQuery(cmd).ToString();      
                
            }
            catch (Exception ex)
            {               
                result = "";
            }
            return result;
        }

        #endregion Admin User Master

        #region Circular


         public string ListingCircular(int type, int id, out int OutStatus)
        {
            string result;
            
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ListingCircularSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@id", id);
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

         public DataSet CircularTypeMaster()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CircularTypeMasterSP";         
            return db.ExecuteDataSet(cmd);
        }
    
         public DataSet CircularMaster(string search, int pageIndex)
        {           
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CircularMasterSP";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 15);
            return db.ExecuteDataSet(cmd);
        }

         public int InsertCircularMaster(CircularModels FM, out string outCircularNo)
        {
            int result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertCircularMasterSP";
                cmd.Parameters.AddWithValue("@Type", FM.Type);
                cmd.Parameters.AddWithValue("@ID", FM.ID);
                cmd.Parameters.AddWithValue("@Session", FM.Session);
                cmd.Parameters.AddWithValue("@Title", FM.Title);
                cmd.Parameters.AddWithValue("@Attachment", FM.Attachment);
                cmd.Parameters.AddWithValue("@UrlLink", FM.UrlLink);
                cmd.Parameters.AddWithValue("@Category", FM.Category);
                cmd.Parameters.AddWithValue("@UploadDate", FM.UploadDate);
                cmd.Parameters.AddWithValue("@ExpiryDate", FM.ExpiryDate);
                cmd.Parameters.AddWithValue("@IsMarque", FM.IsMarque);
                cmd.Parameters.AddWithValue("@IsActive", FM.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", FM.CreatedBy);
                cmd.Parameters.AddWithValue("@UpdatedBy", FM.UpdatedBy);
                cmd.Parameters.AddWithValue("@CircularTypes", FM.SelectedCircularTypes);
                cmd.Parameters.Add("@OutId", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@CircularNo", SqlDbType.NVarChar, 30).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                int outuniqueid = (int)cmd.Parameters["@OutId"].Value;
                result = outuniqueid;
                outCircularNo = (string)cmd.Parameters["@CircularNo"].Value;
            }
            catch (Exception ex)
            {
                outCircularNo = "";
                result = -1;
            }
            return result;
            //ds = db.ExecuteDataSet(cmd);
            //return ds.Tables[0];
        }
        #endregion



        #region Firm Exam Data Download


         public DataSet FirmExamDataDownload(int Type, string RegOpen, string FirmUser, string Search, out string ErrStatus)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FirmExamDataDownloadSPNew";
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@RegOpen", RegOpen);
                cmd.Parameters.AddWithValue("@FirmUser", FirmUser);
                cmd.Parameters.AddWithValue("@Search", Search);
                ErrStatus = "1";
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                ErrStatus = ex.Message;
                return  null;
            }
        }


         public DataSet GetDataByIdandTypeRN(string id, string type, string roll)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetDataByIdandTypeRN";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@roll", roll);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {                
                return null;
            }
        }


         public string CheckFirmExamDataDownloadMis(DataSet ds, out DataTable dt, string RP)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string ColName = ds.Tables[0].Columns[0].ColumnName.ToString();
                    if (ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string Std_id = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check Std_id " + Std_id + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check Std_id " + Std_id + " in row " + RowNo + ",  ";

                    }
                    else
                    {
                        if (RP != "")
                        {
                            DataSet ds1 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][0].ToString(), RP, ds.Tables[0].Rows[i][0].ToString());// Regular
                            if (ds1 == null || ds1.Tables.Count == 0 || ds1.Tables[0].Rows.Count == 0)
                            {
                                int RowNo = i + 2;
                                string Std_id = ds.Tables[0].Rows[i][0].ToString();
                                Result += "Please check Std_id " + Std_id + " Not Found in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check Std_id " + Std_id + "Not Found in row " + RowNo + ",  ";
                            }
                            else
                            {

                                if (ds1.Tables[0].Rows[0]["Verified"].ToString().ToLower() != "1")
                                {
                                    int RowNo = i + 2;
                                    string Std_id = ds.Tables[0].Rows[i][0].ToString();
                                    Result += "Please check Std_id " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";
                                    dt.Rows[i]["Status"] += "Please check Std_id " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";

                                }
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
            }
            return Result;
        }


        #endregion Firm Exam Data Download


        #region Allot Regno

         public DataSet SelectForMigrateSchools(string Search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SelectForMigrateSchools_sp"; //SelectForMigrateSchools_sp
            cmd.Parameters.AddWithValue("@Search", Search);
            return db.ExecuteDataSet(cmd);
        }


         public DataSet GetAllFormNameBySchl(string SCHL)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetAllFormNameBySchlSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
           
        }

         public DataSet GetStudentRegNoNotAlloted(string search, string schl, int pageNumber)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetStudentRegNoNotAllotedSPPaging"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
           
        }

         public string ErrorAllotRegno(string stdid, string storeid, int Action, int userid, string remarks)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ErrorAllotRegnoSP";
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@storeid", storeid);
                cmd.Parameters.AddWithValue("@action", Action);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@remarks", remarks);
                result = db.ExecuteNonQuery(cmd).ToString();             
            }
            catch (Exception ex)
            {              
                result = "";
            }
            return result;
        }

         public DataTable ManualAllotRegno(string stdid, string regno, out int OutStatus, int userid, string remarks)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ManualAllotRegnoSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@regno", regno);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@remarks", remarks);
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                if (ds.Tables.Count > 0)
                { return ds.Tables[0]; }
                else return null;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }

        }

         public DataSet ViewAllotRegNo(string search, string schl)
        {
            try
            {            
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ViewAllotRegNoSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                return  db.ExecuteDataSet(cmd);  
            }
            catch (Exception)
            {             
                return null;
            }

        }

        public string RemoveRegno(string storeid, int Action, int userid)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RemoveRegnoSP";
                cmd.Parameters.AddWithValue("@storeid", storeid);
                cmd.Parameters.AddWithValue("@Action", Action);
                cmd.Parameters.AddWithValue("@userid", userid);
                result = db.ExecuteNonQuery(cmd).ToString();
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }



        #endregion

        #region Fee Entry
        public DataSet GetFeeCodeMaster(int Type, int FeeCode)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetFeeCodeMaster"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@FeeCode", FeeCode);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public DataSet GetAllFeeMaster2016(string search)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetAllFeeMaster2016SP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@search", search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public DataTable BulkFeeMaster(DataTable dt1, int adminid, out int OutStatus)
        {
            string OutError = "";
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BulkFeeMasterSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@ADMINID", adminid);
                cmd.Parameters.AddWithValue("@BulkFeeMaster", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                 ds =  db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds.Tables[0];
            }
            catch (Exception)
            {
                OutError = "";
                OutStatus = -1;
                return null;
            }

        }



        public int Insert_FeeMaster(FeeModels FM)
        {
            int result;
            ;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertFeeMaster2016SP";
                cmd.Parameters.AddWithValue("@ID", FM.ID);
                cmd.Parameters.AddWithValue("@Type", FM.Type);
                cmd.Parameters.AddWithValue("@FORM", FM.FORM);
                cmd.Parameters.AddWithValue("@FeeCat", FM.FeeCat);
                cmd.Parameters.AddWithValue("@Class", FM.Class);
                cmd.Parameters.AddWithValue("@StartDate", FM.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", FM.EndDate);
                cmd.Parameters.AddWithValue("@BankLastdate", FM.BankLastDate);
                cmd.Parameters.AddWithValue("@Fee", FM.Fee);
                cmd.Parameters.AddWithValue("@LateFee", FM.LateFee);
                cmd.Parameters.AddWithValue("@FeeCode", FM.FeeCode);
                cmd.Parameters.AddWithValue("@AllowBanks", FM.AllowBanks);
                cmd.Parameters.AddWithValue("@RP", FM.RP);
                cmd.Parameters.AddWithValue("@IsActive", FM.IsActive);
                cmd.Parameters.Add("@OutId", SqlDbType.Int, 4).Direction = ParameterDirection.Output;

                result = db.ExecuteNonQuery(cmd);             
                int outuniqueid = (int)cmd.Parameters["@OutId"].Value;
                return outuniqueid;
            }
            catch (Exception ex)
            {
                result = -1;
            }
            return result;            
        }


        #endregion



        #region  ExamErrorListMIS

        public DataSet GetErrorListMISByFirmId(int type, int adminid, out string OutResult)
        {           
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetErrorListMISByFirmId"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.Add("@OutResult", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutResult = (string)cmd.Parameters["@OutResult"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutResult = "0";
                return null;
            }

        }


        public DataSet ExamErrorListMIS(DataTable dt1, int adminid, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ExamErrorListMISSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@tblCandExamError", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }

        }


        public string CheckExamMisExcel(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("ErrStatus", typeof(string)));
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] = "Please check CANDID  " + CANDID + " in row " + RowNo + ",  ";

                    }

                    if (ds.Tables[0].Rows[i][1].ToString().Length != 7 || ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string schl = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check SCHL " + schl + " in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] += "Please check SCHL " + schl + " in row " + RowNo + ",  ";
                    }

                    //get data of candidate id 
                    DataSet ds1 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][0].ToString(), "R", ds.Tables[0].Rows[i][0].ToString());// Regular
                    if (ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                    {
                        if (ds1.Tables[0].Rows[0]["schl"].ToString() != ds.Tables[0].Rows[i][1].ToString())
                        {
                            int RowNo = i + 2;
                            string schl = ds.Tables[0].Rows[i][1].ToString();
                            Result += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                            dt.Rows[i]["ErrStatus"] += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                        }
                        else
                        {
                            if (ds.Tables[0].Rows[i][2].ToString() == "")
                            {
                                int RowNo = i + 2;
                                string errcode = ds.Tables[0].Rows[i][2].ToString();
                                Result += "Please check ERRCODE " + errcode + " in row " + RowNo + ",  ";
                                dt.Rows[i]["ErrStatus"] += "Please check ERRCODE " + errcode + " in row " + RowNo + ",  ";
                            }

                            if (ds.Tables[0].Rows[i][3].ToString() == "")
                            {
                                int RowNo = i + 2;
                                string errcode = ds.Tables[0].Rows[i][3].ToString();
                                Result += "Please check STATUS " + errcode + " in row " + RowNo + ",  ";
                                dt.Rows[i]["ErrStatus"] += "Please  check STATUS " + errcode + " in row " + RowNo + ",  ";
                            }

                            if (ds1.Tables[0].Rows[0]["Verified"].ToString().ToLower() != "1")
                            {
                                int RowNo = i + 2;
                                string Std_id = ds.Tables[0].Rows[i][0].ToString();
                                Result += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";

                            }
                        }

                    }
                    else
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check CANDID " + CANDID + " Not Found in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] = "Please check CANDID  " + CANDID + "  Not Found  in row " + RowNo + ",  ";

                    }

                }

            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }

        #endregion  ExamErrorListMIS


        #region  StudentRollNoMIS

        public string CheckStdRollNoMisOnly(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string ColName = ds.Tables[0].Columns[1].ColumnName.ToString();
                    if (ds.Tables[0].Rows[i][0].ToString().Length < 7 || ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SCHL = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";
                    }

                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";

                    }

                    DataSet ds1 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][1].ToString(), "R", ds.Tables[0].Rows[i][2].ToString());// Regular
                    if (ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                    {
                        if (ds1.Tables[0].Rows[0]["schl"].ToString() != ds.Tables[0].Rows[i][0].ToString())
                        {
                            int RowNo = i + 2;
                            string schl = ds.Tables[0].Rows[i][0].ToString();
                            Result += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                            dt.Rows[i]["Status"] += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                        }
                        else
                        {

                            if (ds.Tables[0].Rows[i][2].ToString().Length != 10 || ds.Tables[0].Rows[i][2].ToString() == "")
                            {
                                int RowNo = i + 2;
                                string roll = ds.Tables[0].Rows[i][2].ToString();
                                Result += "Please check ROLL No " + roll + " in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check ROLL No " + roll + " in row " + RowNo + ",  ";
                            }
                            else
                            {
                                // check roll duplicacy
                                DataSet ds2 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][1].ToString(), "RR", ds.Tables[0].Rows[i][2].ToString());// Regular
                                if (ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                                {
                                    int RowNo = i + 2;
                                    string roll = ds.Tables[0].Rows[i][2].ToString();
                                    Result += "Duplicate roll No " + roll + " in row " + RowNo + ",  ";
                                    dt.Rows[i]["Status"] += "Duplicate Roll No " + roll + " in row " + RowNo + ",  ";
                                }
                            }
                            if (ds1.Tables[0].Rows[0]["Verified"].ToString().ToLower() != "1")
                            {
                                int RowNo = i + 2;
                                string Std_id = ds.Tables[0].Rows[i][0].ToString();
                                Result += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";

                            }
                        }
                    }
                    else
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CANDID " + CANDID + " Not Found in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check CANDID " + CANDID + "  Not Found  in row " + RowNo + ",  ";

                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }


        public string CheckStdRollNoMis(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    string ColName = ds.Tables[0].Columns[1].ColumnName.ToString();
                    if (ds.Tables[0].Rows[i][0].ToString().Length < 7 || ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SCHL = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";
                    }

                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";

                    }

                    DataSet ds1 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][1].ToString(), "R", ds.Tables[0].Rows[i][2].ToString());// Regular
                    if (ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0)
                    {
                        if (ds1.Tables[0].Rows[0]["schl"].ToString() != ds.Tables[0].Rows[i][0].ToString())
                        {
                            int RowNo = i + 2;
                            string schl = ds.Tables[0].Rows[i][0].ToString();
                            Result += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                            dt.Rows[i]["Status"] += "Please check CandId and SCHL Not Matched in row " + RowNo + ",  ";
                        }
                        else
                        {

                            if (ds.Tables[0].Rows[i][2].ToString().Length != 10 || ds.Tables[0].Rows[i][2].ToString() == "")
                            {
                                int RowNo = i + 2;
                                string roll = ds.Tables[0].Rows[i][2].ToString();
                                Result += "Please check ROLL No " + roll + " in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check ROLL No " + roll + " in row " + RowNo + ",  ";
                            }
                            else
                            {
                                // check roll duplicacy
                                DataSet ds2 = GetDataByIdandTypeRN(ds.Tables[0].Rows[i][1].ToString(), "RR", ds.Tables[0].Rows[i][2].ToString());// Regular
                                if (ds2.Tables.Count > 0 && ds2.Tables[0].Rows.Count > 0)
                                {
                                    int RowNo = i + 2;
                                    string roll = ds.Tables[0].Rows[i][2].ToString();
                                    Result += "Duplicate roll No " + roll + " in row " + RowNo + ",  ";
                                    dt.Rows[i]["Status"] += "Duplicate Roll No " + roll + " in row " + RowNo + ",  ";
                                }
                            }

                            if (ds.Tables[0].Rows[i][3].ToString() == "")
                            {
                                int RowNo = i + 2;
                                string cent = ds.Tables[0].Rows[i][3].ToString();
                                Result += "Please check CENT " + cent + " in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check CENT " + cent + " in row " + RowNo + ",  ";
                            }

                            if (ds1.Tables[0].Rows[0]["Verified"].ToString().ToLower() != "1")
                            {
                                int RowNo = i + 2;
                                string Std_id = ds.Tables[0].Rows[i][0].ToString();
                                Result += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += "Please check Refno " + Std_id + " : Challan Not Verified in row " + RowNo + ",  ";

                            }
                        }
                    }
                    else
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CANDID " + CANDID + " Not Found in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check CANDID " + CANDID + "  Not Found  in row " + RowNo + ",  ";

                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }


        public DataSet StudentRollNoMIS(DataTable dt1, int adminid, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 300;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "StudentRollNoMISSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@tblStudentRollNo", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                return null;
            }

        }


        public DataSet StudentRollNoMISONLY(DataTable dt1, int adminid, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "StudentRollNoMISONLYSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@tblStudentRollNoOnly", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                return null;
            }

        }


        public string CheckStdRollNoRangeMis(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][1].ToString().Length < 7 || ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SCHL = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SCHL " + SCHL + " in row " + RowNo + ",  ";

                    }
                    if (ds.Tables[0].Rows[i][1].ToString().Length != 10 || ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string roll = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check Start ROLL No " + roll + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check Start ROLL No " + roll + " in row " + RowNo + ",  ";
                    }

                    if (ds.Tables[0].Rows[i][2].ToString().Length != 10 || ds.Tables[0].Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string roll = ds.Tables[0].Rows[i][2].ToString();
                        Result += "Please check End ROLL No " + roll + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check End ROLL No " + roll + " in row " + RowNo + ",  ";
                    }

                    if (ds.Tables[0].Rows[i][3].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string cent = ds.Tables[0].Rows[i][3].ToString();
                        Result += "Please check CENT " + cent + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check CENT " + cent + " in row " + RowNo + ",  ";
                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }

        public DataSet StudentRollNoRangeMIS(DataTable dt1, int adminid, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "StudentRollNoRangeMISSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@tblStudentRollNoRange", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }

        }

        #endregion  StudentRollNoMIS

        #region  ViewCandidateExamErrorList

        public DataSet ViewCandidateExamErrorList(int Type, string Search, string FirmNM, string CrType, int pageIndex)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ViewCandidateExamErrorListSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@firmNM", FirmNM);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", 30);
                cmd.Parameters.AddWithValue("@Type", Type);
                ds = db.ExecuteDataSet(cmd);               
                return ds;
            }
            catch (Exception)
            {            
                return null;
            }

        }

        public DataSet RemoveCandidateExamError(string username, string candid, string errcode)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RemoveCandidateExamErrorSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@candid", candid);
                cmd.Parameters.AddWithValue("@errcode", errcode);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch (Exception)
            {
                return null;
            }

        }


        #endregion  ViewCandidateExamErrorList


        #region AllowCCE

        public DataSet UnlockCCE(string Schl, string Type, int AdminId, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UnlockCCESP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@Schl", Schl);
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@AdminId", AdminId);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutStatus = 0;
                return null;
            }
        }

        public DataSet ListingSchoolAllowForMarksEntry(int Type, int id, string search, out int OutStatus)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ListingSchoolAllowForMarksEntrySP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutStatus = 0;
                return null;
            }
        }


        public int InsertSchoolAllowForMarksEntry(int type, SchoolAllowForMarksEntry FM, out string SchlMobile)
        {
            int result;

            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertSchoolAllowForMarksEntry";
                cmd.Parameters.AddWithValue("@Panel", FM.Panel);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@Id", FM.Id);
                cmd.Parameters.AddWithValue("@Schl", FM.Schl);
                cmd.Parameters.AddWithValue("@Cls", FM.Cls);
                cmd.Parameters.AddWithValue("@LastDate", FM.LastDate);
                cmd.Parameters.AddWithValue("@AllowTo", FM.AllowTo);
                cmd.Parameters.AddWithValue("@ReceiptNo", FM.ReceiptNo);
                cmd.Parameters.AddWithValue("@DepositDate", FM.DepositDate);
                cmd.Parameters.AddWithValue("@Amount", FM.Amount);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@SchlMobile", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                int outuniqueid = (int)cmd.Parameters["@OutStatus"].Value;
                SchlMobile = (string)cmd.Parameters["@SchlMobile"].Value;
                return outuniqueid;
            }
            catch (Exception ex)
            {
                SchlMobile = "";
                return result = -1;
            }
        }


        #endregion AllowCCE


        #region Practical Submission Unlocked
        public DataSet CheckPracFinalSubmission(string cls, string rp, string pcent, string sub, string fplot)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CheckPracFinalSubmission"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", pcent);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@fplot", fplot);
                ds = db.ExecuteDataSet(cmd);                
                return ds;
            }
            catch (Exception ex)
            {               
                return null;
            }
        }


        public string CheckPracticalSubmissionUnlocked(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CLASS = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check CLASS " + CLASS + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check CLASS " + CLASS + " in row " + RowNo + ",  ";

                    }
                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string RP = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check RP " + RP + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check RP " + RP + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string PCENT = ds.Tables[0].Rows[i][2].ToString();
                        Result += "Please check PCENT " + PCENT + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check PCENT " + PCENT + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][3].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SUB = ds.Tables[0].Rows[i][3].ToString();
                        Result += "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][4].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string LOT = ds.Tables[0].Rows[i][4].ToString();
                        Result += "Please check LOT " + LOT + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check LOT " + LOT + " in row " + RowNo + ",  ";
                    }


                    DataSet ds1 = CheckPracFinalSubmission(ds.Tables[0].Rows[i][0].ToString(), ds.Tables[0].Rows[i][1].ToString(), ds.Tables[0].Rows[i][2].ToString(), ds.Tables[0].Rows[i][3].ToString(), ds.Tables[0].Rows[i][4].ToString());// Regular
                    if (ds1 == null || ds1.Tables.Count == 0 || ds1.Tables[0].Rows.Count == 0)
                    {
                        int RowNo = i + 2;
                        string REFNO = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check Practical Submission are Not Found in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += "Please check Practical Submission are Not Found in row " + RowNo + ",  ";
                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }


        public DataSet PracticalSubmissionUnlocked(string cls, string rp, string cent, string sub, string fplot, int adminid, out string OutError)  
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracticalSubmissionUnlocked"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@fplot", fplot);
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;

                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "-1";
                return null;
            }
        }

        #endregion Practical Submission Unlocked


        #region PracticalCentUpdateMaster

        public DataSet GetPrivateSubjectByRefnoSub(string refno, string sub)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPrivateSubjectByRefnoSub"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@REFNO", refno);
                cmd.Parameters.AddWithValue("@SUB", sub);
                ds = db.ExecuteDataSet(cmd);   
                return ds;
            }
            catch (Exception ex)
            {                
                return null;
            }
        }

        public DataSet CheckPracticalCenter(string cent, string cls)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CheckPracticalCenter"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@cls", cls);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataSet PracticalCentUpdateMaster(DataTable dt1, int adminid, string examtype, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracCentALLSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@PracCentALL", dt1);
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@examtype", examtype);
                cmd.Parameters.AddWithValue("@cls", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "-1";
                return null;
            }
        }


        public DataSet PracCentSTD(DataTable dt1, int adminid, string examtype, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracCentSTDSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@examtype", examtype);
                cmd.Parameters.AddWithValue("@cls", cls);
                cmd.Parameters.AddWithValue("@PracCentSTD", dt1);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                OutError = "-1";
                return null;
            }
        }


        public string CheckRegOpenPracticalCentUpdateMaster(DataSet ds, string cls, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                if (ds.Tables[0].Rows.Count == 0)
                {
                    string ROLL = ds.Tables[0].Rows[0][0].ToString();
                    Result += "Data Not Found";
                    dt.Rows[0]["Status"] = "Data Not Found in file, please change in Text Format ";
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][6].ToString() != "" && cls != "")
                    {
                        int RowNo = i + 2;
                        DataSet dtp = CheckPracticalCenter(ds.Tables[0].Rows[i][6].ToString(), cls);
                        if (dtp.Tables.Count > 0)
                        {
                            if (dtp.Tables[0].Rows.Count == 0)
                            {
                                Result += "Please check Practical Centre is not allowed for this class in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] = "Please check Practical Centre is not allowed for this class in row" + RowNo + ",  ";
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }

        public string CheckStdPracticalCentUpdateMaster(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                if (ds.Tables[0].Rows.Count == 0)
                {
                    string ROLL = ds.Tables[0].Rows[0][0].ToString();
                    Result += "Data Not Found";
                    dt.Rows[0]["Status"] = "Data Not Found in file, please change in Text Format ";
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CANDID = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check CANDID " + CANDID + " in row " + RowNo + ",  ";

                    }
                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string CENT = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CENT " + CENT + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check CENT " + CENT + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SUB = ds.Tables[0].Rows[i][2].ToString();
                        Result += "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }

        public string CheckPrivatePracticalCentUpdateMaster(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("Status", typeof(string)));

                if (ds.Tables[0].Rows.Count == 0)
                {
                    string ROLL = ds.Tables[0].Rows[0][0].ToString();
                    Result += "Data Not Found";
                    dt.Rows[0]["Status"] = "Data Not Found in file, please change in Text Format ";
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string ROLL = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check ROLL " + ROLL + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check ROLL " + ROLL + " in row " + RowNo + ",  ";

                    }
                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string REFNO = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check REFNO " + REFNO + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check REFNO " + REFNO + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string PRC_CENT = ds.Tables[0].Rows[i][2].ToString();
                        Result += "Please check PRC_CENT " + PRC_CENT + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check PRC_CENT " + PRC_CENT + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][3].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string SUB = ds.Tables[0].Rows[i][3].ToString();
                        Result += "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check SUB " + SUB + " in row " + RowNo + ",  ";
                    }
                    if (ds.Tables[0].Rows[i][1].ToString() != "" && ds.Tables[0].Rows[i][3].ToString() != "")
                    {
                        int RowNo = i + 2;
                        DataSet dtp = GetPrivateSubjectByRefnoSub(ds.Tables[0].Rows[i][1].ToString(), ds.Tables[0].Rows[i][3].ToString());
                        if (dtp.Tables.Count > 0)
                        {
                            if (dtp.Tables[1].Rows.Count == 0)
                            {
                                Result += "Please check REFNO are Not Found in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] = "Please check REFNO Not Found in row" + RowNo + ",  ";
                            }
                            if (dtp.Tables[0].Rows.Count == 0)
                            {
                                Result += "Please check REFNO and SUB are not Matched in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] = "Please check REFNO and SUB are not Matched in row" + RowNo + ",  ";
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }

        #endregion PracticalCentUpdateMaster


        #region UnlockClusterTheoryMarks
        public DataSet UnlockClusterTheoryMarks(DataTable dt1, int adminid, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UnlockClusterTheoryMarksSP"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@tblUnlockClusterTheoryMarks", dt1);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
                
            }
            catch (Exception ex)
            {
                OutError = "-1";
                return null;
            }

        }


        public string CheckUnlockClusterTheoryExcel(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("ErrStatus", typeof(string)));
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (ds.Tables[0].Rows[i][0].ToString().Length !=7 || ds.Tables[0].Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string schl = ds.Tables[0].Rows[i][0].ToString();
                        Result += "Please check SCHL " + schl + " in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] += "Please check SCHL " + schl + " in row " + RowNo + ",  ";

                    }

                    if (ds.Tables[0].Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string ccode = ds.Tables[0].Rows[i][1].ToString();
                        Result += "Please check CCODE " + ccode + " in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] += "Please check CCODE " + ccode + " in row " + RowNo + ",  ";
                    }

                    if (ds.Tables[0].Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string Remarks = ds.Tables[0].Rows[i][2].ToString();
                        Result += "Please Enter Remarks " + Remarks + " in row " + RowNo + ",  ";
                        dt.Rows[i]["ErrStatus"] += "Please Enter Remarks " + Remarks + " in row " + RowNo + ",  ";
                    }

                    

                }

            }
            catch (Exception ex)
            {
                dt = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }


        #endregion

        #region Admin Result Update MIS
        public string CheckResultMisExcel(DataTable dt, string userNM, out DataTable dt1)
        {
            string Result = "";
            try
            {
                dt1 = dt;
                dt1.Columns.Add(new DataColumn("Status", typeof(string)));
                string itemFirm = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    itemFirm = "SAI,PERF";
                    if (!AbstractLayer.StaticDB.ContainsValue(itemFirm.ToUpper(), dt.Rows[i][0].ToString().ToUpper()))
                    {
                        int RowNo = i + 2;
                        string FirmNM = dt.Rows[i][0].ToString().ToUpper();
                        Result += "Please check FIRM " + FirmNM + " in row " + RowNo + ",  ";
                        dt1.Rows[i]["Status"] += "Please check FIRM " + FirmNM + " in row " + RowNo + ",  ";
                    }
                    if (dt.Rows[i][12].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string ID = dt.Rows[i][12].ToString();
                        Result += "Please check ID " + ID + " in row " + RowNo + ",  ";
                        dt1.Rows[i]["Status"] += "Please check ID " + ID + " in row " + RowNo + ",  ";
                    }

                    if (dt.Rows[i][13].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string Roll = dt.Rows[i][13].ToString();
                        Result += "Please check Roll " + Roll + " in row " + RowNo + ",  ";
                        dt1.Rows[i]["Status"] += "Please check Roll " + Roll + " in row " + RowNo + ",  ";
                    }
                }

            }
            catch (Exception ex)
            {
                dt1 = null;
                Result = ex.Message;
                //return "Please Check All Fields";
            }
            return Result;
        }


        public DataSet AdminResultUpdateMIS(DataTable dt1, int adminid, out string OutError)  
        {
            int OutStatus;
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "AdminResultUpdateMIS_SPNew"; //GetAllFormName_sp
                cmd.Parameters.AddWithValue("@adminid", adminid);
                cmd.Parameters.AddWithValue("@TypetblAdminResultUpdateMIS", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;

            }
            catch (Exception ex)
            {
                OutError = ex.Message;
                OutStatus = -1;
                return null;
            }

        }


       
        #endregion  Admin Result Update MIS


    }
}