using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.AbstractLayer;
using System.Threading.Tasks;
using System.IO;
using PsebPrimaryMiddle.Filters;
using PsebPrimaryMiddle.Repository;

namespace PsebPrimaryMiddle.Controllers
{
    [AdminMenuFilter]
    public class SchoolProfileController : Controller
    {
        private readonly ISchoolRepository _schoolRepository;

        public SchoolProfileController(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }


        // GET: SchoolProfile
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetBankDetailsByIFSC(string IFSC)
        {            
            string outid = "0";        
            DataSet ds = DBClass.GetBankNameList(2, "", IFSC);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    outid = "1";                    
                }
            }
            return Json(new { bank = ds.Tables[0].Rows[0]["BANK"].ToString(), br = ds.Tables[0].Rows[0]["BRANCH"].ToString().Trim(),
                add = ds.Tables[0].Rows[0]["address"].ToString().Trim(), dist = ds.Tables[0].Rows[0]["district"].ToString().Trim(),
                bankid = ds.Tables[0].Rows[0]["BANKID"].ToString().Trim(), oid = outid }, JsonRequestBehavior.AllowGet);
        }

        [SessionCheckFilter]
        public ActionResult Update_School_Information()
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                //ViewBag.BankList
                List<SelectListItem> objBankList = new List<SelectListItem>();
                DataSet dsBankData = DBClass.GetBankNameList(0, "", "");
                if (dsBankData.Tables.Count > 0)
                {
                    if (dsBankData.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in dsBankData.Tables[0].Rows)
                        {
                            objBankList.Add(new SelectListItem { Text = dr["Bank"].ToString(), Value = dr["Bank"].ToString() });
                        }

                    }
                }
                ViewBag.BankList = objBankList;
                //

                DataSet result1 = DBClass.SelectAllTehsil(loginSession.DIST);
                ViewBag.MyTeh = result1.Tables[0];
                List<SelectListItem> TehList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyTeh.Rows)
                {

                    TehList.Add(new SelectListItem { Text = @dr["TEHSIL"].ToString(), Value = @dr["TCODE"].ToString() });

                }
                ViewBag.MyTeh = TehList;


                //bind block by dist
                DataSet result = DBClass.SelectBlock(loginSession.DIST);
                ViewBag.MyEdublock = result.Tables[0];
                List<SelectListItem> BlockList = new List<SelectListItem>();
                //BlockList.Add(new SelectListItem { Text = "---Edu Block---", Value = "0" });
                foreach (System.Data.DataRow dr in ViewBag.MyEdublock.Rows)
                {
                    BlockList.Add(new SelectListItem { Text = @dr["Edu Block Name"].ToString(), Value = @dr["Edu Block Name"].ToString() });
                }
                ViewBag.MyEdublock = BlockList;

                List<SelectListItem> EduClusterList = new List<SelectListItem>();
                EduClusterList.Add(new SelectListItem { Text = "---Edu Cluster---", Value = "0" });
                ViewBag.MyEduCluster = EduClusterList;

                //new            
               // ViewBag.SubUserTypeList = DBClass.GetSubUserType();
                ViewBag.EstalimentYearList = DBClass.GetEstalimentYearList();
                //



                ViewBag.AREAList = DBClass.GetArea();
                // YesNo 
                ViewBag.YesNoList = DBClass.GetYesNo();
                // Status
                ViewBag.StatusList = DBClass.GetStatus();
                // Session 
                ViewBag.SessionList = DBClass.GetSessionAll(); //ViewBag.SessionList = DBClass.GetSessionAdmin();
                // Class 
                ViewBag.ClassTypeList = DBClass.GetClass();
                // School 
                ViewBag.SchoolTypeList = DBClass.GetSchool();
                // English Dist 

                ViewBag.DistEList = DBClass.GetDistE();
                // Punjabi   Dist         
                ViewBag.DistPList = DBClass.GetDistP();

                DataSet ds = new DataSet();
                SchoolModels sm = _schoolRepository.GetSchoolDataBySchl(loginSession.SCHL, out ds);
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    ViewData["resultUSI"] = 2;
                    return View();
                }
                else
                {
                    ViewBag.Bank = sm.Bank;
                    ViewBag.ACNO = sm.acno;
                    ViewBag.NSQF_flag = sm.NSQF_flag;
                    //

                    DataSet Eduresult = DBClass.Select_CLUSTER_NAME(sm.Edublock);
                    ViewBag.MyEduCluster = Eduresult.Tables[0];
                    foreach (System.Data.DataRow dr in ViewBag.MyEduCluster.Rows)
                    {
                        EduClusterList.Add(new SelectListItem { Text = @dr["CLUSTER_NAME"].ToString(), Value = @dr["CLUSTER_NAME"].ToString() });
                    }

                    ViewBag.MyEduCluster = EduClusterList;
                    ViewBag.ESTDYR = sm.SchlEstd;


                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        sm.CorrectionNoOld = ds.Tables[1].Rows[0]["CorrectionNoOld"].ToString();
                        sm.RemarksOld = ds.Tables[1].Rows[0]["RemarksOld"].ToString();
                        sm.RemarkDateOld = ds.Tables[1].Rows[0]["RemarkDateOld"].ToString();
                    }
                }

                return View(sm);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path)); 
                return View();
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public async Task<ActionResult> Update_School_Information(SchoolModels sm, string chkBank, string BankValue)
        {
            ViewBag.SessionList = DBClass.GetSessionAll();

            //new            
            // ViewBag.SubUserTypeList = DBClass.GetSubUserType();
            ViewBag.EstalimentYearList = DBClass.GetEstalimentYearList();
            //

            //ViewBag.BankList
            List<SelectListItem> objBankList = new List<SelectListItem>();
            DataSet dsBankData = DBClass.GetBankNameList(0, "", "");
            if (dsBankData.Tables.Count > 0)
            {
                if (dsBankData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsBankData.Tables[0].Rows)
                    {
                        objBankList.Add(new SelectListItem { Text = dr["Bank"].ToString(), Value = dr["Bank"].ToString() });
                    }

                }
            }
            ViewBag.BankList = objBankList;


            try
            {
                string DOB = sm.DOB;
                string DOJ = sm.DOJ;
                string ExperienceYr = sm.ExperienceYr;
                string PQualification = sm.PQualification;

                string Emailid = sm.EMAILID;
                string Mobile = sm.MOBILE;
             
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                sm.SCHL = loginSession.SCHL;                

                DataSet mresult1 = DBClass.SelectAllTehsil(loginSession.DIST);
                ViewBag.MyTeh = mresult1.Tables[0];
                List<SelectListItem> TehList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyTeh.Rows)
                {

                    TehList.Add(new SelectListItem { Text = @dr["TEHSIL"].ToString(), Value = @dr["TCODE"].ToString() });

                }
                ViewBag.MyTeh = TehList;

                //
                DataSet mresult = DBClass.SelectBlock(loginSession.DIST);
                ViewBag.MyEdublock = mresult.Tables[0];
                List<SelectListItem> BlockList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyEdublock.Rows)
                {
                    BlockList.Add(new SelectListItem { Text = @dr["Edu Block Name"].ToString(), Value = @dr["Edu Block Name"].ToString() });
                }
                ViewBag.MyEdublock = BlockList;

                List<SelectListItem> EduClusterList = new List<SelectListItem>();
                EduClusterList.Add(new SelectListItem { Text = "---Edu Cluster---", Value = "0" });
                ViewBag.MyEduCluster = EduClusterList;

                // Check A/c number and ifsc code

                if (!string.IsNullOrEmpty(sm.confirmacno))
                {

                    if (string.IsNullOrEmpty(sm.acno))
                    {
                        if (chkBank == null && BankValue == "ViewBank")
                        {
                            sm.acno = sm.confirmacno;
                        }
                        else
                        {
                            ViewData["resultUSI"] = 10;
                            return View(sm);

                        }

                    }
                    else if (sm.acno != sm.confirmacno)
                    {
                        ViewData["resultUSI"] = 11;
                        return View(sm);
                    }
                }

                if (!string.IsNullOrEmpty(sm.IFSC))
                {
                    DataSet dschkIFSC = DBClass.GetBankNameList(1, sm.Bank, sm.IFSC);
                    if (dschkIFSC.Tables == null || dschkIFSC.Tables[0].Rows.Count == 0)
                    {
                        ViewData["resultUSI"] = 12;
                        return View(sm);
                    }
                }
                //


                //#region Call API to update school master details
                //string apiStatus = "";
                //try
                //{

                //    sm.userip = StaticDB.GetFullIPAddress();

                //    apiStatus = await PsebAPIServiceDB.UpdateUSIPSEBJuniorToPSEBMain(sm);
                //    ViewBag.ApiStatus = apiStatus;
                //}
                //catch (Exception)
                //{
                //    ViewBag.ApiStatus = apiStatus;
                //}
                //#endregion

               // int result = apiStatus == "200" ? 1 : 0;
                int result = _schoolRepository.UpdateUSIJunior(sm); // passing Value to _schoolRepository.from model and Type 1 For regular
                if (result > 0)
                {
                    ViewData["resultUSI"] = 1;
                    ViewBag.Message = "Your school information is updated successfully. Your correction number is " + result;
                    return View(sm);

                }
                else
                {
                    ViewData["resultUSI"] = 0;
                    ModelState.AddModelError("", "Not Update");
                    return View();
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path)); 
                return View();
            }
        }

        [SessionCheckFilter]
        public ActionResult Change_Password()
        {           
            return View();
        }
        [SessionCheckFilter]
        [HttpPost]
        public async Task<ActionResult> Change_Password(FormCollection frm)
        {           
            string CurrentPassword = string.Empty;
            string NewPassword = string.Empty;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];


            if (frm["ConfirmPassword"] != "" && frm["NewPassword"] != "")
            {
                if (frm["ConfirmPassword"].ToString() == frm["NewPassword"].ToString())
                {
                    CurrentPassword = frm["CurrentPassword"].ToString();
                    NewPassword = frm["NewPassword"].ToString();


                    SchoolChangePasswordModel sm = new SchoolChangePasswordModel()
                    {
                        SCHL = loginSession.SCHL,
                        CurrentPassword = frm["CurrentPassword"].ToString(),
                        NewPassword = frm["NewPassword"].ToString()
                    };

                    //#region Call API to update school master details
                    //string apiStatus = "";
                    //try
                    //{
                    //    // apiStatus = await new AbstractLayer.PsebAPIServiceDB().SchoolChangePasswordPSEBJunior(sm);
                    //    apiStatus = await PsebAPIServiceDB.SchoolChangePasswordPSEBJunior(sm);
                    //    ViewBag.ApiStatus = apiStatus;
                    //}
                    //catch (Exception)
                    //{
                    //    ViewBag.ApiStatus = apiStatus;
                    //}
                    //#endregion

                  //  int result = apiStatus == "200" ? 1 : 0;

                   int result = _schoolRepository.SchoolChangePassword(loginSession.SCHL, CurrentPassword, NewPassword); 
                    if (result > 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewData["resultSCP"] = 0;
                        ModelState.AddModelError("", "Not Update");
                        return View();
                    }
                }
                else
                {
                    ViewData["resultSCP"] = 3;
                    ModelState.AddModelError("", "Fill All Fields");
                    return View();
                }
            }
            else
            {
                ViewData["resultSCP"] = 2;
                ModelState.AddModelError("", "Fill All Fields");
                return View();
            }
        }


    }
}