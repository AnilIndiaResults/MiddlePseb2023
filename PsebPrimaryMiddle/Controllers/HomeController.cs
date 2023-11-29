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
using System.Threading.Tasks;
using System.Data.Entity;

namespace PsebPrimaryMiddle.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBContext _context;
        private readonly ISchoolRepository _schoolRepository;
        private readonly IBankRepository _bankRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IChallanRepository _challanRepository;

        public HomeController(IChallanRepository challanRepository, ISchoolRepository schoolRepository, IBankRepository bankRepository, IAdminRepository adminRepository)
        {
            _context = new DBContext();
            _challanRepository = challanRepository;
            _schoolRepository = schoolRepository;
            _bankRepository = bankRepository;
            _adminRepository = adminRepository;
        }



        [SessionCheckFilter]
        public ActionResult Index(int? page)
        {
            Printlist obj = new Printlist();
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                ViewBag.Session = loginSession.CurrentSession;
                ViewBag.Middle = loginSession.middle;
                ViewBag.Primary = loginSession.fifth;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;


                #region Circular

                string Search = string.Empty;
                Search = "Id like '%' and   CircularTypeName like '%SCHOOL-HOME%'   and Convert(Datetime,Convert(date,ExpiryDateDD))>=Convert(Datetime,Convert(date,getdate()))";


                // Cache
                DataSet dsCircular = new DataSet();
                DataSet cacheData = HttpContext.Cache.Get("HomeCircular") as DataSet;

                if (cacheData == null)
                {
                    dsCircular = _adminRepository.CircularMaster(Search, pageIndex);
                    cacheData = dsCircular;
                    HttpContext.Cache.Insert("HomeCircular", cacheData, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);

                }
                else
                {
                    dsCircular = cacheData;
                }
                // Cache end 

                if (dsCircular == null || dsCircular.Tables[0].Rows.Count == 0)
                {
                    ViewBag.TotalCircular = 0;
                }
                else
                {
                    //var type7 = dsCircular.Tables[0].Columns[7].DataType.Name.ToString();
                    var type8 = dsCircular.Tables[0].Columns[9].DataType.Name.ToString();
                    ViewBag.TotalCircular = dsCircular.Tables[0].Rows.Count;

                    //MarQue
                    IEnumerable<DataRow> query = from order in dsCircular.Tables[0].AsEnumerable()
                                                 where order.Field<byte>("IsMarque") == 1 && order.Field<Boolean>("IsActive") == true
                                                 select order;
                    // Create a table of Marque from the query.
                    if (query.Any())
                    {
                        obj.dsMarque = query.CopyToDataTable<DataRow>();
                        ViewBag.MarqueCount = obj.dsMarque.Rows.Count;
                    }
                    else { ViewBag.MarqueCount = 0; }

                    // circular
                    IEnumerable<DataRow> query1 = from order in dsCircular.Tables[0].AsEnumerable()
                                                  where order.Field<byte>("IsMarque") == 0 && order.Field<Boolean>("IsActive") == true
                                                  select order;
                    // Create a table of Marque from the query.
                    if (query1.Any())
                    {
                        obj.dsCircular = query1.CopyToDataTable<DataRow>();
                        ViewBag.CircularCount = obj.dsCircular.Rows.Count;

                        //
                        int count = Convert.ToInt32(dsCircular.Tables[2].Rows[0]["CircularCount"]);
                        ViewBag.TotalCircularCount = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 15;
                        int cal = 15 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCircularCount) - cal;
                        if (res >= 1)
                        { ViewBag.pn = pn + 1; }
                        else
                        { ViewBag.pn = pn; }


                    }
                    else
                    {
                        ViewBag.CircularCount = 0;
                        ViewBag.TotalCircularCount = 0;
                    }
                }
                #endregion


                DataSet ds = new DataSet();
                SchoolModels sm = _schoolRepository.GetSchoolDataBySchl(loginSession.SCHL, out ds);
                obj.numofprimary = sm.NoOfPrimary;
                obj.numofmiddle = sm.NoOfMiddle;
                Session["IsAffiliation"] = ds.Tables[0].Rows[0]["IsAffiliationMiddle"].ToString() == "1" ? "1" : null;

            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["result"] = "ERR";
                ViewData["resultMsg"] = ex.Message;
            }
            return View(obj);
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult undertaking(Printlist obj)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string outstatus = string.Empty;
            _schoolRepository.InsertUndertaking(loginSession.SCHL, obj.numofprimary, obj.numofmiddle, out outstatus);
            TempData["undertakingResult"] = outstatus;
            return RedirectToAction("Index", "Home", outstatus);
        }



        public ActionResult PrintUndertaking(Printlist obj)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string outstatus = string.Empty;
            obj.StoreAllData = _schoolRepository.SchoolMasterViewSP(5, loginSession.SCHL, "");
            if (obj.StoreAllData.Tables.Count > 0)
            {
                TempData["PrintUndertaking"] = "1";
            }
            return View(obj);
        }

        public ActionResult Home()
        {
            return View();
        }

        [SessionCheckFilter]
        public ActionResult DownloadMissingReport(string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                if (Request.QueryString["File"] == null)
                {
                    return RedirectToAction("CalculateFee", "Home");
                }
                else
                {
                    string UserType = "User";
                    string FileExport = Request.QueryString["File"].ToString();
                    DataSet ds = null;

                    if (loginSession.SCHL == "")
                    {
                        return RedirectToAction("Logout", "Login");
                    }
                    else
                    {
                        string fileName1 = string.Empty;
                        string Search = string.Empty;
                        Search = "SCHL=" + loginSession.SCHL;
                        Search += "  and type='" + UserType + "'";

                        if (FileExport == "Excel")
                        {
                            fileName1 = loginSession.SCHL + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                        }
                        else
                        {
                            Search += "  and form_name='" + FileExport + "'";
                            fileName1 = FileExport + '_' + loginSession.SCHL + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                        }

                        ds = _challanRepository.GetMissingCheckFeeStatusSPJunior(Search);

                        if (ds == null)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                bool ResultDownload;
                                try
                                {
                                    switch (FileExport)
                                    {
                                        case "Excel":
                                            using (XLWorkbook wb = new XLWorkbook())
                                            {
                                                wb.Worksheets.Add(ds);
                                                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                wb.Style.Font.Bold = true;
                                                Response.Clear();
                                                Response.Buffer = true;
                                                Response.Charset = "";
                                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                                Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + "");
                                                using (MemoryStream MyMemoryStream = new MemoryStream())
                                                {
                                                    wb.SaveAs(MyMemoryStream);
                                                    MyMemoryStream.WriteTo(Response.OutputStream);
                                                    Response.Flush();
                                                    Response.End();
                                                }
                                            }
                                            break;
                                        default:
                                            using (XLWorkbook wb = new XLWorkbook())
                                            {
                                                wb.Worksheets.Add(ds);
                                                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                wb.Style.Font.Bold = true;
                                                Response.Clear();
                                                Response.Buffer = true;
                                                Response.Charset = "";
                                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                                Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + "");
                                                using (MemoryStream MyMemoryStream = new MemoryStream())
                                                {
                                                    wb.SaveAs(MyMemoryStream);
                                                    MyMemoryStream.WriteTo(Response.OutputStream);
                                                    Response.Flush();
                                                    Response.End();
                                                }
                                            }
                                            break;
                                    }
                                    ResultDownload = true;
                                }
                                catch (Exception)
                                {
                                    ResultDownload = false;
                                }
                            }
                        }
                    }
                }
                return RedirectToAction("CalculateFee", "Home");
            }
            catch (Exception ex)
            {
                // ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("CalculateFee", "Home");
            }

        }



        #region Calculate Fee of Registration
        [SessionCheckFilter]
        public ActionResult CalculateFee(string id, string Status, FormCollection frc)
        {
            try
            {
                FeeHomeViewModel fhvm = new FeeHomeViewModel();
                LoginSession loginSession = (LoginSession)Session["LoginSession"];

                TempData["CalculateFee"] = null;
                TempData["AllowBanks"] = null;
                TempData["PaymentForm"] = null;
                TempData["FeeStudentList"] = null;
                ViewBag.selectedClass = "";

                if (loginSession.GovFlag != "GO")
                {
                    ViewData["FeeStatus"] = "2";
                    ViewBag.OutStatus = "2";

					//return View(fhvm);
				}
                //else
                //{
                //    return RedirectToAction("Index", "Home");
                //}
				return View(fhvm);

			}
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }

        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CalculateFee(string id, string Status, FormCollection frc, string aa, string ChkId, string selectedClass)
        {
            try
            {
                FeeHomeViewModel fhvm = new FeeHomeViewModel();
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                // ViewBag.Eighth = loginSession.middle == "Y" ? "1" : "0";
                /// ViewBag.Fifth = loginSession.fifth == "Y" ? "1" : "0";

                if (Status == "Successfully" || Status == "Failure")
                {
                    //ViewData["Status"] = Status;
                    ViewData["FeeStatus"] = Status;
                    return View();
                }
                else
                {
                    ViewBag.selectedClass = selectedClass;
                    // FormCollection frc = new FormCollection();
                    string FormNM = frc["ChkId"];
                    if (FormNM == null || FormNM == "")
                    {
                        return View();
                    }



                    ViewData["Status"] = "";
                    string UserType = "User";


                    string Search = string.Empty;
                    Search = "SCHL='" + loginSession.SCHL + "'";
                    FormNM = "'" + FormNM.Replace(",", "','") + "'";
                    Search += "  and type='" + UserType + "' and form_name in(" + FormNM + ")";

                    string SearchString = DateTime.Now.ToString("dd/MM/yyyy");
                    DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);


                    //string SearchString = DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy");
                    //DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);


                    //*******for Testing Start by Rohit

                    // Change after  
                    //DataSet ds = _challanRepository.GetCalculateFeeBySchool(Search, loginSession.SCHL, date);
                    //fhvm.StoreAllData = ds;
                    //if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
                    //{
                    //    ViewBag.Message = "Record Not Found";
                    //    ViewBag.TotalCount = 0;
                    //    ViewData["FeeStatus"] = "3";
                    //    return View();
                    //}
                    //else
                    //{
                    //    ViewData["FeeStatus"] = "1";
                    //    //Session["CalculateFee"] = ds;
                    //    TempData["CalculateFee"] = ds;
                    //    ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
                    //    //Session["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
                    //    TempData["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
                    //    fhvm.TotalFeesInWords = ds.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
                    //    fhvm.EndDate = ds.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + ds.Tables[0].Rows[0]["FeeValidDate"].ToString();                      
                    //}

                    //*******for Testing End


                    /// for Live

                    DataSet dsCheckFee = _challanRepository.CheckFeeStatus(loginSession.SCHL, UserType, FormNM.ToUpper(), date);//CheckFeeStatusSPByView

                    //CheckFeeStatus
                    if (dsCheckFee == null)
                    {
                        // return RedirectToAction("Index", "Home");
                        ViewData["FeeStatus"] = "11";
                        return View();
                    }
                    else
                    {
                        if (dsCheckFee.Tables[0].Rows.Count > 0)
                        {
                            if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "0")
                            {
                                //Not Exist (if lot '0' not xist)
                                ViewData["FeeStatus"] = "0";
                                ViewBag.Message = "All Fees are submmited/ Data Not Available for Fee Calculation...";
                                ViewBag.TotalCount = 0;
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "2")
                            {
                                //Not Allowed Some Form are not in Fee
                                ViewData["FeeStatus"] = "2";
                                // ViewBag.Message = "Not Allowed,Some FORM are not in Fee Structure..please contact Punjab School Education Board";
                                // ViewBag.Message = "Calculate fee is allowed only for M1 and T1 Form only";
                                DataSet ds = dsCheckFee;
                                fhvm.StoreAllData = dsCheckFee;
                                ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
                                // return View(fhvm);
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "3" || dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "5")
                            {
                                int CountTable = dsCheckFee.Tables.Count;
                                ViewBag.CountTable = CountTable;
                                ViewBag.OutStatus = dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString();
                                //Not Allowed Some Form are not in Fee
                                ViewData["FeeStatus"] = "2";
                                // ViewBag.Message = "Not Allowed, Some Mandatory Fields are not Filled.";
                                // ViewBag.Message = "Some Mandatory Fields Like Subject's/Photograph's,Signature's of Listed Form wise Candidate's are Missing or Duplicate Records. Please Update These Details Then Try Again to Calculate Fee & Final Submission";
                                DataSet ds = dsCheckFee;
                                fhvm.StoreAllData = dsCheckFee;
                                if (CountTable > 1)
                                {
                                    ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
                                    if (dsCheckFee.Tables[1].Rows.Count > 0)
                                    {
                                        if (dsCheckFee.Tables[1].Rows[0]["Outstatus"].ToString() == "5")
                                        {
                                            ViewBag.TotalCountDuplicate = dsCheckFee.Tables[1].Rows.Count;
                                        }
                                        else
                                        { ViewBag.TotalCountDuplicate = 0; }
                                    }
                                    else
                                    { ViewBag.TotalCountDuplicate = 0; }

                                }

                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "1" && !string.IsNullOrEmpty(selectedClass))
                            {
                                string cls = selectedClass;
                                DataSet ds = _challanRepository.GetCalculateFeeBySchool(cls, Search, loginSession.SCHL, date);
                                fhvm.StoreAllData = ds;
                                if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
                                {
                                    ViewBag.Message = "Record Not Found";
                                    ViewBag.TotalCount = 0;
                                    ViewData["FeeStatus"] = "3";
                                    return View();
                                }
                                else
                                {
                                    ViewData["FeeStatus"] = "1";
                                    TempData["CalculateFee"] = ds;
                                    ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
                                    TempData["AllowBanks"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
                                    fhvm.TotalFeesInWords = ds.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
                                    fhvm.EndDate = ds.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + ds.Tables[0].Rows[0]["FeeValidDate"].ToString();
                                }
                            }
                        }
                        else
                        {
                            //return RedirectToAction("Index", "Home");
                            ViewData["FeeStatus"] = "22";
                            return View();
                        }
                    }
                    return View(fhvm);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }

        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PaymentForm()
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                PaymentformViewModel pfvm = new PaymentformViewModel();

                if (TempData["CalculateFee"] == null || TempData["CalculateFee"].ToString() == "")
                {
                    return RedirectToAction("CalculateFee", "Home");
                }

                string schl = loginSession.SCHL;
                DataSet ds = _challanRepository.GetSchoolLotDetails(schl);
                pfvm.PaymentFormData = ds;
                if (pfvm.PaymentFormData == null || pfvm.PaymentFormData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    pfvm.LOTNo = Convert.ToInt32(ds.Tables[0].Rows[0]["LOT"].ToString());
                    pfvm.Dist = ds.Tables[0].Rows[0]["Dist"].ToString();
                    pfvm.District = ds.Tables[0].Rows[0]["diste"].ToString();
                    pfvm.DistrictFull = ds.Tables[0].Rows[0]["DistrictFull"].ToString();
                    //pfvm.SchoolCode = Convert.ToInt32(ds.Tables[0].Rows[0]["schl"].ToString());
                    pfvm.SchoolCode = ds.Tables[0].Rows[0]["schl"].ToString();
                    pfvm.SchoolName = ds.Tables[0].Rows[0]["SchoolFull"].ToString(); // Schollname with station and dist 
                    ViewBag.TotalCount = pfvm.PaymentFormData.Tables[0].Rows.Count;

                    DataSet dscalFee = (DataSet)TempData["CalculateFee"];

                    pfvm.totaddsubfee = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["HardCopyCertificateFee"].ToString());/// TOTAL  CERT FEE
                    pfvm.totaddfee = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalExamFee"].ToString());/// EXAM FEE
                    pfvm.TotalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFee"].ToString()); // REG FEE
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
                    TempData["PaymentForm"] = pfvm;

                    if (Convert.ToDateTime(pfvm.OfflineLastDate).Date >= DateTime.Now.Date)
                    {

                        ViewData["IsOfflineAllow"] = "1";

                    }
                    else
                    {
                        ViewData["IsOfflineAllow"] = "0";
                    }

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


                    //string CheckFee = ds.Tables[1].Rows[0]["TotalFeeAmount"].ToString();
                    //if (pfvm.TotalFinalFees == 0 && pfvm.TotalFees == 0)
                    //{
                    //    ViewBag.CheckForm = 1; // only verify for M1 and T1 
                    //    Session["CheckFormFee"] = 0;
                    //}
                    //else
                    //{
                    //    ViewBag.CheckForm = 0; // only verify for M1 and T1 
                    //    Session["CheckFormFee"] = 1;
                    //}

                    if (pfvm.TotalFinalFees == 0)
                    {
                        ChallanMasterModel CM = new ChallanMasterModel();
                        string AllowBanks = "203";
                        pfvm.BankCode = AllowBanks;
                        if (AllowBanks == "203")
                        {

                            string PayModValue = "hod";
                            string bankName = "PSEB HOD";
                            CM.FEEMODE = "CASH";

                            #region HOD Payment


                            pfvm.BankName = bankName;

                            string BankCode = pfvm.BankCode;
                            PaymentformViewModel PFVMSession = (PaymentformViewModel)TempData["PaymentForm"];
                            if (ModelState.IsValid)
                            {
                                string SCHL = loginSession.SCHL;
                                string FeeStudentList = TempData["FeeStudentList"].ToString();
                                CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);
                                CM.FEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                                CM.TOTFEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                                CM.addfee = Convert.ToInt32(PFVMSession.totaddfee);// exam fee
                                CM.regfee = Convert.ToInt32(PFVMSession.TotalFees);
                                CM.latefee = Convert.ToInt32(PFVMSession.TotalLateFees);
                                CM.addsubfee = Convert.ToInt32(PFVMSession.totaddsubfee);// cert fee
                                CM.FEECAT = PFVMSession.FeeCategory;
                                CM.FEECODE = PFVMSession.FeeCode;
                                //  CM.FEEMODE = "ONLINE";
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
                                if (DateTime.TryParseExact(PFVMSession.FeeDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                                {
                                    CM.ChallanVDateN = CHLNVDATE2;
                                }
                                CM.CHLNVDATE = PFVMSession.FeeDate;
                                string SchoolMobile = "";
                                // string result = "0";
                                string result = _challanRepository.InsertPaymentFormJunior(CM, out SchoolMobile);
                                if (result == null || result == "0")
                                {
                                    //--------------Not saved
                                    ViewData["resultHOD"] = 0;
                                    ViewData["Error"] = SchoolMobile;
                                }
                                else if (result == "-1")
                                {
                                    //-----alredy exist
                                    ViewData["resultHOD"] = -1;
                                }
                                else
                                {
                                    ViewData["resultHOD"] = 1;
                                    ViewBag.ChallanNo = result;

                                    string Sms = "Your Challan no. " + result + " of Lot no  " + CM.LOT + " successfully generated and valid till Dt " + CM.CHLNVDATE + ". Regards PSEB";
                                    try
                                    {
                                        string getSms = DBClass.gosms(SchoolMobile, Sms);
                                        //string getSms = DBClass.gosms("9711819184", Sms);
                                    }
                                    catch (Exception) { }

                                    return RedirectToAction("FinalPrint", "Home");

                                }

                                return RedirectToAction("CalculateFee", "Home");
                                #endregion

                            }
                        }

                    }
                    return View(pfvm);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PaymentForm(PaymentformViewModel pfvm, FormCollection frm, string PayModValue, string AllowBanks)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];


                ChallanMasterModel CM = new ChallanMasterModel();
                if (pfvm.BankCode == null)
                {
                    ViewBag.Message = "Please Select Bank";
                    ViewData["SelectBank"] = "1";
                    return View(pfvm);
                }


                if (AllowBanks == null)
                {
                    AllowBanks = pfvm.BankCode;
                }
                else
                {
                    pfvm.BankCode = AllowBanks;
                }
                //pfvm.BankCode = AllowBanks;


                if (TempData["PaymentForm"] == null || TempData["PaymentForm"].ToString() == "")
                {
                    return RedirectToAction("PaymentForm", "Home");
                }
                if (TempData["FeeStudentList"] == null || TempData["FeeStudentList"].ToString() == "")
                {
                    return RedirectToAction("PaymentForm", "Home");
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

                string BankCode = pfvm.BankCode;
                PaymentformViewModel PFVMSession = (PaymentformViewModel)TempData["PaymentForm"];
                if (ModelState.IsValid)
                {
                    string SCHL = loginSession.SCHL;
                    string FeeStudentList = TempData["FeeStudentList"].ToString();
                    CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);
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
                        // DataSet ds1 = new AbstractLayer.BankDB().GetBankDataByBCODE(BM, out OutStatus);
                        DataSet ds1 = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
                        BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
                        PFVMSession.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
                    }
                    ///////////////

                    CM.FEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                    CM.TOTFEE = Convert.ToInt32(PFVMSession.TotalFinalFees);
                    CM.addfee = Convert.ToInt32(PFVMSession.totaddfee);// exam fee
                    CM.regfee = Convert.ToInt32(PFVMSession.TotalFees);
                    CM.latefee = Convert.ToInt32(PFVMSession.TotalLateFees);
                    CM.addsubfee = Convert.ToInt32(PFVMSession.totaddsubfee);// cert fee
                    CM.FEECAT = PFVMSession.FeeCategory;
                    CM.FEECODE = PFVMSession.FeeCode;
                    //  CM.FEEMODE = "ONLINE";
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
                    if (DateTime.TryParseExact(PFVMSession.FeeDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                    {
                        CM.ChallanVDateN = CHLNVDATE2;
                    }
                    CM.CHLNVDATE = PFVMSession.FeeDate;
                    string SchoolMobile = "";
                    // string result = "0";
                    string result = _challanRepository.InsertPaymentFormJunior(CM, out SchoolMobile);
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

                        if (PayModValue.ToString().ToLower().Trim() == "online")
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
                            string Sms = "Your Challan no. " + result + " of Lot no  " + CM.LOT + " successfully generated and valid till Dt " + CM.CHLNVDATE + ". Regards PSEB";
                            try
                            {
                                string getSms = DBClass.gosms(SchoolMobile, Sms);
                                //string getSms = DBClass.gosms("9711819184", Sms);
                            }
                            catch (Exception) { }

                            ModelState.Clear();
                            //--For Showing Message---------//                   
                            return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });
                        }
                    }
                }
                return RedirectToAction("PaymentForm", "Home");
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                //return View(pfvm);
                return RedirectToAction("PaymentForm", "Home");
            }
        }

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
        //        else {
        //            PayModValue = "offline";
        //        }


        //        if (TempData["PaymentForm"] == null || TempData["PaymentForm"].ToString() == "")
        //        {
        //            return RedirectToAction("PaymentForm", "Home");
        //        }
        //        if (TempData["FeeStudentList"] == null || TempData["FeeStudentList"].ToString() == "")
        //        {
        //            return RedirectToAction("PaymentForm", "Home");
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
        //                         string getSms = DBClass.gosms(SchoolMobile, Sms);
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

        public ActionResult GenerateChallaan(string ChallanId)
        {
            try
            {
                //LoginSession loginSession = (LoginSession)Session["LoginSession"];
                if (ChallanId == null || ChallanId == "0")
                {
                    return RedirectToAction("Index", "Home");
                }
                ChallanMasterModel CM = new ChallanMasterModel();

                string schl = "";
                string ChallanId1 = ChallanId.ToString();

                DataSet ds = _challanRepository.GetChallanDetailsById(ChallanId1);
                CM.ChallanMasterData = ds;
                if (CM.ChallanMasterData == null || CM.ChallanMasterData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {

                    CM.CHALLANID = ds.Tables[0].Rows[0]["CHALLANID"].ToString();
                    CM.CHLNDATE = ds.Tables[0].Rows[0]["ChallanGDateN1"].ToString();
                    CM.CHLNVDATE = ds.Tables[0].Rows[0]["ChallanVDateN1"].ToString();
                    // CM.CHLNDATE = ds.Tables[0].Rows[0]["CHLNDATE"].ToString();
                    // CM.CHLNVDATE = ds.Tables[0].Rows[0]["CHLNVDATE"].ToString();
                    CM.FEE = float.Parse(ds.Tables[0].Rows[0]["FEE"].ToString());
                    CM.latefee = Convert.ToInt32(ds.Tables[0].Rows[0]["latefee"].ToString());
                    CM.TOTFEE = float.Parse(ds.Tables[0].Rows[0]["PaidFees"].ToString());
                    CM.FEECAT = ds.Tables[0].Rows[0]["FEECAT"].ToString();
                    CM.FEECODE = ds.Tables[0].Rows[0]["FEECODE"].ToString();
                    CM.FEEMODE = ds.Tables[0].Rows[0]["FEEMODE"].ToString();
                    CM.BANK = ds.Tables[0].Rows[0]["BANK"].ToString();
                    ViewBag.BCODE = CM.BCODE = ds.Tables[0].Rows[0]["BCODE"].ToString();
                    CM.BANKCHRG = float.Parse(ds.Tables[0].Rows[0]["BANKCHRG"].ToString());
                    CM.SchoolCode = ds.Tables[0].Rows[0]["SchoolCode"].ToString();
                    // CM.SchoolCode = Session["SCHL"].ToString();
                    CM.DIST = ds.Tables[0].Rows[0]["DIST"].ToString();
                    CM.DISTNM = ds.Tables[0].Rows[0]["DISTNM"].ToString();
                    CM.LOT = Convert.ToInt32(ds.Tables[0].Rows[0]["LOT"].ToString());
                    CM.TotalFeesInWords = ds.Tables[0].Rows[0]["TotalFeesInWords"].ToString();
                    CM.SchoolName = ds.Tables[0].Rows[0]["SchoolName"].ToString();
                    CM.DepositoryMobile = ds.Tables[0].Rows[0]["DepositoryMobile"].ToString();
                    CM.type = ds.Tables[0].Rows[0]["type"].ToString();
                    CM.APPNO = ds.Tables[0].Rows[0]["APPNO"].ToString();
                    CM.SCHLREGID = ds.Tables[0].Rows[0]["SCHLREGID"].ToString();
                    CM.SCHLCANDNM = ds.Tables[0].Rows[0]["SCHLCANDNM"].ToString();
                    if (ds.Tables[0].Rows[0]["Verified"].ToString() == "1")
                    {
                        CM.BRCODE = ds.Tables[0].Rows[0]["BRCODE"].ToString();
                        CM.BRANCH = ds.Tables[0].Rows[0]["BRANCH"].ToString();
                        CM.J_REF_NO = ds.Tables[0].Rows[0]["J_REF_NO"].ToString();
                        CM.DEPOSITDT = ds.Tables[0].Rows[0]["DEPOSITDT"].ToString();
                    }

                    TempData["CalculateFee"] = null;
                    TempData["PaymentForm"] = null;
                    TempData["FeeStudentList"] = null;

                    return View(CM);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }
        }

        [SessionCheckFilter]
        public ActionResult FinalPrint()
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];

                ChallanMasterModel CM = new ChallanMasterModel();


                DataSet ds = _challanRepository.GetFinalPrintChallan(loginSession.SCHL);//GetFinalPrintChallanSP
                CM.ChallanMasterData = ds;
                if (CM.ChallanMasterData == null || CM.ChallanMasterData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = CM.ChallanMasterData.Tables[0].Rows.Count;
                    return View(CM);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        public ActionResult FinalReport(string schl, string lot)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="7",Name="Regno"},}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            FeeHomeViewModel FM = new FeeHomeViewModel();
            FM.StoreAllData = RegistrationDB.GetStudentFinalPrint5th8thSP(schl, lot);
            TempData["FinalPrintFormName"] = loginSession.SCHL + "-" + lot;
            if (FM.StoreAllData == null || FM.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
            }
            else
            {


                ViewBag.TotalCount = FM.StoreAllData.Tables[0].Rows.Count;
                if (FM.StoreAllData.Tables[1].Rows.Count > 0)
                {
                    ViewBag.SCHL = FM.StoreAllData.Tables[1].Rows[0]["SCHL"];
                    ViewBag.IDNO = FM.StoreAllData.Tables[1].Rows[0]["IDNO"];
                    ViewBag.SSE = FM.StoreAllData.Tables[1].Rows[0]["SchoolStationE"];
                    ViewBag.SSP = FM.StoreAllData.Tables[1].Rows[0]["SchoolStationP"];
                    ViewBag.Principal = FM.StoreAllData.Tables[1].Rows[0]["PRINCIPAL"];
                    ViewBag.phno = FM.StoreAllData.Tables[1].Rows[0]["PHONE"];
                    ViewBag.mob = FM.StoreAllData.Tables[1].Rows[0]["MOBILE"];
                    ViewBag.DC = FM.StoreAllData.Tables[1].Rows[0]["DIST"];
                    ViewBag.DN = FM.StoreAllData.Tables[1].Rows[0]["DISTE"];
                }

                if (FM.StoreAllData.Tables[2].Rows.Count > 0)
                {
                    ViewBag.table2Count = "1";
                    ViewBag.regdt = FM.StoreAllData.Tables[2].Rows[0]["regdt"].ToString();
                }
                else
                {
                    ViewBag.table2Count = "0";
                }
                if (FM.StoreAllData.Tables[3].Rows.Count > 0)
                {
                    ViewBag.table3Count = "1";
                    ViewBag.bcode = FM.StoreAllData.Tables[3].Rows[0]["bcode"];
                    ViewBag.branch = FM.StoreAllData.Tables[3].Rows[0]["branch"];
                    ViewBag.Bank = FM.StoreAllData.Tables[3].Rows[0]["bank"].ToString();
                    ViewBag.amt = FM.StoreAllData.Tables[3].Rows[0]["fee"];
                    ViewBag.challanid = FM.StoreAllData.Tables[3].Rows[0]["challanid"];
                    ViewBag.depositdt = FM.StoreAllData.Tables[3].Rows[0]["DEPOSITDT"];
                    ViewBag.lot = FM.StoreAllData.Tables[3].Rows[0]["lot"];
                }
                else
                {
                    ViewBag.table3Count = "0";
                }

                return View(FM);
            }


            // return View();
        }


        [SessionCheckFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult FinalReport(string schl, string lot, string cmd, string PrintHtml)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            lot = ""; schl = "";
            string FormName = loginSession.SCHL;
            if (TempData["FinalPrintFormName"] != null)
            {
                FormName = TempData["FinalPrintFormName"].ToString();
                schl = FormName.Split('-')[0].ToString();
                lot = FormName.Split('-')[1].ToString();
            }



            if (!string.IsNullOrEmpty(cmd) && loginSession.SCHL.ToString() == schl.ToString())
            {

                string FilepathExist = Path.Combine(Server.MapPath("~/Upload/FinalPrints/"));
                if (!Directory.Exists(FilepathExist))
                {
                    Directory.CreateDirectory(FilepathExist);
                }

                if (cmd.ToLower().Contains("download") && !string.IsNullOrEmpty(PrintHtml))
                {
                    string fileNM = "FinalPrint_" + FormName + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + ".pdf";
                    string htmlToConvert = PrintHtml.ToString();
                    var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                    htmlToPdf.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                    var pdfBytes = htmlToPdf.GeneratePdf(htmlToConvert);
                    //
                    string filePath = "Upload/FinalPrints/" + fileNM;
                    string fileLocation = Server.MapPath("~/Upload/FinalPrints/" + fileNM);

                    if (System.IO.File.Exists(fileLocation))
                    {
                        try
                        {
                            System.IO.File.Delete(fileLocation);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    //file save
                    System.IO.File.WriteAllBytes(fileLocation, pdfBytes);


                    #region sent mail with attachment
                    string subject = "Download Primary-Middle Final Print";
                    string body = "<table width=" + 600 + " cellpadding=" + 4 + " cellspacing=" + 4 + " border=" + 0 + "><tr><td><b>Dear " + loginSession.SCHL + "</b>,</td></tr><tr><td><b>Final Report:-</b><br /><b>Lot Number:</b> " + lot.ToString() + "<br /></td></tr><tr><td height=" + 30 + "><b>Please Download attached final report</b> </td></tr><tr><td><b>Note:</b> Please Read Instruction Carefully Before filling the Online Form .</td></tr><tr><td>This is a system generated e-mail and please do not reply. Add <a target=_blank href=mailto:noreply@psebonline.in>noreply@psebonline.in</a> to your white list / safe sender list. Else, your mailbox filter or ISP (Internet Service Provider) may stop you from receiving e-mails.</td></tr><tr><td><b><i>Regards</b><i>,<br /> Tech Team, <br />Punjab School Education Board<br /></td></tr>";
                    //string body = "Download Primary-Middle Final Print";
                    //  string to = "rohitnanda26@gmail.com";
                    string to = loginSession.EMAILID;
                    bool sentstatus = true;
                    // bool sentstatus = DBClass.mailwithattachment(subject, body, to, fileLocation);
                    if (sentstatus)
                    {
                        FinalPrintStatus fps = new FinalPrintStatus()
                        {
                            ReportName = "FinalPrint",
                            Schl = schl,
                            Lot = lot,
                            EmailTo = to,
                            SentDate = DateTime.Now.ToString(),
                            SentStatus = "DONE",
                            CreatedBy = loginSession.SCHL,
                            FilePath = filePath
                        };

                        string upStatus = RegistrationDB.UpdateFinalPrintStatus(fps);
                    }
                    #endregion
                    System.Diagnostics.Process.Start(fileLocation);

                }
            }
            return RedirectToAction("FinalPrint", "Home");

        }


        [SessionCheckFilter]
        public ActionResult FinalReportPrint(string schl, string lot)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="7",Name="Regno"},}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            FeeHomeViewModel FM = new FeeHomeViewModel();
            FM.StoreAllData = RegistrationDB.GetStudentFinalPrint5th8thSP(schl, lot);
            TempData["FinalPrintFormName"] = loginSession.SCHL + "-" + lot;
            if (FM.StoreAllData == null || FM.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
            }
            else
            {


                ViewBag.TotalCount = FM.StoreAllData.Tables[0].Rows.Count;
                if (FM.StoreAllData.Tables[1].Rows.Count > 0)
                {
                    ViewBag.SCHL = FM.StoreAllData.Tables[1].Rows[0]["SCHL"];
                    ViewBag.IDNO = FM.StoreAllData.Tables[1].Rows[0]["IDNO"];
                    ViewBag.SSE = FM.StoreAllData.Tables[1].Rows[0]["SchoolStationE"];
                    ViewBag.SSP = FM.StoreAllData.Tables[1].Rows[0]["SchoolStationP"];
                    ViewBag.Principal = FM.StoreAllData.Tables[1].Rows[0]["PRINCIPAL"];
                    ViewBag.phno = FM.StoreAllData.Tables[1].Rows[0]["PHONE"];
                    ViewBag.mob = FM.StoreAllData.Tables[1].Rows[0]["MOBILE"];
                    ViewBag.DC = FM.StoreAllData.Tables[1].Rows[0]["DIST"];
                    ViewBag.DN = FM.StoreAllData.Tables[1].Rows[0]["DISTE"];
                }

                if (FM.StoreAllData.Tables[2].Rows.Count > 0)
                {
                    ViewBag.table2Count = "1";
                    ViewBag.regdt = FM.StoreAllData.Tables[2].Rows[0]["regdt"].ToString();
                }
                else
                {
                    ViewBag.table2Count = "0";
                }
                if (FM.StoreAllData.Tables[3].Rows.Count > 0)
                {
                    ViewBag.table3Count = "1";
                    ViewBag.bcode = FM.StoreAllData.Tables[3].Rows[0]["bcode"];
                    ViewBag.branch = FM.StoreAllData.Tables[3].Rows[0]["branch"];
                    ViewBag.Bank = FM.StoreAllData.Tables[3].Rows[0]["bank"].ToString();
                    ViewBag.amt = FM.StoreAllData.Tables[3].Rows[0]["fee"];
                    ViewBag.challanid = FM.StoreAllData.Tables[3].Rows[0]["challanid"];
                    ViewBag.depositdt = FM.StoreAllData.Tables[3].Rows[0]["DEPOSITDT"];
                    ViewBag.lot = FM.StoreAllData.Tables[3].Rows[0]["lot"];
                }
                else
                {
                    ViewBag.table3Count = "0";
                }

                return View(FM);
            }


            // return View();
        }


        public JsonResult jqReGenerateChallaanNew(string ChallanId, string BCODE)
        {
            string ChallanIdOut = "";
            string dee = "0";
            try
            {
                if (ChallanId == null || ChallanId == "")
                {
                    dee = "-1";
                }
                if (BCODE == null || BCODE == "")
                {
                    dee = "-1";
                }
                ChallanMasterModel CM = new ChallanMasterModel();
                string ChallanId1 = ChallanId.ToString();
                string Usertype = "User";
                int OutStatus = 0;
                //ReGenerateChallaanByIdJunior
                DataSet ds = _challanRepository.ReGenerateChallaanByIdJunior(ChallanId1, BCODE, Usertype, out OutStatus, out ChallanIdOut);//ReGenerateChallaanByIdBankSP
                if (OutStatus == 1)
                {
                    dee = "1";
                }
                else
                {
                    dee = "0";
                }
            }
            catch (Exception ex)
            {
                dee = "-3";
            }
            return Json(new { dee = dee, chid = ChallanIdOut }, JsonRequestBehavior.AllowGet);

        }

        // Cancel Challan
        public JsonResult CancelOfflineChallanBySchl(string cancelremarks, string challanid, string Type)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {
                string dee = "";
                string outstatus = "";

                DataSet ds = _challanRepository.CancelOfflineChallanBySchl(cancelremarks, challanid, out outstatus, loginSession.SCHL, Type);//ChallanDetailsCancelSP               
                dee = outstatus;
                return Json(new { sn = dee, chid = challanid }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { sn = "-1", chid = challanid }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion


        #region Challan Deposit Details 


        //public ActionResult FeeDepositDetails(string id)
        //{
        //    if (id == null || id.ToString() == "")
        //    {
        //        return RedirectToAction("FinalPrint", "Home");
        //    }
        //    DataSet dst = _bankRepository.GetChallanDetailsByIdSPBank(id);
        //    if (dst == null || dst.Tables[0].Rows.Count == 0)
        //    {
        //        return RedirectToAction("FinalPrint", "Home");
        //    }
        //    else
        //    {
        //        string StudentList = dst.Tables[0].Rows[0]["StudentList"].ToString().Trim();
        //        if (StudentList != "")
        //        {
        //            TempData["StudentList"] = StudentList;
        //            return RedirectToAction("ChallanDepositDetails", "Home");
        //        }
        //    }
        //    return RedirectToAction("FinalPrint", "Home");
        //}



        public ActionResult ChallanDepositDetails(string id)
        {
            ChallanDepositDetailsModel cdm = new ChallanDepositDetailsModel();
            if (id == null || id.ToString() == "")
            {
                return RedirectToAction("FinalPrint", "Home");
            }

            DataSet dst1 = _bankRepository.GetChallanDetailsByIdSPBank(id);// id = challanid
            if (dst1 == null || dst1.Tables[0].Rows.Count == 0)
            {
                return RedirectToAction("FinalPrint", "Home");
            }
            else
            {
                string StudentList = dst1.Tables[0].Rows[0]["StudentList"].ToString().Trim();
                TempData["StudentList"] = StudentList;

                if (string.IsNullOrEmpty(StudentList))
                {
                    return RedirectToAction("Logout", "Login");
                }
                else
                {
                    ViewBag.cid = StudentList.ToString();
                    ViewBag.MyChallanId = ViewBag.SelectedChallanId = "";
                    // For all challans either cancel or not. by student list
                    DataSet dst = _challanRepository.GetChallanDetailsByStudentList(StudentList);
                    if (dst == null || dst.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCount = 1;
                        if (dst.Tables[0].Rows.Count > 0)
                        {

                            var itemCHALLANID = dst.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
                            {
                                Text = dataRow.Field<string>("CHALLANID").ToString(),
                                Value = dataRow.Field<string>("CHALLANID").ToString(),
                            }).ToList();
                            ViewBag.MyChallanId = itemCHALLANID.ToList();


                        }
                    }
                }
            }
            return View(cdm);
        }

        [HttpPost]
        public ActionResult ChallanDepositDetails(string id, ChallanDepositDetailsModel cdm, FormCollection frm, string submit)
        {

            if (TempData["StudentList"] == null || TempData["StudentList"].ToString() == "")
            {
                return RedirectToAction("FinalPrint", "Home");
            }

            id = TempData["StudentList"].ToString();


            try
            {
                if (id == null || id.ToString() == "")
                {
                    return RedirectToAction("Logout", "Login");
                }
                else
                {
                    ViewBag.cid = id.ToString();
                    // For all challans either cancel or not. by student list
                    DataSet dst = _challanRepository.GetChallanDetailsByStudentList(id);
                    if (dst == null || dst.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        if (dst.Tables[0].Rows.Count > 0)
                        {
                            var itemCHALLANID = dst.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
                            {
                                Text = dataRow.Field<string>("CHALLANID").ToString(),
                                Value = dataRow.Field<string>("CHALLANID").ToString(),
                            }).ToList();
                            ViewBag.MyChallanId = itemCHALLANID.ToList();
                        }
                    }
                }


                if (frm["CHALLANID"] != "")
                {
                    if (submit != null)
                    {

                        if (submit.ToUpper() == "CANCEL")
                        {
                            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                            if (frm["CHALLANID"] != "" && adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                            {
                                string outstatus = "";
                                int AdminId = Convert.ToInt32(adminLoginSession.AdminId);
                                string challanid = cdm.CHALLANID;
                                _challanRepository.ChallanDepositDetailsCancel("Cancel", frm["CHALLANID"].ToString(), out outstatus, AdminId);//ChallanDetailsCancelSP
                                if (outstatus == "1")
                                {
                                    ViewData["Result"] = "10";
                                }
                                else
                                {
                                    ViewData["Result"] = "11";
                                }
                            }
                            return View(cdm);
                        }
                    }

                    DataSet ds1 = _bankRepository.GetChallanDetailsByIdSPBank(frm["CHALLANID"].ToString());
                    cdm.dsData = ds1;
                    if (cdm.dsData == null || cdm.dsData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        cdm.CHALLANID = ds1.Tables[0].Rows[0]["CHALLANID"].ToString();
                        cdm.SCHLREGID = ds1.Tables[0].Rows[0]["SCHLREGID"].ToString();
                        cdm.LOT = Convert.ToInt32(ds1.Tables[0].Rows[0]["LOT"].ToString());
                        cdm.APPNO = ds1.Tables[0].Rows[0]["APPNO"].ToString();
                        cdm.SCHLCANDNM = ds1.Tables[0].Rows[0]["SCHLCANDNM"].ToString();
                        cdm.CHLNDATE = ds1.Tables[0].Rows[0]["CHLNDATE"].ToString();
                        cdm.CHLVDATE = ds1.Tables[0].Rows[0]["CHLNVDATE"].ToString();
                        cdm.BANK = ds1.Tables[0].Rows[0]["BANK"].ToString();
                        cdm.FEE = ds1.Tables[0].Rows[0]["FEE"].ToString();
                        cdm.DOWNLDDATE = ds1.Tables[0].Rows[0]["DOWNLDDATE"].ToString();
                        cdm.DOWNLDFLOT = ds1.Tables[0].Rows[0]["DOWNLDFLOT"].ToString();
                    }

                    DataSet ds = new DataSet();
                    string OutError = "0";
                    if (cdm.CHALLANID != "")
                    {
                        ds = _challanRepository.ChallanDepositDetails(cdm, out OutError);
                        if (OutError == "1")
                        {
                            ViewData["Result"] = "1";
                        }
                        else
                        {
                            ViewData["Result"] = "0";
                        }

                    }
                }
                return View(cdm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }

        // Get Challan
        public JsonResult GetChallanDetails(string challanid)
        {
            try
            {
                ChallanDepositDetailsModel cdm = new ChallanDepositDetailsModel();
                int OutStatus = 0;
                DataSet ds1 = _bankRepository.GetChallanDetailsByIdSPBank(challanid);
                if (ds1 == null || ds1.Tables[0].Rows.Count == 0)
                {
                    OutStatus = 0;
                }
                else
                {
                    OutStatus = 1;
                    cdm.CHALLANID = ds1.Tables[0].Rows[0]["CHALLANID"].ToString();
                    cdm.SCHLREGID = ds1.Tables[0].Rows[0]["SCHLREGID"].ToString();
                    cdm.LOT = Convert.ToInt32(ds1.Tables[0].Rows[0]["LOT"].ToString());
                    cdm.APPNO = ds1.Tables[0].Rows[0]["APPNO"].ToString();
                    cdm.SCHLCANDNM = ds1.Tables[0].Rows[0]["SCHLCANDNM"].ToString();
                    cdm.CHLNDATE = ds1.Tables[0].Rows[0]["CHLNDATE"].ToString().Split(' ')[0].ToString();
                    cdm.CHLVDATE = ds1.Tables[0].Rows[0]["CHLNVDATE"].ToString().Split(' ')[0].ToString();
                    cdm.BANK = ds1.Tables[0].Rows[0]["BANK"].ToString();
                    cdm.FEE = ds1.Tables[0].Rows[0]["FEE"].ToString();
                    cdm.DOWNLDDATE = ds1.Tables[0].Rows[0]["DOWNLDDATE"].ToString();
                    cdm.DOWNLDFLOT = ds1.Tables[0].Rows[0]["DOWNLDFLOT"].ToString();

                    cdm.BRANCHCAND = ds1.Tables[0].Rows[0]["BRANCHCAND"].ToString();
                    cdm.BRCODECAND = ds1.Tables[0].Rows[0]["BRCODECAND"].ToString();
                    cdm.J_REF_NOCAND = ds1.Tables[0].Rows[0]["J_REF_NOCAND"].ToString();
                    cdm.DEPOSITDTCAND = ds1.Tables[0].Rows[0]["DEPOSITDTCAND"].ToString();
                    cdm.DOWNLDFLG = Convert.ToInt32(ds1.Tables[0].Rows[0]["DOWNLDFLG"].ToString());
                }
                var results = new
                {
                    status = OutStatus,
                    chid = cdm.CHALLANID,
                    SCHLREGID = cdm.SCHLREGID,
                    LOT = cdm.LOT,
                    APPNO = cdm.APPNO,
                    SCHLCANDNM = cdm.SCHLCANDNM,
                    CHLNDATE = cdm.CHLNDATE,
                    CHLVDATE = cdm.CHLVDATE,
                    BANK = cdm.BANK,
                    FEE = cdm.FEE,
                    DOWNLDDATE = cdm.DOWNLDDATE,
                    DOWNLDFLOT = cdm.DOWNLDFLOT,
                    DOWNLDFLG = cdm.DOWNLDFLG,
                    DOWNLDFSTATUS = Convert.ToInt32(cdm.DOWNLDFLG) == 1 ? "Downloaded on : " + cdm.DOWNLDDATE + "" : "Not Downloaded",
                    BRANCHCAND = cdm.BRANCHCAND,
                    BRCODECAND = cdm.BRCODECAND,
                    J_REF_NOCAND = cdm.J_REF_NOCAND,
                    DEPOSITDTCAND = cdm.DEPOSITDTCAND
                };
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #endregion Challan Deposit Details



        #region Calculate Fee of Registration
        [SessionCheckFilter]
        public ActionResult CalculateFeeWithoutLateFee(string id, string Status, FormCollection frc)
        {
            try
            {
                FeeHomeViewModel fhvm = new FeeHomeViewModel();
                LoginSession loginSession = (LoginSession)Session["LoginSession"];

                TempData["CalculateFeeWithoutLateFee"] = null;
                TempData["AllowBanksWithoutLateFee"] = null;
                TempData["PaymentFormWithoutLateFee"] = null;
                TempData["FeeStudentListWithoutLateFee"] = null;
                return View(fhvm);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }

        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CalculateFeeWithoutLateFee(string id, string Status, FormCollection frc, string aa, string ChkId, string selectedClass)
        {
            try
            {
                FeeHomeViewModel fhvm = new FeeHomeViewModel();
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                // ViewBag.Eighth = loginSession.middle == "Y" ? "1" : "0";
                /// ViewBag.Fifth = loginSession.fifth == "Y" ? "1" : "0";

                if (Status == "Successfully" || Status == "Failure")
                {
                    //ViewData["Status"] = Status;
                    ViewData["FeeStatusWithoutLateFee"] = Status;
                    return View();
                }
                else
                {
                    // FormCollection frc = new FormCollection();
                    string FormNM = frc["ChkId"];
                    if (FormNM == null || FormNM == "")
                    {
                        return View();
                    }



                    ViewData["Status"] = "";
                    string UserType = "User";


                    string Search = string.Empty;
                    Search = "SCHL='" + loginSession.SCHL + "'";
                    FormNM = "'" + FormNM.Replace(",", "','") + "'";
                    Search += "  and type='" + UserType + "' and form_name in(" + FormNM + ")";

                    string SearchString = DateTime.Now.ToString("dd/MM/yyyy");
                    DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);


                    //string SearchString = DateTime.Now.AddDays(-10).ToString("dd/MM/yyyy");
                    //DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);




                    /// for Live

                    DataSet dsCheckFee = _challanRepository.CheckFeeStatus(loginSession.SCHL, UserType, FormNM.ToUpper(), date);//CheckFeeStatusSPByView

                    //CheckFeeStatus
                    if (dsCheckFee == null)
                    {
                        // return RedirectToAction("Index", "Home");
                        ViewData["FeeStatusWithoutLateFee"] = "11";
                        return View();
                    }
                    else
                    {
                        if (dsCheckFee.Tables[0].Rows.Count > 0)
                        {
                            if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "0")
                            {
                                //Not Exist (if lot '0' not xist)
                                ViewData["FeeStatusWithoutLateFee"] = "0";
                                ViewBag.Message = "All Fees are submmited/ Data Not Available for Fee Calculation...";
                                ViewBag.TotalCount = 0;
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "2")
                            {
                                //Not Allowed Some Form are not in Fee
                                ViewData["FeeStatusWithoutLateFee"] = "2";
                                // ViewBag.Message = "Not Allowed,Some FORM are not in Fee Structure..please contact Punjab School Education Board";
                                // ViewBag.Message = "Calculate fee is allowed only for M1 and T1 Form only";
                                DataSet ds = dsCheckFee;
                                fhvm.StoreAllData = dsCheckFee;
                                ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
                                // return View(fhvm);
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "3" || dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "5")
                            {
                                int CountTable = dsCheckFee.Tables.Count;
                                ViewBag.CountTable = CountTable;
                                ViewBag.OutStatus = dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString();
                                //Not Allowed Some Form are not in Fee
                                ViewData["FeeStatusWithoutLateFee"] = "2";
                                // ViewBag.Message = "Not Allowed, Some Mandatory Fields are not Filled.";
                                // ViewBag.Message = "Some Mandatory Fields Like Subject's/Photograph's,Signature's of Listed Form wise Candidate's are Missing or Duplicate Records. Please Update These Details Then Try Again to Calculate Fee & Final Submission";
                                DataSet ds = dsCheckFee;
                                fhvm.StoreAllData = dsCheckFee;
                                if (CountTable > 1)
                                {
                                    ViewBag.TotalCount = dsCheckFee.Tables[0].Rows.Count;
                                    if (dsCheckFee.Tables[1].Rows.Count > 0)
                                    {
                                        if (dsCheckFee.Tables[1].Rows[0]["Outstatus"].ToString() == "5")
                                        {
                                            ViewBag.TotalCountDuplicate = dsCheckFee.Tables[1].Rows.Count;
                                        }
                                        else
                                        { ViewBag.TotalCountDuplicate = 0; }
                                    }
                                    else
                                    { ViewBag.TotalCountDuplicate = 0; }

                                }


                                // return View(fhvm);
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "1")
                            {
                                string cls = selectedClass;
                                DataSet ds = _challanRepository.GetCalculateFeeBySchoolWithoutLateFee(cls, Search, loginSession.SCHL, date);
                                fhvm.StoreAllData = ds;
                                if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
                                {
                                    ViewBag.Message = "Record Not Found";
                                    ViewBag.TotalCount = 0;
                                    ViewData["FeeStatusWithoutLateFee"] = "3";
                                    return View();
                                }
                                else
                                {
                                    ViewData["FeeStatusWithoutLateFee"] = "1";
                                    TempData["CalculateFeeWithoutLateFee"] = ds;
                                    ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
                                    TempData["AllowBanksWithoutLateFee"] = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
                                    fhvm.TotalFeesInWords = ds.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
                                    fhvm.EndDate = ds.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + ds.Tables[0].Rows[0]["FeeValidDate"].ToString();
                                }
                            }
                        }
                        else
                        {
                            //return RedirectToAction("Index", "Home");
                            ViewData["FeeStatusWithoutLateFee"] = "22";
                            return View();
                        }
                    }
                    return View(fhvm);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }

        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PaymentFormWithoutLateFee()
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                PaymentformViewModel pfvm = new PaymentformViewModel();

                if (TempData["CalculateFeeWithoutLateFee"] == null || TempData["CalculateFeeWithoutLateFee"].ToString() == "")
                {
                    return RedirectToAction("CalculateFee", "Home");
                }

                string schl = loginSession.SCHL;
                DataSet ds = _challanRepository.GetSchoolLotDetails(schl);
                pfvm.PaymentFormData = ds;
                if (pfvm.PaymentFormData == null || pfvm.PaymentFormData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    pfvm.LOTNo = Convert.ToInt32(ds.Tables[0].Rows[0]["LOT"].ToString());
                    pfvm.Dist = ds.Tables[0].Rows[0]["Dist"].ToString();
                    pfvm.District = ds.Tables[0].Rows[0]["diste"].ToString();
                    pfvm.DistrictFull = ds.Tables[0].Rows[0]["DistrictFull"].ToString();
                    //pfvm.SchoolCode = Convert.ToInt32(ds.Tables[0].Rows[0]["schl"].ToString());
                    pfvm.SchoolCode = ds.Tables[0].Rows[0]["schl"].ToString();
                    pfvm.SchoolName = ds.Tables[0].Rows[0]["SchoolFull"].ToString(); // Schollname with station and dist 
                    ViewBag.TotalCount = pfvm.PaymentFormData.Tables[0].Rows.Count;

                    DataSet dscalFee = (DataSet)TempData["CalculateFeeWithoutLateFee"];

                    pfvm.totaddfee = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalExamFee"].ToString());/// 
                    pfvm.TotalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFee"].ToString());
                    pfvm.TotalLateFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalLateFee"].ToString());
                    pfvm.TotalFinalFees = Convert.ToInt32(dscalFee.Tables[1].Rows[0]["TotalFeeAmount"].ToString());
                    pfvm.TotalFeesInWords = dscalFee.Tables[1].Rows[0]["TotalFeesWords"].ToString();
                    //  pfvm.FeeDate = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy");
                    pfvm.FeeDate = dscalFee.Tables[0].Rows[0]["FeeValidDateFormat"].ToString();
                    //TotalCandidates
                    pfvm.TotalCandidates = Convert.ToInt32(dscalFee.Tables[0].AsEnumerable().Sum(x => x.Field<int>("CountStudents")));
                    pfvm.FeeCode = dscalFee.Tables[0].Rows[0]["FeeCode"].ToString();
                    pfvm.FeeCategory = dscalFee.Tables[0].Rows[0]["FEECAT"].ToString();
                    TempData["PaymentFormWithoutLateFee"] = pfvm;

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

                    return View(pfvm);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PaymentFormWithoutLateFee(PaymentformViewModel pfvm, FormCollection frm, string PayModValue, string IsOnline)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                ChallanMasterModel CM = new ChallanMasterModel();
                if (pfvm.BankCode == null)
                {
                    ViewBag.Message = "Please Select Bank";
                    ViewData["SelectBankWithoutLateFee"] = "1";
                    return View(pfvm);
                }

                if (pfvm.BankCode == "301" || pfvm.BankCode == "302")
                {
                    PayModValue = "online";
                }
                else if (pfvm.BankCode == "203")
                {
                    PayModValue = "hod";
                }
                else
                {
                    PayModValue = "offline";
                }


                if (TempData["PaymentFormWithoutLateFee"] == null || TempData["PaymentFormWithoutLateFee"].ToString() == "")
                {
                    return RedirectToAction("PaymentForm", "Home");
                }
                if (TempData["FeeStudentListWithoutLateFee"] == null || TempData["FeeStudentListWithoutLateFee"].ToString() == "")
                {
                    return RedirectToAction("PaymentForm", "Home");
                }


                string BankCode = pfvm.BankCode;
                PaymentformViewModel PFVMSession = (PaymentformViewModel)TempData["PaymentFormWithoutLateFee"];
                if (ModelState.IsValid)
                {
                    string SCHL = loginSession.SCHL;
                    string FeeStudentList = TempData["FeeStudentListWithoutLateFee"].ToString();
                    CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);

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
                    string TotfeePG = (CM.TOTFEE).ToString();
                    CM.addfee = Convert.ToInt32(PFVMSession.totaddfee);// exam fee
                    CM.regfee = Convert.ToInt32(PFVMSession.TotalFees);
                    CM.latefee = Convert.ToInt32(PFVMSession.TotalLateFees);
                    CM.FEECAT = PFVMSession.FeeCategory;
                    CM.FEECODE = PFVMSession.FeeCode;
                    CM.FEEMODE = "CASH";
                    CM.BANK = pfvm.BankName;
                    CM.BCODE = pfvm.BankCode;
                    CM.BANKCHRG = PFVMSession.BankCharges;
                    CM.SchoolCode = PFVMSession.SchoolCode.ToString();
                    CM.DIST = PFVMSession.Dist.ToString();
                    CM.DISTNM = PFVMSession.District;
                    CM.LOT = PFVMSession.LOTNo;
                    CM.SCHLREGID = PFVMSession.SchoolCode.ToString();
                    CM.type = "schle";
                    DateTime CHLNVDATE2;
                    if (DateTime.TryParseExact(PFVMSession.FeeDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                    {
                        CM.ChallanVDateN = CHLNVDATE2;
                    }
                    CM.CHLNVDATE = PFVMSession.FeeDate;
                    string SchoolMobile = "";
                    //string result = "0";
                    string result = _challanRepository.InsertPaymentFormJunior(CM, out SchoolMobile);
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

                        ViewData["SelectBankWithoutLateFee"] = null;
                        ViewData["result"] = 1;
                        ViewBag.ChallanNo = result;

                        if (PayModValue.ToString().ToLower().Trim() == "online")
                        {
                            #region Payment Gateyway
                            string paymenttype = BankCode;
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


                                //return View("../Gateway/AtomCheckoutUrl", new AtomViewModel(strURL));

                                // return View("CcAvenue", new CcAvenueViewModel(queryParameter.Encrypt
                                //(BuildCcAvenueRequestParameters(invoiceNumber, amount), WorkingKey), AccessCode, CheckoutUrl));
                            }
                            else if (paymenttype.ToUpper() == "302" && ViewBag.ChallanNo != "")/*ATOM*/
                            {
                                string strURL;
                                //string MerchantLogin = "197";
                                //string MerchantPass = "Test@123";
                                string MerchantLogin = ConfigurationManager.AppSettings["ATOMLoginId"].ToString();
                                string MerchantPass = ConfigurationManager.AppSettings["ATOMPassword"].ToString();
                                string MerchantDiscretionaryData = "NB";  // for netbank
                                string ClientCode = "PSEB";
                                string ProductID = ConfigurationManager.AppSettings["ATOMProductID"].ToString();
                                string CustomerAccountNo = "10000036600";
                                string TransactionType = "NBFundTransfer";  // for netbank
                                                                            //string TransactionAmount = "1";
                                string TransactionAmount = TotfeePG;
                                string TransactionCurrency = "INR";
                                string TransactionServiceCharge = "0";
                                string TransactionID = ViewBag.ChallanNo;// Unique Challan Number
                                string TransactionDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                                // string TransactionDateTime = "18/10/2019 13:15:19";
                                string BankID = "ATOM TEST";
                                //string ru = "https://localhost:57360/Gateway/ATOMPaymentResponse";
                                //string ru = ConfigurationManager.AppSettings["ATOMRUTEST"].ToString();

                                string ru = ConfigurationManager.AppSettings["ATOMRU"].ToString();
                                // User Details
                                string udf1CustName = CM.SCHLREGID;
                                string udf2CustEmail = CM.SCHLREGID;
                                string udf3CustMob = "9876543210";

                                strURL = GatewayController.ATOMTransferFund(MerchantLogin, MerchantPass, MerchantDiscretionaryData, ProductID, ClientCode, CustomerAccountNo, TransactionType,
                                  TransactionAmount, TransactionCurrency, TransactionServiceCharge, TransactionID, TransactionDateTime, BankID, ru, udf1CustName, udf2CustEmail, udf3CustMob);

                                if (!string.IsNullOrEmpty(strURL))
                                {
                                    return View("../Gateway/AtomCheckoutUrl", new AtomViewModel(strURL));
                                }
                                else
                                {
                                    ViewData["result"] = -10;
                                    return View(PFVMSession);
                                }
                            }
                            #endregion Payment Gateyway
                        }
                        else
                        {

                            //Your Challan no. XXXXXXXXXX of Lot no  XX successfully generated and valid till Dt XXXXXXXXXXX. Regards PSEB
                            string Sms = "Your Challan no. " + result + " of Lot no  " + CM.LOT + " successfully generated and valid till Dt " + CM.CHLNVDATE + ". Regards PSEB";
                            try
                            {
                                string getSms = DBClass.gosms(SchoolMobile, Sms);
                                // string getSms = DBClass.gosms("9711819184", Sms);
                            }
                            catch (Exception) { }

                            ModelState.Clear();
                            //--For Showing Message---------//                   
                            return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });

                        }

                    }

                    TempData["CalculateFeeWithoutLateFee"] = null;
                    TempData["AllowBanksWithoutLateFee"] = null;
                    TempData["PaymentFormWithoutLateFee"] = null;
                    TempData["FeeStudentListWithoutLateFee"] = null;

                }
                return View(PFVMSession);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
                return View(pfvm);
            }
        }

        #endregion


        #region UndertakingOfQuestionPapers

        [HttpGet]
        public ActionResult UndertakingOfQuestionPapers(UndertakingOfQuestionPapers undertakingOfQuestionPapers)
        {
            if (Session["LoginSession"] == null && Session["CenterHeadLoginSession"] == null)
            {
                return RedirectToAction("Logout", "Login");
            }

            if (Session["LoginSession"] != null)
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                undertakingOfQuestionPapers.CentCode = loginSession.SCHL;
                undertakingOfQuestionPapers.ClassList = DBClass.GetAllPSEBCLASS().Where(s => s.Value == "8").ToList();
            }

            if (Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];

                undertakingOfQuestionPapers.CentCode = centerHeadLoginSession.UserName.ToString();
                undertakingOfQuestionPapers.ClassList = DBClass.GetAllPSEBCLASS().Where(s => s.Value == "5").ToList();
            }

            undertakingOfQuestionPapers.StatusList = DBClass.GetYesNoText();

            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;

            if (currentMonth > 3)
            {
                undertakingOfQuestionPapers.MonthList = DBClass.GetMonthNameNumber().Where(s => Convert.ToInt32(s.Value) < currentMonth).ToList();
            }
            else
            {
                undertakingOfQuestionPapers.MonthList = DBClass.GetMonthNameNumber().Where(s => Convert.ToInt32(s.Value) <= currentMonth).ToList();
            }



            if (currentMonth > 3)
            {
                undertakingOfQuestionPapers.YearList = DBClass.GetSessionSingle().Where(s => Convert.ToInt32(s.Value) == currentYear).ToList();
            }
            else
            {
                undertakingOfQuestionPapers.YearList = DBClass.GetSessionSingle().Where(s => Convert.ToInt32(s.Value) <= currentYear).ToList();
            }

            undertakingOfQuestionPapers.UndertakingOfQuestionPapersList = _context.UndertakingOfQuestionPapers.AsNoTracking().Where(s => s.CentCode == undertakingOfQuestionPapers.CentCode && s.IsActive == true).OrderByDescending(s => s.QPID).ToList();

            return View(undertakingOfQuestionPapers);
        }


        public async Task<ActionResult> UndertakingOfQuestionPapers(UndertakingOfQuestionPapers undertakingOfQuestionPapers, FormCollection fc)
        {
            try
            {

                if (string.IsNullOrEmpty(undertakingOfQuestionPapers.QP_Month) || string.IsNullOrEmpty(undertakingOfQuestionPapers.QP_Year) ||
                    string.IsNullOrEmpty(undertakingOfQuestionPapers.QP_Class))
                {
                    TempData["resultIns"] = "FILL";
                    return View(undertakingOfQuestionPapers);
                }

                if (Session["LoginSession"] != null)
                {
                    LoginSession loginSession = (LoginSession)Session["LoginSession"];
                    undertakingOfQuestionPapers.CentCode = loginSession.SCHL;
                    undertakingOfQuestionPapers.ClassList = DBClass.GetAllPSEBCLASS().Where(s => s.Value == "8").ToList();
                }

                if (Session["CenterHeadLoginSession"] != null)
                {
                    CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                    undertakingOfQuestionPapers.CentCode = centerHeadLoginSession.UserName.ToString();
                    undertakingOfQuestionPapers.ClassList = DBClass.GetAllPSEBCLASS().Where(s => s.Value == "5").ToList();
                }

                int NOS_Entry = _context.UndertakingOfQuestionPapers.Where(s => s.CentCode == undertakingOfQuestionPapers.CentCode
                && s.QP_Class == undertakingOfQuestionPapers.QP_Class
                && s.QP_Month == undertakingOfQuestionPapers.QP_Month
                && s.QP_Year == undertakingOfQuestionPapers.QP_Year).Count();

                if (NOS_Entry == 0)
                {
                    undertakingOfQuestionPapers.Refno = undertakingOfQuestionPapers.CentCode
                        + "C" + undertakingOfQuestionPapers.QP_Class
                        + undertakingOfQuestionPapers.QP_Month + undertakingOfQuestionPapers.QP_Year;

                    UndertakingOfQuestionPapers dataToSave = new UndertakingOfQuestionPapers()
                    {
                        QPID = 0,
                        CentCode = undertakingOfQuestionPapers.CentCode,
                        Refno = undertakingOfQuestionPapers.Refno.ToUpper(),
                        QP_Class = undertakingOfQuestionPapers.QP_Class,
                        QP_Month = undertakingOfQuestionPapers.QP_Month,
                        QP_Year = undertakingOfQuestionPapers.QP_Year,
                        QP_Description1 = undertakingOfQuestionPapers.QP_Description1,
                        QP_Status1 = undertakingOfQuestionPapers.QP_Status1,
                        QP_Description2 = undertakingOfQuestionPapers.QP_Description2,
                        QP_Remarks1 = undertakingOfQuestionPapers.QP_Remarks1,
                        QP_Status2 = undertakingOfQuestionPapers.QP_Status2,
                        QP_Remarks2 = undertakingOfQuestionPapers.QP_Remarks2,
                        QP_Description3 = undertakingOfQuestionPapers.QP_Description3,
                        QP_Status3 = undertakingOfQuestionPapers.QP_Status3,
                        QP_Remarks3 = undertakingOfQuestionPapers.QP_Remarks3,
                        SubmitBy = undertakingOfQuestionPapers.CentCode,
                        SubmitOn = DateTime.Now,
                        IsActive = true,
                        IsFinalLock = false
                    };

                    _context.UndertakingOfQuestionPapers.Add(dataToSave);
                    int insertedRecords = await _context.SaveChangesAsync();

                    if (insertedRecords > 0)
                    {
                        TempData["resultIns"] = "S";
                    }
                    else
                    {
                        TempData["resultIns"] = "F";
                    }
                }

                else
                {

                    TempData["resultIns"] = "DUP";
                    undertakingOfQuestionPapers.StatusList = DBClass.GetYesNoText();
                    undertakingOfQuestionPapers.ClassList = DBClass.GetAllPSEBCLASS().Where(s => s.Value == "5" || s.Value == "8").ToList();
                    undertakingOfQuestionPapers.MonthList = DBClass.GetMonthNameNumber();
                    undertakingOfQuestionPapers.YearList = DBClass.GetSession();
                    return View(undertakingOfQuestionPapers);
                }
                return RedirectToAction("UndertakingOfQuestionPapers", "Home");
            }
            catch (Exception ex)
            {
                TempData["resultIns"] = "Error : " + ex.Message;
            }
            return RedirectToAction("UndertakingOfQuestionPapers", "Home");
            //return View(undertakingOfQuestionPapers);
        }


        [Route("ActionUndertakingOfQuestionPapers/{id}/{act}")]
        public async Task<ActionResult> ActionUndertakingOfQuestionPapers(string id, string act)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(act))
            {
                if (act.ToUpper() == "D")
                {
                    UndertakingOfQuestionPapers data = _context.UndertakingOfQuestionPapers.SingleOrDefault(s => s.Refno.ToUpper() == id.ToUpper());
                    if (data != null && data.IsFinalLock == false)
                    {
                        _context.Entry(data).State = EntityState.Deleted;
                        int insertedRecords = await _context.SaveChangesAsync();
                        TempData["resultIns"] = "DEL";
                    }
                }
                else if (act.ToUpper() == "F")
                {
                    UndertakingOfQuestionPapers data = _context.UndertakingOfQuestionPapers.SingleOrDefault(s => s.Refno.ToUpper() == id.ToUpper());
                    data.FinalSubmitOn = DateTime.Now;
                    data.IsFinalLock = true;
                    if (data != null)
                    {
                        _context.Entry(data).State = EntityState.Modified;
                        int insertedRecords = await _context.SaveChangesAsync();
                        TempData["resultIns"] = "FNL";
                    }
                }

            }
            return RedirectToAction("UndertakingOfQuestionPapers", "Home");
        }
        #endregion


    }
}

