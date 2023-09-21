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
using System.Reflection;

namespace PsebJunior.AbstractLayer
{
    public class SchoolDB : ISchoolRepository
    {
        private Database db;
        private DBContext context;
        private string CommonCon = "myDBConnection";

        public SchoolDB()
        {
            db = DatabaseFactory.CreateDatabase();
            context = new DBContext();
            if (HttpContext.Current.Session["Session"] == null)
            {
                CommonCon = "myDBConnection";
            }
            else
            {
                CommonCon = "myDBConnection";
            }
        }

        //update school profile
        public int UpdateUSIJunior(SchoolModels SM)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            string userIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            int result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateUSIJuniorSP";
                cmd.Parameters.AddWithValue("@SCHL", SM.SCHL);
                // new for primary
                cmd.Parameters.AddWithValue("@SCHLE", SM.SCHLE);
                cmd.Parameters.AddWithValue("@STATIONE", SM.STATIONE);
                cmd.Parameters.AddWithValue("@SCHLP", SM.SCHLP);
                cmd.Parameters.AddWithValue("@STATIONP", SM.STATIONP);
                cmd.Parameters.AddWithValue("@DIST", SM.dist);
                //
                cmd.Parameters.AddWithValue("@AREA", SM.AREA);
                cmd.Parameters.AddWithValue("@udisecode", SM.udisecode);
                cmd.Parameters.AddWithValue("@idno", SM.idno);
                //

                cmd.Parameters.AddWithValue("@PRINCIPAL", SM.PRINCIPAL);
                cmd.Parameters.AddWithValue("@STDCODE", SM.STDCODE);
                cmd.Parameters.AddWithValue("@PHONE", SM.PHONE);
                cmd.Parameters.AddWithValue("@EMAILID", SM.EMAILID);
                cmd.Parameters.AddWithValue("@MOBILE", SM.MOBILE);
                cmd.Parameters.AddWithValue("@CONTACTPER", SM.CONTACTPER);
                cmd.Parameters.AddWithValue("@CPSTD", SM.CPSTD);
                cmd.Parameters.AddWithValue("@CPPHONE", SM.CPPHONE);
                cmd.Parameters.AddWithValue("@OtContactno", SM.OtContactno);
                cmd.Parameters.AddWithValue("@ADDRESSE", SM.ADDRESSE);
                cmd.Parameters.AddWithValue("@ADDRESSP", SM.ADDRESSP);
                cmd.Parameters.AddWithValue("@mobile2", SM.mobile2);
                cmd.Parameters.AddWithValue("@REMARKS", SM.REMARKS);

                cmd.Parameters.AddWithValue("@SCHLESTD", SM.SchlEstd);
                cmd.Parameters.AddWithValue("@SCHLTYPE", SM.SchlType);
                cmd.Parameters.AddWithValue("@Tehsile", SM.Tehsile);
                cmd.Parameters.AddWithValue("@EDUBLOCK", SM.Edublock);
                cmd.Parameters.AddWithValue("@EDUCLUSTER", SM.EduCluster);
                cmd.Parameters.AddWithValue("@DOB", SM.DOB);
                cmd.Parameters.AddWithValue("@DOJ", SM.DOJ);
                cmd.Parameters.AddWithValue("@ExperienceYr", SM.ExperienceYr);
                cmd.Parameters.AddWithValue("@PQualification", SM.PQualification);
                // School Bank Details (2019)
                cmd.Parameters.AddWithValue("@Bank", SM.Bank);
                cmd.Parameters.AddWithValue("@IFSC", SM.IFSC);
                cmd.Parameters.AddWithValue("@acno", SM.acno);
                cmd.Parameters.AddWithValue("@userip", userIP);
                cmd.Parameters.AddWithValue("@correctionno", SM.correctionno);
                cmd.Parameters.Add("@GetCorrectionNo", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                result = (int)cmd.Parameters["@GetCorrectionNo"].Value;

            }
            catch (Exception ex)
            {
                result = -1;
            }
            return result;
        }


        #region SchoolChangePassword

