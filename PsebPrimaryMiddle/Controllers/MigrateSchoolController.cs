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
    public class MigrateSchoolController : Controller
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMigrateSchoolRepository _migrateSchoolRepository;
        private readonly IAdminRepository _adminRepository;

        public MigrateSchoolController(ISchoolRepository schoolRepository,IMigrateSchoolRepository migrateSchoolRepository, IAdminRepository adminRepository)
        {
            _schoolRepository = schoolRepository;
            _migrateSchoolRepository = migrateSchoolRepository;
            _adminRepository = adminRepository;
        }

        
        [AdminLoginCheckFilter]
        public ActionResult Index()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                AdminModels MS = new AdminModels();


                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsREG = 1; }
                else
                {
                    ViewBag.IsREG = 0;
                }
                #endregion Action Assign Method
                List<SelectListItem> items = new List<SelectListItem>();


                // Dist Allowed
                string DistAllow = adminLoginSession.Dist_Allow;
                //
                ViewBag.MyDist = _adminRepository.getAdminDistAllowList("admin", adminLoginSession.AdminId.ToString());

                var itemsch = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = "School ID" } }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();

                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult Index(FormCollection frm, string SelDist)
        {
            AdminModels MS = new AdminModels();
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {

                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsREG = 1; }
                else
                {
                    ViewBag.IsREG = 0;
                    }
                #endregion Action Assign Method                

                //List<SelectListItem> items = new List<SelectListItem>();               
                // Dist Allowed
                string DistAllow = adminLoginSession.Dist_Allow;
                //
                ViewBag.MyDist = _adminRepository.getAdminDistAllowList("admin", adminLoginSession.AdminId.ToString());

                // End Dist Allowed


                var itemsch = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = "School ID" } }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                if (ModelState.IsValid)
                {
                    string Search = string.Empty;
                    if (SelDist != "")
                    {
                        Search = "District='" + SelDist + "' ";
                    }
                    else
                    {

                        Search = "District like '%%' ";
                    }
                    if (frm["SelList"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());
                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and SCHL='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and IDNO='" + frm["SearchString"].ToString() + "'"; }
                        }
                    }

                    MS.StoreAllData = _schoolRepository.SchoolMasterViewSP(4, "", Search);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                return View();
            }
            return View(MS);
        }



        #region ViewAllCandidatesOfSchool
        [AdminLoginCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ViewAllCandidatesOfSchool(MigrateSchoolModels MS, string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
            string SCHL = id;

            var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();
            try
            {
                DataSet result1 = _adminRepository.GetAllFormNameBySchl(id); 
                ViewBag.MyForm = result1.Tables[0];
                List<SelectListItem> FormList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
                {
                    FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
                }
                ViewBag.MyForm = FormList;

                ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;
                if (id != null)
                {
                    string Search = string.Empty;
                    Search = "SCHL='" + id + "'";
                    MS.StoreAllData = _migrateSchoolRepository.ViewAllCandidatesOfSchool(SCHL, Search);
                    ViewBag.schlCode = MS.StoreAllData.Tables[1].Rows[0]["schlCode"].ToString();
                    ViewBag.schlID = MS.StoreAllData.Tables[1].Rows[0]["schlID"].ToString();
                    ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                        MS.idno = MS.StoreAllData.Tables[0].Rows[0]["Std_id"].ToString();
                        MS.SchlCode = MS.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                        MS.Sno = MS.StoreAllData.Tables[0].Rows[0]["Class_Roll_Num_Section"].ToString();
                        MS.RegNo = MS.StoreAllData.Tables[0].Rows[0]["Registration_num"].ToString();
                        MS.FormName = MS.StoreAllData.Tables[0].Rows[0]["form_Name"].ToString();
                        MS.Candi_Name = MS.StoreAllData.Tables[0].Rows[0]["Candi_Name"].ToString();
                        MS.Father_Name = MS.StoreAllData.Tables[0].Rows[0]["Father_Name"].ToString();
                        MS.Mother_Name = MS.StoreAllData.Tables[0].Rows[0]["Mother_Name"].ToString();
                        MS.DOB = MS.StoreAllData.Tables[0].Rows[0]["DOB"].ToString();
                        MS.Gender = MS.StoreAllData.Tables[0].Rows[0]["Gender"].ToString();                     
                        MS.AdmDate = MS.StoreAllData.Tables[0].Rows[0]["Admission_Date"].ToString();
                        MS.Fee = MS.StoreAllData.Tables[0].Rows[0]["REGFEE"].ToString();
                        MS.Lot = MS.StoreAllData.Tables[0].Rows[0]["LOT"].ToString();
                        MS.Std_Sub = MS.StoreAllData.Tables[0].Rows[0]["StdSub"].ToString();                      
                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {                
                return RedirectToAction("Index", "MigrateSchool");
            }

            return View();
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ViewAllCandidatesOfSchool(MigrateSchoolModels MS, string id,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "MigrateSchool");
            }

            var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();


            string SCHL = MS.SchlCode =  id;          
           DataSet result1 = _adminRepository.GetAllFormNameBySchl(id);
             
            ViewBag.MyForm = result1.Tables[0];
            List<SelectListItem> FormList = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
            {
                FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
            }
            ViewBag.MyForm = FormList;
            ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;             

                MS.SelForm = frm["SelForm"].ToString();
                MS.SelLot = frm["SelLot"].ToString();
                ViewBag.SelLot = frm["SelLot"].ToString();
            MS.SelFilter = frm["SelFilter"].ToString();          
            
     
            if (id != null && id != "")
            {
                string Search = string.Empty;
                Search = "SCHL='" + id + "' ";
                if (frm["SelForm"] != "")
                {
                    Search += " and form_name='" + frm["SelForm"].ToString() + "' ";
                }
                if (frm["SelLot"] != "")
                {
                    Search += " and LOT='" + frm["SelLot"].ToString() + "' ";
                }
                else
                {
                    Search += " and LOT >0 ";
                }
                if (frm["SelFilter"] != "")
                {
                    int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                    if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                    {                      
                        if (SelValueSch == 1)
                        { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 2)
                        { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 3)
                        { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 4)
                        { Search += " and  Father_Name  like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 5)
                        { Search += " and Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 6)
                        { Search += " and DOB='" + frm["SearchString"].ToString() + "'"; }
                    }
                }


                MS.StoreAllData = _migrateSchoolRepository.ViewAllCandidatesOfSchool(SCHL,Search);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.schlID = ViewBag.schlCode = id;                    
                    ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();

                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.schlCode = MS.StoreAllData.Tables[1].Rows[0]["schlCode"].ToString();
                    ViewBag.schlID = MS.StoreAllData.Tables[1].Rows[0]["schlID"].ToString();
                    ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();

                    MS.idno = MS.StoreAllData.Tables[0].Rows[0]["Std_id"].ToString();
                    MS.SchlCode = MS.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                    MS.Sno = MS.StoreAllData.Tables[0].Rows[0]["Class_Roll_Num_Section"].ToString();
                    MS.RegNo = MS.StoreAllData.Tables[0].Rows[0]["Registration_num"].ToString();
                    MS.FormName = MS.StoreAllData.Tables[0].Rows[0]["form_Name"].ToString();
                    MS.Candi_Name = MS.StoreAllData.Tables[0].Rows[0]["Candi_Name"].ToString();
                    MS.Father_Name = MS.StoreAllData.Tables[0].Rows[0]["Father_Name"].ToString();
                    MS.Mother_Name = MS.StoreAllData.Tables[0].Rows[0]["Mother_Name"].ToString();
                    MS.DOB = MS.StoreAllData.Tables[0].Rows[0]["DOB"].ToString();
                    MS.Gender = MS.StoreAllData.Tables[0].Rows[0]["Gender"].ToString();                   
                    MS.AdmDate = MS.StoreAllData.Tables[0].Rows[0]["Admission_Date"].ToString();
                    MS.Fee = MS.StoreAllData.Tables[0].Rows[0]["REGFEE"].ToString();
                    MS.Lot = MS.StoreAllData.Tables[0].Rows[0]["LOT"].ToString();
                    MS.Std_Sub = MS.StoreAllData.Tables[0].Rows[0]["StdSub"].ToString();

                    return View(MS);
                }
            }
            return View();
        }
        #endregion ViewAllCandidatesOfSchool


        #region Begin MigrationForm
        [AdminLoginCheckFilter]
        public ActionResult MigrationForm(string id, MigrateSchoolModels MS)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "MigrateSchool");
            }

            string stdid = id; 
            if (id != null)
            {                
                MS.StoreAllData = _migrateSchoolRepository.GetMigrationDataByStudentId(0, id);//GetMigrationForm_SP
                ViewBag.schlCode = MS.StoreAllData.Tables[1].Rows[0]["schlCode"].ToString();
                ViewBag.schlID = MS.StoreAllData.Tables[1].Rows[0]["schlID"].ToString();
                ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;                   
                    MS.idno = MS.StoreAllData.Tables[0].Rows[0]["Std_id"].ToString();
                    MS.SchlCode = MS.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                    MS.Sno = MS.StoreAllData.Tables[0].Rows[0]["Class_Roll_Num_Section"].ToString();
                    MS.RegNo = MS.StoreAllData.Tables[0].Rows[0]["Registration_num"].ToString();
                    MS.FormName = MS.StoreAllData.Tables[0].Rows[0]["form_Name"].ToString();
                  
                     MS.Class = MS.StoreAllData.Tables[0].Rows[0]["Class"].ToString();                  
                    MS.Candi_Name = MS.StoreAllData.Tables[0].Rows[0]["Candi_Name"].ToString();
                    MS.Father_Name = MS.StoreAllData.Tables[0].Rows[0]["Father_Name"].ToString();
                    MS.Mother_Name = MS.StoreAllData.Tables[0].Rows[0]["Mother_Name"].ToString();
                    MS.DOB = MS.StoreAllData.Tables[0].Rows[0]["DOB"].ToString();
                    MS.Gender = MS.StoreAllData.Tables[0].Rows[0]["Gender"].ToString();                  
                    MS.AdmDate = MS.StoreAllData.Tables[0].Rows[0]["Admission_Date"].ToString();
                    MS.Fee = MS.StoreAllData.Tables[0].Rows[0]["REGFEE"].ToString();
                    MS.Lot = MS.StoreAllData.Tables[0].Rows[0]["LOT"].ToString();
                    MS.Std_SubOld = MS.StoreAllData.Tables[0].Rows[0]["Group_Name"].ToString();
                    MS.Std_Sub = MS.StoreAllData.Tables[0].Rows[0]["Group_Name"].ToString();                   
                    return View(MS);
                }
            }
            else
            {
                ViewData["MigrateStatus"] = "1";
                return View();
            }
           
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult MigrationForm(string id, MigrateSchoolModels MS,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
            string stdid = id ;  
            if (id != null)
            {
                MS.StdId = id;
                MS.StdId = frm["idno"];
                MS.SchlCode = frm["SCHLCode"];
                MS.RegNo = frm["RegNo"];
                MS.Candi_Name = frm["Candi_Name"];
                MS.Father_Name = frm["Father_Name"];
                MS.Mother_Name = frm["Mother_Name"];
                MS.SchlCodeNew = frm["SchlCodeNew"];
                //MS.DistName = Session["DSTName"].ToString();
                MS.Std_Sub = "";
                //MS.rdoDD = frm["rdoDD"];
                //MS.rdoBrdRcpt = frm["rdoBrdRcpt"];
                MS.DDRcptNo = frm["DDRcptNo"];
                MS.Amount = frm["Amount"];
                MS.DepositDt = frm["DepositDt"];
                MS.BankName = frm["BankName"];

                MS.DiryOrderNo = frm["DiryOrderNo"];
                MS.OrderDt = frm["OrderDt"];
                MS.OrderBy = frm["OrderBy"];
                MS.Remark = frm["Remark"];
                MS.FormName = frm["FormName"];


                string scode = ViewBag.schlName = MS.SchlCode;
              //  MS.SchlName = ViewBag.schlName;
                MS.UserName = adminLoginSession.USER.ToString();


              
                DataSet result = _migrateSchoolRepository.Insert_MigrationForm(MS);
                if (result == null)
                {
                    ViewData["MigrateStatus"] = result;
                    return RedirectToAction("ViewAllCandidatesOfSchool", "MigrateSchool");
                }
                else
                {
                    string rr = result.Tables[0].Rows[0]["RESULT"].ToString();
                    // rr = 0 MIGRATE SCHL NOT TO BE SAME
                    // rr = 1  MIGRATE SCHL Std ALREADY
                    // rr = 2 MIGRATE SCHL SUCCESSFULLY
                    // rr = 5 MIGRATE SCHL Wrong
                    if (rr == "0" || rr == "1" || rr == "2" || rr == "5")
                    {
                        ViewData["StdID"] = id;
                        ViewData["MigrateStatus"] = rr;
                        return View();                        
                    }
                }
            }
            return View(frm);
        }
        #endregion

        #region  Migration REC
        [AdminLoginCheckFilter]
        public ActionResult MigrationRec(MigrateSchoolModels MS)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            var itemsch = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = "Candidate ID" }, new { ID = "3", Name = "Candidate Name" }, new { ID = "4", Name = "Father Name" }, new { ID = "5", Name = "Mother Name" }, }, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();
            try
            {
                string DistAllow = adminLoginSession.Dist_Allow;
                ViewBag.MyDist = _adminRepository.getAdminDistAllowList("admin", adminLoginSession.AdminId.ToString());

                string Search = string.Empty;
                Search = "smm.DIST like '%%' ";
                MS.StoreAllData = _migrateSchoolRepository.GetMigrationDataByIdandSearch(1, "", Search);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;                   
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;                    
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
            return View(MS);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult MigrationRec(MigrateSchoolModels MS, FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            var itemsch = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = "Candidate ID" }, new { ID = "3", Name = "Candidate Name" }, new { ID = "4", Name = "Father Name" }, new { ID = "5", Name = "Mother Name" }, }, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();

            try
            {

                string DistAllow = adminLoginSession.Dist_Allow;
                ViewBag.MyDist = _adminRepository.getAdminDistAllowList("admin", adminLoginSession.AdminId.ToString());


                MS.SelDist = frm["SelDist"].ToString();
                MS.SelList = frm["SelList"].ToString();


                string Search = string.Empty;
                if (MS.SelDist != "")
                {
                    Search = "smm.DIST='" + MS.SelDist + "' ";
                }
                else
                {
                    Search = "smm.DIST like '%%' ";
                }
                if (frm["SelList"] != "")
                {
                    ViewBag.SelectedItem = frm["SelList"];
                    int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());
                    if (frm["SearchString"] != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and smm.SchlCodeNew='" + frm["SearchString"].ToString() + "'"; }
                        if (SelValueSch == 2)
                        { Search += " and smm.StdId='" + frm["SearchString"].ToString() + "'"; }
                        if (SelValueSch == 3)
                        { Search += " and smm.Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        if (SelValueSch == 4)
                        { Search += " and smm.Father_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        if (SelValueSch == 5)
                        { Search += " and smm.Mother_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                    }
                }
                else
                {
                    MS.SearchString = string.Empty;
                }
                MS.StoreAllData = _migrateSchoolRepository.GetMigrationDataByIdandSearch(1, "", Search);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    MS.SearchString = string.Empty;
                    ViewBag.TotalCount = 0;                  
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;                
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
            return View(MS);
        }


        #endregion

        [AdminLoginCheckFilter]
        public ActionResult MigrationDetailView(string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];            
            try
            {
                MigrateSchoolModels MS = new MigrateSchoolModels();
              
                string Search = string.Empty;
                Search = "smm.ID= '" + id + "'";               
                MS.StoreAllData = _migrateSchoolRepository.GetMigrationDataByIdandSearch(1, id,Search);//SelectMigrateSchools_sp
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    MS.SearchString = string.Empty;
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    MS.SchlCode = MS.StoreAllData.Tables[0].Rows[0]["SchlCode"].ToString();
                    MS.idno = MS.StoreAllData.Tables[0].Rows[0]["StdId"].ToString();
                    MS.RegNo = MS.StoreAllData.Tables[0].Rows[0]["RegNo"].ToString();
                    MS.Candi_Name = MS.StoreAllData.Tables[0].Rows[0]["Candi_Name"].ToString();
                    MS.Father_Name = MS.StoreAllData.Tables[0].Rows[0]["Father_Name"].ToString();
                    MS.Mother_Name = MS.StoreAllData.Tables[0].Rows[0]["Mother_Name"].ToString();
                    MS.SchlCodeNew = MS.StoreAllData.Tables[0].Rows[0]["SchlCodeNew"].ToString();
                   
                    MS.DDRcptNo = MS.StoreAllData.Tables[0].Rows[0]["DDRcptNo"].ToString();
                    MS.Amount = MS.StoreAllData.Tables[0].Rows[0]["Amount"].ToString();
                    MS.DepositDt = MS.StoreAllData.Tables[0].Rows[0]["DepositDt"].ToString();
                    MS.BankName = MS.StoreAllData.Tables[0].Rows[0]["BankName"].ToString();

                    MS.MigrateNo = MS.StoreAllData.Tables[0].Rows[0]["ID"].ToString();
                    MS.DiryOrderNo = MS.StoreAllData.Tables[0].Rows[0]["DiryOrderNo"].ToString();
                    MS.OrderDt = MS.StoreAllData.Tables[0].Rows[0]["OrderDt"].ToString();
                    MS.OrderBy = MS.StoreAllData.Tables[0].Rows[0]["OrderBy"].ToString();
                    MS.Remark = MS.StoreAllData.Tables[0].Rows[0]["Remark"].ToString();

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "MigrateSchool");
            }          
        }


        [AdminLoginCheckFilter]
        public ActionResult MigrationPrint(string id, MigrateSchoolModels MS)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];            
            try
            {
                MS.StoreAllData = _migrateSchoolRepository.GetMigrationDataByStudentId(1, id);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    MS.SearchString = string.Empty;
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    MS.MigrateNo = MS.StoreAllData.Tables[0].Rows[0]["ID"].ToString();
                    MS.RegSet = MS.StoreAllData.Tables[0].Rows[0]["RegSet"].ToString();
                    MS.Migrationdate = MS.StoreAllData.Tables[0].Rows[0]["MigrationDate"].ToString();

                    MS.SchlCode = MS.StoreAllData.Tables[0].Rows[0]["SchlCode"].ToString();
                    MS.newSchlDetail = MS.StoreAllData.Tables[0].Rows[0]["newSchlDetail"].ToString();
                    MS.newForm = MS.StoreAllData.Tables[0].Rows[0]["Form"].ToString();
                    if (MS.newForm == "F1" || MS.newForm == "F2")
                    {
                        MS.newForm = "5th";
                    }
                    else if (MS.newForm == "A1" || MS.newForm == "A2")
                    {
                        MS.newForm = "8th";
                    }                   

                    MS.DistNameP = MS.StoreAllData.Tables[0].Rows[0]["DISTNMP"].ToString();
                    MS.SchlCodeNew = MS.StoreAllData.Tables[0].Rows[0]["SchlCodeNew"].ToString();
                    MS.OldSchlDetail = MS.StoreAllData.Tables[0].Rows[0]["OldSchlDetail"].ToString();
                    MS.idno = MS.StoreAllData.Tables[0].Rows[0]["StdId"].ToString();
                    MS.RegNo = MS.StoreAllData.Tables[0].Rows[0]["RegNo"].ToString();
                    MS.Candi_Name = MS.StoreAllData.Tables[0].Rows[0]["Candi_Name"].ToString();
                    MS.Father_Name = MS.StoreAllData.Tables[0].Rows[0]["Father_Name"].ToString();
                    MS.Mother_Name = MS.StoreAllData.Tables[0].Rows[0]["Mother_Name"].ToString();                 

                    MS.Pname = MS.StoreAllData.Tables[0].Rows[0]["Pname"].ToString();
                    MS.PFname = MS.StoreAllData.Tables[0].Rows[0]["PFname"].ToString();
                    MS.PMname = MS.StoreAllData.Tables[0].Rows[0]["PMname"].ToString();

                    MS.DDRcptNo = MS.StoreAllData.Tables[0].Rows[0]["DDRcptNo"].ToString();
                    MS.Amount = MS.StoreAllData.Tables[0].Rows[0]["Amount"].ToString();
                    MS.DepositDt = MS.StoreAllData.Tables[0].Rows[0]["DepositDt"].ToString();
                    MS.BankName = MS.StoreAllData.Tables[0].Rows[0]["BankName"].ToString();

                    MS.MigrateNo = MS.StoreAllData.Tables[0].Rows[0]["ID"].ToString();
                    MS.DiryOrderNo = MS.StoreAllData.Tables[0].Rows[0]["DiryOrderNo"].ToString();
                    MS.OrderDt = MS.StoreAllData.Tables[0].Rows[0]["OrderDt"].ToString();
                    MS.OrderBy = MS.StoreAllData.Tables[0].Rows[0]["OrderBy"].ToString();
                    MS.Remark = MS.StoreAllData.Tables[0].Rows[0]["Remark"].ToString();

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult MigrationDelete(string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                if (id == null)
                {
                    return RedirectToAction("MigrationRec", "MigrateSchool");
                }
                else
                {
                    string result = _migrateSchoolRepository.DeleteMigrationData(id); // passing Value to DBClass from model                    

                    ViewData["MigrateDeleteStatus"] = result;
                    return RedirectToAction("MigrationRec", "MigrateSchool");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "MigrateSchool");
            }
        }


    }
}