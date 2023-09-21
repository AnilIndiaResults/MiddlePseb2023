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
    public class BankController : Controller
    {
        private readonly IBankRepository _bankRepository;

        public BankController(IBankRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }

        private BankModels BM = new BankModels();

        // GET: Bank
        public ActionResult Index()
        {
            return View();
        }

        [Route("Bank")]
        [Route("Bank/Login")]
        public ActionResult Login()
        {
            try
            {
                if (TempData["result"] != null)
                {
                    ViewData["result"] = TempData["result"];
                }
                ViewBag.SessionList = DBClass.GetSession().ToList();
                return View();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));                
                return View();
            }
        }

        [Route("Bank")]
        [Route("Bank/Login")]
        [HttpPost]
        public ActionResult Login(LoginModel lm)
        {
            try
            {
                BankLoginSession bankLoginSession = _bankRepository.BankLogin(lm); // passing Value to SchoolDB from model and Type 1 For regular              

                bankLoginSession.CurrentSession = lm.Session;
                TempData["result"] = bankLoginSession.STATUS;
                if (bankLoginSession.STATUS == 1)
                {
                    Session["BankLoginSession"] = bankLoginSession;
                    return RedirectToAction("Welcome", "Bank");
                }
                return RedirectToAction("login", "Bank");
            }          
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("login", "Bank");
            }
        }

        public ActionResult Logout()
        {
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();          
            return RedirectToAction("Login", "Bank");

        }


        [BankLoginCheckFilter]
        public ActionResult Welcome(BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                BM.Session = bankLoginSession.CurrentSession;
                BM.BANK = bankLoginSession.BANKNAME;


                string FilepathExist = Path.Combine(Server.MapPath("~/BankUpload/"));
                if (!Directory.Exists(FilepathExist))
                {
                    Directory.CreateDirectory(FilepathExist);
                }

                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }




        [BankLoginCheckFilter]
        public ActionResult FileSeparator()
        {
            return View();
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult FileSeparator(BankModels BM)       
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                string fileLocation = "";
                string filename = "";
                BM.Session = bankLoginSession.CurrentSession.ToString();
                BM.BCODE = bankLoginSession.BCODE.ToString();
                if (BM.file != null)
                {
                    filename = Path.GetFileName(BM.file.FileName);
                }
                DataSet ds = new DataSet();
                if (BM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                {
                    string fileName1 = "MIS_" + BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm");  //MIS_201_110720161210
                    string fileExtension = System.IO.Path.GetExtension(BM.file.FileName);
                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        fileLocation = Server.MapPath("~/BankUpload/" + fileName1 + fileExtension);
                 
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
                        
                        BM.file.SaveAs(fileLocation);
                        string excelConnectionString = string.Empty;
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        if (fileExtension == ".xls")
                        {
                            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        }
                        //connection String for xlsx file format.
                        else if (fileExtension == ".xlsx")
                        {
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        }
                        //Create Connection to Excel work book and add oledb namespace
                        OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                        excelConnection.Open();
                        DataTable dt = new DataTable();
                        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dt == null)
                        {
                            return null;
                        }

                        String[] excelSheets = new String[dt.Rows.Count];
                        int t = 0;
                        //excel data saves in temp file here.
                        foreach (DataRow row in dt.Rows)
                        {
                            excelSheets[t] = row["TABLE_NAME"].ToString(); // bank_mis     TABLE_NAME
                            t++;
                        }
                        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                        string query = string.Format("Select * from [{0}]", excelSheets[0]);
                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                        {
                            dataAdapter.Fill(ds);
                        }

                        string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                        bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                        if (CheckChln == true)
                        {
                            ViewData["Result"] = "11";
                            ViewBag.Message = "Duplicate Challan Number";
                            return View();
                        }
                        else
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {                               
                                var filterPrimary = ds.Tables[0].AsEnumerable().Where(x => x.Field<string>("ChallanId").Substring(3, 2) == "64");
                                var filterSenior = ds.Tables[0].AsEnumerable().Where(x => x.Field<string>("ChallanId").Substring(3, 2) != "64");

                                DataSet dsfilter = new DataSet();
                                if (filterPrimary.Any())
                                {
                                    DataTable dtPrimary = filterPrimary.CopyToDataTable();
                                    dtPrimary.TableName = "PrimaryChallans";
                                    dsfilter.Tables.Add(dtPrimary);
                                   // ExportDataFromDataTable(2, dtPrimary, BM.BCODE,"PrimaryChallans");
                                }

                                if (filterSenior.Any())
                                {
                                    DataTable dtSenior = filterSenior.CopyToDataTable();
                                    dtSenior.TableName = "SeniorChallans";
                                    dsfilter.Tables.Add(dtSenior);
                                    //ExportDataFromDataTable(2, dtSenior, BM.BCODE, "SeniorChallans");
                                }

                                ExportDataFromDataSet(2, dsfilter, BM.BCODE, "FilterChallans");
                                return View();
                            }
                        }
                    }
                    else
                    {

                        ViewData["Result"] = "2";
                        ViewBag.Message = "Please Upload Only .xls and .xlsx only";
                        return View();
                    }

                }
            }
            catch (Exception ex)
            {
                ViewData["Result"] = "0";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }


        [BankLoginCheckFilter]
        public ActionResult ErrorList()
        {            
            ErrorShowList rm = new ErrorShowList();
            rm.StoreAllData = _bankRepository.GetErrorDetails();
            return View(rm);
        }

       
         #region ViewProfile
        [BankLoginCheckFilter]
        public ActionResult ViewProfile(BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                    int OutStatus;               
                
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    DataSet ds = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
                    BM.BankMasterData = ds;
                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        BM.BCODE = ds.Tables[0].Rows[0]["BCODE"].ToString();
                        BM.BANKNAME = ds.Tables[0].Rows[0]["BANKNAME"].ToString();
                        BM.ADDRESS = ds.Tables[0].Rows[0]["ADDRESS"].ToString();
                        BM.DISTRICT = ds.Tables[0].Rows[0]["DISTRICT"].ToString();
                        BM.PINCODE = ds.Tables[0].Rows[0]["PINCODE"].ToString();
                        BM.MOBILE = ds.Tables[0].Rows[0]["MOBILE"].ToString();
                        BM.STD = ds.Tables[0].Rows[0]["STD"].ToString();
                        BM.PHONE = ds.Tables[0].Rows[0]["PHONE"].ToString();
                        BM.EMAILID1 = ds.Tables[0].Rows[0]["EMAILID1"].ToString();
                        BM.EMAILID2 = ds.Tables[0].Rows[0]["EMAILID2"].ToString();
                        BM.ACNO = ds.Tables[0].Rows[0]["ACNO"].ToString();
                        BM.IFSC = ds.Tables[0].Rows[0]["IFSC"].ToString();
                        BM.MICR = ds.Tables[0].Rows[0]["MICR"].ToString();
                        BM.ID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString());                       
                        BM.BRNCODE = ds.Tables[0].Rows[0]["BRNCODE"].ToString();
                        BM.NODAL_BRANCH = ds.Tables[0].Rows[0]["NODAL_BRANCH"].ToString();
                        BM.MNAGER_NM = ds.Tables[0].Rows[0]["MNAGER_NM"].ToString();
                        BM.TECHNICAL_PERSON = ds.Tables[0].Rows[0]["TECHNICAL_PERSON"].ToString();
                        BM.OTCONTACT = ds.Tables[0].Rows[0]["OTCONTACT"].ToString();                        
                        ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                    }

               
                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewProfile(BankModels BM, FormCollection frm)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;
                BM.BCODE = bankLoginSession.BCODE.ToString();
                DataSet ds = _bankRepository.UpdateBankDataByBCODE(BM, out OutStatus);
                if (OutStatus == 1)
                {
                    ViewData["Result"] = "1";
                    return View();
                }
                else
                {
                    ViewData["Result"] = "0";
                    return View();
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("ViewProfile", "Bank");
            }
        }

        #endregion ViewProfile

        #region DownloadChallan
        [BankLoginCheckFilter]
        public ActionResult DownloadChallan(BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;
                BM.Session = bankLoginSession.CurrentSession.ToString();
                BM.BCODE = bankLoginSession.BCODE.ToString();
                BM.BANK = bankLoginSession.BANKNAME.ToString();
                BM.DOWNLDFLG = 0; // O - Challans Not Downloaded
                DataSet ds = _bankRepository.DownloadChallan(BM, out OutStatus);
                BM.BankMasterData = ds;
                if (ds == null || OutStatus == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                        ViewBag.CurrentDownloadedLot = ds.Tables[2].Rows[0]["LotNew"].ToString();  // harpal sir 
                    }
                    else if (ds.Tables[1].Rows.Count > 0)
                    {
                        ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                        ViewBag.Message = "Record Not Found";
                    }
                    else
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                    }

                }
                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [BankLoginCheckFilter]
        public ActionResult DownloadPreviousChallan(BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;
                
                    TempData["PrevLOT"] = null;
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    BM.BANK = bankLoginSession.BANKNAME.ToString();
                    BM.DOWNLDFLG = 1; // 1 -  Challans Already Downloaded
                    DataSet ds = _bankRepository.DownloadChallan(BM, out OutStatus);
                    BM.BankMasterData = ds;
                    List<SelectListItem> itemLotList = new List<SelectListItem>();
                    itemLotList.Add(new SelectListItem { Text = "--Select Lot--", Value = "0" });
                    ViewBag.LotList = itemLotList;

                    if (ds == null || OutStatus == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {                        
                        if (ds.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.TotalCount = 0;
                            ViewBag.CurrentDownloadedLot = ds.Tables[3].Rows.Count;
                            foreach (System.Data.DataRow dr in ds.Tables[3].Rows)
                            {
                                itemLotList.Add(new SelectListItem { Text = @dr["DOWNLDLOT"].ToString(), Value = @dr["DOWNLDLOT"].ToString() });
                            }
                            ViewBag.LotList = itemLotList;
                            ViewBag.Message = "Record Not Found";
                        }
                        else
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount1 = 0;
                        }
                   
                }
                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult DownloadPreviousChallan(BankModels BM, FormCollection frm)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                int OutStatus;

                BM.Session = bankLoginSession.CurrentSession.ToString();
                BM.BCODE = bankLoginSession.BCODE.ToString();
                BM.BANK = bankLoginSession.BANKNAME.ToString();
                BM.DOWNLDFLG = 1; // 1 -  Challans Already Downloaded
                DataSet ds = _bankRepository.DownloadChallan(BM, out OutStatus);


                string[] lotsearch = frm["LOT"].ToString().Split('-');
                BM.LOT = Convert.ToInt32(lotsearch[0].ToString());
                TempData["PrevLOT"] = BM.LOT.ToString();
                int Type1 = 3; //3  Get lot list by bcode
                DataSet ds1 = _bankRepository.BankMisDetails(BM, Type1, out OutStatus);
                BM.BankMasterData = ds1;

                List<SelectListItem> itemLotList = new List<SelectListItem>();
                itemLotList.Add(new SelectListItem { Text = "--Select Lot--", Value = "0" });
                ViewBag.LotList = itemLotList;

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        ViewBag.TotalCount = ds1.Tables[0].Rows.Count;
                        ViewBag.CurrentDownloadedLot = BM.LOT;
                        foreach (System.Data.DataRow dr in ds.Tables[3].Rows)
                        {
                            itemLotList.Add(new SelectListItem { Text = @dr["DOWNLDLOT"].ToString(), Value = @dr["DOWNLDLOT"].ToString() });
                        }
                        ViewBag.LotList = itemLotList;
                    }
                    else
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount1 = 0;
                    }
                }
                return View(BM);

            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("DownloadPreviousChallan", "Bank");
            }
        }
        #endregion DownloadChallan

        #region VerifyMIS
        [BankLoginCheckFilter]
        public ActionResult VerifyMIS()
        {            
            return View();
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult VerifyMIS(BankModels BM) // HttpPostedFileBase file
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                string fileLocation = "";
                string filename = "";
                BM.Session = bankLoginSession.CurrentSession.ToString();
                BM.BCODE = bankLoginSession.BCODE.ToString();
                if (BM.file != null)
                {
                    filename = Path.GetFileName(BM.file.FileName);
                }
                DataSet ds = new DataSet();
                if (BM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                {
                    string fileName1 = "MIS_" + BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm");  //MIS_201_110720161210
                    string fileExtension = System.IO.Path.GetExtension(BM.file.FileName);
                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        fileLocation = Server.MapPath("~/BankUpload/" + fileName1 + fileExtension);
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
                        BM.file.SaveAs(fileLocation);
                        string excelConnectionString = string.Empty;
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                         if (fileExtension == ".xls")
                        {
                            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        }
                        //connection String for xlsx file format.
                        else if (fileExtension == ".xlsx")
                        {
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                            fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        }
                        //Create Connection to Excel work book and add oledb namespace
                        OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                        excelConnection.Open();
                        DataTable dt = new DataTable();
                        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dt == null)
                        {
                            return null;
                        }

                        String[] excelSheets = new String[dt.Rows.Count];
                        int t = 0;
                        //excel data saves in temp file here.
                        foreach (DataRow row in dt.Rows)
                        {
                            excelSheets[t] = row["TABLE_NAME"].ToString(); // bank_mis     TABLE_NAME
                            t++;
                        }
                        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                        string query = string.Format("Select * from [{0}]", excelSheets[0]);
                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                        {
                            dataAdapter.Fill(ds);
                        }

                        if (ds.Tables[0].Rows.Count > 5000)
                        {
                            ViewData["Result"] = "6";
                            ViewBag.Message = "Please Upload less than 5000 Challans";
                            return View();
                        }

                        string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                       bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                        if (CheckChln == true)
                        {
                            ViewData["Result"] = "11";
                            ViewBag.Message = "Duplicate Challan Number";
                            return View();
                        }
                        //  string CheckMis = _bankRepository.CheckMisExcel(ds);
                        DataTable dtexport = new DataTable();
                        string CheckMis = _bankRepository.CheckMisExcelExportRN(ds, out dtexport);//CheckMisExcelExport
                        if (CheckMis == "")
                        {
                            //---Unique Uploadlot For Bulk Excel Sheet -----Changed By Amar Jnana
                            Int32 UPLOADLOT = 0;
                            DataSet Bmis = _bankRepository.GetTotBankMIS(BM);
                            UPLOADLOT = Convert.ToInt32(Bmis.Tables[0].Rows[0][0].ToString());
                            //End Changed By Amar Jnana

                            // Start Update Bulk Code by Rohit
                            if (ds.Tables.Count > 0)
                            {
                                if (ds.Tables[0].Rows.Count > 0)
                                {

                                    DataTable dt1 = ds.Tables[0];
                                    if (dt1.Columns.Contains("STATUS"))
                                    {
                                        dt1.Columns.Remove("STATUS");
                                    }
                                    dt1.AcceptChanges();
                                    BM.MIS_FILENM = fileName1;                               
                                    int OutStatus = 0;
                                    // DataTable dtResult = objDM.BulkChallanEntry(dt1, AdminId, out OutStatus);// OutStatus mobile
                                    int BankId = 0;
                                    string OutError = "";
                                    DataTable dtResult = _bankRepository.BulkChallanBank(dt1, BankId, UPLOADLOT, BM, out OutStatus, out OutError);  // BulkChallanBank
                                    if (OutStatus > 0)
                                    {
                                        ViewData["Result"] = "1";
                                        ViewBag.Message = "All Challans Uploaded Successfully";
                                    }
                                    else
                                    {
                                        ViewData["Result"] = "5";
                                        ViewBag.Message = "Challans Upload Failure: " + OutError;
                                    }

                                }
                            }
                            // End Bulk Code by Rohit  
                            return View();
                        }
                        else
                        {
                            if (dtexport != null)
                            {
                                ExportDataFromDataTable(1, dtexport, BM.BCODE,"ERROR");
                            }
                            ViewData["Result"] = "-1";
                            ViewBag.Message = CheckMis;
                            return View();
                        }
                    }
                    else
                    {

                        ViewData["Result"] = "2";
                        ViewBag.Message = "Please Upload Only .xls and .xlsx only";
                        return View();
                    }

                }
            }
            catch (Exception ex)
            {
                ViewData["Result"] = "0";                
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion VerifyMIS


        #region ViewMISDetails
        [BankLoginCheckFilter]
        public ActionResult ViewMISDetails(BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;

                BM.Session = bankLoginSession.CurrentSession.ToString();
                BM.BCODE = bankLoginSession.BCODE.ToString();
                //challanid,j_refno,bcode,upldate only date
                var itemsch = new SelectList(new[]{new {ID="1",Name="Challan Id"},new{ID="2",Name="J_REF_NO"},
                     new{ID="3",Name="Branch Code"},new{ID="4",Name="Upload Date"}}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                ViewBag.SelectedItem = "";
                string Search = string.Empty;
                Search = "BCODE='" + BM.BCODE + "' ";
                DataSet ds = _bankRepository.BankMisDetailsSearch(Search);
                BM.BankMasterData = ds;

                int Type1 = 2; // Get lot list by bcode
                DataSet ds1 = _bankRepository.BankMisDetails(BM, Type1, out OutStatus);
                //BM.BankMasterData = ds1;

                List<SelectListItem> itemLotList = new List<SelectListItem>();
                itemLotList.Add(new SelectListItem { Text = "--Select Lot--", Value = "0" });
                ViewBag.LotList = itemLotList;

                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount1 = 0;
                    // return View();
                }
                else
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        // ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                        ViewBag.Message = "Search Record";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                }

                if (ds1 == null || ds1.Tables[0].Rows.Count == 0)
                {

                }
                else if (ds1 != null || ds1.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow dr in ds1.Tables[0].Rows)
                    {
                        itemLotList.Add(new SelectListItem { Text = @dr["UPLOADLOT"].ToString(), Value = @dr["UPLOADLOT"].ToString() });
                    }
                    ViewBag.LotList = itemLotList;
                }

                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewMISDetails(BankModels BM, FormCollection frm)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                int OutStatus;                
                if (frm["LOT"] == null || frm["LOT"] == "")
                {
                    return RedirectToAction("Welcome", "Bank");
                }
                else
                {
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    string[] lotsearch = frm["LOT"].ToString().Split('-');
                    BM.LOT = Convert.ToInt32(lotsearch[0].ToString());


                    var itemsch = new SelectList(new[]{new {ID="1",Name="Challan Id"},new{ID="2",Name="J_REF_NO"},
                     new{ID="3",Name="Branch Code"},new{ID="4",Name="Upload Date"}}, "ID", "Name", 1);
                    ViewBag.MySch = itemsch.ToList();

                    ViewBag.SelectedItem = "";
                    string Search = string.Empty;
                    Search = "BCODE='" + BM.BCODE + "' ";
                    if (BM.LOT > 0)
                    {
                        Search += " and UPLOADLOT='" + BM.LOT + "'";
                    }
                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());


                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and CHALLANID='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 2)
                            { Search += " and  J_REF_NO='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and BRCODE='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 4)
                            { Search += " and convert(varchar(10),UPLOADDT)='" + frm["SearchString"].ToString() + "'"; }
                        }
                    }
                    DataSet ds = _bankRepository.BankMisDetailsSearch(Search);
                    BM.BankMasterData = ds;

                    
                    int Type2 = 2; // Get lot list by bcode
                    DataSet ds1 = _bankRepository.BankMisDetails(BM, Type2, out OutStatus);

                    List<SelectListItem> itemLotList = new List<SelectListItem>();
                    itemLotList.Add(new SelectListItem { Text = "--Select Lot--", Value = "0" });
                    ViewBag.LotList = itemLotList;
                    if (ds1 == null || ds1.Tables[0].Rows.Count == 0)
                    {

                    }
                    else if (ds1 != null || ds1.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in ds1.Tables[0].Rows)
                        {
                            itemLotList.Add(new SelectListItem { Text = @dr["UPLOADLOT"].ToString(), Value = @dr["UPLOADLOT"].ToString() });
                        }
                        ViewBag.LotList = itemLotList;
                    }
                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.CurrentDownloadedLot = ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                        }
                        else
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                        }
                    }

                }
                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("ViewMISDetails", "Bank");
            }
        }
        #endregion ViewMISDetails

        #region ChangePassword
        [BankLoginCheckFilter]
        public ActionResult ChangePassword(BankModels BM)
        {
            try
            {

                if (TempData["result"] != null)
                {
                    ViewData["result"] = TempData["result"];
                }
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;
                BM.BCODE = bankLoginSession.BCODE.ToString();
                DataSet ds = _bankRepository.GetBankDataByBCODE(BM, out OutStatus);
                BM.BankMasterData = ds;
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    BM.BCODE = ds.Tables[0].Rows[0]["BCODE"].ToString();
                    BM.BANKNAME = ds.Tables[0].Rows[0]["BANKNAME"].ToString();
                    BM.buser_id = ds.Tables[0].Rows[0]["buser_id"].ToString();
                    BM.ID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString());                   
                    ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                }
                return View(BM);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult ChangePassword(BankModels BM, FormCollection frm)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                int OutStatus;               
                if (BM.Newpassword != BM.ConfirmPassword)
                {
                    TempData["result"] = "3";
                }
                else
                {
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    DataSet ds = _bankRepository.ChangePasswordBank(BM, out OutStatus);
                    TempData["result"] = OutStatus.ToString();                  
                }                
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));                
            }
            return RedirectToAction("ChangePassword", "Bank");
        }
        #endregion ChangePassword


        //--------------------------------Export Data To Excel---------------
        public ActionResult ExportData(BankModels BM)
        {

            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                if (Request.QueryString["File"] == null)
                {
                    return RedirectToAction("Welcome", "Bank");
                }
                else
                {
                    string FileExport = Request.QueryString["File"].ToString();
                    string LotNew = "";
                    DataSet ds = null;
                    int OutStatus;

                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    BM.BANK = bankLoginSession.BANKNAME.ToString();
                    BM.DOWNLDFLG = 0; // O - Challans Not Downloaded
                    ds = _bankRepository.DownloadChallan(BM, out OutStatus);

                    if (OutStatus == 1 && ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            if (ds.Tables[0].Columns.Contains("Mobile"))
                            { ds.Tables[0].Columns.Remove("Mobile"); }
                            if (ds.Tables[0].Columns.Contains("BankShort"))
                            { ds.Tables[0].Columns.Remove("BankShort"); }
                            if (ds.Tables[0].Columns.Contains("LOT"))
                            { ds.Tables[0].Columns.Remove("LOT"); }
                            if (ds.Tables[0].Columns.Contains("DOWNLDLOT"))
                            { ds.Tables[0].Columns.Remove("DOWNLDLOT"); }
                            LotNew = ds.Tables[2].Rows[0]["LotNew"].ToString();
                            bool ResultDownload;
                            try
                            {
                                switch (FileExport)
                                {
                                    case "Excel":
                                        string fileName1 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".xls";  //103_230820162209_347
                                        using (XLWorkbook wb = new XLWorkbook())
                                        {
                                            //wb.Worksheets.Add(dt);
                                            wb.Worksheets.Add(ds);
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
                                        break;
                                    case "CSV":
                                        string fileName2 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".csv";  //103_230820162209_347
                                        StringBuilder sb = new StringBuilder();
                                        DataTable dt = new DataTable();
                                        dt = ds.Tables[0];
                                        Response.Clear();
                                        Response.Buffer = true;
                                        Response.AddHeader("content-disposition", "attachment;filename='" + fileName2 + "'");
                                        Response.Charset = "";
                                        Response.ContentType = "application/text";
                                        for (int k = 0; k < dt.Columns.Count; k++)
                                        {
                                            //add separator
                                            sb.Append(dt.Columns[k].ColumnName + ',');
                                        }
                                        //append new line
                                        sb.Append("\r\n");
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            for (int k = 0; k < dt.Columns.Count; k++)
                                            {
                                                //add separator
                                                sb.Append(dt.Rows[i][k].ToString().Replace(",", ";") + ',');
                                            }
                                            //append new line
                                            sb.Append("\r\n");
                                        }
                                        Response.Output.Write(sb.ToString());
                                        Response.Flush();
                                        Response.End();
                                        break;
                                    case "Txt":
                                        string fileName3 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".txt";  //103_230820162209_347
                                        string txt = string.Empty;
                                        foreach (DataColumn column in ds.Tables[0].Columns)
                                        {
                                            //Add the Header row for Text file.
                                            txt += column.ColumnName + "\t\t";
                                        }
                                        //Add new line.
                                        txt += "\r\n";
                                        foreach (DataRow row in ds.Tables[0].Rows)
                                        {
                                            foreach (DataColumn column in ds.Tables[0].Columns)
                                            {
                                                //Add the Data rows.
                                                txt += row[column.ColumnName].ToString() + "\t\t";
                                            }

                                            //Add new line.
                                            txt += "\r\n";
                                        }
                                        //Download the Text file.
                                        Response.Clear();
                                        Response.Buffer = true;
                                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName3 + "");
                                        Response.Charset = "";
                                        Response.ContentType = "application/text";
                                        Response.Output.Write(txt);
                                        Response.Flush();
                                        Response.End();
                                        break;

                                }
                                ResultDownload = true;
                            }
                            catch (Exception)
                            {
                                ResultDownload = false;
                            }

                            if (ResultDownload == true)
                            {
                                // BM.Session = bankLoginSession.CurrentSession.ToString();
                                //  BM.BCODE = bankLoginSession.BCODE.ToString();
                                //  BM.BANK = bankLoginSession.BANKNAME.ToString();
                                BM.DOWNLDFLG = 3;   //  3 for updated download flag O to 1
                                ds = _bankRepository.DownloadChallan(BM, out OutStatus);
                                if (OutStatus == 1)
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        string LOT1 = ds.Tables[0].Rows[0]["LOT"].ToString();
                                        string CHLNVDATE1 = ds.Tables[0].Rows[0]["CHLNVDATE"].ToString();
                                        string MOBILE1 = ds.Tables[0].Rows[0]["MOBILE"].ToString();
                                        string BankShort = ds.Tables[0].Rows[0]["BankShort"].ToString();
                                        string CHALLANID1 = ds.Tables[0].Rows[0]["CHALLANID"].ToString();
                                        //BankShort
                                        //You can deposit your Fee of challan no XXXXXXXXXX for Lot no XXX in the  bank XXXXX  before Dt  XXXXXX. Regards PSEB
                                        string Sms = "You can deposit your Fee of challan no " + CHALLANID1 + " for Lot no " + LOT1 + " in the  bank " + BankShort + "  before Dt  " + CHLNVDATE1 + ". Regards PSEB";
                                        try
                                        {
                                            string getSms = DBClass.gosms(MOBILE1, Sms);
                                            //  string getSms = DBClass.gosms("9711819184", Sms);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                        }
                    }

                }
                return RedirectToAction("Welcome", "Bank");
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("DownloadChallan", "Bank");
            }

        }

        [BankLoginCheckFilter]
        public ActionResult PreviousExportData(BankModels BM, FormCollection frm)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];


                if (Request.QueryString["File"] == null)
                {
                    return RedirectToAction("Welcome", "Bank");
                }
                else
                {
                    string FileExport = Request.QueryString["File"].ToString();
                    string LotNew = "";
                    DataSet ds = null;
                    int OutStatus;                    
                    if (TempData["PrevLOT"] == null)
                    {
                        BM.Session = bankLoginSession.CurrentSession.ToString();
                        BM.BCODE = bankLoginSession.BCODE.ToString();
                        BM.BANK = bankLoginSession.BANKNAME.ToString();
                        BM.DOWNLDFLG = 1; // 1 - Challans Downloaded
                        ds = _bankRepository.DownloadChallan(BM, out OutStatus);
                        BM.BankMasterData = ds;
                        LotNew = "All";
                    }
                    else
                    {
                        BM.Session = bankLoginSession.CurrentSession.ToString();
                        BM.BCODE = bankLoginSession.BCODE.ToString();
                        BM.BANK = bankLoginSession.BANKNAME.ToString();
                        BM.DOWNLDFLG = 1; // 1 - Challans Downloaded
                                          // ds = _bankRepository.DownloadChallan(BM, out OutStatus);

                        BM.LOT = Convert.ToInt32(TempData["PrevLOT"].ToString());
                        int Type1 = 3; //3  Get lot list by bcode
                        ds = _bankRepository.BankMisDetails(BM, Type1, out OutStatus);
                        BM.BankMasterData = ds;
                        LotNew = BM.LOT.ToString();
                    }
                    if (OutStatus == 1 && ds.Tables[0].Rows.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            if (ds.Tables[0].Columns.Contains("Mobile"))
                            { ds.Tables[0].Columns.Remove("Mobile"); }
                            if (ds.Tables[0].Columns.Contains("BankShort"))
                            { ds.Tables[0].Columns.Remove("BankShort"); }
                            if (ds.Tables[0].Columns.Contains("LOT"))
                            { ds.Tables[0].Columns.Remove("LOT"); }
                            if (ds.Tables[0].Columns.Contains("DOWNLDLOT"))
                            { ds.Tables[0].Columns.Remove("DOWNLDLOT"); }
                            // string fileName1 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".xls";  //103_230820162209_347
                            bool ResultDownload;
                            try
                            {
                                switch (FileExport)
                                {
                                    case "Excel":
                                        string fileName1 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".xls";  //103_230820162209_347
                                        using (XLWorkbook wb = new XLWorkbook())
                                        {
                                            //wb.Worksheets.Add(dt);
                                            wb.Worksheets.Add(ds);
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
                                        break;
                                    case "CSV":
                                        string fileName2 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".csv";  //103_230820162209_347
                                        StringBuilder sb = new StringBuilder();
                                        DataTable dt = new DataTable();
                                        dt = ds.Tables[0];
                                        Response.Clear();
                                        Response.Buffer = true;
                                        Response.AddHeader("content-disposition", "attachment;filename='" + fileName2 + "'");
                                        Response.Charset = "";
                                        Response.ContentType = "application/text";
                                        for (int k = 0; k < dt.Columns.Count; k++)
                                        {
                                            //add separator
                                            sb.Append(dt.Columns[k].ColumnName + ',');
                                        }
                                        //append new line
                                        sb.Append("\r\n");
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            for (int k = 0; k < dt.Columns.Count; k++)
                                            {
                                                //add separator
                                                sb.Append(dt.Rows[i][k].ToString().Replace(",", ";") + ',');
                                            }
                                            //append new line
                                            sb.Append("\r\n");
                                        }
                                        Response.Output.Write(sb.ToString());
                                        Response.Flush();
                                        Response.End();


                                        //------------------Amar jnana--------------------------

                                        break;
                                    case "Txt":
                                        string fileName3 = BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm") + '_' + LotNew + ".txt";  //103_230820162209_347
                                        string txt = string.Empty;
                                        foreach (DataColumn column in ds.Tables[0].Columns)
                                        {
                                            //Add the Header row for Text file.
                                            txt += column.ColumnName + "\t\t";
                                        }
                                        //Add new line.
                                        txt += "\r\n";
                                        foreach (DataRow row in ds.Tables[0].Rows)
                                        {
                                            foreach (DataColumn column in ds.Tables[0].Columns)
                                            {
                                                //Add the Data rows.
                                                txt += row[column.ColumnName].ToString() + "\t\t";
                                            }

                                            //Add new line.
                                            txt += "\r\n";
                                        }
                                        //Download the Text file.
                                        Response.Clear();
                                        Response.Buffer = true;
                                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName3 + "");
                                        Response.Charset = "";
                                        Response.ContentType = "application/text";
                                        Response.Output.Write(txt);
                                        Response.Flush();
                                        Response.End();
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

                return RedirectToAction("Welcome", "Bank");
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("DownloadPreviousChallan", "Bank");
            }

        }

        public ActionResult ExportDataFromDataSet(int tt, DataSet ds, string BCODE, string FileNAME)
        {
            try
            {
                if (ds.Tables.Count == 0)
                {
                    return RedirectToAction("VerifyMIS", "Bank");
                }
                else
                {
                    string fileName1 = "";
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        if (tt == 1)
                        {
                            fileName1 = "ERROR_" + BCODE + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                        }
                        else
                        { fileName1 = FileNAME + "_" + BCODE + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls"; }


                        // dt.TableName = fileName1;
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

                return RedirectToAction("VerifyMIS", "Bank");
            }
            catch (Exception ex)
            {
                return RedirectToAction("VerifyMIS", "Bank");
            }

        }


        public ActionResult ExportDataFromDataTable(int tt, DataTable dt, string BCODE, string FileNAME)
        {
            try
            {
                if (dt.Rows.Count == 0)
                {
                    return RedirectToAction("VerifyMIS", "Bank");
                }
                else
                {
                    string fileName1 = "";
                    if (dt.Rows.Count > 0)
                    {
                        if (tt == 1)
                        {
                            fileName1 = "ERROR_" + BCODE + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";
                        }
                        else
                        { fileName1 = FileNAME + "_" + BCODE + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls"; }


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

                return RedirectToAction("VerifyMIS", "Bank");
            }
            catch (Exception ex)
            {
                return RedirectToAction("VerifyMIS", "Bank");
            }

        }


        #region VerifyMIS PSEB
        [BankLoginCheckFilter]
        public ActionResult VerifyMISPSEB()
        {
            BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
            if (bankLoginSession.BCODE.ToString() != "203")
            {
                return RedirectToAction("login", "Bank");
            }
            return View();
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult VerifyMISPSEB(BankModels BM) // HttpPostedFileBase file
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];

                if (bankLoginSession.BCODE.ToString() != "203")
                {
                    return RedirectToAction("login", "Bank");
                }
                else
                {
                    string fileLocation = "";
                    string filename = "";
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    if (BM.file != null)
                    {
                        filename = Path.GetFileName(BM.file.FileName);
                    }
                    DataSet ds = new DataSet();
                    if (BM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "MIS_" + BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm");  //MIS_201_110720161210
                        string fileExtension = System.IO.Path.GetExtension(BM.file.FileName);
                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            fileLocation = Server.MapPath("~/BankUpload/" + fileName1 + fileExtension);
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
                            BM.file.SaveAs(fileLocation);
                            string excelConnectionString = string.Empty;
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            //connection String for xls file format.
                            //if (Path.GetExtension(path).ToLower().Trim() == ".xls" && Environment.Is64BitOperatingSystem == false)
                            if (fileExtension == ".xls")
                            {
                                excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            }
                            //connection String for xlsx file format.
                            else if (fileExtension == ".xlsx")
                            {
                                excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            }
                            //Create Connection to Excel work book and add oledb namespace
                            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                            excelConnection.Open();
                            DataTable dt = new DataTable();
                            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (dt == null)
                            {
                                return null;
                            }

                            String[] excelSheets = new String[dt.Rows.Count];
                            int t = 0;
                            //excel data saves in temp file here.
                            foreach (DataRow row in dt.Rows)
                            {
                                excelSheets[t] = row["TABLE_NAME"].ToString(); // bank_mis     TABLE_NAME
                                t++;
                            }
                            OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                            string query = string.Format("Select * from [{0}]", excelSheets[0]);
                            using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                            {
                                dataAdapter.Fill(ds);
                            }

                            if (ds.Tables[0].Rows.Count > 5000)
                            {
                                ViewData["Result"] = "6";
                                ViewBag.Message = "Please Upload less than 5000 Challans";
                                return View();
                            }

                            string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                            // bool CheckStatus = StaticDB.CheckArrayDuplicates(array);
                            bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                            if (CheckChln == true)
                            {
                                ViewData["Result"] = "11";
                                ViewBag.Message = "Duplicate Challan Number";
                                return View();
                            }


                            //  string CheckMis = _bankRepository.CheckMisExcel(ds);
                            DataTable dtexport = new DataTable();
                            string CheckMis = _bankRepository.CheckMisExcelExportPSEB(ds, out dtexport);
                            if (CheckMis == "")
                            {
                                //---Unique Uploadlot For Bulk Excel Sheet -----Changed By Amar Jnana
                                Int32 UPLOADLOT = 0;
                                DataSet Bmis = _bankRepository.GetTotBankMIS(BM);
                                UPLOADLOT = Convert.ToInt32(Bmis.Tables[0].Rows[0][0].ToString());
                                //End Changed By Amar Jnana

                                // Start Update Bulk Code by Rohit
                                if (ds.Tables.Count > 0)
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {

                                        DataTable dt1 = ds.Tables[0];
                                        if (dt1.Columns.Contains("STATUS"))
                                        {
                                            dt1.Columns.Remove("STATUS");
                                        }
                                        dt1.AcceptChanges();
                                        BM.MIS_FILENM = fileName1;
                                        string Result1 = "";
                                        int OutStatus = 0;
                                        int BankId = 0;
                                        DataTable dtResult = _bankRepository.BulkChallanBankPSEBHOD(dt1, BankId, UPLOADLOT, BM, out OutStatus);  // BulkChallanBankPSEBHOD
                                        if (OutStatus > 0)
                                        {
                                            ViewBag.Result1 = "All Challans Uploaded Successfully";
                                        }
                                        else
                                        {
                                            ViewBag.Result1 = "Challans Upload Failure";
                                        }
                                        ViewData["Result"] = "1";
                                    }
                                }
                                // End Bulk Code by Rohit  
                                return View();
                            }
                            else
                            {
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(1, dtexport, BM.BCODE,"ERROR");
                                }
                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }
                        }
                        else
                        {

                            ViewData["Result"] = "2";
                            ViewBag.Message = "Please Upload Only .xls and .xlsx only";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Result"] = "0";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion VerifyMIS PSEB

        #region View All Fee Deposit By BCODE
        [BankLoginCheckFilter]
        public ActionResult ViewAllFeeDeposit(string id, BankModels BM)
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];


                var itemsch = new SelectList(new[]{new {ID="1",Name="Challan Id"},new{ID="2",Name="AppNo/RefNo"},
                     new{ID="3",Name="Download Date"},new{ID="4",Name="Deposit Date"}}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                int OutStatus;
                if (bankLoginSession.BANKNAME == null || Session["RoleType"].ToString() != "Bank")
                {
                    return RedirectToAction("login", "Bank");
                }
                else
                {
                    ViewBag.SelectedItem = "";
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    string OutError = "0";
                    string Search = string.Empty;
                    Search = "BCODE='" + BM.BCODE + "' ";
                    DataSet ds = _bankRepository.GetAllFeeDepositByBCODE(BM, Search, out OutError);
                    BM.BankMasterData = ds;
                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                    }

                    if (id != null)
                    {
                        if (id.ToLower() == "download")
                        {
                            System.Data.DataView view = new System.Data.DataView(ds.Tables[0]);
                            System.Data.DataTable DtSelected =
                                    view.ToTable("Selected", false, "ChallanId", "Mobile", "BRCODECAND", "BRANCHCAND", "J_REF_NOCAND",
                                    "DEPOSITDTCAND", "CHLNDATE", "CHLNVDATE", "PaidFees", "APPNO", "SCHLREGID", "SCHLCANDNM", "DOWNLDDATE", "DOWNLDFLOT", "SUBMITCAND");

                            ExportDataFromDataTable(2, DtSelected, BM.BCODE,"Download");
                        }

                    }
                }
                return View(BM);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewAllFeeDeposit(BankModels BM, FormCollection frm)
        {
            try
            {

                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
                var itemsch = new SelectList(new[]{new {ID="1",Name="Challan Id"},new{ID="2",Name="AppNo/RefNo"},
                     new{ID="3",Name="Download Date"},new{ID="4",Name="Deposit Date"}}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                int OutStatus;
                if (bankLoginSession.BANKNAME == null || Session["RoleType"].ToString() != "Bank")
                {
                    return RedirectToAction("login", "Bank");
                }
                else
                {
                    ViewBag.SelectedItem = "";
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    string OutError = "0";

                    string Search = string.Empty;
                    Search = "BCODE='" + BM.BCODE + "' ";
                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());
                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and CHALLANID='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 2)
                            { Search += " and  AppNo='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and BRCODECAND='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 4)
                            { Search += " and convert(varchar(10),DEPOSITDTCAND)='" + frm["SearchString"].ToString() + "'"; }
                        }
                    }
                    DataSet ds = _bankRepository.GetAllFeeDepositByBCODE(BM, Search, out OutError);
                    BM.BankMasterData = ds;
                    if (ds == null || ds.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCount = ds.Tables[0].Rows.Count;
                    }
                }
                return View(BM);
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        #endregion View All Fee Deposit By BCODE



        #region VerifyMIS Cancel
        [BankLoginCheckFilter]
        public ActionResult VerifyMISCancel()
        {

            BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];            
            return View();
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult VerifyMISCancel(BankModels BM) // HttpPostedFileBase file
        {
            try
            {
                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
               
                    string fileLocation = "";
                    string filename = "";
                    BM.Session = bankLoginSession.CurrentSession.ToString();
                    BM.BCODE = bankLoginSession.BCODE.ToString();
                    if (BM.file != null)
                    {
                        filename = Path.GetFileName(BM.file.FileName);
                    }
                    DataSet ds = new DataSet();
                    if (BM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "MIS_" + BM.BCODE + '_' + DateTime.Now.ToString("ddMMyyyyHHmm");  //MIS_201_110720161210
                        string fileExtension = System.IO.Path.GetExtension(BM.file.FileName);
                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            fileLocation = Server.MapPath("~/BankUpload/" + fileName1 + fileExtension);
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
                            BM.file.SaveAs(fileLocation);
                            string excelConnectionString = string.Empty;
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            if (fileExtension == ".xls")
                            {
                                excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            }
                            //connection String for xlsx file format.
                            else if (fileExtension == ".xlsx")
                            {
                                excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            }
                            //Create Connection to Excel work book and add oledb namespace
                            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                            excelConnection.Open();
                            DataTable dt = new DataTable();
                            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (dt == null)
                            {
                                return null;
                            }

                            String[] excelSheets = new String[dt.Rows.Count];
                            int t = 0;
                            //excel data saves in temp file here.
                            foreach (DataRow row in dt.Rows)
                            {
                                excelSheets[t] = row["TABLE_NAME"].ToString(); // bank_mis     TABLE_NAME
                                t++;
                            }
                            OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);
                            string query = string.Format("Select * from [{0}]", excelSheets[0]);
                            using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                            {
                                dataAdapter.Fill(ds);
                            }

                            if (ds.Tables[0].Rows.Count > 5000)
                            {
                                ViewData["Result"] = "6";
                                ViewBag.Message = "Please Upload less than 5000 Challans";
                                return View();
                            }

                            string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                            // bool CheckStatus = StaticDB.CheckArrayDuplicates(array);
                            bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                            if (CheckChln == true)
                            {
                                ViewData["Result"] = "11";
                                ViewBag.Message = "Duplicate Challan Number";
                                return View();
                            }
                            //  string CheckMis = _bankRepository.CheckMisExcel(ds);
                            DataTable dtexport = new DataTable();
                            string CheckMis = _bankRepository.CheckMisExcelExportRN(ds, out dtexport);//CheckMisExcelExport
                            if (CheckMis == "")
                            {
                                //---Unique Uploadlot For Bulk Excel Sheet -----Changed By Amar Jnana
                                int UPLOADLOT = 0;
                                DataSet Bmis = _bankRepository.GetTotBankMIS(BM);
                                UPLOADLOT = Convert.ToInt32(Bmis.Tables[0].Rows[0][0].ToString());
                                //End Changed By Amar Jnana                                                        


                                #region SingleChallan
                                string Result1 = "";
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    // BankModels BM = new BankModels();
                                    BM.CHALLANID = ds.Tables[0].Rows[i][0].ToString();
                                    BM.TOTFEE = Convert.ToInt32(ds.Tables[0].Rows[i][1].ToString());
                                    BM.BRCODE = ds.Tables[0].Rows[i][2].ToString();
                                    BM.BRANCH = ds.Tables[0].Rows[i][3].ToString();
                                    BM.J_REF_NO = ds.Tables[0].Rows[i][4].ToString();
                                    BM.DEPOSITDT = ds.Tables[0].Rows[i][5].ToString();
                                    BM.MIS_FILENM = fileName1;       // "MIS_101_210620161659.xls"  

                                    // change for Cancel challan
                                    //DataSet dsChallanDetails = _bankRepository.GetChallanDetailsById(BM.CHALLANID);

                                    DataSet dsChallanDetails = _bankRepository.GetChallanDetailsByIdSPBank(BM.CHALLANID);
                                    if (dsChallanDetails.Tables[0].Rows.Count > 0)
                                    {
                                        int OutStatus;
                                        string Mobile = "0";
                                        int IsVerified = Convert.ToInt32(dsChallanDetails.Tables[0].Rows[0]["VERIFIED"].ToString());
                                        if (IsVerified == 0)
                                        {

                                            BM.LOT = Convert.ToInt32(dsChallanDetails.Tables[0].Rows[0]["LOT"].ToString());
                                            //ImportBankMis
                                            DataTable dtResult = _bankRepository.ImportBankMis(BM, UPLOADLOT, out OutStatus, out Mobile);// OutStatus mobile
                                            if (OutStatus > 0)
                                            {

                                            }
                                            else
                                            {
                                                //ViewData["Result"] = "0";
                                                //return View();
                                                int RowNo = i + 2;
                                                Result1 += "Challan not/already uploaded, please check ChallanId " + BM.CHALLANID + " in row  " + RowNo + ",  ";
                                                // ViewBag.Result1 = Result1;
                                            }
                                        }
                                    }
                                }
                                if (Result1 == "")
                                { ViewBag.Result1 = "All Challans Uploaded Successfully"; }
                                else { ViewBag.Result1 = Result1 + ", Rest of the Challans Uploaded Successfully"; }
                                ViewData["Result"] = "1";
                                #endregion SingleChallan


                                return View();
                            }
                            else
                            {
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(1, dtexport, BM.BCODE,"ERROR");
                                }
                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }
                        }
                        else
                        {

                            ViewData["Result"] = "2";
                            ViewBag.Message = "Please Upload Only .xls and .xlsx only";
                            return View();
                        }
                    }
               
            }
            catch (Exception ex)
            {
                ViewData["Result"] = "0";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion VerifyMIS

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }


        #region Check Challan Status
        [BankLoginCheckFilter]
        public ActionResult CheckChallanStatus(int? page)
        {
            try
            {

                BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];
              
                ViewBag.date = DateTime.Now.ToString("dd/MM/yyyy");

                // By Rohit  -- select bank from database
                DataSet dsBank = DBClass.Fll_FeeCat_Details();
                if (dsBank != null)
                {
                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["Bank"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        // ViewBag.MySch = itemBank.ToList().Select(s=>s.Value == bankLoginSession.BCODE.ToString()).ToList();

                        itemBank = itemBank.Where(item => item.Value.ToUpper().Trim() == bankLoginSession.BCODE.ToString().ToUpper().Trim()).ToList();
                        ViewBag.MySch = itemBank.ToList();
                    }
                }
                // End 

                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);

                ViewBag.feecat = DBClass.GetFeeCat();

                ViewBag.MySch1 = itemsch1;
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                Challan obj = new Challan();
                string session = null;
                string schl = null;               
                string Search = string.Empty;
                if (pageIndex == 1)
                {
                    TempData["search"] = null;
                    TempData["SelList"] = null;
                    TempData["SearchString"] = null;
                }
                string dee = "%";
                if (TempData["search"] == null)
                    Search = "SCHLREGID like '" + dee + "'";
                else
                {
                    Search = Convert.ToString(TempData["search"]);
                    ViewBag.SelectedItem = TempData["SelList"];
                    ViewBag.Searchstring = TempData["SearchString"];
                }

                Search += " and BCODE= '" + bankLoginSession.BCODE + "'";
                obj.StoreAllData = _bankRepository.GetChallanDetails(Search, session, pageIndex);//GetChallanDetails
                int count = 0;
                for (int i = 0; i <= obj.StoreAllData.Tables[1].Rows.Count - 1; i++)
                {
                    count = Convert.ToInt32(obj.StoreAllData.Tables[1].Rows[i].ItemArray[0]);

                }
                if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.TotalCount1 = count;
                    int tp = Convert.ToInt32(count);
                    int pn = tp / 20;
                    int cal = 20 * pn;
                    int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                    if (res >= 1)
                        ViewBag.pn = pn + 1;
                    else
                        ViewBag.pn = pn;

                }

                return View(obj);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [BankLoginCheckFilter]
        [HttpPost]
        public ActionResult CheckChallanStatus(int? page, Challan obj, FormCollection frm, string cmd, string BankName, string srhfld, string SearchString, string feecat1)
        {

            BankLoginSession bankLoginSession = (BankLoginSession)Session["BankLoginSession"];           
            try
            {
                if (cmd != null)
                {
                    if (cmd.ToLower() == "reset")
                    {
                        TempData["search"] = null;
                        TempData["SelList"] = null;
                        TempData["srhfld"] = null;
                        TempData["SearchString"] = null;
                        return RedirectToAction("CheckChallanStatus", "Bank");
                    }
                }


                ViewBag.date = DateTime.Now.ToString("dd/MM/yyyy");

                // By Rohit  -- select bank from database
                DataSet dsBank = DBClass.Fll_FeeCat_Details();
                if (dsBank != null)
                {
                    if (dsBank.Tables[1].Rows.Count > 0)
                    {
                        List<SelectListItem> itemBank = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsBank.Tables[1].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["Bank"].ToString().Trim(), Value = @dr["Bcode"].ToString().Trim() });
                        }
                        itemBank = itemBank.Where(item => item.Value.ToUpper().Trim() == bankLoginSession.BCODE.ToString().ToUpper().Trim()).ToList();
                        ViewBag.MySch = itemBank.ToList();
                    }
                }

                // End

                var itemsch1 = new SelectList(new[] { new { ID = "CHALLANID", Name = "Challan ID" }, new { ID = "APPNO", Name = "Appno/RefNo" }, new { ID = "SCHLREGID", Name = "SCHLREGID" }, new { ID = "LOT", Name = "downloaded lot" }, new { ID = "J_REF_NO", Name = "Journal No" } }, "ID", "Name", 1);
                ViewBag.MySch1 = itemsch1.ToList();
                ViewBag.feecat = DBClass.GetFeeCat();
                string session = null;
                string schl = null;
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;              
                Challan rm = new Challan();

                string Search = "";
                if (cmd == "Search")
                {
                    string dee = "%";
                    Search = "SCHLREGID like '" + dee + "'";
                    if (BankName != "")
                    {
                        Search += " and a.Bcode='" + BankName.ToString().Trim() + "'";
                        ViewBag.SelectedItem = BankName;
                    }
                    if (feecat1 != "")
                    {
                        Search += " and a.FEECAT like '%" + feecat1.ToString().Trim() + "%'";
                        ViewBag.SelectedItem = BankName;
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
                        //else
                        //    Search += " and a.SCHLREGID='" + SearchString.ToString().Trim() + "'";

                    }

                    Search += " and BCODE= '" + bankLoginSession.BCODE + "'";

                    TempData["search"] = Search;
                    TempData["SelList"] = BankName;
                    TempData["srhfld"] = srhfld;
                    TempData["SearchString"] = SearchString.ToString().Trim();
                    ViewBag.Searchstring = SearchString.ToString().Trim();

                    rm.StoreAllData = _bankRepository.GetChallanDetails(Search, session, pageIndex);//GetChallanDetails
                    int count = 0;
                    for (int i = 0; i <= rm.StoreAllData.Tables[1].Rows.Count - 1; i++)
                    {
                        count = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[i].ItemArray[0]);

                    }
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = Convert.ToInt32(count);
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        //return View(rm);
                        //ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;


                        return View(rm);

                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        #endregion Check Challan Status
    }
}