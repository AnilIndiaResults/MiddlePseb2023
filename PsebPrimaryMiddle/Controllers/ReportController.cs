using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.Models;
using System.Data;
using System.Web.UI;
using System.IO;
using System.Web.Routing;
using ClosedXML;
using ClosedXML.Excel;
using System.Data.OleDb;
using PsebPrimaryMiddle.Filters;
using PsebJunior.AbstractLayer;
using PsebPrimaryMiddle.Repository;

namespace PsebPrimaryMiddle.Controllers
{
   
    public class ReportController : Controller
    {

        private readonly IReportRepository _reportRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly ICenterHeadRepository _centerheadrepository;

        public ReportController(IReportRepository reportRepository, ICenterHeadRepository centerheadrepository, IAdminRepository adminRepository)
        {
            _centerheadrepository = centerheadrepository;
            _reportRepository = reportRepository;
            _adminRepository = adminRepository;
        }

        // GET: Report
        public ActionResult Index()
        {
            return View();
        }



        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult PrimaryMiddleSummaryReports(ReportModel rp, string id = "0")
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {                
                ViewBag.tab = id;
                int tid = Convert.ToInt32(id);
               rp.StoreAllData = _reportRepository.PrimaryMiddleSummaryReportsByType(tid);  //1 for Total Registration by School Report
                TempData["ExportDataFromDataTable"] = rp.StoreAllData;
                if (Request.IsAjaxRequest())
                {                    
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    if (tid == 1)
                    {
                        TempData["FileNAME"] = "primarymiddle_formwisereport";
                        return PartialView("_primarymiddle_summaryreport", rp);
                    }
                    else if (tid == 2)
                    {
                        TempData["FileNAME"] = "primarymiddle_formwisereport";
                        return PartialView("_primarymiddle_formwisereport", rp);
                    }
                    else if (tid == 5)
                    {
                        TempData["FileNAME"] = "regexamformfeesummary";
                        return PartialView("_regexamformfeesummary", rp);
                    }
                    else if (tid == 6)
                    {
                        TempData["FileNAME"] = "pendingschoolsforregistration";
                        return PartialView("_pendingschoolsforregistration", rp);
                    }
                    else if (tid == 7)
                    {
                        TempData["FileNAME"] = "regnoerrorsummary";
                        return PartialView("_regnoerrorsummary", rp);
                    }
                    else if (tid == 8)
                    {
                        TempData["FileNAME"] = "_UserTypeWiseStudentCount";
                        return PartialView("_UserTypeWiseStudentCount", rp);
                    }
                }
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    return View(rp);
                }
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult ExportDataFromDataTable(DataSet ds, string FileNAME)
        {
            try
            {


                if (TempData["ExportDataFromDataTable"] == null)
                {
                    return RedirectToAction("PrimaryMiddleSummaryReports", "Report");
                }
                else
                {
                    ds = (DataSet)TempData["ExportDataFromDataTable"];
                }

                string fileName1 = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    fileName1 = FileNAME + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                    // dt.TableName = fileName1;
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(ds.Tables[0]);
                        wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        wb.Style.Font.Bold = true;
                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + "");
                        //Response.AddHeader("content-disposition", "attachment;filename= DownloadChallanReport.xlsx");

                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("PrimaryMiddleSummaryReports", "Report");
        }


