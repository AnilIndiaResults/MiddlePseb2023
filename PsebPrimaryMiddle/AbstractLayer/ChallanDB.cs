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
using System.Web.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebPrimaryMiddle.Repository;

namespace PsebJunior.AbstractLayer
{
    public class ChallanDB : IChallanRepository
    {
               

        #region ShiftChallanDetails

        public DataSet ShiftChallanDetails(ShiftChallanDetails scd, out string OutError, out int OutSID)
        {
            DataSet result = new DataSet();
            try
            {               
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ShiftChallanDetailsSP";
                cmd.Parameters.AddWithValue("@ShiftId", scd.ShiftId);
                cmd.Parameters.AddWithValue("@WrongChallan1", scd.WrongChallan);
                cmd.Parameters.AddWithValue("@CorrectChallan1", scd.CorrectChallan); //.Replace('?','2')
                cmd.Parameters.AddWithValue("@ShiftFile", scd.ShiftFile);
                cmd.Parameters.AddWithValue("@ShiftRemarks", scd.ShiftRemarks);
                cmd.Parameters.AddWithValue("@Type", scd.ActionType);
                cmd.Parameters.AddWithValue("@AdminId", scd.AdminId);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutSID", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                result = db.ExecuteDataSet(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
                OutSID = (int)cmd.Parameters["@OutSID"].Value;
            }
            catch (Exception ex)
            {
                OutError = "";
                OutSID = -1;
                result = null;
            }
            return result;
           
        }



