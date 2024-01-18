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
using PsebPrimaryMiddle.Repository;

namespace PsebJunior.AbstractLayer
{
    public class DBClass 
    {
        #region Check ConString

        public static List<SelectListItem> GetAllFormName()
        {
            List<SelectListItem> formNameList = new List<SelectListItem>();
            formNameList.Add(new SelectListItem { Text = "---Select Form--", Value = "0" });         
            formNameList.Add(new SelectListItem { Text = "F1", Value = "F1" });
            formNameList.Add(new SelectListItem { Text = "F2", Value = "F2" });
            formNameList.Add(new SelectListItem { Text = "A1", Value = "A1" });
            formNameList.Add(new SelectListItem { Text = "A2", Value = "A2" });
            return formNameList;
        }

        public static int GetClassNumber(string FormName)
        {
            int ClassNo = 0;            
            if (FormName == "F1" || FormName == "F2")
            { ClassNo = 5; }
            if (FormName == "A2" || FormName == "A2")
            { ClassNo = 8; }
            return ClassNo;
        }

        public static List<SelectListItem> GetSubUserType()
        {
            DataTable dsSchool = DBClass.GetAllSchoolType().Tables[0]; // passing Value to SchoolDB from model

            var itemSubUType = dsSchool.AsEnumerable().Where(s => s.Field<string>("subtype").ToString() == "Y")
            .Select(dataRow => new SelectListItem
            {
                Text = dataRow.Field<string>("schooltype").ToString(),
                Value = dataRow.Field<string>("abbr").ToString(),
            }).ToList();
            return itemSubUType.ToList();
        }

        public static List<SelectListItem> GetEstalimentYearList()
        {
            List<SelectListItem> itemSession = new List<SelectListItem>();
            for (int i = DateTime.Now.Year; i > 1800; i--)
            {
                int a = i;
                int b = i + 1;
                itemSession.Add(new SelectListItem { Text = a.ToString() + "-" + b.ToString(), Value = a.ToString() + "-" + b.ToString() });
            }

            return itemSession;
        }


        public static List<SelectListItem> GetOrderBy()
        {
            List<SelectListItem> itemOrder = new List<SelectListItem>();
            itemOrder.Add(new SelectListItem { Text = "Superintendent ", Value = "1" });
            itemOrder.Add(new SelectListItem { Text = "Assistant Secretary", Value = "2" });
            itemOrder.Add(new SelectListItem { Text = "Deputy Secretary", Value = "3" });
            itemOrder.Add(new SelectListItem { Text = "Director Computer", Value = "4" });
            itemOrder.Add(new SelectListItem { Text = "Secretary", Value = "5" });
            itemOrder.Add(new SelectListItem { Text = "Vice Chairman", Value = "6" });
            itemOrder.Add(new SelectListItem { Text = "Chairman", Value = "7" });
            return itemOrder;
        }

        public static List<SelectListItem> GetAllResult()
        {
            List<SelectListItem> iList = new List<SelectListItem>();
            iList.Add(new SelectListItem { Text = "PASS", Value = "PASS" });
            iList.Add(new SelectListItem { Text = "FAIL", Value = "FAIL" });
            iList.Add(new SelectListItem { Text = "RE-APPEAR", Value = "RE-APPEAR" });
            iList.Add(new SelectListItem { Text = "ABSENT", Value = "ABSENT" });
            iList.Add(new SelectListItem { Text = "CANCEL", Value = "CANCEL" });
            return iList;
        }

        public static List<SelectListItem> GetAllClass()
        {
            List<SelectListItem> classList = new List<SelectListItem>();
            classList.Add(new SelectListItem { Text = "5", Value = "5" });
            classList.Add(new SelectListItem { Text = "8", Value = "8" });
            classList.Add(new SelectListItem { Text = "10", Value = "10" });
            classList.Add(new SelectListItem { Text = "12", Value = "12" });
            return classList;
        }

        #endregion  Check ConString


        public static List<SelectListItem> GetAcceptRejectDDL()
        {
            List<SelectListItem> itemStatus = new List<SelectListItem>();
            itemStatus.Add(new SelectListItem { Text = "Accept", Value = "A" });
            itemStatus.Add(new SelectListItem { Text = "Reject", Value = "R" });
            return itemStatus;
        }

        #region MASTER TABLE DATA

        public static DataSet SchoolDist(string schl)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SchoolDist_sp";
            cmd.Parameters.AddWithValue("@schl", schl);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet TehsilMaster()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "TehsilMasterSP";
            return db.ExecuteDataSet(cmd);
        }      

