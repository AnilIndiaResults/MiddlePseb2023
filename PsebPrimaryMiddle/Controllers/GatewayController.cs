using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.Models;
using System.Configuration;
using CCA.Util;
using System.Reflection;
using System.Data;
using PsebJunior.AbstractLayer;
using System.Text;
using System.Collections.Specialized;


namespace PsebPrimaryMiddle.Controllers
{
    public class GatewayController : Controller
    {
        //string AccessCode = ConfigurationManager.AppSettings["CcAvenueAccessCode"];
        //string CheckoutUrl = ConfigurationManager.AppSettings["CcAvenueCheckoutUrl"];
        //string WorkingKey = ConfigurationManager.AppSettings["CcAvenueWorkingKey"];
        //string MerchantId = ConfigurationManager.AppSettings["CcAvenueMerchantId"];

        public ActionResult Index()
        {
            return View();
        }


        #region ATOM PG


        public ActionResult AtomCheckoutUrl(string ChallanNo, string amt, string clientCode, string cmn, string cme, string cmno)
        {
            string strURL;
            string MerchantLogin = ConfigurationManager.AppSettings["ATOMLoginId"].ToString();
            string MerchantPass = ConfigurationManager.AppSettings["ATOMPassword"].ToString();
            string MerchantDiscretionaryData = "NB";  // for netbank
            //string ClientCode = "PSEBONLINE";
            string ClientCode = clientCode;
            string ProductID = ConfigurationManager.AppSettings["ATOMProductID"].ToString();
            string CustomerAccountNo = "0123456789";
            string TransactionType = "NBFundTransfer";  // for netbank
                                                        //string TransactionAmount = "1";
            string TransactionAmount = encrypt.QueryStringModule.Decrypt(amt);
            // string TransactionAmount = "100";
            string TransactionCurrency = "INR";
            string TransactionServiceCharge = "0";
            string TransactionID = encrypt.QueryStringModule.Decrypt(ChallanNo);
            string TransactionDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            // string TransactionDateTime = "18/10/2019 13:15:19";
            string BankID = "ATOM";
            string ru = ConfigurationManager.AppSettings["ATOMRU"].ToString();
            // User Details
            string udf1CustName = encrypt.QueryStringModule.Decrypt(cmn);
            string udf2CustEmail = !string.IsNullOrEmpty(cme) ? cme : "";
            string udf3CustMob = encrypt.QueryStringModule.Decrypt(cmno);

            strURL = GatewayController.ATOMTransferFund(MerchantLogin, MerchantPass, MerchantDiscretionaryData, ProductID, ClientCode, CustomerAccountNo, TransactionType,
              TransactionAmount, TransactionCurrency, TransactionServiceCharge, TransactionID, TransactionDateTime, BankID, ru, udf1CustName, udf2CustEmail, udf3CustMob);


            if (!string.IsNullOrEmpty(strURL))
            {
                return View(new AtomViewModel(strURL));
            }


            return Redirect(strURL);
        }




