using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using PsebJunior.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
//using NReco.PdfGenerator;
using PsebJunior.AbstractLayer;
using PsebPrimaryMiddle.Filters;
using System.Threading.Tasks;
using PsebPrimaryMiddle.Repository;
using System.Configuration;
using CCA.Util;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon;

namespace PsebPrimaryMiddle.Controllers
{
    public class RegistrationPortalController : Controller
    {
        //AbstractLayer.DBClass objCommon = new AbstractLayer.DBClass();
        //AbstractLayer.ErrorLog oErrorLog = new AbstractLayer.ErrorLog();
        //
        private readonly IAdminRepository _adminRepository;
        private readonly IChallanRepository _challanRepository;
        private const string BUCKET_NAME = "psebdata";
        public RegistrationPortalController(IChallanRepository challanRepository, IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
            _challanRepository = challanRepository;
        }


        RegistrationModels rm = new RegistrationModels();


        // GET: RegistrationPortal



        public JsonResult BindBoardByCategory(string SelCat) // Calling on http post (on Submit)
        {

            List<SelectListItem> _list = new List<SelectListItem>();
            if (SelCat == "4TH PASSED")
            {
                _list = DBClass.GetBoardList(); // 
            }
            else if (SelCat == "5TH FAILED")
            {
                _list = DBClass.GetBoardList2(); // remove pseb
            }
            else if (SelCat == "7TH PASSED")
            {
                _list = DBClass.GetBoardList(); // 
            }
            else if (SelCat == "8TH FAILED")
            {
                _list = DBClass.GetBoardList2(); // remove pseb
            }
            ViewBag.MyBoard = _list;
            return Json(_list);
        }