        public static DataSet GetAllDistrict()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllDistrict";           
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet SelectAllTehsil(string DISTID)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllTehsil";
            cmd.Parameters.AddWithValue("@DISTID", DISTID);
            return db.ExecuteDataSet(cmd);
        }
       

        public static DataSet SelectBlock(string DIST)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetALLBLOCK_Sp";
            cmd.Parameters.AddWithValue("@DIST", DIST);
            return db.ExecuteDataSet(cmd);
        }


        public static DataSet Select_CLUSTER_NAME(string BLOCK)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetALLCluster_Sp";
            cmd.Parameters.AddWithValue("@BLOCK", BLOCK);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet SchoolSet(int Type1)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SchoolSetSP";
            cmd.Parameters.AddWithValue("@Type", Type1);
            return db.ExecuteDataSet(cmd);
        }

        public static DataTable SessionMaster()
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SessionMasterSP";
            ds = db.ExecuteDataSet(cmd);
            return ds.Tables[0];
        }       

        public static DataSet GetAllClassType()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllClassType";
            // cmd.Parameters.AddWithValue("@Type", Type1);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet GetAllSchoolType()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAllSchoolType";
            // cmd.Parameters.AddWithValue("@Type", Type1);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet Sub08ElectiveSubjectSP()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Sub08ElectiveSubjectSP";
            // cmd.Parameters.AddWithValue("@Type", Type1);
            return db.ExecuteDataSet(cmd);
        }

        public static List<SelectListItem> GetSub08ElectiveSubjectList()
        {
            DataSet ds = Sub08ElectiveSubjectSP(); // passing Value to SchoolDB from model
            DataTable dsClass = ds.Tables[0];
            List <SelectListItem> itemClass = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsClass.Rows)
            {
                itemClass.Add(new SelectListItem { Text = @dr["name_eng"].ToString(), Value = @dr["sub"].ToString() });
            }
            return itemClass;
        }




        #endregion  MASTER TABLE DATA




        #region MASTER TABLE DATA LIST



        public static List<SelectListItem> GetClass()
        {         
            DataSet ds = GetAllClassType(); // passing Value to SchoolDB from model
            DataTable dsClass = ds.Tables[0];
            List<SelectListItem> itemClass = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsClass.Rows)
            {
                itemClass.Add(new SelectListItem { Text = @dr["class"].ToString(), Value = @dr["Id"].ToString() });
            }
            return itemClass;
        }
        public static List<SelectListItem> GetSchool()
        {            
            DataSet ds = GetAllSchoolType(); // passing Value to SchoolDB from model
            DataTable dsSchool = ds.Tables[0];
            List<SelectListItem> itemSchool = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsSchool.Rows)
            {
                itemSchool.Add(new SelectListItem { Text = @dr["schooltype"].ToString(), Value = @dr["schooltype"].ToString() });
            }
            return itemSchool;
        }

        public static List<SelectListItem> GetSchoolAbbr()
        {           
            DataSet ds = GetAllSchoolType(); // passing Value to SchoolDB from model
            DataTable dsSchool = ds.Tables[0];
            List<SelectListItem> itemSchool = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsSchool.Rows)
            {
                itemSchool.Add(new SelectListItem { Text = @dr["schooltype"].ToString(), Value = @dr["abbr"].ToString() });
            }            
            return itemSchool;
        }

        public static List<SelectListItem> GetDistE()
        {
            DataSet dsDist = GetAllDistrict();
            // English
            List<SelectListItem> itemDist = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsDist.Tables[0].Rows)
            {
                itemDist.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() });
            }
            return itemDist;
        }
        public static List<SelectListItem> GetDistP()
        {
            DataSet dsDist = GetAllDistrict();
            // Punjabi
            List<SelectListItem> itemDistP = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsDist.Tables[0].Rows)
            {
                itemDistP.Add(new SelectListItem { Text = @dr["DISTNMP"].ToString(), Value = @dr["DIST"].ToString() });
            }
            return itemDistP;
        }
        public static List<SelectListItem> GetAllTehsil()
        {    
            DataSet ds = TehsilMaster(); // passing Value to SchoolDB from model
            DataTable dsTehsilSession = ds.Tables[0];
            List<SelectListItem> itemTeh = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsTehsilSession.Rows)
            {
                itemTeh.Add(new SelectListItem { Text = @dr["TEHSIL"].ToString(), Value = @dr["TCODE"].ToString() });


            }
            return itemTeh;
        }

        public static List<SelectListItem> GetAllTehsilP()
        {
            DataSet ds = TehsilMaster(); // passing Value to SchoolDB from model
            DataTable dsTehsilSession = ds.Tables[0];          
            List<SelectListItem> itemTeh = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsTehsilSession.Rows)
            {
                itemTeh.Add(new SelectListItem { Text = @dr["TEHSILP"].ToString(), Value = @dr["TCODE"].ToString() });


            }
            return itemTeh;
        }
        #endregion MASTER TABLE DATA LIST

        public static List<SelectListItem> GetAdminUser()
        {
            List<SelectListItem> adminUser = new List<SelectListItem>();
            DataSet dsUser = new AbstractLayer.AdminDB().GetAllAdminUser(0, "id like '%%'");
            foreach (System.Data.DataRow dr in dsUser.Tables[0].Rows)
            {
                adminUser.Add(new SelectListItem { Text = @dr["user"].ToString(), Value = @dr["id"].ToString() });
            }
            return adminUser;
        }

        public static DataSet CheckBankAllowByFeeCodeDate(int feecode)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CheckBankAllowByFeeCodeDate";
            cmd.Parameters.AddWithValue("@feecode", feecode);
            return db.ExecuteDataSet(cmd);
        }

   
       

        public static string GetAmountInWords(int amount)
        {
            string AmountInWords = "";
            DataSet result = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAmountInWords";
            cmd.Parameters.AddWithValue("@amount", amount);
            result =  db.ExecuteDataSet(cmd);
            if (result.Tables.Count > 0)
            {
                if (result.Tables[0].Rows.Count > 0)
                {
                    AmountInWords = result.Tables[0].Rows[0]["AmountInWords"].ToString();
                }
            }
            return AmountInWords;
        }

        public static DataSet GetAdminDetailsById(int adminid, int year)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAdminDetailsByIdSP";
            cmd.Parameters.AddWithValue("@adminid", adminid);
            cmd.Parameters.AddWithValue("@year", year);
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet GetActionOfSubMenu(int AdminId, string cont, string act)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetActionOfSubMenuSP";
            cmd.Parameters.AddWithValue("@AdminId", AdminId);
            cmd.Parameters.AddWithValue("@Controller", cont);
            cmd.Parameters.AddWithValue("@Action", act);
            return db.ExecuteDataSet(cmd);
        }





        #region Get All Session Year Admin

        public static DataTable SessionMasterSPAdmin()
        {
            DataSet result = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SessionMasterSPAdmin";
            //cmd.Parameters.AddWithValue("@AdminId", AdminId);
            //cmd.Parameters.AddWithValue("@Controller", cont);
            //cmd.Parameters.AddWithValue("@Action", act);
            result =  db.ExecuteDataSet(cmd);
            return result.Tables[0];
        }

    
        public static List<SelectListItem> GetSessionYearSchoolAdmin()
        {
            DataTable dsSession = SessionMasterSPAdmin(); // SessionMasterSPAdmin
            List<SelectListItem> itemSession = new List<SelectListItem>();
            // itemSession.Add(new SelectListItem { Text = "2017", Value = "2017" });
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["yearto"].ToString(), Value = @dr["yearto"].ToString() });
            }
            itemSession.Add(new SelectListItem { Text = "1969", Value = "1969" });
            return itemSession;
        }

        public static List<SelectListItem> GetSessionAdmin()
        {
            DataTable dsSession = SessionMasterSPAdmin(); // passing Value to SchoolDB from model
            List<SelectListItem> itemSession = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["YearFull"].ToString(), Value = @dr["YearFull"].ToString() });
            }
            return itemSession;
        }

        #endregion Get All Session Year Admin

        public static DataSet schooltypes(string schid)
        {          
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSchoolType";
            cmd.Parameters.AddWithValue("@SCHL", schid);
            return db.ExecuteDataSet(cmd);
        }


    
        //-----------------For SMS-----------------
     
        public static string gosms(string mobileno, string message)
        {
            StringBuilder xml = new StringBuilder();
            if (mobileno == "0000000000")
            {
                return "Not Valid";
            }
            else
            {
                string setmobileno = mobileno;

                string answer = "";
                string Apistatus = "";
                try
                {
                    int count = 0;
                    bool IsHindi;
                    string mobno = string.Empty;
                    string url = string.Empty;
                    String surl = string.Empty;
                    string sSendrnumber = string.Empty;
                    string Adddigit91 = "";
                    try
                    {
                        DataSet ds = new DataSet();
                        ds = getsmsSetup();
                        // RunProcedure("Udp_getActiveSmsSetUp", out ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            surl = ds.Tables[0].Rows[0]["URL"].ToString();
                            sSendrnumber = ds.Tables[0].Rows[0]["Sender"].ToString();
                            Adddigit91 = ds.Tables[0].Rows[0]["AddDigit91"].ToString();
                        }
                    }
                    catch (Exception ex) { }
                    int chkcount = 0;
                    int checksms = 0;
                    xml.Append("<root>");
                    string[] mob = mobileno.ToString().Trim().Split(',');

                    //********Start Changes of Hindi
                    IsHindi = IsUnicode(message);
                    if (IsHindi == true)
                    {
                        surl = surl.Insert(surl.Length, "&type=1");
                    }
                    //**********End

                    if (mob.Length > 0)
                    {
                        for (int ln = 0; ln < mob.Length; ln++)
                        {
                            checksms = 1;
                            chkcount = chkcount + 1;
                            mobileno = mob[ln].Trim();//dr["MobileNo"].ToString();
                            int j = mobileno.Length;
                            int counts = mobileno.Length;
                            int k = j - 10;
                            if (j < 10)
                            {
                                mobileno = "";
                            }
                            if (j == 10)
                            {
                                if (Adddigit91 == "Y")
                                    mobno = "91" + "" + mobileno;
                                else
                                    mobno = mobileno;
                            }
                            if (j > 10)
                            {
                                mobileno = mobileno.Remove(0, k);
                                if (Adddigit91 == "Y")
                                    mobno = "91" + "" + mobileno;
                                else
                                    mobno = mobileno;
                            }

                            //  message = message.Replace("&", "%26");
                            if (mobileno != "&nbsp;" && mobileno != "")
                            {
                                xml.Append("<SMS>");
                                xml.Append("<subject>" + message + "</subject>");
                                xml.Append("<Mobile_No>" + mobileno + "</Mobile_No>");
                                xml.Append("<Sender>" + sSendrnumber + "</Sender>");
                                xml.Append("<Update_Datetime>" + System.DateTime.Now + "</Update_Datetime>");
                                #region Genrate URL
                                url = surl;
                                url = url.Replace("@MN@", mobno);
                                url = url.Replace("@MText@", message);
                                string status = readHtmlPage(url);
                                Apistatus = status;
                                #endregion
                                count = count + 1;
                                int length = message.Length;
                                int divlength = length / 157;
                                decimal remilngth = length % 157;
                                if (divlength == 0)
                                {
                                    length = 1;
                                }
                                else
                                {
                                    length = divlength;
                                    if (remilngth != 0)
                                    {
                                        length = length + 1;
                                    }
                                }
                                xml.Append("</SMS>");
                                int legthxml = xml.Length;
                                if (legthxml > 7000 && legthxml < 7950)
                                {
                                    xml.Append("</root>");
                                    xml = new StringBuilder();
                                    xml.Append("<root>");
                                }
                            }

                        }
                    }

                    xml.Append("</root>");
                    try
                    {

                        if (count == 0 && chkcount != 0)
                        {
                            answer = "Applicant mobile no. not available.";
                        }
                        if (count == 0 && chkcount != 0 && mobileno == "")
                        {
                            answer = "Applicant mobile no. not valid.";
                        }
                        if (count > 0)
                        {
                            answer = count + " SMS send successfully to your Applicant.";
                        }
                    }
                    catch (Exception)
                    {
                        answer = "Sorry ! your information is not send, please contact administrator";
                    }
                }
                catch (Exception e)
                {
                    answer = "Sorry ! your information is not send, please contact administrator";
                }

                try
                {
                    string result;
                    string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                    string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                    Database db = DatabaseFactory.CreateDatabase();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_sms_history";
                    cmd.Parameters.AddWithValue("@mobile", setmobileno);
                    cmd.Parameters.AddWithValue("@sms", message);
                    cmd.Parameters.AddWithValue("@status", answer);
                    cmd.Parameters.AddWithValue("@Apistatus", Apistatus);
                    cmd.Parameters.AddWithValue("@Ip", myIP);
                    result = cmd.ExecuteNonQuery().ToString();
                }
                catch (Exception ex)
                {
                    //throw;
                }
                return answer;
            }
        }

        public static string gosmsForPseb(string mobileno, string message,string tempId)
        {
            StringBuilder xml = new StringBuilder();
            if (mobileno == "0000000000")
            {
                return "Not Valid";
            }
            else
            {
                string setmobileno = mobileno;

                string answer = "";
                string Apistatus = "";
                try
                {
                    int count = 0;
                    bool IsHindi;
                    string mobno = string.Empty;
                    string url = string.Empty;
                    String surl = string.Empty;
                    string sSendrnumber = string.Empty;
                    string Adddigit91 = "";
                    try
                    {
                        DataSet ds = new DataSet();
                        ds = getsmsSetup();
                        // RunProcedure("Udp_getActiveSmsSetUp", out ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            surl = ds.Tables[0].Rows[0]["URL"].ToString();
                            sSendrnumber = ds.Tables[0].Rows[0]["Sender"].ToString();
                            Adddigit91 = ds.Tables[0].Rows[0]["AddDigit91"].ToString();
                        }
                    }
                    catch (Exception ex) { }
                    int chkcount = 0;
                    int checksms = 0;
                    xml.Append("<root>");
                    string[] mob = mobileno.ToString().Trim().Split(',');

                    //********Start Changes of Hindi
                    IsHindi = IsUnicode(message);
                    if (IsHindi == true)
                    {
                        surl = surl.Insert(surl.Length, "&type=1");
                    }
                    //**********End

                    if (mob.Length > 0)
                    {
                        for (int ln = 0; ln < mob.Length; ln++)
                        {
                            checksms = 1;
                            chkcount = chkcount + 1;
                            mobileno = mob[ln].Trim();//dr["MobileNo"].ToString();
                            int j = mobileno.Length;
                            int counts = mobileno.Length;
                            int k = j - 10;
                            if (j < 10)
                            {
                                mobileno = "";
                            }
                            if (j == 10)
                            {
                                if (Adddigit91 == "Y")
                                    mobno = "91" + "" + mobileno;
                                else
                                    mobno = mobileno;
                            }
                            if (j > 10)
                            {
                                mobileno = mobileno.Remove(0, k);
                                if (Adddigit91 == "Y")
                                    mobno = "91" + "" + mobileno;
                                else
                                    mobno = mobileno;
                            }

                            //  message = message.Replace("&", "%26");
                            if (mobileno != "&nbsp;" && mobileno != "")
                            {
                                xml.Append("<SMS>");
                                xml.Append("<subject>" + message + "</subject>");
                                xml.Append("<Mobile_No>" + mobileno + "</Mobile_No>");
                                xml.Append("<Sender>" + sSendrnumber + "</Sender>");
                                xml.Append("<Update_Datetime>" + System.DateTime.Now + "</Update_Datetime>");
                                #region Genrate URL
                                url = surl;
                                url = url.Replace("@MN", mobno);
                                url = url.Replace("@MText", message);
                                url = url.Replace("@tid", tempId);
								//string status = readHtmlPage(url);
								//Apistatus = status;
								try
								{
									using (WebClient webClient = new WebClient())
									{
										Uri StringToUri = new Uri(string.Format(url, message));
										webClient.DownloadData(StringToUri);
									}
								}
								catch (Exception ex)
								{

								}
								#endregion
								count = count + 1;
                                int length = message.Length;
                                int divlength = length / 157;
                                decimal remilngth = length % 157;
                                if (divlength == 0)
                                {
                                    length = 1;
                                }
                                else
                                {
                                    length = divlength;
                                    if (remilngth != 0)
                                    {
                                        length = length + 1;
                                    }
                                }
                                xml.Append("</SMS>");
                                int legthxml = xml.Length;
                                if (legthxml > 7000 && legthxml < 7950)
                                {
                                    xml.Append("</root>");
                                    xml = new StringBuilder();
                                    xml.Append("<root>");
                                }
                            }

                        }
                    }

                    xml.Append("</root>");
                    try
                    {

                        if (count == 0 && chkcount != 0)
                        {
                            answer = "Applicant mobile no. not available.";
                        }
                        if (count == 0 && chkcount != 0 && mobileno == "")
                        {
                            answer = "Applicant mobile no. not valid.";
                        }
                        if (count > 0)
                        {
                            answer = count + " SMS send successfully to your Applicant.";
                        }
                    }
                    catch (Exception)
                    {
                        answer = "Sorry ! your information is not send, please contact administrator";
                    }
                }
                catch (Exception e)
                {
                    answer = "Sorry ! your information is not send, please contact administrator";
                }

                try
                {
                    string result;
                    string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                    string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                    Database db = DatabaseFactory.CreateDatabase();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_sms_history";
                    cmd.Parameters.AddWithValue("@mobile", setmobileno);
                    cmd.Parameters.AddWithValue("@sms", message);
                    cmd.Parameters.AddWithValue("@status", answer);
                    cmd.Parameters.AddWithValue("@Apistatus", Apistatus);
                    cmd.Parameters.AddWithValue("@Ip", myIP);
                    result = cmd.ExecuteNonQuery().ToString();
                }
                catch (Exception ex)
                {
                    //throw;
                }
                return answer;
            }
        }
        public static String readHtmlPage(string url)
        {
            String result = "";
            String sResult = "";
            String strPost = "x=1&y=2&z=YouPostedOk";
            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            // HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("");
            objRequest.Method = "POST";
            objRequest.ContentLength = strPost.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(strPost);
            }
            catch (Exception e)
            {
                sResult = "0";
                return sResult;
            }
            finally
            {
                myWriter.Dispose();
                myWriter.Close();
            }
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader sr =
            new StreamReader(objResponse.GetResponseStream()))
            {

                result = sr.ReadToEnd();
                sr.Close();
            }

            return result;
        }
        public static bool IsUnicode(string input)
        {
            var asciiBytesCount = Encoding.ASCII.GetByteCount(input);
            var unicodBytesCount = Encoding.UTF8.GetByteCount(input);
            return asciiBytesCount != unicodBytesCount;
        }

        //----------------End SMS---------------
        public static bool mail(string subject, string body, string to)
        {
            string Status = "";
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                string mail_from = ("noreply@pseb.ac.in");
                mail.From = new MailAddress(mail_from, "pseb.ac.in");
                if (to == "")
                {

                }
                else
                {
                    mail.To.Add(to);
                }

                string[] multi = to.Split(',');
                foreach (string MultiMail in multi)
                {
                    mail.To.Add(new MailAddress(MultiMail));
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                //smtp.Host = "mail.smtp2go.com";
                //smtp.Port = 2525;
                smtp.Host = "smtp.sendgrid.net";
                smtp.Port = 587;
                //smtp.Credentials = new System.Net.NetworkCredential("indiaresultspep", "LxnTMoQgKN2023");
                smtp.Credentials = new System.Net.NetworkCredential("apikey", "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");

                try
                {
                    smtp.Send(mail);
                    mail.Dispose();
                    Status = "true";
                    try
                    {
                        int result;
                        string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                        string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                        Database db = DatabaseFactory.CreateDatabase();
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "sp_email_history";
                        cmd.Parameters.AddWithValue("@email", to);
                        cmd.Parameters.AddWithValue("@text", body);
                        cmd.Parameters.AddWithValue("@status", Status);
                        cmd.Parameters.AddWithValue("@subject", subject);
                        cmd.Parameters.AddWithValue("@IP", myIP);
                         result = db.ExecuteNonQuery(cmd);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    Status = "false";
                    return false;
                }

            }
            catch (Exception ex)
            {
                Status = "false";
                return false;
            }



        }


        public static bool mailwithattachment(string subject, string body, string to, string filepath)
        {
            string Status = "";
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();             
                string from_name = "rohit.nanda@ethical.in";
                mail.From = new MailAddress(from_name, "PSEB");
                if (to == "")
                {

                }
                else
                {
                    mail.To.Add(to);
                }


                string[] multi = to.Split(',');
                foreach (string MultiMail in multi)
                {
                    mail.To.Add(new MailAddress(MultiMail));
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
               
                System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment("E:/Difference in Admission and Exam Fees of Open School.pdf");
                attachment = new System.Net.Mail.Attachment(filepath);
                mail.Attachments.Add(attachment);

                SmtpClient smtp = new SmtpClient();               
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("rohit.nanda@ethical.in", "rnanda@26");
                // smtp.Credentials = new System.Net.NetworkCredential("helpdeskPsebJunior@gmail.com", "helpdesk@26");
                try
                {
                    smtp.Send(mail);
                    mail.Dispose();
                    Status = "true";

                    try
                    {
                        int result;
                        string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                        string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                        Database db = DatabaseFactory.CreateDatabase();
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "sp_email_history";
                        cmd.Parameters.AddWithValue("@email", to);
                        cmd.Parameters.AddWithValue("@text", body);
                        cmd.Parameters.AddWithValue("@status", Status);
                        cmd.Parameters.AddWithValue("@subject", subject);
                        cmd.Parameters.AddWithValue("@IP", myIP);
                         result = db.ExecuteNonQuery(cmd);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return true;
                    }
                    //  return true;
                }
                catch (Exception ex)
                {
                    Status = "false";
                    return false;
                }

            }
            catch (Exception ex)
            {
                Status = "false";
                return false;
            }

        }


        public static DataSet getsmsSetup()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Udp_getActiveSmsSetUp";
           // cmd.Parameters.AddWithValue("@schl", schl);
            return db.ExecuteDataSet(cmd);
        }

       

        public static DataSet GetBankNameList(int type, string Bank, string IfscCode)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetBankNameListSP";
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@Bank", Bank);
            cmd.Parameters.AddWithValue("@IfscCode", IfscCode);
            return db.ExecuteDataSet(cmd);
        }
      
        public static List<SelectListItem> GetWritter()
        {
            List<SelectListItem> WantWritter = new List<SelectListItem>();
            //WantWritter.Add(new SelectListItem { Text = "---SELECT--", Value = "0" });
            WantWritter.Add(new SelectListItem { Text = "NO", Value = "NO" });
            WantWritter.Add(new SelectListItem { Text = "SCRIBE", Value = "SCRIBE" });
            WantWritter.Add(new SelectListItem { Text = "READER", Value = "READER" });
            WantWritter.Add(new SelectListItem { Text = "LAB ASSISTANT", Value = "LAB ASSISTANT" });
            return WantWritter;
        }
        public static List<SelectListItem> GetBankList()
        {
            List<SelectListItem> BankList = new List<SelectListItem>();
            //BankList.Add(new SelectListItem { Text = "State Bank of Patiala", Value = "101" });
            //BankList.Add(new SelectListItem { Text = "Punjab National Bank", Value = "102" });
            BankList.Add(new SelectListItem { Text = "HDFC Bank", Value = "301" });
            BankList.Add(new SelectListItem { Text = "Punjab And Sind Bank", Value = "302" });
            BankList.Add(new SelectListItem { Text = "PSEB HOD", Value = "103" });
            return BankList;
        }
        public static List<SelectListItem> GetBoardList2()
        {
            List<SelectListItem> BoardN2List = new List<SelectListItem>();
            BoardN2List.Add(new SelectListItem { Text = "---Select Board--", Value = "0" });
            BoardN2List.Add(new SelectListItem { Text = "CBSE BOARD", Value = "CBSE BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "P.S.E.B BOARD", Value = "P.S.E.B BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "I.C.S.E BOARD", Value = "ICSE BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "HARYANA BOARD", Value = "HARYANA BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "HIMACHAL BOARD", Value = "HIMACHAL BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "J&K BOARD", Value = "J&K BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "RAJASTHAN BOARD", Value = "RAJASTHAN BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "OTHER BOARD", Value = "OTHER BOARD" });
            return BoardN2List;
        }
        public static List<SelectListItem> GetBoardList()
        {
            List<SelectListItem> BoardN2List = new List<SelectListItem>();
            BoardN2List.Add(new SelectListItem { Text = "---Select Board--", Value = "0" });
            BoardN2List.Add(new SelectListItem { Text = "CBSE BOARD", Value = "CBSE BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "P.S.E.B BOARD", Value = "P.S.E.B BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "I.C.S.E BOARD", Value = "ICSE BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "HARYANA BOARD", Value = "HARYANA BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "HIMACHAL BOARD", Value = "HIMACHAL BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "J&K BOARD", Value = "J&K BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "RAJASTHAN BOARD", Value = "RAJASTHAN BOARD" });
            BoardN2List.Add(new SelectListItem { Text = "OTHER BOARD", Value = "OTHER BOARD" });
            return BoardN2List;
        }
        public static List<SelectListItem> GetGroupMedium()
        {
            List<SelectListItem> TM = new List<SelectListItem>();
           // TM.Add(new SelectListItem { Text = "Medium", Value = "Medium" });
            TM.Add(new SelectListItem { Text = "PUNJABI", Value = "PUNJABI" });
            TM.Add(new SelectListItem { Text = "HINDI", Value = "HINDI" });
            TM.Add(new SelectListItem { Text = "ENGLISH", Value = "ENGLISH" });
            return TM;
        }
        public static List<SelectListItem> GetMediumAll()
        {
            List<SelectListItem> BM = new List<SelectListItem>();
           // BM.Add(new SelectListItem { Text = "Medium", Value = "Medium" });
            BM.Add(new SelectListItem { Text = "PUNJABI", Value = "PUNJABI" });
            BM.Add(new SelectListItem { Text = "HINDI", Value = "HINDI" });
            BM.Add(new SelectListItem { Text = "ENGLISH", Value = "ENGLISH" });
            BM.Add(new SelectListItem { Text = "SANSKRIT", Value = "SANSKRIT" });
            BM.Add(new SelectListItem { Text = "URDU", Value = "URDU" });
            BM.Add(new SelectListItem { Text = "PERSIAN", Value = "PERSIAN" });
            BM.Add(new SelectListItem { Text = "ARABIC", Value = "ARABIC" });
            BM.Add(new SelectListItem { Text = "FRENCH", Value = "FRENCH" });
            BM.Add(new SelectListItem { Text = "GERMAN", Value = "GERMAN" });
            BM.Add(new SelectListItem { Text = "RUSSIAN", Value = "RUSSIAN" });

            return BM;
        }
        public static List<SelectListItem> GroupName()
        {
            List<SelectListItem> GroupList = new List<SelectListItem>();
            GroupList.Add(new SelectListItem { Text = "--Select--", Value = "0" });
            GroupList.Add(new SelectListItem { Text = "SCIENCE", Value = "SCIENCE" });
            GroupList.Add(new SelectListItem { Text = "COMMERCE", Value = "COMMERCE" });
            GroupList.Add(new SelectListItem { Text = "HUMANITIES", Value = "HUMANITIES" });
            GroupList.Add(new SelectListItem { Text = "VOCATIONAL", Value = "VOCATIONAL" });
            GroupList.Add(new SelectListItem { Text = "AGRICULTURE", Value = "AGRICULTURE" });

            return GroupList;
        }
   

        public static string getPunjabiName(string text)
        {
            int i;
            //String en = Request.QueryString["id"].ToString();
            string en = text;
            char[] seps = { ' ' };
            string[] en1 = en.Split(seps);
            string pn = "";

            for (i = 0; i < en1.Length; i++)
            {
                // string mqry = "Select  * From dictonary where name='" + en1[i].ToString() + "'";  //t2
                //  DataSet ds = SqlHelper.ExecuteDataset(obj.getconnectionstring, CommandType.Text, mqry);
                DataTable ds = GetPunjabiNameSP(en1[i].ToString());
                if (ds.Rows.Count > 0)
                {
                    if (i == 0)
                    {
                        pn += ds.Rows[0]["pnbname"].ToString().Trim();
                    }
                    else
                    {
                        pn += " " + ds.Rows[0]["pnbname"].ToString().Trim();
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        pn += en1[i].ToString();
                    }
                    else
                    {
                        pn += " " + en1[i].ToString();
                    }
                }
            }
            //txt_candnm_pun.Text = pn;
            // Response.Write(pn);
            return pn;
        }

        public static DataTable GetPunjabiNameSP(string name)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetPunjabiNameSP";
            cmd.Parameters.AddWithValue("@name", name);
            ds =  db.ExecuteDataSet(cmd);
            return ds.Tables[0];
        }

        //Commmo Region
        public static List<SelectListItem> GetMonthNameNumber()
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();
            MonthList.Add(new SelectListItem { Text = "Jan", Value = "01" });
            MonthList.Add(new SelectListItem { Text = "Feb", Value = "02" });
            MonthList.Add(new SelectListItem { Text = "Mar", Value = "03" });
            MonthList.Add(new SelectListItem { Text = "April", Value = "04" });
            MonthList.Add(new SelectListItem { Text = "May", Value = "05" });
            MonthList.Add(new SelectListItem { Text = "Jun", Value = "06" });
            MonthList.Add(new SelectListItem { Text = "July", Value = "07" });
            MonthList.Add(new SelectListItem { Text = "Aug", Value = "08" });
            MonthList.Add(new SelectListItem { Text = "Sept", Value = "09" });
            MonthList.Add(new SelectListItem { Text = "Oct", Value = "10" });
            MonthList.Add(new SelectListItem { Text = "Nov", Value = "11" });
            MonthList.Add(new SelectListItem { Text = "Dec", Value = "12" });
            return MonthList;
        }

        public static List<SelectListItem> GetMonth()
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();
            MonthList.Add(new SelectListItem { Text = "--Select Month--", Value = "0" });
            MonthList.Add(new SelectListItem { Text = "Jan", Value = "Jan" });
            MonthList.Add(new SelectListItem { Text = "Feb", Value = "Feb" });
            MonthList.Add(new SelectListItem { Text = "Mar", Value = "Mar" });
            MonthList.Add(new SelectListItem { Text = "April", Value = "April" });
            MonthList.Add(new SelectListItem { Text = "May", Value = "May" });
            MonthList.Add(new SelectListItem { Text = "Jun", Value = "Jun" });
            MonthList.Add(new SelectListItem { Text = "July", Value = "July" });
            MonthList.Add(new SelectListItem { Text = "Aug", Value = "Aug" });
            MonthList.Add(new SelectListItem { Text = "Sept", Value = "Sept" });
            MonthList.Add(new SelectListItem { Text = "Oct", Value = "Oct" });
            MonthList.Add(new SelectListItem { Text = "Nov", Value = "Nov" });
            MonthList.Add(new SelectListItem { Text = "Dec", Value = "Dec" });
            return MonthList;
        }
        public static List<SelectListItem> GetCaste()
        {
            List<SelectListItem> CastList = new List<SelectListItem>();           
                CastList.Add(new SelectListItem { Text = "General", Value = "General" });
                CastList.Add(new SelectListItem { Text = "BC", Value = "BC" });
                CastList.Add(new SelectListItem { Text = "OBC", Value = "OBC" });
                CastList.Add(new SelectListItem { Text = "SC", Value = "SC" });
                CastList.Add(new SelectListItem { Text = "ST", Value = "ST" });
            return CastList;
        }


        public static DataTable GetDA_DB()
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetDA_DB_SP";            
            ds = db.ExecuteDataSet(cmd);
            return ds.Tables[0];
        }

        public static List<SelectListItem> GetDA()
        {
            DataTable dsGetDA_DB = GetDA_DB();
            List<SelectListItem> DList = new List<SelectListItem>();
            foreach (DataRow dr in dsGetDA_DB.Rows)
            {
                DList.Add(new SelectListItem { Text = @dr["dacat"].ToString(), Value = @dr["dacat"].ToString() });
            }
            return DList;
        }
        public static List<SelectListItem> GetCategory()
        {
            List<SelectListItem> catgilist = new List<SelectListItem>();
            catgilist.Add(new SelectListItem { Text = "8th Passed", Value = "8th Passed" });
            catgilist.Add(new SelectListItem { Text = "9th Failed", Value = "9th Failed" });
            return catgilist;
        }

     
       
        public static List<SelectListItem> GetBoard()
        {
            List<SelectListItem> BoardList = new List<SelectListItem>();
            BoardList.Add(new SelectListItem { Text = "Board", Value = "Board" });
            BoardList.Add(new SelectListItem { Text = "School", Value = "School" });
            return BoardList;
        }
        public static List<SelectListItem> GetReligion()
        {
            List<SelectListItem> Relist = new List<SelectListItem>();
            Relist.Add(new SelectListItem { Text = "Hindu", Value = "Hindu" });
            Relist.Add(new SelectListItem { Text = "Muslim", Value = "Muslim" });
            Relist.Add(new SelectListItem { Text = "Sikh", Value = "Sikh" });
            Relist.Add(new SelectListItem { Text = "Christian", Value = "Christian" });
            Relist.Add(new SelectListItem { Text = "Others", Value = "Others" });
            return Relist;
        }
        public static List<SelectListItem> GetSection()
        {
            List<SelectListItem> itemSection = new List<SelectListItem>();
            itemSection.Add(new SelectListItem { Text = "Select", Value = "0" });
            itemSection.Add(new SelectListItem { Text = "A", Value = "A" });
            itemSection.Add(new SelectListItem { Text = "B", Value = "B" });
            itemSection.Add(new SelectListItem { Text = "C", Value = "C" });
            itemSection.Add(new SelectListItem { Text = "D", Value = "D" });
            itemSection.Add(new SelectListItem { Text = "E", Value = "E" });
            itemSection.Add(new SelectListItem { Text = "F", Value = "F" });
            itemSection.Add(new SelectListItem { Text = "G", Value = "G" });
            itemSection.Add(new SelectListItem { Text = "H", Value = "H" });
            itemSection.Add(new SelectListItem { Text = "I", Value = "I" });
            itemSection.Add(new SelectListItem { Text = "J", Value = "J" });
            itemSection.Add(new SelectListItem { Text = "K", Value = "K" });
            itemSection.Add(new SelectListItem { Text = "L", Value = "L" });
            itemSection.Add(new SelectListItem { Text = "M", Value = "M" });
            itemSection.Add(new SelectListItem { Text = "N", Value = "N" });
            itemSection.Add(new SelectListItem { Text = "O", Value = "O" });
            itemSection.Add(new SelectListItem { Text = "P", Value = "P" });
            itemSection.Add(new SelectListItem { Text = "Q", Value = "Q" });
            itemSection.Add(new SelectListItem { Text = "R", Value = "R" });
            itemSection.Add(new SelectListItem { Text = "S", Value = "S" });
            itemSection.Add(new SelectListItem { Text = "T", Value = "T" });
            itemSection.Add(new SelectListItem { Text = "U", Value = "U" });
            itemSection.Add(new SelectListItem { Text = "V", Value = "V" });
            itemSection.Add(new SelectListItem { Text = "W", Value = "W" });
            itemSection.Add(new SelectListItem { Text = "X", Value = "X" });
            itemSection.Add(new SelectListItem { Text = "Y", Value = "Y" });
            itemSection.Add(new SelectListItem { Text = "Z", Value = "Z" });
            return itemSection;
        }
        public static List<SelectListItem> GetArea()
        {
            List<SelectListItem> itemArea = new List<SelectListItem>();
            itemArea.Add(new SelectListItem { Text = "URBAN", Value = "U" });
            itemArea.Add(new SelectListItem { Text = "RURAL", Value = "R" });
            return itemArea;
        }
        public static List<SelectListItem> GetYesNo()
        {
            List<SelectListItem> itemYesNo = new List<SelectListItem>();
            itemYesNo.Add(new SelectListItem { Text = "NO", Value = "N" });
            itemYesNo.Add(new SelectListItem { Text = "YES", Value = "Y" });
            return itemYesNo;
        }

        public static List<SelectListItem> GetYesNoText()
        {
            List<SelectListItem> itemYesNo = new List<SelectListItem>();
            itemYesNo.Add(new SelectListItem { Text = "NO", Value = "NO" });
            itemYesNo.Add(new SelectListItem { Text = "YES", Value = "YES" });
            return itemYesNo;
        }
        public static List<SelectListItem> GetStatus()
        {
            List<SelectListItem> itemStatus = new List<SelectListItem>();
            itemStatus.Add(new SelectListItem { Text = "DONE", Value = "DONE" });
            itemStatus.Add(new SelectListItem { Text = "CANCEL", Value = "CANCEL" });
            return itemStatus;
        }

        public static List<SelectListItem> GetSessionSingle()
        {
            List<SelectListItem> itemSession = new List<SelectListItem>();
            itemSession.Add(new SelectListItem { Text = "2021", Value = "2021" });
            itemSession.Add(new SelectListItem { Text = "2022", Value = "2022" });
            itemSession.Add(new SelectListItem { Text = "2023", Value = "2023" });
            return itemSession;
        }

        public static List<SelectListItem> GetSession()
        {
           List<SelectListItem> itemSession = new List<SelectListItem>();
            itemSession.Add(new SelectListItem { Text = "2023-2024", Value = "2023-2024" });
            return itemSession;
        }


        public static List<SelectListItem> GetSessionAll()
        {
            DataTable dsSession = SessionMaster(); // passing Value to SchoolDB from model
            List<SelectListItem> itemSession = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["YearFull"].ToString(), Value = @dr["YearFull"].ToString() });
            }
            return itemSession;
        }

        public static List<SelectListItem> GetSessionYear()
        {
            DataTable dsSession = SessionMaster(); // passing Value to SchoolDB from model
            List<SelectListItem> itemSession = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["yearfrom"].ToString(), Value = @dr["yearfrom"].ToString() });
            }
            return itemSession;
        }

        public static List<SelectListItem> GetSessionYearSchool()
        {
            DataTable dsSession = SessionMaster(); // passing Value to SchoolDB from model
            List<SelectListItem> itemSession = new List<SelectListItem>();
            itemSession.Add(new SelectListItem { Text = "2018", Value = "2018" });
            foreach (System.Data.DataRow dr in dsSession.Rows)
            {
                itemSession.Add(new SelectListItem { Text = @dr["yearfrom"].ToString(), Value = @dr["yearfrom"].ToString() });
            }
            return itemSession;
        }

        public static string GetSubjectName(int id)
        {
            string result = "";
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetSubjectName";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.Add("@subjectname", SqlDbType.NVarChar, 900).Direction = ParameterDirection.Output;
           // result = db.ExecuteDataSet(cmd);
            result = cmd.ExecuteNonQuery().ToString();
            result = Convert.ToString(cmd.Parameters["@subjectname"].Value);
            return result;
        }



        public static DataSet Fll_Subject_Details(int id)
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pro_GetAllSubjectById";
            cmd.Parameters.AddWithValue("@id", id);
            return db.ExecuteDataSet(cmd);
        }
               
      
        public static List<SelectListItem> GetSubject(int id)
        {
            DataSet dsDist = Fll_Subject_Details(id);
            // English
            List<SelectListItem> itemDist = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsDist.Tables[0].Rows)
            {
                itemDist.Add(new SelectListItem { Text = @dr["subjectname"].ToString(), Value = @dr["id"].ToString() });
            }
            return itemDist;
        }

        public static List<SelectListItem> GetCadre()
        {
            DataSet dsDist = Fll_Cadre_Details();
            // English
            List<SelectListItem> itemDist = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsDist.Tables[0].Rows)
            {
                itemDist.Add(new SelectListItem { Text = @dr["cadrename"].ToString(), Value = @dr["id"].ToString() });
            }
            return itemDist;
        }


        public static DataSet Fll_Cadre_Details()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pro_GetAllCadre";
            return db.ExecuteDataSet(cmd);
        }

        public static DataSet Fll_FeeCat_Details()
        {
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "pro_GetAllFeeCat";
            return db.ExecuteDataSet(cmd);
        }

        public static List<SelectListItem> GetFeeCat()
        {
            DataSet dsDist = Fll_FeeCat_Details();
            // English
            List<SelectListItem> itemDist = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in dsDist.Tables[0].Rows)
            {
                itemDist.Add(new SelectListItem { Text = @dr["FEECAT"].ToString().Trim(), Value = @dr["FEECAT"].ToString().Trim() });
            }
            return itemDist;
        }



        public static DataTable GetAssignSubjectBySchoolandStudent(string Schl, string stdsub)
        {
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetAssignSubjectBySchoolandStudent_Sp";
            cmd.Parameters.AddWithValue("@Schl", Schl);
            cmd.Parameters.AddWithValue("@stdsub", stdsub);
            ds =  db.ExecuteDataSet(cmd);
            return ds.Tables[0];
        }



        public static void ReGenerateChallaanByIdSPAdmin(float lumsumfine, string lumsumremarks, string challanid, float fee, out int outstatus)
        {
            int result;
            DataSet ds = new DataSet();
            Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "ReGenerateChallaanByIdSPAdmin";
            cmd.Parameters.AddWithValue("@CHALLANID", challanid);
            cmd.Parameters.AddWithValue("@type", "Admin");
            cmd.Parameters.AddWithValue("@lumsumfine", lumsumfine);
            cmd.Parameters.AddWithValue("@FEE", fee);
            cmd.Parameters.AddWithValue("@lumsumremarks", lumsumremarks);
            cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;           
             result = db.ExecuteNonQuery(cmd);
            outstatus = (int)cmd.Parameters["@OutStatus"].Value;
            //ds = db.ExecuteDataSet(cmd);
            //return ds.Tables[0];
        }



        public static int ReGenerateChallaanByIdSPAdminNew(float lumsumfine, string lumsumremarks, string challanid, out string OutError, DateTime? date = null, float fee = 0, DateTime? dateV = null)
        {
            int result;
            try
            {
                
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ReGenerateChallaanByIdPSEB";
                cmd.Parameters.AddWithValue("@CHALLANID", challanid);
                cmd.Parameters.AddWithValue("@type", "Admin");
                cmd.Parameters.AddWithValue("@lumsumfine", lumsumfine);
                cmd.Parameters.AddWithValue("@FEE", fee);
                cmd.Parameters.AddWithValue("@lumsumremarks", lumsumremarks);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@dateV", dateV);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                ds = db.ExecuteDataSet(cmd);
                result = Convert.ToInt32(cmd.Parameters["@OutStatus"].Value);
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



        public static string ChallanDetailsCancel(string cancelremarks, string challanid, out string outstatus, int AdminId, string Type)
        {
            int result;
            try
            {
                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ChallanDetailsCancelSP";
                cmd.Parameters.AddWithValue("@CHALLANID", challanid);
                cmd.Parameters.AddWithValue("@AdminId", AdminId);
                cmd.Parameters.AddWithValue("@cancelremarks", cancelremarks);
                cmd.Parameters.AddWithValue("@Type", Type);
                cmd.Parameters.Add("@OutStatus", SqlDbType.VarChar, 5).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd);
                outstatus = Convert.ToString(cmd.Parameters["@OutStatus"].Value);
              
            }
            catch (Exception ex)
            {
                outstatus = "-2";               
            }
            return outstatus;           
        }


     
        public static List<SelectListItem> GetAllPSEBCLASS()
        {
            List<SelectListItem> iList = new List<SelectListItem>();
            iList.Add(new SelectListItem { Text = "None", Value = "0" });
            iList.Add(new SelectListItem { Text = "5th", Value = "5" });
            iList.Add(new SelectListItem { Text = "8th", Value = "8" });
            iList.Add(new SelectListItem { Text = "9th", Value = "9" });
            iList.Add(new SelectListItem { Text = "10th", Value = "10" });
            iList.Add(new SelectListItem { Text = "11th", Value = "11" });
            iList.Add(new SelectListItem { Text = "12th", Value = "12" });
            return iList;
        }

        public static List<SelectListItem> GetAllPSEBCLASS_5to12()
        {
            List<SelectListItem> iList = new List<SelectListItem>();
            iList.Add(new SelectListItem { Text = "None", Value = "0" });
            iList.Add(new SelectListItem { Text = "5th Class Label", Value = "5" });
            iList.Add(new SelectListItem { Text = "8th Class Label", Value = "8" });
            iList.Add(new SelectListItem { Text = "9th Class Label", Value = "9" });
            iList.Add(new SelectListItem { Text = "10th Class Label", Value = "10" });
            iList.Add(new SelectListItem { Text = "11th Class Label", Value = "11" });
            iList.Add(new SelectListItem { Text = "12th Class Label", Value = "12" });
            return iList;
        }

        #region New DBSet

        public static ClassFifthInitilizeListModel ClassFifthInitilizeList()
        {
            DataSet ds = new DataSet();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            List<SelectListItem> itemSession = new List<SelectListItem>();
            List<SelectListItem> DList = new List<SelectListItem>();
            List<SelectListItem> itemDist = new List<SelectListItem>();
            try
            {
        
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClassFifthInitilizeListSP";
                ds = db.ExecuteDataSet(cmd);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        
                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            itemSession.Add(new SelectListItem { Text = @dr["yearfrom"].ToString(), Value = @dr["yearfrom"].ToString() });
                        }
                        classFifthInitilizeListModel.SessionYearList = itemSession.ToList();
                    }
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                       
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            DList.Add(new SelectListItem { Text = @dr["dacat"].ToString(), Value = @dr["dacat"].ToString() });
                        }
                        classFifthInitilizeListModel.DAList = DList.ToList();
                    }
                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        
                        foreach (System.Data.DataRow dr in ds.Tables[2].Rows)
                        {
                            itemDist.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() });
                        }
                        classFifthInitilizeListModel.MyDist = itemDist.ToList();
                    }

                }
             
            }
            catch (Exception)
            {
                classFifthInitilizeListModel.SessionYearList = itemSession;
                classFifthInitilizeListModel.DAList = DList;
                classFifthInitilizeListModel.MyDist = itemDist;

            }
            return classFifthInitilizeListModel;
        }



        public static ClassMiddleInitilizeListModel ClassMiddleInitilizeList()
        {
            DataSet ds = new DataSet();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            List<SelectListItem> itemSession = new List<SelectListItem>();
            List<SelectListItem> DList = new List<SelectListItem>();
            List<SelectListItem> itemDist = new List<SelectListItem>();
            List<SelectListItem> itemElectiveSub = new List<SelectListItem>();
            try
            {

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClassMiddleInitilizeListSP";
                ds = db.ExecuteDataSet(cmd);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {

                        foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                        {
                            itemSession.Add(new SelectListItem { Text = @dr["yearfrom"].ToString(), Value = @dr["yearfrom"].ToString() });
                        }
                        ClassMiddleInitilizeListModel.SessionYearList = itemSession.ToList();
                    }
                    if (ds.Tables[1].Rows.Count > 0)
                    {

                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            DList.Add(new SelectListItem { Text = @dr["dacat"].ToString(), Value = @dr["dacat"].ToString() });
                        }
                        ClassMiddleInitilizeListModel.DAList = DList.ToList();
                    }
                    if (ds.Tables[2].Rows.Count > 0)
                    {

                        foreach (System.Data.DataRow dr in ds.Tables[2].Rows)
                        {
                            itemDist.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() });
                        }
                        ClassMiddleInitilizeListModel.MyDist = itemDist.ToList();
                    }
                    if (ds.Tables[3].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in ds.Tables[3].Rows)
                        {
                            itemElectiveSub.Add(new SelectListItem { Text = @dr["name_eng"].ToString(), Value = @dr["sub"].ToString() });
                        }
                        ClassMiddleInitilizeListModel.ElectiveSubjects = itemElectiveSub.ToList();
                    }

                    
                }

            }
            catch (Exception)
            {
                ClassMiddleInitilizeListModel.SessionYearList = itemSession;
                ClassMiddleInitilizeListModel.DAList = DList;
                ClassMiddleInitilizeListModel.MyDist = itemDist;
                ClassMiddleInitilizeListModel.ElectiveSubjects = itemElectiveSub;

            }
            return ClassMiddleInitilizeListModel;
        }


        public static DataSet ValidateRequestId(string RequestId, string CandName)
        {
            DataSet ds = new DataSet();
            ds = null;
            try
            {
                

                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ValidateRequestId_sp";
                cmd.Parameters.AddWithValue("@RequestId", RequestId);
                cmd.Parameters.AddWithValue("@CandName", CandName);
                ds = db.ExecuteDataSet(cmd);
                return ds;
            }
            catch(Exception ex)
            {
                return ds = null;
            }
            
        }

        #endregion



    }
}