        public DataSet ViewShiftChallanDetails(int Sid, int Type, string search, int pageNumber, int PageSize)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ViewShiftChallanDetailsSP";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Sid", Sid);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
            cmd.Parameters.AddWithValue("@PageSize", PageSize);
            return db.ExecuteDataSet(cmd);
        }

        #endregion ShiftChallanDetails


        #region Challan Deposit Details
        public DataSet GetChallanDetailsByStudentList(string studentlist)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetChallanDetailsByStudentList";
            cmd.Parameters.AddWithValue("@studentlist", studentlist);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet ChallanDepositDetails(ChallanDepositDetailsModel cdm, out string OutError)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ChallanDepositDetailsSP";
            cmd.Parameters.AddWithValue("@CHALLANID", cdm.CHALLANID);
            cmd.Parameters.AddWithValue("@BRCODECAND", cdm.BRCODECAND);
            cmd.Parameters.AddWithValue("@BRANCHCAND", cdm.BRANCHCAND);
            cmd.Parameters.AddWithValue("@J_REF_NOCAND", cdm.J_REF_NOCAND);
            cmd.Parameters.AddWithValue("@DEPOSITDTCAND", cdm.DEPOSITDTCAND);
            cmd.Parameters.AddWithValue("@ChallanRemarks", cdm.challanremarks);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            OutError = (string)cmd.Parameters["@OutError"].Value;
            return ds;
        }


        public string ChallanDepositDetailsCancel(string cancelremarks, string challanid, out string outstatus, int AdminId)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ChallanDepositDetailsCancelSP";
            cmd.Parameters.AddWithValue("@CHALLANID", challanid);
            cmd.Parameters.AddWithValue("@AdminId", AdminId);
            cmd.Parameters.AddWithValue("@cancelremarks", cancelremarks);
            cmd.Parameters.Add("@OutStatus", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            outstatus = Convert.ToString(cmd.Parameters["@OutStatus"].Value);
            return outstatus;
        }
        #endregion



        #region Calculate Fee of Registration 

        public DataSet CancelOfflineChallanBySchl(string cancelremarks, string challanid, out string outstatus, string Schl, string Type)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CancelOfflineChallanBySchlSP";
            cmd.Parameters.AddWithValue("@CHALLANID", challanid);
            cmd.Parameters.AddWithValue("@Schl", Schl);
            cmd.Parameters.AddWithValue("@cancelremarks", cancelremarks);
            cmd.Parameters.AddWithValue("@Type", Type);
            cmd.Parameters.Add("@OutStatus", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            outstatus = (string)cmd.Parameters["@OutStatus"].Value.ToString();
            return ds;
        }

        public DataSet GetFinalPrintChallan(string SchoolCode)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetFinalPrintChallanSPJunior";
            cmd.Parameters.AddWithValue("@SchoolCode", SchoolCode);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetCalculateFeeBySchool(string cls, string search, string schl, DateTime? date = null)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCalculateFeeBySchoolSP_Junior_Exemption";//GetCalculateFeeBySchoolSP_Junior
            cmd.Parameters.AddWithValue("@cls", cls);
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@Schl", schl);
            cmd.Parameters.AddWithValue("@date", date);
            ds= db.ExecuteDataSet(cmd);
            return ds;
        }

        public DataSet GetCalculateFeeBySchoolWithoutLateFee(string cls, string search, string schl, DateTime? date = null)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCalculateFeeBySchoolSP_Junior_ExemptionWithoutLateFee";//
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@Schl", schl);
            cmd.Parameters.AddWithValue("@date", date);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetSchoolLotDetails(string schl)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSchoolLotDetailsSP";
            cmd.Parameters.AddWithValue("@schl", schl);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet GetChallanDetailsById(string ChallanId)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetChallanDetailsByIdSP";
            cmd.Parameters.AddWithValue("@CHALLANID", ChallanId);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetMissingCheckFeeStatusSPJunior(string Search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetMissingCheckFeeStatusSPJunior";
            cmd.Parameters.AddWithValue("@Search", Search);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet CheckFeeStatus(string SCHL, string type, string id, DateTime date)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "CheckFeeStatusSPByViewJunior";
                cmd.CommandText = "CheckFeeStatusSPByViewForMidPrimary";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@form", id);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex )
            {
                return null;
            }
        }


        public DataSet GetChallanDetailsBySearch(string search)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetChallanDetailsBySearch";
                cmd.Parameters.AddWithValue("@search", search);
                ds = db.ExecuteDataSet(cmd);                
                return ds;
            }
            catch (Exception)
            {                
                return null;
            }
        }

        public DataSet ReGenerateChallaanByIdJunior(string ChallanId, string BCODE, string usertype, out int OutStatus, out string CHALLANIDOut)  // ReGenerateChallaan
        {
            try
            {


                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ReGenerateChallaanByIdBankSP";  //ReGenerateChallaanByIdJuniorSP
                cmd.Parameters.AddWithValue("@CHALLANID", ChallanId);
                cmd.Parameters.AddWithValue("@BCODE", BCODE);
                cmd.Parameters.AddWithValue("@type", usertype);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@CHALLANIDOut", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                CHALLANIDOut = (string)cmd.Parameters["@CHALLANIDOut"].Value;
                return ds;
            }
            catch (Exception)
            {
                OutStatus = -3;
                CHALLANIDOut = "-3";
                return null;
            }
        }

        public string InsertPaymentForm(ChallanMasterModel CM, out string SchoolMobile)
        {
            string result = "";

            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "InsertPaymentFormMainSP";
                cmd.Parameters.AddWithValue("@SchoolCode", CM.SchoolCode);
                cmd.Parameters.AddWithValue("@CHLNDATE", CM.CHLNDATE);
                cmd.Parameters.AddWithValue("@CHLNVDATE", CM.CHLNVDATE);
                cmd.Parameters.AddWithValue("@FEEMODE", CM.FEEMODE);
                cmd.Parameters.AddWithValue("@FEECODE", CM.FEECODE);
                cmd.Parameters.AddWithValue("@FEECAT", CM.FEECAT);
                cmd.Parameters.AddWithValue("@BCODE", CM.BCODE);
                cmd.Parameters.AddWithValue("@BANK", CM.BANK);
                cmd.Parameters.AddWithValue("@ACNO", CM.ACNO);
                cmd.Parameters.AddWithValue("@FEE", CM.FEE);
                cmd.Parameters.AddWithValue("@BANKCHRG", CM.BANKCHRG);
                cmd.Parameters.AddWithValue("@TOTFEE", CM.TOTFEE);
                cmd.Parameters.AddWithValue("@SCHLREGID", CM.SCHLREGID);
                cmd.Parameters.AddWithValue("@DIST", CM.DIST);
                cmd.Parameters.AddWithValue("@DISTNM", CM.DISTNM);
                cmd.Parameters.AddWithValue("@SCHLCANDNM", CM.SCHLCANDNM);
                cmd.Parameters.AddWithValue("@BRCODE", CM.BRCODE);
                cmd.Parameters.AddWithValue("@BRANCH", CM.BRANCH);
                //cmd.Parameters.AddWithValue("@DEPOSITDT", CM.DEPOSITDT);
                cmd.Parameters.AddWithValue("@addfee", CM.addfee);
                cmd.Parameters.AddWithValue("@latefee", CM.latefee);
                cmd.Parameters.AddWithValue("@prosfee", CM.prosfee);
                cmd.Parameters.AddWithValue("@addsubfee", CM.addsubfee);
                cmd.Parameters.AddWithValue("@add_sub_count", CM.add_sub_count);
                cmd.Parameters.AddWithValue("@regfee", CM.regfee);
                cmd.Parameters.AddWithValue("@type", CM.type);
                cmd.Parameters.AddWithValue("@LOT", CM.LOT);
                cmd.Parameters.AddWithValue("@FeeStudentList", CM.FeeStudentList);
                if (CM.LSFRemarks != null && CM.LSFRemarks != "")
                {
                    cmd.Parameters.AddWithValue("@LumsumFine", CM.LumsumFine);
                    cmd.Parameters.AddWithValue("@LSFRemarks", CM.LSFRemarks);
                }
                //cmd.Parameters.AddWithValue("@ChallanGDateN", CM.ChallanGDateN);
                cmd.Parameters.AddWithValue("@ChallanVDateN", CM.ChallanVDateN);
                cmd.Parameters.Add("@CHALLANID", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@SchoolMobile", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string outuniqueid = (string)cmd.Parameters["@CHALLANID"].Value;
                SchoolMobile = (string)cmd.Parameters["@SchoolMobile"].Value;
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                result = outuniqueid;
            }
            catch (Exception ex)
            {
                SchoolMobile = ex.Message;
                result = "";
            }
            return result;
        }


        public string InsertPaymentFormJunior(ChallanMasterModel CM, out string SchoolMobile)
        {
            string result = "";

            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "InsertPaymentFormMainSP";
                cmd.Parameters.AddWithValue("@SchoolCode", CM.SchoolCode);
                cmd.Parameters.AddWithValue("@CHLNDATE", CM.CHLNDATE);
                cmd.Parameters.AddWithValue("@CHLNVDATE", CM.CHLNVDATE);
                cmd.Parameters.AddWithValue("@FEEMODE", CM.FEEMODE);
                cmd.Parameters.AddWithValue("@FEECODE", CM.FEECODE);
                cmd.Parameters.AddWithValue("@FEECAT", CM.FEECAT);
                cmd.Parameters.AddWithValue("@BCODE", CM.BCODE);
                cmd.Parameters.AddWithValue("@BANK", CM.BANK);
                cmd.Parameters.AddWithValue("@ACNO", CM.ACNO);
                cmd.Parameters.AddWithValue("@FEE", CM.FEE);
                cmd.Parameters.AddWithValue("@BANKCHRG", CM.BANKCHRG);
                cmd.Parameters.AddWithValue("@TOTFEE", CM.TOTFEE);
                cmd.Parameters.AddWithValue("@SCHLREGID", CM.SCHLREGID);
                cmd.Parameters.AddWithValue("@DIST", CM.DIST);
                cmd.Parameters.AddWithValue("@DISTNM", CM.DISTNM);
                cmd.Parameters.AddWithValue("@SCHLCANDNM", CM.SCHLCANDNM);
                cmd.Parameters.AddWithValue("@BRCODE", CM.BRCODE);
                cmd.Parameters.AddWithValue("@BRANCH", CM.BRANCH);
                //cmd.Parameters.AddWithValue("@DEPOSITDT", CM.DEPOSITDT);
                cmd.Parameters.AddWithValue("@addfee", CM.addfee);
                cmd.Parameters.AddWithValue("@latefee", CM.latefee);
                cmd.Parameters.AddWithValue("@prosfee", CM.prosfee);
                cmd.Parameters.AddWithValue("@addsubfee", CM.addsubfee);
                cmd.Parameters.AddWithValue("@add_sub_count", CM.add_sub_count);
                cmd.Parameters.AddWithValue("@regfee", CM.regfee);
                cmd.Parameters.AddWithValue("@type", CM.type);
                cmd.Parameters.AddWithValue("@LOT", CM.LOT);
                cmd.Parameters.AddWithValue("@FeeStudentList", CM.FeeStudentList);
                if (CM.LSFRemarks != null && CM.LSFRemarks != "")
                {
                    cmd.Parameters.AddWithValue("@LumsumFine", CM.LumsumFine);
                    cmd.Parameters.AddWithValue("@LSFRemarks", CM.LSFRemarks);
                }
                //cmd.Parameters.AddWithValue("@ChallanGDateN", CM.ChallanGDateN);
                cmd.Parameters.AddWithValue("@ChallanVDateN", CM.ChallanVDateN);
                cmd.Parameters.Add("@CHALLANID", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@SchoolMobile", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string outuniqueid = (string)cmd.Parameters["@CHALLANID"].Value;
                SchoolMobile = (string)cmd.Parameters["@SchoolMobile"].Value;
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                result = outuniqueid;
            }
            catch (Exception ex)
            {
                SchoolMobile = ex.Message;
                result = "";
            }
            return result;
        }

       
        #endregion

    }
}