        public JsonResult BindRegistrationSessionYear(string SelCat, string SelForm, string SelBoard) // Calling on http post (on Submit)
        {

            List<SelectListItem> yearlist = new List<SelectListItem>();
            if (SelForm == "F1")
            {
                if (SelCat == "4TH PASSED")
                {
                    yearlist = DBClass.GetSessionYear().Take(3).ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 2021 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }

            }
            else if (SelForm == "F2")
            {
                //if (SelCat == "SD" && SelRP == "R")
                if (SelCat == "4TH PASSED" && SelBoard.Contains("P.S.E.B"))
                {
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2020).ToList();
                }
                else if (SelCat == "4TH PASSED" && !SelBoard.Contains("P.S.E.B"))
                {
                    yearlist = DBClass.GetSessionYear().ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }
                else if (SelCat == "5TH FAILED" && SelBoard.Contains("P.S.E.B"))
                {
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2020).ToList();
                }
                else if (SelCat == "5TH FAILED" && !SelBoard.Contains("P.S.E.B"))
                {
                    //yearlist = DBClass.GetSessionYear().Take(2).ToList();
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2023).ToList();
                }
                else
                {
                    yearlist = DBClass.GetSessionYear().ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }
            }
            else if (SelForm == "A1")
            {
                if (SelCat == "7TH PASSED")
                {
                    yearlist = DBClass.GetSessionYear().Take(3).ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 2021 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }

            }
            else if (SelForm == "A2")
            {
                if (SelCat == "7TH PASSED" && SelBoard.Contains("P.S.E.B"))
                {
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2020).ToList();
                }
                else if (SelCat == "7TH PASSED" && !SelBoard.Contains("P.S.E.B"))
                {
                    yearlist = DBClass.GetSessionYear().ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }
                else if (SelCat == "8TH FAILED" && !SelBoard.Contains("P.S.E.B"))
                {
                    //yearlist = DBClass.GetSessionYear().Take(2).ToList();
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2023).ToList();
                }
                else if (SelCat == "8TH FAILED" && SelBoard.Contains("P.S.E.B"))
                {
                    //yearlist = DBClass.GetSessionYear().Take(2).ToList();
                    yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2020).ToList();
                }

                else
                {
                    yearlist = DBClass.GetSessionYear().ToList();
                    //yearlist = DBClass.GetSessionYear().Where(s => Convert.ToInt32(s.Value) >= 1969 && Convert.ToInt32(s.Value) <= 2022).ToList();
                }
            }

            ViewBag.MyYear = yearlist;
            return Json(yearlist);
        }


        [SessionCheckFilter]
        public ActionResult CommonFormView(string id, string formNM)
        {

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("A2Formgrid", "RegistrationPortal");
            }
            string formname = formNM;
            //          
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(formname))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                string search = "";
                DataSet ds = new DataSet();
                rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                if (rm == null)
                {
                    return RedirectToAction("A2Formgrid", "RegistrationPortal");
                }
                else
                {
                    #region  Check Subject Table for Class 8th
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                            {

                                if (i == 0)
                                {
                                    rm.subS1 = ds.Tables[1].Rows[0]["SUBNM"].ToString();
                                }
                                else if (i == 1)
                                {
                                    rm.subS2 = ds.Tables[1].Rows[1]["SUBNM"].ToString();
                                }
                                else if (i == 2)
                                {
                                    rm.subS3 = ds.Tables[1].Rows[2]["SUBNM"].ToString();
                                }
                                else if (i == 3)
                                {
                                    rm.subS4 = ds.Tables[1].Rows[3]["SUBNM"].ToString();
                                }
                                else if (i == 4)
                                {
                                    rm.subS5 = ds.Tables[1].Rows[4]["SUBNM"].ToString();
                                }

                                else if (i == 5)
                                {
                                    rm.subS6 = ds.Tables[1].Rows[5]["SUBNM"].ToString();
                                }
                                else if (i == 6)
                                {
                                    rm.subS7 = ds.Tables[1].Rows[6]["SUBNM"].ToString();
                                }
                                else if (i == 7)
                                {
                                    rm.subS8 = ds.Tables[1].Rows[7]["SUBNM"].ToString();
                                }
                                else if (i == 8)
                                {
                                    rm.subS9 = ds.Tables[1].Rows[i]["SUBNM"].ToString();
                                }
                                else if (i == 9)
                                {
                                    rm.s9 = ds.Tables[1].Rows[i]["SUBNM"].ToString();
                                }
                            }

                        }
                    }
                    #endregion

                }


            }
            return View(rm);
        }

        #region portal & agree
        public ActionResult Index()
        {
            return View();
        }

        [SessionCheckFilter]
        public ActionResult Portal()
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                ViewBag.Eighth = loginSession.middle == "Y" ? "1" : "0";
                ViewBag.Fifth = loginSession.fifth == "Y" ? "1" : "0";
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
            }
            return View();
        }

        [SessionCheckFilter]
        public ActionResult Agree(string id)
        {
            try
            {
                //Session["FormName"] =
                ViewBag.FormName = id;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
            }
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult Agree(string id, RegistrationModels rm, FormCollection frm)
        {
            try
            {
                string s = frm["Agree"].ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Portal", "RegistrationPortal");
                }
                else
                {
                    ViewBag.FormName = id.ToString();
                    if (s == "Agree")
                    {
                        if (ViewBag.FormName == "F1")//F1Master
                        {
                            return RedirectToAction("F1Master", "RegistrationPortal");
                        }
                        else if (ViewBag.FormName == "F2")
                        {
                            return RedirectToAction("F2Form", "RegistrationPortal");
                        }

                        else if (ViewBag.FormName == "A1")//E1Master
                        {
                            //return RedirectToAction("E1Form", "RegistrationPortal");
                            return RedirectToAction("A1Master", "RegistrationPortal");
                        }
                        else if (ViewBag.FormName == "A2")
                        {
                            return RedirectToAction("A2Form", "RegistrationPortal");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Portal", "RegistrationPortal");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("Logout", "Login");
            }
        }

        #endregion

        #region F1 Master for 5th Class
        [SessionCheckFilter]
        public ActionResult F1Master()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }
            return View();
        }


        [SessionCheckFilter]
        public ActionResult F1Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }
            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("F1", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }




        [SessionCheckFilter]
        public ActionResult F1FormDelete(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("F1Formgrid", "RegistrationPortal");
            }
            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            ViewData["resultDelete"] = OutError;
            return RedirectToAction("F1Formgrid", "RegistrationPortal");
        }

        [SessionCheckFilter]
        public ActionResult F1Form(string id, RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }


            ViewBag.CANDID = id;
            string formname = "F1";
            //****** Common 
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;


            if (string.IsNullOrEmpty(id))
            {
                rm.LastEntryCandidate = new LastEntryCandidate();
                DataSet seleLastCan = RegistrationDB.SelectlastEntryCandidate(formname, loginSession.SCHL);
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {
                    ViewBag.message = "1";
                    rm.LastEntryCandidate.Board_Roll_Num = seleLastCan.Tables[0].Rows[0]["Board_Roll_Num"].ToString();
                    rm.LastEntryCandidate.Admission_Num = seleLastCan.Tables[0].Rows[0]["Admission_Num"].ToString();
                    rm.LastEntryCandidate.Candi_Name = seleLastCan.Tables[0].Rows[0]["Candi_Name"].ToString();
                    rm.LastEntryCandidate.Father_Name = seleLastCan.Tables[0].Rows[0]["Father_Name"].ToString();
                    rm.LastEntryCandidate.Id = seleLastCan.Tables[0].Rows[0]["Std_id"].ToString();
                    rm.LastEntryCandidate.Lot = seleLastCan.Tables[0].Rows[0]["lot"].ToString();
                }
                else
                {
                    ViewBag.message = "Record Not Found";
                }
            }
            else if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("F1Formgrid", "RegistrationPortal");
                    }
                    else
                    {

                        #region  Check Subject Table for Class 5th an 8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[0]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[0]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[1]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[1]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[2]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[2]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[3]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[3]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[4]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[4]["MEDIUM"].ToString();
                                    }
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[5]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[5]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }





            #region  Class FifthInitilize List Model
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static

            //ViewBag.SessionYearList = DBClass.GetSessionYear(); // DB         
            //ViewBag.DAList = DBClass.GetDA();// DB
            //ViewBag.MyDist = DBClass.GetDistE();// DB
            #endregion  Class FifthInitilize List Model
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult F1Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.CANDID = id;

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;

            //
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static

            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;
                string formName = "F1";

                // Start Subject Master
                DataTable dtFifthSubject = new DataTable();
                dtFifthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 6; i++)
                {
                    dr = dtFifthSubject.NewRow();
                    dr["CLASS"] = 5;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = "";

                    //if (rm.DA == "N.A.")
                    //{                     
                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    j = i;
                    dtFifthSubject.Rows.Add(dr);
                }

                dtFifthSubject.AcceptChanges();
                dtFifthSubject = dtFifthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master
                if (dtFifthSubject.Rows.Count != 6)
                {
                    ViewData["result"] = "SUBCOUNT";
                    return View(rm);
                }


                // DOB FORMAT
                if (!string.IsNullOrEmpty(rm.DOB))
                {
                    rm.DOB = StaticDB.DateInFormat(rm.DOB) == true ? rm.DOB : "";
                }


                if (string.IsNullOrEmpty(id) && cmd.ToLower().Contains("save"))
                {
                    string result = RegistrationDB.Ins_F_Form_Data(rm, frm, formName, loginSession.CurrentSession, loginSession.SCHL, dtFifthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = -1;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 1;
                    }
                }
                //Update data
                else if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_F_Data(rm, frm, formName, id, "", "", dtFifthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }



            }

            return View(rm);
        }

        #endregion


        #region  F2 Form 

        [SessionCheckFilter]
        public ActionResult F2Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }

            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("F2", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }

        [SessionCheckFilter]
        public ActionResult F2FormDelete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("F2Formgrid", "RegistrationPortal");
            }
            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            ViewData["resultDelete"] = OutError;
            return RedirectToAction("F2Formgrid", "RegistrationPortal");
        }

        [SessionCheckFilter]
        public ActionResult F2Form(string id, RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }
            ViewBag.CANDID = id;

            string formname = "F2";
            string schl = loginSession.SCHL.ToString();
            //****** Common 
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;



            if (string.IsNullOrEmpty(id))
            {
                rm.LastEntryCandidate = new LastEntryCandidate();
                DataSet seleLastCan = RegistrationDB.SelectlastEntryCandidate(formname, loginSession.SCHL);
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {
                    ViewBag.message = "1";
                    rm.LastEntryCandidate.Board_Roll_Num = seleLastCan.Tables[0].Rows[0]["Board_Roll_Num"].ToString();
                    rm.LastEntryCandidate.Admission_Num = seleLastCan.Tables[0].Rows[0]["Admission_Num"].ToString();
                    rm.LastEntryCandidate.Candi_Name = seleLastCan.Tables[0].Rows[0]["Candi_Name"].ToString();
                    rm.LastEntryCandidate.Father_Name = seleLastCan.Tables[0].Rows[0]["Father_Name"].ToString();
                    rm.LastEntryCandidate.Id = seleLastCan.Tables[0].Rows[0]["Std_id"].ToString();
                    rm.LastEntryCandidate.Lot = seleLastCan.Tables[0].Rows[0]["lot"].ToString();
                }
                else
                {
                    ViewBag.message = "Record Not Found";
                }
            }
            else if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("F2Formgrid", "RegistrationPortal");
                    }
                    else
                    {
                        #region  Check Subject Table for Class 5th an 8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[0]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[0]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[1]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[1]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[2]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[2]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[3]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[3]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[4]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[4]["MEDIUM"].ToString();
                                    }
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[5]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[5]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }
            // Last Entry Candidate


            #region  Class FifthInitilize List Model
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            //rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); 

            rm.ClassFifthInitilizeListModel.MyBoard = new List<SelectListItem>();
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static

            //ViewBag.SessionYearList = DBClass.GetSessionYear(); // DB         
            //ViewBag.DAList = DBClass.GetDA();// DB
            //ViewBag.MyDist = DBClass.GetDistE();// DB
            #endregion  Class FifthInitilize List Model

            if (id != null && rm != null)
            {
                if (rm.Category == "4TH PASSED")
                {
                    rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // 
                }
                else if (rm.Category == "5TH FAILED")
                {
                    rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList2(); // remove pseb
                }
            }
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult F2Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.CANDID = id;
            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;
            //
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static


            //Check server side validation using data annotation
            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;
                string formName = "F2";


                // Start Subject Master
                DataTable dtFifthSubject = new DataTable();
                dtFifthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 6; i++)
                {
                    dr = dtFifthSubject.NewRow();
                    dr["CLASS"] = 5;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = rm.subM6 = "";

                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    j = i;
                    dtFifthSubject.Rows.Add(dr);
                }

                dtFifthSubject.AcceptChanges();
                dtFifthSubject = dtFifthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master


                if (dtFifthSubject.Rows.Count != 6)
                {
                    ViewData["result"] = "SUBCOUNT";
                    return View(rm);
                }

                if (rm.DocProofCertificate != null)
                {
                    rm.ProofCertificate = Path.GetFileName(rm.DocProofCertificate.FileName);
                }

                if (rm.DocProofNRICandidates != null)
                {
                    rm.ProofNRICandidates = Path.GetFileName(rm.DocProofNRICandidates.FileName);
                }

                //IsNRICandidate
                if (rm.IsNRICandidate == true && (rm.DocProofNRICandidates != null || !string.IsNullOrEmpty(rm.ProofNRICandidates)))
                {
                    rm.IsNRICandidate = true;
                }
                else
                {
                    rm.IsNRICandidate = false;
                    rm.ProofNRICandidates = "";
                    rm.DocProofNRICandidates = null;
                }



                // DOB FORMAT
                if (!string.IsNullOrEmpty(rm.DOB))
                {
                    rm.DOB = StaticDB.DateInFormat(rm.DOB) == true ? rm.DOB : "";
                }



                int savestatus = 0;
                string studentID = "";
                if (string.IsNullOrEmpty(id) && cmd.ToLower().Contains("save"))
                {
                    string result = RegistrationDB.Ins_F_Form_Data(rm, frm, formName, loginSession.CurrentSession, loginSession.SCHL, dtFifthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = -1;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        savestatus = 1;
                        studentID = result;
                        ModelState.Clear();
                        ViewData["result"] = 1;
                    }
                }
                //Update data
                else if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_F_Data(rm, frm, formName, id, "", "", dtFifthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        savestatus = 20;
                        studentID = result;
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }

                string schlDist = loginSession.DIST;


                if (savestatus == 1 || savestatus == 20)
                {
                    // Documents

                    if (rm.DocProofCertificate != null)
                    {
                        string fileExt = Path.GetExtension(rm.DocProofCertificate.FileName);
                        string Orgfile = studentID + "C" + fileExt;

                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = rm.DocProofCertificate.InputStream,
                                    Key = string.Format("allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofCertificate/{0}", Orgfile),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }
                        rm.ProofCertificate = "allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofCertificate" + "/" + studentID + "C" + fileExt;
                        string type = "C";
                        string UpdatePicC = RegistrationDB.Updated_Pic_Data(studentID, rm.ProofCertificate, type);

                    }

                    if (rm.DocProofNRICandidates != null)
                    {
                        string fileExt = Path.GetExtension(rm.DocProofNRICandidates.FileName);
                        string Orgfile = studentID + "NRI" + fileExt;

                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = rm.DocProofNRICandidates.InputStream,
                                    Key = string.Format("allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofNRICandidates/{0}", Orgfile),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }
                        rm.ProofNRICandidates = "allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofNRICandidates" + "/" + studentID + "NRI" + fileExt;
                        string type = "NRI";
                        string UpdatePicC = RegistrationDB.Updated_Pic_Data(studentID, rm.ProofNRICandidates, type);

                    }

                }

            }

            return View(rm);
        }

        #endregion  F2 Form 

        #region A1 Master for 8th Class
        [SessionCheckFilter]
        public ActionResult A1Master()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }
            return View();
        }


        [SessionCheckFilter]
        public ActionResult A1Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("A1", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }


        [SessionCheckFilter]
        public ActionResult A1FormDelete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("A1Formgrid", "RegistrationPortal");
            }
            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            ViewData["resultDelete"] = OutError;
            return RedirectToAction("A1Formgrid", "RegistrationPortal");
        }

        [SessionCheckFilter]
        public ActionResult A1Form(string id, RegistrationModels rm)
        {



            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            ViewBag.CANDID = id;

            string formname = "A1";

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;

            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            if (string.IsNullOrEmpty(id))
            {
                rm.LastEntryCandidate = new LastEntryCandidate();
                DataSet seleLastCan = RegistrationDB.SelectlastEntryCandidate(formname, loginSession.SCHL);
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {
                    ViewBag.message = "1";
                    rm.LastEntryCandidate.Board_Roll_Num = seleLastCan.Tables[0].Rows[0]["Board_Roll_Num"].ToString();
                    rm.LastEntryCandidate.Admission_Num = seleLastCan.Tables[0].Rows[0]["Admission_Num"].ToString();
                    rm.LastEntryCandidate.Candi_Name = seleLastCan.Tables[0].Rows[0]["Candi_Name"].ToString();
                    rm.LastEntryCandidate.Father_Name = seleLastCan.Tables[0].Rows[0]["Father_Name"].ToString();
                    rm.LastEntryCandidate.Id = seleLastCan.Tables[0].Rows[0]["Std_id"].ToString();
                    rm.LastEntryCandidate.Lot = seleLastCan.Tables[0].Rows[0]["lot"].ToString();
                }
                else
                {
                    ViewBag.message = "Record Not Found";
                }
            }
            else if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("A1Formgrid", "RegistrationPortal");
                    }
                    else
                    {

                        #region  Check Subject Table for Class  8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    //
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 6)
                                    {
                                        rm.subS7 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM7 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 7)
                                    {
                                        rm.subS8 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM8 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 8)
                                    {
                                        rm.subS9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 9)
                                    {
                                        rm.s9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.m9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }



            #region  Class Middle Initilize List Model
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            #endregion
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult A1Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.CANDID = id;
            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;
            //common
            //
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            //ViewBag.ElectiveSubjects = DBClass.GetSub08ElectiveSubjectList();

            //Check server side validation using data annotation
            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;
                string formName = "A1";
                // Start Subject Master
                DataTable dtEighthSubject = new DataTable();
                dtEighthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 10; i++)
                {
                    dr = dtEighthSubject.NewRow();
                    dr["CLASS"] = 8;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = rm.subM6 = rm.subM7 = rm.subM8 = rm.subM9 = rm.m9 = "";


                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    //
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 7)
                    {
                        dr["SUB"] = rm.subS7; dr["MEDIUM"] = rm.subM7; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 8)
                    {
                        dr["SUB"] = rm.subS8; dr["MEDIUM"] = rm.subM8; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 9)
                    {
                        dr["SUB"] = rm.subS9; dr["MEDIUM"] = rm.subM9; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 10)
                    {
                        dr["SUB"] = rm.s9; dr["MEDIUM"] = rm.m9; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "A";
                    }
                    j = i;

                    dtEighthSubject.Rows.Add(dr);
                }

                dtEighthSubject.AcceptChanges();
                dtEighthSubject = dtEighthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master

                if (dtEighthSubject.Rows.Count != 10)
                {
                    ViewData["result"] = "SUBCOUNT";
                    return View(rm);
                }


                // DOB FORMAT
                if (!string.IsNullOrEmpty(rm.DOB))
                {
                    rm.DOB = StaticDB.DateInFormat(rm.DOB) == true ? rm.DOB : "";
                }

                if (string.IsNullOrEmpty(id) && cmd.ToLower().Contains("save"))
                {
                    string result = RegistrationDB.Ins_A_Form_Data(rm, frm, formName, loginSession.CurrentSession, loginSession.SCHL, dtEighthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = -1;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 1;
                    }
                }
                //Update data
                else if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_A_Data(rm, frm, formName, id, "", "", dtEighthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }



            }

            return View(rm);
        }

        #endregion

        #region  A2 Form 


        [SessionCheckFilter]
        public ActionResult A2Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("A2", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }




        [SessionCheckFilter]
        public ActionResult A2FormDelete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("A2Formgrid", "RegistrationPortal");
            }

            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            //ViewData["resultDelete"] = OutError;
            return RedirectToAction("A2Formgrid", "RegistrationPortal");
        }

        [SessionCheckFilter]
        public ActionResult A2Form(string id, RegistrationModels rm)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            ViewBag.CANDID = id;
            string formname = "A2";

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;

            //****** Common
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            if (string.IsNullOrEmpty(id))
            {
                rm.LastEntryCandidate = new LastEntryCandidate();
                DataSet seleLastCan = RegistrationDB.SelectlastEntryCandidate(formname, loginSession.SCHL);
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {
                    ViewBag.message = "1";
                    rm.LastEntryCandidate.Board_Roll_Num = seleLastCan.Tables[0].Rows[0]["Board_Roll_Num"].ToString();
                    rm.LastEntryCandidate.Admission_Num = seleLastCan.Tables[0].Rows[0]["Admission_Num"].ToString();
                    rm.LastEntryCandidate.Candi_Name = seleLastCan.Tables[0].Rows[0]["Candi_Name"].ToString();
                    rm.LastEntryCandidate.Father_Name = seleLastCan.Tables[0].Rows[0]["Father_Name"].ToString();
                    rm.LastEntryCandidate.Id = seleLastCan.Tables[0].Rows[0]["Std_id"].ToString();
                    rm.LastEntryCandidate.Lot = seleLastCan.Tables[0].Rows[0]["lot"].ToString();
                }
                else
                {
                    ViewBag.message = "Record Not Found";
                }
            }
            else if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("A2Formgrid", "RegistrationPortal");
                    }
                    else
                    {


                        #region  Check Subject Table for Class  8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    //
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 6)
                                    {
                                        rm.subS7 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM7 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 7)
                                    {
                                        rm.subS8 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM8 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 8)
                                    {
                                        rm.subS9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 9)
                                    {
                                        rm.s9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.m9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }
            // Last Entry Candidate

            #region  Class Middle Initilize List Model
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static

            // rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); 
            rm.ClassMiddleInitilizeListModel.MyBoard = new List<SelectListItem>();

            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            #endregion


            if (id != null && rm != null)
            {
                if (rm.Category == "7TH PASSED")
                {
                    rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // 
                }
                else if (rm.Category == "8TH FAILED")
                {
                    rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList2(); // remove pseb
                }
            }
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult A2Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            ViewBag.CANDID = id;

            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;
            //common
            //
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
                                                                                 // rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassMiddleInitilizeListModel.MyBoard = new List<SelectListItem>();
            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            //Check server side validation using data annotation
            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;

                string formName = "A2";


                // Start Subject Master
                DataTable dtEighthSubject = new DataTable();
                dtEighthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 10; i++)
                {
                    dr = dtEighthSubject.NewRow();
                    dr["CLASS"] = 8;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = rm.subM6 = rm.subM7 = rm.subM8 = rm.subM9 = rm.m9 = "";


                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    //
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 7)
                    {
                        dr["SUB"] = rm.subS7; dr["MEDIUM"] = rm.subM7; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 8)
                    {
                        dr["SUB"] = rm.subS8; dr["MEDIUM"] = rm.subM8; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 9)
                    {
                        dr["SUB"] = rm.subS9; dr["MEDIUM"] = rm.subM9; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 10)
                    {
                        dr["SUB"] = rm.s9; dr["MEDIUM"] = rm.m9; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "A";
                    }
                    j = i;

                    dtEighthSubject.Rows.Add(dr);
                }

                dtEighthSubject.AcceptChanges();
                dtEighthSubject = dtEighthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master

                if (dtEighthSubject.Rows.Count != 10)
                {
                    ViewData["result"] = "SUBCOUNT";
                    return View(rm);
                }
                if (rm.DocProofCertificate != null)
                {
                    rm.ProofCertificate = Path.GetFileName(rm.DocProofCertificate.FileName);
                }

                if (rm.DocProofNRICandidates != null)
                {
                    rm.ProofNRICandidates = Path.GetFileName(rm.DocProofNRICandidates.FileName);
                }

                //IsNRICandidate
                if (rm.IsNRICandidate == true && (rm.DocProofNRICandidates != null || !string.IsNullOrEmpty(rm.ProofNRICandidates)))
                {
                    rm.IsNRICandidate = true;
                }
                else
                {
                    rm.IsNRICandidate = false;
                    rm.ProofNRICandidates = "";
                    rm.DocProofNRICandidates = null;
                }



                // DOB FORMAT
                if (!string.IsNullOrEmpty(rm.DOB))
                {
                    rm.DOB = StaticDB.DateInFormat(rm.DOB) == true ? rm.DOB : "";
                }


                int savestatus = 0;
                string studentID = "";
                if (string.IsNullOrEmpty(id) && cmd.ToLower().Contains("save"))
                {
                    string result = RegistrationDB.Ins_A_Form_Data(rm, frm, formName, loginSession.CurrentSession, loginSession.SCHL, dtEighthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = -1;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        savestatus = 1;
                        studentID = result;
                        ModelState.Clear();
                        ViewData["result"] = 1;
                    }
                }
                //Update data
                else if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_A_Data(rm, frm, formName, id, "", "", dtEighthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        savestatus = 20;
                        studentID = result;
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }

                string schlDist = loginSession.DIST;


                if (savestatus == 1 || savestatus == 20)
                {
                    // Documents

                    if (rm.DocProofCertificate != null)
                    {
                        string fileExt = Path.GetExtension(rm.DocProofCertificate.FileName);
                        string Orgfile = studentID + "C" + fileExt;

                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = rm.DocProofCertificate.InputStream,
                                    Key = string.Format("allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofCertificate/{0}", Orgfile),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }
                        rm.ProofCertificate = "allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofCertificate" + "/" + studentID + "C" + fileExt;
                        string type = "C";
                        string UpdatePicC = RegistrationDB.Updated_Pic_Data(studentID, rm.ProofCertificate, type);
                    }

                    if (rm.DocProofNRICandidates != null)
                    {
                        string fileExt = Path.GetExtension(rm.DocProofNRICandidates.FileName);
                        string Orgfile = studentID + "NRI" + fileExt;

                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = rm.DocProofNRICandidates.InputStream,
                                    Key = string.Format("allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofNRICandidates/{0}", Orgfile),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }
                        rm.ProofNRICandidates = "allfiles/Upload2023/" + formName + "/" + schlDist + "/ProofNRICandidates" + "/" + studentID + "NRI" + fileExt;
                        string type = "NRI";
                        string UpdatePicC = RegistrationDB.Updated_Pic_Data(studentID, rm.ProofNRICandidates, type);
                    }

                }



            }

            return View(rm);
        }

        #endregion 

        #region Rough Report & Student Verification Form
        [SessionCheckFilter]
        public ActionResult RoughReportJunior(string Id)
        {
            ViewBag.FormName = Id;
            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
               new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult RoughReportJunior(string Id, FormCollection frm, string cmd, string PrintHtml)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.FormName = Id;
            try
            {
                var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
               new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                if (!string.IsNullOrEmpty(cmd))
                {
                    //if (cmd.ToLower().Contains("download") && !string.IsNullOrEmpty(PrintHtml))
                    //{

                    //    //string fileNM = SCHL + "_" + ChallanId;
                    //    string fileNM = "FinalPrint_" + FormName + ".pdf";                        
                    //    string htmlToConvert = PrintHtml.ToString();
                    //    var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();                       
                    //    var pdfBytes = htmlToPdf.GeneratePdf(htmlToConvert);
                    //    Response.Clear();
                    //    MemoryStream ms = new MemoryStream(pdfBytes);
                    //    Response.ContentType = "application/pdf";                       
                    //    Response.AddHeader("content-disposition", "attachment;filename="+ fileNM + "");
                    //    Response.Buffer = true;
                    //    ms.WriteTo(Response.OutputStream);
                    //    Response.End();                       
                    //}
                }


                RegistrationModels rm = new RegistrationModels();
                if (ModelState.IsValid)
                {

                    string Search = string.Empty;
                    Search = "form_Name='" + Id + "'  and schl='" + loginSession.SCHL + "' ";

                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());


                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += "and std_id like '%' "; }
                            else if (SelValueSch == 2)
                            { Search += " and std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and Father_Name  like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 5)
                            { Search += " and Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }

                            else if (SelValueSch == 7)
                            { Search += " and registration_Num like '%" + frm["SearchString"].ToString() + "%'"; }

                        }

                    }

                    rm.StoreAllData = RegistrationDB.GetStudentRoughReport5th8th_Sp(Search);
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.SCHL = rm.StoreAllData.Tables[0].Rows[0]["SCHL"];
                        ViewBag.IDNO = rm.StoreAllData.Tables[0].Rows[0]["IDNO"];
                        ViewBag.SSE = rm.StoreAllData.Tables[0].Rows[0]["SchoolStationE"];
                        ViewBag.SSP = rm.StoreAllData.Tables[0].Rows[0]["SchoolStationP"];
                        return View(rm);
                    }
                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }

        }


        [SessionCheckFilter]
        public ActionResult StudentVerificationFormJunior(string id)
        {
            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            ViewBag.FormName = id;
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult StudentVerificationFormJunior(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Unique ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},
                    }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                ViewBag.FormName = id;
                RegistrationModels rm = new RegistrationModels();
                if (ModelState.IsValid)
                {

                    string Search = string.Empty;
                    Search = "form_Name='" + id + "'  and schl='" + loginSession.SCHL + "' ";

                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());


                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += "and std_id like '%' "; }
                            else if (SelValueSch == 2)
                            { Search += " and std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and Father_Name  like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 5)
                            { Search += " and Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 7)
                            { Search += " and registration_Num like '%" + frm["SearchString"].ToString() + "%'"; }

                        }

                    }

                    rm.StoreAllData = RegistrationDB.GetStudentVerificationForm5th8th_Sp(Search);
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        return View(rm);
                    }
                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        #endregion

        #region OtherBoardAllottedRegno


        [SessionCheckFilter]
        public async Task<ActionResult> OtherBoardAllottedRegno(RegistrationAllStudentAdminModelList registrationAllStudentAdminModelList)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            // F2 means Form 2
            DataSet dsOut = new DataSet();
            registrationAllStudentAdminModelList.RegistrationAllStudentAdminModel = await RegistrationDB.GetAllFinalSubmittedStudentBySchlPM("F2", loginSession.SCHL, out dsOut);
            registrationAllStudentAdminModelList.StoreAllData = dsOut;
            return View(registrationAllStudentAdminModelList);
        }

        #endregion OtherBoardAllottedRegno


        #region  

        [SessionCheckFilter]
        public ActionResult F3Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }
            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("F3", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }

        [SessionCheckFilter]
        public ActionResult F3FormView(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }
            else if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("F3Formgrid", "RegistrationPortal");
            }
            //loginSession = (LoginSession)Session["LoginSession"];

            //

            string formname = "F3";
            if (id != null)
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                string search = "";
                DataSet ds = new DataSet();
                rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                if (rm == null)
                {
                    return RedirectToAction("F3Formgrid", "RegistrationPortal");
                }
                else
                {
                    #region  Check Subject Table for Class 5th an 8th
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                            {

                                if (i == 0)
                                {
                                    rm.subS1 = ds.Tables[1].Rows[0]["SUBNM"].ToString();
                                    // rm.subm1 = ds.Tables[1].Rows[0]["MEDIUM"].ToString();
                                }
                                else if (i == 1)
                                {
                                    rm.subS2 = ds.Tables[1].Rows[1]["SUBNM"].ToString();
                                    // rm.subM2 = ds.Tables[1].Rows[1]["MEDIUM"].ToString();
                                }
                                else if (i == 2)
                                {
                                    rm.subS3 = ds.Tables[1].Rows[2]["SUBNM"].ToString();
                                    // rm.subm3 = ds.Tables[1].Rows[2]["MEDIUM"].ToString();
                                }
                                else if (i == 3)
                                {
                                    rm.subS4 = ds.Tables[1].Rows[3]["SUBNM"].ToString();
                                    // rm.subM4 = ds.Tables[1].Rows[3]["MEDIUM"].ToString();
                                }
                                else if (i == 4)
                                {
                                    rm.subS5 = ds.Tables[1].Rows[4]["SUBNM"].ToString();
                                    //  rm.subM5 = ds.Tables[1].Rows[4]["MEDIUM"].ToString();
                                }
                                else if (i == 5)
                                {
                                    rm.subS6 = ds.Tables[1].Rows[5]["SUBNM"].ToString();
                                    //  rm.subM5 = ds.Tables[1].Rows[4]["MEDIUM"].ToString();
                                }
                            }

                        }
                    }
                    #endregion

                }


            }
            return View(rm);
        }

        [SessionCheckFilter]
        public ActionResult F3FormDelete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("F3Formgrid", "RegistrationPortal");
            }
            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            ViewData["resultDelete"] = OutError;
            return RedirectToAction("F3Formgrid", "RegistrationPortal");
        }


        [SessionCheckFilter]
        public ActionResult F3Form(string id, RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }

            ViewBag.CANDID = id;
            string formname = "F3";
            //****** Common 
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();




            if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("F1Formgrid", "RegistrationPortal");
                    }
                    else
                    {

                        #region  Check Subject Table for Class 5th an 8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[0]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[0]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[1]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[1]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[2]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[2]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[3]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[3]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[4]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[4]["MEDIUM"].ToString();
                                    }
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[5]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[5]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }





            #region  Class FifthInitilize List Model
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            //ViewBag.SessionYearList = DBClass.GetSessionYear(); // DB         
            //ViewBag.DAList = DBClass.GetDA();// DB
            //ViewBag.MyDist = DBClass.GetDistE();// DB
            #endregion  Class FifthInitilize List Model
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult F3Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.CANDID = id;

            //
            rm.ClassFifthInitilizeListModel = new ClassFifthInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassFifthInitilizeListModel classFifthInitilizeListModel = DBClass.ClassFifthInitilizeList();
            rm.ClassFifthInitilizeListModel.SessionYearList = classFifthInitilizeListModel.SessionYearList;
            rm.ClassFifthInitilizeListModel.DAList = classFifthInitilizeListModel.DAList;
            rm.ClassFifthInitilizeListModel.MyDist = classFifthInitilizeListModel.MyDist;
            // Pre-Defined Data
            rm.ClassFifthInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassFifthInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassFifthInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassFifthInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassFifthInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassFifthInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassFifthInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassFifthInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;
                string formName = "F3";

                // Start Subject Master
                DataTable dtFifthSubject = new DataTable();
                dtFifthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtFifthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 6; i++)
                {
                    dr = dtFifthSubject.NewRow();
                    dr["CLASS"] = 5;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = "";

                    //if (rm.DA == "N.A.")
                    //{                     
                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    j = i;
                    dtFifthSubject.Rows.Add(dr);
                }

                dtFifthSubject.AcceptChanges();
                dtFifthSubject = dtFifthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master


                if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_F_Data(rm, frm, formName, id, "", "", dtFifthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }



            }

            return View(rm);
        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ImportDataF3Form(int? page)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                if (loginSession.fifth == "N")
                { return RedirectToAction("Index", "Home"); }

                var sessionYear = RegistrationDB.GetImportSessionLast2();
                ViewBag.MySessionYear = sessionYear.ToList();
                ViewBag.SelectedSessionYear = "0";

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" }, new { ID = "3", Name = "AADHAR NO." }, }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                var sessionsrc = new SelectList(new[] { new { ID = "Self", Name = "Self School" }, new { ID = "Other", Name = "Other School" } }, "ID", "Name", 1);
                ViewBag.MySession = sessionsrc.ToList();
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                Import obj = new Import();
                string session = null;
                string schl = null;
                session = loginSession.CurrentSession.ToString();
                schl = loginSession.SCHL.ToString();

                string Search = string.Empty;
                if (TempData["ImportDataF3Formsearch"] != null)
                {
                    Search = Convert.ToString(TempData["ImportDataF3Formsearch"]);
                    ViewBag.SelectedItem = TempData["ImportDataF3FormSelList"];
                    ViewBag.Searchstring = TempData["ImportDataF3FormSearchString"];
                    ViewBag.SelectedSession = TempData["ImportDataF3FormSession"];
                    ViewBag.SelectedSessionYear = TempData["ImportDataF3FormSessionYear"];

                    //---------------Fill Data On page Load-----------------          
                    obj.StoreAllData = RegistrationDB.SelectAllImportF3(schl, pageIndex, ViewBag.SelectedSessionYear, Search);  //SelAllImportDataF3Form_SpN1
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Doesn't Exist";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        TempData["ImportDataF3Formsearch"] = Search;
                        TempData["ImportDataF3FormSelList"] = ViewBag.SelectedItem;
                        TempData["ImportDataF3FormSearchString"] = ViewBag.Searchstring;
                        TempData["ImportDataF3FormSession"] = ViewBag.SelectedSession;
                        TempData["ImportDataF3FormSessionYear"] = ViewBag.SelectedSessionYear;

                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        // ViewBag.TotalCount1 = Convert.ToInt32(obj.TotalCount.Tables[0].Rows[0]["decount"]);
                        ViewBag.TotalCount1 = Convert.ToInt32(obj.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int tp = Convert.ToInt32(obj.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;


                    }
                }
                return View(obj);
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [HttpPost]
        public ActionResult ImportDataF3Form(string id, int? page, Import imp, FormCollection frm, string cmd, string SelList, string SearchString, string Session1, string SessionYear)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                if (loginSession.fifth == "N")
                { return RedirectToAction("Index", "Home"); }
                var sessionYear = RegistrationDB.GetImportSessionLast2();
                ViewBag.MySessionYear = sessionYear.ToList();
                ViewBag.SelectedSessionYear = "0";

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" }, new { ID = "3", Name = "AADHAR NO." }, }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                var sessionsrc = new SelectList(new[] { new { ID = "Self", Name = "Self School" }, new { ID = "Other", Name = "Other School" } }, "ID", "Name", 1);
                ViewBag.MySession = sessionsrc.ToList();

                string result = "";
                string session = loginSession.CurrentSession;
                string schl = loginSession.SCHL;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;




                string Search = "";
                if (cmd == "Search")
                {

                    if (Session1.ToString().ToUpper() == "SELF")
                    {
                        Search = " schl ='" + loginSession.SCHL + "'";
                    }


                    if (SelList != "")
                    {
                        if (Search != "")
                        {
                            Search += " and ";
                        }
                        ViewBag.SelectedItem = SelList;
                        int SelValueSch = Convert.ToInt32(SelList.ToString());
                        if (SelValueSch == 1)
                        {
                            Search += "  ROLL='" + frm["SearchString"].ToString() + "'";
                        }
                        else if (SelValueSch == 2)
                        {
                            Search += "  REGNO like '%" + SearchString.ToString().Trim() + "%'";
                        }
                        else if (SelValueSch == 3)
                        {
                            Search += "  Aadhar_num ='" + frm["SearchString"].ToString() + "'";
                        }
                    }

                    ViewBag.SelectedItem = SelList;
                    TempData["ImportDataF3Formsearch"] = Search;
                    TempData["ImportDataF3FormSelList"] = SelList;
                    TempData["ImportDataF3FormSearchString"] = SearchString.ToString().Trim();
                    TempData["ImportDataF3FormSession"] = Session1;
                    TempData["ImportDataF3FormSessionYear"] = SessionYear;
                    ViewBag.SelectedSession = Session1;
                    ViewBag.Searchstring = SearchString.ToString().Trim();
                    ViewBag.SelectedSessionYear = SessionYear;


                    imp.StoreAllData = RegistrationDB.SelectAllImportF3(schl, pageIndex, SessionYear, Search);//SelectAllImportN3Form8thPassSP


                    if (imp.StoreAllData == null || imp.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Doesn't Exist";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {

                        TempData["ImportDataF3Formsearch"] = Search;
                        TempData["ImportDataF3FormSelList"] = ViewBag.SelectedItem;
                        TempData["ImportDataF3FormSearchString"] = ViewBag.Searchstring;
                        TempData["ImportDataF3FormSession"] = ViewBag.SelectedSession;
                        TempData["ImportDataF3FormSessionYear"] = ViewBag.SelectedSessionYear;

                        ViewBag.TotalCount = imp.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int tp = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        return View(imp);
                    }
                }
                else
                {
                    /////-----------------------Import Begins-------

                    // string importToSchl = frm["ImportSchlTo"];
                    string importToSchl = loginSession.SCHL;

                    string collectId = frm["cbImportSelected"];
                    int cnt = collectId.Count(x => x == ',');
                    TempData["TotImported"] = cnt + 1;
                    string CurrentSchl = loginSession.SCHL.ToString();
                    DataTable dt = new DataTable();
                    collectId = "" + collectId + "";
                    string Sub = String.Empty;

                    if (SelList != "")
                    {
                        if (Session1.ToString().ToUpper() == "SELF")
                        {
                            Search = " schl ='" + loginSession.SCHL + "'";
                        }
                        //Change by Harpal sir  Any student can  be imported either self or other
                        //else { Search = " schl !='" + imp.schoolcode + "'"; }

                        if (SelList != "")
                        {
                            if (Search != "")
                            {
                                Search += " and ";
                            }
                            ViewBag.SelectedItem = SelList;
                            int SelValueSch = Convert.ToInt32(SelList.ToString());
                            if (SelValueSch == 1)
                            {
                                Search += "  ROLL='" + frm["SearchString"].ToString() + "'";
                            }
                            else if (SelValueSch == 2)
                            {
                                Search += "  REGNO like '%" + SearchString.ToString().Trim() + "%'";
                            }
                            else if (SelValueSch == 3)
                            {
                                Search += "  Aadhar_num ='" + frm["SearchString"].ToString() + "'";
                            }
                        }
                    }

                    DataSet dsr = RegistrationDB.ImportF3Form(importToSchl, CurrentSchl, collectId, Session1);//ImportStudent10thFailN1
                    dt = (DataTable)dsr.Tables[0];
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Not Imported";
                        ViewData["result"] = -1;
                        return View();
                    }
                    else
                    {

                        TempData["result"] = 1;
                        ViewData["result"] = 1;
                    }

                    /////
                    if (TempData["ImportDataF3Formsearch"] != null)
                    {
                        Search = Convert.ToString(TempData["ImportDataF3Formsearch"]);
                        ViewBag.SelectedItem = TempData["ImportDataF3FormSelList"];
                        ViewBag.Searchstring = TempData["ImportDataF3FormSearchString"];
                        ViewBag.SelectedSession = TempData["ImportDataF3FormSession"];
                        ViewBag.SelectedSessionYear = TempData["ImportDataF3FormSessionYear"];

                        //---------------Fill Data On page Load-----------------          
                        imp.StoreAllData = RegistrationDB.SelectAllImportF3(schl, pageIndex, SessionYear, Search);
                        if (imp.StoreAllData == null || imp.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Data Doesn't Exist";
                            ViewBag.TotalCount = 0;
                        }
                        else
                        {
                            ViewBag.TotalCount = imp.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount1 = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                            int tp = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                            int pn = tp / 20;
                            int cal = 20 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;


                        }
                    }

                    return View(imp);
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        #endregion  ImportDataF3Form


        #region  ImportDataA3Form

        [SessionCheckFilter]
        public ActionResult A3Formgrid(RegistrationSearchModelList registrationSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }




            DataSet dsOut = new DataSet();
            registrationSearchModel.RegistrationSearchModel = RegistrationDB.GetStudentRecordsSearchPM("A3", loginSession.SCHL, out dsOut);
            registrationSearchModel.StoreAllData = dsOut;
            return View(registrationSearchModel);
        }

        [SessionCheckFilter]
        public ActionResult A3FormView(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            else if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("A3Formgrid", "RegistrationPortal");
            }
            //loginSession = (LoginSession)Session["LoginSession"];

            //

            string formname = "A3";
            if (id != null)
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                string search = "";
                DataSet ds = new DataSet();
                rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                if (rm == null)
                {
                    return RedirectToAction("A3Formgrid", "RegistrationPortal");
                }
                else
                {
                    #region  Check Subject Table for Class 8th
                    if (ds.Tables.Count > 1)
                    {
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                            {

                                if (i == 0)
                                {
                                    rm.subS1 = ds.Tables[1].Rows[0]["SUBNM"].ToString();
                                }
                                else if (i == 1)
                                {
                                    rm.subS2 = ds.Tables[1].Rows[1]["SUBNM"].ToString();
                                }
                                else if (i == 2)
                                {
                                    rm.subS3 = ds.Tables[1].Rows[2]["SUBNM"].ToString();
                                }
                                else if (i == 3)
                                {
                                    rm.subS4 = ds.Tables[1].Rows[3]["SUBNM"].ToString();
                                }
                                else if (i == 4)
                                {
                                    rm.subS5 = ds.Tables[1].Rows[4]["SUBNM"].ToString();
                                }

                                else if (i == 5)
                                {
                                    rm.subS6 = ds.Tables[1].Rows[5]["SUBNM"].ToString();
                                }
                                else if (i == 6)
                                {
                                    rm.subS7 = ds.Tables[1].Rows[6]["SUBNM"].ToString();
                                }
                                else if (i == 7)
                                {
                                    rm.subS8 = ds.Tables[1].Rows[7]["SUBNM"].ToString();
                                }
                                else if (i == 8)
                                {
                                    rm.subS9 = ds.Tables[1].Rows[i]["SUBNM"].ToString();
                                }
                                else if (i == 9)
                                {
                                    rm.s9 = ds.Tables[1].Rows[i]["SUBNM"].ToString();
                                }
                            }

                        }
                    }
                    #endregion

                }


            }
            return View(rm);
        }

        [SessionCheckFilter]
        public ActionResult A3FormDelete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("A3Formgrid", "RegistrationPortal");
            }
            int result = 0;
            string OutError = "";
            result = RegistrationDB.DeleteFromData(id, out OutError);
            ViewData["resultDelete"] = OutError;
            return RedirectToAction("A3Formgrid", "RegistrationPortal");
        }


        [SessionCheckFilter]
        public ActionResult A3Form(string id, RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }

            ViewBag.CANDID = id;

            string formname = "A3";

            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            if (!string.IsNullOrEmpty(id))
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                if (id != null)
                {
                    string search = "";
                    DataSet ds = new DataSet();
                    rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                    if (rm == null)
                    {
                        return RedirectToAction("A3Formgrid", "RegistrationPortal");
                    }
                    else
                    {

                        #region  Check Subject Table for Class  8th
                        if (ds.Tables.Count > 1)
                        {
                            if (ds.Tables[1].Rows.Count > 0)
                            {
                                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                                {

                                    if (i == 0)
                                    {
                                        rm.subS1 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm1 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 1)
                                    {
                                        rm.subS2 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM2 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 2)
                                    {
                                        rm.subS3 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subm3 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 3)
                                    {
                                        rm.subS4 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM4 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 4)
                                    {
                                        rm.subS5 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM5 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    //
                                    else if (i == 5)
                                    {
                                        rm.subS6 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM6 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 6)
                                    {
                                        rm.subS7 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM7 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 7)
                                    {
                                        rm.subS8 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM8 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 8)
                                    {
                                        rm.subS9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.subM9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                    else if (i == 9)
                                    {
                                        rm.s9 = ds.Tables[1].Rows[i]["SUB"].ToString();
                                        rm.m9 = ds.Tables[1].Rows[i]["MEDIUM"].ToString();
                                    }
                                }

                            }
                        }
                        #endregion

                    }
                }
            }



            #region  Class Middle Initilize List Model
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            #endregion
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult A3Form(string id, RegistrationModels rm, FormCollection frm, string cmd)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.CANDID = id;

            //common
            //
            rm.ClassMiddleInitilizeListModel = new ClassMiddleInitilizeListModel();
            rm.LastEntryCandidate = new LastEntryCandidate();
            ClassMiddleInitilizeListModel ClassMiddleInitilizeListModel = DBClass.ClassMiddleInitilizeList();
            rm.ClassMiddleInitilizeListModel.SessionYearList = ClassMiddleInitilizeListModel.SessionYearList;
            rm.ClassMiddleInitilizeListModel.DAList = ClassMiddleInitilizeListModel.DAList;
            rm.ClassMiddleInitilizeListModel.MyDist = ClassMiddleInitilizeListModel.MyDist;
            rm.ClassMiddleInitilizeListModel.ElectiveSubjects = ClassMiddleInitilizeListModel.ElectiveSubjects;
            // Pre-Defined Data
            rm.ClassMiddleInitilizeListModel.MyMedium = DBClass.GetGroupMedium(); // Static
            rm.ClassMiddleInitilizeListModel.MyCaste = DBClass.GetCaste();  // Static
            rm.ClassMiddleInitilizeListModel.MyReligion = DBClass.GetReligion(); // Static
            rm.ClassMiddleInitilizeListModel.MyBoard = DBClass.GetBoardList(); // Static
            rm.ClassMiddleInitilizeListModel.Mon = DBClass.GetMonth(); // Static
            rm.ClassMiddleInitilizeListModel.MyWritter = DBClass.GetWritter();// Static
            rm.ClassMiddleInitilizeListModel.SectionList = DBClass.GetSection();// Static
            rm.ClassMiddleInitilizeListModel.YesNoListText = DBClass.GetYesNoText();// Static
            //ViewBag.ElectiveSubjects = DBClass.GetSub08ElectiveSubjectList();

            //Check server side validation using data annotation
            if (ModelState.IsValid)
            {
                string group = rm.MyGroup;
                string formName = "A3";
                // Start Subject Master
                DataTable dtEighthSubject = new DataTable();
                dtEighthSubject.Columns.Add(new DataColumn("CLASS", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("MEDIUM", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUBCAT", typeof(string)));
                dtEighthSubject.Columns.Add(new DataColumn("SUB_SEQ", typeof(int)));
                DataRow dr = null;
                int j = 0;
                for (int i = 1; i <= 9; i++)
                {
                    dr = dtEighthSubject.NewRow();
                    dr["CLASS"] = 8;
                    DataSet dsSub = new DataSet();

                    rm.subm1 = rm.subM2 = rm.subm3 = rm.subM4 = rm.subM5 = rm.subM6 = rm.subM7 = rm.subM8 = rm.m9 = "";


                    if (i == 1)
                    {
                        dr["SUB"] = rm.subS1; dr["MEDIUM"] = rm.subm1; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 2)
                    {
                        dr["SUB"] = rm.subS2; dr["MEDIUM"] = rm.subM2; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 3)
                    {
                        dr["SUB"] = rm.subS3; dr["MEDIUM"] = rm.subm3; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 4)
                    {
                        dr["SUB"] = rm.subS4; dr["MEDIUM"] = rm.subM4; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 5)
                    {
                        dr["SUB"] = rm.subS5; dr["MEDIUM"] = rm.subM5; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    //
                    else if (i == 6)
                    {
                        dr["SUB"] = rm.subS6; dr["MEDIUM"] = rm.subM6; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 7)
                    {
                        dr["SUB"] = rm.subS7; dr["MEDIUM"] = rm.subM7; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";
                    }
                    else if (i == 8)
                    {
                        dr["SUB"] = rm.subS8; dr["MEDIUM"] = rm.subM8; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "R";

                    }
                    else if (i == 9)
                    {
                        dr["SUB"] = rm.s9; dr["MEDIUM"] = rm.m9; dr["SUB_SEQ"] = i; dr["SUBCAT"] = "A";
                    }
                    j = i;

                    dtEighthSubject.Rows.Add(dr);
                }

                dtEighthSubject.AcceptChanges();
                dtEighthSubject = dtEighthSubject.AsEnumerable().Where(r => (r.ItemArray[1].ToString() != "") && (r.ItemArray[1].ToString() != "0")).CopyToDataTable();
                // End Subject Master
                if (dtEighthSubject.Rows.Count != 10)
                {
                    ViewData["result"] = "SUBCOUNT";
                    return View(rm);
                }

                if (!string.IsNullOrEmpty(id) && cmd.ToLower().Contains("update"))
                {
                    id = encrypt.QueryStringModule.Decrypt(id);
                    string result = RegistrationDB.Update_A_Data(rm, frm, formName, id, "", "", dtEighthSubject);
                    if (result == "0" || string.IsNullOrEmpty(result))
                    {
                        //--------------Not saved
                        ViewData["result"] = 10;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        ViewData["result"] = 11;
                    }
                    else if (result == "-5")
                    {
                        //-----alredy exist
                        ViewData["result"] = result;
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewData["result"] = 20;
                    }
                }



            }

            return View(rm);
        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ImportDataA3Form(int? page)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                if (loginSession.middle == "N")
                { return RedirectToAction("Index", "Home"); }



                var sessionYear = RegistrationDB.GetImportSessionLast2();
                ViewBag.MySessionYear = sessionYear.ToList();
                ViewBag.SelectedSessionYear = "0";


                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" }, new { ID = "3", Name = "AADHAR NO." }, }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                var sessionsrc = new SelectList(new[] { new { ID = "Self", Name = "Self School" }, new { ID = "Other", Name = "Other School" } }, "ID", "Name", 1);
                ViewBag.MySession = sessionsrc.ToList();
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                Import obj = new Import();
                string session = null;
                string schl = null;
                session = loginSession.CurrentSession.ToString();
                schl = loginSession.SCHL.ToString();

                string Search = string.Empty;
                if (TempData["ImportDataA3Formsearch"] != null)
                {
                    Search = Convert.ToString(TempData["ImportDataA3Formsearch"]);
                    ViewBag.SelectedItem = TempData["ImportDataA3FormSelList"];
                    ViewBag.Searchstring = TempData["ImportDataA3FormSearchString"];
                    ViewBag.SelectedSession = TempData["ImportDataA3FormSession"];
                    ViewBag.SelectedSessionYear = TempData["ImportDataA3FormSessionYear"];

                    //---------------Fill Data On page Load-----------------          
                    obj.StoreAllData = RegistrationDB.SelectAllImportA3(schl, pageIndex, ViewBag.SelectedSessionYear, Search);  //SelAllImportDataA3Form_SpN1
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Doesn't Exist";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        TempData["ImportDataA3Formsearch"] = Search;
                        TempData["ImportDataA3FormSelList"] = ViewBag.SelectedItem;
                        TempData["ImportDataA3FormSearchString"] = ViewBag.Searchstring;
                        TempData["ImportDataA3FormSession"] = ViewBag.SelectedSession;
                        TempData["ImportDataA3FormSessionYear"] = ViewBag.SelectedSessionYear;

                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                        // ViewBag.TotalCount1 = Convert.ToInt32(obj.TotalCount.Tables[0].Rows[0]["decount"]);
                        ViewBag.TotalCount1 = Convert.ToInt32(obj.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int tp = Convert.ToInt32(obj.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;


                    }
                }
                return View(obj);
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [SessionCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [HttpPost]
        public ActionResult ImportDataA3Form(string id, int? page, Import imp, FormCollection frm, string cmd, string SelList, string SearchString, string Session1, string SessionYear)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                var sessionYear = RegistrationDB.GetImportSessionLast2();
                ViewBag.MySessionYear = sessionYear.ToList();
                ViewBag.SelectedSessionYear = "0";

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" }, new { ID = "3", Name = "AADHAR NO." }, }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                var sessionsrc = new SelectList(new[] { new { ID = "Self", Name = "Self School" }, new { ID = "Other", Name = "Other School" } }, "ID", "Name", 1);
                ViewBag.MySession = sessionsrc.ToList();

                string result = "";
                string session = loginSession.CurrentSession;
                string schl = loginSession.SCHL;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;




                string Search = "";
                if (cmd == "Search")
                {
                    if (Session1.ToString().ToUpper() == "SELF")
                    {
                        Search = " schl ='" + loginSession.SCHL + "'";
                    }

                    if (SelList != "")
                    {
                        if (Search != "")
                        {
                            Search += " and ";
                        }
                        ViewBag.SelectedItem = SelList;
                        int SelValueSch = Convert.ToInt32(SelList.ToString());
                        if (SelValueSch == 1)
                        {
                            Search += "  ROLL='" + frm["SearchString"].ToString() + "'";
                        }
                        else if (SelValueSch == 2)
                        {
                            Search += "  REGNO like '%" + SearchString.ToString().Trim() + "%'";
                        }
                        else if (SelValueSch == 3)
                        {
                            Search += "  Aadhar_num ='" + frm["SearchString"].ToString() + "'";
                        }
                    }

                    ViewBag.SelectedItem = SelList;
                    TempData["ImportDataA3Formsearch"] = Search;
                    TempData["ImportDataA3FormSelList"] = SelList;
                    TempData["ImportDataA3FormSearchString"] = SearchString.ToString().Trim();
                    TempData["ImportDataA3FormSession"] = Session1;
                    TempData["ImportDataA3FormSessionYear"] = SessionYear;
                    ViewBag.SelectedSession = Session1;
                    ViewBag.Searchstring = SearchString.ToString().Trim();
                    ViewBag.SelectedSessionYear = SessionYear;

                    imp.StoreAllData = RegistrationDB.SelectAllImportA3(schl, pageIndex, SessionYear, Search);//SelectAllImportN3Form8thPassSP


                    if (imp.StoreAllData == null || imp.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Doesn't Exist";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = imp.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int tp = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        return View(imp);
                    }
                }
                else
                {
                    /////-----------------------Import Begins-------

                    //  string importToSchl = frm["ImportSchlTo"];
                    string importToSchl = loginSession.SCHL;

                    string collectId = frm["cbImportSelected"];
                    int cnt = collectId.Count(x => x == ',');
                    TempData["TotImported"] = cnt + 1;
                    string CurrentSchl = loginSession.SCHL.ToString();
                    DataTable dt = new DataTable();
                    collectId = "" + collectId + "";
                    string Sub = String.Empty;

                    if (SelList != "")
                    {
                        if (Session1.ToString().ToUpper() == "SELF")
                        {
                            Search = " schl ='" + loginSession.SCHL + "'";
                        }
                        //Change by Harpal sir  Any student can  be imported either self or other
                        //else { Search = " schl !='" + imp.schoolcode + "'"; }

                        if (SelList != "")
                        {
                            if (Search != "")
                            {
                                Search += " and ";
                            }
                            ViewBag.SelectedItem = SelList;
                            int SelValueSch = Convert.ToInt32(SelList.ToString());
                            if (SelValueSch == 1)
                            {
                                Search += "  ROLL='" + frm["SearchString"].ToString() + "'";
                            }
                            else if (SelValueSch == 2)
                            {
                                Search += "  REGNO like '%" + SearchString.ToString().Trim() + "%'";
                            }
                            else if (SelValueSch == 3)
                            {
                                Search += "  Aadhar_num ='" + frm["SearchString"].ToString() + "'";
                            }
                        }
                    }

                    DataSet dsr = RegistrationDB.ImportA3Form(importToSchl, CurrentSchl, collectId, Session1);//ImportStudent10thFailN1
                    dt = (DataTable)dsr.Tables[0];
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        ViewBag.Message = "Data Not Imported";
                        ViewData["result"] = -1;
                        return View();
                    }
                    else
                    {

                        TempData["result"] = 1;
                        ViewData["result"] = 1;
                    }

                    /////
                    if (TempData["ImportDataA3Formsearch"] != null)
                    {
                        Search = Convert.ToString(TempData["ImportDataA3Formsearch"]);
                        ViewBag.SelectedItem = TempData["ImportDataA3FormSelList"];
                        ViewBag.Searchstring = TempData["ImportDataA3FormSearchString"];
                        ViewBag.SelectedSession = TempData["ImportDataA3FormSession"];
                        ViewBag.SelectedSessionYear = TempData["ImportDataA3FormSessionYear"];

                        //---------------Fill Data On page Load-----------------          
                        imp.StoreAllData = RegistrationDB.SelectAllImportA3(schl, pageIndex, SessionYear, Search);
                        if (imp.StoreAllData == null || imp.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Data Doesn't Exist";
                            ViewBag.TotalCount = 0;
                        }
                        else
                        {

                            TempData["ImportDataA3Formsearch"] = Search;
                            TempData["ImportDataA3FormSelList"] = ViewBag.SelectedItem;
                            TempData["ImportDataA3FormSearchString"] = ViewBag.Searchstring;
                            TempData["ImportDataA3FormSession"] = ViewBag.SelectedSession;
                            TempData["ImportDataA3FormSessionYear"] = ViewBag.SelectedSessionYear;
                            ViewBag.TotalCount = imp.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount1 = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                            int tp = Convert.ToInt32(imp.StoreAllData.Tables[1].Rows[0]["decount"]);
                            int pn = tp / 20;
                            int cal = 20 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;


                        }
                    }

                    return View(imp);
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        #endregion  ImportDataA3Form


        #region Inter Board Migration Schl

        [SessionCheckFilter]
        public ActionResult InterBoardMigrationSchl()
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];


            ViewBag.FormName = Request.QueryString["Form"];
            Session["FormName"] = Request.QueryString["Form"];
            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Request ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="Regno"}}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            RegistrationModels rm = new RegistrationModels();
            string Search = string.Empty;
            string schlid = loginSession.SCHL.ToString();

            Search = "schl='" + schlid + "' and class  in (5,8) and Form  in ('A2','F2') and   isnull(PanelType,'')='IB' ";
            rm.StoreAllData = RegistrationDB.GetLateAdmissionSchl(Search);
            if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
            }
            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult InterBoardMigrationSchl(FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {

                var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Request ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="Regno"}}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();


                RegistrationModels rm = new RegistrationModels();
                if (ModelState.IsValid)
                {
                    string Search = string.Empty;
                    string schlid = loginSession.SCHL.ToString();
                    Search = "schl='" + schlid + "' and class  in (5,8) and Form  in ('A2','F2') and   isnull(PanelType,'')='IB' ";

                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());


                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += "and RequestID like '%' "; }
                            else if (SelValueSch == 2)
                            { Search += " and RequestID='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and Father_Name  like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 5)
                            { Search += " and Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 6)
                            { Search += " and RegNo like '%" + frm["SearchString"].ToString() + "%'"; }
                        }

                    }

                    rm.StoreAllData = RegistrationDB.GetLateAdmissionSchl(Search);
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        return View(rm);
                    }
                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }

        }


        #region Inter Board Migration PayFee 

        [SessionCheckFilter]
        public ActionResult InterBoardMigrationPayFee(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string Schl = loginSession.SCHL.ToString();
            RegistrationModels rm = new RegistrationModels();
            EAffiliationFee _EAffiliationFee = new EAffiliationFee();
            if (!string.IsNullOrEmpty(id))
            {
                DataSet ds = RegistrationDB.GetInterBoardMigrationPayFee(id);
                _EAffiliationFee.PaymentFormData = ds;
                if (_EAffiliationFee.PaymentFormData == null || _EAffiliationFee.PaymentFormData.Tables[0].Rows.Count == 0)
                { ViewBag.TotalCount = 0; Session["InterBoardMigrationPayFee"] = null; }
                else
                {
                    Session["InterBoardMigrationPayFee"] = ds;
                    ViewBag.TotalFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("fee")));
                    ViewBag.TotalLateFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("latefee")));
                    ViewBag.TotalTotfee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("Totfee")));

                    ViewBag.Total = ViewBag.TotalFee + ViewBag.TotalLateFee;
                    ViewData["result"] = 10;
                    ViewData["FeeStatus"] = "1";
                    ViewBag.TotalCount = 1;
                    return View(_EAffiliationFee);
                }
            }
            else
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }


            return View(_EAffiliationFee);
        }



        [HttpPost]
        [SessionCheckFilter]
        public ActionResult InterBoardMigrationPayFee(string id, FormCollection frm, string PayModValue, string AllowBanks)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string Schl = loginSession.SCHL.ToString();
            try
            {

                EAffiliationFee pfvm = new EAffiliationFee();
                ChallanMasterModel CM = new ChallanMasterModel();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
                }
                if (Session["InterBoardMigrationPayFee"] == null)
                {
                    return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
                }
                string appno = id;
                DataSet ds = (DataSet)Session["InterBoardMigrationPayFee"];
                pfvm.PaymentFormData = ds;

                string bankName = "";

                if (AllowBanks == "301" || AllowBanks == "302")
                {
                    PayModValue = "online";
                    CM.FEEMODE = "ONLINE";
                    if (AllowBanks == "301")
                    {
                        bankName = "HDFC Bank";
                    }
                    else if (AllowBanks == "302")
                    {
                        bankName = "Punjab And Sind Bank";
                    }
                }
                else if (AllowBanks == "203")
                {
                    CM.FEEMODE = "CASH";
                    PayModValue = "hod";
                    bankName = "PSEB HOD";
                }
                else if (AllowBanks == "202" || AllowBanks == "204")
                {
                    CM.FEEMODE = "CASH";
                    PayModValue = "offline";
                    if (AllowBanks == "202")
                    {
                        bankName = "Punjab National Bank";
                    }
                    else if (AllowBanks == "204")
                    {
                        bankName = "State Bank of India";
                    }
                }



                ViewBag.TotalFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("fee")));
                ViewBag.TotalLateFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("latefee")));
                ViewBag.TotalTotfee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("Totfee")));
                ViewBag.Total = ViewBag.TotalFee + ViewBag.TotalLateFee;

                string stdid = ds.Tables[0].Rows[0]["RequestID"].ToString();

                if (string.IsNullOrEmpty(AllowBanks))
                {
                    ViewBag.Message = "Please Select Bank";
                    ViewData["SelectBank"] = "1";
                    return View(pfvm);
                }


                if (ModelState.IsValid)
                {
                    CM.FEE = Convert.ToInt32(ViewBag.TotalFee);
                    CM.latefee = Convert.ToInt32(ViewBag.TotalLateFee);
                    CM.TOTFEE = Convert.ToInt32(ViewBag.TotalTotfee);
                    string TotfeePG = (CM.TOTFEE).ToString();
                    CM.FEECAT = ds.Tables[0].Rows[0]["FeeCat"].ToString();
                    CM.FEECODE = ds.Tables[0].Rows[0]["FeeCode"].ToString();
                    CM.BCODE = AllowBanks;
                    CM.BANK = bankName;
                    CM.BANKCHRG = 0;
                    CM.SchoolCode = Schl;
                    CM.DIST = "";
                    CM.DISTNM = "";
                    CM.LOT = 1;
                    CM.SCHLREGID = Schl;
                    CM.FeeStudentList = stdid;
                    CM.APPNO = stdid;
                    CM.type = "schle";
                    CM.CHLNVDATE = Convert.ToString(ds.Tables[0].Rows[0]["BankLastdate"].ToString());
                    DateTime BankLastDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["BankLastdate"].ToString());
                    DateTime CHLNVDATE2;
                    if (DateTime.TryParseExact(BankLastDate.ToString("dd/MM/yyyy"), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out CHLNVDATE2))
                    {
                        CM.ChallanVDateN = CHLNVDATE2;
                    }
                    else
                    {
                        CM.ChallanVDateN = BankLastDate;
                    }

                    string SchoolMobile = "";
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

                        ViewData["SelectBank"] = null;
                        ViewData["result"] = 1;
                        ViewBag.ChallanNo = result;
                        string paymenttype = CM.BCODE;
                        string bnkLastDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["BankLastdate"].ToString()).ToString("dd/MM/yyyy");
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
                                string strURL;
                                string MerchantLogin = ConfigurationManager.AppSettings["ATOMLoginId"].ToString();
                                string MerchantPass = ConfigurationManager.AppSettings["ATOMPassword"].ToString();
                                string MerchantDiscretionaryData = "NB";  // for netbank                               
                                string ClientCode = CM.APPNO;
                                string ProductID = ConfigurationManager.AppSettings["ATOMProductID"].ToString();
                                string CustomerAccountNo = "0123456789";
                                string TransactionType = "NBFundTransfer";  // for netbank
                                                                            //string TransactionAmount = "1";
                                string TransactionAmount = TotfeePG;
                                string TransactionCurrency = "INR";
                                string TransactionServiceCharge = "0";
                                string TransactionID = ViewBag.ChallanNo;// Unique Challan Number
                                string TransactionDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                                // string TransactionDateTime = "18/10/2020 13:15:19";
                                string BankID = "ATOM";

                                string ru = ConfigurationManager.AppSettings["ATOMRU"].ToString();
                                // User Details
                                string udf1CustName = CM.SCHLREGID; // roll number

                                string udf2CustEmail = CM.FEECAT; /// Kindly submit Appno/Refno in client id, Fee cat in Emailid (ATOM)
                                string udf3CustMob = SchoolMobile;

                                strURL = GatewayController.ATOMTransferFund(MerchantLogin, MerchantPass, MerchantDiscretionaryData, ProductID, ClientCode, CustomerAccountNo, TransactionType,
                                  TransactionAmount, TransactionCurrency, TransactionServiceCharge, TransactionID, TransactionDateTime, BankID, ru, udf1CustName, udf2CustEmail, udf3CustMob);

                                if (!string.IsNullOrEmpty(strURL))
                                {
                                    return View("../Gateway/AtomCheckoutUrl", new AtomViewModel(strURL));
                                }
                                else
                                {
                                    ViewData["result"] = -10;
                                    return View(pfvm);
                                }
                            }
                            #endregion Payment Gateyway
                        }
                        else if (result.Length > 5)
                        {
                            return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });

                        }
                    }
                }
                return View(pfvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
            }
        }



        public ActionResult InterBoardMigrationLetter(string CID)
        {
            RegistrationModels rm = new RegistrationModels();
            string result = "";
            if (CID != "" || CID != null)
            {
                rm.StoreAllData = RegistrationDB.LateAdmPrintLetter(CID);
                if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                }
                return View(rm);
            }
            else
            {
                return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
            }
        }

        #endregion Inter Board Migration PayFee 


        #endregion Inter Board Migration Schl

        #region Late Admission Schl

        [SessionCheckFilter]
        public ActionResult LateAdmissionSchl()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            ViewBag.FormName = Request.QueryString["Form"];
            Session["FormName"] = Request.QueryString["Form"];
            var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Request ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="Regno"}}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            RegistrationModels rm = new RegistrationModels();
            string Search = string.Empty;
            string schlid = loginSession.SCHL.ToString();


            Search = "schl='" + schlid + "' and class in (5,8) ";//show all
            rm.StoreAllData = RegistrationDB.GetLateAdmissionSchl(Search);
            if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
            }

            ViewBag.LastDate = rm.StoreAllData.Tables[1].Rows[0]["eDate"].ToString();



            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult LateAdmissionSchl(FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {

                var itemsch = new SelectList(new[]{new {ID="1",Name="ALL"},new{ID="2",Name="Request ID"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="Regno"}}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                RegistrationModels rm = new RegistrationModels();
                if (ModelState.IsValid)
                {

                    string Search = string.Empty;
                    string schlid = loginSession.SCHL;

                    Search = "schl='" + schlid + "' and class in (5,8)  ";

                    if (frm["SelList"] != "")
                    {
                        ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());


                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += "and RequestID like '%' "; }
                            else if (SelValueSch == 2)
                            { Search += " and RequestID='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and Father_Name  like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 5)
                            { Search += " and Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 6)
                            { Search += " and RegNo like '%" + frm["SearchString"].ToString() + "%'"; }
                        }

                    }

                    rm.StoreAllData = RegistrationDB.GetLateAdmissionSchl(Search);
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        return View(rm);
                    }

                    ViewBag.LastDate = rm.StoreAllData.Tables[1].Rows[0]["eDate"].ToString();
                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult AddLateAdmissionSchl(RegistrationModels rm, FormCollection frm, string PreNSQF, string NSQFsubS6, string NsqfPattern, string PanelType)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string fileNM = "";
            try
            {
                string RID = "";
                string cls = frm["class"];
                string formNM = frm["formNM"];
                string regno = frm["regno"];
                string name = frm["name"];
                string fname = frm["fname"];
                string mname = frm["mname"];
                string dob = frm["dob"];
                string mobileno = frm["mobileno"];
                string OBoard = "P.S.E.B BOARD";
                if (formNM == "N2" || formNM == "M2" || formNM == "E2" || formNM == "T2" || formNM == "F2" || formNM == "A2")
                {
                    OBoard = frm["Board"];
                }

                string usertype = "Schl";
                if (rm.file != null)
                {
                    fileNM = Path.GetFileName(rm.file.FileName);
                }

                string refno = RegistrationDB.SetLateAdmissionSchl(loginSession.SCHL, loginSession.SCHL, PanelType, loginSession.SCHL, RID, cls, formNM, regno, name, fname, mname, dob, mobileno, fileNM, usertype, OBoard);

                if (rm.file != null)
                {
                    fileNM = refno + Path.GetExtension(rm.file.FileName);
                    ////var path = Path.Combine(Server.MapPath("~/Upload/"+ formName + "/" + dist + "/Photo"), stdPic);
                    //var path = Path.Combine(Server.MapPath("~/Upload/upload2022/LateAdmission/" + fileNM));
                    //string FilepathExist = Path.Combine(Server.MapPath("~/Upload/upload2022/LateAdmission/"));
                    //if (!Directory.Exists(FilepathExist))
                    //{
                    //    Directory.CreateDirectory(FilepathExist);
                    //}
                    //rm.file.SaveAs(path);

                    using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                    {
                        using (var newMemoryStream = new MemoryStream())
                        {
                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = rm.file.InputStream,
                                Key = string.Format("allfiles/Upload2024/LateAdmission/{0}", fileNM),
                                BucketName = BUCKET_NAME,
                                CannedACL = S3CannedACL.PublicRead
                            };

                            var fileTransferUtility = new TransferUtility(client);
                            fileTransferUtility.Upload(uploadRequest);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(refno) && refno.Length > 5)
                {
                    TempData["resultUpdate"] = "11";
                    TempData["RequestId"] = refno;

                    if (!string.IsNullOrEmpty(mobileno))
                    {
                        //     //Dear Student, Your request for late admission permission is {#var#} submitted to PSEB with Req ID : {#var#}. Thanks PSEB 
                        string Sms = "Dear Student, Your request for late admission permission is successfully submitted to PSEB with Req ID : " + refno + ". Thanks PSEB";
                        string getSms = DBClass.gosms(mobileno, Sms);
                    }
                }

                if (string.IsNullOrEmpty(PanelType))
                {
                    PanelType = "LA";
                }
                if (PanelType == "IB")
                {
                    return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
                }
                return RedirectToAction("LateAdmissionSchl", "RegistrationPortal");

            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ModifyLateAdmissionSchl(RegistrationModels rm, FormCollection frm, string cmd, string RequestID, string PanelType)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string fileNM = "";
            try
            {
                //var did = frm.AllKeys[8].ToString();
                string RID = frm["RID"];
                string cls = frm["class"];
                string formNM = frm["formNM"];
                string regno = frm["regno"];
                string name = frm["name"];
                string fname = frm["fname"];
                string mname = frm["mname"];
                string dob = frm["dob"];
                string mobileno = frm["mobileno"];
                string usertype = "Schl";
                //rm.file = frm["file"];
                string OBoard = "P.S.E.B BOARD";
                if (formNM == "N2" || formNM == "M2" || formNM == "E2" || formNM == "T2" || formNM == "F2" || formNM == "A2")
                {
                    OBoard = frm["Board"];
                }

                if (rm.fileM != null)
                {
                    fileNM = Path.GetFileName(rm.fileM.FileName);
                }
                if (RID != "" && cls != "0" && formNM != "0" && name != "" && fname != "" && mname != "" && dob != "")
                {

                    string refno = RegistrationDB.SetLateAdmissionSchl(loginSession.SCHL, loginSession.SCHL, PanelType, loginSession.SCHL, RID, cls, formNM, regno, name, fname, mname, dob, mobileno, fileNM, usertype, OBoard);

                    if (rm.fileM != null)
                    {
                        fileNM = refno + Path.GetExtension(rm.fileM.FileName);
                        ////var path = Path.Combine(Server.MapPath("~/Upload/"+ formName + "/" + dist + "/Photo"), stdPic);
                        //var path = Path.Combine(Server.MapPath("~/Upload/upload2022/LateAdmission/" + fileNM));
                        //string FilepathExist = Path.Combine(Server.MapPath("~/Upload/upload2022/LateAdmission/"));
                        //if (!Directory.Exists(FilepathExist))
                        //{
                        //    Directory.CreateDirectory(FilepathExist);
                        //}
                        //rm.fileM.SaveAs(path);

                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = rm.fileM.InputStream,
                                    Key = string.Format("allfiles/Upload2024/LateAdmission/{0}", fileNM),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }
                    }
                    TempData["resultUpdate"] = "12";
                }
                if (PanelType == "IB")
                {
                    return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");
                }
                return RedirectToAction("LateAdmissionSchl", "RegistrationPortal");

            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public JsonResult SetLateAdmissionSchl(string RID, string cls, string formNM, string regno, string name, string fname,
            string mname, string dob, string mobileno, HttpFileCollectionBase file)
        {
            RegistrationDB objDB = new RegistrationDB();
            RegistrationModels rm = new RegistrationModels();
            FormCollection frc = new FormCollection();
            ViewBag.updStatus = "";
            return Json(ViewBag.updStatus);
        }
        public ActionResult FinalSubmit(string CID, string mobileNo, string formNM, string PanelType)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string result = "";
            if (CID != "" || CID != null)
            {
                result = RegistrationDB.FinalSubmitLateAdmissionSchl(CID);
                ViewData["resultUpdate"] = result;
                TempData["resultUpdate"] = result;
                if (mobileNo != null && mobileNo != "" && result != "")
                {
                    string Sms = "";
                    if (PanelType == "IB")
                    {
                        Sms = "Your Inter Board Migration Form Request No. " + CID + " has been final submitted, Save it till process done.";

                    }
                    else { Sms = "Your late admission form Request No. " + CID + " has been final submitted, Save it till process done."; }
                    string getSms = DBClass.gosms(mobileNo, Sms);
                    // string getSms = objCommon.gosms("9711021501", Sms);
                }
            }
            else
            {
                ViewData["resultUpdate"] = result;
                TempData["resultUpdate"] = result;

            }

            if (PanelType == "IB")
            {
                return RedirectToAction("InterBoardMigrationSchl", "RegistrationPortal");

            }
            return RedirectToAction("LateAdmissionSchl", "RegistrationPortal");
        }
        public ActionResult LateAdmPrintLetter(string CID)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            RegistrationModels rm = new RegistrationModels();
            string result = "";
            if (CID != "" || CID != null)
            {
                rm.StoreAllData = RegistrationDB.LateAdmPrintLetter(CID);
                if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                }
                return View(rm);
            }
            else
            {
                return RedirectToAction("LateAdmissionSchl", "RegistrationPortal");
            }
        }

        public JsonResult GetLateAdmRIDVerify(int RID)
        {
            DataSet rsch = RegistrationDB.GetLateAdmRIDVerify(RID);
            if (rsch.Tables[0].Rows.Count > 0)
            {
                ViewBag.LateAdmDate = rsch.Tables[0].Rows[0]["ApprovalUPTO"].ToString();
            }
            else
            {
                ViewBag.LateAdmDate = null;
            }
            return Json(ViewBag.LateAdmDate);
        }

        public JsonResult GetLateAdm()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string admdate = "", LateAdmDate = "", StartAdmDate = "";
            RegistrationDB.CheckReg_AdmDate_and_LateAdmDate(loginSession.SCHL, "", out admdate, out LateAdmDate, out StartAdmDate);
            ViewBag.admdate = admdate; ViewBag.LateAdmDate = LateAdmDate; ViewBag.StartAdmDate = StartAdmDate;
            return Json(ViewBag.admdate);
        }

        [HttpPost]
        public JsonResult GetLateAdmRIDDataVerify(int RID, string CNM, string FNM, string MNM, string DOB)
        {
            DataSet rsch = RegistrationDB.GetLateAdmRIDDataVerify(RID, CNM, FNM, MNM, DOB);
            if (rsch.Tables[0].Rows.Count > 0)
            {
                ViewBag.updStatus = rsch.Tables[0].Rows[0]["result"].ToString();
            }
            else
            {
                ViewBag.updStatus = null;
            }
            return Json(new { sn = ViewBag.updStatus }, JsonRequestBehavior.AllowGet);
        }
        #endregion Late Admission Schl


        #region check regno 


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [SessionCheckFilter]
        public ActionResult CheckRegularRegno(Import obj)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" },
                new { ID = "3", Name = "TC Ref No" }, new { ID = "4", Name = "Aadhar Number" }, new { ID = "5", Name = "E-Punjab Id" }}, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();
            return View(obj);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CheckRegularRegno(Import obj, FormCollection frm, string cmd, string SelList, string SearchString, string Session1)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Roll Number" }, new { ID = "2", Name = "Registration Number" },
                new { ID = "3", Name = "TC Ref No" }, new { ID = "4", Name = "Aadhar Number" }, new { ID = "5", Name = "E-Punjab Id" }}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                string Search = string.Empty;
                if (cmd == "Search")
                {

                    if (!string.IsNullOrEmpty(SelList))
                    {
                        ViewBag.SelectedItem = SelList;
                        TempData["SelectedItem"] = SelList;
                        int SelValueSch = Convert.ToInt32(SelList.ToString());
                        if (SearchString != "")
                        {
                            ViewBag.Searchstring = SearchString;
                            if (SelValueSch == 1)
                            { Search += " a.Board_Roll_Num='" + SearchString.ToString().Trim() + "'"; }
                            else if (SelValueSch == 2)
                            { Search += "  a.Registration_num ='" + SearchString.ToString().Trim() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " a.tcrefno ='" + SearchString.ToString().Trim() + "'"; }
                            else if (SelValueSch == 4)
                            { Search += " a.Aadhar_num='" + SearchString.ToString().Trim() + "'"; }
                            else if (SelValueSch == 5)
                            { Search += " a.E_punjab_Std_id='" + SearchString.ToString().Trim() + "'"; }
                        }
                    }



                    obj.StoreAllData = RegistrationDB.CheckRegularRegno(Search);//GetImportData2017
                    if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {

                        ViewBag.TotalCount = obj.StoreAllData.Tables[0].Rows.Count;
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return View(obj);
        }

        #endregion

        [SessionCheckFilter]
        public ActionResult ReExamStudents(Printlist obj, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();

                #region Circular

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                string Search = string.Empty;
                Search = "Id like '%' and CircularTypes like '%7%' and Convert(Datetime,Convert(date,ExpiryDateDD))>=Convert(Datetime,Convert(date,getdate()))";


                DataSet dsCircular = _adminRepository.CircularMaster(Search, pageIndex);//GetAllFeeMaster2016SP
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
            }
            catch (Exception ex)
            {

                //return RedirectToAction("Logout", "Login");
                //return View();
            }
            return View(obj);
        }



        [SessionCheckFilter]
        public ActionResult OnDemandCertificate(Printlist obj, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {
                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();

                #region Circular

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                string Search = string.Empty;
                Search = "Id like '%' and CircularTypes like '%7%' and Convert(Datetime,Convert(date,ExpiryDateDD))>=Convert(Datetime,Convert(date,getdate()))";
                DataSet dsCircular = new AdminDB().CircularMaster(Search, pageIndex);//GetAllFeeMaster2016SP
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
            }
            catch (Exception ex)
            {
            }
            return View(obj);
        }

    }
}