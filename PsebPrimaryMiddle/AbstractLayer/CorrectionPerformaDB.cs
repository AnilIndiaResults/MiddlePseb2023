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
    public class CorrectionPerformaDB : ICorrectionPerformaRepository
    {
        


        public DataSet GetCorrPunjabiName(string id)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCorrPunjabiName_Sp";
            cmd.Parameters.AddWithValue("@sid", id);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetStudentRecordsCorrectionData(int type , string schlid,string Corrections)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRecordsCorrectionDataJuniorSP";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@schlid", schlid);
            cmd.Parameters.AddWithValue("@Corrections", Corrections);
            return db.ExecuteDataSet(cmd);
        }       

        public DataSet getCorrrectionField(string std_Class)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "getCorrrectionFieldsp";
            cmd.Parameters.AddWithValue("@std_Class", std_Class);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet GetCorrectionStudentRecordsSearchJunior(string search, int formName, int pageIndex)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCorrectionStudentRecordsSearchJunior";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@formName", formName);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 30);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet InsertSchoolCorrectionAddJunior(RegistrationModels RM)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InsertSchoolCorrectionAddJuniorSP";
            cmd.Parameters.AddWithValue("@UserId", RM.UserId);
            cmd.Parameters.AddWithValue("@UserType", RM.UserType);
            cmd.Parameters.AddWithValue("@Schl", RM.SCHL);
            cmd.Parameters.AddWithValue("@Class", RM.Class);
            cmd.Parameters.AddWithValue("@Std_id", RM.Std_id);
            cmd.Parameters.AddWithValue("@OldValue", RM.CorrectionPerformaModel.oldVal);
            cmd.Parameters.AddWithValue("@NewValue", RM.CorrectionPerformaModel.newVal);
            cmd.Parameters.AddWithValue("@CorrectionType", RM.CorrectionPerformaModel.Correctiontype);
            cmd.Parameters.AddWithValue("@Remark", RM.CorrectionPerformaModel.Remark);
            return db.ExecuteDataSet(cmd);
        }


        public string AiddedCorrectionRecordDelete(string CorrectionId)
        {            
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AiddedCorrectionRecordDelete_Sp";
            cmd.Parameters.AddWithValue("@CorrectionId", CorrectionId);
            return db.ExecuteNonQuery(cmd).ToString();
        }

        public string FinalSubmitCorrectionJunior(RegistrationModels RM)
        {
            string result = "0";
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FinalSubmitCorrectionJuniorSP";
                cmd.Parameters.AddWithValue("@Schl", RM.SCHL);
                cmd.Parameters.AddWithValue("@cr", RM.CorrectionPerformaModel.Correctiontype);
                result = db.ExecuteNonQuery(cmd).ToString();
                return result;
            }
            catch (Exception ex)
            {
                return "0";
            }
        }


        public DataSet GetCorrectiondataFinalPrintList(RegistrationModels RM)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetCorrectiondataFinalPrintListJunior_SP"; // for junior
            cmd.Parameters.AddWithValue("@schl", RM.SCHL);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet SchoolCorrectionPerformaFinalPrintSession(RegistrationModels RM)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SchoolCorrectionPerformaFinalPrintSessionsp";
            cmd.Parameters.AddWithValue("@SCHL", RM.SCHL);
            cmd.Parameters.AddWithValue("@CorrectionLot", RM.CorrectionPerformaModel.CorrectionLot);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet SchoolCorrectionStatus(string schlid, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SchoolCorrectionStatusJuniorSP"; // for junior
            cmd.Parameters.AddWithValue("@schlid", schlid);
            cmd.Parameters.AddWithValue("@search", search);
           return db.ExecuteDataSet(cmd);
        }

        public string CorrLotAcceptReject(string correctionType, string correctionLot, string acceptid, string rejectid, string removeid, string adminid, out string OutStatus)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CorrLotAcceptRejectSP";
            cmd.Parameters.AddWithValue("@correctionType", correctionType);
            cmd.Parameters.AddWithValue("@correctionLot", correctionLot);
            cmd.Parameters.AddWithValue("@acceptid", acceptid);
            cmd.Parameters.AddWithValue("@rejectid", rejectid);
            cmd.Parameters.AddWithValue("@removeid", removeid);
            cmd.Parameters.AddWithValue("@adminid", adminid);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            OutStatus = (string)cmd.Parameters["@OutError"].Value;
            return OutStatus;            
        }


        public string CorrLotRejectRemarksSP(string corid, string remarks, string adminid)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CorrLotRejectRemarksSP";
            cmd.Parameters.AddWithValue("@corid", corid);
            cmd.Parameters.AddWithValue("@remarks", remarks);
            cmd.Parameters.AddWithValue("@adminid", adminid);
            return db.ExecuteNonQuery(cmd).ToString();            
        }


        public DataSet SchoolCorrectionPerformaRoughReport(RegistrationModels RM)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SchoolCorrectionPerformaRoughReportJunior_sp";
            cmd.Parameters.AddWithValue("@SCHL", RM.SCHL);
            cmd.Parameters.AddWithValue("@CType", RM.CorrectionPerformaModel.CorrectionLot);
            return db.ExecuteDataSet(cmd);
        }




        #region Subject


        public string DeleteSubDataByCorrectionId(string CorrectionId, out string OutError)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DeleteSubDataByCorrectionIdSP";
            cmd.Parameters.AddWithValue("@Corid ", CorrectionId);
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);          
            OutError = (string)cmd.Parameters["@OutError"].Value;
            return OutError.ToString();
        }



        public DataSet PendingCorrectionSubjects(string Schl, string cls)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PendingCorrectionSubjectsJunior_SP";
            cmd.Parameters.AddWithValue("@Schl", Schl);
            cmd.Parameters.AddWithValue("@cls", cls);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet SearchStudentGetByData_SubjectCORR(string sid, string frmname)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRecordsByID_SUBJECTCORR";
            cmd.Parameters.AddWithValue("@sid", sid);
            cmd.Parameters.AddWithValue("@formName", frmname);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet ViewAllCorrectionSubjects(string Schl)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ViewAllCorrectionSubjectsJunior_sp";
            cmd.Parameters.AddWithValue("@Schl", Schl);
            return db.ExecuteDataSet(cmd);
        }

        public DataSet SearchCorrectionStudentDetails(string cls, string schl, string sid)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SearchCorrectionStudentDetails_SP";
            cmd.Parameters.AddWithValue("@formName", cls);
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@sid", sid);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet NewCorrectionSubjects(string sid, string frmname, string scode)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "NewCorrectionSubjects_sp";
            cmd.Parameters.AddWithValue("@sid", sid);
            cmd.Parameters.AddWithValue("@formName", frmname);
            cmd.Parameters.AddWithValue("@scode", scode);
            return db.ExecuteDataSet(cmd);
        }


        public DataSet SearchOldStudent_Subject(string sid, string frmname, string scode)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SearchOldStudent_Subject_sp";
            cmd.Parameters.AddWithValue("@sid", sid);
            cmd.Parameters.AddWithValue("@formName", frmname);
            cmd.Parameters.AddWithValue("@scode", scode);
            return db.ExecuteDataSet(cmd);
        }

        public string FinalSubmitSubjectCorrection(RegistrationModels RM, string Class)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "FinalSubmitCorrection_Sp_221218";
            cmd.Parameters.AddWithValue("@Schl", RM.SCHL);
            cmd.Parameters.AddWithValue("@cl", Class);
            cmd.Parameters.AddWithValue("@cr", RM.CorrectionPerformaModel.Correctiontype);
            return db.ExecuteNonQuery(cmd).ToString();
        }


        public string Primary_Subject_Correction(RegistrationModels RM,  string sid, DataTable dtFifthSubject)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Update_Primary_SubjectCorrection_Sp";
            cmd.Parameters.AddWithValue("@Std_id", sid);           
            cmd.Parameters.AddWithValue("@Differently_Abled", RM.DA);

            if (RM.DA.ToString() == "N.A.")
            {
                cmd.Parameters.AddWithValue("@ScribeWriter", "NO");

                if (RM.PreNSQF == "" || RM.PreNSQF == null)
                {
                    if (RM.PreNSQF == "NO")
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", "NO");
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", "NO");
                        cmd.Parameters.AddWithValue("@DP", 0);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 0);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", null);
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", null);
                        cmd.Parameters.AddWithValue("@DP", 0);
                    }

                }
                else //if (RM.PreNSQF != "" || RM.PreNSQF != null)
                {
                    cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                    cmd.Parameters.AddWithValue("@NSQF_SUB", RM.NsqfsubS6);
                    cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", RM.PreNSQF);
                    cmd.Parameters.AddWithValue("@DP", 0);
                }


            }
            else
            {
                cmd.Parameters.AddWithValue("@DP", RM.DP);
                cmd.Parameters.AddWithValue("@NSQF", 0);
                cmd.Parameters.AddWithValue("@ScribeWriter", RM.scribeWriter);
            }

            //----------------------End SubDetails-----------------
            cmd.Parameters.AddWithValue("@tblFifthSubjects", dtFifthSubject);
            // cmd.Parameters.AddWithValue("@MyIP", AbstractLayer.StaticDB.GetFullIPAddress());
            cmd.Parameters.AddWithValue("@MyIP","");
            cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            string OutError = (string)cmd.Parameters["@OutError"].Value;
            string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;            
            return outuniqueid;
         
        }



        public string Middle_Subject_Correction(RegistrationModels RM,string sid, DataTable dtEighthSubject)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Update_Middle_SubjectCorrection_Sp";
            cmd.Parameters.AddWithValue("@Std_id", sid);
            cmd.Parameters.AddWithValue("@Differently_Abled", RM.DA);

            if (RM.DA.ToString() == "N.A.")
            {
                cmd.Parameters.AddWithValue("@ScribeWriter", "NO");

                if (RM.PreNSQF == "" || RM.PreNSQF == null)
                {
                    if (RM.PreNSQF == "NO")
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", "NO");
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", "NO");
                        cmd.Parameters.AddWithValue("@DP", 0);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 0);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", null);
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", null);
                        cmd.Parameters.AddWithValue("@DP", 0);
                    }

                }
                else //if (RM.PreNSQF != "" || RM.PreNSQF != null)
                {
                    cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                    cmd.Parameters.AddWithValue("@NSQF_SUB", RM.NsqfsubS6);
                    cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", RM.PreNSQF);
                    cmd.Parameters.AddWithValue("@DP", 0);
                }


            }
            else
            {
                cmd.Parameters.AddWithValue("@DP", RM.DP);
                cmd.Parameters.AddWithValue("@NSQF", 0);
                cmd.Parameters.AddWithValue("@ScribeWriter", RM.scribeWriter);
            }

            //----------------------End SubDetails-----------------
            cmd.Parameters.AddWithValue("@tblEighthSubjects", dtEighthSubject);
            // cmd.Parameters.AddWithValue("@MyIP", AbstractLayer.StaticDB.GetFullIPAddress());
            cmd.Parameters.AddWithValue("@MyIP", "");
            cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
            ds = db.ExecuteDataSet(cmd);
            string OutError = (string)cmd.Parameters["@OutError"].Value;
            string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;
            return outuniqueid;

        }

        #endregion

        #region

        public DataSet GetCorrectionDataFirm(string search, string CrType, int pageIndex)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCorrectionDataFirm_SP_Cor_Junior";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@CrType", CrType);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", 30);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataSet GetCorrectionDataFirmUpdated(string search, string CrType, int pageIndex)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCorrectionDataFirmUpdated_SP_CorJunior";
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@PageSize", 30);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DataSet CheckFeeAllCorrectionDataByFirmSP(int ActionType, string UserName, string FirmCorrectionLot)
        {
            try
            {

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CheckFeeAllCorrectionDataByFirmSPJunior";
                cmd.Parameters.AddWithValue("@ActionType", ActionType);
                cmd.Parameters.AddWithValue("@FirmName", UserName);
                cmd.Parameters.AddWithValue("@FirmCorrectionLot", FirmCorrectionLot);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
              return  null;
            }
        }


        public DataSet CorrectionDataFirmFinalSubmitSPRN(string userNM, out string FirmCorrectionLot, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CorrectionDataFirmFinalSubmitSPRNJunior";
                cmd.Parameters.AddWithValue("@FirmUser", userNM);
                cmd.Parameters.Add("@FirmCorrectionLot", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                FirmCorrectionLot = (string)cmd.Parameters["@FirmCorrectionLot"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                FirmCorrectionLot = "-1";
                OutError = ex.Message;
                return null;
            }
        }


        public DataSet CorrectionDataFirmFinalSubmitSPRNJuniorMiddleSubjectOnly(string userNM, out string FirmCorrectionLot, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CorrectionDataFirmFinalSubmitSPRNJuniorMiddleSubjectOnly";
                cmd.Parameters.AddWithValue("@FirmUser", userNM);
                cmd.Parameters.Add("@FirmCorrectionLot", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                FirmCorrectionLot = (string)cmd.Parameters["@FirmCorrectionLot"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                FirmCorrectionLot = "-1";
                OutError = ex.Message;
                return null;
            }
        }

        #endregion


        public DataSet AllCancelFirmSchoolCorrection(string FirmUser, string CorType, string search)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllCancelFirmSchoolCorrection_SP";
                cmd.Parameters.AddWithValue("@FirmUser", FirmUser);
                cmd.Parameters.AddWithValue("@CrType", CorType);
                cmd.Parameters.AddWithValue("@search", search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DataSet SetCorrectionDataFirmFeeDetails(AdminModels am, string userNM)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SetCorrectionDataFirmFeeDetails_SP";
                cmd.Parameters.AddWithValue("@FirmUser", userNM);
                cmd.Parameters.AddWithValue("@Correctionlot", am.CorrectionLot);
                cmd.Parameters.AddWithValue("@RecieptNo", am.CorrectionRecieptNo);
                cmd.Parameters.AddWithValue("@RecieptDate", am.CorrectionRecieptDate);
                cmd.Parameters.AddWithValue("@NoCapproved", am.CorrectionNoCapproved);
                cmd.Parameters.AddWithValue("@Amount", am.CorrectionAmount);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataSet CorrectionDataFirmFinalSubmitSPRNByCorrectionLot(string CorrectionLot, string userNM, out string FirmCorrectionLot, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CorrectionDataFirmFinalSubmitSPRNByCorrectionLotJunior";
                cmd.Parameters.AddWithValue("@CorrectionLot", CorrectionLot);
                cmd.Parameters.AddWithValue("@FirmUser", userNM);
                cmd.Parameters.Add("@FirmCorrectionLot", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                FirmCorrectionLot = (string)cmd.Parameters["@FirmCorrectionLot"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return ds;
            }
            catch (Exception ex)
            {
                FirmCorrectionLot = "-1";
                OutError = ex.Message;
                return null;
            }
        }

    }
}