        public int InsertUndertaking(string SCHL, string NoOfPrimary, string NoOfMiddle, out string OutError)
        {
            int result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertUndertakingSP";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                cmd.Parameters.AddWithValue("@NoOfPrimary", NoOfPrimary);
                cmd.Parameters.AddWithValue("@NoOfMiddle", NoOfMiddle);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                OutError = (string)cmd.Parameters["@OutError"].Value;
            }
            catch (Exception ex)
            {
                OutError = "0";
                result = -1;
            }
            return result;
            //ds = db.ExecuteDataSet(cmd);
            //return ds.Tables[0];
        }


        #endregion SchoolChangePassword

        public Task<SchoolDataBySchlModel> GetSchoolDataBySchl(string sid)
        {
            SchoolDataBySchlModel loginSession = new SchoolDataBySchlModel();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSchoolDataBySchl";
            cmd.Parameters.AddWithValue("@sid", sid);
            using (IDataReader reader = db.ExecuteReader(cmd))
            {

                if (reader.Read())
                {
                    loginSession.STATUS = DBNull.Value != reader["STATUS"] ? (string)reader["STATUS"] : default(string);
                    loginSession.SCHL = DBNull.Value != reader["SCHL"] ? (string)reader["SCHL"] : default(string);
                    loginSession.PASSWORD = DBNull.Value != reader["PASSWORD"] ? (string)reader["PASSWORD"] : default(string);

                    loginSession.middle = DBNull.Value != reader["middle"] ? (string)reader["middle"] : default(string);
                    loginSession.fifth = DBNull.Value != reader["fifth"] ? (string)reader["fifth"] : default(string);
                    loginSession.Approved = DBNull.Value != reader["Approved"] ? (bool)reader["Approved"] : default(bool);
                    loginSession.MOBILE = DBNull.Value != reader["MOBILE"] ? (string)reader["MOBILE"] : default(string);
                    loginSession.EMAILID = DBNull.Value != reader["EMAILID"] ? (string)reader["EMAILID"] : default(string);
                    loginSession.LoginStatus = DBNull.Value != reader["LoginStatus"] ? (int)reader["LoginStatus"] : default(int);
                    loginSession.SCHLNME = DBNull.Value != reader["SCHLNME"] ? (string)reader["SCHLNME"] : default(string);


                }
            }
            Thread.Sleep(2000);
            return Task.FromResult(loginSession);

        }


        public Task<LoginSession> CheckLogin(LoginModel LM)  // Type 1=Regular, 2=Open
        {
            LoginSession loginSession = new LoginSession();
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LoginJuniorSP";// LoginSP(old)
                cmd.Parameters.AddWithValue("@UserName", LM.UserName);
                cmd.Parameters.AddWithValue("@Password", LM.Password);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        loginSession.STATUS = DBNull.Value != reader["STATUS"] ? (string)reader["STATUS"] : default(string);
                        loginSession.DIST = DBNull.Value != reader["DIST"] ? (string)reader["DIST"] : default(string);
                        loginSession.SCHL = DBNull.Value != reader["SCHL"] ? (string)reader["SCHL"] : default(string);
                        loginSession.middle = DBNull.Value != reader["middle"] ? (string)reader["middle"] : default(string);
                        loginSession.fifth = DBNull.Value != reader["fifth"] ? (string)reader["fifth"] : default(string);
                        loginSession.Approved = DBNull.Value != reader["Approved"] ? (bool)reader["Approved"] : default(bool);
                        loginSession.MOBILE = DBNull.Value != reader["MOBILE"] ? (string)reader["MOBILE"] : default(string);
                        loginSession.EMAILID = DBNull.Value != reader["EMAILID"] ? (string)reader["EMAILID"] : default(string);
                        loginSession.LoginStatus = DBNull.Value != reader["LoginStatus"] ? (int)reader["LoginStatus"] : default(int);
                        loginSession.DateFirstLogin = DBNull.Value != reader["DateFirstLogin"] ? (DateTime)reader["DateFirstLogin"] : default(DateTime);
                        loginSession.SCHLNME = DBNull.Value != reader["SCHLNME"] ? (string)reader["SCHLNME"] : default(string);
                        loginSession.SCHLNMP = DBNull.Value != reader["SCHLNMP"] ? (string)reader["SCHLNMP"] : default(string);
                        //
                        loginSession.PRINCIPAL = DBNull.Value != reader["PRINCIPAL"] ? (string)reader["PRINCIPAL"] : default(string);
                        loginSession.EXAMCENT = DBNull.Value != reader["EXAMCENT"] ? (string)reader["EXAMCENT"] : default(string);
                        loginSession.PRACCENT = DBNull.Value != reader["PRACCENT"] ? (string)reader["PRACCENT"] : default(string);
                        loginSession.USERTYPE = DBNull.Value != reader["SCHLNME"] ? (string)reader["USERTYPE"] : default(string);
                        loginSession.CLUSTERDETAILS = DBNull.Value != reader["CLUSTERDETAILS"] ? (string)reader["CLUSTERDETAILS"] : default(string);
                        //
                        loginSession.IsMeritoriousSchool = DBNull.Value != reader["IsMeritoriousSchool"] ? (int)reader["IsMeritoriousSchool"] : default(int);
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


        #region SchoolChangePassword

        public int SchoolChangePassword(string SCHL, string CurrentPassword, string NewPassword)
        {
            int result;

            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SchoolChangePasswordSP";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                cmd.Parameters.AddWithValue("@CurrentPassword", CurrentPassword);
                cmd.Parameters.AddWithValue("@NewPassword", NewPassword);
                result = db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                return result = -1;
            }
            return result;
            //ds = db.ExecuteDataSet(cmd);
            //return ds.Tables[0];
        }


        #endregion SchoolChangePassword




        #region  Get SchoolMAster Data by From SchoolMasterViewSP


        public DataSet SchoolMasterViewSP(int type, string SCHL, string search)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SchoolMasterViewSP";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                cmd.Parameters.AddWithValue("@search", search);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }

        }


        public SchoolModels GetSchoolDataBySchl(string SCHL, out DataSet result)
        {
            SchoolModels sm = new SchoolModels();
            DataSet ds = new DataSet();
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SelectSchoolDatabyID";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                ds = db.ExecuteDataSet(cmd);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        sm.id = Convert.ToInt32(ds.Tables[0].Rows[0]["id"].ToString());
                        sm.udisecode = ds.Tables[0].Rows[0]["udisecode"].ToString();
                        sm.SCHL = ds.Tables[0].Rows[0]["schl"].ToString();
                        sm.idno = ds.Tables[0].Rows[0]["idno"].ToString();
                        sm.CLASS = ds.Tables[0].Rows[0]["CLASS"].ToString();
                        sm.OCODE = ds.Tables[0].Rows[0]["OCODE"].ToString();
                        sm.USERTYPE = ds.Tables[0].Rows[0]["USERTYPE"].ToString();
                        sm.session = ds.Tables[0].Rows[0]["SESSION"].ToString();
                        sm.status = ds.Tables[0].Rows[0]["status"].ToString();
                        sm.Approved = ds.Tables[0].Rows[0]["IsApproved"].ToString() == "Y" ? "YES" : "NO";
                        sm.vflag = ds.Tables[0].Rows[0]["IsVerified"].ToString() == "Y" ? "YES" : "NO";
                        sm.AREA = ds.Tables[0].Rows[0]["SchoolArea"].ToString();
                        sm.VALIDITY = ds.Tables[0].Rows[0]["VALIDITY"].ToString();
                        sm.PASSWORD = ds.Tables[0].Rows[0]["PASSWORD"].ToString();
                        sm.ACTIVE = ds.Tables[0].Rows[0]["ACTIVE"].ToString();
                        sm.ADDRESSE = ds.Tables[0].Rows[0]["ADDRESSE"].ToString();
                        sm.ADDRESSP = ds.Tables[0].Rows[0]["ADDRESSP"].ToString();
                        sm.AGRI = ds.Tables[0].Rows[0]["AGRI"].ToString();
                        sm.SCHLE = ds.Tables[0].Rows[0]["SCHLE"].ToString();
                        // sm.SCHLE = ds.Tables[0].Rows[0]["SCHLEfull"].ToString();
                        sm.dist = ds.Tables[0].Rows[0]["DIST"].ToString();
                        sm.DISTE = ds.Tables[0].Rows[0]["DISTE"].ToString();
                        sm.STATIONE = ds.Tables[0].Rows[0]["STATIONE"].ToString();
                        sm.DISTNM = ds.Tables[0].Rows[0]["DISTNM"].ToString();

                        sm.SCHLP = ds.Tables[0].Rows[0]["SCHLP"].ToString();
                        // sm.SCHLP = ds.Tables[0].Rows[0]["SCHLPfull"].ToString();
                        sm.DISTP = ds.Tables[0].Rows[0]["DISTP"].ToString();
                        sm.STATIONP = ds.Tables[0].Rows[0]["STATIONP"].ToString();

                        sm.PRINCIPAL = ds.Tables[0].Rows[0]["PRINCIPAL"].ToString();
                        sm.MOBILE = ds.Tables[0].Rows[0]["MOBILE"].ToString();
                        sm.mobile2 = ds.Tables[0].Rows[0]["mobile2"].ToString();
                        sm.ADDRESSE = ds.Tables[0].Rows[0]["ADDRESSE"].ToString();
                        sm.ADDRESSP = ds.Tables[0].Rows[0]["ADDRESSP"].ToString();
                        sm.CONTACTPER = ds.Tables[0].Rows[0]["CONTACTPER"].ToString();
                        sm.EMAILID = ds.Tables[0].Rows[0]["EMAILID"].ToString();
                        sm.STDCODE = ds.Tables[0].Rows[0]["STDCODE"].ToString();
                        sm.PHONE = ds.Tables[0].Rows[0]["PHONE"].ToString();
                        sm.OtContactno = ds.Tables[0].Rows[0]["OtContactno"].ToString();

                        sm.DOB = ds.Tables[0].Rows[0]["DOB"].ToString();
                        sm.DOJ = ds.Tables[0].Rows[0]["DOJ"].ToString();
                        sm.ExperienceYr = ds.Tables[0].Rows[0]["ExperienceYr"].ToString();
                        sm.PQualification = ds.Tables[0].Rows[0]["PQualification"].ToString();
                        sm.NSQF_flag = ds.Tables[0].Rows[0]["NSQF_flag"].ToString() == "Y" ? "YES" : "NO";


                        //Regular
                        sm.middle = ds.Tables[0].Rows[0]["middle"].ToString() == "Y" ? "YES" : "NO";
                        sm.MATRIC = ds.Tables[0].Rows[0]["MATRIC"].ToString() == "Y" ? "YES" : "NO";
                        sm.HUM = ds.Tables[0].Rows[0]["HUM"].ToString() == "Y" ? "YES" : "NO";
                        sm.SCI = ds.Tables[0].Rows[0]["SCI"].ToString() == "Y" ? "YES" : "NO";
                        sm.COMM = ds.Tables[0].Rows[0]["COMM"].ToString() == "Y" ? "YES" : "NO";
                        sm.VOC = ds.Tables[0].Rows[0]["VOC"].ToString() == "Y" ? "YES" : "NO";
                        sm.TECH = ds.Tables[0].Rows[0]["TECH"].ToString() == "Y" ? "YES" : "NO";
                        sm.AGRI = ds.Tables[0].Rows[0]["AGRI"].ToString() == "Y" ? "YES" : "NO";

                        //OPen
                        sm.omiddle = ds.Tables[0].Rows[0]["omiddle"].ToString() == "Y" ? "YES" : "NO";
                        sm.OMATRIC = ds.Tables[0].Rows[0]["OMATRIC"].ToString() == "Y" ? "YES" : "NO";
                        sm.OHUM = ds.Tables[0].Rows[0]["OHUM"].ToString() == "Y" ? "YES" : "NO";
                        sm.OSCI = ds.Tables[0].Rows[0]["OSCI"].ToString() == "Y" ? "YES" : "NO";
                        sm.OCOMM = ds.Tables[0].Rows[0]["OCOMM"].ToString() == "Y" ? "YES" : "NO";
                        sm.OVOC = ds.Tables[0].Rows[0]["OVOC"].ToString() == "Y" ? "YES" : "NO";
                        sm.OTECH = ds.Tables[0].Rows[0]["OTECH"].ToString() == "Y" ? "YES" : "NO";
                        sm.OAGRI = ds.Tables[0].Rows[0]["OAGRI"].ToString() == "Y" ? "YES" : "NO";

                        //---------------Ranjan------------
                        sm.HID_UTYPE = ds.Tables[0].Rows[0]["HID_UTYPE"].ToString();
                        sm.MID_UTYPE = ds.Tables[0].Rows[0]["MID_UTYPE"].ToString();
                        sm.H_UTYPE = ds.Tables[0].Rows[0]["H_UTYPE"].ToString();
                        sm.S_UTYPE = ds.Tables[0].Rows[0]["S_UTYPE"].ToString();
                        sm.C_UTYPE = ds.Tables[0].Rows[0]["C_UTYPE"].ToString();
                        sm.V_UTYPE = ds.Tables[0].Rows[0]["V_UTYPE"].ToString();
                        sm.A_UTYPE = ds.Tables[0].Rows[0]["A_UTYPE"].ToString();
                        sm.T_UTYPE = ds.Tables[0].Rows[0]["T_UTYPE"].ToString();

                        sm.HID_YR = sm.MATRIC == "YES" ? ds.Tables[0].Rows[0]["HID_YR"].ToString() : "XXX";
                        sm.MID_YR = sm.MATRIC == "YES" ? ds.Tables[0].Rows[0]["MID_YR"].ToString() : "XXX";
                        sm.HYR = sm.HUM == "YES" ? ds.Tables[0].Rows[0]["HYR"].ToString() : "XXX";
                        sm.SYR = sm.SCI == "YES" ? ds.Tables[0].Rows[0]["SYR"].ToString() : "XXX";
                        sm.CYR = sm.COMM == "YES" ? ds.Tables[0].Rows[0]["CYR"].ToString() : "XXX";
                        sm.VYR = sm.VOC == "YES" ? ds.Tables[0].Rows[0]["VYR"].ToString() : "XXX";
                        sm.TYR = sm.TECH == "YES" ? ds.Tables[0].Rows[0]["TYR"].ToString() : "XXX";
                        sm.AYR = sm.AGRI == "YES" ? ds.Tables[0].Rows[0]["AYR"].ToString() : "XXX";

                        sm.OHID_YR = sm.OMATRIC == "YES" ? ds.Tables[0].Rows[0]["HID_YR"].ToString() : "XXX";
                        sm.OMID_YR = sm.OMATRIC == "YES" ? ds.Tables[0].Rows[0]["MID_YR"].ToString() : "XXX";
                        sm.OHYR = sm.OHUM == "YES" ? ds.Tables[0].Rows[0]["HYR"].ToString() : "XXX";
                        sm.OSYR = sm.OSCI == "YES" ? ds.Tables[0].Rows[0]["SYR"].ToString() : "XXX";
                        sm.OCYR = sm.OCOMM == "YES" ? ds.Tables[0].Rows[0]["CYR"].ToString() : "XXX";
                        sm.OVYR = sm.OVOC == "YES" ? ds.Tables[0].Rows[0]["VYR"].ToString() : "XXX";
                        sm.OTYR = sm.OTECH == "YES" ? ds.Tables[0].Rows[0]["TYR"].ToString() : "XXX";
                        sm.OAYR = sm.OAGRI == "YES" ? ds.Tables[0].Rows[0]["AYR"].ToString() : "XXX";
                        //---Secsion ---------------
                        sm.HID_YR_SEC = sm.MATRIC == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.MID_YR_SEC = sm.MATRIC == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.HYR_SEC = sm.HUM == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.SYR_SEC = sm.SCI == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.CYR_SEC = sm.COMM == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.VYR_SEC = sm.VOC == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.TYR_SEC = sm.TECH == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";
                        sm.AYR_SEC = sm.AGRI == "YES" && sm.HID_UTYPE != "AFF" && sm.HID_UTYPE != "ASS" ? "N.A." : "XXX";

                        sm.OHID_YR_SEC = sm.OMATRIC == "YES" ? "N.A." : "XXX";
                        sm.OMID_YR_SEC = sm.OMATRIC == "YES" ? "N.A." : "XXX";
                        sm.OHYR_SEC = sm.OHUM == "YES" ? "N.A." : "XXX";
                        sm.OSYR_SEC = sm.OSCI == "YES" ? "N.A." : "XXX";
                        sm.OCYR_SEC = sm.OCOMM == "YES" ? "N.A." : "XXX";
                        sm.OVYR_SEC = sm.OVOC == "YES" ? "N.A." : "XXX";
                        sm.OTYR_SEC = sm.OTECH == "YES" ? "N.A." : "XXX";
                        sm.OAYR_SEC = sm.OAGRI == "YES" ? "N.A." : "XXX";

                        sm.Tehsile = ds.Tables[0].Rows[0]["Tcode"].ToString();

                        sm.SchlEstd = ds.Tables[0].Rows[0]["SCHLESTD"].ToString();
                        sm.SchlType = ds.Tables[0].Rows[0]["SCHLTYPE"].ToString();
                        sm.Edublock = ds.Tables[0].Rows[0]["EDUBLOCK"].ToString();
                        sm.EduCluster = ds.Tables[0].Rows[0]["EDUCLUSTER"].ToString();
                        //
                        sm.omattype = ds.Tables[0].Rows[0]["omattype"].ToString();
                        sm.ohumtype = ds.Tables[0].Rows[0]["ohumtype"].ToString();
                        sm.oscitype = ds.Tables[0].Rows[0]["oscitype"].ToString();
                        sm.ocommtype = ds.Tables[0].Rows[0]["ocommtype"].ToString();
                        sm.Bank = ds.Tables[0].Rows[0]["bank"].ToString();
                        sm.IFSC = ds.Tables[0].Rows[0]["ifsc"].ToString();
                        sm.acno = ds.Tables[0].Rows[0]["acno"].ToString();


                        //FIFTH
                        sm.fifth = ds.Tables[0].Rows[0]["fifth"].ToString() == "Y" ? "YES" : "NO";
                        sm.FIF_YR = sm.fifth == "YES" ? ds.Tables[0].Rows[0]["FIF_YR"].ToString() : "XXX";
                        sm.FIF_UTYPE = ds.Tables[0].Rows[0]["FIF_UTYPE"].ToString();
                        sm.FIF_S = ds.Tables[0].Rows[0]["FIF_S"].ToString();
                        sm.lclass = ds.Tables[0].Rows[0]["lclass"].ToString();
                        //
                        sm.NoOfMiddle = ds.Tables[0].Rows[0]["NoOfMiddle"].ToString();
                        sm.NoOfPrimary = ds.Tables[0].Rows[0]["NoOfPrimary"].ToString();
                    }
                }
                result = ds;
                return sm;

            }
            catch (Exception ex)
            {
                result = null;
                return null;
            }

        }



        #endregion  Get SchoolMAster Data by From SchoolMasterViewSP



        public DataSet SearchSchoolDetailsPaging(string search, int startIndex, int endIndex)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAdminSchoolMasterNewPaging";
            cmd.Parameters.AddWithValue("@startIndex", startIndex);
            cmd.Parameters.AddWithValue("@endIndex", endIndex);
            cmd.Parameters.AddWithValue("@search", search);
            return db.ExecuteDataSet(cmd);
        }

        #region School FinalAdmitCard

        public DataSet PrintAdmitCard(string Search, string schl, string cls)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PrintAdmitCardSP_Junior";
                cmd.Parameters.AddWithValue("@search", Search);
                cmd.Parameters.AddWithValue("@SCHL", schl);
                cmd.Parameters.AddWithValue("@class", cls);

                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        #endregion  School FinalAdmitCard


        #region Marks Entry Panel For Primary Class Regular

        public DataSet GetMarksEntryDataBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetMarksEntryDataBySCHL"; //Regular
                                                             // cmd.CommandText = "GetMarksEntryDataBySCHL_PVT"; //PRivate Reappeatr
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }



        public string AllotMarksEntry(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotMarksEntry"; //Regular
                //cmd.CommandText = "AllotMarksEntry_PVT"; ////PRivate Reappeatr
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet MarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "MarksEntryReport"; //Regular
                //cmd.CommandText = "MarksEntryReport_PVT"; ////PRivate Reappeatr
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion Marks Entry Panel For Primary Class  Regular

        #region  School Cut List
        public DataSet GetCentreSchl(string schl, string cls)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCentreSchlSP"; //GetCentreSchlSP
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@cls", cls);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public DataSet CutList(string Search, string schl, string CLASS, string Type, string Status)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CutListSP_Junior"; //CutListSP_Junior
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@CLASS", CLASS);
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.AddWithValue("@Status", Status);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        #endregion  School Cut List


        #region CCE Marks Entry Panel  

        public DataSet GetCCEMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetCCEMarksDataBySCHLPrimaryMiddle"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public string AllotCCEMarks(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotCCEMarksPrimaryMiddle"; //AllotCCESenior
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet CCEMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CCEMarksEntryReportPrimaryMiddle"; //
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion CCE Marks Entry Panel 


        #region    Practical Exams

        public DataSet GetPracticalMarks_Schl(string SCHL, string cls)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracticalChart_SP";
                cmd.Parameters.AddWithValue("@schl", SCHL);
                cmd.Parameters.AddWithValue("@cls", cls);

                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }




        public DataSet PracExamEnterMarks(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber)
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracExamEnterMarksSP";
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@SelectedAction", SelectedAction);
                cmd.Parameters.AddWithValue("@pageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                return result = null;
            }
        }

        public DataSet PracExamViewList(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub)
        {

            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracExamViewListSP_ExceptVOC";
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@SelectedAction", SelectedAction);
                cmd.Parameters.AddWithValue("@pageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                return result = null;
            }
        }


        public DataSet ViewPracExamEnterMarks(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub)
        {

            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ViewPracExamEnterMarksSP_ExceptVOC";
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@SelectedAction", SelectedAction);
                cmd.Parameters.AddWithValue("@pageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                return result = null;
            }
        }


        public DataSet ViewPracExamFinalSubmit(string class1, string rp, string cent, string Search, int SelectedAction, int pageNumber, string sub)
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ViewPracExamFinalSubmitSPNew_ExceptVOC";
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@rp", rp);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@SelectedAction", SelectedAction);
                cmd.Parameters.AddWithValue("@pageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                return result = null;
            }

        }


        public string AllotPracMarks(string RP, DataTable dtSub, string class1, out int OutStatus, out string OutError)  // BankLoginSP
        {
            string result = "";
            try
            {

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotPracMarks";
                cmd.Parameters.AddWithValue("@RP", RP);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return result;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                OutError = ex.Message;
                //mbox(ex);
                return result = "";
            }
        }




        public string RemovePracMarks(string RP, DataTable dtSub, string class1, out int OutStatus, out string OutError)  // BankLoginSP
        {

            string result = "";
            try
            {

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RemovePracMarks";
                cmd.Parameters.AddWithValue("@RP", RP);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return result;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                OutError = ex.Message;
                //mbox(ex);
                return result = "";
            }

        }


        public string PracExamFinalSubmit(string ExamCent, string class1, string RP, string cent, string sub, string schl, DataTable dtSub, out int OutStatus, out string OutError)  // BankLoginSP
        {

            string result = "";
            try
            {

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PracExamFinalSubmitSPRN";
                cmd.Parameters.AddWithValue("@ExamCent", ExamCent);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@RP", RP);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@sub", sub);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;
                return result;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                OutError = ex.Message;
                //mbox(ex);
                return result = "";
            }
        }

        #endregion Prac Exam Enter

        #region Signature Chart and Confidential List Primary Middle Both

        public DataSet SignatureChart(int type, string cls, string SCHL, string cent)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SignatureChartSP_Junior";
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@cls", cls);
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                cmd.Parameters.AddWithValue("@cent", cent);

                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public DataSet GetSignatureChart(SchoolModels sm)
        {
            try
            {
                string roll = "";
                if (sm.ExamRoll != "")
                {
                    roll = "and roll='" + sm.ExamRoll + "'";
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSignatureChartSP_Junior";
                cmd.Parameters.AddWithValue("@cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@sub", sm.ExamSub);
                cmd.Parameters.AddWithValue("@roll", roll);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataSet GetConfidentialList(SchoolModels sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetConfidentialListSP_Junior";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        #endregion Signature Chart and Confidential List Primary Middle Both

        #region CapacityLetter
        public DataSet CapacityLetter(string SCHL)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CapacityLetter_Sp";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        #endregion CapacityLetter

        #region PhyChlMarksEntry Marks Entry Panel  

        public DataSet GetPhyChlMarksEntryMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPhyChlMarksEntryMarksDataBySCHL"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public string AllotPhyChlMarksEntryMarks(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotPhyChlMarksEntryMarks"; //AllotPhyChlMarksEntrySenior
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet PhyChlMarksEntryMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PhyChlMarksEntryMarksEntryReport"; //
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion PhyChlMarksEntry Marks Entry Panel 


        #region ElectiveTheory Marks Entry Panel  

        public DataSet GetElectiveTheoryMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetElectiveTheoryMarksDataBySCHL"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public string AllotElectiveTheoryMarks(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotElectiveTheoryMarks"; //AllotCCESenior
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet ElectiveTheoryMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ElectiveTheoryMarksEntryReport"; //
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion ElectiveTheory Marks Entry Panel 


        #region  School Result Declare 

        public DataSet GetSchoolResultDetails(string Search, string schl, string class1, string rp)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSchoolResultMarch"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@Search", Search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@type", rp);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        #endregion  School Result Declare 


        public SchoolPremisesInformation SchoolPremisesInformationBySchl(string SCHL, out DataSet ds1)
        {
            SchoolPremisesInformation sm = new SchoolPremisesInformation();
            DataSet ds = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalServer"].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("SchoolPremisesInformationBySchl", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SCHL", SCHL);
                    ad.SelectCommand = cmd;
                    ad.Fill(ds);
                    con.Open();

                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                    {
                        ds1 = null;
                        return null;
                    }
                    else
                    {
                        ds1 = ds;
                        sm.ID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString() == "" ? "0" : ds.Tables[0].Rows[0]["ID"].ToString());
                        if (sm.ID > 0)
                        {
                            sm.SSD1 = ds.Tables[0].Rows[0]["SSD1"].ToString();
                            sm.SSD2 = ds.Tables[0].Rows[0]["SSD2"].ToString();
                            sm.SSD3 = Convert.ToInt32(ds.Tables[0].Rows[0]["SSD3"].ToString());
                            sm.SSD4 = Convert.ToInt32(ds.Tables[0].Rows[0]["SSD4"].ToString());
                            sm.CB5 = float.Parse(ds.Tables[0].Rows[0]["CB5"].ToString());
                            sm.CB6 = float.Parse(ds.Tables[0].Rows[0]["CB6"].ToString());
                            sm.CB7 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB7"].ToString());
                            sm.CB8 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB8"].ToString());
                            sm.CB9 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB9"].ToString());
                            sm.CB10 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB10"].ToString());
                            sm.CB11 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB11"].ToString());
                            sm.CB12 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB12"].ToString());
                            sm.CB13 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB13"].ToString());
                            sm.CB14 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB14"].ToString());
                            sm.CB15 = Convert.ToInt32(ds.Tables[0].Rows[0]["CB15"].ToString());
                            sm.CB16 = Convert.ToString(ds.Tables[0].Rows[0]["CB16"].ToString());
                            sm.ECD17 = ds.Tables[0].Rows[0]["ECD17"].ToString();
                            sm.ECD18 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD18"].ToString());
                            sm.ECD19 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD19"].ToString());
                            sm.ECD20 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD20"].ToString());
                            sm.ECD21 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD21"].ToString());
                            sm.ECD22 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD22"].ToString());
                            sm.ECD23 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD23"].ToString());
                            sm.ECD24 = Convert.ToInt32(ds.Tables[0].Rows[0]["ECD24"].ToString());
                            sm.ECD25 = ds.Tables[0].Rows[0]["ECD25"].ToString();
                            sm.CWS26 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS26"].ToString());
                            sm.CWS27 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS27"].ToString());
                            sm.CWS28 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS28"].ToString());
                            sm.CWS29 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS29"].ToString());
                            sm.CWS30 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS30"].ToString());
                            sm.CWS31 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS31"].ToString());
                            sm.CWS32 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS32"].ToString());
                            sm.CWS33 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS33"].ToString());
                            sm.CWS34 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS34"].ToString());
                            sm.CWS35 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS35"].ToString());
                            sm.CWS36 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS36"].ToString());
                            sm.CWS37 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS37"].ToString());
                            sm.CWS38 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS38"].ToString());
                            sm.CWS39 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS39"].ToString());
                            sm.CWS40 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS40"].ToString());
                            sm.CWS41 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS41"].ToString());
                            sm.CWS42 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS42"].ToString());
                            sm.CWS43 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS43"].ToString());
                            sm.CWS44 = Convert.ToInt32(ds.Tables[0].Rows[0]["CWS44"].ToString());
                            sm.CSS45 = Convert.ToInt32(ds.Tables[0].Rows[0]["CSS45"].ToString());
                            sm.CSS46 = Convert.ToInt32(ds.Tables[0].Rows[0]["CSS46"].ToString());
                            sm.CSS47 = Convert.ToInt32(ds.Tables[0].Rows[0]["CSS47"].ToString());
                            sm.CSS48 = Convert.ToInt32(ds.Tables[0].Rows[0]["CSS48"].ToString());
                            sm.PG49 = float.Parse(ds.Tables[0].Rows[0]["PG49"].ToString());
                            sm.PG50 = Convert.ToInt32(ds.Tables[0].Rows[0]["PG50"].ToString());
                            sm.PG51 = ds.Tables[0].Rows[0]["PG51"].ToString();
                            sm.PG52 = Convert.ToInt32(ds.Tables[0].Rows[0]["PG52"].ToString());
                            sm.PG53 = ds.Tables[0].Rows[0]["PG53"].ToString();
                            sm.PG54 = Convert.ToInt32(ds.Tables[0].Rows[0]["PG54"].ToString());
                            sm.LIB55 = float.Parse(ds.Tables[0].Rows[0]["LIB55"].ToString());
                            sm.LIB56 = ds.Tables[0].Rows[0]["LIB56"].ToString();
                            sm.LIB57 = Convert.ToInt32(ds.Tables[0].Rows[0]["LIB57"].ToString());
                            sm.LIB58 = Convert.ToInt32(ds.Tables[0].Rows[0]["LIB58"].ToString());
                            sm.LIB59 = Convert.ToInt32(ds.Tables[0].Rows[0]["LIB59"].ToString());
                            sm.LAB60 = float.Parse(ds.Tables[0].Rows[0]["LAB60"].ToString());
                            sm.LAB61 = Convert.ToInt32(ds.Tables[0].Rows[0]["LAB61"].ToString());
                            sm.LAB62 = float.Parse(ds.Tables[0].Rows[0]["LAB62"].ToString());
                            sm.LAB63 = Convert.ToInt32(ds.Tables[0].Rows[0]["LAB63"].ToString());
                            sm.LAB64 = float.Parse(ds.Tables[0].Rows[0]["LAB64"].ToString());
                            sm.LAB65 = Convert.ToInt32(ds.Tables[0].Rows[0]["LAB65"].ToString());
                            sm.LAB66 = float.Parse(ds.Tables[0].Rows[0]["LAB66"].ToString());
                            sm.LAB67 = Convert.ToInt32(ds.Tables[0].Rows[0]["LAB67"].ToString());
                            sm.CLAB68 = float.Parse(ds.Tables[0].Rows[0]["CLAB68"].ToString());
                            sm.CLAB69 = Convert.ToInt32(ds.Tables[0].Rows[0]["CLAB69"].ToString());
                            sm.CLAB70 = Convert.ToInt32(ds.Tables[0].Rows[0]["CLAB70"].ToString());
                            sm.CLAB71 = Convert.ToString(ds.Tables[0].Rows[0]["CLAB71"].ToString());
                            sm.OTH72 = ds.Tables[0].Rows[0]["OTH72"].ToString();
                            sm.OTH73 = ds.Tables[0].Rows[0]["OTH73"].ToString();
                            sm.OTH74 = ds.Tables[0].Rows[0]["OTH74"].ToString();
                            sm.OTH75 = ds.Tables[0].Rows[0]["OTH75"].ToString();
                            sm.OTH76 = ds.Tables[0].Rows[0]["OTH76"].ToString();
                            sm.OTH77 = ds.Tables[0].Rows[0]["OTH77"].ToString();
                            sm.OTH78 = ds.Tables[0].Rows[0]["OTH78"].ToString();
                            sm.OTH79 = ds.Tables[0].Rows[0]["OTH79"].ToString();
                            sm.OTH80 = ds.Tables[0].Rows[0]["OTH80"].ToString();
                            sm.OTH81 = ds.Tables[0].Rows[0]["OTH81"].ToString();
                            sm.OTH82 = ds.Tables[0].Rows[0]["OTH82"].ToString();
                            sm.OTH83 = ds.Tables[0].Rows[0]["OTH83"].ToString();
                            sm.ISACTIVE = Convert.ToBoolean(ds.Tables[0].Rows[0]["ISACTIVE"].ToString());
                            sm.CREATEDDATE = Convert.ToDateTime(string.IsNullOrEmpty(ds.Tables[0].Rows[0]["CREATEDDATE"].ToString()) ? "1990-01-01 00:00:00.000" : ds.Tables[0].Rows[0]["CREATEDDATE"].ToString());
                            sm.UDISECODE = Convert.ToString(ds.Tables[0].Rows[0]["UDISECODE"].ToString());
                            sm.ChallanId = Convert.ToString(ds.Tables[0].Rows[0]["ChallanId"].ToString());
                            sm.ChallanDt = Convert.ToString(ds.Tables[0].Rows[0]["ChallanDt"].ToString());
                            sm.challanVerify = Convert.ToInt32(ds.Tables[0].Rows[0]["challanVerify"].ToString());


                            sm.IsFinalSubmit = Convert.ToInt32(string.IsNullOrEmpty(ds.Tables[0].Rows[0]["IsFinalSubmit"].ToString()) ? "0" : ds.Tables[0].Rows[0]["IsFinalSubmit"].ToString());
                            if (string.IsNullOrEmpty(ds.Tables[0].Rows[0]["FinalSubmitOn"].ToString()))
                            {
                                sm.FinalSubmitOn = Convert.ToDateTime("1990-01-01 00:00:00.000");
                            }
                            else
                            {
                                sm.FinalSubmitOn = Convert.ToDateTime(ds.Tables[0].Rows[0]["FinalSubmitOn"].ToString());
                                //DateTime FinalSubmitDate;
                                //if (DateTime.TryParseExact(ds.Tables[0].Rows[0]["FinalSubmitOn"].ToString().Split(' ')[0], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out FinalSubmitDate))
                                //{
                                //    sm.FinalSubmitOn = FinalSubmitDate;
                                //}
                            }
                        }
                        return sm;
                    }
                }
            }
            catch (Exception ex)
            {
                ds1 = null;
                return null;
            }

        }


        #region Pre Board Exam Marks Entry Panel  

        public DataSet GetPreBoardExamTheoryBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPreBoardExamTheoryBySCHL"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public string AllotPreBoardExamTheoryPM(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotPreBoardExamTheoryPM"; //AllotCCESenior
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet PreBoardExamTheoryReportPM(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PreBoardExamTheoryReportPM"; //
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion  Pre Board Exam Marks Entry Panel    


        #region School to School Migration

        public static DataSet ApplyStudentSchoolMigrationSearch(int type, string search, string schl)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "ApplyStudentSchoolMigrationSearchSP"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);                
                return ds;

            }
            catch (Exception ex)
            {

                return null;
            }

        }




        public static string CancelStudentSchoolMigration(string cancelremarks, string stdid, string migid, out string outstatus, string updatedby, string Type)
        {
            try
            {
                string result = "";
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "CancelStudentSchoolMigrationSP";
                cmd.Parameters.AddWithValue("@MigrationId", migid);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@updatedby", updatedby);
                cmd.Parameters.AddWithValue("@cancelremarks", cancelremarks);
                cmd.Parameters.AddWithValue("@Type", Type);
                string userIP = AbstractLayer.StaticDB.GetFullIPAddress();
                cmd.Parameters.AddWithValue("@UserIP", userIP);
                cmd.Parameters.Add("@OutStatus", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                outstatus = Convert.ToString(cmd.Parameters["@OutStatus"].Value);
                string OutError = Convert.ToString(cmd.Parameters["@OutError"].Value);
                return outstatus;
            }
            catch (Exception ex)
            {
                outstatus = "-1";
                return outstatus;
            }
        }

        public static string UpdateStatusStudentSchoolMigration(string remarks, string stdid, string migid, string status, string AppLevel, out string outstatus, string updatedby, string Type)
        {
            try
            {
                string result = "";
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "UpdateStatusStudentSchoolMigrationSP";
                cmd.Parameters.AddWithValue("@MigrationId", migid);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@AppLevel", AppLevel);
                cmd.Parameters.AddWithValue("@remarks", remarks);
                cmd.Parameters.AddWithValue("@updatedby", updatedby);
                cmd.Parameters.AddWithValue("@Type", Type);
                string userIP = AbstractLayer.StaticDB.GetFullIPAddress();
                cmd.Parameters.AddWithValue("@UserIP", userIP);
                cmd.Parameters.Add("@OutStatus", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                outstatus = Convert.ToString(cmd.Parameters["@OutStatus"].Value);
                string OutError = Convert.ToString(cmd.Parameters["@OutError"].Value);
                return outstatus;
            }
            catch (Exception ex)
            {
                outstatus = "-1";
                return outstatus;
            }
        }


        public static DataSet StudentSchoolMigrationsSearch(int type, string search, string schl)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "StudentSchoolMigrationsSearchSP"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);                
                return ds;

            }
            catch (Exception ex)
            {

                return null;
            }

        }

        public static List<StudentSchoolMigrationViewModel> StudentSchoolMigrationsSearchModel(int type, string search, string schl)
        {
            List<StudentSchoolMigrationViewModel> studentSchoolMigrationViewModel = new List<StudentSchoolMigrationViewModel>();
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "StudentSchoolMigrationsSearchSP"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW               
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                ds = db.ExecuteDataSet(cmd);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var itemSubUType = StaticDB.DataTableToList<StudentSchoolMigrationViewModel>(ds.Tables[0]);
                    studentSchoolMigrationViewModel = itemSubUType.ToList();

                }
                return studentSchoolMigrationViewModel;

            }
            catch (Exception ex)
            {
                return studentSchoolMigrationViewModel;
            }
        }



        public static DataSet GetStudentSchoolMigrationsPayment(int migid, string stdid)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "GetStudentSchoolMigrationsPaymentSP"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW
                cmd.Parameters.AddWithValue("@MigrationId", migid);
                cmd.Parameters.AddWithValue("@StdId", stdid);
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

        #region
        public static DataSet MeritoriousCentreMaster(string schl)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "MeritoriousCentreMasterSp"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW
                cmd.Parameters.AddWithValue("@schl", schl);
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);                
                return ds;

            }
            catch (Exception ex)
            {

                return null;
            }

        }


        public static DataSet GetMeritoriousCentCodeBySchl(string SCHL)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetMeritoriousCentCodeBySchlSP";
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                ds = db.ExecuteDataSet(cmd);
                //result = db.ExecuteNonQuery(cmd);                
                return ds;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public static DataSet GetMeritoriousConfidentialList(SchoolModels sm)
        {
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.CommandText = "GetMeritoriousConfidentialListSP";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@ExamRoll", sm.ExamRoll);
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


        #region  Re-Exam For Absent Student in Term-1
        public static List<ReExamTermStudentsSearchModel> GetReExamTermStudentList(string type, string RP, string cls, string schl, string search, out DataSet dsOut)
        {
            List<ReExamTermStudentsSearchModel> registrationSearchModels = new List<ReExamTermStudentsSearchModel>();
            DataSet ds = new DataSet();
            Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetReExamTermStudentListSP";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@RP", RP);
            cmd.Parameters.AddWithValue("@Class", cls);
            cmd.Parameters.AddWithValue("@schl", schl);
            cmd.Parameters.AddWithValue("@search", search);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {
                var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new ReExamTermStudentsSearchModel
                {
                    Std_id = dataRow.Field<long>("Std_id"),
                    Roll = dataRow.Field<string>("Roll"),
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
                    IsExistsInReExamTermStudents = dataRow.Field<int>("IsExistsInReExamTermStudents"),
                    ReExamId = dataRow.Field<long>("ReExamId"),
                    IsChallanCancel = dataRow.Field<int>("IsChallanCancel"),

                }).ToList();

                registrationSearchModels = eList.ToList();
            }
            dsOut = ds;
            return registrationSearchModels;

        }


        public int InsertReExamTermStudentList(List<ReExamTermStudents> list)
        {
            int result = 0;
            if (list.Count() > 0)
            {
                context.ReExamTermStudents.AddRange(list);
                result = context.SaveChanges();
            }
            return result;
        }

        public int RemoveRangeOnDemandCertificateStudentList(List<ReExamTermStudents> list)
        {
            int result = 0;
            if (list.Count() > 0)
            {
                int i = 0;
                foreach (ReExamTermStudents reExamTermStudents in list)
                {
                    context.ReExamTermStudents.Attach(reExamTermStudents);
                    context.ReExamTermStudents.Remove(reExamTermStudents);
                    context.SaveChanges();
                    i++;
                }
                result = i;
            }
            return result;
        }


        public static List<ReExamTermStudents_ChallanDetailsViews> ReExamTermStudents_ChallanList(string schl, out DataSet dsOut)
        {
            List<ReExamTermStudents_ChallanDetailsViews> registrationSearchModels = new List<ReExamTermStudents_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ReExamTermStudents_ChallanListMiddlePrimarySP";
            cmd.Parameters.AddWithValue("@schl", schl);
            ds = db.ExecuteDataSet(cmd);
            if (ds != null)
            {
                var eList = StaticDB.DataTableToList<ReExamTermStudents_ChallanDetailsViews>(ds.Tables[0]);
                registrationSearchModels = eList.ToList();
            }
            dsOut = ds;
            return registrationSearchModels;

        }


        public static DataSet ReExamTermStudentsCountRecordsClassWise(string search, string schl)
        {
            List<ReExamTermStudents_ChallanDetailsViews> registrationSearchModels = new List<ReExamTermStudents_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ReExamTermStudentsCountRecordsClassWise_SP";
            cmd.Parameters.AddWithValue("@search", search);
            cmd.Parameters.AddWithValue("@Schl", schl);
            ds = db.ExecuteDataSet(cmd);
            return ds;

        }


        public static DataSet ReExamTermStudentCalculateFee(string cls, string date, string search, string schl)
        {
            List<ReExamTermStudents_ChallanDetailsViews> registrationSearchModels = new List<ReExamTermStudents_ChallanDetailsViews>();
            DataSet ds = new DataSet();
            Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ReExamTermStudentCalculateFeeSP";
            cmd.Parameters.AddWithValue("@cls", cls);
            cmd.Parameters.AddWithValue("@Schl", schl);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@search", search);
            ds = db.ExecuteDataSet(cmd);

            return ds;

        }
        #endregion



        #region  SchoolBasedExams Marks Entry Panel     

        public DataSet GetSchoolBasedExamsMarksDataBySCHL(string search, string schl, int pageNumber, string class1, int action1)
        {
            try
            {
                DataSet ds = new DataSet();
                Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSchoolBasedExamsMarksDataBySCHLPrimaryMiddle"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", 20);
                cmd.Parameters.AddWithValue("@Action", action1);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public string AllotSchoolBasedExamsMarks(string submitby, string stdid, DataTable dtSub, string class1, out int OutStatus)
        {
            try
            {
                string result = "";
                Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AllotSchoolBasedExamsMarks"; //AllotCCESenior
                cmd.Parameters.AddWithValue("@submitby", submitby);
                cmd.Parameters.AddWithValue("@stdid", stdid);
                cmd.Parameters.AddWithValue("@dtSub", dtSub);
                cmd.Parameters.AddWithValue("@class", class1);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                return result;
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                return null;
            }
        }


        public DataSet SchoolBasedExamsMarksEntryReport(string CenterId, int reporttype, string search, string schl, string cls, out string OutError)
        {
            try
            {
                Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                DataSet ds = new DataSet();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SchoolBasedExamsMarksEntryReportPrimaryMiddle"; //
                cmd.Parameters.AddWithValue("@CenterId", CenterId);
                cmd.Parameters.AddWithValue("@reporttype", reporttype);
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
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

        #endregion  SchoolBasedExams Marks Entry Panel     


        #region Signature and Confidential  Chart Middle   
        public DataSet SignatureChartMiddleSub(SchoolModels sm)
        {

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SignatureChartMiddleSub_Sp";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                //cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet SignatureChartMiddle(SchoolModels sm)
        {
            string roll = "";
            if (sm.ExamRoll != "")
            {
                roll = "and roll='" + sm.ExamRoll + "'";
            }
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSignatureChart_SP2702";
                cmd.Parameters.AddWithValue("@cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@sub", sm.ExamSub);
                cmd.Parameters.AddWithValue("@roll", roll);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet ConfidentialListMiddle(SchoolModels sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetConfi_SP2702";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DataSet ConfidentialListMiddleDetail(SchoolModels sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetConfidentialCentcode_SP";
                cmd.Parameters.AddWithValue("@SCHL", sm.ExamCent);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion SSignature and Confidential  Chart Middle--End



        #region Signature and Confidential  Chart  Primary   
        public DataSet SignatureChartPrimarySub(SchoolModels sm)
        {

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "SignatureChartPrimarySub_Sp";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                //cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet SignatureChartPrimary(SchoolModels sm)
        {
            string roll = "";
            if (sm.ExamRoll != "")
            {
                roll = "and roll='" + sm.ExamRoll + "'";
            }
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSignatureChart_SP2702";
                cmd.Parameters.AddWithValue("@cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@sub", sm.ExamSub);
                cmd.Parameters.AddWithValue("@roll", roll);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet ConfidentialListPrimary(SchoolModels sm)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetConfi_SP2702";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion Signature and Confidential  Chart  Primary   Pankaj/Dheeraj


        #region  Practical  SignatureChart and Confidential List


        public DataSet GetPracCentcodeByClass(string schl, int cls)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPracCentcodeByClass";
                cmd.Parameters.AddWithValue("@SCHL", schl);
                cmd.Parameters.AddWithValue("@class", cls);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }



        public DataSet GetSubFromSubMasters(int cls, string type, string schl, string cent)
        {

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetSubFromSubMasters";
                cmd.Parameters.AddWithValue("@schl", schl);
                cmd.Parameters.AddWithValue("@cent", cent);
                cmd.Parameters.AddWithValue("@class", cls);
                cmd.Parameters.AddWithValue("@type", type);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }




        public DataSet PracSignatureChart(SchoolModels sm)
        {
            string roll = "";
            if (sm.ExamRoll != "")
            {
                roll = "and roll='" + sm.ExamRoll + "'";
            }

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPracticalSignatureChart_SP2702";
                cmd.Parameters.AddWithValue("@cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@sub", sm.ExamSub);
                cmd.Parameters.AddWithValue("@roll", roll);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataSet PracConfidentialList(SchoolModels sm)
        {

            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetPracticalConfi_SP2702";
                cmd.Parameters.AddWithValue("@Cent", sm.ExamCent);
                cmd.Parameters.AddWithValue("@class", sm.CLASS);
                return db.ExecuteDataSet(cmd);
            }
            catch (Exception)
            {
                return null;
            }


        }

        #endregion



        #region  InfrasturePerforma

        public DataSet PanelEntryLastDate(string sModule)
        {
            try
            {
                DataSet ds = new DataSet();
                Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_PanelEntryLastDatebyModule"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@Pv_ModeuleName", sModule);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public DataSet GetSchoolUserTableData(string SCHL)
        {
            try
            {
                DataSet ds = new DataSet();
                Microsoft.Practices.EnterpriseLibrary.Data.Database db = DatabaseFactory.CreateDatabase("myDBConnection");
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_GetSchoolUserTableData"; //GetDataBySCHL
                cmd.Parameters.AddWithValue("@SCHL", SCHL);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public DataSet SchoolCenterName(string SchoolCode)
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[CommonCon].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("sp_SchoolCenterName", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SchoolCode", SchoolCode);
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

        public DataSet SchoolCenterNameNearest(string Dist)
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[CommonCon].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("sp_SchoolCenterNameNearest", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Dist", Dist);
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
        public DataSet SchoolCenterAnswerSheetNearest(string Dist)
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[CommonCon].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("sp_SchoolCenterAnswerSheetNearest", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Dist", Dist);
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

        public bool IFSCCheck(string IFSC, string BANK)
        {
            bool b = false;
            Tblifsccodes obj = new Tblifsccodes();
            if (IFSC != null)
            {
                obj = context.Tblifsccodes.SingleOrDefault(x => x.BANK.Trim() == BANK.Trim() && x.IFSC.Trim() == IFSC.Trim());
                if (obj == null)
                {
                    b = false;
                }
                else
                {
                    b = true;
                }

            }
            return b;
        }

        public Task<InfrasturePerformas> GetInfrasturePerformaBySCHL(LoginSession LM)  // Type 1=Regular, 2=Open
        {
            InfrasturePerformas obj = new InfrasturePerformas();
            if (LM != null)
            {
                obj = context.InfrasturePerformas.SingleOrDefault(x => x.SCHL.Trim() == LM.SCHL.Trim());
                if (obj == null)
                {
                    var ipsNew = new InfrasturePerformas()
                    {
                        SCHL = LM.SCHL
                    };
                    context.InfrasturePerformas.Add(ipsNew);
                    context.SaveChanges();
                    obj = context.InfrasturePerformas.SingleOrDefault(x => x.SCHL.Trim() == LM.SCHL.Trim());
                }

            }
            Thread.Sleep(2000);
            return Task.FromResult(obj);

        }


        public Task<tblSchUsers> GetInfrastureTblSchUserPerformaBySCHL(LoginSession LM)  // Type 1=Regular, 2=Open
        {
            tblSchUsers obj = new tblSchUsers();
            if (LM != null)
            {
                obj = context.tblSchUsers.SingleOrDefault(x => x.schl.Trim() == LM.SCHL.Trim());
            }
            Thread.Sleep(2000);
            return Task.FromResult(obj);

        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public DataTable GetInfrasturePerformaBySCHLListSearch(string SCHL, string Dist)  // Type 1=Regular, 2=Open
        {
            List<InfrasturePerformasList> obj = new List<InfrasturePerformasList>();
            if (Dist.ToLower() == "all" && SCHL.ToLower() == "all")
            {
                obj = context.InfrasturePerformasList.ToList();
            }
            else if (SCHL.ToLower() == "all")
            {
                obj = context.InfrasturePerformasList.Where(x => x.DIST.Trim() == Dist).ToList();
            }
            else if (Dist.ToLower() == "all")
            {
                obj = context.InfrasturePerformasList.Where(x => x.SCHL.Trim() == SCHL).ToList();
            }
            else
            {
                obj = context.InfrasturePerformasList.Where(x => x.DIST.Trim() == Dist && x.SCHL == SCHL).ToList();
            }
            Thread.Sleep(2000);
            return ToDataTable(obj);
            //return obj;

        }

        public Task<ChallanModels> GetChallanDetail(string ChallanID)  // Type 1=Regular, 2=Open
        {
            ChallanModels obj = new ChallanModels();
            obj = context.ChallanModels.Where(x => x.CHALLANID.Trim() == ChallanID).FirstOrDefault();

            return Task.FromResult(obj);

        }
        public DataTable GetAllTCode()
        {
            DataSet result = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter();
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings[CommonCon].ToString()))
                {
                    SqlCommand cmd = new SqlCommand("select distinct Tcode,DIST from InfrasturePerformasListWithSchools", con);
                    cmd.CommandType = CommandType.Text;
                    ad.SelectCommand = cmd;
                    ad.Fill(result);
                    con.Open();
                    return result.Tables[0];
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<SelectListItem> GetTCode()
        {
            DataTable dsSession = GetAllTCode(); // SessionMasterSPAdmin
            List<SelectListItem> itemSession = new List<SelectListItem>();
            // itemSession.Add(new SelectListItem { Text = "2017", Value = "2017" });
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["Tcode"].ToString(), Value = @dr["DIST"].ToString() });
            }
            return itemSession;
        }


        public Task<List<InfrasturePerformasListWithSchool>> GetInfrasturePerformaWithSchool(string sDIST, string sSCHL, string sTcode)  // Type 1=Regular, 2=Open
        {
            List<InfrasturePerformasListWithSchool> obj = new List<InfrasturePerformasListWithSchool>();
            if (sTcode.ToLower() == "all")
            {
                if (sSCHL.ToLower() == "all" && sDIST.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.ToList();
                }
                else if (sSCHL.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.DIST == sDIST).ToList();
                }
                else if (sDIST.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.SCHL == sSCHL).ToList();
                }
                else
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.SCHL == sSCHL && x.DIST == sDIST).ToList();
                }
            }
            else
            {
                if (sSCHL.ToLower() == "all" && sDIST.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.Tcode == sTcode).ToList();
                }
                else if (sSCHL.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.DIST == sDIST && x.Tcode == sTcode).ToList();
                }
                else if (sDIST.ToLower() == "all")
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.SCHL == sSCHL && x.Tcode == sTcode).ToList();
                }
                else
                {
                    obj = context.InfrasturePerformasListWithSchool.Where(x => x.SCHL == sSCHL && x.DIST == sDIST && x.Tcode == sTcode).ToList();
                }

            }
            Thread.Sleep(2000);
            return Task.FromResult(obj);

        }

        public Task<InfrasturePerformasList> GetInfrasturePerformaBySCHLList(string SCHL)  // Type 1=Regular, 2=Open
        {
            InfrasturePerformasList obj = new InfrasturePerformasList();
            if (SCHL != null)
            {
                obj = context.InfrasturePerformasList.SingleOrDefault(x => x.SCHL.Trim() == SCHL.Trim());
                if (obj == null)
                {
                    var ipsNew = new InfrasturePerformasList()
                    {
                        SCHL = SCHL
                    };
                    context.InfrasturePerformasList.Add(ipsNew);
                    context.SaveChanges();
                    obj = context.InfrasturePerformasList.SingleOrDefault(x => x.SCHL.Trim() == SCHL.Trim());
                }

            }
            Thread.Sleep(2000);
            return Task.FromResult(obj);

        }


        public Task<InfrasturePerformas> UpdateInfrasturePerformaBySCHL(InfrasturePerformas ips, out int ireturn)
        {
            InfrasturePerformas obj = context.InfrasturePerformas.SingleOrDefault(s => s.SCHL.ToUpper() == ips.SCHL.ToUpper());
            obj.Col1 = ips.Col1;
            obj.Col2 = ips.Col2;
            obj.Col3 = ips.Col3;
            obj.Col4 = ips.Col4;
            obj.Col5 = ips.Col5;
            obj.Col6 = ips.Col6;
            obj.Col7 = ips.Col7;
            obj.Col8 = ips.Col8;
            obj.Col9 = ips.Col9;
            obj.Col10 = ips.Col10;
            obj.Col11 = ips.Col11;
            obj.Col12 = ips.Col12;
            obj.Col13 = ips.Col13;
            obj.Col14 = ips.Col14;
            obj.Col15 = ips.Col15;
            obj.Col16 = ips.Col16;
            obj.Col17 = ips.Col17;
            obj.Col18 = ips.Col18;
            obj.Col19 = ips.Col19;
            obj.Col20 = ips.Col20;
            obj.Col21 = ips.Col21;
            obj.Col22 = ips.Col22;
            obj.Col23 = ips.Col23;
            obj.Col24 = ips.Col24;
            obj.Col25A = ips.Col25A;
            obj.Col25B = ips.Col25B;
            obj.Col25C = ips.Col25C;
            obj.Col29 = ips.Col29;
            obj.Col30 = ips.Col30;
            obj.Col31 = ips.Col31;
            obj.Col32 = ips.Col32;
            obj.Col33 = ips.Col33;
            obj.Col34 = ips.Col34;
            obj.Col35 = ips.Col35;
            obj.Col36 = ips.Col36;
            obj.Col37 = ips.Col37;
            obj.Col38 = ips.Col38;
            obj.Col39 = ips.Col39;
            obj.Col40 = ips.Col40;
            obj.Col41 = ips.Col41;
            obj.Col42 = ips.Col42;
            obj.Col43 = ips.Col43;
            obj.Count5th = ips.Count5th;
            obj.Count8th = ips.Count8th;
            obj.Count9th = ips.Count9th;
            obj.Count10th = ips.Count10th;
            obj.Count11th = ips.Count11th;
            obj.Count12th = ips.Count12th;
            obj.SchoolName1 = ips.SchoolName1;
            obj.DistanceFromTheSchool1 = ips.DistanceFromTheSchool1;
            obj.SchoolName2 = ips.SchoolName2;
            obj.DistanceFromTheSchool2 = ips.DistanceFromTheSchool2;
            obj.SchoolName3 = ips.SchoolName3;
            obj.DistanceFromTheSchool3 = ips.DistanceFromTheSchool3;
            obj.IFSC = ips.IFSC;
            obj.Bank = ips.Bank;
            obj.BankAddress = ips.BankAddress;
            obj.SOLID = ips.SOLID;
            obj.IFSC1 = ips.IFSC1;
            obj.Bank1 = ips.Bank1;
            obj.BankBranch1 = ips.BankBranch1;
            obj.IFSC2 = ips.IFSC2;
            obj.Bank2 = ips.Bank2;
            obj.BankBranch2 = ips.BankBranch2;
            obj.IFSC3 = ips.IFSC3;
            obj.Bank3 = ips.Bank3;
            obj.FinalSubmitStatus = ips.FinalSubmitStatus;
            obj.FinalSubmitDate = ips.FinalSubmitDate;
            obj.BankBranch3 = ips.BankBranch3;
            obj.Statisfied8th = ips.Statisfied8th;
            obj.Statisfied10th = ips.Statisfied10th;
            obj.Statisfied12th = ips.Statisfied12th;
            obj.Distance8th = ips.Distance8th;
            obj.Distance10th = ips.Distance10th;
            obj.Distance12th = ips.Distance12th;
            obj.SchoolCenterNewFor8th = ips.SchoolCenterNewFor8th;
            obj.SchoolCenterNewFor10th = ips.SchoolCenterNewFor10th;
            obj.SchoolCenterNewFor12th = ips.SchoolCenterNewFor12th;

            context.Entry(obj).State = System.Data.Entity.EntityState.Modified;
            ireturn = context.SaveChanges();
            if (ips != null)
            {
                obj = context.InfrasturePerformas.SingleOrDefault(x => x.SCHL.Trim() == ips.SCHL.Trim());
            }
            Thread.Sleep(2000);
            return Task.FromResult(obj);

        }
        #endregion



    }
}
