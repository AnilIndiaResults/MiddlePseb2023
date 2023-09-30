using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.Models;
using System.Data;
using System.IO;
using System.Web.Services;
using Newtonsoft.Json;
using System.Web.Routing;
using ClosedXML.Excel;
using System.Web.Caching;
using System.Web.UI;
using PsebJunior.AbstractLayer;
using System.Threading.Tasks;
using PsebPrimaryMiddle.Repository;
using PsebPrimaryMiddle.Filters;

namespace PsebPrimaryMiddle.Controllers
{
    [RoutePrefix("Master")]
    public class MasterController : Controller
    {
        private readonly DBContext _context = new DBContext();

        private readonly ISchoolRepository _schoolRepository;
        private readonly ICenterHeadRepository _centerheadrepository;

        public MasterController(ISchoolRepository schoolRepository, ICenterHeadRepository centerheadrepository)
        {
            _centerheadrepository = centerheadrepository;
            _schoolRepository = schoolRepository;
        }

        string sp = System.Configuration.ConfigurationManager.AppSettings["upload"];
        //AbstractLayer.DBClass objCommon = new AbstractLayer.DBClass();
        //AbstractLayer._schoolRepository.objDB = new AbstractLayer._schoolRepository.);
        //AbstractLayer.ErrorLog oErrorLog = new AbstractLayer.ErrorLog();        
        string sp1 = System.Configuration.ConfigurationManager.AppSettings["ImagePathCor"];

      
        public JsonResult GetSchoolLoginDetails(string schl)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            return Json(loginSession);
        }


