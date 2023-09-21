using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CCA.Util;
using PsebJunior.Models;
using PsebPrimaryMiddle.Filters;
using PSEBONLINE.Models;
using PsebPrimaryMiddle.Controllers;
using PsebPrimaryMiddle.Repository;
using PsebJunior.AbstractLayer;

namespace PSEBONLINE.Controllers
{
    [SessionCheckFilter]
    public class OnDemandCertificateController : Controller
    {
        private DBContext _context = new DBContext();
        // GET: OnDemandCertificate

        private readonly IBankRepository _bankRepository;
        private readonly IChallanRepository _challanRepository;
        private readonly ISchoolRepository _schoolRepository;
        public OnDemandCertificateController(IBankRepository bankRepository, IChallanRepository challanRepository, ISchoolRepository schoolRepository)
        {
            _bankRepository = bankRepository;
            _challanRepository = challanRepository;
            _schoolRepository = schoolRepository;
        }



        public ActionResult ViewStudentList(string id, OnDemandCertificateModelList onDemandCertificateSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("OnDemandCertificate", "RegistrationPortal");
                }
                string Search = "", RP = "", cls = "";
                ViewBag.id = id;
              
                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();
 
               
                switch (id)
                {
                    //case "S":
                    //    RP = "R";
                    //     cls = "4";
                    //    break;
                    //case "SO":
                    //    RP = "O";
                    //    cls = "4";
                    //    break;
                    //case "M":
                    //    RP = "R";
                    //    cls = "2";
                    //    break;
                    //case "MO":
                    //    RP = "O";
                    //    cls = "2";
                    //    break;
                    case "Primary":
                        RP = "R";
                        cls = "5";
                        break;
                    case "Middle":
                        RP = "R";
                        cls = "8";
                        break;
                    default:
                        RP = ""; 
                        cls = "";
                        break;
                }
                ViewBag.RP = RP;
                ViewBag.cls = cls;