        public static string ATOMTransferFund(string MerchantLogin, string MerchantPass, string MerchantDiscretionaryData, string ProductID, string ClientCode, string CustomerAccountNo, string TransactionType, string TransactionAmount, string TransactionCurrency,
                                           string TransactionServiceCharge, string TransactionID, string TransactionDateTime, string BankID, string returnURL, string udf1CustName, string udf2CustEmail, string udf3CustMob)
        {

            string strURL, strClientCode, strClientCodeEncoded;
            byte[] b;
            string strResponse = "";
            try
            {
                b = Encoding.UTF8.GetBytes(ClientCode);
                strClientCode = Convert.ToBase64String(b);
                strClientCodeEncoded = HttpUtility.UrlEncode(strClientCode);
                strURL = "" + ConfigurationManager.AppSettings["ATOMTransferURL"].ToString();///
                strURL = strURL.Replace("[MerchantLogin]", MerchantLogin + "&");
                strURL = strURL.Replace("[MerchantPass]", MerchantPass + "&");
                strURL = strURL.Replace("[TransactionType]", TransactionType + "&");
                strURL = strURL.Replace("[ProductID]", ProductID + "&");
                strURL = strURL.Replace("[TransactionAmount]", TransactionAmount + "&");
                strURL = strURL.Replace("[TransactionCurrency]", TransactionCurrency + "&");
                strURL = strURL.Replace("[TransactionServiceCharge]", TransactionServiceCharge + "&");
                strURL = strURL.Replace("[ClientCode]", strClientCodeEncoded + "&");
                strURL = strURL.Replace("[TransactionID]", TransactionID + "&");
                strURL = strURL.Replace("[TransactionDateTime]", TransactionDateTime + "&");
                strURL = strURL.Replace("[CustomerAccountNo]", CustomerAccountNo + "&");
                strURL = strURL.Replace("[ru]", returnURL + "&");// Remove on Production
                                                                 //*****************
                string reqHashKey = ConfigurationManager.AppSettings["ATOMReqHashKey"].ToString();
                string signature = "";
                string strsignature = MerchantLogin + MerchantPass + TransactionType + ProductID + TransactionID + TransactionAmount + TransactionCurrency;
                byte[] bytes = Encoding.UTF8.GetBytes(reqHashKey);
                byte[] bt = new System.Security.Cryptography.HMACSHA512(bytes).ComputeHash(Encoding.UTF8.GetBytes(strsignature));
                signature = ATOMbyteToHexString(bt).ToLower();
                strURL = strURL.Replace("[signature]", signature + "&");
                strURL = strURL.Replace("[udf1]", udf1CustName + "&");
                strURL = strURL.Replace("[udf2]", udf2CustEmail + "&");
                strURL = strURL.Replace("[udf3]", udf3CustMob);
                return strURL;
            }
            catch (Exception ex)
            {
                strURL = "Exception encountered. " + ex.Message;
                return strURL;
            }

        }

        //public ActionResult ATOMPaymentResponse(string id)
        //{
        //    return View();
        //}