        public  void ExportDataFromDataTable(DataTable dt, string FileNAME)
        {           
            using (XLWorkbook wb = new XLWorkbook())
            {

              string  fileName1 = FileNAME + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";

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



        #region JsonResult All

        public JsonResult GetEduCluster(string BLOCK) // Calling on http post (on Submit)
        {           
            DataSet ds = DBClass.Select_CLUSTER_NAME(BLOCK);
            List<SelectListItem> EduClusterList = new List<SelectListItem>();            
            EduClusterList = ds.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
            {
                Text = dataRow.Field<string>("CLUSTER_NAME").ToString(),
                Value = dataRow.Field<string>("CLUSTER_NAME").ToString(),
            }).ToList();           

            return Json(EduClusterList);

        }


        public JsonResult GetTehID(string DIST) // Calling on http post (on Submit)
        {           
            DataSet result = DBClass.SelectAllTehsil(DIST); // passing Value to DBClass from model

            ViewBag.MyTeh = result.Tables[0];// ViewData["result"] = result; // for dislaying message after saving storing output.
            List<SelectListItem> TehList = new List<SelectListItem>();
            //List<string> items = new List<string>();
            TehList.Add(new SelectListItem { Text = "---Select Tehsil---", Value = "0" });
            foreach (System.Data.DataRow dr in ViewBag.MyTeh.Rows)
            {
                TehList.Add(new SelectListItem { Text = @dr["TEHSIL"].ToString(), Value = @dr["TCODE"].ToString() });
            }
            ViewBag.MyTeh = TehList;

            return Json(TehList);

        }


        public JsonResult ValidateRequestID(string requestID, string CandName)
        {


            string schoolName = "";
            string formName = "";
            string cls = "";
            string candName = "";
            string status = "";
            string ApprovalUp = "";
            string DateForMax = "";
            try
            {
                DataSet result = DBClass.ValidateRequestId(requestID, CandName);
                schoolName = result.Tables[0].Rows[0]["Schl"].ToString();
                formName = result.Tables[0].Rows[0]["Form"].ToString();
                candName = result.Tables[0].Rows[0]["candName"].ToString();
                cls = result.Tables[0].Rows[0]["Class"].ToString();
                status = result.Tables[0].Rows[0]["status"].ToString();
                ApprovalUp = result.Tables[0].Rows[0]["ApprovalUpto"].ToString();
                DateTime currDate = DateTime.Now;
                //string Data = result.Tables[0].Rows[0]["status"].ToString();
                DateTime date1 = Convert.ToDateTime(ApprovalUp);
                if (date1 > currDate)
                {
                    DateForMax = currDate.ToString();
                }
                else
                {
                    DateForMax = date1.ToString();
                }


                return Json(new { schoolName = schoolName, formName = formName, candName = candName, status = status, cls = cls, ApprovalUp = DateForMax }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { schoolName = schoolName, formName = formName, candName = candName, status = status, cls = cls, ApprovalUp = ApprovalUp }, JsonRequestBehavior.AllowGet);

            }

            //return Json(result);



        }

        public JsonResult FindSchoolName(string schoolcode)
        {
            //string retval = "";
            // DBClass.FindSchoolName(schoolcode, out schoolname);
            string schoolname = "";
            DataSet ds = _schoolRepository.SchoolMasterViewSP(1, schoolcode, "");    //SelectSchoolDatabyID 
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    schoolname = ds.Tables[0].Rows[0]["schlnme"].ToString();
                }
            }            
            string dee = schoolname;
            return Json(new { sn = dee }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPunjabiName(string text)
        {
            ViewBag.Name = DBClass.getPunjabiName(text);
            return Json(ViewBag.Name);
        }

       
        public JsonResult GetTEHSILE_ByDISTID(string DIST) // TEHSIL English
        {           
            DataSet result = DBClass.SelectAllTehsil(DIST);
            List<SelectListItem> TehList = new List<SelectListItem>();
            TehList.Add(new SelectListItem { Text = "--Select Tehsil--", Value = "0" });
            foreach (System.Data.DataRow dr in result.Tables[0].Rows)
            {
                TehList.Add(new SelectListItem { Text = @dr["TEHSIL"].ToString(), Value = @dr["TCode"].ToString() });
            }
            ViewBag.MyTeh = TehList;
            return Json(TehList);
        }

        public JsonResult GetTEHSILP_ByDISTID(string DIST) // TEHSIL Punjabi By DIST ID
        {        
            DataSet result = DBClass.SelectAllTehsil(DIST);
            List<SelectListItem> TehListP = new List<SelectListItem>();
            TehListP.Add(new SelectListItem { Text = "--Select Tehsil--", Value = "0" });
            foreach (System.Data.DataRow dr in result.Tables[0].Rows)
            {
                TehListP.Add(new SelectListItem { Text = @dr["TEHSILP"].ToString(), Value = @dr["TCode"].ToString() });
            }
            ViewBag.MyTehP = TehListP;
            return Json(TehListP);
        }

        public JsonResult GetTEHSILPNAME_ByTehsilId(int Teh) // TEHSIL Pun NAMe by ID
        {
            string TehPText = Teh.ToString(); 
            List<SelectListItem> TehP = DBClass.GetAllTehsilP();

            if (TehP.Any(i => i.Value == TehPText))
            {
                TehPText = ViewBag.MyTehP = TehP.Where(i => i.Value == TehPText).Single().Text;
            }
            return Json(TehPText);
        }

        public JsonResult GetDISTPNAME_ByDISTID(int DIST) // DIST Pun NAMe by ID
        {
            string DistPText = "";
            string DistText = "";
            if (DIST.ToString().Length == 2)
            { DistText = "0" + DIST.ToString(); }
            else
            {
                DistText = DIST.ToString();
            }
            List<SelectListItem> DistP = DBClass.GetDistP();

            if (DistP.Any(i => i.Value == DistText))
            {
                DistPText = ViewBag.MyTehP = DistP.Where(i => i.Value == DistText).Single().Text;
            }
            return Json(DistPText);
        }


        public JsonResult JqSendPasswordEmail(string Schl, string Type, string SentTo)
        {
            string outid = "0";
            if (Schl != "")
            {
                DataSet ds = _schoolRepository.SchoolMasterViewSP(1,Schl,"");    //SelectSchoolDatabyID 
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string mobile = ds.Tables[0].Rows[0]["MOBILE"].ToString().Trim();
                        string emailid = ds.Tables[0].Rows[0]["EMAILID"].ToString().Trim();
                        string Password = ds.Tables[0].Rows[0]["PASSWORD"].ToString().Trim();
                        string PRINCIPAL = ds.Tables[0].Rows[0]["PRINCIPAL"].ToString().Trim();
                        if (Type == "1")
                        {
                            string Sms = "Respected " + PRINCIPAL + ", Your School User Id: " + Schl + " and Password: " + Password + " . Kindly change Password after first login for security reason.";
                            //  string Sms = "Your Login details are School Code:: " + Schl + " and Password: " + Password + ". Click to Login Here https://registration2019.pseb.ac.in/Login. Regards PSEB";
                            string getSms = DBClass.gosms(mobile, Sms);
                            if (getSms.ToLower().Contains("success"))
                            {
                                outid = "1";
                            }

                        }
                        else if (Type == "2")
                        {
                            string body = "<table width=" + 600 + " cellpadding=" + 4 + " cellspacing=" + 4 + " border=" + 0 + "><tr><td><b>Respected " + PRINCIPAL + "</b>,</td></tr><tr><td><b>Your School Login Details are given Below for Punjab School Education Board Web Portal :-</b><br /><b>School Code :</b> " + Schl + "<br /><b>School Name :</b> " + Password + "<br /></td></tr><tr><td><b>Path :</b><a href=https://www.registration.pseb.ac.in target = _blank>www.registration.pseb.ac.in</a><br /><b>UserId :</b> " + Schl + "<br /><b>Password :</b> " + Password + "<br /></td></tr><tr><td><b>Note:</b>Make sure change password after first login for Security reason.</td></tr><tr><td>This is a system generated e-mail and please do not reply. Add <a target=_blank href=mailto:noreply@PsebJunior.in>noreply@PsebJunior.in</a> to your white list / safe sender list. Else, your mailbox filter or ISP (Internet Service Provider) may stop you from receiving e-mails.</td></tr><tr><td><b><i>Regards</b><i>,<br /> Tech Team, <br />Punjab School Education Board<br /><tr><td><b>Contact Us</b><br><b>Email Id:</b> <a href=mailto:Contact1@PsebJunior.in target=_blank>contact1@PsebJunior.in</a><br><b>Toll Free Help Line No. :</b> 18002700280<br>DISTRICTS:- BARNALA, FATEHGARH SAHIB, GURDASPUR, HOSHIARPUR, JALANDHAR, KAPURTHALA, SHRI MUKTSAR SAHIB, S.B.S. NAGAR, PATHANKOT, PATIALA, SANGRUR, CHANDIGARH &amp; OTHER STATES<br><br><b>Email Id:</b> <a href=mailto:Contact2@PsebJunior.in target=_blank>contact2@PsebJunior.in</a><br><b>Toll Free Help Line No. :</b> 18004190690<br>DISTRICTS:- AMRITSAR, BATHINDA, FARIDKOT, FAZILKA, FEROZEPUR, LUDHIANA, MANSA, MOGA, ROOP NAGAR, S.A.S NAGAR,TARN TARAN<br></td></tr>";
                            string subject = "School Login Details for PSEB Portal.";
                            bool result = DBClass.mail(subject, body, emailid);
                            if (result == true)
                            {
                                outid = "1";
                            }

                        }
                    }
                }
            }