                //Search,
                DataSet dsOut = new DataSet();               
                onDemandCertificateSearchModel.OnDemandCertificateSearchModel = AbstractLayer.OnDemandCertificateDB.GetOnDemandCertificateStudentList("GET",RP, cls, SCHL, Search, out dsOut);
                onDemandCertificateSearchModel.StoreAllData = dsOut;
                return View(onDemandCertificateSearchModel);       
            }
            catch (Exception ex)
            {               
                return View(id);
            }
        }
     
        public JsonResult JqOnDemandCertificateApplyStudents(string studentlist,string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
          
            int result = 0;
            if (!string.IsNullOrEmpty(studentlist))
            {
                studentlist = studentlist.Remove(studentlist.Length - 1);


                List<int> listComma = studentlist.Split(',').Select(int.Parse).ToList();
                List<OnDemandCertificates> list = new List<OnDemandCertificates>();

                foreach (var stdid in listComma)
                {
                    list.Add(new OnDemandCertificates { DemandId=0, Std_id = stdid, Schl = loginSession.SCHL, Cls = Convert.ToInt32(cls),IsActive=1,IsPrinted=0,SubmitOn=DateTime.Now });
                }

                if (list.Count() > 0)
                {
                    result = new AbstractLayer.OnDemandCertificateDB().InsertOnDemandCertificateStudentList(list);
                }
                else
                {
                    result = -1;
                }
            }
                return Json(new { Result = result }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult OnDemandCertificateAppliedStudentList(string id, OnDemandCertificateModelList onDemandCertificateSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("OnDemandCertificate", "RegistrationPortal");
                }
                string Search = "", RP = "", cls = "";
                ViewBag.id = id;

                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();


                switch (id)
                {
                //    case "S":
                //        RP = "R";
                //        cls = "4";
                //        break;
                //    case "SO":
                //        RP = "O";
                //        cls = "4";
                //        break;
                //    case "M":
                //        RP = "R";
                //        cls = "2";
                //        break;
                //    case "MO":
                //        RP = "O";
                //        cls = "2";
                //        break;
                    case "Primary":
                        RP = "R";
                        cls = "5";
                        break;
                    case "Middle":
                        RP = "R";
                        cls = "8";
                        break;
                    default:                   
                        RP = "";
                        cls = "";
                        break;
                }
                ViewBag.RP = RP;
                ViewBag.cls = cls;

                //Search,
                DataSet dsOut = new DataSet();
                onDemandCertificateSearchModel.OnDemandCertificateSearchModel = AbstractLayer.OnDemandCertificateDB.GetOnDemandCertificateStudentList("ADDED", RP, cls, SCHL, Search, out dsOut);
                onDemandCertificateSearchModel.StoreAllData = dsOut;
                return View(onDemandCertificateSearchModel);
            }
            catch (Exception ex)
            {
                return View(id);
            }
        }


        public JsonResult JqRemoveOnDemandCertificateApplyStudents(string demandIdList, string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            int result = 0;
            if (!string.IsNullOrEmpty(demandIdList))
            {
                demandIdList = demandIdList.Remove(demandIdList.Length - 1);


                List<int> listComma = demandIdList.Split(',').Select(int.Parse).ToList();
                List<OnDemandCertificates> list = new List<OnDemandCertificates>();
          
                foreach (var did in listComma)
                {
                    list.Add(new OnDemandCertificates { DemandId = did });
                }

                if (list.Count() > 0)
                {
                    result = new AbstractLayer.OnDemandCertificateDB().RemoveRangeOnDemandCertificateStudentList(list);
                }
                else
                {
                    result = -1;
                }
            }
            return Json(new { Result = result }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult OnDemandCertificate_ChallanList(OnDemandCertificate_ChallanDetailsViewsModelList obj)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];               
                DataSet dsOut = new DataSet();
                obj.OnDemandCertificate_ChallanDetailsViews = AbstractLayer.OnDemandCertificateDB.OnDemandCertificate_ChallanListMiddlePrimary(loginSession.SCHL, out dsOut);
                obj.StoreAllData = dsOut;
                return View(obj);               
            }
            catch (Exception ex)
            {               
                return View();
            }
        }


        #region Calculate Fee
        [SessionCheckFilter]
        public ActionResult OnDemandCertificateCalculateFee(string id, string D)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {
                Session["OnDemandCertificateCalculateFee"] = null;
                FeeHomeViewModel fhvm = new FeeHomeViewModel();

                string Search = string.Empty;
                Search = "SCHL=" + loginSession.SCHL.ToString();
                DataSet ds = new DataSet();
                ds = AbstractLayer.OnDemandCertificateDB.OnDemandCertificatesCountRecordsClassWise(Search, loginSession.SCHL.ToString());
                if ((ds == null) || (ds.Tables[0].Rows.Count == 0 && ds.Tables[1].Rows.Count == 0))
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.M8R = ViewBag.F5R = ViewBag.SR = ViewBag.MR = ViewBag.TotalCount = 0;           
                    return View(fhvm);
                }
                else
                {
                    ViewBag.MR = ds.Tables[0].Rows[0]["MR"].ToString();
                    ViewBag.SR = ds.Tables[1].Rows[0]["SR"].ToString();
                    ViewBag.F5R = ds.Tables[2].Rows[0]["F5R"].ToString();
                    ViewBag.M8R = ds.Tables[3].Rows[0]["M8R"].ToString();
                }

                ViewBag.SearchId = id;
                string date = DateTime.Now.ToString("dd/MM/yyyy");
                if (!string.IsNullOrEmpty(id))
                {
                    fhvm.StoreAllData = AbstractLayer.OnDemandCertificateDB.OnDemandCertificateCalculateFee(id, date, Search, loginSession.SCHL);

                    if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        ViewData["FeeStatus"] = "3";
                    }
                    else
                    {
                        ViewData["FeeStatus"] = "1";
                        if (Session["OnDemandCertificateCalculateFee"] != null)
                        {
                            Session["OnDemandCertificateCalculateFee"] = null;
                        }

                        Session["OnDemandCertificateCalculateFee"] = fhvm.StoreAllData;
                        ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
                        fhvm.TotalFeesInWords = fhvm.StoreAllData.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
                        fhvm.EndDate = fhvm.StoreAllData.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + fhvm.StoreAllData.Tables[0].Rows[0]["FeeValidDate"].ToString();
                    }
                }
                else {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewData["FeeStatus"] = "5";
                }
                return View(fhvm);
            }
            catch (Exception ex)
            {              
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult OnDemandCertificatePaymentForm()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {

                PaymentformViewModel pfvm = new PaymentformViewModel();
                if (Session["OnDemandCertificateCalculateFee"] == null || Session["OnDemandCertificateCalculateFee"].ToString() == "")
                {
                    return RedirectToAction("OnDemandCertificateCalculateFee", " OnDemandCertificate");
                }

                // ViewBag.BankList = objCommon.GetBankList();
                string schl = loginSession.SCHL;

                pfvm.LOTNo = Convert.ToInt32(1);
                pfvm.Dist = loginSession.DIST.ToString();
                pfvm.District = loginSession.DIST.ToString(); ;
                pfvm.DistrictFull = loginSession.DIST.ToString(); 
                pfvm.SchoolCode = loginSession.SCHL.ToString();
                pfvm.SchoolName = loginSession.SCHLNME.ToString();
                ViewBag.TotalCount = 1;

                DataSet dscalFee = (DataSet)Session["OnDemandCertificateCalculateFee"];
                pfvm.TotalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFee"].ToString());
                pfvm.TotalLateFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalLateFee"].ToString());
                pfvm.TotalFinalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFeeAmount"].ToString());
                pfvm.TotalFeesInWords = dscalFee.Tables[1].Rows[0]["TotalFeesWords"].ToString();
                //  pfvm.FeeDate = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy");
                pfvm.FeeDate = dscalFee.Tables[0].Rows[0]["FeeValidDateFormat"].ToString();
                pfvm.OfflineLastDate = dscalFee.Tables[0].Rows[0]["OfflineLastDate"].ToString();
                pfvm.StartDate = dscalFee.Tables[0].Rows[0]["FeeStartDate"].ToString();
                //TotalCandidates
                pfvm.TotalCandidates = Convert.ToInt32(dscalFee.Tables[0].AsEnumerable().Sum(x => x.Field<int>("CountStudents")));
                pfvm.FeeCode = dscalFee.Tables[0].Rows[0]["FeeCode"].ToString();
                pfvm.FeeCategory = dscalFee.Tables[0].Rows[0]["FEECAT"].ToString();
                Session["OnDemandCertificatePaymentForm"] = pfvm;

                // new add AllowBanks by rohit
                pfvm.AllowBanks = dscalFee.Tables[0].Rows[0]["AllowBanks"].ToString();
                string[] bls = pfvm.AllowBanks.Split(',');
                // BankList bl = new BankList();
                BankModels BM = new BankModels();
                pfvm.bankList = new List<BankListModel>();
                for (int b = 0; b < bls.Count(); b++)
                {
                    int OutStatus;
                    BM.BCODE = bls[b].ToString();
                    DataSet ds1 = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
                    BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
                    pfvm.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
                }
                ///////////////



                if (pfvm.TotalFinalFees == 0 && pfvm.TotalFees == 0)
                {
                    ViewBag.CheckForm = 1; // only verify for M1 and T1 
                    TempData["OnDemandCertificateCheckFormFee"] = 0;
                }
                else
                {
                    ViewBag.CheckForm = 0; // only verify for M1 and T1 
                   TempData["OnDemandCertificateCheckFormFee"] = 1;
                }
                return View(pfvm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("OnDemandCertificateCalculateFee", " OnDemandCertificate");
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult OnDemandCertificatePaymentForm(PaymentformViewModel pfvm, FormCollection frm, string PayModValue, string AllowBanks, string IsOnline)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {
                ChallanMasterModel CM = new ChallanMasterModel();
                if (AllowBanks == null)
                {
                    AllowBanks = pfvm.BankCode;
                }
                else
                {
                    pfvm.BankCode = AllowBanks;
                }

                if (pfvm.BankCode == null)
                {
                    ViewBag.Message = "Please Select Bank";
                    ViewData["SelectBank"] = "1";
                    return View(pfvm);
                }
                //if (Session["OnDemandCertificateCheckFormFee"].ToString() == "0")
                //{ pfvm.BankCode = "203"; }


                if (Session["OnDemandCertificatePaymentForm"] == null || Session["OnDemandCertificatePaymentForm"].ToString() == "")
                {
                    return RedirectToAction("OnDemandCertificatePaymentForm", "OnDemandCertificate");
                }
                if (Session["OnDemandCertificate_FeeStudentList"] == null || Session["OnDemandCertificate_FeeStudentList"].ToString() == "")
                {
                    return RedirectToAction("OnDemandCertificatePaymentForm", "OnDemandCertificate");
                }

                string bankName = "";

                if (AllowBanks == "301" || AllowBanks == "302")
                {
                    PayModValue = "online";
                    if (AllowBanks == "301")
                    {
                        bankName = "HDFC Bank";
                    }
                    else if (AllowBanks == "302")
                    {
                        bankName = "Punjab And Sind Bank";
                    }
                    CM.FEEMODE = "ONLINE";
                }
                else if (AllowBanks == "203")
                {
                    PayModValue = "hod";
                    bankName = "PSEB HOD";
                    CM.FEEMODE = "CASH";
                }
                else if (AllowBanks == "202" || AllowBanks == "204")
                {

                    PayModValue = "offline";
                    if (AllowBanks == "202")
                    {
                        bankName = "Punjab National Bank";
                    }
                    else if (AllowBanks == "204")
                    {
                        bankName = "State Bank of India";
                    }
                    CM.FEEMODE = "CASH";
                }
                pfvm.BankName = bankName;


                if (ModelState.IsValid)
                {
                    string SCHL = loginSession.SCHL;
                    string FeeStudentList = Session["OnDemandCertificate_FeeStudentList"].ToString();
                    CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);
                    PaymentformViewModel PFVMSession = (PaymentformViewModel)Session["OnDemandCertificatePaymentForm"];
                    // new add AllowBanks by rohit
                    //pfvm.AllowBanks = dscalFee.Tables[0].Rows[0]["AllowBanks"].ToString();
                    string[] bls = PFVMSession.AllowBanks.Split(',');
                    // BankList bl = new BankList();
                    BankModels BM = new BankModels();
                    PFVMSession.bankList = new List<BankListModel>();
                    for (int b = 0; b < bls.Count(); b++)
                    {
                        int OutStatus;
                        BM.BCODE = bls[b].ToString();
                        DataSet ds1 = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
                        BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
                        PFVMSession.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
                    }
                    ///////////////


                    CM.FEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                    CM.TOTFEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                    CM.regfee = Convert.ToInt32(PFVMSession.TotalFees);
                    CM.latefee = Convert.ToInt32(PFVMSession.TotalLateFees);
                    CM.FEECAT = PFVMSession.FeeCategory;
                    CM.FEECODE = PFVMSession.FeeCode;
                    //CM.FEEMODE = "CASH";
                    CM.BANK = pfvm.BankName;
                    CM.BCODE = pfvm.BankCode;
                    CM.BANKCHRG = PFVMSession.BankCharges;
                    CM.SchoolCode = PFVMSession.SchoolCode.ToString();
                    CM.DIST = PFVMSession.Dist.ToString();
                    CM.DISTNM = PFVMSession.District;
                    CM.LOT = PFVMSession.LOTNo;
                    //
                    CM.SCHLREGID = PFVMSession.SchoolCode.ToString();
                    CM.APPNO = PFVMSession.SchoolCode.ToString();
                    //
                    CM.type = "schle";
                    DateTime CHLNVDATE2;
                    CM.CHLNVDATE = PFVMSession.FeeDate;
                    if (DateTime.TryParseExact(PFVMSession.FeeDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                    {
                        CM.ChallanVDateN = CHLNVDATE2;
                    }


                    if (AllowBanks == "202" || AllowBanks == "204")
                    {
                        if (Convert.ToDateTime(PFVMSession.OfflineLastDate).Date >= DateTime.Now.Date)
                        {
                            //  $("#divOffline").show();
                        }
                        else
                        {
                            ViewData["result"] = 20;
                            return RedirectToAction("OnDemandCertificatePaymentForm", "OnDemandCertificate");
                        }
                    }

                    string SchoolMobile = "";
                    // string result = "0";
                    string result = _challanRepository.InsertPaymentForm(CM,  out SchoolMobile);
                    if (result == null || result == "0")
                    {
                        //--------------Not saved
                        ViewData["result"] = 0;
                        ViewData["Error"] = SchoolMobile;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = -1;
                    }
                    else
                    {
                        ViewBag.ChallanNo = result;
                        string paymenttype = CM.BCODE;
                        string TotfeePG = (CM.TOTFEE).ToString();

                        if (PayModValue.ToString().ToLower().Trim() == "online" && result.ToString().Length > 10)
                        {
                            #region Payment Gateyway

                            if (paymenttype.ToUpper() == "301" && ViewBag.ChallanNo != "") /*HDFC*/
                            {
                                string AccessCode = ConfigurationManager.AppSettings["CcAvenueAccessCode"];
                                string CheckoutUrl = ConfigurationManager.AppSettings["CcAvenueCheckoutUrl"];
                                string WorkingKey = ConfigurationManager.AppSettings["CcAvenueWorkingKey"];
                                //******************
                                string invoiceNumber = ViewBag.ChallanNo;
                                string amount = TotfeePG;

                                //***************
                                var queryParameter = new CCACrypto();

                                string strURL = GatewayController.BuildCcAvenueRequestParameters(invoiceNumber, amount);

                                return View("../Gateway/CcAvenue", new CcAvenueViewModel(queryParameter.Encrypt
                                           (strURL, WorkingKey), AccessCode, CheckoutUrl));

                            }
                            else if (paymenttype.ToUpper() == "302" && ViewBag.ChallanNo != "")/*ATOM*/
                            {

                                string TransactionID = encrypt.QueryStringModule.Encrypt(ViewBag.ChallanNo);
                                string TransactionAmount = encrypt.QueryStringModule.Encrypt(TotfeePG);
                                string clientCode = CM.APPNO;
                                // User Details
                                string udf1CustName = encrypt.QueryStringModule.Encrypt(CM.SCHLREGID); // roll number
                                string udf2CustEmail = CM.FEECAT; /// Kindly submit Appno/Refno in client id, Fee cat in Emailid (ATOM)
                                string udf3CustMob = encrypt.QueryStringModule.Encrypt(SchoolMobile);

                                //AtomCheckoutUrl(string ChallanNo, string amt, string clientCode, string cmn, string cme, string cmno)
                                return RedirectToAction("AtomCheckoutUrl", "Gateway", new { ChallanNo = TransactionID, amt = TransactionAmount, clientCode = clientCode, cmn = udf1CustName, cme = udf2CustEmail, cmno = udf3CustMob });

                            }
                            #endregion Payment Gateyway
                        }
                        else
                        {
                            ////{#var#} Challan no. {#var#} of Ref no. {#var#} successfully generated and valid till Dt {#var#}. Regards PSEB
                            string Sms = "Your Challan no. " + result + " of Ref no  " + CM.LOT + " successfully generated and valid till Dt " + CM.CHLNVDATE + ". Regards PSEB";
                            try
                            {
                                string getSms = DBClass.gosms(SchoolMobile, Sms);
                                //string getSms = objCommon.gosms("9711819184", Sms);
                            }
                            catch (Exception) { }

                            ModelState.Clear();
                            //--For Showing Message---------//                   
                            return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });
                        }
                    }
                }            
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;              
            }
            return RedirectToAction("OnDemandCertificatePaymentForm", "OnDemandCertificate");
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}