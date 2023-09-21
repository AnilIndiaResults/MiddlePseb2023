using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.Models;
using PsebJunior.AbstractLayer;
using PsebPrimaryMiddle.Filters;
using System.IO;
using System.Data;
using System.Web.Security;
using System.Data.OleDb;
using ClosedXML.Excel;
using System.Text;
using PsebPrimaryMiddle.Repository;

namespace PsebPrimaryMiddle.Controllers
{
    [AdminMenuFilter]
    [AdminLoginCheckFilter]
    public class ChallanController : Controller
    {
        private readonly IChallanRepository _challanRepository;
        private readonly IBankRepository _bankRepository;

        public ChallanController(IChallanRepository challanRepository, IBankRepository bankRepository)
        {            
            _bankRepository = bankRepository;
            _challanRepository = challanRepository;
        }

        // GET: Challan
        public ActionResult Index()
        {
            return View();
        }

        #region ChallanDetails
        [AdminLoginCheckFilter]
        public ActionResult ChallanDetails(Challan obj,int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {
                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);
                ViewBag.MySch1 = itemsch1.ToList();

                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsPrint = 1; ViewBag.IsView = 1; ViewBag.IsRegenerate = 1; ViewBag.IsCancel = 1; }
                else
                {

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();                 
                    DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                    if (aAct.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.IsPrint = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("HOME/GENERATECHALLAAN")).Count();
                        ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsRegenerate = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsCancel = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                    }
                }
                #endregion Action Assign Method
               
                // By Rohit  -- select bank from database
                string BankAllow = string.Empty;
                DataSet dsBank = DBClass.GetAdminDetailsById(Convert.ToInt32(adminLoginSession.AdminId), Convert.ToInt32(adminLoginSession.CurrentSession.Substring(0, 4)));
                if (dsBank != null)
                {
                    if (dsBank.Tables[3].Rows.Count >= 0)
                    {
                        //Bank
                        var _bankList = dsBank.Tables[3].AsEnumerable().Select(dataRow => new SelectListItem
                        {
                            Text = dataRow.Field<string>("Bank").ToString(),
                            Value = dataRow.Field<string>("Bcode").ToString(),
                        }).ToList();
                        obj.BankList = _bankList.ToList();
                    }
                }
                // End 
                obj.FeeCatList = DBClass.GetFeeCat();


                // Data searching by paging 
                if (TempData["SearchString"] != null)
                {
                    string Search = Convert.ToString(TempData["SearchString"]);
                    TempData["SearchString"] = Search;
                    obj.StoreAllData = _bankRepository.GetChallanDetails(Search, "", pageIndex);
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {

                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = obj.StoreAllData.Tables[1].Rows[0]["TotalCnt"].ToString();
                        int tp = Convert.ToInt32(ViewBag.TotalCount1);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                    }
                }
                // Data searching by paging 

