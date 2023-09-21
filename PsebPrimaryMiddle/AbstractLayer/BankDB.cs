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
using System.Globalization;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebPrimaryMiddle.Repository;

namespace PsebJunior.AbstractLayer
{
    public class BankDB : IBankRepository
    {
        public DataSet GetBankDataByBCODE(BankModels BM, out int OutStatus)  // GetBankDataByBCODE
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetBankDataByBCODESP";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            //result = db.ExecuteNonQuery(cmd);
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }

        public BankLoginSession BankLogin(LoginModel LM)  // Type 1=Regular, 2=Open
        {
            BankLoginSession bankLoginSession = new BankLoginSession();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BankLoginSP";// LoginSP(old)
            cmd.Parameters.AddWithValue("@UserName", LM.UserName);
            cmd.Parameters.AddWithValue("@Password", LM.Password);
            using (IDataReader reader = db.ExecuteReader(cmd))
            {
                if (reader.Read())
                {
                    bankLoginSession.BANKNAME = DBNull.Value != reader["BANKNAME"] ? (string)reader["BANKNAME"] : default(string);
                    bankLoginSession.BCODE = DBNull.Value != reader["BCODE"] ? (string)reader["BCODE"] : default(string);
                    bankLoginSession.STATUS = DBNull.Value != reader["STATUS"] ? (int)reader["STATUS"] : default(int);                  
                }
            }
            return bankLoginSession;
        }


        public DataSet GetErrorDetails()  // GetBankDataByBCODE
        {
            try
            {               
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetErrorDetails";
                return db.ExecuteDataSet(cmd); 
            }
            catch (Exception)
            {                
                return null;
            }

        }