            return Json(new { status = outid }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckSchoolCode(string schoolcode)
        {
            string schoolname = "";
            string districtname = "";
            string outid = "0";
            string verifylogin = "";
            DataSet ds = _schoolRepository.SchoolMasterViewSP(1,schoolcode,"");    //SchoolMasterViewSP 
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    outid = "1";
                    ViewBag.schoolname = schoolname = ds.Tables[0].Rows[0]["schlnme"].ToString();
                    ViewBag.districtname = districtname = ds.Tables[0].Rows[0]["DISTE"].ToString().Trim();
                    verifylogin = ds.Tables[0].Rows[0]["IsVerified"].ToString().Trim();
                }
            }
            return Json(new { sn = schoolname, dn = districtname, vl = verifylogin, oid = outid }, JsonRequestBehavior.AllowGet);
        }


        #endregion JsonResult All


        #region registration JSONResult
       
        #region UpdateAadharEnrollNo
        public JsonResult UpdateAadharEnrollNo(string std_id, string aadhar_num, string SCHL, string Caste, string gender, string BPL, string Rel, string Epunid)
        {           
            RegistrationModels rm = new RegistrationModels();
            try
            {

                string dee = "";
                string outstatus = "";
                string Search = string.Empty;
                DataSet res1 = RegistrationDB.UpdaadharEnrollmentNo(std_id, aadhar_num, SCHL, Caste, gender, BPL, Rel, Epunid);
                string res = res1.Tables[0].Rows[0]["res"].ToString();
                if (res != "0")
                {
                    //dee = res;
                    dee = "Yes";
                }
                else
                    dee = "No";

                return Json(new { sn = dee, chid = res }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path)); 
                 return null;
            }
        }
        #endregion UpdateAadharEnrollNo

              
        public JsonResult CancelStdRegNo(string Remarks, string stdid)
        {
            try
            {
                string dee = "";
                string res = null;
                DataSet result = RegistrationDB.CancelStdRegNo(Remarks, stdid);
                res = result.Tables[0].Rows.Count.ToString();
                if (result.Tables[0].Rows.Count.ToString() != "0")
                {
                    dee = "Yes";
                }
                else
                { dee = "No"; }


                return Json(new { sn = dee, chid = res }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ////ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path)); 
                //return RedirectToAction("Logout", "Login");
                return null;
            }
        }


        public JsonResult SwitchForm(string Remarks, string stdid)
        {
            try
            {
                string dee = "";
                int OutStatus = 0;
                RegistrationDB.SwitchForm(Remarks, stdid, out OutStatus);                
                if (OutStatus == 0)
                {
                    dee = "Yes";
                }
                else if (OutStatus == 0)
                {
                    dee = "NotAllowed";
                }
                else
                { dee = "No"; }


                return Json(new { sn = dee, chid = OutStatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {               
                return null;
            }
        }


        #endregion


        #region VerifyMobileSchl
        public async Task<JsonResult> VerifyMobileSchl(string SCHL, string MOBILE, string VerifyMobile)
        {           
            try
            {
                string result = "";
                string smsStatus = "";
                if (string.IsNullOrEmpty(SCHL) || string.IsNullOrEmpty(MOBILE) || string.IsNullOrEmpty(VerifyMobile))
                {
                    result = "FILL";
                }

                if (MOBILE.Trim() == VerifyMobile.Trim())
                {
                    result = "Yes";
                    SchoolDataBySchlModel schoolDataBySchlModel = await _schoolRepository.GetSchoolDataBySchl(SCHL);

                    if (!string.IsNullOrEmpty(MOBILE) && MOBILE.Length == 10)
                    {
                        string Sms = "Your Login details are School Code:: " + SCHL + " and Password: " + schoolDataBySchlModel.PASSWORD + ". Click to Login Here https://middleprimary.pseb.ac.in. Regards PSEB";
                        string getSms = DBClass.gosms(MOBILE, Sms);
                        if (getSms.ToLower().Contains("success"))
                        {
                            smsStatus = "success";
                        }
                        else { smsStatus = "failed"; }
                    }
                }
                else
                { result = "No"; }

                return  Json(new { sn = result, vmobile = VerifyMobile, smsstatus = smsStatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {               
                return null;
               // return Json(new { sn = null, chid = null }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion VerifyMobileSchl

        #region VerifyMobileCluster
        public JsonResult VerifyMobileCluster(string UserName, string MOBILE, string VerifyMobile)
        {
            try
            {
                string result = "";
                string smsStatus = "";
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(MOBILE) || string.IsNullOrEmpty(VerifyMobile))
                {
                    result = "FILL";
                }

                if (MOBILE.Trim() == VerifyMobile.Trim())
                {
                    result = "Yes";
                    //SchoolDataBySchlModel schoolDataBySchlModel = await _ce.GetSchoolDataBySchl(SCHL);
                    DataSet ds =   _centerheadrepository.CenterHeadMaster(6, Convert.ToInt32(UserName), "");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string PASSWORD = ds.Tables[0].Rows[0]["pwd"].ToString();
                        if (!string.IsNullOrEmpty(MOBILE) && MOBILE.Length == 10)
                        {
                            string Sms = "Cluster Login details are UserName: " + UserName + " and Pwd: " + PASSWORD + ". Click to Login Here https://middleprimary.pseb.ac.in/CenterHead. Regards PSEB";
                            string getSms = DBClass.gosms(MOBILE, Sms);
                            if (getSms.ToLower().Contains("success"))
                            {
                                smsStatus = "success";
                            }
                            else { smsStatus = "failed"; }
                        }
                    }
                    
                }
                else
                { result = "No"; }

                return Json(new { sn = result, vmobile = VerifyMobile, smsstatus = smsStatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
                // return Json(new { sn = null, chid = null }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion VerifyMobileCluster



        #region StudentSchoolMigration

        // CancelStudentSchoolMigration
        public JsonResult CancelStudentSchoolMigration(string cancelremarks, string stdid, string migid, string Type)
        {
            try
            {
                string dee = "";
                string outstatus = "";
                string UpdatedBy = "";
                if (Session["AdminLoginSession"] == null && Session["LoginSession"] == null)
                {
                    return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
                }

                if (Session["LoginSession"] != null)
                {
                    UpdatedBy = "SCHL-" + Session["LoginSession"].ToString();
                }

                if (Session["AdminLoginSession"] != null)
                {
                    UpdatedBy = "ADMIN-" + Session["AdminId"].ToString();
                }

                string result = SchoolDB.CancelStudentSchoolMigration(cancelremarks, stdid, migid, out outstatus, UpdatedBy, Type);//ChallanDetailsCancelSP                
                dee = outstatus;
                return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // CancelStudentSchoolMigration
        public JsonResult UpdateStatusStudentSchoolMigration(string remarks, string stdid, string migid, string status, string AppLevel, string Type)
        {
            string dee = "";
            string outstatus = "";
            string UpdatedBy = "";
            try
            {

                if (Session["AdminLoginSession"] == null && Session["LoginSession"] == null)
                {
                    return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
                }

                if (Session["LoginSession"] != null)
                {
                    UpdatedBy = "SCHL-" + Session["LoginSession"].ToString();
                }

                if (Session["AdminLoginSession"] != null)
                {
                    UpdatedBy = "ADMIN-" + Session["AdminId"].ToString();
                }
                StudentSchoolMigrationViewModel studentSchoolMigrationViewModel = new StudentSchoolMigrationViewModel();
                string result = SchoolDB.UpdateStatusStudentSchoolMigration(remarks, stdid, migid, status, AppLevel, out outstatus, UpdatedBy, Type);//ChallanDetailsCancelSP                
                dee = outstatus;
                if (outstatus == "1")
                {
                    string upStautus = status == "A" ? "Approved" : status == "R" ? "Rejected" : "Updated";
                    string SchoolMobile = "";
                    string Search = "MigrationId =" + migid;
                    List<StudentSchoolMigrationViewModel> studentSchoolMigrationViewModelList = SchoolDB.StudentSchoolMigrationsSearchModel(2, Search, ""); // type=2 for mig
                    if (studentSchoolMigrationViewModelList.Count() > 0)
                    {
                        ViewBag.TotalCount = studentSchoolMigrationViewModelList.Count;
                        studentSchoolMigrationViewModel = studentSchoolMigrationViewModelList.Where(s => s.MigrationId == Convert.ToInt32(migid)).FirstOrDefault();
                        SchoolMobile = studentSchoolMigrationViewModel.NEWSCHLMOBILE;

                        if (!string.IsNullOrEmpty(SchoolMobile))
                        {
                            string Sms = "";
                            if (AppLevel.ToUpper() == "SCHL".ToUpper() && !string.IsNullOrEmpty(SchoolMobile))
                            {
                                //SchoolMobile = Session["SchoolMobile"].ToString();
                                Sms = "School to School Migration of Student " + stdid + " of Class is " + upStautus + " by old school. Check status under School Migration -> Applied List. Regards PSEB";
                            }
                            else if (AppLevel.ToUpper() == "HOD".ToUpper() && !string.IsNullOrEmpty(SchoolMobile))
                            {
                                // SchoolMobile = Session["SchoolMobile"].ToString();
                                Sms = "School to School Migration of Student " + stdid + " of Class is " + upStautus + " by Head Office. Check status under School Migration -> Applied List. Regards PSEB";
                            }
                            try
                            {
                                string getSms = DBClass.gosms(SchoolMobile, Sms);
                            }
                            catch (Exception) { }
                        }
                    }
                }

                return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { sn = dee, chid = outstatus }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        [HttpPost]
        public ActionResult JqUpdateOtherBoardDocuments()
        {
            try
            {
                RegistrationModels rm = new RegistrationModels();
                // Checking no of files injected in Request object  
                if (Request.Files.Count > 0)
                {
                    try
                    {
                        int flag = 0;
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {

                            HttpPostedFileBase file = files[i];
                            string fname;
                            string fileKey = i == 0 ? "DocProofCertificate" : i == 1 ? "DocProofNRICandidates" : "";
                            string result = Request.Form["StudentUniqueId"].ToString();
                            string formName = Request.Form["formName"].ToString();
                            string schlDist = Request.Form["schlDist"].ToString();
                            string stdid = Request.Form["stdid"].ToString();
                            // Get the complete folder path and store the file inside it.  
                            //fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                            //file.SaveAs(fname);


                            if (file != null && fileKey == "DocProofCertificate")
                            {
                                string fileExt = Path.GetExtension(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + formName + "/" + schlDist + "/ProofCertificate"), result + "C" + fileExt);
                                string FilepathExist = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + formName + "/" + schlDist + "/ProofCertificate"));
                                if (!Directory.Exists(FilepathExist))
                                {
                                    Directory.CreateDirectory(FilepathExist);
                                }
                                file.SaveAs(path);
                                rm.ProofCertificate = formName + "/" + schlDist + "/ProofCertificate" + "/" + result + "C" + fileExt;
                                string type = "CM";
                                string UpdatePicC = RegistrationDB.Updated_Pic_Data(result, rm.ProofCertificate, type);
                                flag = 1;
                            }

                            if (file != null && fileKey == "DocProofNRICandidates")
                            {
                                string fileExt = Path.GetExtension(file.FileName);
                                string pathName = formName + "/" + schlDist + "/ProofNRICandidates";
                                var path = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + pathName), result + "NRI" + fileExt);
                                string FilepathExist = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + pathName));
                                if (!Directory.Exists(FilepathExist))
                                {
                                    Directory.CreateDirectory(FilepathExist);
                                }
                                file.SaveAs(path);
                                rm.ProofNRICandidates = pathName + "/" + result + "NRI" + fileExt;
                                string type = "NRIM";
                                string UpdatePicC = RegistrationDB.Updated_Pic_Data(result, rm.ProofNRICandidates, type);
                                flag = 1;
                            }
                        }
                        // Returns message that successfully uploaded  

                        if (flag == 0)
                        {
                            return Json(new { oid = flag, msg = "failure" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { oid = flag, msg = "success" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { oid = 5, msg = "Error occurred : " + ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { oid = 2, msg = "No files Selected" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { oid = 5, msg = "Error occurred : " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult JqUpdateOtherBoardDocumentsBySchool()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string Schl = loginSession.SCHL.ToString();
            try
            {
                RegistrationModels rm = new RegistrationModels();
                // Checking no of files injected in Request object  
                if (Request.Files.Count > 0)
                {
                    try
                    {   
                        int flag = 0;
                        //  Get all files from Request object  
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {

                            HttpPostedFileBase file = files[i];
                            string fname;
                            string fileKey = i == 0 ? "DocAddDocument" : "";
                            string result = Request.Form["StudentUniqueId"].ToString();
                            string formName = Request.Form["formName"].ToString();
                            string schlDist = Request.Form["schlDist"].ToString();
                            string stdid = Request.Form["stdid"].ToString();
                            string addDocumentRemarks = Request.Form["Remarks"].ToString();
                            // Get the complete folder path and store the file inside it.  
                            //fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                            //file.SaveAs(fname);
                            string myUniqueFileName = StaticDB.GenerateFileName(result);

                            if (file != null && fileKey == "DocAddDocument")
                            {
                                string fileExt = Path.GetExtension(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + "OtherBoardDocumentsBySchool"), myUniqueFileName + fileExt);
                                string FilepathExist = Path.Combine(Server.MapPath("~/Upload/" + "upload2023/" + "OtherBoardDocumentsBySchool"));
                                if (!Directory.Exists(FilepathExist))
                                {
                                    Directory.CreateDirectory(FilepathExist);
                                }
                                file.SaveAs(path);
                                rm.ProofCertificate = "OtherBoardDocumentsBySchool" + "/" + myUniqueFileName + fileExt;
                                string type = "AD";

                                tblOtherBoardDocumentsBySchool tblOtherBoardDocuments = new tblOtherBoardDocumentsBySchool()
                                {
                                    Stdid = long.Parse(stdid),
                                    Filepath = rm.ProofCertificate,
                                    Remarks = addDocumentRemarks,
                                    IsActive = true,
                                    SubmitOn = DateTime.Now,
                                    SubmitBy = Schl
                                };

                                _context.tblOtherBoardDocumentsBySchool.Add(tblOtherBoardDocuments);
                                int insertedRecords = _context.SaveChanges();
                                if (insertedRecords > 0)
                                {
                                    flag = 1;
                                }
                                else
                                {
                                    flag = 0;
                                }
                            }


                        }
                        // Returns message that successfully uploaded  

                        if (flag == 0)
                        {
                            return Json(new { oid = flag, msg = "failure" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { oid = flag, msg = "success" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { oid = 5, msg = "Error occurred : " + ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { oid = 2, msg = "No files Selected" }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return Json(new { oid = 5, msg = "Error occurred : " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: Master
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Ins_School_Center_Choice(string CenterChoice, string CenterDisTance)
        {
            //List<SelectListItem> objGroupList = new List<SelectListItem>();

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();

            DataTable dt = null;

            try
            {
                string code = CenterChoice.Split('-')[0].Replace(" ", "");
                DataSet result = RegistrationDB.Ins_School_Center_ChoiceOld(CenterChoice, CenterDisTance, code);


                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();
                        objGroupList.Add(objGroupLists);
                    }
                }


                return Json(objGroupList);
            }
            catch (Exception ex)
            {
                return Json(objGroupList);

            }

            //return Json(result);



        }

        public JsonResult Ins_School_Center_Choice_New(string CenterChoice, string CenterDisTance)
        {
            //List<SelectListItem> objGroupList = new List<SelectListItem>();

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();

            DataTable dt = null;

            try
            {
                string code = CenterChoice.Split('-')[0].Replace(" ", "");
                DataSet result = RegistrationDB.Ins_School_Center_ChoiceNew(CenterChoice, CenterDisTance, code);


                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();
                        objGroupList.Add(objGroupLists);
                    }
                }


                return Json(objGroupList);
            }
            catch (Exception ex)
            {
                return Json(objGroupList);

            }

            //return Json(result);



        }

        public JsonResult Delete_School_Center_Choice(int Id)
        {
            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.Finalsubmittedforchoice == 0)
            {
                DataTable dt = null;
                DataSet result = RegistrationDB.Delete_School_Center_Choice(Id);
                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();
                        objGroupList.Add(objGroupLists);
                    }
                }
            }
            else
            {
                DataSet result = RegistrationDB.Get_School_Center_Choice();
                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();

                        objGroupList.Add(objGroupLists);
                    }
                }
            }

            return Json(objGroupList);



        }

        public JsonResult Get_School_Center_Choice()
        {
            //List<SelectListItem> objGroupList = new List<SelectListItem>();

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();

            DataTable dt = null;

            try
            {
                DataSet result = RegistrationDB.Get_School_Center_Choice();


                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();

                        objGroupList.Add(objGroupLists);
                    }
                }


                return Json(objGroupList);
            }
            catch (Exception ex)
            {
                return Json(objGroupList);

            }

            //return Json(result);



        }

        public JsonResult Get_School_Center_Choice_New()
        {
            //List<SelectListItem> objGroupList = new List<SelectListItem>();

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();

            DataTable dt = null;

            try
            {
                DataSet result = RegistrationDB.Get_School_Center_Choice_New();


                if (result.Tables.Count > 0)
                {
                    foreach (DataRow dr in result.Tables[0].Rows) // For addition Section
                    {
                        ExamCenterDetail objGroupLists = new ExamCenterDetail();

                        objGroupLists.ID = Convert.ToInt32(dr["ID"].ToString());
                        objGroupLists.schl = dr["schl"].ToString();
                        objGroupLists.choiceschlcode = dr["choiceschlcode"].ToString();
                        objGroupLists.distance = dr["distance"].ToString();
                        objGroupLists.insertdate = dr["insertdate"].ToString();
                        objGroupLists.choiceschoolcode = dr["choiceschoolcode"].ToString();

                        objGroupList.Add(objGroupLists);
                    }
                }


                return Json(objGroupList);
            }
            catch (Exception ex)
            {
                return Json(objGroupList);

            }

            //return Json(result);



        }

    }
}