                return View(obj);
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChallanDetails(int? page, Challan obj, FormCollection frm, string cmd,  string srhfld, string SearchString)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {

                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);
                ViewBag.MySch1 = itemsch1.ToList();


                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsPrint = 1; ViewBag.IsView = 1; ViewBag.IsRegenerate = 1; ViewBag.IsCancel = 1; }
                else
                {

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();                   
                    DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                    if (aAct.Tables[0].Rows.Count > 0)
                    {
                        //ViewBag.IsEdit = aAct.Tables[0].AsEnumerable().Where(c => c.Field<int>("MenuId").Equals(61)).Count();                        
                        ViewBag.IsPrint = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("HOME/GENERATECHALLAAN")).Count();
                        ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsRegenerate = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsCancel = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                    }
                }
                #endregion Action Assign Method
               
                // By Rohit  -- select bank from database
                string BankAllow = string.Empty;
                DataSet dsBank = DBClass.GetAdminDetailsById(Convert.ToInt32(adminLoginSession.AdminId), Convert.ToInt32(adminLoginSession.CurrentSession.ToString().Substring(0, 4)));
                if (dsBank != null)
                {
                    if (dsBank.Tables[3].Rows.Count >= 0)
                    { 
                        //Bank
                        var _bankList = dsBank.Tables[3].AsEnumerable().Select(dataRow => new SelectListItem
                        {
                            Text = dataRow.Field<string>("Bank").ToString(),
                            Value = dataRow.Field<string>("Bcode").ToString(),
                        }).ToList();
                        obj.BankList = _bankList.ToList();
                    }
                }
                // End 

                obj.FeeCatList = DBClass.GetFeeCat();


               ///////
                           
               

             
                string Search = "Challanid like '%%' ";
                if (cmd == "Search")
                {                                  
                    if (!string.IsNullOrEmpty(obj.BANK))
                    {
                        Search += " and a.Bcode='" + obj.BANK.ToString().Trim() + "'";                       
                    }
                    if (!string.IsNullOrEmpty(obj.FEECAT))
                    {
                        Search += " and a.FEECAT='" + obj.FEECAT.ToString().Trim() + "'";
                    }
                    if (srhfld != "" && SearchString != "")
                    {
                        ViewBag.srhfld = srhfld;
                        if (srhfld == "CHALLANID")
                            Search += " and a.CHALLANID='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "APPNO")
                            Search += " and a.APPNO='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "SCHLREGID")
                            Search += " and a.SCHLREGID='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "LOT")
                            Search += " and a.DOWNLDFLOT='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "J_REF_NO")
                            Search += " and a.J_REF_NO='" + SearchString.ToString().Trim() + "'";                       
                    }


                    TempData["SearchString"] = Search.ToString().Trim();                    
                    obj.StoreAllData = _bankRepository.GetChallanDetails(Search, "", pageIndex); 
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = obj.StoreAllData.Tables[1].Rows[0]["TotalCnt"].ToString();
                        int tp = Convert.ToInt32(ViewBag.TotalCount1    );
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;                       
                        return View(obj);
                    }
                }
                return View();               
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        public JsonResult ChallanDetailsRegerate(float lumsumfine, string lumsumremarks, string challanid, string gdate, string schl, string vdate)
        {
            try
            {
                string dee = "No";
                string outError = "0";
                int outstatus = 0;                      
                float fee = 0;
                DateTime date, dateV;
                if (DateTime.TryParseExact(gdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    if (DateTime.TryParseExact(vdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateV))
                    {
                        if (dateV >= date)
                        {
                            outstatus = DBClass.ReGenerateChallaanByIdSPAdminNew(lumsumfine, lumsumremarks, challanid, out outError, date, fee, dateV);
                            if (Convert.ToInt32(outstatus) > 0)
                            {
                                dee = "Yes";
                            }
                            else
                            { dee = "No"; }
                        }
                        else
                        {
                            outError = "-5";
                            dee = "date";
                        }
                    }
                }
                return Json(new { sn = dee, chid = outError }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {            
                return null;
            }
        }


        // Cancel Challan
        public JsonResult ChallanDetailsCancel(string cancelremarks, string challanid, string Type)
        {
            try
            {

                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

                string dee = "";
                string outstatus = "";
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId);
                outstatus = DBClass.ChallanDetailsCancel(cancelremarks, challanid, out outstatus, AdminId, Type);//ChallanDetailsCancelSP
              
                dee = outstatus;
                return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion


        #region PSEBHOD ChallanDetails

        [AdminLoginCheckFilter]
        public ActionResult PSEBChallanDetails(Challan obj, int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];          
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {
                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);
                ViewBag.MySch1 = itemsch1.ToList();

                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsPrint = 1; ViewBag.IsView = 1; ViewBag.IsRegenerate = 1; ViewBag.IsCancel = 1; }
                else
                {

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                    if (aAct.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.IsPrint = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("HOME/GENERATECHALLAAN")).Count();
                        ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsRegenerate = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsCancel = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                    }
                }
                #endregion Action Assign Method

                // By Rohit  -- select bank from database
                string BankAllow = string.Empty;
                DataSet dsBank = DBClass.GetAdminDetailsById(Convert.ToInt32(adminLoginSession.AdminId), Convert.ToInt32(adminLoginSession.CurrentSession.Substring(0, 4)));
                if (dsBank != null)
                {
                    if (dsBank.Tables[3].Rows.Count >= 0)
                    {
                        //Bank
                        var _bankList = dsBank.Tables[3].AsEnumerable().Select(dataRow => new SelectListItem
                        {
                            Text = dataRow.Field<string>("Bank").ToString(),
                            Value = dataRow.Field<string>("Bcode").ToString(),
                        }).ToList();
                        obj.BankList = _bankList.ToList();
                    }
                }
                // End 
                obj.FeeCatList = DBClass.GetFeeCat();

                // Data searching by paging 
                if (TempData["PSEBSearchString"] != null)
                {
                    string Search = Convert.ToString(TempData["PSEBSearchString"]);
                    TempData["PSEBSearchString"] = Search;
                   // TempData.Keep();
                    obj.StoreAllData = _bankRepository.GetChallanDetails(Search, "", pageIndex);
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {

                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = obj.StoreAllData.Tables[1].Rows[0]["TotalCnt"].ToString();
                        int tp = Convert.ToInt32(ViewBag.TotalCount1);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                    }
                }
                // Data searching by paging 

                return View(obj);
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    

        [HttpPost]
        public ActionResult PSEBChallanDetails(int? page, Challan obj, FormCollection frm, string cmd, string BankName, string srhfld, string SearchString, string feecat1)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {

                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);
                ViewBag.MySch1 = itemsch1.ToList();


                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsPrint = 1; ViewBag.IsView = 1; ViewBag.IsRegenerate = 1; ViewBag.IsCancel = 1; }
                else
                {

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                    if (aAct.Tables[0].Rows.Count > 0)
                    {
                        //ViewBag.IsEdit = aAct.Tables[0].AsEnumerable().Where(c => c.Field<int>("MenuId").Equals(61)).Count();                        
                        ViewBag.IsPrint = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("HOME/GENERATECHALLAAN")).Count();
                        ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsRegenerate = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                        ViewBag.IsCancel = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("CHALLAN/CHALLANDETAILS")).Count();
                    }
                }
                #endregion Action Assign Method

                // By Rohit  -- select bank from database
                string BankAllow = string.Empty;
                DataSet dsBank = DBClass.GetAdminDetailsById(Convert.ToInt32(adminLoginSession.AdminId), Convert.ToInt32(adminLoginSession.CurrentSession.ToString().Substring(0, 4)));
                if (dsBank != null)
                {
                    if (dsBank.Tables[3].Rows.Count >= 0)
                    {
                        //Bank
                        var _bankList = dsBank.Tables[3].AsEnumerable().Select(dataRow => new SelectListItem
                        {
                            Text = dataRow.Field<string>("Bank").ToString(),
                            Value = dataRow.Field<string>("Bcode").ToString(),
                        }).ToList();
                        obj.BankList = _bankList.ToList();
                    }
                }
                // End 

                obj.FeeCatList = DBClass.GetFeeCat();


                ///////
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;


                string Search = "Challanid like '%%' ";
                if (cmd == "Search")
                {                  
                    if (!string.IsNullOrEmpty(obj.BANK))
                    {
                        // Search += " and a.Bcode='" + obj.BANK.ToString().Trim() + "'";
                        Search += " and a.Bcode='" + obj.BANK.ToString().Trim() + "'";
                    }
                    if (!string.IsNullOrEmpty(obj.FEECAT))
                    {
                        Search += " and a.FEECAT='" + obj.FEECAT.ToString().Trim() + "'";
                    }
                    if (srhfld != "" && SearchString != "")
                    {
                        ViewBag.srhfld = srhfld;
                        if (srhfld == "CHALLANID")
                            Search += " and a.CHALLANID='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "APPNO")
                            Search += " and a.APPNO='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "SCHLREGID")
                            Search += " and a.SCHLREGID='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "LOT")
                            Search += " and a.DOWNLDFLOT='" + SearchString.ToString().Trim() + "'";
                        else if (srhfld == "J_REF_NO")
                            Search += " and a.J_REF_NO='" + SearchString.ToString().Trim() + "'";
                    }


                    TempData["PSEBSearchString"] = Search.ToString().Trim();
                   // TempData.Keep();                  
                    obj.StoreAllData = _bankRepository.GetChallanDetails(Search, "", pageIndex);
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = obj.StoreAllData.Tables[1].Rows[0]["TotalCnt"].ToString();
                        int tp = Convert.ToInt32(ViewBag.TotalCount1);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        return View(obj);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        public JsonResult VerifyPSEBHOD(string receiptnumber, string depositdate, string challanid, string challandateV, string brcode)
        {
            try
            {
                BankModels BM = new BankModels();
                string dee = "No";
                string outError = "0";
                int outstatus = 0;
                string Search = string.Empty;
                string UserType = "Admin";
                float fee = 0;
                DateTime date, dateV;
                if (DateTime.TryParseExact(depositdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    if (DateTime.TryParseExact(challandateV, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateV))
                    {
                        if (dateV >= date)
                        {
                            DataSet ds1 = _bankRepository.GetChallanDetailsByIdSPBank(challanid);
                            if (ds1.Tables.Count > 0)
                            {
                                if (ds1.Tables[0].Rows.Count > 0)
                                {
                                    BM.BCODE = ds1.Tables[0].Rows[0]["BCODE"].ToString();
                                    BM.TOTFEE = Convert.ToInt32(ds1.Tables[0].Rows[0]["TOTFEE"].ToString());
                                }
                            }

                            BM.CHALLANID = challanid;
                            if (brcode == "203")
                            {
                                BM.BRCODE = "203";
                                BM.BRANCH = "PSEB";
                                BM.BCODE = "203";
                            }
                            else
                            {
                                BM.BRCODE = brcode;
                                BM.BRANCH = DBClass.GetDistE().Where(s => s.Value == brcode).Select(s => s.Text).FirstOrDefault();
                            }

                            BM.J_REF_NO = receiptnumber;
                            BM.DEPOSITDT = depositdate;
                            BM.MIS_FILENM = "";

                            int UPLOADLOT = 0;
                            string Mobile;
                            DataSet Bmis = _bankRepository.GetTotBankMIS(BM);
                            UPLOADLOT = Convert.ToInt32(Bmis.Tables[0].Rows[0][0].ToString());
                            DataTable dt = _bankRepository.ImportBankMisSPJuniorPSEB(BM, UPLOADLOT, out outstatus, out Mobile);
                            if (Convert.ToInt32(outstatus) > 0)
                            {
                                dee = "Yes";
                            }
                            else
                            { dee = "No"; }
                        }
                        else
                        {
                            outError = "-5";
                            dee = "date";
                        }
                    }
                }
                return Json(new { sn = dee, chid = outError }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {              
                return null;
            }
        }

        #endregion PSEBHOD ChallanDetails

        #region registration CalculateFeeAdmin

        [AdminLoginCheckFilter]
        public ActionResult CalculateFeeAdmin(string Status)
        {
            FeeHomeViewModel fhvm = new FeeHomeViewModel();
            TempData["calfeeschl"] = null;
            ViewData["Status"] = ViewBag.Schl = ViewBag.SchlName = "";
            ViewData["FeeStatus"] = null;
            ViewBag.date = DateTime.Now.ToString("dd/MM/yyyy");
            return View(fhvm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult CalculateFeeAdmin(string Status, string SearchString, string cmd, string schl, FormCollection frc, FeeHomeViewModel fhvm,string selectedClass)
        {
            try
            {
                ViewBag.date = DateTime.Now.ToString("dd/MM/yyyy");
                DateTime date = DateTime.ParseExact(SearchString, "dd/MM/yyyy", null);
                ViewBag.Searchstring = SearchString;

                string UserType = fhvm.Type;

                if (schl == null || schl == "")
                { return RedirectToAction("CalculateFeeAdmin", "Challan"); }

             
                TempData["calfeeschl"] = ViewBag.Schl = schl.ToString();

                if (Status == "Successfully" || Status == "Failure")
                {
                    //ViewData["Status"] = Status;
                    ViewData["FeeStatus"] = Status;
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
                    //string UserType = "User";



                    string Search = string.Empty;
                    Search = "SCHL='" + schl.ToString() + "'";
                    FormNM = "'" + FormNM.Replace(",", "','") + "'";
                    Search += "  and type='" + UserType + "' and form_name in(" + FormNM + ")";



                    DataSet dsCheckFee = _challanRepository.CheckFeeStatus(schl.ToString(), UserType, FormNM.ToUpper(), date);//CheckFeeStatusSPByView

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


                                // return View(fhvm);
                            }
                            else if (dsCheckFee.Tables[0].Rows[0]["Outstatus"].ToString() == "1")
                            {
                                string cls = selectedClass;
                                DataSet ds = _challanRepository.GetCalculateFeeBySchool(cls,Search, schl.ToString(), date);
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


        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult GenerateLumsumFineChallan(string lumsumfine, string lumsumremarks, FormCollection frm, string ValidDate)
        {
            PaymentformViewModel pfvm = new PaymentformViewModel();           
          
            if (TempData["CalculateFee"] == null || TempData["CalculateFee"].ToString() == "")
            {
                return RedirectToAction("CalculateFeeAdmin", "Challan");
            }

            if (TempData["calfeeschl"] == null || TempData["calfeeschl"].ToString() == "")
            {
                return RedirectToAction("CalculateFeeAdmin", "Challan");
            }

         
            string schl = string.Empty;
            schl = TempData["calfeeschl"].ToString();           
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

                TempData["PaymentForm"] = pfvm;

                //string CheckFee = ds.Tables[1].Rows[0]["TotalFeeAmount"].ToString();
                if (pfvm.TotalFinalFees == 0 && pfvm.TotalFees == 0)
                {
                    ViewBag.CheckForm = 1; // only verify for M1 and T1 
                    TempData["CheckFormFeeAdmin"] = 0;
                }
                else
                {
                    ViewBag.CheckForm = 0; // only verify for M1 and T1 
                    TempData["CheckFormFeeAdmin"] = 1;
                }
            }
            //post here

            ChallanMasterModel CM = new ChallanMasterModel();
            pfvm.BankCode = "203";
            if (TempData["FeeStudentList"] == null || TempData["FeeStudentList"].ToString() == "")
            {
                return RedirectToAction("CalculateFeeAdmin", "Challan");
            }            
            if (ModelState.IsValid)
            {
                string SCHL = TempData["calfeeschl"].ToString();
                string FeeStudentList = TempData["FeeStudentList"].ToString();
                CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);

                CM.FEE = Convert.ToInt32(pfvm.TotalFinalFees);
                CM.TOTFEE = Convert.ToInt32(pfvm.TotalFinalFees);
                string TotfeePG = (CM.TOTFEE).ToString();
                CM.addfee = Convert.ToInt32(pfvm.totaddfee);// exam fee
                CM.regfee = Convert.ToInt32(pfvm.TotalFees);
                CM.latefee = Convert.ToInt32(pfvm.TotalLateFees);
                CM.FEECAT = pfvm.FeeCategory;
                CM.FEECODE = pfvm.FeeCode;
                CM.FEEMODE = "CASH";
                CM.BANK = "PSEB HOD";
                CM.BCODE = pfvm.BankCode;
                CM.BANKCHRG = pfvm.BankCharges;
                CM.SchoolCode = pfvm.SchoolCode.ToString();
                CM.DIST = pfvm.Dist.ToString();
                CM.DISTNM = pfvm.District;
                CM.LOT = pfvm.LOTNo;
                CM.SCHLREGID = pfvm.SchoolCode.ToString();
                CM.type = "schle";
                DateTime CHLNVDATE2;
                if (DateTime.TryParseExact(ValidDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                {
                    CM.ChallanVDateN = CHLNVDATE2;
                }
                //CM.CHLNVDATE = pfvm.FeeDate;
                CM.CHLNVDATE = ValidDate;
                CM.LumsumFine = Convert.ToInt32(lumsumfine);
                CM.LSFRemarks = lumsumremarks;

                string SchoolMobile = "";
                string result = "0";
                result = _challanRepository.InsertPaymentFormJunior(CM, out SchoolMobile);
                if (result == "0")
                {
                    //--------------Not saved
                    ViewData["result"] = 0;
                }
                if (result == "-1")
                {
                    //-----alredy exist
                    ViewData["result"] = -1;
                }
                else
                {                    
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
            return View();
        }


        #endregion registration CalculateFeeAdmin



    }
}