        public DataSet UpdateBankDataByBCODE(BankModels BM, out int OutStatus)   // GetBankDataByBCODE
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdateBankDataByBCODESP";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.AddWithValue("@ADDRESS", BM.ADDRESS);
            cmd.Parameters.AddWithValue("@DISTRICT", BM.DISTRICT);
            cmd.Parameters.AddWithValue("@PINCODE", BM.PINCODE);
            cmd.Parameters.AddWithValue("@MOBILE", BM.MOBILE);
            cmd.Parameters.AddWithValue("@STD", BM.STD);
            cmd.Parameters.AddWithValue("@PHONE", BM.PHONE);
            cmd.Parameters.AddWithValue("@EMAILID1", BM.EMAILID1);
            cmd.Parameters.AddWithValue("@EMAILID2", BM.EMAILID2);
            cmd.Parameters.AddWithValue("@ACNO", BM.ACNO);
            cmd.Parameters.AddWithValue("@IFSC", BM.IFSC);
            cmd.Parameters.AddWithValue("@MICR", BM.MICR);
            cmd.Parameters.AddWithValue("@buser_id", BM.buser_id);
            cmd.Parameters.AddWithValue("@password", BM.password);
            cmd.Parameters.AddWithValue("@BRNCODE", BM.BRNCODE);
            cmd.Parameters.AddWithValue("@NODAL_BRANCH", BM.NODAL_BRANCH);
            cmd.Parameters.AddWithValue("@MNAGER_NM", BM.MNAGER_NM);
            cmd.Parameters.AddWithValue("@TECHNICAL_PERSON", BM.TECHNICAL_PERSON);
            cmd.Parameters.AddWithValue("@OTCONTACT", BM.OTCONTACT);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);            
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }

        public DataSet GetAllFeeDepositByBCODE(BankModels BM, string search, out string OutError)   // GetBankDataByBCODE
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllFeeDepositByBCODE";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            //result = db.ExecuteNonQuery(cmd);
            OutError = (string)cmd.Parameters["@OutError"].Value;
            return ds;
        }


        public DataSet DownloadChallan(BankModels BM, out int OutStatus)  // BankLoginSP
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DownloadChallanSP";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.AddWithValue("@Session", BM.Session);
            cmd.Parameters.AddWithValue("@DOWNLDFLG", BM.DOWNLDFLG);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            //result = db.ExecuteNonQuery(cmd);
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }

        public DataSet BankMisDetails(BankModels BM, int Type1, out int OutStatus)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "BankMisDetailsSp";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.AddWithValue("@LOT", BM.LOT);
            cmd.Parameters.AddWithValue("@Type", Type1);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            //result = db.ExecuteNonQuery(cmd);
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }

        public DataSet GetTotBankMIS(BankModels BM)  // GetBankDataByBCODE
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GettotMis_Sp";
                cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }


        public DataSet ChangePasswordBank(BankModels BM, out int OutStatus)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ChangePasswordBank";
            cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
            cmd.Parameters.AddWithValue("@oldpassword", BM.OldPassword);
            cmd.Parameters.AddWithValue("@newpassword", BM.Newpassword);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            //result = db.ExecuteNonQuery(cmd);
            OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
            return ds;
        }


        public DataTable BulkChallanBank(DataTable dt1, int adminid, int UPLOADLOT, BankModels BM, out int OutStatus, out string OutError)  // BulkChallanBank
        {
            try
            {

                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BulkChallanBankSP";
                cmd.Parameters.AddWithValue("@ADMINID", adminid);
                cmd.Parameters.AddWithValue("@MIS_FILENM", BM.MIS_FILENM);
                cmd.Parameters.AddWithValue("@IP_ADD", myIP);
                cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
                cmd.Parameters.AddWithValue("@UPLOADLOT", UPLOADLOT);
                cmd.Parameters.AddWithValue("@BulkChallanBank", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return dt1;
            }
            catch (Exception ex)
            {
                OutError = ex.Message;
                OutStatus = -1;
                return null;
            }
        }


      public DataSet BankMisDetailsSearch(string search)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BankMisDetailsSpSearch";
                cmd.Parameters.AddWithValue("@search", search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }



        public DataTable BulkChallanBankPSEBHOD(DataTable dt1, int adminid, int UPLOADLOT, BankModels BM, out int OutStatus)  // BulkChallanBank
        {
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "BulkChallanBankPSEBHOD";
                cmd.Parameters.AddWithValue("@ADMINID", adminid);
                cmd.Parameters.AddWithValue("@MIS_FILENM", BM.MIS_FILENM);
                cmd.Parameters.AddWithValue("@IP_ADD", myIP);
                cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
                cmd.Parameters.AddWithValue("@UPLOADLOT", UPLOADLOT);
                cmd.Parameters.AddWithValue("@BulkChallanBank", dt1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds.Tables[0];
            }
            catch (Exception)
            {                
                OutStatus = -1;
                return null;
            }
        }


        public DataSet GetChallanDetailsByIdSPBank(string ChallanId)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetChallanDetailsByIdSPBank";
                cmd.Parameters.AddWithValue("@CHALLANID", ChallanId);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }


        public DataSet GetChallanDetails(string search, string sesssion, int pageIndex)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetChallanDetails";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }

        }


       
        public DataTable ImportBankMis(BankModels BM, int UPLOADLOT, out int OutStatus, out string Mobile)
        {
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ImportBankMisSPJunior";
                cmd.Parameters.AddWithValue("@CHALLANID", BM.CHALLANID);
                cmd.Parameters.AddWithValue("@TOTFEE", BM.TOTFEE);
                cmd.Parameters.AddWithValue("@BRCODE", BM.BRCODE);
                cmd.Parameters.AddWithValue("@BRANCH", BM.BRANCH);
                cmd.Parameters.AddWithValue("@J_REF_NO", BM.J_REF_NO);
                cmd.Parameters.AddWithValue("@DEPOSITDT", BM.DEPOSITDT);
                cmd.Parameters.AddWithValue("@MIS_FILENM", BM.MIS_FILENM);
                cmd.Parameters.AddWithValue("@IP_ADD", myIP);
                cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
                cmd.Parameters.AddWithValue("@UPLOADLOT", UPLOADLOT);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Mobile", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                Mobile = (string)cmd.Parameters["@Mobile"].Value;
                return ds.Tables[0];
            }
            catch (Exception)
            {
                OutStatus = -1;
                Mobile = "0";
                return null;
            }
        }



        public DataTable ImportBankMisSPJuniorPSEB(BankModels BM, int UPLOADLOT, out int OutStatus, out string Mobile)
        {
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ImportBankMisSPJuniorPSEB";
                cmd.Parameters.AddWithValue("@CHALLANID", BM.CHALLANID);
                cmd.Parameters.AddWithValue("@TOTFEE", BM.TOTFEE);
                cmd.Parameters.AddWithValue("@BRCODE", BM.BRCODE);
                cmd.Parameters.AddWithValue("@BRANCH", BM.BRANCH);
                cmd.Parameters.AddWithValue("@J_REF_NO", BM.J_REF_NO);
                cmd.Parameters.AddWithValue("@DEPOSITDT", BM.DEPOSITDT);
                cmd.Parameters.AddWithValue("@MIS_FILENM", BM.MIS_FILENM);
                cmd.Parameters.AddWithValue("@IP_ADD", myIP);
                cmd.Parameters.AddWithValue("@BCODE", BM.BCODE);
                cmd.Parameters.AddWithValue("@UPLOADLOT", UPLOADLOT);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Mobile", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                Mobile = (string)cmd.Parameters["@Mobile"].Value;
                return ds.Tables[0];
            }
            catch (Exception)
            {
                OutStatus = -1;
                Mobile = "0";
                return null;
            }
        }


        public string CheckMisExcelExportRN(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("STATUS", typeof(string)));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][0].ToString().Length < 13 || dt.Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string challanid = dt.Rows[i][0].ToString();
                        Result = "Please check ChallanId " + challanid + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check ChallanId " + challanid + " in row " + RowNo + ",  ";
                    }


                    int totalfeeamount = 0;
                    if (dt.Rows[i][1].ToString() != "")
                    {
                        totalfeeamount = Convert.ToInt32(dt.Rows[i][1].ToString());
                    }
                    if (totalfeeamount <= 0 || dt.Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        //** gcm 
                        //dt.Rows[i]["ErrDetails"]+="Total Fee Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of Total Fees in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of Total Fees in row " + RowNo + ",  ";
                    }

                    DataSet dsChallanDetails = GetChallanDetailsByIdSPBank(dt.Rows[i]["ChallanID"].ToString());
                    if (dsChallanDetails == null || dsChallanDetails.Tables[0].Rows.Count == 0)
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "Invalid Challan Id";
                        string challanid = dt.Rows[i][0].ToString();
                        //Result += "Please check ChallanId " + challanid + " of  Total Fees and challan id in row " + RowNo + ",   ";
                        Result += "Please check ChallanId " + challanid + " for Challan Details/NotFound    in row " + RowNo + ",   ";
                        dt.Rows[i]["Status"] = "Please check ChallanId " + challanid + " for Challan Details/NotFound    in row " + RowNo + ",   ";
                    }
                    else
                    {
                        if (Convert.ToInt32(dsChallanDetails.Tables[0].Rows[0]["TOTFEE"].ToString()) != totalfeeamount)
                        {
                            int RowNo = i + 2;
                            // dt.Rows[i]["ErrDetails"] += "Total Fee Error";
                            string challanid = dt.Rows[i][0].ToString();
                            Result += "Please check ChallanId " + challanid + " of Total Fees not matched with challan  in row " + RowNo + ",   ";
                            dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of Total Fees not matched with challan  in row " + RowNo + ",   ";
                        }
                    }


                    if (dt.Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "BRCODE Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of BRCODE in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = dt.Rows[i]["Status"].ToString() + " , " + "Please check ChallanId " + challanid + " of BRCODE in row " + RowNo + ",  ";
                    }



                    if (dt.Rows[i][3].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "BRANCH Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of BRANCH in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of BRANCH in row " + RowNo + ",  ";
                    }
                    else if (dt.Rows[i][3].ToString().Length > 75)
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "BRANCH Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of BRANCH in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of BRANCH Length Can't be greater than 75 Characters in row " + RowNo + ",  ";
                    }



                    if (dt.Rows[i][4].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "J_REF_NO Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of J_REF_NO in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of J_REF_NO in row " + RowNo + ",  ";
                    }

                    // DataSet dsChallanDetails = GetChallanDetailsByIdSPBank(dt.Rows[i]["ChallanID"].ToString());
                    //24/08/2016 05:55:38PM
                    if (dt.Rows[i][5].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " for Fill Date in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " for Fill Date in row " + RowNo + ",  ";
                    }
                    else if (dt.Rows[i][5].ToString() != "")
                    {
                        if (dsChallanDetails.Tables[0].Rows.Count > 0)
                        {
                            DateTime LastDepositDate = Convert.ToDateTime(dsChallanDetails.Tables[0].Rows[0]["BankLastdateNew"].ToString());
                            DateTime GenDate = Convert.ToDateTime(dsChallanDetails.Tables[0].Rows[0]["ChallanGDateN"].ToString());
                            // 24/08/2016 05:55:38PM
                            DateTime fromDateValue;
                            string s = dt.Rows[i][5].ToString();
                            //  string s = dt.Rows[i][5].ToString().Substring(0,10);
                            // var formats = new[] { "dd/MM/yyyy hh:mm:sstt", "yyyy-MM-dd" };                      
                            if (DateTime.TryParseExact(s, "dd/MM/yyyy hh:mm:sstt", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue))
                            { // do for valid date 

                                if (fromDateValue.Date < GenDate.Date)
                                {
                                    int RowNo = i + 2;
                                    string challanid = ds.Tables[0].Rows[i][0].ToString();
                                    Result += "Please Check ChallanId " + challanid + " for Wrong Deposit Date  in row " + RowNo + ",  ";
                                    dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Wrong Deposit Date in row " + RowNo + ",  ";
                                }
                                if (fromDateValue.Date > LastDepositDate.Date)
                                {
                                    int RowNo = i + 2;
                                    string challanid = ds.Tables[0].Rows[i][0].ToString();
                                    Result += "Please Check ChallanId " + challanid + " for Wrong Deposit Date  in row " + RowNo + ",  ";
                                    dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Wrong Deposit Date in row " + RowNo + ",  ";
                                }

                            }
                            else
                            {
                                int RowNo = i + 2;
                                string challanid = dt.Rows[i][0].ToString();
                                Result += "Please Check ChallanId " + challanid + " for Date Format in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Date Format in row " + RowNo + ",  ";
                                // do for invalid date
                            }

                            if (dsChallanDetails.Tables[0].Rows[0]["VERIFIED"].ToString() == "1")
                            {
                                int RowNo = i + 2;
                                // dt.Rows[i]["ErrDetails"] += "Total Fee Error";
                                string challanid = dt.Rows[i][0].ToString();
                                Result += "Please check ChallanId " + challanid + " is Already Verified  in row " + RowNo + ",   ";
                                dt.Rows[i]["Status"] = "Already Verified";
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
            //insertErrorFromExcel(Result);

            return Result;
        }



        public string CheckMisExcelExportPSEB(DataSet ds, out DataTable dt)
        {
            string Result = "";
            try
            {
                dt = ds.Tables[0];
                dt.Columns.Add(new DataColumn("STATUS", typeof(string)));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][0].ToString().Length < 13 || dt.Rows[i][0].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string challanid = dt.Rows[i][0].ToString();
                        Result = "Please check ChallanId " + challanid + " in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] = "Please check ChallanId " + challanid + " in row " + RowNo + ",  ";
                    }

                    //

                    int totalfeeamount = 0;
                    if (dt.Rows[i][1].ToString() != "")
                    {
                        totalfeeamount = Convert.ToInt32(dt.Rows[i][1].ToString());
                    }
                    if (totalfeeamount <= 0 || dt.Rows[i][1].ToString() == "")
                    {
                        int RowNo = i + 2;
                        //** gcm 
                        //dt.Rows[i]["ErrDetails"]+="Total Fee Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of Fees in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of Fees in row " + RowNo + ",  ";
                    }

                    //                    
                    DataSet dsChallanDetails = GetChallanDetailsByIdSPBank(dt.Rows[i]["ChallanID"].ToString());
                    if (dsChallanDetails == null || dsChallanDetails.Tables[0].Rows.Count == 0)
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "Invalid Challan Id";
                        string challanid = dt.Rows[i][0].ToString();
                        //Result += "Please check ChallanId " + challanid + " of  Total Fees and challan id in row " + RowNo + ",   ";
                        Result += "Please check ChallanId " + challanid + " for Challan Details/NotFound    in row " + RowNo + ",   ";
                        dt.Rows[i]["Status"] = "Please check ChallanId " + challanid + " for Challan Details/NotFound    in row " + RowNo + ",   ";
                    }
                    else
                    {
                        if (Convert.ToInt32(dsChallanDetails.Tables[0].Rows[0]["FEE"].ToString()) != totalfeeamount)
                        {
                            if (Convert.ToInt32(dsChallanDetails.Tables[0].Rows[0]["TOTFEE"].ToString()) != totalfeeamount)
                            {
                                int RowNo = i + 2;
                                // dt.Rows[i]["ErrDetails"] += "Total Fee Error";
                                string challanid = dt.Rows[i][0].ToString();
                                Result += "Please check ChallanId " + challanid + " of Total Fees not matched with challan  in row " + RowNo + ",   ";
                                dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of Total Fees not matched with challan  in row " + RowNo + ",   ";
                            }
                        }
                    }
                    //
                    if (dt.Rows[i][2].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "BRCODE Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of BRCODE in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of BRCODE in row " + RowNo + ",  ";
                    }

                    //
                    if (dt.Rows[i][3].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "BRANCH Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of BRANCH in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of BRANCH in row " + RowNo + ",  ";
                    }

                    //
                    if (dt.Rows[i][4].ToString() == "")
                    {
                        int RowNo = i + 2;
                        // dt.Rows[i]["ErrDetails"] += "J_REF_NO Error";
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " of J_REF_NO in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " of J_REF_NO in row " + RowNo + ",  ";
                    }

                    //

                    // DataSet dsChallanDetails = GetChallanDetailsByIdSPBank(dt.Rows[i]["ChallanID"].ToString());
                    //24/08/2016 05:55:38PM
                    if (dt.Rows[i][5].ToString() == "")
                    {
                        int RowNo = i + 2;
                        string challanid = dt.Rows[i][0].ToString();
                        Result += "Please check ChallanId " + challanid + " for Fill Date in row " + RowNo + ",  ";
                        dt.Rows[i]["Status"] += " , " + "Please check ChallanId " + challanid + " for Fill Date in row " + RowNo + ",  ";
                    }
                    else if (dt.Rows[i][5].ToString() != "")
                    {
                        if (dsChallanDetails.Tables[0].Rows.Count > 0)
                        {
                            DateTime LastDepositDate = Convert.ToDateTime(dsChallanDetails.Tables[0].Rows[0]["BankLastdateNew"].ToString());
                            DateTime GenDate = Convert.ToDateTime(dsChallanDetails.Tables[0].Rows[0]["ChallanGDateN"].ToString());
                            // 24/08/2016 05:55:38PM
                            DateTime fromDateValue;
                            string s = dt.Rows[i][5].ToString();
                            //  string s = dt.Rows[i][5].ToString().Substring(0,10);
                            // var formats = new[] { "dd/MM/yyyy hh:mm:sstt", "yyyy-MM-dd" };                      
                            if (DateTime.TryParseExact(s, "dd/MM/yyyy hh:mm:sstt", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue))
                            { // do for valid date 

                                //if (fromDateValue.Date < GenDate.Date)
                                //{
                                //    int RowNo = i + 2;
                                //    string challanid = ds.Tables[0].Rows[i][0].ToString();
                                //    Result += "Please Check ChallanId " + challanid + " for Wrong Deposit Date  in row " + RowNo + ",  ";
                                //    dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Wrong Deposit Date in row " + RowNo + ",  ";

                                //}
                                //if (fromDateValue.Date > LastDepositDate.Date)
                                //{
                                //    int RowNo = i + 2;
                                //    string challanid = ds.Tables[0].Rows[i][0].ToString();
                                //    Result += "Please Check ChallanId " + challanid + " for Wrong Deposit Date  in row " + RowNo + ",  ";
                                //    dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Wrong Deposit Date in row " + RowNo + ",  ";

                                //}
                            }
                            else
                            {

                                int RowNo = i + 2;
                                string challanid = dt.Rows[i][0].ToString();
                                Result += "Please Check ChallanId " + challanid + " for Date Format in row " + RowNo + ",  ";
                                dt.Rows[i]["Status"] += " , " + "Please Check ChallanId " + challanid + " for Date Format in row " + RowNo + ",  ";
                                // do for invalid date
                            }

                            if (dsChallanDetails.Tables[0].Rows[0]["VERIFIED"].ToString() == "1")
                            {
                                int RowNo = i + 2;
                                // dt.Rows[i]["ErrDetails"] += "Total Fee Error";
                                string challanid = dt.Rows[i][0].ToString();
                                Result += "Please check ChallanId " + challanid + " is Already Verified  in row " + RowNo + ",   ";
                                dt.Rows[i]["Status"] = "Already Verified";
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

            //insertErrorFromExcel(Result);

            return Result;
        }

       
    }

}