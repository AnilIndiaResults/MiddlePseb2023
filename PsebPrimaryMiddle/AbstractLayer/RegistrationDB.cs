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
using System.Web.Services;
using System.Runtime.InteropServices;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace PsebJunior.AbstractLayer
{
    public class RegistrationDB
    {
        private string CommonCon = "myDBConnection";
        public RegistrationDB()
        {
            if (HttpContext.Current.Session["Session"] == null)
            {
                CommonCon = "myDBConnection";
            }
            else
            {
                CommonCon = "myDBConnection";
            }

        }

        #region common  data

        public static DataSet CheckRegularRegno(string search)
        {

            Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckRegularRegNoSP"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet RegDataViewForAllClassSP(int type, string sid, string frmname, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "RegDataViewForAllClassPrimaryMiddleSP"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@sid", sid);
            cmd.Parameters.AddWithValue("@formName", frmname);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }


        public static RegistrationModels RegDataModalForAllClassSP(int type, string sid, string frmname, string search, out DataSet outDS)
        {
            RegistrationModels rm = new RegistrationModels();
            DataSet ds = new DataSet();
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RegDataViewForAllClassPrimaryMiddleSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@StdId", sid);
                cmd.Parameters.AddWithValue("@formName", frmname);
                cmd.Parameters.AddWithValue("@search", search);
                ds = db.ExecuteDataSet(cmd);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    rm.Std_id = Convert.ToInt32(ds.Tables[0].Rows[0]["Std_id"].ToString());
                    rm.form_Name = ds.Tables[0].Rows[0]["form_Name"].ToString();
                    rm.MotherTongue = ds.Tables[0].Rows[0]["MotherTongue"].ToString();
                    rm.IsHardCopyCertificate = ds.Tables[0].Rows[0]["IsHardCopyCertificate"].ToString();

                    rm.IsSmartPhone = ds.Tables[0].Rows[0]["IsSmartPhone"].ToString();
                    rm.EmpUserId = ds.Tables[0].Rows[0]["EmpUserId"].ToString();
                    rm.EligibilityCriteria = ds.Tables[0].Rows[0]["EligibilityCriteria"].ToString();

                    //

                    rm.ProofCertificate = ds.Tables[0].Rows[0]["ProofCertificate"].ToString() == "" ? "" : ConfigurationManager.AppSettings["AWSURL"] + ds.Tables[0].Rows[0]["ProofCertificate"].ToString();
                    rm.ProofNRICandidates = ds.Tables[0].Rows[0]["ProofNRICandidates"].ToString() == "" ? "" : ConfigurationManager.AppSettings["AWSURL"] + ds.Tables[0].Rows[0]["ProofNRICandidates"].ToString();

                    rm.IsNRICandidate = ds.Tables[0].Rows[0]["IsNRICandidate"] == null ? false : ds.Tables[0].Rows[0]["IsNRICandidate"].ToString() == "0" ? false : ds.Tables[0].Rows[0]["IsNRICandidate"].ToString().ToLower() == "false" ? false : true;
                    rm.Class = ds.Tables[0].Rows[0]["Class"].ToString();
                    rm.Category = ds.Tables[0].Rows[0]["Category"].ToString();
                    //  rm.Registration_num = ds.Tables[0].Rows[0]["RegNo"].ToString();

                    int IsPSEBRegNum = Convert.ToInt32(ds.Tables[0].Rows[0]["IsPSEBRegNum"].ToString() == "" ? "0" : ds.Tables[0].Rows[0]["IsPSEBRegNum"].ToString());


                    if (!string.IsNullOrEmpty(ds.Tables[0].Rows[0]["RegNo"].ToString()))
                    {
                        rm.IsPSEBRegNum = true;
                        rm.Registration_num = ds.Tables[0].Rows[0]["RegNo"].ToString();
                    }
                    else
                    {
                        rm.IsPSEBRegNum = false;
                        rm.Registration_num = ds.Tables[0].Rows[0]["RegNo"].ToString();
                    }

                    rm.SCHL = ds.Tables[0].Rows[0]["schl"].ToString();
                    rm.Candi_Name = ds.Tables[0].Rows[0]["Name"].ToString();
                    rm.Candi_Name_P = ds.Tables[0].Rows[0]["PName"].ToString();
                    rm.Father_Name = ds.Tables[0].Rows[0]["FName"].ToString();
                    rm.Father_Name_P = ds.Tables[0].Rows[0]["PFName"].ToString();
                    rm.Mother_Name = ds.Tables[0].Rows[0]["MName"].ToString();
                    rm.Mother_Name_P = ds.Tables[0].Rows[0]["PMName"].ToString();
                    rm.Mobile = ds.Tables[0].Rows[0]["Mobile"].ToString();
                    rm.CandiMedium = ds.Tables[0].Rows[0]["CandStudyMedium"].ToString();

                    rm.Caste = ds.Tables[0].Rows[0]["Caste"].ToString();
                    rm.Gender = ds.Tables[0].Rows[0]["Gender"].ToString();
                    rm.Religion = ds.Tables[0].Rows[0]["Religion"].ToString();
                    rm.DOB = ds.Tables[0].Rows[0]["DOB"].ToString();
                    rm.Belongs_BPL = ds.Tables[0].Rows[0]["BPL"].ToString();
                    rm.DP = rm.DA = ds.Tables[0].Rows[0]["DisabilityPercent"].ToString();
                    rm.Differently_Abled = rm.DA = ds.Tables[0].Rows[0]["phy_chal"].ToString();
                    //
                    rm.scribeWriter = ds.Tables[0].Rows[0]["writer"].ToString();
                    rm.Board = ds.Tables[0].Rows[0]["Board"].ToString();
                    rm.Other_Board = ds.Tables[0].Rows[0]["Other_Board"].ToString();
                    rm.Board_Roll_Num = ds.Tables[0].Rows[0]["BRoll"].ToString();
                    rm.Prev_School_Name = ds.Tables[0].Rows[0]["PrvSchlnm"].ToString();
                    rm.Month = ds.Tables[0].Rows[0]["Month"].ToString();
                    rm.Year = ds.Tables[0].Rows[0]["Year"].ToString();
                    //
                    // rm.AWRegisterNo = ds.Tables[0].Rows[0]["AWRegNo"].ToString();
                    rm.Admission_Num = ds.Tables[0].Rows[0]["Admno"].ToString();
                    rm.Admission_Date = ds.Tables[0].Rows[0]["AdmDate"].ToString();
                    //rm.Class_Roll_Num_Section = ds.Tables[0].Rows[0]["clsRoll"].ToString();
                    //rm.Section = Convert.ToChar(ds.Tables[0].Rows[0]["clsSec"].ToString());
                    //
                    rm.E_punjab_Std_id = ds.Tables[0].Rows[0]["Epbid"].ToString();
                    rm.AadharEnroll = ds.Tables[0].Rows[0]["AadharEnroll"].ToString();
                    rm.Aadhar_num = ds.Tables[0].Rows[0]["Aadhar"].ToString();                        //
                                                                                                      //
                    rm.Address = ds.Tables[0].Rows[0]["Address"].ToString();
                    rm.LandMark = ds.Tables[0].Rows[0]["LandMark"].ToString();
                    rm.Block = ds.Tables[0].Rows[0]["Block"].ToString();
                    rm.PinCode = ds.Tables[0].Rows[0]["PinCode"].ToString();
                    //
                    rm.DIST = Convert.ToString(ds.Tables[0].Rows[0]["District"].ToString());
                    rm.Tehsil = Convert.ToInt32(ds.Tables[0].Rows[0]["Tehsil"].ToString());
                    rm.DISTNM = ds.Tables[0].Rows[0]["DISTNM"].ToString();
                    rm.MYTehsil = ds.Tables[0].Rows[0]["TehsilNM"].ToString();
                }


                outDS = ds;
                return rm;

            }
            catch (Exception ex)
            {
                outDS = null;
                return rm = null;
            }

        }





        public static DataSet GetSubjectDetailsByClsandSubCode(int cls, string SUB)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSubjectDetailsByClsandSubCodeSP";
            cmd.Parameters.AddWithValue("@cls", cls);
            cmd.Parameters.AddWithValue("@SUB", SUB);
            return db.ExecuteDataSet(cmd);
        }

        public int CheckSchoolClassStatusBySchool(int cls, string schl)
        {
            int status = 0;
            DataSet result = DBClass.schooltypes(schl);
            if (result == null)
            {
                status = -1;
            }
            else if (result.Tables[1].Rows.Count > 0)
            {
                if (cls == 8)
                { status = result.Tables[1].Rows[0]["Eighth"].ToString() == "1" ? 1 : 0; }

                else if (cls == 5) { status = result.Tables[1].Rows[0]["Fifth"].ToString() == "1" ? 1 : 0; }

            }
            return status;
        }


        public static DataSet GetStudentRecordsSearchNew(string form_name, string schl, int SelValueSch, string SearchString)
        {
            try
            {


                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetStudentRecordsSearch_New";

                if (!string.IsNullOrEmpty(form_name))
                {
                    cmd.Parameters.AddWithValue("@form_name", form_name);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@form_name", DBNull.Value);
                }
                if (!string.IsNullOrEmpty(schl))
                {
                    cmd.Parameters.AddWithValue("@schl", schl);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@schl", DBNull.Value);
                }

                if (SelValueSch == 1)
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        cmd.Parameters.AddWithValue("@Std_id", SearchString);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Std_id", DBNull.Value);
                    }
                }
                else if (SelValueSch == 2)
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        cmd.Parameters.AddWithValue("@Candi_Name", SearchString);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Candi_Name", DBNull.Value);
                    }
                }
                else if (SelValueSch == 3)
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        cmd.Parameters.AddWithValue("@Father_Name", SearchString);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Father_Name", DBNull.Value);
                    }
                }
                else if (SelValueSch == 4)
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        cmd.Parameters.AddWithValue("@Mother_Name", SearchString);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Mother_Name", DBNull.Value);
                    }
                }
                else if (SelValueSch == 5)
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        cmd.Parameters.AddWithValue("@DOB", SearchString);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@DOB", DBNull.Value);
                    }
                }
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<RegistrationSearchModel> GetStudentRecordsSearchPM(string form_name, string schl, out DataSet dsOut)
        {
            List<RegistrationSearchModel> registrationSearchModels = new List<RegistrationSearchModel>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRecordsSearchPM";
            cmd.Parameters.AddWithValue("@form_name", form_name);
            cmd.Parameters.AddWithValue("@schl", schl);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {
                var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new RegistrationSearchModel
                {
                    Std_id = dataRow.Field<int>("Std_id"),
                    form_Name = dataRow.Field<string>("form_Name"),
                    schl = dataRow.Field<string>("schl"),
                    Admission_Date = dataRow.Field<string>("Admission_Date"),
                    Category = dataRow.Field<string>("Category"),
                    Candi_Name = dataRow.Field<string>("Candi_Name"),
                    Father_Name = dataRow.Field<string>("Father_Name"),
                    Mother_Name = dataRow.Field<string>("Mother_Name"),
                    DOB = dataRow.Field<string>("DOB"),
                    LOT = dataRow.Field<int>("LOT"),
                    aadhar_num = dataRow.Field<string>("aadhar_num"),
                    CreatedDate = dataRow.Field<DateTime?>("CreatedDate"),
                    UPDT = dataRow.Field<DateTime?>("UPDT"),
                    SubjectList = dataRow.Field<string>("SubjectList"),
                    //
                    REGNO = dataRow.Field<string>("Registration_num"),
                    ProofCertificate = dataRow.Field<string>("ProofCertificate"),
                    ProofNRICandidates = dataRow.Field<string>("ProofNRICandidates"),
                    StudentUniqueId = dataRow.Field<string>("StudentUniqueId"),
                    schlDist = dataRow.Field<string>("SCHLDIST"),

                }).ToList();

                registrationSearchModels = eList.ToList();
            }
            dsOut = ds;
            return registrationSearchModels;

        }


        public static Task<List<RegistrationAllStudentAdminModel>> GetAllFinalSubmittedStudentBySchlPM(string form_name, string schl, out DataSet dsOut)
        {
            List<RegistrationAllStudentAdminModel> registrationAllStudentAdminModels = new List<RegistrationAllStudentAdminModel>();
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllFinalSubmittedStudentBySchlPM";
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@form_name", form_name);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {

                var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new RegistrationAllStudentAdminModel
                {
                    Std_id = dataRow.Field<int>("Std_id"),
                    form_Name = dataRow.Field<string>("form_Name"),
                    schl = dataRow.Field<string>("schl"),
                    Admission_Date = dataRow.Field<string>("Admission_Date"),
                    Candi_Name = dataRow.Field<string>("Candi_Name"),
                    Father_Name = dataRow.Field<string>("Father_Name"),
                    Mother_Name = dataRow.Field<string>("Mother_Name"),
                    DOB = dataRow.Field<string>("DOB"),
                    LOT = dataRow.Field<int>("LOT"),
                    aadhar_num = dataRow.Field<string>("aadhar_num"),
                    CreatedDate = dataRow.Field<DateTime?>("CreatedDate"),
                    UPDT = dataRow.Field<DateTime?>("UPDT"),

                    RegNo = dataRow.Field<string>("RegNo"),
                    AadharNo = dataRow.Field<string>("AadharNo"),
                    Fee = dataRow.Field<int?>("Fee"),
                    Remarks = dataRow.Field<string>("Remarks"),
                    IsCancel = dataRow.Field<bool?>("IsCancel"),
                    CancelDT = dataRow.Field<DateTime?>("CancelDT"),

                }).ToList();

                registrationAllStudentAdminModels = eList.ToList();
            }
            Thread.Sleep(2000);
            dsOut = (DataSet)ds;
            return Task.FromResult(registrationAllStudentAdminModels);

        }


        //public static DataSet GetStudentRecordsSearchPM(string form_name, string schl)
        //{
        //    Database db = DatabaseFactory.CreateDatabase();
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.CommandText = "GetStudentRecordsSearchPM";
        //    cmd.Parameters.AddWithValue("@form_name", form_name);
        //    cmd.Parameters.AddWithValue("@schl", schl);
        //    return db.ExecuteDataSet(cmd);
        //}

        public static DataSet GetStudentRecordsSearch(string search, int pageIndex)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRecordsSearch";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 30);
            return db.ExecuteDataSet(cmd);
        }


        public static DataSet SearchStudentGetByData(string sid, string frmname)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRecordsByID";
            cmd.Parameters.AddWithValue("@sid", sid);
            cmd.Parameters.AddWithValue("@formName", frmname);
            return db.ExecuteDataSet(cmd);
        }


        public static DataSet UpdaadharEnrollmentNo(string std_id, string aadhar_num, string SCHL, string Caste, string gender, string BPL, string Rel, string Epunid)
        {

            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdateaadharNum_SpPM";
            cmd.Parameters.AddWithValue("@std_id", std_id);
            cmd.Parameters.AddWithValue("@aadhar_num", aadhar_num);
            cmd.Parameters.AddWithValue("@SCHL", SCHL);


            string ipaddress = "";
            try
            {
                ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
            }
            catch (Exception)
            {
                ipaddress = "";
            }
            cmd.Parameters.AddWithValue("@MyIP", ipaddress);
            return db.ExecuteDataSet(cmd);
        }


        public static DataSet SelectlastEntryCandidate(string formName, string schl)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLastEntryStudentRecords";
            cmd.Parameters.AddWithValue("@formName", formName);
            cmd.Parameters.AddWithValue("@schl", schl);
            return db.ExecuteDataSet(cmd);
        }





        #region Delete_FormDataSP

        public static int DeleteFromData(string stdid, out string OutError)
        {
            int result;
            try
            {
                Int64 newstdid = Convert.ToInt64(stdid);
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Delete_FormDataSPPM";
                cmd.Parameters.AddWithValue("@Std_id", newstdid);

                string ipaddress = "";
                try
                {
                    ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
                }
                catch (Exception)
                {
                    ipaddress = "";
                }
                cmd.Parameters.AddWithValue("@MyIP", ipaddress);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
            }
            catch (Exception ex)
            {
                OutError = "";
                result = -2;
            }
            return result;
            //ds = db.ExecuteDataSet(cmd);
            //return ds.Tables[0];
        }

        public static string Updated_Pic_Data(string Myresult, string PhotoSignName, string Type)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Update_Uploaded_Photo_Sign";
                cmd.Parameters.AddWithValue("@StudentUniqueID", Myresult);
                cmd.Parameters.AddWithValue("@PhotoSignName", PhotoSignName);
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@IsNew", "");
                result = db.ExecuteNonQuery(cmd).ToString();
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        #endregion Delete_FormDataSP

        #endregion


        public static List<SelectListItem> GetImportSessionLast2()
        {
            List<SelectListItem> itemSession = new List<SelectListItem>();
            itemSession.Add(new SelectListItem { Text = "2023", Value = "2023" });
            itemSession.Add(new SelectListItem { Text = "2022", Value = "2022" });
            itemSession.Add(new SelectListItem { Text = "2021", Value = "2021" });
            //itemSession.Add(new SelectListItem { Text = "2020", Value = "2020" });
            return itemSession;
        }

        #region  5th Class Insert & Update Data of F1-F2


        public static string Ins_F_Form_Data(RegistrationModels RM, FormCollection frm, string FormType, string session, string schl, DataTable dtFifthSubject)
        {
            string result = "";

            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "insert_F1_F2_Subject_Forms_Sp";
                cmd.Parameters.AddWithValue("@MotherTongue", RM.MotherTongue);
                //cmd.Parameters.AddWithValue("@IsHardCopyCertificate", RM.IsHardCopyCertificate);
                cmd.Parameters.AddWithValue("@IsHardCopyCertificate", "YES");
                cmd.Parameters.AddWithValue("@EligibilityCriteria", RM.EligibilityCriteria);
                cmd.Parameters.AddWithValue("@IsNRICandidate", RM.IsNRICandidate);
                cmd.Parameters.AddWithValue("@form_Name", FormType);
                cmd.Parameters.AddWithValue("@Category", RM.Category);
                cmd.Parameters.AddWithValue("@Board", RM.Board);
                cmd.Parameters.AddWithValue("@Other_Board", RM.Other_Board);
                cmd.Parameters.AddWithValue("@Board_Roll_Num", RM.Board_Roll_Num);
                cmd.Parameters.AddWithValue("@Prev_School_Name", RM.Prev_School_Name);
                cmd.Parameters.AddWithValue("@Group_Name", RM.MyGroup);
                if (RM.IsPSEBRegNum.ToString() == "True" && !string.IsNullOrEmpty(RM.Registration_num))
                {
                    cmd.Parameters.AddWithValue("@Registration_num", RM.Registration_num);
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Registration_num", "");
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 0);
                }

                cmd.Parameters.AddWithValue("@Month", RM.Month);
                cmd.Parameters.AddWithValue("@Year", RM.Year);
                cmd.Parameters.AddWithValue("@Admission_Date", RM.Admission_Date);
                cmd.Parameters.AddWithValue("@Class_Roll_Num_Section", RM.Class_Roll_Num_Section);

                cmd.Parameters.AddWithValue("@Candi_Name", RM.Candi_Name);
                cmd.Parameters.AddWithValue("@Candi_Name_P", RM.Candi_Name_P);
                cmd.Parameters.AddWithValue("@Father_Name", RM.Father_Name);
                cmd.Parameters.AddWithValue("@Father_Name_P", RM.Father_Name_P);
                cmd.Parameters.AddWithValue("@Mother_Name", RM.Mother_Name);
                cmd.Parameters.AddWithValue("@Mother_Name_P", RM.Mother_Name_P);
                cmd.Parameters.AddWithValue("@Caste", RM.Caste);
                cmd.Parameters.AddWithValue("@Gender", RM.Gender);
                cmd.Parameters.AddWithValue("@Differently_Abled", frm["DA"].ToString());
                cmd.Parameters.AddWithValue("@Religion", RM.Religion);
                cmd.Parameters.AddWithValue("@DOB", RM.DOB);
                cmd.Parameters.AddWithValue("@Belongs_BPL", RM.Belongs_BPL);
                cmd.Parameters.AddWithValue("@Mobile", RM.Mobile);
                cmd.Parameters.AddWithValue("@Aadhar_num", RM.Aadhar_num);
                cmd.Parameters.AddWithValue("@E_punjab_Std_id", RM.E_punjab_Std_id);
                cmd.Parameters.AddWithValue("@Address", RM.Address);
                cmd.Parameters.AddWithValue("@LandMark", RM.LandMark);
                cmd.Parameters.AddWithValue("@Block", RM.Block);
                cmd.Parameters.AddWithValue("@Tehsil", RM.Tehsil);
                cmd.Parameters.AddWithValue("@District", RM.DIST);
                cmd.Parameters.AddWithValue("@PinCode", RM.PinCode);

                if (RM.Provisional.ToString() == "True")
                {

                    cmd.Parameters.AddWithValue("@Provisional", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Provisional", 0);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", RM.AWRegisterNo);
                    cmd.Parameters.AddWithValue("@Admission_Num", RM.Admission_Num);

                }

                if (RM.IsPrevSchoolSelf.ToString() == "True")
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 0);
                }

                if (RM.file != null)
                {
                    cmd.Parameters.AddWithValue("@std_Photo", RM.file.FileName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Photo", null);
                }
                if (RM.std_Sign != null)
                {
                    cmd.Parameters.AddWithValue("@std_Sign", RM.std_Sign.FileName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Sign", null);
                }



                if (frm["DA"].ToString() == "N.A.")
                {
                    cmd.Parameters.AddWithValue("@ScribeWriter", "NO");

                    if (RM.PreNSQF == "" || RM.PreNSQF == null)
                    {
                        if (RM.PreNSQF == "NO")
                        {
                            cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                            cmd.Parameters.AddWithValue("@NSQF_SUB", "NO");
                            cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", "NO");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@NSQF_flag", 0);
                            cmd.Parameters.AddWithValue("@NSQF_SUB", null);
                            cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", null);
                        }

                    }
                    else if (RM.PreNSQF != "" || RM.PreNSQF != null)
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", RM.NsqfsubS6);
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", RM.PreNSQF);

                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@NSQF", 0);
                    cmd.Parameters.AddWithValue("@ScribeWriter", RM.scribeWriter);
                }
                //----------------------End SubDetails-----------------
                if (frm["DA"].ToString() == "N.A.")
                {
                    cmd.Parameters.AddWithValue("@DP", 0);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@DP", RM.DP);
                }
                cmd.Parameters.AddWithValue("@tblFifthSubjects", dtFifthSubject);
                cmd.Parameters.AddWithValue("@Section", RM.Section);
                cmd.Parameters.AddWithValue("@SCHL", schl);
                cmd.Parameters.AddWithValue("@SESSION", session);
                cmd.Parameters.AddWithValue("@CandStudyMedium", RM.CandiMedium);

                string ipaddress = "";
                try
                {
                    ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
                }
                catch (Exception)
                {
                    ipaddress = "";
                }
                cmd.Parameters.AddWithValue("@MyIP", ipaddress);
                cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;
                return outuniqueid;

            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        public static string Update_F_Data(RegistrationModels RM, FormCollection frm, string FormType, string sid, string FilePhoto, string sign, DataTable dtFifthSubject)
        {
            string result = "";

            try
            {
                long stdid = long.Parse(sid);
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Update_F1_F2_SubjectsForms_Sp";

                cmd.Parameters.AddWithValue("@MotherTongue", RM.MotherTongue);
                cmd.Parameters.AddWithValue("@IsHardCopyCertificate", "YES");
                cmd.Parameters.AddWithValue("@EligibilityCriteria", RM.EligibilityCriteria);
                cmd.Parameters.AddWithValue("@IsNRICandidate", RM.IsNRICandidate);
                cmd.Parameters.AddWithValue("@Std_id", stdid);
                cmd.Parameters.AddWithValue("@form_Name", FormType);
                cmd.Parameters.AddWithValue("@Category", RM.Category);
                cmd.Parameters.AddWithValue("@Board", RM.Board);
                cmd.Parameters.AddWithValue("@Other_Board", RM.Other_Board);
                cmd.Parameters.AddWithValue("@Board_Roll_Num", RM.Board_Roll_Num);
                cmd.Parameters.AddWithValue("@Prev_School_Name", RM.Prev_School_Name);

                cmd.Parameters.AddWithValue("@Month", RM.Month);
                cmd.Parameters.AddWithValue("@Year", RM.Year);
                cmd.Parameters.AddWithValue("@Admission_Date", RM.Admission_Date);
                cmd.Parameters.AddWithValue("@Class_Roll_Num_Section", RM.Class_Roll_Num_Section);
                cmd.Parameters.AddWithValue("@Candi_Name", RM.Candi_Name);
                cmd.Parameters.AddWithValue("@Candi_Name_P", RM.Candi_Name_P);
                cmd.Parameters.AddWithValue("@Father_Name", RM.Father_Name);
                cmd.Parameters.AddWithValue("@Father_Name_P", RM.Father_Name_P);
                cmd.Parameters.AddWithValue("@Mother_Name", RM.Mother_Name);
                cmd.Parameters.AddWithValue("@Mother_Name_P", RM.Mother_Name_P);
                cmd.Parameters.AddWithValue("@Caste", RM.Caste);
                cmd.Parameters.AddWithValue("@Gender", RM.Gender);
                cmd.Parameters.AddWithValue("@Differently_Abled", RM.DA);
                cmd.Parameters.AddWithValue("@Religion", RM.Religion);
                cmd.Parameters.AddWithValue("@DOB", RM.DOB);
                cmd.Parameters.AddWithValue("@Belongs_BPL", RM.Belongs_BPL);
                cmd.Parameters.AddWithValue("@Mobile", RM.Mobile);
                //cmd.Parameters.AddWithValue("@Father_MobNo", RM.Father_MobNo);
                //cmd.Parameters.AddWithValue("@Father_Occup", RM.Father_Occup);
                //cmd.Parameters.AddWithValue("@Mother_MobNo", RM.Mother_MobNo);
                //cmd.Parameters.AddWithValue("@Mother_Occup", RM.Mother_Occup);
                cmd.Parameters.AddWithValue("@Aadhar_num", RM.Aadhar_num);
                cmd.Parameters.AddWithValue("@Group_Name", RM.MyGroup);
                cmd.Parameters.AddWithValue("@E_punjab_Std_id", RM.E_punjab_Std_id);
                cmd.Parameters.AddWithValue("@Address", RM.Address);
                cmd.Parameters.AddWithValue("@LandMark", RM.LandMark);
                cmd.Parameters.AddWithValue("@Block", RM.Block);

                cmd.Parameters.AddWithValue("@District", RM.DIST);
                cmd.Parameters.AddWithValue("@Tehsil", RM.Tehsil);
                cmd.Parameters.AddWithValue("@PinCode", RM.PinCode);

                if (RM.file == null)
                {
                    cmd.Parameters.AddWithValue("@std_Photo", FilePhoto);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Photo", RM.file.FileName);
                }
                if (RM.std_Sign == null)
                {
                    cmd.Parameters.AddWithValue("@std_Sign", sign);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Sign", RM.std_Sign.FileName);
                }

                //----
                if (RM.IsPrevSchoolSelf.ToString() == "True")
                {
                    //cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", RM.IsPrevSchoolSelf);
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 0);
                }


                if (RM.IsPSEBRegNum.ToString() == "True" && !string.IsNullOrEmpty(RM.Registration_num))
                {
                    cmd.Parameters.AddWithValue("@Registration_num", RM.Registration_num);
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Registration_num", "");
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 0);
                }

                if (RM.Provisional.ToString() == "True")
                {

                    cmd.Parameters.AddWithValue("@Provisional", 1);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", null);
                    cmd.Parameters.AddWithValue("@Admission_Num", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Provisional", 0);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", RM.AWRegisterNo);
                    cmd.Parameters.AddWithValue("@Admission_Num", RM.Admission_Num);

                }

                if (frm["DA"].ToString() == "N.A.")
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
                cmd.Parameters.AddWithValue("@Section", RM.Section);
                cmd.Parameters.AddWithValue("@CandStudyMedium", RM.CandiMedium);

                string ipaddress = "";
                try
                {
                    ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
                }
                catch (Exception)
                {
                    ipaddress = "";
                }
                cmd.Parameters.AddWithValue("@MyIP", ipaddress);
                cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;

                return outuniqueid;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        #endregion


        #region A1-A2 - 8th Class


        public static string Ins_A_Form_Data(RegistrationModels RM, FormCollection frm, string FormType, string session, string schl, DataTable dtEighthSubject)
        {
            string result = "";

            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "insert_A1_A2_Subject_Forms_Sp";
                cmd.Parameters.AddWithValue("@MotherTongue", RM.MotherTongue);
                cmd.Parameters.AddWithValue("@IsHardCopyCertificate", "YES");
                cmd.Parameters.AddWithValue("@EligibilityCriteria", RM.EligibilityCriteria);
                cmd.Parameters.AddWithValue("@IsNRICandidate", RM.IsNRICandidate);
                cmd.Parameters.AddWithValue("@form_Name", FormType);
                cmd.Parameters.AddWithValue("@Category", RM.Category);
                cmd.Parameters.AddWithValue("@Board", RM.Board);
                cmd.Parameters.AddWithValue("@Other_Board", RM.Other_Board);
                cmd.Parameters.AddWithValue("@Board_Roll_Num", RM.Board_Roll_Num);
                cmd.Parameters.AddWithValue("@Prev_School_Name", RM.Prev_School_Name);
                cmd.Parameters.AddWithValue("@Group_Name", RM.MyGroup);
                if (RM.IsPSEBRegNum.ToString() == "True" && !string.IsNullOrEmpty(RM.Registration_num))
                {
                    cmd.Parameters.AddWithValue("@Registration_num", RM.Registration_num);
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Registration_num", "");
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 0);
                }

                cmd.Parameters.AddWithValue("@Month", RM.Month);
                cmd.Parameters.AddWithValue("@Year", RM.Year);
                cmd.Parameters.AddWithValue("@Admission_Date", RM.Admission_Date);
                cmd.Parameters.AddWithValue("@Class_Roll_Num_Section", RM.Class_Roll_Num_Section);

                cmd.Parameters.AddWithValue("@Candi_Name", RM.Candi_Name);
                cmd.Parameters.AddWithValue("@Candi_Name_P", RM.Candi_Name_P);
                cmd.Parameters.AddWithValue("@Father_Name", RM.Father_Name);
                cmd.Parameters.AddWithValue("@Father_Name_P", RM.Father_Name_P);
                cmd.Parameters.AddWithValue("@Mother_Name", RM.Mother_Name);
                cmd.Parameters.AddWithValue("@Mother_Name_P", RM.Mother_Name_P);
                cmd.Parameters.AddWithValue("@Caste", RM.Caste);
                cmd.Parameters.AddWithValue("@Gender", RM.Gender);
                cmd.Parameters.AddWithValue("@Differently_Abled", frm["DA"].ToString());
                cmd.Parameters.AddWithValue("@Religion", RM.Religion);
                cmd.Parameters.AddWithValue("@DOB", RM.DOB);
                cmd.Parameters.AddWithValue("@Belongs_BPL", RM.Belongs_BPL);
                cmd.Parameters.AddWithValue("@Mobile", RM.Mobile);
                cmd.Parameters.AddWithValue("@Aadhar_num", RM.Aadhar_num);
                cmd.Parameters.AddWithValue("@E_punjab_Std_id", RM.E_punjab_Std_id);
                cmd.Parameters.AddWithValue("@Address", RM.Address);
                cmd.Parameters.AddWithValue("@LandMark", RM.LandMark);
                cmd.Parameters.AddWithValue("@Block", RM.Block);
                cmd.Parameters.AddWithValue("@Tehsil", RM.Tehsil);
                cmd.Parameters.AddWithValue("@District", RM.DIST);
                cmd.Parameters.AddWithValue("@PinCode", RM.PinCode);

                if (RM.Provisional.ToString() == "True")
                {

                    cmd.Parameters.AddWithValue("@Provisional", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Provisional", 0);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", RM.AWRegisterNo);
                    cmd.Parameters.AddWithValue("@Admission_Num", RM.Admission_Num);

                }

                if (RM.IsPrevSchoolSelf.ToString() == "True")
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 0);
                }

                if (RM.file != null)
                {
                    cmd.Parameters.AddWithValue("@std_Photo", RM.file.FileName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Photo", null);
                }
                if (RM.std_Sign != null)
                {
                    cmd.Parameters.AddWithValue("@std_Sign", RM.std_Sign.FileName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Sign", null);
                }



                if (frm["DA"].ToString() == "N.A.")
                {
                    cmd.Parameters.AddWithValue("@ScribeWriter", "NO");

                    if (RM.PreNSQF == "" || RM.PreNSQF == null)
                    {
                        if (RM.PreNSQF == "NO")
                        {
                            cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                            cmd.Parameters.AddWithValue("@NSQF_SUB", "NO");
                            cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", "NO");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@NSQF_flag", 0);
                            cmd.Parameters.AddWithValue("@NSQF_SUB", null);
                            cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", null);
                        }

                    }
                    else if (RM.PreNSQF != "" || RM.PreNSQF != null)
                    {
                        cmd.Parameters.AddWithValue("@NSQF_flag", 1);
                        cmd.Parameters.AddWithValue("@NSQF_SUB", RM.NsqfsubS6);
                        cmd.Parameters.AddWithValue("@PRE_NSQF_SUB", RM.PreNSQF);

                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@NSQF", 0);
                    cmd.Parameters.AddWithValue("@ScribeWriter", RM.scribeWriter);
                }
                //----------------------End SubDetails-----------------
                if (frm["DA"].ToString() == "N.A.")
                {
                    cmd.Parameters.AddWithValue("@DP", 0);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@DP", RM.DP);
                }
                cmd.Parameters.AddWithValue("@tblEighthSubjects", dtEighthSubject);
                cmd.Parameters.AddWithValue("@Section", RM.Section);
                cmd.Parameters.AddWithValue("@SCHL", schl);
                cmd.Parameters.AddWithValue("@SESSION", session);
                cmd.Parameters.AddWithValue("@CandStudyMedium", RM.CandiMedium);

                string ipaddress = "";
                try
                {
                    ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
                }
                catch (Exception)
                {
                    ipaddress = "";
                }
                cmd.Parameters.AddWithValue("@MyIP", ipaddress);
                cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;
                return outuniqueid;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }



        public static string Update_A_Data(RegistrationModels RM, FormCollection frm, string FormType, string sid, string FilePhoto, string sign, DataTable dtEighthSubject)
        {
            string result = "";

            try
            {
                long stdid = long.Parse(sid);
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Update_A1_A2_SubjectsForms_Sp";
                cmd.Parameters.AddWithValue("@MotherTongue", RM.MotherTongue);
                cmd.Parameters.AddWithValue("@IsHardCopyCertificate", "YES");
                cmd.Parameters.AddWithValue("@EligibilityCriteria", RM.EligibilityCriteria);
                cmd.Parameters.AddWithValue("@IsNRICandidate", RM.IsNRICandidate);
                cmd.Parameters.AddWithValue("@Std_id", stdid);
                cmd.Parameters.AddWithValue("@form_Name", FormType);
                cmd.Parameters.AddWithValue("@Category", RM.Category);
                cmd.Parameters.AddWithValue("@Board", RM.Board);
                cmd.Parameters.AddWithValue("@Other_Board", RM.Other_Board);
                cmd.Parameters.AddWithValue("@Board_Roll_Num", RM.Board_Roll_Num);
                cmd.Parameters.AddWithValue("@Prev_School_Name", RM.Prev_School_Name);
                cmd.Parameters.AddWithValue("@Month", RM.Month);
                cmd.Parameters.AddWithValue("@Year", RM.Year);
                cmd.Parameters.AddWithValue("@Admission_Date", RM.Admission_Date);
                cmd.Parameters.AddWithValue("@Class_Roll_Num_Section", RM.Class_Roll_Num_Section);
                cmd.Parameters.AddWithValue("@Candi_Name", RM.Candi_Name);
                cmd.Parameters.AddWithValue("@Candi_Name_P", RM.Candi_Name_P);
                cmd.Parameters.AddWithValue("@Father_Name", RM.Father_Name);
                cmd.Parameters.AddWithValue("@Father_Name_P", RM.Father_Name_P);
                cmd.Parameters.AddWithValue("@Mother_Name", RM.Mother_Name);
                cmd.Parameters.AddWithValue("@Mother_Name_P", RM.Mother_Name_P);
                cmd.Parameters.AddWithValue("@Caste", RM.Caste);
                cmd.Parameters.AddWithValue("@Gender", RM.Gender);
                cmd.Parameters.AddWithValue("@Differently_Abled", RM.DA);
                cmd.Parameters.AddWithValue("@Religion", RM.Religion);
                cmd.Parameters.AddWithValue("@DOB", RM.DOB);
                cmd.Parameters.AddWithValue("@Belongs_BPL", RM.Belongs_BPL);
                cmd.Parameters.AddWithValue("@Mobile", RM.Mobile);
                //cmd.Parameters.AddWithValue("@Father_MobNo", RM.Father_MobNo);
                //cmd.Parameters.AddWithValue("@Father_Occup", RM.Father_Occup);
                //cmd.Parameters.AddWithValue("@Mother_MobNo", RM.Mother_MobNo);
                //cmd.Parameters.AddWithValue("@Mother_Occup", RM.Mother_Occup);
                cmd.Parameters.AddWithValue("@Aadhar_num", RM.Aadhar_num);
                cmd.Parameters.AddWithValue("@Group_Name", RM.MyGroup);
                cmd.Parameters.AddWithValue("@E_punjab_Std_id", RM.E_punjab_Std_id);
                cmd.Parameters.AddWithValue("@Address", RM.Address);
                cmd.Parameters.AddWithValue("@LandMark", RM.LandMark);
                cmd.Parameters.AddWithValue("@Block", RM.Block);

                cmd.Parameters.AddWithValue("@District", RM.DIST);
                cmd.Parameters.AddWithValue("@Tehsil", RM.Tehsil);
                cmd.Parameters.AddWithValue("@PinCode", RM.PinCode);
                if (RM.file == null)
                {
                    cmd.Parameters.AddWithValue("@std_Photo", FilePhoto);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Photo", RM.file.FileName);
                }
                if (RM.std_Sign == null)
                {
                    cmd.Parameters.AddWithValue("@std_Sign", sign);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@std_Sign", RM.std_Sign.FileName);
                }

                //----
                if (RM.IsPrevSchoolSelf.ToString() == "True")
                {
                    //cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", RM.IsPrevSchoolSelf);
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@IsPrevSchoolSelf", 0);
                }

                if (RM.IsPSEBRegNum.ToString() == "True" && !string.IsNullOrEmpty(RM.Registration_num))
                {
                    cmd.Parameters.AddWithValue("@Registration_num", RM.Registration_num);
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 1);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Registration_num", "");
                    cmd.Parameters.AddWithValue("@IsPSEBRegNum", 0);
                }

                if (RM.Provisional.ToString() == "True")
                {

                    cmd.Parameters.AddWithValue("@Provisional", 1);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", null);
                    cmd.Parameters.AddWithValue("@Admission_Num", null);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Provisional", 0);
                    cmd.Parameters.AddWithValue("@AWRegisterNo", RM.AWRegisterNo);
                    cmd.Parameters.AddWithValue("@Admission_Num", RM.Admission_Num);

                }

                if (frm["DA"].ToString() == "N.A.")
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
                cmd.Parameters.AddWithValue("@tblEighthSubject", dtEighthSubject);
                cmd.Parameters.AddWithValue("@Section", RM.Section);
                cmd.Parameters.AddWithValue("@CandStudyMedium", RM.CandiMedium);

                string ipaddress = "";
                try
                {
                    ipaddress = AbstractLayer.StaticDB.GetFullIPAddress();
                }
                catch (Exception)
                {
                    ipaddress = "";
                }
                cmd.Parameters.AddWithValue("@MyIP", ipaddress);
                cmd.Parameters.Add("@StudentUniqueId", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string OutError = (string)cmd.Parameters["@OutError"].Value;
                string outuniqueid = (string)cmd.Parameters["@StudentUniqueId"].Value;
                return outuniqueid;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }

        #endregion


        #region Rough Report & Student Verification Form



        public static DataSet GetStudentRoughReport5th8th_Sp(string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentRoughReport5th8th_Sp";
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet GetStudentVerificationForm5th8th_Sp(string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentVerificationForm5th8th_Sp";
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }
        #endregion


        #region Final Submitted Records form All Forms For Admin

        public static DataSet FinalSubmittedRecordsAll(string search, int pageIndex)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "FinalSubmittedRecordsAll_sp";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 30);
            return db.ExecuteDataSet(cmd);
        }


        public static DataSet CancelStdRegNo(string remarks, string stdid)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CancelStdRegNo_spPM";
            cmd.Parameters.AddWithValue("@remarks", remarks);
            cmd.Parameters.AddWithValue("@stdid", stdid);
            return db.ExecuteDataSet(cmd);
        }

        public static int SwitchForm(string remarks, string stdid, out int OutStatus)
        {
            try
            {
                string result = "";
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SwitchFormSP";
                cmd.Parameters.AddWithValue("@remarks", remarks);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.Add("@outstatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@outstatus"].Value;
                return OutStatus;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return OutStatus;
            }

        }



        public static DataSet GetStudentFinalPrint5th8thSP(string schl, string lot)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetStudentFinalPrint5th8thSP";
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@lot", lot);
            return db.ExecuteDataSet(cmd);
        }




        #endregion Final Submitted Records form All Forms For Admin

        #region UpdateFinalPrintStatus
        public static string UpdateFinalPrintStatus(FinalPrintStatus fps)
        {
            string result = "";

            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateFinalPrintStatusPMSP";
                cmd.Parameters.AddWithValue("@ReportName", fps.ReportName);
                cmd.Parameters.AddWithValue("@Schl", fps.Schl);
                cmd.Parameters.AddWithValue("@Lot", fps.Lot);
                cmd.Parameters.AddWithValue("@EmailTo", fps.EmailTo);
                cmd.Parameters.AddWithValue("@SentDate", fps.SentDate);
                cmd.Parameters.AddWithValue("@SentStatus", fps.SentStatus);
                cmd.Parameters.AddWithValue("@CreatedBy", fps.CreatedBy);
                cmd.Parameters.AddWithValue("@FilePath", fps.FilePath);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                int OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                result = OutStatus.ToString();
                return result;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }



        #endregion

        #region  ImportData F3Form

        public static DataSet SelectAllImportF3(string schl, int pageIndex, string Ses1, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SelectAllImportF3SP";
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 20);
            cmd.Parameters.AddWithValue("@Ses1", Ses1);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);

        }

        public static DataSet ImportF3Form(string Impschoolcode, string CurrentSchl, string chkid, string year)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ImportF3FormSP";
            cmd.Parameters.AddWithValue("@Impschoolcode", Impschoolcode);
            cmd.Parameters.AddWithValue("@CurrentSchl", CurrentSchl);
            cmd.Parameters.AddWithValue("@chkid", chkid);
            cmd.Parameters.AddWithValue("@Ses1", year);
            return db.ExecuteDataSet(cmd);
        }




        #endregion  ImportData F3Form 

        #region  ImportData A3Form

        public static DataSet SelectAllImportA3(string schl, int pageIndex, string Ses1, string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SelectAllImportA3SP";
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@PageIndex", pageIndex);
            cmd.Parameters.AddWithValue("@PageSize", 20);
            cmd.Parameters.AddWithValue("@Ses1", Ses1);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet ImportA3Form(string Impschoolcode, string CurrentSchl, string chkid, string year)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ImportA3FormSP";
            cmd.Parameters.AddWithValue("@Impschoolcode", Impschoolcode);
            cmd.Parameters.AddWithValue("@CurrentSchl", CurrentSchl);
            cmd.Parameters.AddWithValue("@chkid", chkid);
            cmd.Parameters.AddWithValue("@Ses1", year);
            return db.ExecuteDataSet(cmd);
        }




        #endregion  ImportData A3Form 

        public static DataSet CheckReg_AdmDate_and_LateAdmDate(string SCHL1, string Form, out string AdmDate, out string LateAdmDate, out string StartAdmDate)
        {
            DataSet result = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            AdmDate = "";
            LateAdmDate = "";
            StartAdmDate = "";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckReg_AdmDate_and_LateAdmDateSP"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@SCHL", SCHL1);
            cmd.Parameters.AddWithValue("@Form", Form);
            result = db.ExecuteDataSet(cmd);
            AdmDate = result.Tables[0].Rows[0]["AdmDate"].ToString();
            LateAdmDate = result.Tables[0].Rows[0]["LateAdmDate"].ToString();
            StartAdmDate = result.Tables[0].Rows[0]["startadmdate"].ToString();
            return result;
        }


        #region Late Admission


        public static DataSet GetLateAdmissionSchl(string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLateAdmissionSchl_Sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        public static string SetLateAdmissionSchl(string CreatedBy, string ModifyBy, string PanelType, string schl, string RID, string cls, string formNM, string regno, string name, string fname, string mname, string dob, string mobileno, string file, string usertype, string OBoard)
        {
            string result = "";
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SetLateAdmissionSchl_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@PanelType", PanelType);
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@RID", RID);
            cmd.Parameters.AddWithValue("@Class", cls);
            cmd.Parameters.AddWithValue("@form", formNM);
            cmd.Parameters.AddWithValue("@regno", regno);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@fname", fname);
            cmd.Parameters.AddWithValue("@mname", mname);
            cmd.Parameters.AddWithValue("@dob", dob);
            cmd.Parameters.AddWithValue("@mobileno", mobileno);
            cmd.Parameters.AddWithValue("@filepath", file);
            cmd.Parameters.AddWithValue("@usertype", usertype);
            cmd.Parameters.AddWithValue("@OBoard", OBoard);
            cmd.Parameters.AddWithValue("@CreatedBy", CreatedBy);
            cmd.Parameters.AddWithValue("@ModifyBy", ModifyBy);
            cmd.Parameters.Add("@Outstatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            result = db.ExecuteNonQuery(cmd).ToString();
            result = Convert.ToString(cmd.Parameters["@Outstatus"].Value);
            return result;

        }

        public static string FinalSubmitLateAdmissionSchl(string RID)
        {
            string result = "";
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "FinalSubmitLateAdmissionSchl_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@RID", RID);
            result = db.ExecuteNonQuery(cmd).ToString();
            return result;

        }

        public static DataSet LateAdmPrintLetter(string search)
        {

            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLateAdmPrintLetter_Sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);

        }
        public static DataSet LateAdmHistory(string search)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLateAdmHistory_Sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);

        }
        public static string ApproveRejectLateAdmissionAdmin(string RID, string action)
        {
            string result = "";
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ApprRejLateAdmissionAdmin_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@RID", RID);
            cmd.Parameters.AddWithValue("@action", action);
            result = db.ExecuteNonQuery(cmd).ToString();
            return result;
        }
        public static string UpdStsLateAdmissionAdmin(string UserNM, string RID, string status, string ApprDate, string remarks)
        {
            string result = "";
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "UpdStsLateAdmissionAdmin_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@UserNM", UserNM);
            cmd.Parameters.AddWithValue("@RID", RID);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@ApprDate", ApprDate);
            cmd.Parameters.AddWithValue("@remarks", remarks);
            cmd.Parameters.Add("@Outstatus", SqlDbType.Int).Direction = ParameterDirection.Output;
            result = db.ExecuteNonQuery(cmd).ToString();
            result = Convert.ToString(cmd.Parameters["@Outstatus"].Value);
            return result;
        }
        public static DataSet GetLateAdmRIDVerify(int RID)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLateAdmRIDVerify_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@RID", RID);
            return db.ExecuteDataSet(cmd);
        }
        public static DataSet GetLateAdmRIDDataVerify(int RID, string CNM, string FNM, string MNM, string DOB)
        {

            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetLateAdmRIDDataVerify_sp"; //RegDataViewForAllClassSP
            cmd.Parameters.AddWithValue("@RID", RID);
            cmd.Parameters.AddWithValue("@CNM", CNM);
            cmd.Parameters.AddWithValue("@FNM", FNM);
            cmd.Parameters.AddWithValue("@MNM", MNM);
            cmd.Parameters.AddWithValue("@DOB", DOB);
            return db.ExecuteDataSet(cmd);
        }
        #endregion Late Admission


        #region InterBoardMigrationP


        public static DataSet GetInterBoardMigrationPayFee(string RequestID)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "GetInterBoardMigrationPayFeeSP";
                cmd.Parameters.AddWithValue("@RequestID", RequestID);
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);                
                return ds;

            }
            catch (Exception ex)
            {

                return null;
            }

        }
        #endregion

        public DataSet schooltypes(string schid)
        {
            // SqlConnection con = null;
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[CommonCon].ToString()))
                {

                    SqlCommand cmd = new SqlCommand("GetSchoolType", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SCHL", schid);

                    ad.SelectCommand = cmd;
                    ad.Fill(result);
                    con.Open();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result = null;
            }
        }


    }
}