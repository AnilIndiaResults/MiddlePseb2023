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
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Web.UI;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Threading.Tasks;
using System.Threading;

namespace PsebJunior.AbstractLayer
{
    public class GatewayService
    {

        #region Update Data in Challan master


        public static string InsertOnlinePaymentMIS(PaymentSuccessModel BM, out int OutStatus, out string Mobile, out string OutError, out string OutSCHLREGID, out string OutAPPNO)
        {
            try
            {
                decimal d = decimal.Parse(BM.amount);
                int payamount = Decimal.ToInt32(d);
                //
                DataSet result = new DataSet();
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertOnlinePaymentMISSPNEW"; //InsertOnlinePaymentMISSP // InsertOnlinePaymentMISSPNEW
                cmd.Parameters.AddWithValue("@order_id", BM.order_id);
                cmd.Parameters.AddWithValue("@trans_id", BM.tracking_id);
                cmd.Parameters.AddWithValue("@amount", payamount.ToString());
                cmd.Parameters.AddWithValue("@trans_date", BM.trans_date);
                cmd.Parameters.AddWithValue("@bank_ref_no", BM.bank_ref_no);
                cmd.Parameters.AddWithValue("@order_status", BM.order_status);
                cmd.Parameters.AddWithValue("@payment_mode", BM.payment_mode);
                cmd.Parameters.AddWithValue("@merchant_param1", BM.merchant_param1);
                cmd.Parameters.AddWithValue("@bankname", BM.bankname);
                cmd.Parameters.AddWithValue("@bankcode", BM.bankcode);
                cmd.Parameters.Add("@OutStatus", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutError", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Mobile", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                //
                cmd.Parameters.Add("@OutSCHLREGID", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@OutAPPNO", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;

                result = db.ExecuteDataSet(cmd);
                OutStatus = (int)cmd.Parameters["@OutStatus"].Value;
                Mobile = (string)cmd.Parameters["@Mobile"].Value;
                OutError = (string)cmd.Parameters["@OutError"].Value;

                OutSCHLREGID = (string)cmd.Parameters["@OutSCHLREGID"].Value;
                OutAPPNO = (string)cmd.Parameters["@OutAPPNO"].Value;
                return OutStatus.ToString();
            }
            catch (Exception ex)
            {
                OutStatus = -1;
                Mobile = "";
                OutAPPNO = "";
                OutSCHLREGID = "";
                OutError = "ERR : " + ex.Message;
                return null;
            }
        }



        public static ChallanMasterModel GetAnyChallanDetailsById(string ChallanId)  // Type 1=Regular, 2=Open
        {
            ChallanMasterModel challanMasterModel = new ChallanMasterModel();
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetChallanDetailsByIdSPBank";// LoginSP(old)
                cmd.Parameters.AddWithValue("@CHALLANID", ChallanId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        challanMasterModel.CHALLANID = DBNull.Value != reader["CHALLANID"] ? (string)reader["CHALLANID"] : default(string);
                        challanMasterModel.APPNO = DBNull.Value != reader["APPNO"] ? (string)reader["APPNO"] : default(string);
                        challanMasterModel.SCHLREGID = DBNull.Value != reader["SCHLREGID"] ? (string)reader["SCHLREGID"] : default(string);
                        challanMasterModel.DepositoryMobile = DBNull.Value != reader["DepositoryMobile"] ? (string)reader["DepositoryMobile"] : default(string);


                        challanMasterModel.FEECODE = DBNull.Value != reader["FEECODE"] ? (string)reader["FEECODE"] : default(string);
                        challanMasterModel.FEECAT = DBNull.Value != reader["FEECAT"] ? (string)reader["FEECAT"] : default(string);
                        challanMasterModel.BCODE = DBNull.Value != reader["BCODE"] ? (string)reader["BCODE"] : default(string);
                        challanMasterModel.BANK = DBNull.Value != reader["BANK"] ? (string)reader["BANK"] : default(string);
                        challanMasterModel.ChallanGDateN = DBNull.Value != reader["ChallanGDateN"] ? (DateTime)reader["ChallanGDateN"] : default(DateTime);


                        challanMasterModel.FEE = DBNull.Value != reader["FEE"] ? Convert.ToSingle(Convert.ToInt32(reader["FEE"])) : default(float);
                        challanMasterModel.TOTFEE = DBNull.Value != reader["TOTFEE"] ? Convert.ToSingle(Convert.ToInt32(reader["TOTFEE"])) : default(float);
                        challanMasterModel.J_REF_NO = DBNull.Value != reader["J_REF_NO"] ? (string)reader["J_REF_NO"] : default(string);

                    }
                }
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                challanMasterModel = null;
            }
            return challanMasterModel;
        }



        #endregion

        #region Algorithm
        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));
                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        public string Encrypt(string input, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(input);
            Aes kgen = Aes.Create("AES");
            kgen.Mode = CipherMode.ECB;
            //kgen.Padding = PaddingMode.None;
            kgen.Key = keyArray;
            ICryptoTransform cTransform = kgen.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        #endregion

    }
}