        public ActionResult ExportToExcelDataFromDataTable(string File, string PageType)
        {
            try
            {

                DataTable dt = new DataTable();

                if (TempData["ExportToExcelDataFromDataTable"] == null)
                {
                    
                        if (PageType.ToLower() == "clustersubjectstatusreport") { return RedirectToAction("ClusterSubjectStatusReport", "Report"); }
                    else if (PageType.ToLower() == "subjectwisereport") { return RedirectToAction("SubjectWiseReports", "Report"); }
                    else
                        return RedirectToAction("PrimaryMiddleSummaryReports", "Report");
                }
                else
                {
                    dt = (DataTable)TempData["ExportToExcelDataFromDataTable"];
                }

                string fileName1 = "";
                if (dt.Rows.Count > 0)
                {


                    fileName1 = File.Replace(" ", "") + "_" + PageType + "_Data_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                    // dt.TableName = fileName1;
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
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

                }
            }
            catch (Exception ex)
            {

            }

            if (PageType.ToLower() == "clustersubjectstatusreport") { TempData["ExportToExcel"] = "1";
                return RedirectToAction("ClusterSubjectStatusReport", "Report"); }
            else     if (PageType.ToLower() == "subjectwisereport") {
                TempData["ExportToExcel"] = "1";
                return RedirectToAction("SubjectWiseReports", "Report");
            }
            else
            { return RedirectToAction("PrimaryMiddleSummaryReports", "Report"); }
        }




        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult SubjectWiseReports(ReportModel RM)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                var itemsch = new SelectList(new[] {
                    new { ID = "1", Name = "Subject Wise" }, new { ID = "2", Name = "District Subject Wise" },
                     new { ID = "3", Name = "School Wise" },new { ID = "4", Name = "School Subject Wise" },
                       new { ID = "5", Name = "District School Subject Wise" },new { ID = "6", Name = "District School Wise" },
                }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";

                ViewBag.DistEList = DBClass.GetDistE();
                ViewBag.SelectedDIST = "0";

                return View(RM);
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult SubjectWiseReports(ReportModel RM, FormCollection frm, string submit)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                var itemsch = new SelectList(new[] {
                    new { ID = "1", Name = "Subject Wise" }, new { ID = "2", Name = "District Subject Wise" },
                     new { ID = "3", Name = "School Wise" },new { ID = "4", Name = "School Subject Wise" },
                       new { ID = "5", Name = "District School Subject Wise" },new { ID = "6", Name = "District School Wise" },
                }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";

                ViewBag.DistEList = DBClass.GetDistE();
                ViewBag.SelectedDIST = "0";
             
                string Search = string.Empty;
                if (!string.IsNullOrEmpty(frm["SelList"]) && !string.IsNullOrEmpty(frm["SelClass"]))
                {
                    ViewBag.SelectedItem = frm["SelList"] == null ? "ALL" : frm["SelList"];
                    ViewBag.SelectedItemText = frm["SelList"] == null ? "ALL" : itemsch.ToList().Where(s => s.Value == ViewBag.SelectedItem).Select(s => s.Text).FirstOrDefault();
                    ViewBag.Selectedcls = frm["SelClass"] == null ? "ALL" : frm["SelClass"];
                    ViewBag.SelectedDIST = frm["SelDist"] == null ? "ALL" : frm["SelDist"];
                    RM.StoreAllData = _reportRepository.SubjectWiseReports(ViewBag.SelectedItem, Convert.ToInt32(ViewBag.Selectedcls), ViewBag.SelectedDIST);
                    if (RM.StoreAllData == null || RM.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        TempData["ExportToExcelDataFromDataTable"] = RM.StoreAllData.Tables[0];
                        if (submit != null)
                        {
                            if (submit.ToLower().Contains("excel") || submit.ToLower().Contains("download"))
                            {
                                TempData["ExportToExcel"] = "1";
                                ExportToExcelDataFromDataTable(ViewBag.SelectedItemText, "SubjectWiseReport");
                            }
                        }
                        ViewBag.TotalCount = RM.StoreAllData.Tables[0].Rows.Count;
                    }
                }
                else
                {
                    ViewBag.Message = "2";
                    ViewBag.TotalCount = 0;
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            return View(RM);
        }



        #region RegistrationReport
        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult RegistrationReport()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
         
            DataSet ds = new DataSet();          
            ds = _adminRepository.GetAllAdminUser(adminLoginSession.AdminId,"");
            List<SelectListItem> forms = new List<SelectListItem>();
            List<SelectListItem> districts = new List<SelectListItem>();
            List<SelectListItem> category = new List<SelectListItem>();
            // category.Add(new SelectListItem { Text = "All", Value = "0", Selected = true });
            category.Add(new SelectListItem { Text = "Reg Pending", Value = "1" });
            category.Add(new SelectListItem { Text = "Reg Error", Value = "2" });
            category.Add(new SelectListItem { Text = "Reg Descrepancy", Value = "3" });
            ViewBag.cat = category;
            TempData["category"] = category;

            if (ds.Tables.Count > 1)
            {
                if (ds.Tables[3].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[3].Rows)
                    {
                        forms.Add(new SelectListItem { Text = dr["form_name"].ToString(), Value = dr["form_name"].ToString() });
                    }
                }
                if (ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        districts.Add(new SelectListItem { Text = dr["DISTNM"].ToString(), Value = dr["DIST"].ToString() });
                    }
                }
            }
            // districts.Insert(0, new SelectListItem { Text = "-- Select District --", Value = "0" });
            ViewBag.districts = districts;
            TempData["districts"] = districts;
            //forms.Insert(0, new SelectListItem { Text = "-- Select Form --", Value = "0" });
            ViewBag.forms = forms;
            TempData["forms"] = forms;
            ViewBag.TotalCount = 0;
            return View();
        }

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult RegistrationReport(FormCollection fc)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
           
            DataSet ds = new DataSet();
            ds = _adminRepository.GetAllAdminUser(adminLoginSession.AdminId, "");

