using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using CCA.Util;
using ClosedXML.Excel;
using PsebJunior.AbstractLayer;
using PsebJunior.Models;
using PsebPrimaryMiddle.Filters;
using NReco.PdfGenerator;
using IronPdf;
using PsebPrimaryMiddle.Repository;

namespace PsebPrimaryMiddle.Controllers
{
    public class TestController : Controller
    {

        private readonly IBankRepository _bankRepository;
        private readonly IChallanRepository _challanRepository;
        public TestController(IChallanRepository challanRepository, IBankRepository bankRepository)
        {
            _challanRepository = challanRepository;
            _bankRepository = bankRepository;
        }

        RegistrationModels rm = new RegistrationModels();

        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        #region A1Formgrid
        [HttpGet]
        public JsonResult GetA1Grid() // Calling on http post (on Submit)
        {
            //LoginSession loginSession = (LoginSession)Session["LoginSession"];
            //string id = "A1";




            //rm.StoreAllData = RegistrationDB.GetStudentRecordsSearchPM(id, loginSession.SCHL);
            //if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{

            //     var source = StaticDB.DataTableToNewtonsoftJSON(rm.StoreAllData.Tables[0]);
            //    return Json(new { data = source }, JsonRequestBehavior.AllowGet);
            //}
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [SessionCheckFilter]
        public ActionResult A1Formgrid(RegistrationSearchModelList registrationSearchModel)
        {            
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            //var itemsch = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new{ID="2",Name="Candidate Name"},
            //new{ID="3",Name="Father's Name"},new{ID="4",Name="Mother's Name"},new{ID="5",Name="DOB"},}, "ID", "Name", 1);
            //ViewBag.MySch = itemsch.ToList();
      
            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("A1", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult A1Formgrid(int? page, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            //var itemsch = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new{ID="2",Name="Candidate Name"},
            //new{ID="3",Name="Father's Name"},new{ID="4",Name="Mother's Name"},new{ID="5",Name="DOB"},}, "ID", "Name", 1);
            //ViewBag.MySch = itemsch.ToList();


            return View(rm);
        }

        #endregion A1Formgrid

        //#region Calculate Fee of Registration
        //[SessionCheckFilter]
        //public ActionResult CalculateFee(string id, string Status, FormCollection frc)
        //{
        //    try
        //    {
        //        FeeHomeViewModel fhvm = new FeeHomeViewModel();
        //        LoginSession loginSession = (LoginSession)Session["LoginSession"];

        //        TempData["CalculateFee"] = null;
        //        TempData["AllowBanks"] = null;
        //        TempData["PaymentForm"] = null;
        //        TempData["FeeStudentList"] = null;
        //        return View(fhvm);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
        //        return RedirectToAction("Logout", "Login");
        //    }

        //}


        //[SessionCheckFilter]
        //[HttpPost]
        //public ActionResult CalculateFee(string id, string Status, FormCollection frc, string aa, string ChkId)
        //{
        //    try
        //    {
        //        FeeHomeViewModel fhvm = new FeeHomeViewModel();
        //        LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //        // ViewBag.Eighth = loginSession.middle == "Y" ? "1" : "0";
        //        /// ViewBag.Fifth = loginSession.fifth == "Y" ? "1" : "0";

        //        if (Status == "Successfully" || Status == "Failure")
        //        {
        //            //ViewData["Status"] = Status;
        //            ViewData["FeeStatus"] = Status;
        //            return View();
        //        }
        //        else
        //        {
        //            // FormCollection frc = new FormCollection();
        //            string FormNM = frc["ChkId"];
        //            if (FormNM == null || FormNM == "")
        //            {
        //                return View();
        //            }



        //            ViewData["Status"] = "";
        //            string UserType = "User";


        //            string Search = string.Empty;
        //            Search = "SCHL='" + loginSession.SCHL + "'";
        //            FormNM = "'" + FormNM.Replace(",", "','") + "'";
        //            Search += "  and type='" + UserType + "' and form_name in(" + FormNM + ")";

        //            string SearchString = DateTime.Now.ToString("dd/MM/yyyy");
        //            DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);


        //            //*******for Testing Start by Rohit

        //            // Change after  
        //            //DataSet ds = _challanRepository.GetCalculateFeeBySchool(Search, loginSession.SCHL, date);
        //            //fhvm.StoreAllData = ds;
        //            //if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
        //            //{
        //            //    ViewBag.Message = "Record Not Found";
        //            //    ViewBag.TotalCount = 0;
        //            //    ViewData["FeeStatus"] = "3";
        //            //    return View();
        //            //}
        //            //else
        //            //{
        //            //    ViewData["FeeStatus"] = "1";
        //            //    //Session["CalculateFee"] = ds;
        //            //    TempData["CalculateFee"] = ds;
        //            //    ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
        //            //    //Session["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
        //            //    TempData["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
        //            //    fhvm.TotalFeesInWords = ds.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
        //            //    fhvm.EndDate = ds.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + ds.Tables[0].Rows[0]["FeeValidDate"].ToString();                      
        //            //}

        //            //*******for Testing End


        //            /// for Live

        //            DataSet dsCheckFee = _challanRepository.CheckFeeStatus(loginSession.SCHL, UserType, FormNM.ToUpper(), date);//CheckFeeStatusSPByView

        //            //CheckFeeStatus
        //            if (dsCheckFee == null)
        //            {
        //                // return RedirectToAction("Index", "Home");
        //                ViewData["FeeStatus"] = "11";
        //                return View();
        //            }
        //            else
        //            {
        //                if (dsCheckFee.Tables[0].Rows.Count > 0)
        //                {
        //                    if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "0")
        //                    {
        //                        //Not Exist (if lot '0' not xist)
        //                        ViewData["FeeStatus"] = "0";
        //                        ViewBag.Message = "All Fees are submmited/ Data Not Available for Fee Calculation...";
        //                        ViewBag.TotalCount = 0;
        //                    }
        //                    else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "2")
        //                    {
        //                        //Not Allowed Some Form are not in Fee
        //                        ViewData["FeeStatus"] = "2";
        //                        // ViewBag.Message = "Not Allowed,Some FORM are not in Fee Structure..please contact Punjab School Education Board";
        //                        ViewBag.Message = "Calculate fee is allowed only for M1 and T1 Form only";
        //                        DataSet ds = dsCheckFee;
        //                        fhvm.StoreAllData = dsCheckFee;
        //                        ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
        //                        // return View(fhvm);
        //                    }
        //                    else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "3" || dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "5")
        //                    {
        //                        int CountTable = dsCheckFee.Tables.Count;
        //                        ViewBag.CountTable = CountTable;
        //                        ViewBag.OutStatus = dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString();
        //                        //Not Allowed Some Form are not in Fee
        //                        ViewData["FeeStatus"] = "2";
        //                        // ViewBag.Message = "Not Allowed, Some Mandatory Fields are not Filled.";
        //                        // ViewBag.Message = "Some Mandatory Fields Like Subject's/Photograph's,Signature's of Listed Form wise Candidate's are Missing or Duplicate Records. Please Update These Details Then Try Again to Calculate Fee & Final Submission";
        //                        DataSet ds = dsCheckFee;
        //                        fhvm.StoreAllData = dsCheckFee;
        //                        if (CountTable > 1)
        //                        {
        //                            ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
        //                            if (dsCheckFee.Tables[1].Rows.Count > 0)
        //                            {
        //                                if (dsCheckFee.Tables[1].Rows[0]["Outstatus"].ToString() == "5")
        //                                {
        //                                    ViewBag.TotalCountDuplicate = dsCheckFee.Tables[1].Rows.Count;
        //                                }
        //                                else
        //                                { ViewBag.TotalCountDuplicate = 0; }
        //                            }
        //                            else
        //                            { ViewBag.TotalCountDuplicate = 0; }

        //                        }


        //                        // return View(fhvm);
        //                    }
        //                    else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "1")
        //                    {
        //                        DataSet ds = _challanRepository.GetCalculateFeeBySchool(Search, loginSession.SCHL, date);
        //                        fhvm.StoreAllData = ds;
        //                        if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
        //                        {
        //                            ViewBag.Message = "Record Not Found";
        //                            ViewBag.TotalCount = 0;
        //                            ViewData["FeeStatus"] = "3";
        //                            return View();
        //                        }
        //                        else
        //                        {
        //                            ViewData["FeeStatus"] = "1";
        //                            TempData["CalculateFee"] = ds;
        //                            ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
        //                            TempData["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
        //                            fhvm.TotalFeesInWords = ds.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
        //                            fhvm.EndDate = ds.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + ds.Tables[0].Rows[0]["FeeValidDate"].ToString();
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //return RedirectToAction("Index", "Home");
        //                    ViewData["FeeStatus"] = "22";
        //                    return View();
        //                }
        //            }
        //            return View(fhvm);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
        //        return RedirectToAction("Logout", "Login");
        //    }

        //}


        //[SessionCheckFilter]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        //public ActionResult PaymentForm()
        //{
        //    try
        //    {
        //        LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //        PaymentformViewModel pfvm = new PaymentformViewModel();

        //        if (TempData["CalculateFee"] == null || TempData["CalculateFee"].ToString() == "")
        //        {
        //            return RedirectToAction("CalculateFee", "Test");
        //        }

        //        string schl = loginSession.SCHL;
        //        DataSet ds = _challanRepository.GetSchoolLotDetails(schl);
        //        pfvm.PaymentFormData = ds;
        //        if (pfvm.PaymentFormData == null || pfvm.PaymentFormData.Tables[0].Rows.Count == 0)
        //        {
        //            ViewBag.Message = "Record Not Found";
        //            ViewBag.TotalCount = 0;
        //            return View();
        //        }
        //        else
        //        {
        //            pfvm.LOTNo = Convert.ToInt32(ds.Tables[0].Rows[0]["LOT"].ToString());
        //            pfvm.Dist = ds.Tables[0].Rows[0]["Dist"].ToString();
        //            pfvm.District = ds.Tables[0].Rows[0]["diste"].ToString();
        //            pfvm.DistrictFull = ds.Tables[0].Rows[0]["DistrictFull"].ToString();
        //            //pfvm.SchoolCode = Convert.ToInt32(ds.Tables[0].Rows[0]["schl"].ToString());
        //            pfvm.SchoolCode = ds.Tables[0].Rows[0]["schl"].ToString();
        //            pfvm.SchoolName = ds.Tables[0].Rows[0]["SchoolFull"].ToString(); // Schollname with station and dist 
        //            ViewBag.TotalCount = pfvm.PaymentFormData.Tables[0].Rows.Count;

        //            DataSet dscalFee = (DataSet)TempData["CalculateFee"];

        //            pfvm.totaddfee = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalExamFee"].ToString());/// 
        //            pfvm.TotalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFee"].ToString());
        //            pfvm.TotalLateFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalLateFee"].ToString());
        //            pfvm.TotalFinalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFeeAmount"].ToString());
        //            pfvm.TotalFeesInWords = dscalFee.Tables[1].Rows[0]["TotalFeesWords"].ToString();
        //            //  pfvm.FeeDate = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy");
        //            pfvm.FeeDate = dscalFee.Tables[0].Rows[0]["FeeValidDateFormat"].ToString();
        //            //TotalCandidates
        //            pfvm.TotalCandidates = Convert.ToInt32(dscalFee.Tables[0].AsEnumerable().Sum(x => x.Field<int>("CountStudents")));
        //            pfvm.FeeCode = dscalFee.Tables[0].Rows[0]["FeeCode"].ToString();
        //            pfvm.FeeCategory = dscalFee.Tables[0].Rows[0]["FEECAT"].ToString();
        //            TempData["PaymentForm"] = pfvm;

        //            // new add AllowBanks by rohit
        //            pfvm.AllowBanks = dscalFee.Tables[0].Rows[0]["AllowBanks"].ToString();
        //            string[] bls = pfvm.AllowBanks.Split(',');
        //            // BankList bl = new BankList();
        //            BankModels BM = new BankModels();
        //            pfvm.bankList = new List<BankListModel>();
        //            for (int b = 0; b < bls.Count(); b++)
        //            {
        //                int OutStatus;
        //                BM.BCODE = bls[b].ToString();
        //                DataSet ds1 = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
        //                BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
        //                pfvm.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
        //            }
        //            ///////////////


        //            //string CheckFee = ds.Tables[1].Rows[0]["TotalFeeAmount"].ToString();
        //            //if (pfvm.TotalFinalFees == 0 && pfvm.TotalFees == 0)
        //            //{
        //            //    ViewBag.CheckForm = 1; // only verify for M1 and T1 
        //            //    Session["CheckFormFee"] = 0;
        //            //}
        //            //else
        //            //{
        //            //    ViewBag.CheckForm = 0; // only verify for M1 and T1 
        //            //    Session["CheckFormFee"] = 1;
        //            //}
        //            return View(pfvm);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
        //        return RedirectToAction("Logout", "Login");
        //    }
        //}

        //[SessionCheckFilter]
        //[HttpPost]
        //public ActionResult PaymentForm(PaymentformViewModel pfvm, FormCollection frm, string PayModValue, string IsOnline)
        //{
        //    try
        //    {
        //        LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //        ChallanMasterModel CM = new ChallanMasterModel();
        //        if (pfvm.BankCode == null)
        //        {
        //            ViewBag.Message = "Please Select Bank";
        //            ViewData["SelectBank"] = "1";
        //            return View(pfvm);
        //        }

        //        if (pfvm.BankCode == "301" || pfvm.BankCode == "302")
        //        {
        //            PayModValue = "online";
        //        }
        //        else if (pfvm.BankCode == "203")
        //        {
        //            PayModValue = "hod";
        //        }
        //        else
        //        {
        //            PayModValue = "offline";
        //        }


        //        if (TempData["PaymentForm"] == null || TempData["PaymentForm"].ToString() == "")
        //        {
        //            return RedirectToAction("PaymentForm", "Test");
        //        }
        //        if (TempData["FeeStudentList"] == null || TempData["FeeStudentList"].ToString() == "")
        //        {
        //            return RedirectToAction("PaymentForm", "Test");
        //        }


        //        string BankCode = pfvm.BankCode;
        //        PaymentformViewModel PFVMSession = (PaymentformViewModel)TempData["PaymentForm"];
        //        if (ModelState.IsValid)
        //        {
        //            string SCHL = loginSession.SCHL;
        //            string FeeStudentList = TempData["FeeStudentList"].ToString();
        //            CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);

        //            // new add AllowBanks by rohit
        //            //pfvm.AllowBanks = dscalFee.Tables[0].Rows[0]["AllowBanks"].ToString();
        //            string[] bls = PFVMSession.AllowBanks.Split(',');
        //            // BankList bl = new BankList();
        //            BankModels BM = new BankModels();
        //            PFVMSession.bankList = new List<BankListModel>();
        //            for (int b = 0; b < bls.Count(); b++)
        //            {
        //                int OutStatus;
        //                BM.BCODE = bls[b].ToString();
        //                DataSet ds1 = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
        //                BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
        //                PFVMSession.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
        //            }
        //            ///////////////


        //            CM.FEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
        //            CM.TOTFEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
        //            string TotfeePG = (CM.TOTFEE).ToString();
        //            CM.addfee = Convert.ToInt32(PFVMSession.totaddfee);// exam fee
        //            CM.regfee = Convert.ToInt32(PFVMSession.TotalFees);
        //            CM.latefee = Convert.ToInt32(PFVMSession.TotalLateFees);
        //            CM.FEECAT = PFVMSession.FeeCategory;
        //            CM.FEECODE = PFVMSession.FeeCode;
        //            CM.FEEMODE = "CASH";
        //            CM.BANK = pfvm.BankName;
        //            CM.BCODE = pfvm.BankCode;
        //            CM.BANKCHRG = PFVMSession.BankCharges;
        //            CM.SchoolCode = PFVMSession.SchoolCode.ToString();
        //            CM.DIST = PFVMSession.Dist.ToString();
        //            CM.DISTNM = PFVMSession.District;
        //            CM.LOT = PFVMSession.LOTNo;
        //            CM.SCHLREGID = PFVMSession.SchoolCode.ToString();
        //            CM.type = "schle";
        //            DateTime CHLNVDATE2;
        //            if (DateTime.TryParseExact(PFVMSession.FeeDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
        //            {
        //                CM.ChallanVDateN = CHLNVDATE2;
        //            }
        //            CM.CHLNVDATE = PFVMSession.FeeDate;
        //            string SchoolMobile = "";
        //            //string result = "0";
        //            string result = _challanRepository.InsertPaymentFormJunior(CM,  out SchoolMobile);
        //            if (result == null || result == "0")
        //            {
        //                //--------------Not saved
        //                ViewData["result"] = 0;
        //                ViewData["Error"] = SchoolMobile;
        //            }
        //            else if (result == "-1")
        //            {
        //                //-----alredy exist
        //                ViewData["result"] = -1;
        //            }
        //            else
        //            {

        //                ViewData["SelectBank"] = null;
        //                ViewData["result"] = 1;
        //                ViewBag.ChallanNo = result;

        //                if (PayModValue.ToString().ToLower().Trim() == "online")
        //                {
        //                    #region Payment Gateyway
        //                    string paymenttype = BankCode;
        //                    if (paymenttype.ToUpper() == "301" && ViewBag.ChallanNo != "") /*HDFC*/
        //                    {
        //                        string AccessCode = ConfigurationManager.AppSettings["CcAvenueAccessCode"];
        //                        string CheckoutUrl = ConfigurationManager.AppSettings["CcAvenueCheckoutUrl"];
        //                        string WorkingKey = ConfigurationManager.AppSettings["CcAvenueWorkingKey"];
        //                        //******************
        //                        string invoiceNumber = ViewBag.ChallanNo;
        //                        string amount = TotfeePG;
        //                        //***************
        //                        var queryParameter = new CCACrypto();

        //                        string strURL = GatewayController.BuildCcAvenueRequestParameters(invoiceNumber, amount);

        //                        return View("../Gateway/CcAvenue", new CcAvenueViewModel(queryParameter.Encrypt
        //                                   (strURL, WorkingKey), AccessCode, CheckoutUrl));


        //                        //return View("../Gateway/AtomCheckoutUrl", new AtomViewModel(strURL));

        //                        // return View("CcAvenue", new CcAvenueViewModel(queryParameter.Encrypt
        //                        //(BuildCcAvenueRequestParameters(invoiceNumber, amount), WorkingKey), AccessCode, CheckoutUrl));
        //                    }
        //                    else if (paymenttype.ToUpper() == "302" && ViewBag.ChallanNo != "")/*ATOM*/
        //                    {
        //                        string strURL;
        //                        //string MerchantLogin = "197";
        //                        //string MerchantPass = "Test@123";
        //                        string MerchantLogin = ConfigurationManager.AppSettings["ATOMLoginId"].ToString();
        //                        string MerchantPass = ConfigurationManager.AppSettings["ATOMPassword"].ToString();
        //                        string MerchantDiscretionaryData = "NB";  // for netbank
        //                        string ClientCode = "PSEB";
        //                        string ProductID = ConfigurationManager.AppSettings["ATOMProductID"].ToString();
        //                        string CustomerAccountNo = "10000036600";
        //                        string TransactionType = "NBFundTransfer";  // for netbank
        //                                                                    //string TransactionAmount = "1";
        //                        string TransactionAmount = TotfeePG;
        //                        string TransactionCurrency = "INR";
        //                        string TransactionServiceCharge = "0";
        //                        string TransactionID = ViewBag.ChallanNo;// Unique Challan Number
        //                        string TransactionDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
        //                        // string TransactionDateTime = "18/10/2019 13:15:19";
        //                        string BankID = "ATOM TEST";
        //                        //string ru = "https://localhost:57360/Gateway/ATOMPaymentResponse";
        //                        //string ru = ConfigurationManager.AppSettings["ATOMRUTEST"].ToString();

        //                        string ru = ConfigurationManager.AppSettings["ATOMRU"].ToString();
        //                        // User Details
        //                        string udf1CustName = CM.SCHLREGID;
        //                        string udf2CustEmail = CM.SCHLREGID;
        //                        string udf3CustMob = "9876543210";

        //                        strURL = GatewayController.ATOMTransferFund(MerchantLogin, MerchantPass, MerchantDiscretionaryData, ProductID, ClientCode, CustomerAccountNo, TransactionType,
        //                          TransactionAmount, TransactionCurrency, TransactionServiceCharge, TransactionID, TransactionDateTime, BankID, ru, udf1CustName, udf2CustEmail, udf3CustMob);

        //                        if (!string.IsNullOrEmpty(strURL))
        //                        {
        //                            return View("../Gateway/AtomCheckoutUrl", new AtomViewModel(strURL));
        //                        }
        //                        else
        //                        {
        //                            ViewData["result"] = -10;
        //                            return View(PFVMSession);
        //                        }
        //                    }
        //                    #endregion Payment Gateyway
        //                }
        //                else
        //                {

        //                    //Your Challan no. XXXXXXXXXX of Lot no  XX successfully generated and valid till Dt XXXXXXXXXXX. Regards PSEB
        //                    string Sms = "Your Challan no. " + result + " of Lot no  " + CM.LOT + " successfully generated and valid till Dt " + CM.CHLNVDATE + ". Regards PSEB";
        //                    try
        //                    {
        //                        string getSms = DBClass.gosms(SchoolMobile, Sms);
        //                        // string getSms = DBClass.gosms("9711819184", Sms);
        //                    }
        //                    catch (Exception) { }

        //                    ModelState.Clear();
        //                    //--For Showing Message---------//                   
        //                    return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });

        //                }

        //            }

        //            TempData["CalculateFee"] = null;
        //            TempData["AllowBanks"] = null;
        //            TempData["PaymentForm"] = null;
        //            TempData["FeeStudentList"] = null;

        //        }
        //        return View(PFVMSession);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewData["Error"] = ex.Message;
        //        return View(pfvm);
        //    }
        //}

        //#endregion
    }
}