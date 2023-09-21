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
using System.Threading.Tasks;
using Newtonsoft.Json;
using PsebPrimaryMiddle.Repository;

namespace PsebPrimaryMiddle.Controllers
{
    [AdminMenuFilter]
    public class CorrectionPerformaController : Controller
    {
        private readonly ICorrectionPerformaRepository _correctionPerformaRepository;       

        public CorrectionPerformaController(ICorrectionPerformaRepository correctionPerformaRepository)
        {
            _correctionPerformaRepository = correctionPerformaRepository;           
        }


        // GET: CorrectionPerforma
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Instructions()
        {
            return View();
        }

        public JsonResult GetCorrPunjabiName(string s, string f) // Calling on http post (on Submit)
        {   
            DataSet result =_correctionPerformaRepository.GetCorrPunjabiName(s); // passing Value to DBClass from model            
            List<SelectListItem> OList = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in result.Tables[0].Rows)
            {
                if (f == "Candi_Name_P")
                {
                    OList.Add(new SelectListItem { Text = @dr["Candi_Name_P"].ToString() });
                }
                if (f == "Father_Name_P")
                {
                    OList.Add(new SelectListItem { Text = @dr["Father_Name_P"].ToString() });
                }
                if (f == "Mother_Name_P")
                {
                    OList.Add(new SelectListItem { Text = @dr["Mother_Name_P"].ToString() });
                }
            }
            return Json(OList);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public JsonResult GetCorrectionLot(string Prefix)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            List<AdminModels> listResult = new List<AdminModels>();
            string Search = string.Empty;
            string CrType = "2";
            Search = "a.status is not null";
            Search += " and a.FirmUser  = '" + adminLoginSession.USER.ToString() + "'";
            if (adminLoginSession.Dist_Allow.ToString() != "")
            {
                Search += " and s.DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
            }
            DataSet ds =_correctionPerformaRepository.GetCorrectionDataFirmUpdated(Search, CrType, 1);
            // DataSet ds = objDB.SelectSchoolDatabyID(schoolcode);    //SelectSchoolDatabyID 
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[3].Rows.Count > 0)
                {
                    listResult = ds.Tables[3].AsEnumerable().Select(row => new AdminModels
                    {
                        CorrectionLot = String.IsNullOrEmpty(row.Field<string>("CorrectionLot")) ? "not found" : row.Field<string>("CorrectionLot"),
                    }).ToList();
                }
            }
            var _list = (from N in listResult
                         where N.CorrectionLot.ToLower().StartsWith(Prefix.ToLower())
                         select new { N.CorrectionLot });
            return Json(_list, JsonRequestBehavior.AllowGet);
        }



        #region SCHOOL CORRECTION PERFORMA BEGIN 

        [SessionCheckFilter]
        public ActionResult SchoolCorrection()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            DateTime dtTodate = Convert.ToDateTime(DateTime.Today);
            try
            {
                RegistrationModels rm = new RegistrationModels();
                rm.CorrectionPerformaModel = new CorrectionPerformaModel();
                //rm.CorrectionPerformaModel.Correctiondata = new DataSet();
                DataSet result = rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(1,loginSession.SCHL, "P"); // passing Value to DBClass from model
                if (rm.CorrectionPerformaModel.Correctiondata == null || rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = ViewBag.TotalCountadded == 0;
                }
                else
                {
                    ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                }
                
                if (result.Tables[1].Rows.Count > 0)
                {                   
                    DateTime sDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["sDate"]);
                    DateTime eDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["eDate"]);

                    DateTime sDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["sDate"]);
                    DateTime eDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["eDate"]);

                    List<SelectListItem> itemsch = new List<SelectListItem>();
                    if (loginSession.fifth.ToUpper() == "Y" && dtTodate <= eDateP)
                    {
                        itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                    }
                    if (loginSession.middle.ToUpper() == "Y" && dtTodate <= eDateM)
                    {
                        itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                    }
                    rm.CorrectionPerformaModel.CorrectionClassList =  itemsch.ToList();
                }
                return View(rm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SchoolCorrection(FormCollection frm, RegistrationModels rm, int? page, string cmd)
        {
           
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            DateTime dtTodate = Convert.ToDateTime(DateTime.Today);
            try
            {
                

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;               
                

                if (ModelState.IsValid)
                {
                    string Search = "a.schl='" + loginSession.SCHL + "' and ";
                    string Std_id = frm["SearchString"];
                    switch (rm.CorrectionPerformaModel.Class)
                    {
                        case "5":
                            Search = "a.form_Name in ('F1','F2')  and a.std_id='" + Std_id + "'";
                            break;
                        case "8":
                            Search = "a.form_Name in ('A1','A2')  and a.std_id='" + Std_id + "'";
                            break;
                    }
                    ViewBag.Searchstring = frm["SearchString"];
                    int SelValueSch = 0;
                    if (rm.CorrectionPerformaModel.Class.ToString() == "")
                    {
                        SelValueSch = 1;
                        Search = "form_Name in ('')";
                    }
                    else
                    {
                        SelValueSch = Convert.ToInt32(rm.CorrectionPerformaModel.Class.ToString());
                    }
                    #region Add Record Region Begin
                    if (cmd == "Add Record")
                    {
                        rm.Std_id = Convert.ToInt32(frm["SearchString"]);
                        rm.Class = rm.CorrectionPerformaModel.Class;
                        rm.CorrectionPerformaModel.Correctiontype = frm["SelListField"];
                       // rm.CorrectionPerformaModel.oldVal = frm["oldVal"];
                        if (rm.CorrectionPerformaModel.Correctiontype == "Candi_Name_P" || rm.CorrectionPerformaModel.Correctiontype == "Father_Name_P" || rm.CorrectionPerformaModel.Correctiontype == "Mother_Name_P")
                        {
                            //rm.CorrectionPerformaModel.newVal = frm["newValP"];
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newValP;
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Caste")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValCaste"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "DOB")
                        {
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newValDOB;
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Gender")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValGender"];
                        }                       
                        else if (rm.CorrectionPerformaModel.Correctiontype == "CandStudyMedium")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValcorCSM"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Differently_Abled")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Differently_Abled"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "wantwriter")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["wantwriter"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Religion")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Relist"];
                        }  
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Admission_Date")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Admission_Date"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Belongs_BPL")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Belongs_BPL"].ToUpper();
                        }                       
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Tehsil")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["SelAllTehsil"].ToUpper();
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "District")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["SelAllDistrict"].ToUpper();
                        }
                        else
                        {
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newVal;
                        }

                        rm.CorrectionPerformaModel.Remark = frm["Remark"];
                        ViewBag.SelectedItem = rm.CorrectionPerformaModel.Class;
                        ViewBag.Searchstring = frm["SearchString"];
                        rm.SCHL = loginSession.SCHL.ToString();


                        rm.UserId = loginSession.SCHL;
                        rm.UserType = "schl";
                        DataSet result1 =_correctionPerformaRepository.InsertSchoolCorrectionAddJunior(rm);
                        if (result1 != null)
                        {
                            ViewData["Status"] = result1.Tables[0].Rows[0]["res"].ToString();
                            TempData["msg"] = result1.Tables[0].Rows[0]["msg"].ToString();
                        }
                        else
                        {
                            ViewData["Status"] = "-1";
                        }

                    }
                    #endregion Add Record


                    rm.StoreAllData =_correctionPerformaRepository.GetCorrectionStudentRecordsSearchJunior(Search, SelValueSch, pageIndex);
                    rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(1, loginSession.SCHL, "P"); // passing Value to DBClass from model
                    ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;

                    if (rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows.Count > 0)
                    {
                        DateTime sDateP = Convert.ToDateTime(rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows[0]["sDate"]);
                        DateTime eDateP = Convert.ToDateTime(rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows[0]["eDate"]);

                        DateTime sDateM = Convert.ToDateTime(rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows[1]["sDate"]);
                        DateTime eDateM = Convert.ToDateTime(rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows[1]["eDate"]);

                        List<SelectListItem> itemsch = new List<SelectListItem>();
                        if (loginSession.fifth.ToUpper() == "Y" && dtTodate <= eDateP)
                        {
                            itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                        }
                        if (loginSession.middle.ToUpper() == "Y" && dtTodate <= eDateM)
                        {
                            itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                        }
                        rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();
                    }

                    #region Pending Record Region Begin
                    if (cmd == "View All Correction Pending Record")
                    {
                        rm.CorrectionPerformaModel.Correctiondata = null;
                        rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(1,loginSession.SCHL, "P");
                        ViewBag.TotalViewAllCount = ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                    }
                    #endregion Pending Record Region Begin
                    #region View All Correction Record
                    else if (cmd == "View All Correction Record")
                    {                       
                        rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(0, loginSession.SCHL, "P");
                        ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                    }

                   else  if (rm.CorrectionPerformaModel.Correctiondata == null || rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message2 = "Record Not Found";
                        ViewBag.TotalCount2 = 0;
                    }
                    #endregion View All Correction Record


                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;

                        return View(rm);
                    }
                    else
                    {

                        ViewBag.TotalCountSearch = rm.StoreAllData.Tables[0].Rows.Count;
                        string std_Class = rm.StoreAllData.Tables[0].Rows[0]["form_Name"].ToString();
                        
                        // Bind MAsters
                        ViewBag.MyAllTehsil = DBClass.GetAllTehsil();
                        ViewBag.MyAllDistrict = DBClass.GetDistE();


                        DataSet CorrectionField =_correctionPerformaRepository.getCorrrectionField(std_Class);
                        ViewBag.MySchField = CorrectionField.Tables[0];
                        List<SelectListItem> items = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in ViewBag.MySchField.Rows)
                        {
                            items.Add(new SelectListItem { Text = @dr["CorrectionFieldDisplayName"].ToString(), Value = @dr["CorrectionFieldName"].ToString() });
                        }
                        ViewBag.MySchField = items.ToList();



                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        int tp = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        int pn = tp / 30;
                        int cal = 30 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        return View(rm);
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }


      
        public ActionResult AiddedCorrectionRecordDelete(string id)
        {            
            RegistrationModels rm = new RegistrationModels();
            string IsUserAdminSchl = string.Empty;  
            if (Session["LoginSession"] == null && Session["AdminLoginSession"] == null)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
            else if (Session["LoginSession"] != null)
            {

                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                IsUserAdminSchl = "schl";                
            }
            else if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                IsUserAdminSchl = "admin";              
            }

            try
            {
                if (id == null)
                {
                    if(IsUserAdminSchl.ToLower() == "schl") { return RedirectToAction("SchoolCorrection", "CorrectionPerforma"); }
                    else { return RedirectToAction("AdminSchoolCorrection", "CorrectionPerforma"); }
                }
                else
                {
                    string result =_correctionPerformaRepository.AiddedCorrectionRecordDelete(id); // passing Value to DBClass from model
                    if (result == "1")
                    {
                        ViewData["DeleteStatus"] = result;                       
                    }
                    else
                    {
                        ViewData["DeleteStatus"] = "";                     
                    }
                }            
            }
            catch (Exception ex)
            {
                ViewData["DeleteStatus"] = "ERROR : " + ex.Message;
            }

            if (IsUserAdminSchl.ToLower() == "schl") { return RedirectToAction("SchoolCorrection", "CorrectionPerforma"); }
            else { return RedirectToAction("AdminSchoolCorrection", "CorrectionPerforma"); }
        }


        [SessionCheckFilter]
        public ActionResult FinalSubmitCorrection(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {
                if (id != null)
                {
                    rm.SCHL = loginSession.SCHL.ToString();
                    rm.CorrectionPerformaModel.Correctiontype = id.ToString();
                   // rm.CorrectionPerformaModel.Correctiontype = "02";
                   string resultFS =_correctionPerformaRepository.FinalSubmitCorrectionJunior(rm); // passing Value to DBClass from model
                    if (Convert.ToInt32(resultFS) > 0)
                    {
                        ViewData["resultFS"] = resultFS;
                        return RedirectToAction("SchoolCorrectionFinalPrintList", "CorrectionPerforma");
                    }
                    else
                    {
                        ViewData["resultFS"] = "";
                        return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                    }
                }
                else
                {
                    return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }

        [SessionCheckFilter]
        public ActionResult SchoolCorrectionFinalPrintList()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                RegistrationModels rm = new RegistrationModels();
                rm.CorrectionPerformaModel = new CorrectionPerformaModel();
                rm.SCHL = loginSession.SCHL.ToString();
                rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetCorrectiondataFinalPrintList(rm);
             
                //ViewBag.TotalCountaddedStream = rm.CorrectionPerformaModel.Correctiondata.Tables[2].Rows.Count;
                //ViewBag.TotalCountaddedPhotoSign = rm.CorrectionPerformaModel.Correctiondata.Tables[3].Rows.Count;
                if (rm.CorrectionPerformaModel.Correctiondata == null && rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0 
                    && rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                    ViewBag.TotalCountaddedSub = rm.CorrectionPerformaModel.Correctiondata.Tables[1].Rows.Count;

                }
                return View(rm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }


        [SessionCheckFilter]
        public ActionResult SchoolCorrectionPerformaFinalPrintSession(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];          
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {
                rm.CorrectionPerformaModel.CorrectionLot = id;
                rm.SCHL = loginSession.SCHL;
                rm.CorrectionPerformaModel.Correctiondatafinal =_correctionPerformaRepository.SchoolCorrectionPerformaFinalPrintSession(rm); // passing Value to DBClass from model
                if (rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows.Count > 0)
                {
                   // ViewData["resultFS"] = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows.Count;
                    ViewBag.TotalCountaddedRecord = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows.Count;
                    rm.SCHL = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["SCHL"].ToString();
                    rm.DIST = Convert.ToString(rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["DIST"]);
                    rm.CorrectionPerformaModel.schlCorNameE = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["SchlFullNME"].ToString();
                    rm.CorrectionPerformaModel.schlCorConDetails = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["schlCorConDetails"].ToString();
                    rm.Class = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["Class"].ToString();
                    rm.CorrectionPerformaModel.CorrectionLot = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["CorrectionLot"].ToString();
                    rm.CorrectionPerformaModel.Correctiontype = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["CorrectionFieldDisplayName"].ToString();
                    rm.CorrectionPerformaModel.Remark = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["Remark"].ToString();
                    rm.CorrectionPerformaModel.CorrectionInsertDt = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["InsertDt"].ToString();
                    rm.CorrectionPerformaModel.CorrectionFinalSubmitDt = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["CorrectionFinalSubmitDt1"].ToString();
                    rm.CorrectionPerformaModel.CorrectionVerifyDt = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["VerifyDt"].ToString();
                    ViewBag.OldSubjlist = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["OlDsubjectList"].ToString();
                    ViewBag.NewSubjlist = rm.CorrectionPerformaModel.Correctiondatafinal.Tables[0].Rows[0]["NewSubjectList"].ToString();
                    return View(rm);
                }
                else
                {
                    ViewData["resultFS"] = "";
                    return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }


        [SessionCheckFilter]
        public ActionResult SchoolCorrectionAllRecord()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {
               
                var itemsch1 = new SelectList(new[]{new {ID="2",Name="Correction Lot"},new{ID="3",Name="Correction Id"},
                new{ID="4",Name="Student Id"},}, "ID", "Name", 1);
                ViewBag.MySch = itemsch1.ToList();

                List<SelectListItem> itemsch = new List<SelectListItem>();
                if (loginSession.fifth == "Y")
                {
                    itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                }
                if (loginSession.middle == "Y")
                {
                    itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                }
                ViewBag.MySch1 = itemsch.ToList();



                rm.CorrectionPerformaModel.Correctiondata = null;
                //ViewBag.TotalCountadded = "";
                string Search = " schl ='" + loginSession.SCHL + "'  ";   
                rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.SchoolCorrectionStatus(loginSession.SCHL, Search);

                if (rm.CorrectionPerformaModel.Correctiondata == null || rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCountadded =  ViewBag.TotalCount2 = 0;
                }
                else { ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count; }

                return View(rm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SchoolCorrectionAllRecord(FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();

            try
            {
              
                var itemsch1 = new SelectList(new[]{new {ID="2",Name="Correction Lot"},new{ID="3",Name="Correction Id"},
                new{ID="4",Name="Student Id"},}, "ID", "Name", 1);
                ViewBag.MySch = itemsch1.ToList();

                List<SelectListItem> itemsch = new List<SelectListItem>();
                if (loginSession.fifth == "Y")
                {
                    itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                }
                if (loginSession.middle == "Y")
                {
                    itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                }
                ViewBag.MySch1 = itemsch.ToList();


                string Search = "schl ='" + loginSession.SCHL + "' ";
                rm.CorrectionPerformaModel.Correctiondata = null;
                ViewBag.TotalCountadded = "";
                // rm.Correctiondata =_correctionPerformaRepository.GetSchoolCorrectionAllRecord(schlid);
                if (frm["Sch1"] != "")
                {
                    ViewBag.SelectedItem = frm["Sch1"];
                    TempData["SelectedItem"] = frm["Sch1"];
                    int SelValueSch = Convert.ToInt32(frm["Sch1"].ToString());

                    if (frm["SearchString"] != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and SCHL='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 2)
                        { Search += " and   CorrectionLot='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 3)
                        { Search += " and CorrectionId='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 4)
                        { Search += " and std_id='" + frm["SearchString"].ToString() + "'"; }
                    }
                }
                rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.SchoolCorrectionStatus(loginSession.SCHL, Search);
                ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                if (rm.CorrectionPerformaModel.Correctiondata == null || rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = 0;
                }


                return View(rm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }



        [HttpPost]
        public ActionResult CorrLotAcceptReject(string correctionType, string correctionLot, string acceptid, string rejectid, string removeid, string remarksid)
        {
            string IsUserAdminSchl = string.Empty;
            string IsUserAdminSchlID = string.Empty;
            string IsUserAdminSchlValue = string.Empty;
            if (Session["LoginSession"] == null && Session["AdminLoginSession"] == null)
            {
                string OutStatus = "0";
                var results = new
                {
                    status = OutStatus,
                };
                return Json(results);
            }
            else if (Session["LoginSession"] != null)
            {

                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                IsUserAdminSchl = "schl";
                IsUserAdminSchlValue = loginSession.SCHL.ToString();
            }
            else if (Session["AdminLoginSession"] != null)
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                IsUserAdminSchl = "admin";
                IsUserAdminSchlID = adminLoginSession.AdminId.ToString();
                IsUserAdminSchlValue = adminLoginSession.USER.ToString();
            }

            string remstatus = "";
            string status = "";
            if (removeid != "")
            {
                string OutStatus = "0";
                status =_correctionPerformaRepository.CorrLotAcceptReject(correctionType, correctionLot, acceptid, rejectid, removeid, IsUserAdminSchlValue, out OutStatus);
                var results = new
                {
                    status = OutStatus,
                };

                return Json(results);
            }
            else
            {

                if (correctionLot == null || correctionLot == "")
                {
                    var results = new
                    {
                        status = ""
                    };
                    return Json(results);
                }
                else
                {
                    string OutStatus = "0";
                    status =_correctionPerformaRepository.CorrLotAcceptReject(correctionType, correctionLot, acceptid, rejectid, removeid, IsUserAdminSchlValue, out OutStatus);
                    if (Convert.ToInt32(status) > 0 && remarksid != "")
                    {
                        //123(rohit),456(mm)
                        string[] split1 = remarksid.Split(',');
                        int sCount = split1.Length;
                        if (sCount > 0)
                        {
                            foreach (string s in split1)
                            {
                                string corid = s.Split('(')[0];
                                string remark = s.Split('(', ')')[1];
                                if (corid != "")
                                {
                                    if (IsUserAdminSchl.ToLower() == "schl")
                                    {
                                        remstatus =_correctionPerformaRepository.CorrLotRejectRemarksSP(corid, remark, IsUserAdminSchlValue);//CorrLotRejectRemarksSP}
                                    }
                                    else if (IsUserAdminSchl.ToLower() == "admin")
                                    {
                                        remstatus =_correctionPerformaRepository.CorrLotRejectRemarksSP(corid, remark, IsUserAdminSchlValue);//CorrLotRejectRemarksSP}

                                    }
                                }
                            }
                        }                       
                    }
                    var results = new
                    {
                        status = OutStatus,
                    };
                    return Json(results);
                }               
            }
           
        }

        #endregion SCHOOL CORRECTION PERFORMA BEGIN 



        #region Subject Correction Performa Link
        [SessionCheckFilter]
        public ActionResult SubjectCorrectionPerformaLink()
        {

            return View();
        }
        #endregion Subject Correction Performa Link


        //-------------------------Primary Subject Correction---------------------------/
        #region Primary subject correction
        [SessionCheckFilter]
        public ActionResult SubjectCorrectionPerforma(RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];           
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {              
             
                if (loginSession.SCHL == null)
                {
                    return RedirectToAction("Logout", "Login");

                }
                else
                {
                  

                    DataSet seleLastCan =_correctionPerformaRepository.PendingCorrectionSubjects(loginSession.SCHL, "5");
                    if (seleLastCan.Tables[0].Rows.Count > 0)
                    {

                        @ViewBag.message = "1";
                        rm.StoreAllData = seleLastCan;
                        ViewBag.TotalCount = seleLastCan.Tables[0].Rows.Count;
                    }
                    else
                    {
                        @ViewBag.message = "Record Not Found";
                    }                               
                }

                DateTime dtTodate = Convert.ToDateTime(DateTime.Today);
                DataSet result = rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(1, loginSession.SCHL, "S"); // passing Value to DBClass from model
                if (rm.CorrectionPerformaModel.Correctiondata == null || rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = ViewBag.TotalCountadded == 0;
                }
                else
                {
                    ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;
                }

                if (result.Tables[1].Rows.Count > 0)
                {
                    DateTime sDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["sDate"]);
                    DateTime eDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["eDate"]);

                    DateTime sDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["sDate"]);
                    DateTime eDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["eDate"]);

                    List<SelectListItem> itemsch = new List<SelectListItem>();
                    if (loginSession.fifth.ToUpper() == "Y" && dtTodate <= eDateP)
                    {
                        itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                    }
                    if (loginSession.middle.ToUpper() == "Y" && dtTodate <= eDateM)
                    {
                        itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                    }
                    rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();
                }

                if (ModelState.IsValid)
                {
                    return View(rm);
                }
                else
                { return View(rm); }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }
        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SubjectCorrectionPerforma(RegistrationModels rm, FormCollection frm, string cmd)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            List<SelectListItem> itemsch = new List<SelectListItem>();
            if (loginSession.fifth.ToUpper() == "Y")
            {
                itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
            }
            if (loginSession.middle.ToUpper() == "Y" )
            {
                itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
            }
            rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();

            string sid = Convert.ToString(rm.Std_id);                  
            if (string.IsNullOrEmpty(rm.CorrectionPerformaModel.Class))
            {
                return View(rm);
            }

            DataSet ds =_correctionPerformaRepository.SearchStudentGetByData_SubjectCORR(sid, rm.CorrectionPerformaModel.Class.ToString());
            if (ds == null || ds.Tables[0].Rows.Count == 0)
            {
                return View(rm);
            }
            else   if (ds != null && ds.Tables[2].Rows.Count == 1)
            {
                TempData["resultUpdate"] = "5"; // CorrectionPerforma Already exist is is not status is null.
                return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma");
            }
            else 
            {
              
                if (ds.Tables[0].Rows.Count > 0)
                {
                    rm.DA = ds.Tables[0].Rows[0]["Differently_Abled"].ToString(); 

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

                            // for class middle
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
                                rm.subS9 = ds.Tables[1].Rows[i]["SUBNM"].ToString();
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

            }

            //----------------------Matric Subjects End---------------------------



            ViewBag.DAb = DBClass.GetDA();

            if (cmd == "View All Correction Pending Record")
            {               
                DataSet seleLastCan =_correctionPerformaRepository.PendingCorrectionSubjects(loginSession.SCHL, "2");
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {

                    @ViewBag.message = "1";
                    rm.StoreAllData = seleLastCan;
                    ViewBag.TotalViewAllCount = seleLastCan.Tables[0].Rows.Count;
                    ViewBag.TotalCount = seleLastCan.Tables[0].Rows.Count;
                }
                else
                {
                    @ViewBag.message = "Record Not Found";
                }


                return View(rm);
            }
            else if (cmd == "View All Correction Record")
            {
                DataSet seleLastCan =_correctionPerformaRepository.ViewAllCorrectionSubjects(loginSession.SCHL);
                if (seleLastCan.Tables[0].Rows.Count > 0)
                {

                    @ViewBag.message = "1";
                    rm.StoreAllData = seleLastCan;
                    ViewBag.TotalViewAllCount = seleLastCan.Tables[0].Rows.Count;
                    ViewBag.TotalCount = seleLastCan.Tables[0].Rows.Count;
                }
                else
                {
                    @ViewBag.message = "Record Not Found";
                }


                return View(rm);
            }
            else
            {
                try
                {
                    DataSet seleLastCan =_correctionPerformaRepository.SearchCorrectionStudentDetails(rm.CorrectionPerformaModel.Class, loginSession.SCHL, sid);
                    if (seleLastCan.Tables[0].Rows.Count > 0)
                    {

                        @ViewBag.message = "1";
                        @ViewBag.stdid = seleLastCan.Tables[0].Rows[0]["std_id"].ToString();                       
                        @ViewBag.Regno = seleLastCan.Tables[0].Rows[0]["Registration_num"].ToString();
                        @ViewBag.canName = seleLastCan.Tables[0].Rows[0]["Candi_Name"].ToString();
                        @ViewBag.FName = seleLastCan.Tables[0].Rows[0]["Father_Name"].ToString();
                        @ViewBag.Mname = seleLastCan.Tables[0].Rows[0]["Mother_Name"].ToString();
                        @ViewBag.lot = seleLastCan.Tables[0].Rows[0]["lot"].ToString();
                        @ViewBag.DOB = seleLastCan.Tables[0].Rows[0]["DOB"].ToString();
                        @ViewBag.Frm = rm.CorrectionPerformaModel.Class;
                        @ViewBag.Subjlist = seleLastCan.Tables[0].Rows[0]["SubjectList"].ToString();
                    }
                    else
                    {
                        @ViewBag.message = "Record Not Found";
                        return View(rm);
                    }

                    if (ModelState.IsValid)
                    {
                        //-------------New SubList---------------//
                        DataSet NewSub = new DataSet();
                        NewSub =_correctionPerformaRepository.NewCorrectionSubjects(sid, rm.CorrectionPerformaModel.Class, loginSession.SCHL);  //NewCorrectionPerforma
                        List<SelectListItem> NewSUBList = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in NewSub.Tables[0].Rows)
                        {
                            NewSUBList.Add(new SelectListItem { Text = @dr["name_eng"].ToString(), Value = @dr["sub"].ToString() });

                        }
                        List<SelectListItem> itemElectiveSub = new List<SelectListItem>();
                        ViewBag.ElectiveSubjects = itemElectiveSub.ToList();
                        if (rm.CorrectionPerformaModel.Class == "8")
                        {                           
                            if (NewSub.Tables[1].Rows.Count > 0)
                            {
                                foreach (System.Data.DataRow dr in NewSub.Tables[1].Rows)
                                {
                                    itemElectiveSub.Add(new SelectListItem { Text = @dr["name_eng"].ToString(), Value = @dr["sub"].ToString() });
                                }
                                ViewBag.ElectiveSubjects = itemElectiveSub.ToList();
                            }
                        }

                        //-----------End-------------------//                    

                        //----------Old Subject Fill Start----------//
                        DataSet dsOld =_correctionPerformaRepository.SearchOldStudent_Subject(sid, rm.CorrectionPerformaModel.Class, loginSession.SCHL);
                        if (dsOld == null || dsOld.Tables[0].Rows.Count == 0)
                        {
                            return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma");
                        }
                        else 
                        {

                            List<SelectListItem> OLDSUBList = new List<SelectListItem>();
                            foreach (System.Data.DataRow dr in dsOld.Tables[0].Rows)
                            {
                                OLDSUBList.Add(new SelectListItem { Text = @dr["name_eng"].ToString(), Value = @dr["sub"].ToString() });

                            }
                            ViewBag.SubOLd = OLDSUBList;
                            ViewBag.SubCnt = ds.Tables[0].Rows.Count;
                        }                        

                    }
                    else
                    {
                        return View(rm);
                    }
                    return View(rm);

                }
                catch (Exception ex)
                {
                    return View(rm);
                }

            }

        }

        [SessionCheckFilter]
        public ActionResult CorrSubDelete(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];            
            if (id != null)
            {
                string result = "";
                result = _correctionPerformaRepository.DeleteSubDataByCorrectionId(id, out result);
                TempData["DeleteStatus"] = result;
            }
            return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma");
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SubjectCorrectionAdd(RegistrationModels rm, FormCollection frm)
        {
            string result = "";
             string id = frm["Std_id"].ToString();
           // DataSet ds =_correctionPerformaRepository.SearchStudentGetByData_SubjectCORR(id, rm.CorrectionPerformaModel.Class);
            // Start Subject Master
            if (rm.CorrectionPerformaModel.Class == "5")
            {
                #region Primary Subject Master
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

                #endregion


                if (dtFifthSubject.Rows.Count != 6)
                {
                    TempData["resultUpdate"] = "SUBCOUNT";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                if (rm.subS1 == "501" && rm.subS2 == "502")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                else if (rm.subS1 == "503" && rm.subS2 == "504")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                else if (rm.subS1 == "505" && rm.subS2 == "506")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }

                result =_correctionPerformaRepository.Primary_Subject_Correction(rm,  id, dtFifthSubject);
                TempData["resultUpdate"] = result;
               
            }

            else if (rm.CorrectionPerformaModel.Class == "8")
            {
                #region Middle Subject Master
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


                if (dtEighthSubject.Rows.Count != 10)
                {
                    TempData["resultUpdate"] = "SUBCOUNT";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }

                #endregion


                if (rm.subS1 == "801" && rm.subS2 == "802")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                else if (rm.subS1 == "803" && rm.subS2 == "804")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                else if (rm.subS1 == "805" && rm.subS2 == "806")
                {
                    TempData["resultUpdate"] = "NA";
                    return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
                }
                result =_correctionPerformaRepository.Middle_Subject_Correction(rm,  id, dtEighthSubject);
                TempData["resultUpdate"] = result;               
            }
            // End Subject Master




            return RedirectToAction("SubjectCorrectionPerforma", "CorrectionPerforma", result);
        }


        [SessionCheckFilter]
        public ActionResult SchoolCorrectionPerformaRoughReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }

            string CorType = id.ToLower().Trim() == "particular" ? "02" : "01";
            ViewBag.CorType = CorType;
            try
            {
                if (ModelState.IsValid)
                {

                    if (loginSession.SCHL != null)
                    {
                       // id = "02";
                        rm.CorrectionPerformaModel.CorrectionLot = CorType;
                        rm.SCHL = loginSession.SCHL.ToString();
                        rm.StoreAllData =_correctionPerformaRepository.SchoolCorrectionPerformaRoughReport(rm); // passing Value to DBClass from model
                        if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                        {
                            ViewData["resultFS"] = rm.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCountaddedRecord = rm.StoreAllData.Tables[0].Rows.Count;
                            rm.SCHL = rm.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                            rm.DIST = Convert.ToString(rm.StoreAllData.Tables[0].Rows[0]["DIST"]);
                            rm.CorrectionPerformaModel.schlCorNameE = rm.StoreAllData.Tables[0].Rows[0]["SchlFullNME"].ToString();
                            rm.CorrectionPerformaModel.schlCorConDetails = rm.StoreAllData.Tables[0].Rows[0]["schlCorConDetails"].ToString();
                            rm.Class = rm.StoreAllData.Tables[0].Rows[0]["Class"].ToString();
                            rm.CorrectionPerformaModel.CorrectionLot = rm.StoreAllData.Tables[0].Rows[0]["CorrectionLot"].ToString();
                            rm.CorrectionPerformaModel.Correctiontype = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFieldDisplayName"].ToString();
                            rm.CorrectionPerformaModel.Remark = rm.StoreAllData.Tables[0].Rows[0]["Remark"].ToString();
                            rm.CorrectionPerformaModel.CorrectionInsertDt = rm.StoreAllData.Tables[0].Rows[0]["InsertDt"].ToString();
                            rm.CorrectionPerformaModel.CorrectionFinalSubmitDt = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt1"].ToString();
                            rm.CorrectionPerformaModel.CorrectionVerifyDt = rm.StoreAllData.Tables[0].Rows[0]["VerifyDt"].ToString();
                            ViewBag.OldSubjlist = rm.StoreAllData.Tables[0].Rows[0]["OlDsubjectList"].ToString();
                            ViewBag.NewSubjlist = rm.StoreAllData.Tables[0].Rows[0]["NewSubjectList"].ToString();
                            return View(rm);
                        }
                        else
                        {
                            ViewData["resultFS"] = "";
                            return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                        }
                    }
                    else
                    {
                        return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                    }
                }
                else
                {
                    return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
            }
        }


        [SessionCheckFilter]
        public ActionResult SchoolCorrectionPerformaRoughReportSJ(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {
                if (ModelState.IsValid)
                {

                    if (loginSession.SCHL != null)
                    {
                        id = "01";
                        rm.CorrectionPerformaModel.CorrectionLot = id;
                        rm.SCHL = loginSession.SCHL.ToString();
                        rm.StoreAllData =_correctionPerformaRepository.SchoolCorrectionPerformaRoughReport(rm); // passing Value to DBClass from model
                        if (rm.StoreAllData.Tables[0].Rows.Count > 0)
                        {
                            ViewData["resultFS"] = rm.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCountaddedRecord = rm.StoreAllData.Tables[0].Rows.Count;
                            rm.SCHL = rm.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                            rm.DIST = Convert.ToString(rm.StoreAllData.Tables[0].Rows[0]["DIST"]);
                            rm.CorrectionPerformaModel.schlCorNameE = rm.StoreAllData.Tables[0].Rows[0]["SchlFullNME"].ToString();
                            rm.CorrectionPerformaModel.schlCorConDetails = rm.StoreAllData.Tables[0].Rows[0]["schlCorConDetails"].ToString();
                            rm.Class = rm.StoreAllData.Tables[0].Rows[0]["Class"].ToString();
                            rm.CorrectionPerformaModel.CorrectionLot = rm.StoreAllData.Tables[0].Rows[0]["CorrectionLot"].ToString();
                            rm.CorrectionPerformaModel.Correctiontype = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFieldDisplayName"].ToString();
                            rm.CorrectionPerformaModel.Remark = rm.StoreAllData.Tables[0].Rows[0]["Remark"].ToString();
                            rm.CorrectionPerformaModel.CorrectionInsertDt = rm.StoreAllData.Tables[0].Rows[0]["InsertDt"].ToString();
                            rm.CorrectionPerformaModel.CorrectionFinalSubmitDt = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt1"].ToString();
                            rm.CorrectionPerformaModel.CorrectionVerifyDt = rm.StoreAllData.Tables[0].Rows[0]["VerifyDt"].ToString();
                            ViewBag.OldSubjlist = rm.StoreAllData.Tables[0].Rows[0]["OlDsubjectList"].ToString();
                            ViewBag.NewSubjlist = rm.StoreAllData.Tables[0].Rows[0]["NewSubjectList"].ToString();
                            return View(rm);
                        }
                        else
                        {
                            ViewData["resultFS"] = "";
                            return RedirectToAction("SchoolCorrection", "CorrectionPerforma");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Logout", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "Login");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Login");
            }
        }




        #endregion Primary subject correction

        //-------------------------End Primary Subject Correction---------------------------/

        #region VerifySchoolCorrection
        [SessionCheckFilter]
        public ActionResult VerifySchoolCorrection(string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            //  AdminModels am = new AdminModels();  
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();

            //rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {

                string classAssign = "";              
              

                FormCollection frc = new FormCollection();
                var itemsch = new SelectList(new[]{new {ID="1",Name="Particular"},new {ID="2",Name="Subject"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                DateTime dtTodate = Convert.ToDateTime(DateTime.Today);
                DataSet result = rm.CorrectionPerformaModel.Correctiondata = _correctionPerformaRepository.GetStudentRecordsCorrectionData(1, loginSession.SCHL, ""); // passing Value to DBClass from model
                if (result.Tables[1].Rows.Count > 0)
                {
                    DateTime sDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["sDate"]);
                    DateTime eDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["eDate"]);
                    DateTime VerifyLastDateBySchlP = Convert.ToDateTime(result.Tables[1].Rows[0]["VerifyLastDateBySchl"]);

                    DateTime sDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["sDate"]);
                    DateTime eDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["eDate"]);
                    DateTime VerifyLastDateBySchlM = Convert.ToDateTime(result.Tables[1].Rows[1]["VerifyLastDateBySchl"]);

                    List<SelectListItem> itemClass = new List<SelectListItem>();
                    if (loginSession.fifth.ToUpper() == "Y" && dtTodate <= VerifyLastDateBySchlP)
                    {
                        itemClass.Add(new SelectListItem { Text = "Primary", Value = "5" });
                    }
                    if (loginSession.middle.ToUpper() == "Y" && dtTodate <= VerifyLastDateBySchlM)
                    {
                        itemClass.Add(new SelectListItem { Text = "Middle", Value = "8" });
                    }
                    rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();
                    ViewBag.CorrectionClass = rm.CorrectionPerformaModel.CorrectionClassList = itemClass.ToList();
                }



             
                //
                //var itemClass = new SelectList(new[] { new { ID = "1", Name = "9th Class" }, new { ID = "2", Name = "11th Class" }, }, "ID", "Name", 1);
                //ViewBag.CorrectionClass = itemClass.ToList();


                if (ModelState.IsValid)
                {
                    if (TempData["SchlCorLot"] != null)
                    {
                        rm.CorrectionPerformaModel.CorrectionLot = TempData["SchlCorLot"].ToString();
                        ViewBag.SelectedItemcode = TempData["SchlCorrectionType1"].ToString();    
                        
                        string Search = string.Empty;
                        Search = " a.SCHL  ='" + loginSession.SCHL + "'";
                        string CrType = TempData["SchlCorrectionType1"].ToString();
                        Search += " and a.correctionlot='" + rm.CorrectionPerformaModel.CorrectionLot + "' ";

                        if (TempData["SchlCorrectionClass"] != null)
                        {
                            classAssign = rm.CorrectionPerformaModel.Class = TempData["SchlCorrectionClass"].ToString();
                            if (loginSession.fifth == "5")
                            {
                                Search += " and a.class = 'Primary' ";
                            }
                            else if (loginSession.fifth == "8")
                            {
                                Search += " and a.class = 'Middle' ";
                            }

                        }

                        //------ Paging Srt
                        int pageIndex = 1;
                        pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                        ViewBag.pagesize = pageIndex;
                        //string Catg = CrType;   
                        TempData["SchlCorrectionType1"] = ViewBag.SelectedItemcode;
                        TempData["SchlCorLot"] = rm.CorrectionPerformaModel.CorrectionLot;
                        TempData["SchlCorrectionClass"] = rm.CorrectionPerformaModel.Class;
                        //---- Paging end
                        rm.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        ViewBag.CorrectionFinalSubmitDt = "";
                        if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            ViewBag.TotalCountP = 0;
                            return View(rm);
                        }
                        else
                        {
                            ViewBag.CorrectionFinalSubmitDt = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt"].ToString();
                            ViewBag.TotalCountP = rm.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(rm);
                        }
                    }
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(rm);


                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception ex)
            {
                return View(rm);
            }
            
        }
        [SessionCheckFilter]
        [HttpPost]
        public ActionResult VerifySchoolCorrection(int? page, FormCollection frc, string cmd, string id,string CorrectionLot, string CorrectionType1, RegistrationModels rm)
        {
            
            //rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            try
            {               
               
                string classAssign = "";
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" }, }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                DateTime dtTodate = Convert.ToDateTime(DateTime.Today);
                DataSet result = rm.CorrectionPerformaModel.Correctiondata = _correctionPerformaRepository.GetStudentRecordsCorrectionData(1, loginSession.SCHL, ""); // passing Value to DBClass from model
                if (result.Tables[1].Rows.Count > 0)
                {
                    DateTime sDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["sDate"]);
                    DateTime eDateP = Convert.ToDateTime(result.Tables[1].Rows[0]["eDate"]);
                    DateTime VerifyLastDateBySchlP = Convert.ToDateTime(result.Tables[1].Rows[0]["VerifyLastDateBySchl"]);

                    DateTime sDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["sDate"]);
                    DateTime eDateM = Convert.ToDateTime(result.Tables[1].Rows[1]["eDate"]);
                    DateTime VerifyLastDateBySchlM = Convert.ToDateTime(result.Tables[1].Rows[1]["VerifyLastDateBySchl"]);

                    List<SelectListItem> itemClass = new List<SelectListItem>();
                    if (loginSession.fifth.ToUpper() == "Y" && dtTodate <= VerifyLastDateBySchlP)
                    {
                        itemClass.Add(new SelectListItem { Text = "Primary", Value = "5" });
                    }
                    if (loginSession.middle.ToUpper() == "Y" && dtTodate <= VerifyLastDateBySchlM)
                    {
                        itemClass.Add(new SelectListItem { Text = "Middle", Value = "8" });
                    }
                    rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();
                    ViewBag.CorrectionClass = rm.CorrectionPerformaModel.CorrectionClassList = itemClass.ToList();
                }



                if (ModelState.IsValid)
                {
                    //------ Paging Srt
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;
                    //string Catg = CrType;                        

                    //---- Paging end

                    #region Search Record 

                  

                    if (cmd == "Search" && frc["CorrectionType1"] != null && !string.IsNullOrEmpty(rm.CorrectionPerformaModel.Class))
                    {
                        TempData["SchlCorrectionType1"] = ViewBag.SelectedItemcode = frc["CorrectionType1"].ToString();
                        TempData["SchlCorLot"] = rm.CorrectionPerformaModel.CorrectionLot;
                        TempData["SchlCorrectionClass"] = classAssign = rm.CorrectionPerformaModel.Class;

                        string Search = string.Empty;
                        Search = " a.SCHL  ='" + loginSession.SCHL + "'";
                        string CrType = frc["CorrectionType1"].ToString();   
                        Search += "  and a.correctionlot ='" + rm.CorrectionPerformaModel.CorrectionLot + "' ";

                        
                        switch (rm.CorrectionPerformaModel.Class)
                        {
                            case "5":
                                Search += " and a.class = 'Primary' ";
                                break;
                            case "8":
                                Search += " and a.class = 'Middle' ";
                                break;
                        }
                        ViewBag.CorrectionFinalSubmitDt = "";
                       rm.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            ViewBag.TotalCountP = 0;
                            return View(rm);
                        }
                        else
                        {

                            ViewBag.CorrectionFinalSubmitDt = rm.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt"].ToString();
                            ViewBag.TotalCountP = rm.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(rm);
                        }
                    }

                    #endregion Search Record                   
                    return View(rm);
                }
                else
                {
                    return View(rm);
                }
            }
            catch (Exception ex)
            {
                return View(rm);
            }
        }

        [SessionCheckFilter]
        public ActionResult VerifySchoolCorrectionUpdated(int? page)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            try
            {               
               
                var itemFee = new SelectList(new[] { new { ID = "1", Name = "With Fee" }, new { ID = "2", Name = "Widout Fee" }, }, "ID", "Name", 1);
                ViewBag.FeeType = itemFee.ToList();

                FormCollection frc = new FormCollection();
                var itemsch = new SelectList(new[]{new {ID="1",Name="Particular"},new {ID="2",Name="Subject"},}, "ID", "Name", 1);

                ViewBag.CorrectionType = itemsch.ToList();
             
                AdminModels am = new AdminModels();
                if (ModelState.IsValid)
                {
                    string Search = string.Empty;
                    string CrType = "2";
                    Search = "a.status is not null";
                    Search += " and a.schl  = '" + loginSession.SCHL + "'";

                    //------ Paging Srt
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;
                    //---- Paging end
                    am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirmUpdated(Search, CrType, pageIndex);
                    // ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;

                    if (am.StoreAllData == null)
                    {
                        ViewBag.TotalCountP = 0;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(am);
                    }
                    else
                    {
                        DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, "SCHL", "");// check fee exist 
                        ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                        if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.IsFeeExists = "0";
                        }
                        else
                        { ViewBag.IsFeeExists = "1"; }

                        if (am.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.CorrectionFeeDate = am.StoreAllData.Tables[4].Rows[0]["CorrectionFeeDate"].ToString();
                            ViewBag.CorrectionFeeDateStatus = am.StoreAllData.Tables[4].Rows[0]["CorrectionFeeDateStatus"].ToString();
                        }
                        ViewBag.CorrectionFinalSubmitDt = "";
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.TotalCountP = 0;
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.CorrectionFeeDate = am.StoreAllData.Tables[0].Rows[0]["CorrectionFeeDate"].ToString();
                            ViewBag.CorrectionFinalSubmitDt = am.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt"].ToString();
                            ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            TempData["ForFinalCorrectionLot"] = String.Join(",", am.StoreAllData.Tables[3].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(am);
                        }

                    }

                }
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();

            }
            catch (Exception ex)
            {                
                return View();
            }
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult VerifySchoolCorrectionUpdated(int? page, FormCollection frc, string cmd)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            AdminModels am = new AdminModels();
            try
            {     
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" },}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                var itemFee = new SelectList(new[] { new { ID = "1", Name = "With Fee" }, new { ID = "2", Name = "Widout Fee" }, }, "ID", "Name", 1);
                ViewBag.FeeType = itemFee.ToList();
                //------ Paging Srt
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                //---- Paging end                   
                #region View All Correction Pending Record                
                string Search = string.Empty;
                string CrType = "2";
                Search = "a.status is not null";

                if (frc["CorrectionLot"] != null)
                {
                    ViewBag.CorrectionLot = am.CorrectionLot = frc["CorrectionLot"].ToString();
                    Search += " and a.correctionlot ='" + am.CorrectionLot + "' ";

                }
                Search += " and a.SCHL  ='" + loginSession.SCHL.ToString() + "'";
                am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirmUpdated(Search, CrType, pageIndex);

                if (am.StoreAllData == null)
                {
                    ViewBag.TotalCountP = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(am);
                }
                else
                {
                    DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, "SCHL", "");// check fee exist 
                    ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                    if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.IsFeeExists = "0";
                    }
                    else
                    { ViewBag.IsFeeExists = "1"; }
                    //
                    if (am.StoreAllData.Tables[4].Rows.Count > 0)
                    {
                        ViewBag.CorrectionFeeDate = am.StoreAllData.Tables[4].Rows[0]["CorrectionFeeDate"].ToString();
                        ViewBag.CorrectionFeeDateStatus = am.StoreAllData.Tables[4].Rows[0]["CorrectionFeeDateStatus"].ToString();
                    }
                    ViewBag.CorrectionFinalSubmitDt = "";
                    if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCountP = 0;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.CorrectionFinalSubmitDt = am.StoreAllData.Tables[0].Rows[0]["CorrectionFinalSubmitDt"].ToString();                      
                        ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                        ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                        TempData["SchlForFinalCorrectionLot"] = String.Join(",", am.StoreAllData.Tables[3].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                        int tp = Convert.ToInt32(ViewBag.TotalCount);
                        int pn = tp / 30;
                        int cal = 30 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                    }
                }

                #endregion View All Correction Pending Record

                return View(am);

            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [SessionCheckFilter]
        public ActionResult VerifySchoolCorrectionFinalSubmit(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Middle = loginSession.middle;
            ViewBag.Primary = loginSession.fifth;
            AdminModels am = new AdminModels();
            try
            {
                if (id == null)
                {
                    return RedirectToAction("VerifySchoolCorrection", "CorrectionPerforma");
                }

                FormCollection frm = new FormCollection();
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" } }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();



                if (id == "FinalSubmitMiddleSubject")
                {
                    DataSet dschk = _correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, "SCHL", "");// check fee exist 
                    ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                    if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                    {
                        // final submit here   

                        string FirmCorrectionLot = string.Empty;
                        string OutError = string.Empty;
                        DataSet ds1 = _correctionPerformaRepository.CorrectionDataFirmFinalSubmitSPRNJuniorMiddleSubjectOnly(loginSession.SCHL, out FirmCorrectionLot, out OutError);  // Final Submit Main Function                       
                        if (FirmCorrectionLot.Length > 2)
                        {
                            ViewBag.TotalCount = 1;
                            ViewData["Status"] = "1";
                            ViewData["Message"] = "";

                            am.StoreAllData = _correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, loginSession.SCHL, "");
                            if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                            {
                                ViewBag.TotalCount = 0;
                                ViewData["Status"] = "10";
                            }
                            else
                            {

                                ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                                ViewData["Status"] = "11";
                            }

                        }
                        else
                        {
                            ViewBag.TotalCount = 0;
                            ViewData["Status"] = "0";
                            ViewData["Message"] = OutError;
                        }
                        //  am.StoreAllData =_correctionPerformaRepository.GetAllCorrectionDataFirm(UserName);


                        return View(am);
                    }
                    else
                    {
                        ViewBag.commaCorrectionLot = String.Join(",", dschk.Tables[0].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                        ViewBag.TotalCount = 1;
                        ViewData["Status"] = "5";

                    }
                }
                else if (id == "FinalSubmit")
                {
                    DataSet dschk = _correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, "SCHL", "");// check fee exist 
                    ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                    if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                    {
                        // final submit here   

                        string FirmCorrectionLot = string.Empty;
                        string OutError = string.Empty;
                        DataSet ds1 = _correctionPerformaRepository.CorrectionDataFirmFinalSubmitSPRN(loginSession.SCHL, out FirmCorrectionLot, out OutError);  // Final Submit Main Function                       
                        if (FirmCorrectionLot.Length > 2)
                        {
                            ViewBag.TotalCount = 1;
                            ViewData["Status"] = "1";
                            ViewData["Message"] = "";

                            am.StoreAllData = _correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, loginSession.SCHL, "");
                            if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                            {
                                ViewBag.TotalCount = 0;
                                ViewData["Status"] = "10";
                            }
                            else
                            {

                                ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                                ViewData["Status"] = "11";
                            }

                        }
                        else
                        {
                            ViewBag.TotalCount = 0;
                            ViewData["Status"] = "0";
                            ViewData["Message"] = OutError;
                        }
                        //  am.StoreAllData =_correctionPerformaRepository.GetAllCorrectionDataFirm(UserName);


                        return View(am);
                    }
                    else
                    {
                        ViewBag.commaCorrectionLot = String.Join(",", dschk.Tables[0].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                        ViewBag.TotalCount = 1;
                        ViewData["Status"] = "5";

                    }
                }
                else if (id == "ViewAll")
                {
                    am.StoreAllData = _correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, loginSession.SCHL, "");
                    if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCount = 0;
                        ViewData["Status"] = "10";
                    }
                    else
                    {

                        ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                        ViewData["Status"] = "11";
                    }
                }
                return RedirectToAction("SchoolCorrectionAllRecord", "CorrectionPerforma");
                // return View(am);

            }
            catch (Exception ex)
            {
                return RedirectToAction("VerifySchoolCorrectionUpdated", "CorrectionPerforma");
            }
        }

        #endregion



        #region Admin Correction Panel 


        #region SchoolCorrectionStatus
        [AdminLoginCheckFilter]     
        public ActionResult SchoolCorrectionStatus(string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels rm = new AdminModels();
            try
            {
               
                DataSet result = new DataSet();
                string schlid = string.Empty;
                string Search = "schl like '%%' ";


                var itemsch1 = new SelectList(new[]{new{ID="1",Name="School Code"},new {ID="2",Name="Correction Lot"},new{ID="3",Name="Correction Id"},
                new{ID="4",Name="Student Id"},}, "ID", "Name", 1);
                ViewBag.MySch = itemsch1.ToList();

                var itemsch3 = new SelectList(new[]{new {ID="1",Name="All"},new {ID="2",Name="Particular"},new {ID="3",Name="Subject"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch3.ToList();

                ViewBag.Id = id;
                if (!string.IsNullOrEmpty(id))
                {
                    ViewBag.IsSchl = "1";
                    Search += "and std_id ='" + id + "' ";

                     if (adminLoginSession.Dist_Allow != "")
                    {
                        Search += " and DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                    }

                    if (adminLoginSession.ActionRight == "8")
                    {
                        Search += " and class in('Middle')";
                    }
                    else if (adminLoginSession.ActionRight == "5")
                    {
                        Search += " and class in('Primary')";
                    }
                    else if (adminLoginSession.ActionRight == "5,8" || adminLoginSession.ActionRight == "8,5")
                    {
                        Search += " and class in('Primary','Middle')";
                    }


                    rm.StoreAllData =_correctionPerformaRepository.SchoolCorrectionStatus(schlid, Search);
                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message2 = "Record Not Found";
                        ViewBag.TotalCount2 = 0;
                        ViewBag.TotalCountadded = 0;
                    }
                    else
                    {
                        ViewBag.TotalCountadded = rm.StoreAllData.Tables[0].Rows.Count;

                    }
                }
                else
                {
                    rm.StoreAllData = null;     
                    ViewBag.IsSchl = "0";
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = 0;
                    ViewBag.TotalCountadded = 0;
                }
                return View(rm);
            }
            catch (Exception ex)
            {              
            }
            return View(rm);
        }

        [AdminLoginCheckFilter]       
        [HttpPost]
        public ActionResult SchoolCorrectionStatus(string id, FormCollection frm)
        {

            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels rm = new AdminModels();
            try
            {

                
                DataSet result = new DataSet();
                string schlid = string.Empty;
                string Search = "schl like '%%' ";
                ViewBag.Id = id;

                var itemsch1 = new SelectList(new[]{new{ID="1",Name="School Code"},new {ID="2",Name="Correction Lot"},new{ID="3",Name="Correction Id"},
                new{ID="4",Name="Student Id"},}, "ID", "Name", 1);
                ViewBag.MySch = itemsch1.ToList();

                var itemsch3 = new SelectList(new[]{new {ID="1",Name="All"},new {ID="2",Name="Particular"},new {ID="3",Name="Subject"},
            new{ID="4",Name="Photo/Sign"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch3.ToList();


                ViewBag.IsSchl = "0";


                if (frm["CorrectionType"] != null)
                {
                    ViewBag.SelectedCorrectionType = frm["CorrectionType"].ToString();
                    { Search += " and   CorPanel='" + frm["CorrectionType"].ToString() + "'"; }

                }

                if (frm["Sch1"] != "")
                {
                    ViewBag.SelectedItem = frm["Sch1"];
                    TempData["SelectedItem"] = frm["Sch1"];
                    int SelValueSch = Convert.ToInt32(frm["Sch1"].ToString());

                    if (frm["SearchString"] != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and SCHL='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 2)
                        { Search += " and   CorrectionLot='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 3)
                        { Search += " and CorrectionId='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 4)
                        { Search += " and std_id='" + frm["SearchString"].ToString() + "'"; }
                    }
                }


                if (adminLoginSession.Dist_Allow != "")
                {
                    Search += " and DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                }

                if (adminLoginSession.ActionRight == "8")
                {
                    Search += " and class in('Middle')";
                }
                else if (adminLoginSession.ActionRight == "5")
                {
                    Search += " and class in('Primary')";
                }
                else if (adminLoginSession.ActionRight == "5,8" || adminLoginSession.ActionRight == "8,5")
                {
                    Search += " and class in('Primary','Middle')";
                }

                rm.StoreAllData =_correctionPerformaRepository.SchoolCorrectionStatus(schlid, Search);
                if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = 0;
                    ViewBag.TotalCountadded = 0;
                }
                else
                {
                    ViewBag.TotalCountadded = rm.StoreAllData.Tables[0].Rows.Count;

                }
                return View(rm);
            }
            catch (Exception ex)
            {                
            }
            return View(rm);
        }

        #endregion SchoolCorrectionStatus

        #region RejectSchoolCorrection
        [AdminLoginCheckFilter]
        public ActionResult RejectSchoolCorrection(int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            //FormCollection frc = new FormCollection();
            AdminModels am = new AdminModels();
            try
            {             
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" }, }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                
                if (ModelState.IsValid)
                {
                    if (TempData["CorLot"] != null)
                    {
                        string CorLot2 = TempData["CorLot"].ToString();
                        ViewBag.SelectedItemcode = TempData["CorrectionType1"].ToString();
                        am.CorrectionLot = TempData["CorLot"].ToString();
                        string Search = string.Empty;
                        string CrType = TempData["CorrectionType1"].ToString();                        
                        Search = "a.correctionlot='" + CorLot2 + "' ";

                        if (adminLoginSession.Dist_Allow != "")
                        {
                            Search += " and b.DIST in(" + adminLoginSession.Dist_Allow + ")";
                        }

                        //if (adminLoginSession.ActionRight == "8")
                        //{
                        //    Search += " and a.class in('Sr.Secondary Open','Sr.Secondary Regular')";
                        //}
                        //else if (adminLoginSession.ActionRight == "5")
                        //{
                        //    Search += " and a.class in('Matriculation Regular','Matriculation Open')";
                        //}
                        //------ Paging Srt
                        int pageIndex = 1;
                        pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                        ViewBag.pagesize = pageIndex;
                        //string Catg = CrType;                        

                        //---- Paging end
                        am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(am);
                        }
                    }
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();


                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult RejectSchoolCorrection(int? page, FormCollection frc, string cmd, string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();            
            try
            {             
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" },  }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                if (ModelState.IsValid)
                {
                    //------ Paging Srt
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;
                    //string Catg = CrType;                        

                    //---- Paging end

                    #region Search Record 
                    
                    if (cmd == "Search" && frc["CorrectionType1"].ToString() != "")
                    {                      
                        string Search = string.Empty;
                        string CrType = frc["CorrectionType1"].ToString();
                        TempData["CorrectionType1"] = ViewBag.SelectedItemcode = frc["CorrectionType1"].ToString();
                        TempData["CorLot"] = am.CorrectionLot = frc["CorrectionLot"].ToString();
                        
                        am.SearchResult = frc["SearchResult"].ToString();
                       
                        if (frc["SearchResult"].ToString() != "0")
                        {
                            switch (am.SearchResult)
                            {
                                case "1": Search = "a.correctionlot ='" + am.CorrectionLot + "' "; break;
                                case "2": Search = "a.correctionId ='" + am.CorrectionLot + "' "; break;
                                case "3":
                                    if (CrType == "1") Search = "a.std_id ='" + am.CorrectionLot + "' ";
                                    if (CrType == "2") Search = "a.CANDID ='" + am.CorrectionLot + "' ";                                   
                                    break;
                            }

                        }
                        if (adminLoginSession.Dist_Allow != "")
                        {
                            Search += " and b.DIST in(" + adminLoginSession.Dist_Allow + ")";
                        }
                        //if (Session["UserName"].ToString() == "CREA" || Session["UserName"].ToString() == "PERF")
                        //{
                        //    Search += " and a.class in('Sr.Secondary Open','Sr.Secondary Regular')";
                        //}
                        //else if (Session["UserName"].ToString() == "DATA" || Session["UserName"].ToString() == "SAI")
                        //{
                        //    Search += " and a.class in('Matriculation Regular','Matriculation Open')";
                        //}

                        am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(am);
                        }
                    }

                    #endregion Search Record 

                    #region Add All Checked

                    //NAllChkId - 171007875
                    if (cmd == "Reject All Checked")
                    {
                        string NAllchkid = frc["NAllChkId"];
                        string Search = string.Empty;
                        string CrType = frc["CorrectionType1"].ToString();
                        if (NAllchkid != null)
                        {
                            NAllchkid = "CorrectionId in (" + NAllchkid + ")";                            
                            DataSet ds =_correctionPerformaRepository.AllCancelFirmSchoolCorrection(adminLoginSession.USER, CrType, NAllchkid);
                        }
                        string Searchupdate = string.Empty;
                        am.CorrectionLot = frc["CorrectionLot"].ToString();
                        Search = "a.correctionlot ='" + am.CorrectionLot + "' ";
                        if (adminLoginSession.Dist_Allow != "")
                        {
                            Search += " and b.DIST in(" + adminLoginSession.Dist_Allow + ")";
                        }
                       
                        am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            return View(am);
                        }
                    }
                    #endregion Add All Checked
                    return View(am);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {               
                return View();
            }
        }
        #endregion RejectSchoolCorrection


        #region FirmSchoolCorrection 
        [AdminLoginCheckFilter]
        public ActionResult FirmSchoolCorrection(int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            //FormCollection frc = new FormCollection();
            try
            {

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" }, }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();
                   
                if (string.IsNullOrEmpty(adminLoginSession.ActionRight))
                {
                    ViewBag.Result = "10";
                    ViewBag.TotalCount = 0;
                    return View(am);
                }              
              


                if (ModelState.IsValid)
                {
                    if (TempData["CorLot"] != null)
                    {
                        string CorLot2 = am.CorrectionLot = TempData["CorLot"].ToString();
                        string CrType = ViewBag.SelectedItemcode = TempData["CorrectionType1"].ToString();
                        string Search = string.Empty;
                       
                        //ViewBag.CorrectionType = am.CorrectionType;
                        Search = "a.correctionlot='" + CorLot2 + "' ";
                        if (adminLoginSession.Dist_Allow != "")
                        {
                            Search += " and b.DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                        }

                        if (adminLoginSession.ActionRight =="8")
                        {
                            Search += " and a.class in('Middle')";
                        }
                        else if (adminLoginSession.ActionRight == "5")
                        {
                            Search += " and a.class in('Primary')";
                        }
                        else if (adminLoginSession.ActionRight == "5,8" || adminLoginSession.ActionRight == "8,5")
                        {
                            Search += " and a.class in('Primary','Middle')";
                        }                                                

                        //------ Paging Srt
                        int pageIndex = 1;
                        pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                        ViewBag.pagesize = pageIndex;
                        //string Catg = CrType;                        
                        TempData["CorrectionType1"] = ViewBag.SelectedItemcode;
                        TempData["CorLot"] = am.CorrectionLot;                        
                        //---- Paging end
                        am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                        ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(am);
                        }
                    }
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();


                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult FirmSchoolCorrection(int? page, FormCollection frc, string cmd, string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {
                var itemsch = new SelectList(new[]{new {ID="1",Name="Particular"},new {ID="2",Name="Subject"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                if (string.IsNullOrEmpty(adminLoginSession.ActionRight))
                {
                    ViewBag.Result = "10";
                    ViewBag.TotalCount = 0;
                    return View(am);
                }



                //------ Paging Srt
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                //string Catg = CrType;                        

                //---- Paging end

                #region Search Record 
                
                if (cmd == "Search" && frc["CorrectionType1"].ToString() != "")
                {
                    string Search = string.Empty;
                    string CrType = frc["CorrectionType1"].ToString();
                    TempData["CorrectionType1"] = ViewBag.SelectedItemcode = CrType;

                    TempData["CorLot"] = am.CorrectionLot = frc["CorrectionLot"].ToString();  

                    Search = "a.correctionlot ='" + am.CorrectionLot + "' ";
                   
                    if (adminLoginSession.Dist_Allow != "")
                    {
                        Search += " and b.DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                    }

                    if (adminLoginSession.ActionRight == "8")
                    {
                        Search += " and a.class in('Middle')";
                    }
                    else if (adminLoginSession.ActionRight == "5")
                    {
                        Search += " and a.class in('Primary')";
                    }
                    else if (adminLoginSession.ActionRight == "5,8" || adminLoginSession.ActionRight == "8,5")
                    {
                        Search += " and a.class in('Primary','Middle')";
                    }

                    am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirm(Search, CrType, pageIndex);
                    ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                    if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(am);
                    }
                    else
                    {
                        ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                        ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                        int tp = Convert.ToInt32(ViewBag.TotalCount);
                        int pn = tp / 30;
                        int cal = 30 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(am);
                    }
                }

                #endregion Search Record                   
                return View(am);

            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [AdminLoginCheckFilter]
        public ActionResult FirmSchoolCorrectionUpdated(int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {
               
                var itemFee = new SelectList(new[] { new { ID = "1", Name = "With Fee" }, new { ID = "2", Name = "Without Fee" }, }, "ID", "Name", 1);
                ViewBag.FeeType = itemFee.ToList();
                ViewBag.SelectedFeeType = "1";
                
                var itemsch = new SelectList(new[]{new {ID="1",Name="Particular"},new {ID="2",Name="Subject"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();


               
               
                if (ModelState.IsValid)
                {
                    string Search = string.Empty;
                    string CrType = "2";
                    Search = "a.status is not null";                  
                    Search += " and a.FirmUser  = '" + adminLoginSession.USER.ToString() + "'";
                    if (adminLoginSession.Dist_Allow != "")
                    {
                        Search += " and s.DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                    }

                    //------ Paging Srt
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;
                    //---- Paging end
                    am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirmUpdated(Search, CrType, pageIndex);                  

                    if (am.StoreAllData == null)
                    {
                        ViewBag.TotalCountP = 0;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(am);
                    }
                    else
                    {
                        DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, adminLoginSession.USER, "");// check fee exist 
                        ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                        if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.IsFeeExists = "0";
                        }
                        else
                        { ViewBag.IsFeeExists = "1"; }
                        //
                        if (am.StoreAllData.Tables[2].Rows.Count > 0)
                        {
                            ViewBag.CorrectionFeeDate = am.StoreAllData.Tables[2].Rows[0]["CorrectionFeeDate"].ToString();
                            ViewBag.CorrectionFeeDateStatus = am.StoreAllData.Tables[2].Rows[0]["CorrectionFeeDateStatus"].ToString();
                        }

                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.TotalCountP = 0;
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                            ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);
                            Session["ForFinalCorrectionLot"] = String.Join(",", am.StoreAllData.Tables[3].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                            int tp = Convert.ToInt32(ViewBag.TotalCount);
                            int pn = tp / 30;
                            int cal = 30 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(am);
                        }

                    }

                }
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
               
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult FirmSchoolCorrectionUpdated(int? page, FormCollection frc, string cmd)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {                
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Particular" }, new { ID = "2", Name = "Subject" }, }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();

                var itemFee = new SelectList(new[] { new { ID = "1", Name = "With Fee" }, new { ID = "2", Name = "Without Fee" }, }, "ID", "Name", 1);
                ViewBag.FeeType = itemFee.ToList();
                ViewBag.SelectedFeeType = "1";

                //------ Paging Srt
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                #region Submit Fee  Only
                // if (cmd == "Submit Fee")
                if (cmd.ToLower() == "submit fee only")
                {

                    if (Session["ForFinalCorrectionLot"] == null)
                    { ViewData["FeeUpdate"] = "10"; }

                    string ForFinalCorrectionLot = Session["ForFinalCorrectionLot"].ToString();

                    ViewBag.ForFinalCorrectionLot = am.CorrectionLot = ForFinalCorrectionLot;
                    am.CorrectionRecieptNo = frc["CorrectionRecieptNo"].ToString();
                    am.CorrectionRecieptDate = frc["CorrectionRecieptDate"].ToString();
                    am.CorrectionNoCapproved = frc["CorrectionNoCapproved"].ToString();
                    am.CorrectionAmount = frc["CorrectionAmount"].ToString();

                    if (am.CorrectionRecieptNo != "")
                    {
                        DataSet dsFeeUpdate =_correctionPerformaRepository.SetCorrectionDataFirmFeeDetails(am, adminLoginSession.USER);
                        if (dsFeeUpdate == null || dsFeeUpdate.Tables[0].Rows.Count == 0)
                        {
                            ViewData["FeeUpdate"] = null;
                        }
                        else
                        {
                            ViewData["FeeUpdate"] = dsFeeUpdate.Tables[0].Rows[0]["InsUpt"].ToString();
                        }
                    }
                    else { ViewData["FeeUpdate"] = null; }
                }

                #endregion Submit Fee  Only



                #region Submit Fee & Final Submission
                // if (cmd == "Submit Fee")
                if (cmd.ToLower().Contains("final"))
                {
                    if (Session["ForFinalCorrectionLot"] == null)
                    { ViewData["FeeUpdate"] = "10"; }

                    string ForFinalCorrectionLot = Session["ForFinalCorrectionLot"].ToString();
                    ViewBag.ForFinalCorrectionLot = am.CorrectionLot = ForFinalCorrectionLot;
                    am.CorrectionRecieptNo = frc["CorrectionRecieptNo"].ToString();
                    am.CorrectionRecieptDate = frc["CorrectionRecieptDate"].ToString();
                    am.CorrectionNoCapproved = frc["CorrectionNoCapproved"].ToString();
                    am.CorrectionAmount = frc["CorrectionAmount"].ToString();

                    if (am.CorrectionRecieptNo != "")
                    {
                        DataSet dsFeeUpdate =_correctionPerformaRepository.SetCorrectionDataFirmFeeDetails(am, adminLoginSession.USER);
                        if (dsFeeUpdate == null || dsFeeUpdate.Tables[0].Rows.Count == 0)
                        {
                            ViewData["FeeUpdate"] = null;
                        }
                        else
                        {

                            ViewData["FeeUpdate"] = dsFeeUpdate.Tables[0].Rows[0]["InsUpt"].ToString();
                            // fee submit and final submit
                            if (dsFeeUpdate.Tables[0].Rows[0]["InsUpt"].ToString() == "1" || dsFeeUpdate.Tables[0].Rows[0]["InsUpt"].ToString() == "2")
                            {

                                DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(6, adminLoginSession.USER, ForFinalCorrectionLot);// check fee exist 
                                ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                                if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                                {
                                    ViewData["FeeUpdate"] = "5";
                                    ViewBag.commaCorrectionLot = ForFinalCorrectionLot;
                                }
                                else
                                {
                                    // final submit here   
                                    string FirmCorrectionLot = string.Empty;
                                    string OutError = string.Empty;
                                    DataSet ds1 =_correctionPerformaRepository.CorrectionDataFirmFinalSubmitSPRNByCorrectionLot(ForFinalCorrectionLot, adminLoginSession.USER, out FirmCorrectionLot, out OutError);  // Final Submit Main Function                       
                                    if (FirmCorrectionLot.Length > 2)
                                    {
                                        ViewBag.TotalCount = 1;
                                        ViewData["Status"] = "1";
                                        ViewData["FeeUpdate"] = "20";
                                        ViewData["Message"] = "";
                                    }
                                    else
                                    {
                                        ViewBag.TotalCount = 0;
                                        ViewData["Status"] = "0";
                                        ViewData["FeeUpdate"] = "21";
                                        ViewData["Message"] = OutError;
                                    }

                                }
                            }
                            //

                        }
                    }
                    else { ViewData["FeeUpdate"] = null; }
                }

                #endregion Submit Fee & Final Submission



                //---- Paging end                   
                #region View All Correction Pending Record                
                string Search = string.Empty;
                string CrType = "2";
                Search = "a.status is not null";

                if (frc["CorrectionLot"] != null)
                {
                    ViewBag.CorrectionLot = am.CorrectionLot = frc["CorrectionLot"].ToString();
                    Search += " and a.correctionlot ='" + am.CorrectionLot + "' ";

                }

                if (frc["FeeType"] != null)
                {
                    ViewBag.SelectedFeeType = frc["FeeType"].ToString().Trim();
                }

                Search += " and a.FirmUser  ='" + adminLoginSession.USER.ToString() + "'";

                //Search = "a.status is null and a.CorrectionLot= '"+am.CorrectionLot+"'";
                if (adminLoginSession.Dist_Allow != "")
                {
                    Search += " and s.DIST in(" + adminLoginSession.Dist_Allow.ToString() + ")";
                }
                am.StoreAllData =_correctionPerformaRepository.GetCorrectionDataFirmUpdated(Search, CrType, pageIndex);
                //

                if (am.StoreAllData == null)
                {
                    ViewBag.TotalCountP = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(am);
                }
                else
                {

                    DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, adminLoginSession.USER, "");// check fee exist 
                    ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                    if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.IsFeeExists = "0";
                    }
                    else
                    { ViewBag.IsFeeExists = "1"; }
                    //
                    if (am.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CorrectionFeeDate = am.StoreAllData.Tables[2].Rows[0]["CorrectionFeeDate"].ToString();
                        ViewBag.CorrectionFeeDateStatus = am.StoreAllData.Tables[2].Rows[0]["CorrectionFeeDateStatus"].ToString();
                    }

                    if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.TotalCountP = 0;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["totalCount"].ToString());
                        ViewBag.TotalCount1 = Convert.ToInt32(ViewBag.TotalCountP);

                        if (frc["CorrectionLot"] != null)
                        {
                            ViewBag.FeeStatus = Convert.ToInt32(am.StoreAllData.Tables[0].Rows[0]["FeeStatus"].ToString());
                            ViewBag.FirmCorrectionLot = Convert.ToString(am.StoreAllData.Tables[0].Rows[0]["FirmCorrectionLot"].ToString());

                        }
                        else
                        {
                            ViewBag.FeeStatus = null;
                            ViewBag.FirmCorrectionLot = null;
                        }

                        Session["ForFinalCorrectionLot"] = String.Join(",", am.StoreAllData.Tables[3].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                        int tp = Convert.ToInt32(ViewBag.TotalCount);
                        int pn = tp / 30;
                        int cal = 30 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                    }
                }

                #endregion View All Correction Pending Record

                return View(am);

            }
            catch (Exception ex)
            {
                return View();
            }
        }



        [AdminLoginCheckFilter]
        public ActionResult FirmSchoolCorrectionFinalSubmit(string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {
                
                if (id == null)
                {
                    return RedirectToAction("Index", "Admin");
                }                
             
                var itemsch = new SelectList(new[]{new {ID="1",Name="Particular"},new {ID="2",Name="Subject"},}, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();              
                
                if (ModelState.IsValid)
                {
                    if (id.ToLower() == "finalsubmit")
                    {
                        DataSet dschk =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(1, adminLoginSession.USER, "");// check fee exist 
                        ViewBag.TotalCount = 0;//am.StoreAllData.Tables[0].Rows.Count;
                        if (dschk == null || dschk.Tables[0].Rows.Count == 0)
                        {
                            // final submit here   

                            string FirmCorrectionLot = string.Empty;
                            string OutError = string.Empty;
                            DataSet ds1 =_correctionPerformaRepository.CorrectionDataFirmFinalSubmitSPRN(adminLoginSession.USER, out FirmCorrectionLot, out OutError);  // Final Submit Main Function                       
                            if (FirmCorrectionLot.Length > 2)
                            {
                                ViewBag.TotalCount = 1;
                                TempData["Status"] = "1";
                                ViewData["Message"] = "";

                                am.StoreAllData =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, adminLoginSession.USER, "");
                                if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                                {
                                    ViewBag.TotalCount = 0;
                                    TempData["Status"] = "10";
                                }
                                else
                                {

                                    ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                                    TempData["Status"] = "11";
                                }

                            }
                            else
                            {
                                ViewBag.TotalCount = 0;
                                TempData["Status"] = "0";
                                ViewData["Message"] = OutError;
                            }                         
                            return View(am);

                        }
                        else
                        {
                            ViewBag.commaCorrectionLot = String.Join(",", dschk.Tables[0].AsEnumerable().Select(x => x.Field<string>("CorrectionLot").ToString()).ToArray());
                            ViewBag.TotalCount = 1;
                            TempData["Status"] = "5";

                        }
                    }
                    else if (id.ToLower() == "viewall")
                    {
                        am.StoreAllData =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, adminLoginSession.USER, "");
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.TotalCount = 0;
                            TempData["Status"] = "10";
                        }
                        else
                        {

                            ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                            TempData["Status"] = "11";
                        }
                    }

                    else if (id.ToLower().Contains("pendingfee"))
                    {
                        am.StoreAllData =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(7, adminLoginSession.USER, "");
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.TotalCount = 0;
                            TempData["Status"] = "10";
                        }
                        else
                        {

                            ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;

                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                wb.Worksheets.Add(am.StoreAllData.Tables[0]);
                                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                wb.Style.Font.Bold = true;
                                Response.Clear();
                                Response.Buffer = true;
                                Response.Charset = "";
                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.AddHeader("content-disposition", "attachment;filename=" + "PendingFee_CorrectionLotList.xls");
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
                    return View(am);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult DownloadFirmCorrectionLot()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {
                if (Request.QueryString["File"] == null)
                {
                    return RedirectToAction("FirmSchoolCorrectionFinalSubmit", "Admin");
                }
                else
                {
                    string FileExport = Request.QueryString["File"].ToString();
                    DataSet ds = null;
                    if (adminLoginSession.USER != null)
                    {
                        //string UserName = Session["UserName"].ToString();
                        //string AdminType = Session["AdminType"].ToString();
                        //int AdminId = Convert.ToInt32(Session["AdminId"].ToString());
                        string fileName1 = string.Empty;
                        string Search = string.Empty;                     
                        ds =_correctionPerformaRepository.CheckFeeAllCorrectionDataByFirmSP(5, adminLoginSession.USER, FileExport);                               
                        if (ds == null)
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                bool ResultDownload;
                                try
                                {
                                    using (XLWorkbook wb = new XLWorkbook())
                                    {
                                        ////// wb.Worksheets.Add("PNB-TTAmarEN");//PNB-TTAmarEN for Punjabi                                               
                                        wb.Worksheets.Add(ds);
                                        wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        wb.Style.Font.Bold = true;
                                        Response.Clear();
                                        Response.Buffer = true;
                                        Response.Charset = "";
                                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + "");
                                        //string style = @"<style> .textmode {PNB-TTAmarEN:\@; } </style>";
                                        //Response.Output.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
                                        //Response.Write(style);
                                        using (MemoryStream MyMemoryStream = new MemoryStream())
                                        {
                                            wb.SaveAs(MyMemoryStream);
                                            MyMemoryStream.WriteTo(Response.OutputStream);
                                            Response.Flush();
                                            Response.End();
                                        }
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
                return RedirectToAction("FirmSchoolCorrectionFinalSubmit", "CorrectionPerforma");
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return RedirectToAction("FirmSchoolCorrectionFinalSubmit", "CorrectionPerforma");
            }
        }

        #endregion  FirmSchoolCorrection


        #region Admin School Correction

        [AdminLoginCheckFilter]
        public ActionResult AdminSchoolCorrection()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            RegistrationModels rm = new RegistrationModels();
            rm.CorrectionPerformaModel = new CorrectionPerformaModel();
            try
            {               
                List<SelectListItem> itemsch = new List<SelectListItem>();
                itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });                
                rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();               
            }
            catch (Exception ex)
            {
                
            }
            return View(rm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult AdminSchoolCorrection(FormCollection frm, RegistrationModels rm, int? page, string cmd,string SCHL)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
          
            try
            {
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                List<SelectListItem> itemsch = new List<SelectListItem>();
                itemsch.Add(new SelectListItem { Text = "Primary", Value = "5" });
                itemsch.Add(new SelectListItem { Text = "Middle", Value = "8" });
                rm.CorrectionPerformaModel.CorrectionClassList = itemsch.ToList();


                //rm = new RegistrationModels();

                if (frm["SCHL"] == null)
                {
                    TempData["msg"] = "schl";
                    return View(rm);
                }
                ViewBag.SCHLstring = rm.SCHL = frm["SCHL"].ToString();



                if (ModelState.IsValid)
                {
                    string Search = "a.schl='" + rm.SCHL + "' and ";
                    string Std_id = frm["SearchString"];
                    switch (rm.CorrectionPerformaModel.Class)
                    {
                        case "5":
                            Search = "a.form_Name in ('F1','F2')  and a.std_id='" + Std_id + "'";
                            break;
                        case "8":
                            Search = "a.form_Name in ('A1','A2')  and a.std_id='" + Std_id + "'";
                            break;
                    }
                    ViewBag.Searchstring = frm["SearchString"];
                    int SelValueSch = 0;
                    if (rm.CorrectionPerformaModel.Class.ToString() == "")
                    {
                        SelValueSch = 1;
                        Search = "form_Name in ('')";
                    }
                    else
                    {
                        SelValueSch = Convert.ToInt32(rm.CorrectionPerformaModel.Class.ToString());
                    }
                    #region Add Record Region Begin
                    if (cmd == "Add Record")
                    {
                        rm.Std_id = Convert.ToInt32(frm["SearchString"]);
                        rm.Class = rm.CorrectionPerformaModel.Class;
                        rm.CorrectionPerformaModel.Correctiontype = frm["SelListField"];
                        // rm.CorrectionPerformaModel.oldVal = frm["oldVal"];
                        if (rm.CorrectionPerformaModel.Correctiontype == "Candi_Name_P" || rm.CorrectionPerformaModel.Correctiontype == "Father_Name_P" || rm.CorrectionPerformaModel.Correctiontype == "Mother_Name_P")
                        {
                            //rm.CorrectionPerformaModel.newVal = frm["newValP"];
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newValP;
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Caste")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValCaste"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "DOB")
                        {
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newValDOB;
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Gender")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValGender"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "CandStudyMedium")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["newValcorCSM"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Differently_Abled")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Differently_Abled"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "wantwriter")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["wantwriter"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Religion")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Relist"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Admission_Date")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Admission_Date"];
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Belongs_BPL")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["Belongs_BPL"].ToUpper();
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "Tehsil")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["SelAllTehsil"].ToUpper();
                        }
                        else if (rm.CorrectionPerformaModel.Correctiontype == "District")
                        {
                            rm.CorrectionPerformaModel.newVal = frm["SelAllDistrict"].ToUpper();
                        }
                        else
                        {
                            rm.CorrectionPerformaModel.newVal = rm.CorrectionPerformaModel.newVal;
                        }

                        rm.CorrectionPerformaModel.Remark = frm["Remark"];
                        ViewBag.SelectedItem = rm.CorrectionPerformaModel.Class;
                        ViewBag.Searchstring = frm["SearchString"];

                        //rm.CorrectionPerformaModel.Remark = frm["Remark"];
                        //ViewBag.SelectedItem = rm.CorrectionPerformaModel.Class;
                        //ViewBag.Searchstring = frm["SearchString"];
                        //rm.SCHL = loginSession.SCHL.ToString();


                        rm.UserId = adminLoginSession.USER;
                        rm.UserType= "admin";
                        DataSet result1 =_correctionPerformaRepository.InsertSchoolCorrectionAddJunior(rm);
                        if (result1 != null)
                        {
                            ViewData["Status"] = result1.Tables[0].Rows[0]["res"].ToString();
                            TempData["msg"] = result1.Tables[0].Rows[0]["msg"].ToString();
                        }
                        else
                        {
                            ViewData["Status"] = "-1";
                        }

                    }
                    #endregion Add Record

                    rm.StoreAllData =_correctionPerformaRepository.GetCorrectionStudentRecordsSearchJunior(Search, SelValueSch, pageIndex);
                    rm.CorrectionPerformaModel.Correctiondata =_correctionPerformaRepository.GetStudentRecordsCorrectionData(1, rm.SCHL, ""); // passing Value to DBClass from model
                    ViewBag.TotalCountadded = rm.CorrectionPerformaModel.Correctiondata.Tables[0].Rows.Count;


                    if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;

                        return View(rm);
                    }
                    else
                    {

                        ViewBag.TotalCountSearch = rm.StoreAllData.Tables[0].Rows.Count;
                        string std_Class = rm.StoreAllData.Tables[0].Rows[0]["form_Name"].ToString();

                        // Bind MAsters
                        ViewBag.MyAllTehsil = DBClass.GetAllTehsil();
                        ViewBag.MyAllDistrict = DBClass.GetDistE();


                        DataSet CorrectionField =_correctionPerformaRepository.getCorrrectionField(std_Class);
                        ViewBag.MySchField = CorrectionField.Tables[0];
                        List<SelectListItem> items = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in ViewBag.MySchField.Rows)
                        {
                            items.Add(new SelectListItem { Text = @dr["CorrectionFieldDisplayName"].ToString(), Value = @dr["CorrectionFieldName"].ToString() });
                        }
                        ViewBag.MySchField = items.ToList();



                        ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCount1 = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        int tp = Convert.ToInt32(rm.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        int pn = tp / 30;
                        int cal = 30 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;
                        return View(rm);
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        #endregion Admin School Correction


       


        #endregion

    }
}