            List<SelectListItem> forms = new List<SelectListItem>();
            List<SelectListItem> districts = new List<SelectListItem>();
            List<SelectListItem> category = new List<SelectListItem>();
            // category.Add(new SelectListItem { Text = "All", Value = "0", Selected = true });
            category.Add(new SelectListItem { Text = "Reg Pending", Value = "1" });
            category.Add(new SelectListItem { Text = "Reg Error", Value = "2" });
            category.Add(new SelectListItem { Text = "Reg Descrepancy", Value = "3" });
            ViewBag.cat = category;
            TempData["category"] = category;           

            forms = (List<SelectListItem>)TempData["forms"];
            districts = (List<SelectListItem>)TempData["districts"];


            ViewBag.districts = districts;
            TempData["districts"] = districts;

            ViewBag.forms = forms;
            TempData["forms"] = forms;

            ViewBag.SelectedDist = "0";
            ViewBag.SelectedForm = "0";
            ViewBag.SelectedCategory = "0";

            string search = "R.std_id like '%%'";
            if (fc["district"] != null && fc["district"].ToString() != "0" && fc["district"].ToString() != "")
            {
                search += " and R.DIST='" + fc["district"].ToString() + "'";
                ViewBag.SelectedDist = fc["district"].ToString();
            }
            if (fc["form"] != null && fc["form"].ToString() != "0" && fc["form"].ToString() != "")
            {
                search += " and R.Form_Name ='" + fc["form"].ToString() + "'";
                ViewBag.SelectedForm = fc["form"].ToString();
            }
            if (fc["category"] != null && fc["category"].ToString() != "0" && fc["category"].ToString() != "")
            {
                switch (fc["category"].ToString().Trim())
                {
                    case "1": search += " and R.regno is null"; break;
                    case "2": search += " and R.regno like 'ERR%'"; break;
                    case "3": search += " and R.regno like '%:ERR%'"; break;
                }
                ViewBag.SelectedCategory = fc["category"].ToString();

                List<SelectListItem> allcat = (List<SelectListItem>)TempData["category"];
                foreach (var i in allcat)
                {
                    if (i.Value.ToUpper() == ViewBag.SelectedCategory.ToUpper())
                    {
                        i.Selected = true;
                        break;
                    }
                }
                ViewBag.cat = allcat;
                TempData["category"] = allcat;

            }
            if (fc["schl"] != null && fc["schl"].ToString().Trim() != string.Empty)
            {
                search += " and R.SCHL ='" + fc["schl"].ToString().Trim() + "'";
                ViewBag.SCHL = fc["schl"].ToString();
            }


            ds = new DataSet();
            ds = _reportRepository.RegistrationReportSearch(search, adminLoginSession.CurrentSession);
            ViewBag.data = ds;
            if (ds == null || ds.Tables[0].Rows.Count == 0)
            {
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = ds.Tables[0].Rows.Count;
            }