        [HttpPost]
        public ActionResult ATOMPaymentResponse()
        {
            PaymentSuccessModel _PaymentSuccessModel = new PaymentSuccessModel();
            try
            {

                NameValueCollection nvc = Request.Form;

                if (Request.Params["mmp_txn"] != null)
                {
                    string postingmmp_txn = Request.Params["mmp_txn"].ToString();
                    string postingmer_txn = Request.Params["mer_txn"].ToString();
                    string postinamount = Request.Params["amt"].ToString();
                    string postingprod = Request.Params["prod"].ToString();
                    string postingdate = Request.Params["date"].ToString();
                    string postingbank_txn = Request.Params["bank_txn"].ToString();
                    string postingf_code = Request.Params["f_code"].ToString();
                    string postingbank_name = Request.Params["bank_name"].ToString();
                    string signature = Request.Params["signature"].ToString();
                    string postingdiscriminator = Request.Params["discriminator"].ToString();

                    string respHashKey = ConfigurationManager.AppSettings["ATOMRespHashKey"].ToString();
                    string ressignature = "";
                    string strsignature = postingmmp_txn + postingmer_txn + postingf_code + postingprod + postingdiscriminator + postinamount + postingbank_txn;
                    byte[] bytes = Encoding.UTF8.GetBytes(respHashKey);
                    byte[] b = new System.Security.Cryptography.HMACSHA512(bytes).ComputeHash(Encoding.UTF8.GetBytes(strsignature));
                    ressignature = ATOMbyteToHexString(b).ToLower();


                    //Status 
                    if (signature == ressignature)
                    {
                        ViewBag.lblStatus = "Signature matched...";

                        if (postingf_code.ToLower() == "ok")
                        {
                            ViewData["response"] = "success";
                            _PaymentSuccessModel = new PaymentSuccessModel()
                            {
                                order_id = postingmer_txn,
                                tracking_id = postingmmp_txn,
                                amount = postinamount,
                                trans_date = postingdate,
                                bank_ref_no = postingbank_txn,
                                order_status = "success",
                                payment_mode = postingf_code,
                                merchant_param1 = postingprod,
                                bankname = "ATOM",
                                bankcode = "302",
                            };

                            // Update Data in Challan Master
                            int OutStatus;
                            string Mobile = "0";
                            string feecode = _PaymentSuccessModel.order_id.Substring(3, 2);
                            ViewData["feecodePG"] = feecode;

                            try
                            {
                                string OutError, OutSCHLREGID = "", OutAPPNO = "";

                                string dtResult2 = GatewayService.InsertOnlinePaymentMIS(_PaymentSuccessModel, out OutStatus, out Mobile, out OutError, out OutSCHLREGID, out OutAPPNO);// OutStatus mobile
                                ViewData["OutErrorPG"] = OutError;
                                //ViewData["OutSCHLREGID"] = OutSCHLREGID;
                                //ViewData["OutAPPNO"] = OutAPPNO;

                                if (OutStatus == 1)
                                {
                                    //send sms and email
                                    string Sms = "You have paid Rs. " + _PaymentSuccessModel.amount + " against challan no " + _PaymentSuccessModel.order_id + " on " + _PaymentSuccessModel.trans_date + " with ref no " + _PaymentSuccessModel.tracking_id + ". Regards PSEB";
                                    try
                                    {
                                        //string getSms = DBClass.gosms(Mobile, Sms);


                                    }
                                    catch (Exception) { }
                                }
                            }
                            catch (Exception)
                            {
                            }


                        }
                        else if (postingf_code.ToLower() == "f")
                        {
                            ViewData["response"] = "failure";
                            _PaymentSuccessModel = new PaymentSuccessModel()
                            {
                                order_id = postingmer_txn,
                                tracking_id = postingmmp_txn,
                                amount = postinamount,
                                trans_date = postingdate,
                                bank_ref_no = postingbank_txn,
                                order_status = "failure",
                                payment_mode = postingf_code,
                                merchant_param1 = postingprod,
                                bankname = "ATOM",
                                bankcode = "302",
                            };

                            int OutStatus;
                            string Mobile = "0";
                            string feecode = _PaymentSuccessModel.order_id.Substring(3, 2);
                            ViewData["feecodePG"] = feecode;

                            try
                            {
                                string OutError, OutSCHLREGID = "", OutAPPNO = "";
                                string dtResult2 = GatewayService.InsertOnlinePaymentMIS(_PaymentSuccessModel, out OutStatus, out Mobile, out OutError, out OutSCHLREGID, out OutAPPNO);// OutStatus mobile
                                ViewData["OutErrorPG"] = OutError;
                                //ViewData["OutSCHLREGID"] = OutSCHLREGID;
                                //ViewData["OutAPPNO"] = OutAPPNO;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else if (postingf_code.ToLower() == "c")
                        {
                            ViewData["response"] = "cancel";
                            _PaymentSuccessModel = new PaymentSuccessModel()
                            {
                                order_id = postingmer_txn,
                                tracking_id = postingmmp_txn,
                                amount = postinamount,
                                trans_date = postingdate,
                                bank_ref_no = postingbank_txn,
                                order_status = "cancel",
                                payment_mode = postingf_code,
                                merchant_param1 = postingprod,
                                bankname = "ATOM",
                                bankcode = "302",
                            };

                            string feecode = _PaymentSuccessModel.order_id.Substring(3, 2);
                            ViewData["feecodePG"] = feecode;
                        }

                        ChallanMasterModel challanMasterModel = GatewayService.GetAnyChallanDetailsById(_PaymentSuccessModel.order_id.ToString());
                        if (challanMasterModel != null)
                        {
                            _PaymentSuccessModel.FEECODE = challanMasterModel.FEECODE;
                            _PaymentSuccessModel.APPNO = challanMasterModel.APPNO;
                            _PaymentSuccessModel.SCHLREGID = challanMasterModel.SCHLREGID;
                        }

                    }
                    else
                    {
                        ViewData["response"] = "failed";
                        ViewBag.lblStatus = "Signature Mismatched...";
                    }
                }
                return View(_PaymentSuccessModel);
            }

            catch (Exception ex)
            {

            }
            return View();
        }

        public static string ATOMbyteToHexString(byte[] byData)
        {
            StringBuilder sb = new StringBuilder((byData.Length * 2));
            for (int i = 0; (i < byData.Length); i++)
            {
                int v = (byData[i] & 255);
                if ((v < 16))
                {
                    sb.Append('0');
                }

                sb.Append(v.ToString("X"));

            }
            return sb.ToString();
        }
        #endregion ATOM PG



        #region CcAvenue


        public ActionResult CcAvenue()
        {
            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Payment(string invoiceNumber)
        {
            invoiceNumber = DateTime.Now.ToString("yyMMddHHmmssff"); // must be challan id
            string amount = "2";
            var queryParameter = new CCACrypto();
            //CCACrypto is the dll you get when you download the ASP.NET 3.5 integration kit from //ccavenue account.

            return View("CcAvenue", new CcAvenueViewModel(queryParameter.Encrypt
           (BuildCcAvenueRequestParameters(invoiceNumber, amount), ConfigurationManager.AppSettings["CcAvenueWorkingKey"]), ConfigurationManager.AppSettings["CcAvenueAccessCode"], ConfigurationManager.AppSettings["CcAvenueCheckoutUrl"]));
        }


        public static string BuildCcAvenueRequestParameters(string invoiceNumber, string amount)
        {
            var queryParameters = new Dictionary<string, string>
             {
             {"order_id", invoiceNumber},
             {"merchant_id", ConfigurationManager.AppSettings["CcAvenueMerchantId"].ToString()},
             {"amount", amount},
             {"currency","INR" },
             //{"redirect_url","https://localhost:57360/Gateway/CCAvenuePaymentSuccessful" },
             //{"cancel_url","https://localhost:57360/Gateway/CCAvenuePaymentCancelled"},
             {"redirect_url", ConfigurationManager.AppSettings["CCAvenuePaymentSuccessful"].ToString()},
             {"cancel_url", ConfigurationManager.AppSettings["CCAvenuePaymentCancelled"].ToString()},
             {"request_type","JSON" },
             {"response_type","JSON" },
             {"version","1.1" }
        }.Select(item => string.Format("{0}={1}", item.Key, item.Value));
            return string.Join("&", queryParameters);
        }


        //public ActionResult CCAvenuePaymentSuccessful()
        //{
        //    return View();
        //}

        [HttpPost]
        public ActionResult CCAvenuePaymentSuccessful(string encResp)
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            PaymentSuccessModel _PaymentSuccessModel = new PaymentSuccessModel();


            var decryption = new CCACrypto();
            var decryptedParameters = decryption.Decrypt(encResp, ConfigurationManager.AppSettings["CcAvenueWorkingKey"]);

            var keyValuePairs = decryptedParameters.Split('&');
            var splittedKeyValuePairs = new Dictionary<string, string>();

            foreach (var value in keyValuePairs)
            {
                var keyValuePair = value.Split('=');
                splittedKeyValuePairs.Add(keyValuePair[0], keyValuePair[1]);
            }

            //Here you can check the consistency of data i.e what you send is what you get back,
            //Make sure its not corrupted....
            //After that Save the details of the transaction into a db if you want to...

            string orderStatusValue;
            if (splittedKeyValuePairs.TryGetValue("order_status", out orderStatusValue))
            {
                if (orderStatusValue.ToLower() == "success")
                {
                    _PaymentSuccessModel = new PaymentSuccessModel()
                    {
                        order_id = splittedKeyValuePairs["order_id"],
                        tracking_id = splittedKeyValuePairs["tracking_id"],
                        amount = splittedKeyValuePairs["amount"],
                        trans_date = splittedKeyValuePairs["trans_date"],
                        bank_ref_no = splittedKeyValuePairs["bank_ref_no"],
                        order_status = "success".ToLower(),
                        payment_mode = splittedKeyValuePairs["payment_mode"],
                        merchant_param1 = splittedKeyValuePairs["merchant_param1"],
                        bankname = "HDFC",
                        bankcode = "301",

                    };


                    int OutStatus;
                    string Mobile = "0";
                    string feecode = _PaymentSuccessModel.order_id.Substring(3, 2);
                    ViewData["feecodePG"] = feecode;
                    // ViewData["STEP"] = "STEP1";

                    try
                    {
                        string OutError, OutSCHLREGID = "", OutAPPNO = "";

                        string dtResult2 = GatewayService.InsertOnlinePaymentMIS(_PaymentSuccessModel, out OutStatus, out Mobile, out OutError, out OutSCHLREGID, out OutAPPNO);// OutStatus mobile

                        ViewData["OutErrorPG"] = OutError;
                        //ViewData["OutSCHLREGID"] = OutSCHLREGID;
                        //ViewData["OutAPPNO"] = OutAPPNO;
                        if (OutStatus == 1)
                        {
                            //ViewData["STEP"] = "STEP2";
                            //send sms and email
                            string Sms = "You have paid Rs. " + _PaymentSuccessModel.amount + " against challan no " + _PaymentSuccessModel.order_id + " on " + _PaymentSuccessModel.trans_date + " with ref no " + _PaymentSuccessModel.tracking_id + ". Regards PSEB";
                            try
                            {
                                string getSms = DBClass.gosms(Mobile, Sms);


                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception)
                    {
                        //ViewData["STEP"] = "STEP-ERR";
                    }

                }
                else if (orderStatusValue.ToLower() == "failure")
                {
                    _PaymentSuccessModel = new PaymentSuccessModel()
                    {
                        order_id = splittedKeyValuePairs["order_id"],
                        tracking_id = splittedKeyValuePairs["tracking_id"],
                        amount = splittedKeyValuePairs["amount"],
                        trans_date = splittedKeyValuePairs["trans_date"],
                        bank_ref_no = splittedKeyValuePairs["bank_ref_no"],
                        order_status = "failure".ToLower(),
                        payment_mode = splittedKeyValuePairs["payment_mode"],
                        merchant_param1 = splittedKeyValuePairs["merchant_param1"],
                        bankname = "HDFC",
                        bankcode = "301",
                    };

                    int OutStatus;
                    string Mobile = "";
                    string feecode = _PaymentSuccessModel.order_id.Substring(3, 2);
                    ViewData["feecodePG"] = feecode;
                    // ViewData["STEP"] = "STEP1";

                    try
                    {
                        string OutError, OutSCHLREGID = "", OutAPPNO = "";

                        string dtResult2 = GatewayService.InsertOnlinePaymentMIS(_PaymentSuccessModel, out OutStatus, out Mobile, out OutError, out OutSCHLREGID, out OutAPPNO);// OutStatus mobile

                        ViewData["OutErrorPG"] = OutError;
                        //ViewData["OutSCHLREGID"] = OutSCHLREGID;
                        //ViewData["OutAPPNO"] = OutAPPNO;
                        // ViewData["STEP"] = "STEP2";


                    }
                    catch (Exception)
                    {
                        // ViewData["STEP"] = "STEP-ERR";
                    }
                }
            }



            ChallanMasterModel challanMasterModel = GatewayService.GetAnyChallanDetailsById(_PaymentSuccessModel.order_id.ToString());
            if (challanMasterModel != null)
            {
                _PaymentSuccessModel.FEECODE = challanMasterModel.FEECODE;
                _PaymentSuccessModel.APPNO = challanMasterModel.APPNO;
                _PaymentSuccessModel.SCHLREGID = challanMasterModel.SCHLREGID;
            }

            return View(_PaymentSuccessModel);
        }

        [HttpPost]
        public ActionResult CCAvenuePaymentCancelled()
        {
            return RedirectToAction("Home", "Home");
            // return View();
        }

        #endregion CcAvenue





    }
}