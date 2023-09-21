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
using System.Security.Cryptography;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace PsebJunior.AbstractLayer
{
    public class WebSerDB :  IDisposable
    {
       
        private static MemoryStream memoryStream = null;
        private static CryptoStream cryptoStream = null;
        private static Aes encryptor = null;

       
       
        #region Decrypt & Encrypt       

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText.Replace(' ', '+'));

            // Do something
            encryptor =  Aes.Create();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            memoryStream = new MemoryStream();
            cryptoStream = new CryptoStream(memoryStream, encryptor.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
            cryptoStream.Close();
            cipherText = Encoding.Unicode.GetString(memoryStream.ToArray());
            return cipherText;
        }


        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            // Do something
            encryptor = encryptor = Aes.Create();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            memoryStream = new MemoryStream();
            cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(clearBytes, 0, clearBytes.Length);
            cryptoStream.Close();
            clearText = Convert.ToBase64String(memoryStream.ToArray());
            return clearText;
        }



        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }

                if (cryptoStream != null)
                {
                    cryptoStream.Dispose();
                }
            }
        }
        #endregion Decrypt & Encrypt


        public static List<SelectListItem> GenderList()
        {
            List<SelectListItem> GroupList = new List<SelectListItem>();
            GroupList.Add(new SelectListItem { Text = "MALE", Value = "1" });
            GroupList.Add(new SelectListItem { Text = "FEMALE", Value = "2" });
            GroupList.Add(new SelectListItem { Text = "TRANS", Value = "3" });
            return GroupList;
        }

        public static string GetGender(string genderID)
        {
            string sex = (genderID == "1") ? "MALE" : "FEMALE"; // dt1.Rows[0]["genderID"].ToString();
            return sex;
        }

        public static string GetCaste(string casteCategoryCode)
        {
            string caste = (casteCategoryCode == "1") ? "SC" : (casteCategoryCode == "2") ? "ST" : (casteCategoryCode == "3") ? "OBC" : (casteCategoryCode == "4") ? "GENERAL" : "GENERAL";
            return caste;
        }

        public static string GetReligion(string religionCode)
        {
            string reli = (religionCode == "1") ? "HINDU" : (religionCode == "2") ? "MUSLIM" : (religionCode == "3") ? "CHRISTIAN" : (religionCode == "4") ? "SIKH" : "OTHERS";
            return reli;
        }

        public static string GetPHY_CHL(string disabilityType)
        {
            string DA = (disabilityType == "1") ? "Blind/Visually Impaired" : (disabilityType == "2") ? "Blind/Visually Impaired" : (disabilityType == "3") ? "Deaf & Dumb/Hearing Impaired" : (disabilityType == "4") ? "Deaf & Dumb/Hearing Impaired" : (disabilityType == "11") ? "Physically Handicapped" : "OTHERS";
            return DA;
        }

        public static DataSet GetudiCodeDetails_SpJunior(string formname, string udicode)
        {
            DataSet ds = new DataSet();
            try
            {
             Database db = DatabaseFactory.CreateDatabase();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetudiCodeDetails_SpJunior";
            cmd.Parameters.AddWithValue("@udicode", udicode);
            cmd.Parameters.AddWithValue("@formname", formname);
            return db.ExecuteDataSet(cmd);
            }
            catch(Exception e)
            {
                return ds=null;
            }
        }


        public static string GetUdiCode(string schl)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetUdiCode_Sp";
                cmd.Parameters.AddWithValue("@schlcode", schl);
                cmd.Parameters.Add("@uid", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();
                string uid = (string)cmd.Parameters["@uid"].Value;

//                result = db.ExecuteScalar(cmd).ToString();              
               // string uid = (string)cmd.Parameters["@uid"].Value;
                return uid;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        public static string Insert_udiCodeTemp_SpJunior(string formname, webSerModel wm, FormCollection frc, DataTable dt)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "Insert_udiCodeTemp_SpJunior";
                cmd.Parameters.AddWithValue("@formname", formname);
                cmd.Parameters.AddWithValue("@udiCode", wm.UdiseCode);
                cmd.Parameters.AddWithValue("@udiStdDetails", dt);
                cmd.Parameters.AddWithValue("@status", 0);
                cmd.Parameters.Add("@id", SqlDbType.Int, 500);
                cmd.Parameters["@id"].Direction = ParameterDirection.Output;
                result = db.ExecuteNonQuery(cmd).ToString();             
                string id = cmd.Parameters["@id"].Value.ToString();
                return id;
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }


        public static string insert_EPubjabWebservice_DataJunior(string formName, string CurrentSchl, string epunid, string aadhar, string Name, string Pname, string fname, string Pfname, string mname, string Pmname, string dob, string sex, string caste, string reli, string DA, string mob, string admno, string Address, string pincode)
        {
            string result;
            try
            {

                DataSet ds = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "insert_Webservice_DataJunior";
                cmd.Parameters.AddWithValue("@CurrentSchl", CurrentSchl);
                cmd.Parameters.AddWithValue("@epunid", epunid);
                cmd.Parameters.AddWithValue("@form_Name", formName);
                cmd.Parameters.AddWithValue("@aadhar", aadhar);
                cmd.Parameters.AddWithValue("@admno", admno);
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@fname", fname);
                cmd.Parameters.AddWithValue("@mname", mname);

                cmd.Parameters.AddWithValue("@Pname", Pname);
                cmd.Parameters.AddWithValue("@Pfname", Pfname);
                cmd.Parameters.AddWithValue("@Pmname", Pmname);

                cmd.Parameters.AddWithValue("@dob", dob);
                cmd.Parameters.AddWithValue("@sex", sex);
                cmd.Parameters.AddWithValue("@caste", caste);
                cmd.Parameters.AddWithValue("@reli", reli);
                cmd.Parameters.AddWithValue("@DA", DA);
                cmd.Parameters.AddWithValue("@mob", mob);
                cmd.Parameters.AddWithValue("@Address", Address);
                cmd.Parameters.AddWithValue("@pincode", pincode);              
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