            return View();
        }

        #endregion RegistrationReport



        #region ClassSummaryReport
        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult ClassSummaryReport()
        {
            
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "C.C.E" }, new { ID = "2", Name = "Practical Summary Report" },
                new { ID = "3", Name = "Elective Theory" }, new { ID = "4", Name = " Differently Abled" },}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";
                return View();            
        }


        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult ClassSummaryReport(ReportModel RM, FormCollection frm, string Category, string submit) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "C.C.E" }, new { ID = "2", Name = "Practical Summary Report" },
                new { ID = "3", Name = "Elective Theory" }, new { ID = "4", Name = " Differently Abled" },}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";


                string id = frm["Filevalue"].ToString();
                Category = id;

                string Search = string.Empty;
                if (frm["SelList"] != "" && frm["SelClass"] != "")
                {
                    ViewBag.SelectedItem = frm["SelList"];
                    ViewBag.Selectedcls = frm["SelClass"];
                    //TempData["SearchDuplicateCertificate"] = Search;
                    RM.StoreAllData = _reportRepository.ClassSummaryReportByType(Convert.ToInt32(frm["SelList"]), Convert.ToInt32(frm["SelClass"]));
                    if (RM.StoreAllData == null || RM.StoreAllData.Tables[0].Rows.Count == 0)
                    {

                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        TempData["ExportDataFromDataTable"] = RM.StoreAllData;
                        if (submit != null)
                        {
                            if (submit.ToUpper().Contains("DOWNLOAD"))
                            {
                                if (RM.StoreAllData.Tables[0] != null)
                                {
                                    string Type = itemsch.ToList().Where(s=>s.Value== ViewBag.SelectedItem).Select(s => s.Text).SingleOrDefault();
                                    string fileName1 = Type.Replace(" ","").ToUpper().Trim() + "_SummaryReport";
                                    ExportDataFromDataTable(RM.StoreAllData, fileName1);
                                }
                            }
                        }
                        ViewBag.TotalCount = RM.StoreAllData.Tables[0].Rows.Count;
                        return View(RM);
                    }
                }
                else
                {
                    ViewBag.Message = "2";
                    ViewBag.TotalCount = 0;
                    return View();
                }


            }
            catch (Exception ex)
            {

                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }

        }

        #endregion CCE Summary Report



        #region  CategoryWiseFeeCollectionDetails
        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult CategoryWiseFeeCollectionDetails(ReportModel rp)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                // By Rohit  -- select bank from database
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                //DataSet dsBank = _reportRepository.Fll_FeeCat_Details();
                if (dsBank != null)
                {
                    if (dsBank.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[0].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["FEECAT"].ToString().Trim(), Value = @dr["FEECAT"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }
                }               
                // End 

                string Search = string.Empty;
                Search = "a.FEECODE like '%' ";

                string outError = "";
                DataSet ds = _reportRepository.CategoryWiseFeeCollectionDetails(Search, out outError);  //1 for Total Registration by School Report
                rp.StoreAllData = ds;
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewBag.Total = 0;
                    return View(rp);
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    string typeName = rp.StoreAllData.Tables[0].Columns["TOTFEE"].DataType.Name;
                    ViewBag.Total = rp.StoreAllData.Tables[0].AsEnumerable().Sum(x => Convert.ToInt32(x.Field<Double>("TOTFEE")));
                    ViewBag.AmountInWords = DBClass.GetAmountInWords(Convert.ToInt32(ViewBag.Total));
                    return View(rp);
                }
            }
            catch (Exception ex)
            {
                //oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult CategoryWiseFeeCollectionDetails(ReportModel rp,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
               
                // By Rohit  -- select bank from database
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                if (dsBank != null)
                {
                    if (dsBank.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[0].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["FEECAT"].ToString().Trim(), Value = @dr["FEECAT"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }
                }
               
                // End 
                string Search = string.Empty;
                Search = "a.FEECODE like '%' ";

                if (frm["FEECAT"] != "")
                {
                    Search += " and a.FEECAT='" + frm["FEECAT"].ToString().Trim() + "'";
                    ViewBag.SelectedItem = frm["FEECAT"];
                    TempData["FEECAT"] = frm["FEECAT"];
                }

                if (frm["Branch"] != "")
                {                    
                    Search += " and b.NODAL_BRANCH like '" + frm["Branch"].ToString().Trim() + "'";
                    ViewBag.Branch = frm["Branch"];
                    TempData["Branch"] = frm["Branch"];
                }

                string outError = "";
                DataSet ds = _reportRepository.CategoryWiseFeeCollectionDetails(Search, out outError);  //1 for Total Registration by School Report
                rp.StoreAllData = ds;
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewBag.Total = 0;
                    return View(rp);
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    string typeName = rp.StoreAllData.Tables[0].Columns["TOTFEE"].DataType.Name;
                    ViewBag.Total = rp.StoreAllData.Tables[0].AsEnumerable().Sum(x => Convert.ToInt32(x.Field<Double>("TOTFEE")));
                    ViewBag.AmountInWords = DBClass.GetAmountInWords(Convert.ToInt32(ViewBag.Total));
                    return View(rp);
                }
            }
            catch (Exception ex)
            {
                // oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }
        #endregion CategoryWiseFeeCollectionDetails

        #region  BankWiseFeeCollectionDetails

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult BankWiseFeeCollectionDetails(ReportModel rp)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
         
            try
            {
               
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                if (dsBank != null)
                {
                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["Bankname"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "Admin");
                }

                // End 

                string Search = string.Empty;
                Search = "a.bcode like '%' ";

                string outError = "";
                DataSet ds = _reportRepository.BankWiseFeeCollectionDetails(Search, out outError);  //1 for Total Registration by School Report
                rp.StoreAllData = ds;
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewBag.Total = 0;
                    return View(rp);
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    string typeName = rp.StoreAllData.Tables[0].Columns["TOTFEE"].DataType.Name;
                    ViewBag.Total = rp.StoreAllData.Tables[0].AsEnumerable().Sum(x => Convert.ToInt32(x.Field<Double>("TOTFEE")));
                   // ViewBag.AmountInWords = DBClass.GetAmountInWords(Convert.ToInt32(ViewBag.Total));
                    return View(rp);
                }
            }
            catch (Exception ex)
            {
                // oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));                
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult BankWiseFeeCollectionDetails(ReportModel rp,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
           
            try
            {
               
                // By Rohit  -- select bank from database
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                if (dsBank != null)
                {
                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["Bankname"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }
                }
              

                // End 
                string Search = string.Empty;
                Search = "a.bcode like '%' ";

                if (frm["BankName"] != "")
                {
                    Search += " and a.Bcode='" + frm["BankName"].ToString().Trim() + "'";
                    ViewBag.SelectedItem = frm["BankName"];
                    TempData["BankName"] = frm["BankName"];
                }

                if (frm["Branch"] != "")
                {
                    Search += " and b.NODAL_BRANCH like '%" + frm["Branch"].ToString().Trim() + "%'";
                    ViewBag.Branch = frm["Branch"];
                    TempData["Branch"] = frm["Branch"];
                }

                string outError = "";
                DataSet ds = _reportRepository.BankWiseFeeCollectionDetails(Search, out outError);  //1 for Total Registration by School Report
                rp.StoreAllData = ds;
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewBag.Total = 0;
                    return View(rp);
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    string typeName = rp.StoreAllData.Tables[0].Columns["TOTFEE"].DataType.Name;
                    ViewBag.Total = rp.StoreAllData.Tables[0].AsEnumerable().Sum(x => Convert.ToInt32(x.Field<Double>("TOTFEE")));
                    ViewBag.AmountInWords = DBClass.GetAmountInWords(Convert.ToInt32(ViewBag.Total));
                    return View(rp);
                }
            }
            catch (Exception ex)
            {
                //oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }
        #endregion BankWiseFeeCollectionDetails


        #region  DateWiseFeeCollectionDetails

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult DateWiseFeeCollectionDetails(ReportModel rp)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
           
            try
            {
                
                // By Rohit  -- select bank from database
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                if (dsBank != null)
                {
                    if (dsBank.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[0].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["FEECAT"].ToString().Trim(), Value = @dr["FEECAT"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }

                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank1 = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank1.Add(new SelectListItem { Text = @dr["Bankname"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        ViewBag.MyBank = itemBank1.ToList();
                    }
                }              
                var itemcls = new SelectList(new[] { new { ID = "1", Name = "Date Fee Head and Bank Wise " }, new { ID = "2", Name = "Date and Fee Head Wise " },
                    new { ID = "3", Name = "Date Wise" }, new { ID = "4", Name = "Fee Head and Bank Wise" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";

                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                ViewBag.Total = 0;
                return View(rp);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult DateWiseFeeCollectionDetails(ReportModel rp,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {          
                // By Rohit  -- select bank from database
                DataSet dsBank = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecod
                if (dsBank != null)
                {
                    if (dsBank.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[0].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["FEECAT"].ToString().Trim(), Value = @dr["FEECAT"].ToString().Trim() });
                        }
                        ViewBag.MySch = itemBank.ToList();
                    }

                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank1 = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank1.Add(new SelectListItem { Text = @dr["Bankname"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        ViewBag.MyBank = itemBank1.ToList();
                    }
                }
                
                var itemcls = new SelectList(new[] { new { ID = "1", Name = "Date Fee Head and Bank Wise " }, new { ID = "2", Name = "Date and Fee Head Wise " },
                    new { ID = "3", Name = "Date Wise" }, new { ID = "4", Name = "Fee Head and Bank Wise" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";


                // End 
                string Search = string.Empty;
                Search = "a.FEECODE like '%' ";


                if (frm["SelClass"] != "")
                {
                    ViewBag.Selectedcls = frm["SelClass"];
                    TempData["SelClass"] = frm["SelClass"];
                }

                if (frm["FEECAT"] != "")
                {
                    Search += " and a.FEECAT='" + frm["FEECAT"].ToString().Trim() + "'";
                    ViewBag.SelectedItem = frm["FEECAT"];
                    TempData["FEECAT"] = frm["FEECAT"];
                }
                if (frm["Bank"] != "")
                {
                    Search += " and a.bcode='" + frm["Bank"].ToString().Trim() + "'";
                    ViewBag.SelectedBank = frm["Bank"];
                    TempData["Bank"] = frm["Bank"];
                }


                if (frm["FromDate"] != "")
                {
                    ViewBag.FromDate = frm["FromDate"];
                    TempData["FromDate"] = frm["FromDate"];
                    Search += " and CONVERT(DATETIME, CONVERT(varchar(10),DEPOSITDT,103), 103)>=CONVERT(DATETIME, CONVERT(varchar(10),'" + frm["FromDate"].ToString() + "',103), 103)";
                }
                if (frm["ToDate"] != "")
                {
                    ViewBag.ToDate = frm["ToDate"];
                    TempData["ToDate"] = frm["ToDate"];
                    Search += " and CONVERT(DATETIME, CONVERT(varchar(10),DEPOSITDT,103), 103)<=CONVERT(DATETIME, CONVERT(varchar(10),'" + frm["ToDate"].ToString() + "',103), 103)";
                }

                string outError = "";
                DataSet ds = _reportRepository.DateWiseFeeCollectionDetails(Search, ViewBag.Selectedcls, out outError);  //1 for Total Registration by School Report
                rp.StoreAllData = ds;
                if (rp.StoreAllData == null || rp.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    ViewBag.Total = 0;
                    return View(rp);
                }
                else
                {
                    ViewBag.TotalCount = rp.StoreAllData.Tables[0].Rows.Count;
                    string typeName = rp.StoreAllData.Tables[0].Columns["TOTFEE"].DataType.Name;
                    ViewBag.Total = rp.StoreAllData.Tables[0].AsEnumerable().Sum(x => Convert.ToInt32(x.Field<Double>("TOTFEE")));
                    ViewBag.AmountInWords = DBClass.GetAmountInWords(Convert.ToInt32(ViewBag.Total));
                    return View(rp);
                }
            }
            catch (Exception ex)
            {
                // oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }
        #endregion DateWiseFeeCollectionDetails

        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult StatusofCorrection(ReportModel RM)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {             
               RM.StoreAllData = _reportRepository.StatusofCorrection();
                if (RM.StoreAllData != null)
                {
                    ViewBag.TotalCount = RM.StoreAllData.Tables[0].Rows.Count;
                    return View(RM);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    return View(RM);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult MigrationCount(ReportModel rm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                    string search = "";
                    string outError = "";
                    rm.StoreAllData = _reportRepository.MigrationCountReport(search, out outError);
                    if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                    }
                    return View(rm);
              
            }
            catch (Exception ex)
            {
              
                return View();
            }
        }


        #region Cluster/CenterHead Reports 

        public ActionResult ClusterRegisterReport(ReportModel rm)
        {

            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" },
                new { ID = "2", Name = "Cluster Code" }, new { ID = "3", Name = "UDISE Code" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";

            string userNM = "admin";
            if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());

            }

            if (Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());

            }


           

            try
            {
                
                ViewBag.TotalCount = 0;
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }

        [HttpPost]
        public ActionResult ClusterRegisterReport(ReportModel rm, FormCollection frm,string SearchList, string SearchString)
        {
            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" },
                new { ID = "2", Name = "Cluster Code" }, new { ID = "3", Name = "UDISE Code" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";
            string userNM = "admin";
             string userId = "";
            if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                userId = adminLoginSession.AdminId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());

            }

            if (Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                userId = centerHeadLoginSession.CenterHeadId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());
              
            }

            try
            {              
                string outError = "";

                string Search = string.Empty;
                Search = "ccode like '%' ";

                if (!string.IsNullOrEmpty(rm.Dist))
                {
                    Search += " and dist ='" + rm.Dist.ToString() + "' ";
                }

                if (!string.IsNullOrEmpty(SearchList))
                {
                    ViewBag.SelectedSearch = SearchList;
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        if (SearchList == "1")
                        { Search += " and schl ='" + SearchString.ToString() + "'"; }
                        else if (SearchList == "2")
                        { Search += " and ccode ='" + SearchString.ToString() + "'"; }
                        else if (SearchList == "3")
                        { Search += " and chtudise ='" + SearchString.ToString() + "'"; }

                        ViewBag.SearchString = SearchString;
                        TempData["SearchString"] = SearchString;
                    }
                }

                rm.StoreAllData = _reportRepository.ClusterRegisterReport(userId, userNM,Search, out outError);
                if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                {
                    ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                }
                else
                {
                    ViewBag.TotalCount = 0;
                }
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }



        public ActionResult ClusterMarkingStatusReport(ReportModel rm)
        {
            


            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" },
                new { ID = "2", Name = "Cluster Code" }, new { ID = "3", Name = "UDISE Code" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";

            string userNM = "admin";
            if (Session["AdminLoginSession"] != null)
            {       
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());

                var itemReportType = new SelectList(new[] {
            new { ID = "1", Name = "School Marking Status" }, new { ID = "2", Name = "Marking Status Pending School" },
                 new { ID = "4", Name = "District Wise Count Report" },}, "ID", "Name", 1);
                ViewBag.MyReportType = itemReportType.ToList();
                ViewBag.SelectedReportType = "0";

            }
            else if (Session["CenterHeadLoginSession"] != null)
            {
                
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());

                var itemReportType = new SelectList(new[] {new { ID = "2", Name = "Marking Status Pending School" },}, "ID", "Name", 1);
                ViewBag.MyReportType = itemReportType.ToList();
                ViewBag.SelectedReportType = "2";

            }
            


            try
            {

                ViewBag.TotalCount = 0;
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }

        [HttpPost]
        public ActionResult ClusterMarkingStatusReport(ReportModel rm, FormCollection frm, string SearchReportType, string SearchList, string SearchString)
        {

           
            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" },
                new { ID = "2", Name = "Cluster Code" }, new { ID = "3", Name = "UDISE Code" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";
            string userNM = "admin";
            string userId = "";
            if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                userId = adminLoginSession.AdminId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());

                var itemReportType = new SelectList(new[] {
            new { ID = "1", Name = "School Marking Status" }, new { ID = "2", Name = "Marking Status Pending School" },
                new { ID = "4", Name = "District Wise Count Report" },}, "ID", "Name", 1);
                ViewBag.MyReportType = itemReportType.ToList();
                ViewBag.SelectedReportType = "0";

            }

            if (Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                userId = centerHeadLoginSession.CenterHeadId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());
              

                var itemReportType = new SelectList(new[] {
                new { ID = "2", Name = "Marking Status Pending School" },}, "ID", "Name", 1);
                ViewBag.MyReportType = itemReportType.ToList();
                ViewBag.SelectedReportType = "2";

            }

            try
            {
                string outError = "";                
                string Search = string.Empty;
                Search = "ccode like '%' ";

                if (!string.IsNullOrEmpty(rm.Dist))
                {
                    Search += " and dist ='" + rm.Dist.ToString() + "' ";
                }

                if (!string.IsNullOrEmpty(SearchReportType))
                {
                    ViewBag.SelectedReportType = SearchReportType;                   
                    TempData["SearchReportType"] = SearchReportType;

                    if (!string.IsNullOrEmpty(SearchList) && SearchReportType != "4")
                    {
                        ViewBag.SelectedSearch = SearchList;
                        if (!string.IsNullOrEmpty(SearchString))
                        {
                            if (SearchList == "1")
                            { Search += " and schl ='" + SearchString.ToString() + "'"; }
                            else if (SearchList == "2")
                            { Search += " and ccode ='" + SearchString.ToString() + "'"; }
                            else if (SearchList == "3")
                            { Search += " and chtudise ='" + SearchString.ToString() + "'"; }

                            ViewBag.SearchString = SearchString;
                            TempData["SearchString"] = SearchString;
                        }
                    }

                }


                rm.StoreAllData = _reportRepository.ClusterMarkingStatusReport(Convert.ToInt32(ViewBag.SelectedReportType),userId, userNM, Search, out outError);
                if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                {
                    ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                }
                else
                {
                    ViewBag.TotalCount = 0;
                }
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }




        public ActionResult ClusterSubjectStatusReport(ClusterReportModel rm)
        {

            var itemReportType = new SelectList(new[] { new { ID = "3", Name = "Subject Status" },
                new { ID = "5", Name = "Final Submit Pending Status" },}, "ID", "Name", 1);
            ViewBag.MyReportType = itemReportType.ToList();
            ViewBag.SelectedReportType = "0";


            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "Pending" }, }, "ID", "Name", 1);
            //     new { ID = "2", Name = "Marks  Entered" },
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";


            ClusterReportModel clusterReportModel = new ClusterReportModel();
            clusterReportModel = _reportRepository.BindAllListofClusterReport();
            rm.ClusterList = new List<ClusterModel>();
            rm.SubList = clusterReportModel.SubList;

            string userNM = "admin";
            if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());

            }
            else if (Session["CenterHeadLoginSession"] != null)
            {

                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());
            }



            try
            {

                ViewBag.TotalCount = 0;
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }

        [HttpPost]
        public ActionResult ClusterSubjectStatusReport(ClusterReportModel rm, FormCollection frm, string cmd, string SearchReportType, string SearchList)
        {

            var itemReportType = new SelectList(new[] { new { ID = "3", Name = "Subject Status" },
                new { ID = "5", Name = "Final Submit Pending Status" },}, "ID", "Name", 1);
            ViewBag.MyReportType = itemReportType.ToList();
            ViewBag.SelectedReportType = "0";



            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "Pending" }, }, "ID", "Name", 1);
            //     new { ID = "2", Name = "Marks  Entered" },

            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";


            ClusterReportModel clusterReportModel = new ClusterReportModel();
            clusterReportModel = _reportRepository.BindAllListofClusterReport();
            rm.ClusterList = new List<ClusterModel>();
            rm.SubList = clusterReportModel.SubList;

            string userNM = "admin";
            string userId = "";
            if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                userNM = "admin";
                userId = adminLoginSession.AdminId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, adminLoginSession.AdminId.ToString());
                
            }

            if (Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                userNM = "center";
                userId = centerHeadLoginSession.CenterHeadId.ToString();
                rm.DistList = _adminRepository.getAdminDistAllowList(userNM, centerHeadLoginSession.CenterHeadId.ToString());
                
            }

            try
            {
                string outError = "";
                string Search = string.Empty;
                Search = "ccode like '%' ";

                if (!string.IsNullOrEmpty(SearchReportType))
                {
                    ViewBag.SelectedReportType = SearchReportType;
                    TempData["SearchReportType"] = SearchReportType;
                    ViewBag.SelectedItemText = SearchReportType == null ? "ALL" : itemReportType.ToList().Where(s => s.Value == ViewBag.SelectedReportType).Select(s => s.Text).FirstOrDefault();



                    if (!string.IsNullOrEmpty(rm.sub))
                    {
                        Search += " and sub ='" + rm.sub.ToString() + "' ";
                    }

                    if (!string.IsNullOrEmpty(rm.ccode))
                    {
                        if (rm.ccode != "0")
                        { Search += " and ccode ='" + rm.ccode.ToString() + "' "; }

                    }

                    if (!string.IsNullOrEmpty(rm.Dist))
                    {
                        Search += " and dist ='" + rm.Dist.ToString() + "' ";

                        rm.ClusterList = clusterReportModel.ClusterList.Where(s => s.dist == rm.Dist).ToList();

                    }

                    if (!string.IsNullOrEmpty(SearchList) && SearchReportType !="5")
                    {
                        ViewBag.SelectedSearch = SearchList;
                        if (SearchList == "1")
                        { Search += " and NOCP > 0"; }
                        else if (SearchList == "2")
                        { Search += " and  NOCM > 0"; }
                    }
                    rm.StoreAllData = _reportRepository.ClusterMarkingStatusReport(Convert.ToInt32(SearchReportType), userId, userNM, Search, out outError);
                    //  rm.StoreAllData = _reportRepository.ClusterMarkingStatusReport(3, userId, userNM, Search, out outError);
                    if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                    {
                        TempData["ExportToExcelDataFromDataTable"] = rm.StoreAllData.Tables[0];
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        if (cmd != null)
                        {
                            if (cmd.ToLower().Contains("excel") || cmd.ToLower().Contains("download"))
                            {
                                ViewBag.SelectedItemText = "Export_" + ViewBag.SelectedItemText;
                                TempData["ExportToExcel"] = "1";
                                ExportToExcelDataFromDataTable(ViewBag.SelectedItemText, "ClusterSubjectStatusReport");
                            }
                        }
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                    }
                }
                return View(rm);

            }
            catch (Exception ex)
            {

                return View();
            }
        }

        #endregion Cluster/CenterHead Reports 



        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        public ActionResult TheoryMarksStatusReport(ReportModel RM)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                var itemsch = new SelectList(new[] {
                    new { ID = "1", Name = "Marks Entry Status" }, new { ID = "2", Name = "Overall Marks Entry Status" },}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" },  }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "5";
                                
                return View(RM);
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [AdminLoginCheckFilter]
        [AdminMenuFilter]
        [HttpPost]
        public ActionResult TheoryMarksStatusReport(ReportModel RM, FormCollection frm, string submit)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                var itemsch = new SelectList(new[] {
                    new { ID = "1", Name = "Marks Entry Status" }, new { ID = "2", Name = "Overall Marks Entry Status" },}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" },}, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";

                string Search = string.Empty;
                if (!string.IsNullOrEmpty(frm["SelList"]) && !string.IsNullOrEmpty(frm["SelClass"]))
                {
                    ViewBag.SelectedItem = frm["SelList"] == null ? "ALL" : frm["SelList"];
                    ViewBag.SelectedItemText = frm["SelList"] == null ? "ALL" : itemsch.ToList().Where(s => s.Value == ViewBag.SelectedItem).Select(s => s.Text).FirstOrDefault();
                    ViewBag.Selectedcls = frm["SelClass"] == null ? "ALL" : frm["SelClass"];
                    RM.StoreAllData = _reportRepository.TheoryMarksStatusReport(ViewBag.SelectedItem, Convert.ToInt32(ViewBag.Selectedcls));
                    if (RM.StoreAllData == null || RM.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        TempData["ExportToExcelDataFromDataTable"] = RM.StoreAllData.Tables[0];
                        if (submit != null)
                        {
                            if (submit.ToLower().Contains("excel") || submit.ToLower().Contains("download"))
                            {
                                TempData["ExportToExcel"] = "1";
                                ExportToExcelDataFromDataTable(ViewBag.SelectedItemText, "TheoryMarksStatusReport");
                            }
                        }
                        ViewBag.TotalCount = RM.StoreAllData.Tables[0].Rows.Count;
                    }
                }
                else
                {
                    ViewBag.Message = "2";
                    ViewBag.TotalCount = 0;
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            return View(RM);
        }


    }




}