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
using Newtonsoft.Json;
using System.Web.UI;
using ClosedXML.Excel;
using CCA.Util;
using System.Configuration;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using encrypt;
using System.Data.Entity;
using Amazon.S3.Transfer;
using Amazon.S3;
using Amazon;


namespace PsebPrimaryMiddle.Controllers
{

    public class SchoolController : Controller
    {

        private const string BUCKET_NAME = "psebdata";
        string sp = System.Configuration.ConfigurationManager.AppSettings["upload"];
        //AbstractLayer.DBClass objCommon = new AbstractLayer.DBClass();
        string sp1 = System.Configuration.ConfigurationManager.AppSettings["ImagePathCor"];



        private readonly IChallanRepository _challanRepository;

        private readonly ISchoolRepository _schoolRepository;
        private readonly DBContext _context = new DBContext();

        public SchoolController(IChallanRepository challanRepository, ISchoolRepository schoolRepository)
        {
            _challanRepository = challanRepository;
            _schoolRepository = schoolRepository;
        }



        public static Byte[] QRCoder(string qr)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode("https://middleprimary2022.pseb.ac.in/AdmitCard/Index/" + QueryStringModule.Encrypt(qr), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return (BitmapToBytesCode(qrCodeImage));

        }
        [NonAction]
        private static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }



        // GET: School
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetSchoolDataBySchl(string schl)
        {
            DataSet ds = new DataSet();
            SchoolModels schoolModels = _schoolRepository.GetSchoolDataBySchl(schl, out ds);
            return View(schoolModels);
        }


        #region School Admit Card 

        [SessionCheckFilter]
        public ActionResult SchoolAdmitCardList(SchoolModels rm)
        {
            //LoginSession loginSession = (LoginSession)Session["LoginSession"];               
            return View();
        }

        [SessionCheckFilter]
        public ActionResult PrintAdmitCard(string id, RegistrationModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SchoolAdmitCardList", "School");
            }
            ViewBag.cid = id;
            string cls = id.ToLower() == "primary" ? "5" : id.ToLower() == "middle" ? "8" : "";

            return View(rm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PrintAdmitCard(string id, RegistrationModels rm, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SchoolAdmitCardList", "School");
            }
            ViewBag.cid = id;
            string cls = id.ToLower() == "primary" ? "5" : id.ToLower() == "middle" ? "8" : "";
            try
            {
                rm.CandId = frc["CandId"].ToString();
                rm.ExamRoll = frc["ExamRoll"].ToString();
                rm.SelList = frc["SelList"].ToString();

                string search = " reg.schl='" + loginSession.SCHL + "'";
                if (!string.IsNullOrEmpty(rm.CandId))
                {
                    search += " and reg.std_id='" + rm.CandId + "'";
                }
                if (!string.IsNullOrEmpty(rm.ExamRoll))
                {
                    search += " and exm.ROLL='" + rm.ExamRoll + "'";
                }

                rm.StoreAllData = _schoolRepository.PrintAdmitCard(search, loginSession.SCHL, cls);
                //rm.QRCode = QRCoder(_schoolRepository.PrintAdmitCard(search, loginSession.SCHL, cls).ToString());
                ViewBag.TotalCount = rm.StoreAllData.Tables[0].Rows.Count;
                if (rm.StoreAllData == null || rm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message2 = "Record Not Found";
                    ViewBag.TotalCount2 = 0;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ERROR = ex.Message;
            }
            return View(rm);
        }



        #endregion School Admit Card 


        #region  School Cut List


        [SessionCheckFilter]
        public ActionResult SchoolCutList(SchoolModels rm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            return View();
        }

        [SessionCheckFilter]
        public ActionResult CutList_Schl(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolCutList", "School");
                }
                ViewBag.CutlistClass = id;

                if (id.ToLower().Contains("error"))
                {
                    ViewBag.Status = "0";
                }
                else
                {
                    ViewBag.Status = "1";
                }
                if (ViewBag.Status != null)
                {
                    //ViewBag.Status = "1";
                    //if (id == "M" || id == "ME")//Primary
                    if (id.ToLower() == "primary" || id.ToLower() == "primaryerror")//Primary
                    {
                        // Type1 = "REG";
                        DataSet Dresult = _schoolRepository.GetCentreSchl(loginSession.SCHL, "5");
                        List<SelectListItem> SecList = new List<SelectListItem>();
                        SecList.Add(new SelectListItem { Text = "No Centre", Value = "No Centre" });
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {

                            SecList.Add(new SelectListItem { Text = @dr["centE"].ToString(), Value = @dr["CENT"].ToString() });
                        }
                        ViewBag.Ecent = SecList;
                    }
                    else if (id.ToLower() == "middle" || id.ToLower() == "middleerror")//Primary
                    {
                        //Type1 = "REG";
                        DataSet Dresult = _schoolRepository.GetCentreSchl(loginSession.SCHL, "8");
                        List<SelectListItem> SecList = new List<SelectListItem>();
                        SecList.Add(new SelectListItem { Text = "No Centre", Value = "No Centre" });
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {

                            SecList.Add(new SelectListItem { Text = @dr["centE"].ToString(), Value = @dr["CENT"].ToString() });
                        }
                        ViewBag.Ecent = SecList;
                    }
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;

                }
            }
            catch (Exception ex)
            {
            }

            return View(MS);
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CutList_Schl(FormCollection frm, string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolCutList", "School");
                }
                ViewBag.CutlistClass = id;
                string cls = "";
                if (id.ToLower().Contains("error"))
                {
                    ViewBag.Status = "0";
                }
                else
                {
                    ViewBag.Status = "1";
                }
                if (ViewBag.Status != null)
                {
                    if (id.ToLower() == "primary" || id.ToLower() == "primaryerror")//Primary
                    {
                        cls = "5";
                        // Type1 = "REG";
                        DataSet Dresult = _schoolRepository.GetCentreSchl(loginSession.SCHL, cls);
                        List<SelectListItem> SecList = new List<SelectListItem>();
                        SecList.Add(new SelectListItem { Text = "No Centre", Value = "No Centre" });
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {

                            SecList.Add(new SelectListItem { Text = @dr["centE"].ToString(), Value = @dr["CENT"].ToString() });
                        }
                        ViewBag.Ecent = SecList;
                    }
                    else if (id.ToLower() == "middle" || id.ToLower() == "middleerror")//Primary
                    {
                        cls = "8";
                        //Type1 = "REG";
                        DataSet Dresult = _schoolRepository.GetCentreSchl(loginSession.SCHL, cls);
                        List<SelectListItem> SecList = new List<SelectListItem>();
                        SecList.Add(new SelectListItem { Text = "No Centre", Value = "No Centre" });
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {

                            SecList.Add(new SelectListItem { Text = @dr["centE"].ToString(), Value = @dr["CENT"].ToString() });
                        }
                        ViewBag.Ecent = SecList;
                    }
                    MS.ExamCent = frm["ExamCent"].ToString();

                    string Search = "  std_id like '%' ";

                    if (!string.IsNullOrEmpty(MS.ExamCent) && MS.ExamCent != "No Centre")
                    {
                        Search = " ER.Cent= '" + frm["ExamCent"] + "' ";
                    }




                    if (frm["SelList"] != "0")
                    {
                        MS.SelList = frm["SelList"];
                        //ViewBag.SelectedItem = frm["SelList"];
                        int SelValueSch = Convert.ToInt32(frm["SelList"].ToString());
                        if (frm["SearchString"] != "")
                        {
                            if (SelValueSch == 1)
                            { Search += "  std_id='" + frm["SearchByString"].ToString() + "'"; }
                            else if (SelValueSch == 2)
                            { Search += "  Candi_Name like '%" + frm["SearchByString"].ToString() + "%'"; }
                            else if (SelValueSch == 3)
                            { Search += "  Father_Name  like '%" + frm["SearchByString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += "  Mother_Name like '%" + frm["SearchByString"].ToString() + "%'"; }
                            else if (SelValueSch == 5)
                            { Search += "  registration_Num like '%" + frm["SearchByString"].ToString() + "%'"; }

                        }
                    }

                    //GetCutList_Schl To CutList
                    MS.StoreAllData = _schoolRepository.CutList(Search, loginSession.SCHL, cls, "REG", ViewBag.Status);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(MS);
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        return View(MS);
                    }
                }
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion School Cut List


        #region Begin CCE   
        [SessionCheckFilter]
        public ActionResult CCE_Portal()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["CCE_MarksSearch"] = null;
            return View();
        }
        [SessionCheckFilter]
        public ActionResult CCE_Agree(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["CCEClass"] = id;
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CCE_Agree(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string cc = id;
                string s = frm["Agree"].ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("CCE_Portal", "School");
                }
                else
                {
                    if (s == "Agree")
                    {
                        return RedirectToAction("CCE_Marks", "School", new { id = cc });
                    }
                }
                return RedirectToAction("CCE_Portal", "School");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        public ActionResult CCE_Marks(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                string SelectedAction = "0";


                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    if (TempData["CCE_MarksSearch"] != null)
                    {
                        Search += TempData["CCE_MarksSearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelFilter"] = ViewBag.SelectedFilter;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["CCE_MarksSearch"] = Search;
                    }
                    else
                    {
                        Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                    }



                    MS.StoreAllData = _schoolRepository.GetCCEMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, Convert.ToInt32(SelectedAction));
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {

                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);

                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }

                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CCE_Marks(string id, FormCollection frm, int? page)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "";
                    Search = "  a.SCHL = '" + loginSession.SCHL + "' and  a.class='" + CLASS + "' ";

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "")
                        {
                            if (SelValueSch == 3)
                            {
                                SelAction = 2;
                            }
                            //  { Search += " and  IsCCEFilled=1 "; } // Filled
                            else if (SelValueSch == 2)
                            { SelAction = 1; }
                            //{ Search += " and (IsCCEFilled is null or IsCCEFilled=0) "; } // pending
                        }
                        ViewBag.SelectedAction = frm["SelAction"];
                    }

                    if (frm["SelFilter"] != "")
                    {

                        ViewBag.SelectedFilter = frm["SelFilter"];
                        int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                        if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and b.Roll='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelFilter"] = frm["SelFilter"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["CCE_MarksSearch"] = Search;
                    // string class1 = "4";
                    MS.StoreAllData = _schoolRepository.GetCCEMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, SelAction);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqCCEMarks(string stdid, string CandSubject, string cls)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;

            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubject>>(CandSubject);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKS");
            dtSub.Columns.Add("MINMARKS");
            dtSub.Columns.Add("MAXMARKS");
            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                row = dtSub.NewRow();
                if (rowObj.OBTMARKS == "A" || rowObj.OBTMARKS == "ABS")
                {
                    rowObj.OBTMARKS = "ABS";
                }
                else if (rowObj.OBTMARKS == "C" || rowObj.OBTMARKS == "CAN")
                {
                    rowObj.OBTMARKS = "CAN";
                }
                else if (rowObj.OBTMARKS != "")
                {
                    rowObj.OBTMARKS = rowObj.OBTMARKS.PadLeft(3, '0');
                }

                if (rowObj.MINMARKS == "--" || rowObj.MINMARKS == "")
                {
                    rowObj.MINMARKS = "000";
                }
                dtSub.Rows.Add(stdid, rowObj.SUB, rowObj.OBTMARKS, rowObj.MINMARKS, rowObj.MAXMARKS);
            }
            dtSub.AcceptChanges();


            foreach (DataRow dr1 in dtSub.Rows)
            {
                if (dr1["OBTMARKS"].ToString() == "" || dr1["OBTMARKS"].ToString() == "ABS" || dr1["OBTMARKS"].ToString() == "CAN")
                { }
                else if (dr1["OBTMARKS"].ToString() == "0" || dr1["OBTMARKS"].ToString().Contains("A") || dr1["OBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTMARKS"].ToString());
                    int min = Convert.ToInt32(dr1["MINMARKS"].ToString());
                    int max = Convert.ToInt32(dr1["MAXMARKS"].ToString());

                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                // string class1 = "5";
                string class1 = cls == "Primary" ? "5" : cls == "Middle" ? "8" : "5";
                int OutStatus = 0;
                dee = _schoolRepository.AllotCCEMarks(loginSession.SCHL, stdid, dtSub, class1, out OutStatus);
                var results = new
                {
                    status = OutStatus
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        [SessionCheckFilter]
        public ActionResult CCEReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["CCE_MarksSearch"] = null;
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                MS.StoreAllData = _schoolRepository.CCEMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        public ActionResult CCEFinalReport(string id, FormCollection frm)
        {
            TempData["CCE_MarksSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                DataSet dsFinal = new DataSet();
                MS.StoreAllData = dsFinal = _schoolRepository.CCEMarksEntryReport(loginSession.SCHL, 1, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowCCE = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowCCE = 1;
                    MS.StoreAllData = _schoolRepository.CCEMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.CCEDate = MS.StoreAllData.Tables[0].Rows[0]["CCEDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }
                    if (dsFinal.Tables[3].Rows.Count > 0)
                    {
                        MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                        {
                            Cls = dsFinal.Tables[3].Rows[0]["Cls"].ToString(),
                            IsActive = Convert.ToInt32(dsFinal.Tables[3].Rows[0]["IsActive"].ToString()),
                            IsAllow = dsFinal.Tables[3].Rows[0]["IsAllow"].ToString(),
                            LastDate = Convert.ToString(dsFinal.Tables[3].Rows[0]["LastdateDT"].ToString()),
                            Panel = Convert.ToString(dsFinal.Tables[3].Rows[0]["Panel"].ToString())
                        };
                    }
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowCCE = 2;
                    ViewBag.IsFinal = 0;
                    ViewBag.TotalCount1 = 0;
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult CCEFinalReport(string id)
        {
            TempData["CCE_MarksSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;
                string Search = string.Empty;
                string OutError = "";
                MS.StoreAllData = _schoolRepository.CCEMarksEntryReport(loginSession.SCHL, 2, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.CCEDate = MS.StoreAllData.Tables[0].Rows[0]["CCEDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }


        #endregion  End CCE Senior 


        #region   Practical Exams


        [SessionCheckFilter]
        public ActionResult PracticalChartLink()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            return View();
        }

        [SessionCheckFilter]
        public ActionResult PracticalChart(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            ViewBag.cid = id;
            string class1 = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = class1;

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("PracticalChartLink", "School");
            }
            else
            {
                MS.StoreAllData = _schoolRepository.GetPracticalMarks_Schl(loginSession.SCHL, class1);
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
            return View(MS);
        }


        [SessionCheckFilter]
        public ActionResult PracticalAgree(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PracticalAgree"] = "1";
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PracticalAgree(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string s = frm["Agree"].ToString();
                if (s == "Agree")
                {
                    return RedirectToAction("PracticalExamMarks", "School");

                }
                else
                {
                    return RedirectToAction("PracticalChartLink", "School");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        #region Practical Exam Marks View
        [SessionCheckFilter]
        [OutputCache(Duration = 180, VaryByParam = "none", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracticalExamMarks(string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            TempData["PracExamViewListSearch"] = null;
            TempData["PracExamEnterMarksSearch"] = null;
            TempData["ViewPracExamFinalSubmitSearch"] = null;

            try
            {
                ViewBag.cid = id;
                ViewBag.schlCode = loginSession.SCHL;

                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                //ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Subject Code" }, new { ID = "2", Name = " Subject Name" }, new { ID = "3", Name = "Pending" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";


                var itemClass = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                // var itemClass = new SelectList(new[] {  new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                // var itemClass = new SelectList(new[] {  new { ID = "5", Name = "Primary" }, }, "ID", "Name", 1);
                ViewBag.MyClass = itemClass.ToList();
                ViewBag.SelectedClass = "5";



                if (loginSession.SCHL != null)
                {

                    string Search = " Reg.schl is not null ";
                    string SelectedAction = "0";
                    if (TempData["PracticalExamMarksSearch"] != null)
                    {
                        Search = TempData["PracticalExamMarksSearch"].ToString();
                        ViewBag.SelectedClass = TempData["SelClassPE"];
                        ViewBag.SelectedRP = TempData["SelRPPE"].ToString();
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelClassPE"] = ViewBag.SelectedClass;
                        TempData["SelRPPE"] = ViewBag.SelectedRP;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["PracticalExamMarksSearch"] = Search;


                        string class1 = ViewBag.SelectedClass;
                        string rp = ViewBag.SelectedRP;
                        string cent = loginSession.SCHL;

                        MS.StoreAllData = _schoolRepository.PracExamEnterMarks(class1, rp, cent, Search, Convert.ToInt32(SelectedAction), pageIndex);
                        if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                            return View();
                        }
                        else
                        {
                            ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                            int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                            ViewBag.TotalCount1 = count;
                            int tp = Convert.ToInt32(count);
                            int pn = tp / 20;
                            int cal = 20 * pn;
                            int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                            if (res >= 1)
                                ViewBag.pn = pn + 1;
                            else
                                ViewBag.pn = pn;

                            return View(MS);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }

                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        [OutputCache(Duration = 180, VaryByParam = "none", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracticalExamMarks(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            try
            {
                ViewBag.cid = id;
                ViewBag.schlCode = loginSession.SCHL;

                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                //ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Subject Code" }, new { ID = "2", Name = " Subject Name" }, new { ID = "3", Name = "Pending" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";


                var itemClass = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                // var itemClass = new SelectList(new[] {  new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                // var itemClass = new SelectList(new[] { new { ID = "5", Name = "Primary" }, }, "ID", "Name", 1);
                ViewBag.MyClass = itemClass.ToList();
                ViewBag.SelectedClass = "5";

                //------------------------
                string SelClass = "";
                string RP = "";
                string Cent = "";


                if (loginSession.SCHL != null)
                {

                    string Search = " Reg.schl is not null ";


                    Search += " and sb.pcent = '" + loginSession.SCHL + "'";
                    Cent = loginSession.SCHL;
                    if (frm["SelClass"] != "")
                    {
                        SelClass = frm["SelClass"].ToString();
                        ViewBag.SelectedClass = frm["SelClass"].ToString();
                        Search += " and Reg.class='" + SelClass + "'";
                    }

                    if (frm["SelRP"] != "" && frm["SelRP"] != null)
                    {
                        RP = frm["SelRP"].ToString();
                        ViewBag.SelectedRP = frm["SelRP"].ToString();
                        Search += " and Reg.rp='" + RP + "'";
                    }
                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        ViewBag.SelectedAction = frm["SelAction"];
                        SelAction = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelAction == 1)
                            { Search += " and sb.Sub='" + frm["SearchString"].ToString() + "'"; }
                            if (SelAction == 2)
                            { Search += " and sm.Name_eng like '%" + frm["SearchString"].ToString() + "%'"; }
                            if (SelAction == 3)
                            { Search += " and  isnull(fplot,'')='' "; }
                        }
                    }

                    TempData["SelClassPE"] = SelClass;
                    TempData["SelRPPE"] = RP;
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["PracticalExamMarksSearch"] = Search;

                    MS.StoreAllData = _schoolRepository.PracExamEnterMarks(SelClass, RP, Cent, Search, Convert.ToInt32(SelAction), pageIndex);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        Session["PracticalExamMarks"] = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        Session["PracticalExamMarks"] = "1";
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }
        #endregion Practical Exam Marks View



        #region PracExamViewList
        [SessionCheckFilter]
        [OutputCache(Duration = 180, VaryByParam = "id", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracExamViewList(string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            if (Session["PracticalExamMarks"] == null)
            {
                return RedirectToAction("PracticalExamMarks", "School");
            }
            TempData.Keep();
            if (TempData["PracticalExamMarksSearch"] != null)
            { }

            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;

                var itemStatus = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyStatus = itemStatus.ToList();
                ViewBag.SelectedStatus = SelectedStatus = "0";


                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                //ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Stdid/Refno" }, new { ID = "2", Name = "Roll No" },
                    new { ID = "3", Name = "RegNo" }, new { ID = "3", Name = "Name" },  }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "Reg.schl is not null ";
                    string SelectedAction = "0";
                    if (TempData["PracExamViewListSearch"] != null)
                    {
                        Search = TempData["PracExamViewListSearch"].ToString();
                        ViewBag.SelectedStatus = TempData["SelStatus"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelStatus"] = ViewBag.SelectedStatus;
                        TempData["SelRP"] = ViewBag.SelectedRP;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["PracExamViewListSearch"] = Search;
                    }

                    MS.StoreAllData = _schoolRepository.PracExamViewList(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {

                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];

                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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
                    if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        [SessionCheckFilter]
        [HttpPost]
        [OutputCache(Duration = 180, VaryByParam = "id", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracExamViewList(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {

                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "";
                if (frm["cid"] != null)
                {
                    id = frm["cid"].ToString();
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;
                var itemStatus = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyStatus = itemStatus.ToList();
                ViewBag.SelectedStatus = SelectedStatus = "0";
                //
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);
                //ViewBag.MyRP = itemRP.ToList();

                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                //ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Stdid/Refno" }, new { ID = "2", Name = "Roll No" },
                    new { ID = "3", Name = "RegNo" }, new { ID = "4", Name = "Name" },  }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------



                if (loginSession.SCHL != null)
                {
                    string Search = " Reg.schl is not null ";
                    Search += " and sb.pcent = '" + loginSession.SCHL + "'";
                    if (frm["SelStatus"] != "" && frm["SelStatus"] != null)
                    {
                        SelectedStatus = frm["SelStatus"].ToString();
                        ViewBag.SelectedStatus = frm["SelStatus"].ToString();
                        if (SelectedStatus == "1")
                        { Search += " and isnull(OBTMARKSP,'')='' "; }
                        else if (SelectedStatus == "2")
                        { Search += " and isnull(OBTMARKSP,'')!=''  "; }

                    }

                    if (frm["SelRP"] != "" && frm["SelRP"] != null)
                    {
                        ViewBag.SelectedRP = RP = frm["SelRP"].ToString();
                        Search += " and Reg.rp='" + RP + "'";
                    }
                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        ViewBag.SelectedAction = frm["SelAction"];
                        SelAction = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelAction == 1)
                            { Search += " and reg.std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 2)
                            { Search += " and reg.roll='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 3)
                            { Search += " and reg.regno='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 4)
                            { Search += " and reg.name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelStatus"] = frm["SelStatus"];
                    TempData["SelRP"] = frm["SelRP"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["PracExamViewListSearch"] = Search;

                    MS.StoreAllData = _schoolRepository.PracExamViewList(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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

                    if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        #endregion PracExamViewList


        #region PracExamEnterMarks
        [SessionCheckFilter]
        [OutputCache(Duration = 180, VaryByParam = "id", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracExamEnterMarks(string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            if (Session["PracticalExamMarks"] == null)
            {
                return RedirectToAction("PracticalExamMarks", "School");
            }
            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.Class = ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.RP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;
                var itemStatus = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyStatus = itemStatus.ToList();
                ViewBag.SelectedStatus = SelectedStatus = "0";

                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                // ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Stdid/Refno" }, new { ID = "2", Name = "Roll No" },
                    new { ID = "3", Name = "RegNo" }, new { ID = "3", Name = "Name" },  }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    string Search = "Reg.schl is not null  and PracFlg is null ";
                    string SelectedAction = "0";
                    if (TempData["PracExamEnterMarksSearch"] != null)
                    {
                        Search = TempData["PracExamEnterMarksSearch"].ToString();
                        ViewBag.SelectedStatus = TempData["SelStatus"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelStatus"] = ViewBag.SelectedStatus;
                        TempData["SelRP"] = ViewBag.SelectedRP;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["PracExamEnterMarksSearch"] = Search;
                    }
                    //PracExamEnterMarks
                    MS.StoreAllData = _schoolRepository.ViewPracExamEnterMarks(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.fsCount = ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());

                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcentNM"].ToString());

                        ViewBag.fsCount = Convert.ToInt32(MS.StoreAllData.Tables[2].Rows[0]["fsCount"]);

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.fsCount = Convert.ToInt32(MS.StoreAllData.Tables[2].Rows[0]["fsCount"]);
                    }
                    if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PracExamEnterMarks(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {
                // string id = "";
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "";
                if (frm["cid"] != null)
                {
                    id = frm["cid"].ToString();
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.SelectedRP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;
                var itemStatus = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyStatus = itemStatus.ToList();
                ViewBag.SelectedStatus = SelectedStatus = "0";


                var itemRP = new SelectList(new[] { new { ID = "R", Name = "REG" }, }, "ID", "Name", 1);
                //var itemRP = new SelectList(new[] { new { ID = "P", Name = "PVT" }, }, "ID", "Name", 1);  // For RE=Appear Student
                ViewBag.MyRP = itemRP.ToList();
                ViewBag.SelectedRP = "R";
                //ViewBag.SelectedRP = "P";

                //Subject Code, Subject Name,Pending
                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Stdid/Refno" }, new { ID = "2", Name = "Roll No" },
                    new { ID = "3", Name = "RegNo" }, new { ID = "4", Name = "Name" },  }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------



                if (loginSession.SCHL != null)
                {
                    string Search = "Reg.schl is not null and PracFlg is null";
                    Search += " and sb.pcent = '" + loginSession.SCHL + "'";
                    if (frm["SelStatus"] != "" && frm["SelStatus"] != null)
                    {
                        SelectedStatus = frm["SelStatus"].ToString();
                        ViewBag.SelectedStatus = frm["SelStatus"].ToString();
                        if (SelectedStatus == "1")
                        { Search += " and isnull(OBTMARKSP,'')='' "; }
                        else if (SelectedStatus == "2")
                        { Search += " and OBTMARKSP is not null and PracFlg=1 and FPLot is null "; }

                    }

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        ViewBag.SelectedAction = frm["SelAction"];
                        SelAction = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelAction == 1)
                            { Search += " and reg.std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 2)
                            { Search += " and reg.roll='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 3)
                            { Search += " and reg.regno='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelAction == 4)
                            { Search += " and reg.name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelStatus"] = frm["SelStatus"];
                    TempData["SelRP"] = frm["SelRP"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["PracExamEnterMarksSearch"] = Search;

                    MS.StoreAllData = _schoolRepository.ViewPracExamEnterMarks(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.fsCount = ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.fsCount = Convert.ToInt32(MS.StoreAllData.Tables[2].Rows[0]["fsCount"]);

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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


                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.fsCount = Convert.ToInt32(MS.StoreAllData.Tables[2].Rows[0]["fsCount"]);
                    }

                    if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[3].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }


        [HttpPost]
        public JsonResult JqPracExamEnterMarks(string SelClass, string RP, string CandSubjectPrac)
        {
            var flag = 1;

            // CandSubject myObj = JsonConvert.DeserializeObject<CandSubject>(CandSubject);
            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubjectPrac>>(CandSubjectPrac);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKSP");
            dtSub.Columns.Add("MINMARKSP");
            dtSub.Columns.Add("MAXMARKSP");
            dtSub.Columns.Add("PRACDATE");
            dtSub.Columns.Add("ACCEPT");

            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                rowObj.OBTMARKSP = rowObj.OBTMARKSP == null ? "" : rowObj.OBTMARKSP;

                row = dtSub.NewRow();
                if (rowObj.OBTMARKSP.ToUpper() == "A" || rowObj.OBTMARKSP.ToUpper() == "ABS")
                {
                    rowObj.OBTMARKSP = "ABS";
                }
                else if (rowObj.OBTMARKSP.ToUpper() == "C" || rowObj.OBTMARKSP.ToUpper() == "CAN")
                {
                    rowObj.OBTMARKSP = "CAN";
                }
                else if (rowObj.OBTMARKSP.ToUpper() == "U" || rowObj.OBTMARKSP.ToUpper() == "UMC")
                {
                    rowObj.OBTMARKSP = "UMC";
                }
                else if (rowObj.OBTMARKSP.ToUpper() == "H" || rowObj.OBTMARKSP.ToUpper() == "HHH")
                {
                    rowObj.OBTMARKSP = "HHH";
                }
                else if (rowObj.OBTMARKSP != "")
                {
                    rowObj.OBTMARKSP = rowObj.OBTMARKSP.PadLeft(3, '0');
                }

                if (rowObj.PRACDATE != "" && rowObj.OBTMARKSP != "" && rowObj.ACCEPT.ToString().ToLower() == "true")
                {
                    dtSub.Rows.Add(rowObj.CANDID, rowObj.SUB, rowObj.OBTMARKSP, rowObj.MINMARKSP, rowObj.MAXMARKSP, rowObj.PRACDATE);
                }
            }
            dtSub.AcceptChanges();


            foreach (DataRow dr1 in dtSub.Rows)
            {


                if (dr1["OBTMARKSP"].ToString() == "" || dr1["OBTMARKSP"].ToString() == "HHH" || dr1["OBTMARKSP"].ToString() == "ABS" || dr1["OBTMARKSP"].ToString() == "CAN" || dr1["OBTMARKSP"].ToString() == "UMC")
                { }
                else if (dr1["OBTMARKSP"].ToString() == "0" || dr1["OBTMARKSP"].ToString().ToUpper().Contains("A") || dr1["OBTMARKSP"].ToString().ToUpper().Contains("C") || dr1["OBTMARKSP"].ToString().ToUpper().Contains("U"))
                {
                    flag = -1;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTMARKSP"].ToString());
                    int min = Convert.ToInt32(dr1["MINMARKSP"].ToString());
                    int max = Convert.ToInt32(dr1["MAXMARKSP"].ToString());

                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1 && dtSub.Rows.Count > 0)
            {
                if (dtSub.Columns.Contains("ACCEPT"))
                {
                    dtSub.Columns.Remove("ACCEPT");
                }
                string dee = "1";
                string class1 = "4";
                int OutStatus = 0;
                string OutError = string.Empty;
                dee = _schoolRepository.AllotPracMarks(RP, dtSub, SelClass, out OutStatus, out OutError);
                var results = new
                {
                    status = OutError
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }




        //[HttpPost]
        //public JsonResult JqPracExamEnterMarks(string SelClass, string RP, string CandSubjectPrac)
        //{
        //    LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //    var flag = 1;

        //    // CandSubject myObj = JsonConvert.DeserializeObject<CandSubject>(CandSubject);
        //    var objResponse1 = JsonConvert.DeserializeObject<List<CandSubjectPrac>>(CandSubjectPrac);
        //    DataTable dtSub = new DataTable();
        //    dtSub.Columns.Add("CANDID");
        //    dtSub.Columns.Add("SUB");
        //    dtSub.Columns.Add("OBTMARKSP");
        //    dtSub.Columns.Add("MINMARKSP");
        //    dtSub.Columns.Add("MAXMARKSP");
        //    dtSub.Columns.Add("PRACDATE");
        //    dtSub.Columns.Add("ACCEPT");

        //    DataRow row = null;
        //    foreach (var rowObj in objResponse1)
        //    {
        //        rowObj.OBTMARKSP = rowObj.OBTMARKSP == null ? "" : rowObj.OBTMARKSP;

        //        row = dtSub.NewRow();
        //        if (rowObj.OBTMARKSP.ToUpper() == "A" || rowObj.OBTMARKSP.ToUpper() == "ABS")
        //        {
        //            rowObj.OBTMARKSP = "ABS";
        //        }
        //        else if (rowObj.OBTMARKSP.ToUpper() == "C" || rowObj.OBTMARKSP.ToUpper() == "CAN")
        //        {
        //            rowObj.OBTMARKSP = "CAN";
        //        }
        //        else if (rowObj.OBTMARKSP.ToUpper() == "U" || rowObj.OBTMARKSP.ToUpper() == "UMC")
        //        {
        //            rowObj.OBTMARKSP = "UMC";
        //        }
        //        else if (rowObj.OBTMARKSP.ToUpper() == "H" || rowObj.OBTMARKSP.ToUpper() == "HHH")
        //        {
        //            rowObj.OBTMARKSP = "HHH";
        //        }
        //        else if (rowObj.OBTMARKSP != "")
        //        {
        //            rowObj.OBTMARKSP = rowObj.OBTMARKSP.PadLeft(3, '0');
        //        }

        //        if (rowObj.PRACDATE != "" && rowObj.OBTMARKSP != "" && rowObj.ACCEPT.ToString().ToLower() == "true")
        //        {
        //            dtSub.Rows.Add(rowObj.CANDID, rowObj.SUB, rowObj.OBTMARKSP, rowObj.MINMARKSP, rowObj.MAXMARKSP, rowObj.PRACDATE);
        //        }
        //    }
        //    dtSub.AcceptChanges();


        //    foreach (DataRow dr1 in dtSub.Rows)
        //    {


        //        if (dr1["OBTMARKSP"].ToString() == "" || dr1["OBTMARKSP"].ToString() == "HHH" || dr1["OBTMARKSP"].ToString() == "ABS" || dr1["OBTMARKSP"].ToString() == "CAN" || dr1["OBTMARKSP"].ToString() == "UMC")
        //        { }
        //        else if (dr1["OBTMARKSP"].ToString() == "0" || dr1["OBTMARKSP"].ToString().ToUpper().Contains("A") || dr1["OBTMARKSP"].ToString().ToUpper().Contains("C") || dr1["OBTMARKSP"].ToString().ToUpper().Contains("U"))
        //        {
        //            flag = -1;
        //            var results = new
        //            {
        //                status = flag
        //            };
        //            return Json(results);
        //        }
        //        else
        //        {
        //            int obt = Convert.ToInt32(dr1["OBTMARKSP"].ToString());
        //            int min = Convert.ToInt32(dr1["MINMARKSP"].ToString());
        //            int max = Convert.ToInt32(dr1["MAXMARKSP"].ToString());

        //            if ((obt < 0) || (obt > max))
        //            {
        //                flag = -2;
        //            }
        //        }
        //    }
        //    if (flag == 1 && dtSub.Rows.Count > 0)
        //    {
        //        if (dtSub.Columns.Contains("ACCEPT"))
        //        {
        //            dtSub.Columns.Remove("ACCEPT");
        //        }
        //        string dee = "1";
        //        string class1 = "4";
        //        int OutStatus = 0;
        //        string OutError = string.Empty;
        //        dee = _schoolRepository.AllotPracMarks(RP, dtSub, SelClass, out OutStatus, out OutError);
        //        var results = new
        //        {
        //            status = OutError
        //        };
        //        return Json(results);
        //    }
        //    else
        //    {
        //        var results = new
        //        {
        //            status = flag
        //        };
        //        return Json(results);
        //    }

        //    //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
        //    // Do Stuff
        //}


        #endregion PracExamEnterMarks


        #region ViewPracExamFinalSubmit

        [SessionCheckFilter]
        public ActionResult ViewPracExamFinalSubmit(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.Class = ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.RP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;

                //------------------------

                if (loginSession.SCHL != null)
                {

                    string Search = "Reg.schl is not null and FPLot is null and OBTMARKSP is not null and PracFlg=1";
                    //string SelectedAction = "0";
                    if (TempData["ViewPracExamFinalSubmitSearch"] != null)
                    {
                        Search = TempData["ViewPracExamFinalSubmitSearch"].ToString();
                        TempData["ViewPracExamFinalSubmitSearch"] = Search;
                    }

                    MS.StoreAllData = _schoolRepository.ViewPracExamFinalSubmit(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.TotalCount1 = ViewBag.TotalCount = 0;

                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ViewPracExamFinalSubmit(FormCollection frm, int? page)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            try
            {
                string id = "";
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "";
                if (frm["cid"] != null)
                {
                    id = frm["cid"].ToString();
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;

                //------------------------



                if (loginSession.SCHL != null)
                {
                    string Search = "Reg.schl is not null and FPLot is null and OBTMARKSP is not null and PracFlg=1 ";
                    Search += " and sb.pcent = '" + loginSession.SCHL + "'";
                    TempData["ViewPracExamFinalSubmitSearch"] = Search;
                    //PracExamFinalSubmit(string class1, string rp, string cent, string Search,int SelectedAction, int pageNumber, string sub)

                    MS.StoreAllData = _schoolRepository.ViewPracExamFinalSubmit(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.TotalCount1 = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());

                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }


            return View(MS);
        }



        [HttpPost]
        public JsonResult JqRemovePracMarks(string SelClass, string RP, string CandSubjectPrac)
        {
            var flag = 1;

            // CandSubject myObj = JsonConvert.DeserializeObject<CandSubject>(CandSubject);
            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubjectPrac>>(CandSubjectPrac);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKSP");
            dtSub.Columns.Add("MINMARKSP");
            dtSub.Columns.Add("MAXMARKSP");
            dtSub.Columns.Add("PRACDATE");
            dtSub.Columns.Add("ACCEPT");

            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                rowObj.OBTMARKSP = rowObj.OBTMARKSP == null ? "" : rowObj.OBTMARKSP;

                row = dtSub.NewRow();

                if (rowObj.PRACDATE != "" && rowObj.OBTMARKSP != "" && rowObj.ACCEPT.ToString().ToLower() == "true")
                {
                    dtSub.Rows.Add(rowObj.CANDID, rowObj.SUB, rowObj.OBTMARKSP, rowObj.MINMARKSP, rowObj.MAXMARKSP, rowObj.PRACDATE);
                }
            }
            dtSub.AcceptChanges();

            if (flag == 1 && dtSub.Rows.Count > 0)
            {
                if (dtSub.Columns.Contains("ACCEPT"))
                {
                    dtSub.Columns.Remove("ACCEPT");
                }
                string dee = "1";
                string class1 = "4";
                int OutStatus = 0;
                string OutError = string.Empty;
                dee = _schoolRepository.RemovePracMarks(RP, dtSub, SelClass, out OutStatus, out OutError);
                var results = new
                {
                    status = OutError
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        #endregion PracExamFinalSubmit


        #region PracExamRoughReport
        [SessionCheckFilter]
        [OutputCache(Duration = 180, VaryByParam = "id", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracExamRoughReport(string id, int? page)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;
            if (Session["PracticalExamMarks"] == null)
            {
                return RedirectToAction("PracticalExamMarks", "School");
            }

            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.Class = ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.RP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;

                //------------------------

                if (loginSession.SCHL != null)
                {

                    string Search = "Reg.schl is not null  ";
                    Search += " and sb.pcent = '" + loginSession.SCHL + "'";

                    MS.StoreAllData = _schoolRepository.ViewPracExamFinalSubmit(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), pageIndex, SubCode);

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;

                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
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

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        #endregion PracExamRoughReport


        #region PracExamFinalReport     
        [SessionCheckFilter]
        [OutputCache(Duration = 180, VaryByParam = "id", Location = OutputCacheLocation.Client, NoStore = true)]
        public ActionResult PracExamFinalReport(string id, SchoolModels MS, string SelLot, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];


            if (Session["PracticalExamMarks"] == null)
            {
                return RedirectToAction("PracticalExamMarks", "School");
            }
            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.Class = ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.RP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.SelectedLot = SelLot == null ? "0" : SelLot;
                //------------------------

                if (loginSession.SCHL != null)
                {

                    string Search = "Reg.schl is not null and FPLot is not null and OBTMARKSP is not null and PracFlg=1";

                    if (frm["SelLot"] != "" && frm["SelLot"] != null)
                    {
                        ViewBag.SelectedLot = frm["SelLot"].ToString();
                        Search += " and FPLot='" + ViewBag.SelectedLot + "'  ";
                    }

                    MS.StoreAllData = _schoolRepository.ViewPracExamFinalSubmit(SelClass, RP, Cent, Search, Convert.ToInt32(5), 0, SubCode);

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.fplot = MS.StoreAllData.Tables[0].Rows[0]["fplot"].ToString();
                        ViewBag.fplot2 = MS.StoreAllData.Tables[0].Rows[0]["fplot2"].ToString();
                        ViewBag.PracInsDate = MS.StoreAllData.Tables[0].Rows[0]["PracInsDate"].ToString();
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());

                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        ViewBag.TotalCount1 = count;
                    }

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmax"].ToString());
                    }
                    if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                    {
                        DataTable dtLot = MS.StoreAllData.Tables[3];
                        // English
                        List<SelectListItem> itemLot = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dtLot.Rows)
                        {
                            itemLot.Add(new SelectListItem { Text = @dr["fplot"].ToString().Trim(), Value = @dr["fplot"].ToString().Trim() });
                        }
                        ViewBag.itemLot = itemLot;
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqPracExamFinalReport(string ExamCent, string SelClass, string RP, string CandPracExaminer)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;
            string SCHL = loginSession.SCHL.ToString();
            string SUB = "";
            string CENT = "";
            DataTable dtSub = new DataTable();
            if (CandPracExaminer != null)
            {
                // CandSubject myObj = JsonConvert.DeserializeObject<CandSubject>(CandSubject);
                var objResponse1 = JsonConvert.DeserializeObject<List<CandPracExaminer>>(CandPracExaminer);
                dtSub.Columns.Add("SUB");
                dtSub.Columns.Add("CENT");
                dtSub.Columns.Add("EXAMINER");
                dtSub.Columns.Add("SCHOOL");
                dtSub.Columns.Add("TEACHER");
                dtSub.Columns.Add("MOBILE");

                DataRow row = null;
                foreach (var rowObj in objResponse1)
                {
                    //rowObj.OBTMARKSP = rowObj.OBTMARKSP == null ? "" : rowObj.OBTMARKSP;

                    row = dtSub.NewRow();

                    if (rowObj.SUB != "" && rowObj.CENT != "" && rowObj.EXAMINER != "" && rowObj.SCHOOL != "" && rowObj.TEACHER != "" && rowObj.MOBILE != "")
                    {
                        SUB = rowObj.SUB;
                        CENT = rowObj.CENT;
                        dtSub.Rows.Add(rowObj.SUB, rowObj.CENT, rowObj.EXAMINER, rowObj.SCHOOL, rowObj.TEACHER, rowObj.MOBILE);
                    }
                }
                dtSub.AcceptChanges();
            }

            //if (flag == 1 && dtSub.Rows.Count > 0 && SUB!="" && CENT != "" && SCHL != "")
            if (flag == 1 && SUB != "" && CENT != "" && SCHL != "")
            {
                if (string.IsNullOrEmpty(ExamCent))
                {
                    ExamCent = "";
                }

                string dee = "0";
                int OutStatus = 0;
                string OutError = string.Empty;
                dee = _schoolRepository.PracExamFinalSubmit(ExamCent, SelClass, RP, CENT, SUB, SCHL, dtSub, out OutStatus, out OutError);
                var results = new
                {
                    status = OutError
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }


        #endregion PracExamFinalReport



        #region Prac Exam Final Submit      
        [SessionCheckFilter]
        public ActionResult PracExamFinalSubmit(string id, SchoolModels MS)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            if (Session["PracticalExamMarks"] == null)
            {
                return RedirectToAction("PracticalExamMarks", "School");
            }
            try
            {
                string SelClass = "";
                string RP = "";
                string Cent = "";
                string SubCode = "";
                string SelectedStatus = "0";
                if (id != null)
                {
                    string[] split = id.Split('-');
                    //var parm = Class + "-" + ExamType + "-" + Centre + "-" + SUB;
                    ViewBag.Class = ViewBag.SelClass = SelClass = split[0].ToString();
                    ViewBag.RP = ViewBag.SelRP = RP = split[1].ToString();
                    ViewBag.SelCent = Cent = split[2].ToString();
                    SubCode = split[3].ToString();
                    TempData["cid"] = ViewBag.cid = id.ToString();
                }

                ViewBag.schlCode = loginSession.SCHL;

                //------------------------

                if (loginSession.SCHL != null)
                {

                    string Search = "Reg.schl is not null and FPLot is null and OBTMARKSP is not null and PracFlg=1";
                    MS.StoreAllData = _schoolRepository.ViewPracExamFinalSubmit(SelClass, RP, Cent, Search, Convert.ToInt32(SelectedStatus), 0, SubCode);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.LastDateofSub = null;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.Unlocked = ViewBag.TotalCount = 0;
                    }
                    else
                    {
                        ViewBag.LastDateofSub = MS.StoreAllData.Tables[0].Rows[0]["LastDate"];
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        ViewBag.Unlocked = Convert.ToInt32(MS.StoreAllData.Tables[0].Rows[0]["Unlocked"].ToString());
                        ViewBag.TotalCount1 = count;
                    }

                    if (MS.StoreAllData.Tables[2].Rows.Count > 0)
                    {
                        ViewBag.CentreCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcent"].ToString());
                        ViewBag.CentreName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["pcentNM"].ToString());

                        ViewBag.SubCode = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["sub"].ToString());
                        ViewBag.SubName = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["subnm"].ToString());

                        ViewBag.PrMin = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmin"].ToString());
                        ViewBag.PrMax = Convert.ToString(MS.StoreAllData.Tables[2].Rows[0]["prmax"].ToString());
                    }

                    return View(MS);
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }
            return View(MS);
        }



        #endregion Prac Exam Final Submit

        #endregion    Practical Exams



        #region Signature Chart and Confidential List Primary Middle Both
        [SessionCheckFilter]
        public ActionResult SignatureChart(string id, SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChart", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            string Schl = loginSession.SCHL.ToString();
            string Cent = "";
            try
            {
                if (Schl != "")
                {
                    DataSet Dresult = _schoolRepository.SignatureChart(1, sm.CLASS, Schl, Cent);
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    List<SelectListItem> Sublist = new List<SelectListItem>();

                    if (Dresult.Tables[0].Rows.Count > 0)
                    {
                        Cent = Dresult.Tables[0].Rows[0]["cent"].ToString();
                        sm.ExamCent = Cent;
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                        }

                    }

                    if (Dresult.Tables[1].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in Dresult.Tables[1].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                    }

                    ViewBag.MySchCode = schllist;
                    ViewBag.MyExamSub = Sublist;

                    sm.ExamCent = Cent;
                    sm.ExamRoll = sm.ExamSub = "";

                    return View();
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SignatureChart(string id, SchoolModels sm, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChart", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = frc["ExamCent"].ToString();
                string roll = frc["ExamRoll"].ToString();
                if (Cent != "")
                {

                    sm.ExamCent = Cent;
                    sm.ExamSub = frc["ExamSub"].ToString();
                    sm.ExamRoll = frc["ExamRoll"].ToString();

                    DataSet Dresult = _schoolRepository.SignatureChart(1, sm.CLASS, Schl, Cent);
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                    {
                        schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                    }

                    ViewBag.MySchCode = schllist;
                    List<SelectListItem> Sublist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[1].Rows)
                    {
                        Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                    }

                    ViewBag.MyExamSub = Sublist;

                    sm.StoreAllData = _schoolRepository.GetSignatureChart(sm);
                    sm.ExamCent = Cent;
                    ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                    if (ViewBag.SearchMsg == 0)
                    {
                        ViewBag.Message = "No Record Found";
                    }
                    return View(sm);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        public ActionResult ConfidentialList(string id, SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChart", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = "";
                if (Schl != "")
                {

                    DataSet Dresult = _schoolRepository.SignatureChart(1, sm.CLASS, Schl, Cent);
                    List<SelectListItem> schllist = new List<SelectListItem>();

                    if (Dresult.Tables[0].Rows.Count > 0)
                    {
                        Cent = Dresult.Tables[0].Rows[0]["cent"].ToString();
                        sm.ExamCent = Cent;
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                        }

                    }
                    ViewBag.MySchCode = schllist;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ConfidentialList(string id, SchoolModels sm, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChart", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;
            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = frc["ExamCent"].ToString();
                if (Cent != "")
                {

                    sm.ExamCent = Cent;
                    DataSet Dresult = _schoolRepository.SignatureChart(1, sm.CLASS, Schl, Cent);
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                    {
                        schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                    }

                    ViewBag.MySchCode = schllist;
                    if (frc["ExamCent"].ToString() != "")
                    {
                        sm.ExamCent = frc["ExamCent"].ToString();
                    }

                    sm.StoreAllData = _schoolRepository.GetConfidentialList(sm);
                    ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                    return View(sm);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        #endregion  Signature Chart and Confidential List Matric

        #region CapacityLetter

        [SessionCheckFilter]
        public ActionResult CapacityLetter(SchoolModels SM)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            ViewBag.Date = DateTime.Now.ToString("dd/MM/yyyy");
            try
            {
                SM.StoreAllData = _schoolRepository.CapacityLetter(loginSession.SCHL);
                if (SM.StoreAllData == null || SM.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "DATA DOESN'T EXIST";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = SM.StoreAllData.Tables[0].Rows.Count;
                    return View(SM);
                }

            }
            catch (Exception ex)
            {
                return View();

            }
        }
        [HttpPost]
        public ActionResult CapacityLetter(SchoolModels SM, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string schl = null;
            ViewBag.Date = DateTime.Now.ToString("dd/MM/yyyy");
            try
            {
                //if ((Session["SCHL"] == null || Session["SCHL"].ToString() == "") && Session["USER"].ToString().ToUpper() != "ADMIN")
                //{
                //    Session.Clear();
                //    return RedirectToAction("Logout", "Login");
                //}

                //if (Session["USER"].ToString().ToUpper() == "ADMIN")
                //{
                //    DataSet Dresult = _schoolRepository.AdminGetALLSCHL(); //
                //    List<SelectListItem> DistList = new List<SelectListItem>();
                //    foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                //    {
                //        DistList.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["CSCHL"].ToString() });
                //    }
                //    ViewBag.Dist = DistList;
                //}
                schl = frc["SelDist"].ToString();

                SM.StoreAllData = _schoolRepository.CapacityLetter(schl);
                if (SM.StoreAllData == null || SM.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "DATA DOESN'T EXIST";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = SM.StoreAllData.Tables[0].Rows.Count;
                    return View(SM);
                }

            }
            catch (Exception ex)
            {
                return View();

            }
        }

        #endregion CapacityLetter


        #region ExamCentre

        [SessionCheckFilter]
        public ActionResult ExamCentre(SchoolModels SM)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            return View();
        }
        #endregion ExamCentre


        #region Begin PhyChlMarksEntry   


        [SessionCheckFilter]
        public ActionResult PhyChl_Portal()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PhyChl_MarksSearch"] = null;
            return View();
        }


        [SessionCheckFilter]
        public ActionResult PhyChlMarksEntry_Agree(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PhyChlMarksEntryClass"] = id;
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PhyChlMarksEntry_Agree(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string cc = id;
                string s = frm["Agree"].ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("PhyChlMarksEntry_Portal", "School");
                }
                else
                {
                    if (s == "Agree")
                    {
                        return RedirectToAction("PhyChlMarksEntryPanel", "School", new { id = cc });
                    }
                }
                return RedirectToAction("PhyChlMarksEntry_Portal", "School");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        public ActionResult PhyChlMarksEntryPanel(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                string SelectedAction = "0";


                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    if (TempData["PhyChlMarksEntryPanelSearch"] != null)
                    {
                        Search += TempData["PhyChlMarksEntryPanelSearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelFilter"] = ViewBag.SelectedFilter;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["PhyChlMarksEntryPanelSearch"] = Search;
                    }
                    else
                    {
                        Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                    }



                    MS.StoreAllData = _schoolRepository.GetPhyChlMarksEntryMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, Convert.ToInt32(SelectedAction));
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsMarksFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View(MS);
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);

                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsMarksFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }

                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PhyChlMarksEntryPanel(string id, FormCollection frm, int? page)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "";
                    Search = "  a.SCHL = '" + loginSession.SCHL + "' and  a.class='" + CLASS + "' ";

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "")
                        {
                            if (SelValueSch == 3)
                            {
                                SelAction = 2;
                            }
                            //  { Search += " and  IsMarksFilled=1 "; } // Filled
                            else if (SelValueSch == 2)
                            { SelAction = 1; }
                            //{ Search += " and (IsMarksFilled is null or IsMarksFilled=0) "; } // pending
                        }
                        ViewBag.SelectedAction = frm["SelAction"];
                    }

                    if (frm["SelFilter"] != "")
                    {

                        ViewBag.SelectedFilter = frm["SelFilter"];
                        int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                        if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and b.Roll='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelFilter"] = frm["SelFilter"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["PhyChlMarksEntryPanelSearch"] = Search;
                    // string class1 = "4";
                    MS.StoreAllData = _schoolRepository.GetPhyChlMarksEntryMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, SelAction);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsMarksFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsMarksFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqPhyChlMarksEntryMarks(string stdid, string CandSubject, string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;

            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubject>>(CandSubject);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKS");
            dtSub.Columns.Add("MINMARKS");
            dtSub.Columns.Add("MAXMARKS");
            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                row = dtSub.NewRow();
                if (rowObj.OBTMARKS == "A" || rowObj.OBTMARKS == "ABS")
                {
                    rowObj.OBTMARKS = "ABS";
                }
                else if (rowObj.OBTMARKS == "C" || rowObj.OBTMARKS == "CAN")
                {
                    rowObj.OBTMARKS = "CAN";
                }
                else if (rowObj.OBTMARKS == "O" || rowObj.OBTMARKS == "OC")
                {
                    rowObj.OBTMARKS = "OC";
                }
                else if (rowObj.OBTMARKS != "")
                {
                    rowObj.OBTMARKS = rowObj.OBTMARKS.PadLeft(3, '0');
                }
                else if (string.IsNullOrEmpty(rowObj.OBTMARKS))
                {
                    rowObj.OBTMARKS = "";
                }
                if (rowObj.MINMARKS == "--" || rowObj.MINMARKS == "")
                {
                    rowObj.MINMARKS = "000";
                }

                dtSub.Rows.Add(stdid, rowObj.SUB, rowObj.OBTMARKS, rowObj.MINMARKS, rowObj.MAXMARKS);
            }
            dtSub.AcceptChanges();


            foreach (DataRow dr1 in dtSub.Rows)
            {
                if (dr1["OBTMARKS"].ToString() == "" || dr1["OBTMARKS"].ToString() == "OC" || dr1["OBTMARKS"].ToString() == "ABS" || dr1["OBTMARKS"].ToString() == "CAN")
                { }
                else if (dr1["OBTMARKS"].ToString() == "0" || dr1["OBTMARKS"].ToString().Contains("A") || dr1["OBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTMARKS"].ToString());
                    int min = Convert.ToInt32(dr1["MINMARKS"].ToString());
                    int max = Convert.ToInt32(dr1["MAXMARKS"].ToString());

                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                // string class1 = "5";
                string class1 = cls == "Primary" ? "5" : cls == "Middle" ? "8" : "5";
                int OutStatus = 0;
                dee = _schoolRepository.AllotPhyChlMarksEntryMarks(loginSession.SCHL, stdid, dtSub, class1, out OutStatus);
                var results = new
                {
                    status = OutStatus
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        [SessionCheckFilter]
        public ActionResult PhyChlMarksEntryReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PhyChlMarksEntryPanelSearch"] = null;
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                MS.StoreAllData = _schoolRepository.PhyChlMarksEntryMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        public ActionResult PhyChlMarksEntryFinalReport(string id, FormCollection frm)
        {
            TempData["PhyChlMarksEntryPanelSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                DataSet dsFinal = new DataSet();
                MS.StoreAllData = dsFinal = _schoolRepository.PhyChlMarksEntryMarksEntryReport(loginSession.SCHL, 1, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowPhyChlMarksEntry = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowPhyChlMarksEntry = 1;
                    MS.StoreAllData = _schoolRepository.PhyChlMarksEntryMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.MarksFilledDate = MS.StoreAllData.Tables[0].Rows[0]["MarksFilledDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }

                    if (dsFinal.Tables[3].Rows.Count > 0)
                    {
                        MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                        {
                            Cls = dsFinal.Tables[3].Rows[0]["Cls"].ToString(),
                            IsActive = Convert.ToInt32(dsFinal.Tables[3].Rows[0]["IsActive"].ToString()),
                            IsAllow = dsFinal.Tables[3].Rows[0]["IsAllow"].ToString(),
                            LastDate = Convert.ToString(dsFinal.Tables[3].Rows[0]["LastdateDT"].ToString()),
                            Panel = Convert.ToString(dsFinal.Tables[3].Rows[0]["Panel"].ToString())
                        };
                    }
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowPhyChlMarksEntry = 2;
                    ViewBag.IsFinal = 0;
                    ViewBag.TotalCount1 = 0;
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];

                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PhyChlMarksEntryFinalReport(string id)
        {
            TempData["PhyChlMarksEntryPanelSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;
                string Search = string.Empty;
                string OutError = "";
                MS.StoreAllData = _schoolRepository.PhyChlMarksEntryMarksEntryReport(loginSession.SCHL, 2, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.MarksFilledDate = MS.StoreAllData.Tables[0].Rows[0]["MarksFilledDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }


        #endregion  End PhyChlMarksEntry Senior 




        #region  School Result Declare 

        [SessionCheckFilter]
        public ActionResult SchoolResultDeclare()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            return View();
        }



        [SessionCheckFilter]
        public ActionResult ResultDeclareSchoolWise(string id)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                 new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                string Search = string.Empty;
                Search = "a.SCHL Like '%' ";


                MS.StoreAllData = _schoolRepository.GetSchoolResultDetails(Search, loginSession.SCHL, CLASS, "R");
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    Session["SchoolResultDeclare"] = null;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    Session["SchoolResultDeclare"] = MS.StoreAllData.Tables[0];
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;


                    return View(MS);
                }

            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ResultDeclareSchoolWise(FormCollection frm, string id, string cmd, string submit)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                 new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                string Search = string.Empty;
                Search = "a.SCHL Like '%' ";


                if (frm["SelFilter"] != "")
                {
                    TempData["SelFilter"] = frm["SelFilter"];
                    ViewBag.SelectedFilter = frm["SelFilter"];
                    int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                    if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and a.roll='" + frm["SearchString"].ToString() + "'"; }
                        if (SelValueSch == 2)
                        { Search += "and d.Std_id='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 3)
                        { Search += " and d.Regno like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 4)
                        { Search += "and d.Name like '%" + frm["SearchString"].ToString() + "%'"; }
                    }
                }

                MS.StoreAllData = _schoolRepository.GetSchoolResultDetails(Search, loginSession.SCHL, CLASS, "R");
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    Session["SchoolResultDeclare"] = null;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    Session["SchoolResultDeclare"] = MS.StoreAllData.Tables[0];
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;


                    return View(MS);
                }

            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View(MS);
        }




        #endregion  School Result Declare 




        #region  View Student Certificate

        [SessionCheckFilter]
        public ActionResult SchoolResultCertificate()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            return View();
        }



        [SessionCheckFilter]
        public ActionResult StudentResultCertificate(string id)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                 new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                string Search = string.Empty;
                Search = "a.SCHL Like '%' ";


                MS.StoreAllData = _schoolRepository.GetSchoolResultDetails(Search, loginSession.SCHL, CLASS, "R");
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    Session["StudentResultCertificate"] = null;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    Session["StudentResultCertificate"] = MS.StoreAllData.Tables[0];
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;


                    return View(MS);
                }

            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult StudentResultCertificate(FormCollection frm, string id, string cmd, string submit)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                 new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                string Search = string.Empty;
                Search = "a.SCHL Like '%' ";


                if (frm["SelFilter"] != "")
                {
                    TempData["SelFilter"] = frm["SelFilter"];
                    ViewBag.SelectedFilter = frm["SelFilter"];
                    int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                    if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and a.roll='" + frm["SearchString"].ToString() + "'"; }
                        if (SelValueSch == 2)
                        { Search += "and d.Std_id='" + frm["SearchString"].ToString() + "'"; }
                        else if (SelValueSch == 3)
                        { Search += " and d.Regno like '%" + frm["SearchString"].ToString() + "%'"; }
                        else if (SelValueSch == 4)
                        { Search += "and d.Name like '%" + frm["SearchString"].ToString() + "%'"; }
                    }
                }

                MS.StoreAllData = _schoolRepository.GetSchoolResultDetails(Search, loginSession.SCHL, CLASS, "R");
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    Session["StudentResultCertificate"] = null;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    Session["StudentResultCertificate"] = MS.StoreAllData.Tables[0];
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;


                    return View(MS);
                }

            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View(MS);
        }




        #endregion  StudentResultCertificate 


        #region Begin  PreBoardExamTheory
        [SessionCheckFilter]
        public ActionResult PreBoardExamTheory_Portal()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PreBoardExamTheorySearch"] = null;
            return View();
        }
        [SessionCheckFilter]
        public ActionResult PreBoardExamTheoryAgree(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PreBoardExamTheoryClass"] = id;
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PreBoardExamTheoryAgree(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string cc = id;
                string s = frm["Agree"].ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("PreBoardExamTheory_Portal", "School");
                }
                else
                {
                    if (s == "Agree")
                    {
                        return RedirectToAction("PreBoardExamTheory", "School", new { id = cc });
                    }
                }
                return RedirectToAction("PreBoardExamTheory_Portal", "School");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        public ActionResult PreBoardExamTheory(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                string SelectedAction = "0";


                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    if (TempData["PreBoardExamTheorySearch"] != null)
                    {
                        Search += TempData["PreBoardExamTheorySearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelFilter"] = ViewBag.SelectedFilter;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["PreBoardExamTheorySearch"] = Search;
                    }
                    else
                    {
                        Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                    }



                    MS.StoreAllData = _schoolRepository.GetPreBoardExamTheoryBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, Convert.ToInt32(SelectedAction));
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {

                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);

                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsPreBoardMarksFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }

                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PreBoardExamTheory(string id, FormCollection frm, int? page)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "";
                    Search = "  a.SCHL = '" + loginSession.SCHL + "' and  a.class='" + CLASS + "' ";

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "")
                        {
                            if (SelValueSch == 3)
                            {
                                SelAction = 2;
                            }
                            //  { Search += " and  IsCCEFilled=1 "; } // Filled
                            else if (SelValueSch == 2)
                            { SelAction = 1; }
                            //{ Search += " and (IsCCEFilled is null or IsCCEFilled=0) "; } // pending
                        }
                        ViewBag.SelectedAction = frm["SelAction"];
                    }

                    if (frm["SelFilter"] != "")
                    {

                        ViewBag.SelectedFilter = frm["SelFilter"];
                        int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                        if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and b.Roll='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelFilter"] = frm["SelFilter"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["PreBoardExamTheorySearch"] = Search;
                    // string class1 = "4";
                    MS.StoreAllData = _schoolRepository.GetPreBoardExamTheoryBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, SelAction);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsPreBoardMarksFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsCCEFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqPreBoardExamTheory(string stdid, string CandSubject, string cls)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;

            // CandSubject myObj = JsonConvert.DeserializeObject<CandSubject>(CandSubject);
            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubjectPreBoard>>(CandSubject);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKS");
            dtSub.Columns.Add("MINMARKS");
            dtSub.Columns.Add("MAXMARKS");
            dtSub.Columns.Add("PROBTMARKS");
            dtSub.Columns.Add("PRMINMARKS");
            dtSub.Columns.Add("PRMAXMARKS");
            dtSub.Columns.Add("INOBTMARKS");
            dtSub.Columns.Add("INMINMARKS");
            dtSub.Columns.Add("INMAXMARKS");
            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                row = dtSub.NewRow();
                //OBTMARKS
                if (rowObj.OBTMARKS == "A" || rowObj.OBTMARKS == "ABS")
                {
                    rowObj.OBTMARKS = "ABS";
                }
                else if (rowObj.OBTMARKS == "C" || rowObj.OBTMARKS == "CAN")
                {
                    rowObj.OBTMARKS = "CAN";
                }
                else if (string.IsNullOrEmpty(rowObj.OBTMARKS))
                {
                    rowObj.OBTMARKS = "";
                }
                else if (rowObj.OBTMARKS != "")
                {
                    rowObj.OBTMARKS = rowObj.OBTMARKS.PadLeft(3, '0');
                }

                //PROBTMARKS
                if (rowObj.PROBTMARKS == "A" || rowObj.PROBTMARKS == "ABS")
                {
                    rowObj.PROBTMARKS = "ABS";
                }
                else if (rowObj.PROBTMARKS == "C" || rowObj.PROBTMARKS == "CAN")
                {
                    rowObj.PROBTMARKS = "CAN";
                }
                else if (string.IsNullOrEmpty(rowObj.PROBTMARKS))
                {
                    rowObj.PROBTMARKS = "";
                }
                else if (rowObj.PROBTMARKS != "")
                {
                    rowObj.PROBTMARKS = rowObj.PROBTMARKS.PadLeft(3, '0');
                }

                //INOBTMARKS
                if (rowObj.INOBTMARKS == "A" || rowObj.INOBTMARKS == "ABS")
                {
                    rowObj.INOBTMARKS = "ABS";
                }
                else if (rowObj.INOBTMARKS == "C" || rowObj.INOBTMARKS == "CAN")
                {
                    rowObj.INOBTMARKS = "CAN";
                }
                else if (string.IsNullOrEmpty(rowObj.INOBTMARKS))
                {
                    rowObj.INOBTMARKS = "";
                }
                else if (rowObj.INOBTMARKS != "")
                {
                    rowObj.INOBTMARKS = rowObj.INOBTMARKS.PadLeft(3, '0');
                }

                if (string.IsNullOrEmpty(rowObj.PRMINMARKS) || rowObj.PRMINMARKS == "--")
                {
                    rowObj.PRMINMARKS = "000";
                }
                if (string.IsNullOrEmpty(rowObj.PRMAXMARKS) || rowObj.PRMAXMARKS == "--")
                {
                    rowObj.PRMAXMARKS = "000";
                }
                if (string.IsNullOrEmpty(rowObj.INMINMARKS) || rowObj.INMINMARKS == "--")
                {
                    rowObj.INMINMARKS = "000";
                }
                if (string.IsNullOrEmpty(rowObj.INMAXMARKS) || rowObj.INMAXMARKS == "--")
                {
                    rowObj.INMAXMARKS = "000";
                }

                if (!string.IsNullOrEmpty(rowObj.SUB))
                {
                    dtSub.Rows.Add(stdid, rowObj.SUB, rowObj.OBTMARKS, rowObj.MINMARKS, rowObj.MAXMARKS,
                         rowObj.PROBTMARKS, rowObj.PRMINMARKS, rowObj.PRMAXMARKS,
                          rowObj.INOBTMARKS, rowObj.INMINMARKS, rowObj.INMAXMARKS);
                }
            }
            dtSub.AcceptChanges();
            //dtSub =  AbstractLayer.StaticDB.RemoveEmptyAndDuplicateRowFromDataTable(dtSub, "SUB");
            // dtSub.AcceptChanges();
            foreach (DataRow dr1 in dtSub.Rows)
            {
                if (dr1["OBTMARKS"].ToString() == "" || dr1["OBTMARKS"].ToString() == "ABS" || dr1["OBTMARKS"].ToString() == "CAN")
                { }
                else if (dr1["OBTMARKS"].ToString().Contains("A") || dr1["OBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else if (dr1["PROBTMARKS"].ToString().Contains("A") || dr1["PROBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else if (dr1["INOBTMARKS"].ToString().Contains("A") || dr1["INOBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    string SUB = dr1["SUB"].ToString();
                    if (!string.IsNullOrEmpty(SUB))
                    {
                        string OBTMARKS = dr1["OBTMARKS"].ToString() == "" ? "000" : dr1["OBTMARKS"].ToString();
                        string MINMARKS = dr1["MINMARKS"].ToString() == "" ? "000" : dr1["MINMARKS"].ToString();
                        string MAXMARKS = dr1["MAXMARKS"].ToString() == "" ? "000" : dr1["MAXMARKS"].ToString();


                        string PROBTMARKS = dr1["PROBTMARKS"].ToString() == "" ? "000" : dr1["PROBTMARKS"].ToString();
                        string PRMINMARKS = dr1["PRMINMARKS"].ToString() == "" ? "000" : dr1["PRMINMARKS"].ToString();
                        string PRMAXMARKS = dr1["PRMAXMARKS"].ToString() == "" ? "000" : dr1["PRMAXMARKS"].ToString();

                        string INOBTMARKS = dr1["INOBTMARKS"].ToString() == "" ? "000" : dr1["INOBTMARKS"].ToString();
                        string INMINMARKS = dr1["INMINMARKS"].ToString() == "" ? "000" : dr1["INMINMARKS"].ToString();
                        string INMAXMARKS = dr1["INMAXMARKS"].ToString() == "" ? "000" : dr1["INMAXMARKS"].ToString();


                        int obt = Convert.ToInt32(OBTMARKS);
                        int min = Convert.ToInt32(MINMARKS);
                        int max = Convert.ToInt32(MAXMARKS);

                        if ((obt < 0) || (obt > max))
                        {
                            flag = -2;
                        }

                        int PRobt = Convert.ToInt32(PROBTMARKS);
                        int PRmin = Convert.ToInt32(PRMINMARKS);
                        int PRmax = Convert.ToInt32(PRMAXMARKS);

                        if ((PRobt < 0) || (PRobt > PRmax))
                        {
                            flag = -2;
                        }


                        int INobt = Convert.ToInt32(INOBTMARKS);
                        int INmin = Convert.ToInt32(INMINMARKS);
                        int INmax = Convert.ToInt32(INMAXMARKS);

                        if ((INobt < 0) || (INobt > INmax))
                        {
                            flag = -2;
                        }
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                // string class1 = "5";
                string class1 = cls == "Primary" ? "5" : cls == "Middle" ? "8" : "5";
                int OutStatus = 0;
                dee = _schoolRepository.AllotPreBoardExamTheoryPM(loginSession.SCHL, stdid, dtSub, class1, out OutStatus);
                var results = new
                {
                    status = OutStatus
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        [SessionCheckFilter]
        public ActionResult PreBoardExamTheoryReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["PreBoardExamTheorySearch"] = null;
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                MS.StoreAllData = _schoolRepository.PreBoardExamTheoryReportPM(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        public ActionResult PreBoardExamTheoryFinalReport(string id, FormCollection frm)
        {
            TempData["PreBoardExamTheorySearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                DataSet dsFinal = new DataSet();
                MS.StoreAllData = dsFinal = _schoolRepository.PreBoardExamTheoryReportPM(loginSession.SCHL, 1, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowCCE = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowCCE = 1;
                    MS.StoreAllData = _schoolRepository.PreBoardExamTheoryReportPM(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.CCEDate = MS.StoreAllData.Tables[0].Rows[0]["PreBoardDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }
                    if (dsFinal.Tables[3].Rows.Count > 0)
                    {
                        MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                        {
                            Cls = dsFinal.Tables[3].Rows[0]["Cls"].ToString(),
                            IsActive = Convert.ToInt32(dsFinal.Tables[3].Rows[0]["IsActive"].ToString()),
                            IsAllow = dsFinal.Tables[3].Rows[0]["IsAllow"].ToString(),
                            LastDate = Convert.ToString(dsFinal.Tables[3].Rows[0]["LastdateDT"].ToString()),
                            Panel = Convert.ToString(dsFinal.Tables[3].Rows[0]["Panel"].ToString())
                        };
                    }
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowCCE = 2;
                    ViewBag.IsFinal = 0;
                    ViewBag.TotalCount1 = 0;
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult PreBoardExamTheoryFinalReport(string id)
        {
            TempData["PreBoardExamTheorySearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;
                string Search = string.Empty;
                string OutError = "";
                MS.StoreAllData = _schoolRepository.PreBoardExamTheoryReportPM(loginSession.SCHL, 2, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.PreBoardDate = MS.StoreAllData.Tables[0].Rows[0]["PreBoardDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }


        #endregion  End PreBoardExamTheory



        #region School to School Migration


        [SessionCheckFilter]
        public ActionResult ApplyStudentSchoolMigration()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            string Schl = loginSession.SCHL;
            MigrateSchoolModels MS = new MigrateSchoolModels();
            try
            {

                var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Mobile No"},new{ID="5",Name="E-Punjab ID"},}, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();


                //GetAllowedGroupListBySchool
                List<SelectListItem> MyGroupList = new List<SelectListItem>();
                ViewBag.MyGroup = MyGroupList;

            }
            catch (Exception ex)
            {
            }

            return View();
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ApplyStudentSchoolMigration(FormCollection frm, string SelFilter, string SearchString)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            MigrateSchoolModels MS = new MigrateSchoolModels();

            string Schl = loginSession.SCHL.ToString();

            var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Mobile No"},new{ID="5",Name="E-Punjab ID"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();

            //GetAllowedGroupListBySchool
            //List<SelectListItem> MyGroupList = DBClass.GetAllowedGroupListBySchool(Session["SCHL"].ToString());
            //ViewBag.MyGroup = MyGroupList;
            List<SelectListItem> MyGroupList = new List<SelectListItem>();
            ViewBag.MyGroup = MyGroupList;


            string Search = "SCHL!='" + Schl + "'  and class  in (5,8) ";
            if (string.IsNullOrEmpty(SelFilter) || string.IsNullOrEmpty(SearchString))
            {
                ViewBag.TotalCount = -1;
            }
            else
            {

                ViewBag.SelectedItem = SelFilter;
                int SelValueSch = Convert.ToInt32(SelFilter.ToString());
                if (SearchString != "")
                {
                    if (SelValueSch == 1)
                    { Search += " and std_id='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 2)
                    { Search += " and  Registration_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 3)
                    { Search += " and Aadhar_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 4)
                    { Search += " and Mobile ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 5)
                    { Search += " and E_punjab_Std_id ='" + frm["SearchString"].ToString() + "'"; }
                }
            }

            MS.StoreAllData = SchoolDB.ApplyStudentSchoolMigrationSearch(0, Search, Schl);
            if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
            }
            return View(MS);
        }

        [HttpPost]
        public ActionResult AddStudentMigration(StudentSchoolMigrations studentSchoolMigration)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string Schl = loginSession.SCHL.ToString();
            string outid = "0";
            string outStatus = "0";
            string filename = "";
            string FilepathExist = "", path = "";
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                var submitDataModel = Request.Files;

                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.                     

                        string ext = Path.GetExtension(file.FileName);
                        filename = studentSchoolMigration.StdId + "_" + studentSchoolMigration.NewSCHL + "_SchoolMigration" + ext;
                        studentSchoolMigration.StudentMigrationLetter = "allfiles/Upload2024/SchoolMigration/StudentMigrationLetter/" + filename;


                        using (var client = new AmazonS3Client(ConfigurationManager.AppSettings["AWSKey"], ConfigurationManager.AppSettings["AWSValue"], RegionEndpoint.APSouth1))
                        {
                            using (var newMemoryStream = new MemoryStream())
                            {
                                ///file.CopyTo(newMemoryStream);

                                var uploadRequest = new TransferUtilityUploadRequest
                                {
                                    InputStream = file.InputStream,
                                    Key = string.Format("allfiles/Upload2024/SchoolMigration/StudentMigrationLetter/{0}", filename),
                                    BucketName = BUCKET_NAME,
                                    CannedACL = S3CannedACL.PublicRead
                                };

                                var fileTransferUtility = new TransferUtility(client);
                                fileTransferUtility.Upload(uploadRequest);
                            }
                        }

                    }


                    studentSchoolMigration.MigrationId = 0;
                    //                    studentSchoolMigration.RegNo = "";
                    studentSchoolMigration.IsActive = true;
                    studentSchoolMigration.CreatedDate = DateTime.Now;
                    studentSchoolMigration.UpdatedDate = DateTime.Now;
                    studentSchoolMigration.IsDeleted = false;
                    studentSchoolMigration.userName = Schl;
                    if (studentSchoolMigration.NewSCHL.Length < 7)
                    {
                        studentSchoolMigration.NewSCHL = Schl;
                    }

                    //studentSchoolMigration.AppLevel = "SCHL";
                    studentSchoolMigration.MigrationStatusCode = 1; //Applied & Forward to Old School
                    if (studentSchoolMigration.AppLevel.ToUpper() == "SCHL")
                    {
                        studentSchoolMigration.IsAppBySchl = 1;
                        studentSchoolMigration.IsAppByHOD = 0;
                    }
                    else if (studentSchoolMigration.AppLevel.ToUpper() == "HOD")
                    {
                        studentSchoolMigration.IsAppBySchl = 0;
                        studentSchoolMigration.IsAppByHOD = 1;
                    }
                    else
                    {
                        studentSchoolMigration.IsAppBySchl = 0;
                        studentSchoolMigration.IsAppByHOD = 0;
                    }

                    studentSchoolMigration.IsCancel = 0;
                    studentSchoolMigration.UserIP = StaticDB.GetFullIPAddress();

                    _context.StudentSchoolMigrations.Add(studentSchoolMigration);
                    int insertedRecords = _context.SaveChanges();
                    //int insertedRecords =0;
                    if (insertedRecords > 0)
                    {
                        // Returns message that successfully uploaded  
                        outid = "1";
                        outStatus = "Successfully";
                        //DataSet ds = new DataSet();
                        //SchoolModels sm = new SchoolDB().GetSchoolDataBySchl(studentSchoolMigration.CurrentSCHL, out ds);

                        if (!string.IsNullOrEmpty(loginSession.MOBILE.ToString()))
                        {
                            string SchoolMobile = loginSession.MOBILE.ToString();
                            string Sms = "School to School Migration of Student " + studentSchoolMigration.StdId + " of Class is applied by new school. Check there request under School Migration -> Received List and take necessary action. Regards PSEB";
                            try
                            {
                                string getSms = DBClass.gosms(SchoolMobile, Sms);
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                    {
                        outStatus = "Failure";
                        outid = "-1";
                    }

                    return Json(new { migid = outid, status = outStatus }, JsonRequestBehavior.AllowGet);
                    // return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    outid = "err";
                    outStatus = ex.Message;
                    return Json(new { migid = outid, status = outStatus }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                outStatus = "No files selected.";
                outid = "-1";
                return Json(new { migid = outid, status = outStatus }, JsonRequestBehavior.AllowGet);
            }
        }


        #region  Applied 
        [SessionCheckFilter]
        public ActionResult StudentSchoolMigrationApplied()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string Schl = loginSession.SCHL.ToString();
            MigrateSchoolModels MS = new MigrateSchoolModels();
            try
            {

                var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Migration Id"},new{ID="5",Name="By Name"},}, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();


                //GetAllowedGroupListBySchool
                List<SelectListItem> MyGroupList = new List<SelectListItem>();
                ViewBag.MyGroup = MyGroupList;

                string Search = "MigrationId is not null  and class  in (5,8)  ";
                MS.StoreAllData = SchoolDB.StudentSchoolMigrationsSearch(0, Search, Schl);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                }
                return View(MS);

            }
            catch (Exception ex)
            {
            }

            return View();
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult StudentSchoolMigrationApplied(FormCollection frm, string SelFilter, string SearchString)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];


            MigrateSchoolModels MS = new MigrateSchoolModels();

            string Schl = loginSession.SCHL.ToString();

            var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Migration Id"},new{ID="5",Name="By Name"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();

            //GetAllowedGroupListBySchool
            List<SelectListItem> MyGroupList = new List<SelectListItem>();
            ViewBag.MyGroup = MyGroupList;


            string Search = "MigrationId is not null  and class  in (5,8) ";
            if (string.IsNullOrEmpty(SelFilter) || string.IsNullOrEmpty(SearchString))
            {
                ViewBag.TotalCount = -1;
            }
            else
            {

                ViewBag.SelectedItem = SelFilter;
                int SelValueSch = Convert.ToInt32(SelFilter.ToString());
                if (SearchString != "")
                {
                    if (SelValueSch == 1)
                    { Search += " and StdId='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 2)
                    { Search += " and  Registration_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 3)
                    { Search += " and Aadhar_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 4)
                    { Search += " and MigrationId ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 5)
                    { Search += " and Name like '%" + frm["SearchString"].ToString() + "%'"; }
                }
            }

            MS.StoreAllData = SchoolDB.StudentSchoolMigrationsSearch(0, Search, Schl);
            if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
            }
            return View(MS);
        }


        #endregion

        #region Received


        [SessionCheckFilter]
        public ActionResult StudentSchoolMigrationReceived()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string Schl = loginSession.SCHL.ToString();
            MigrateSchoolModels MS = new MigrateSchoolModels();
            try
            {

                var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Migration Id"},new{ID="5",Name="By Name"},}, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();


                //GetAllowedGroupListBySchool
                List<SelectListItem> MyAcceptRejectList = DBClass.GetAcceptRejectDDL();
                ViewBag.MyGroup = MyAcceptRejectList;

                string Search = "MigrationId is not null  and class  in (5,8) ";
                MS.StoreAllData = SchoolDB.StudentSchoolMigrationsSearch(1, Search, Schl); // type=1 for receieved
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                }
                return View(MS);

            }
            catch (Exception ex)
            {
            }

            return View();
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult StudentSchoolMigrationReceived(FormCollection frm, string SelFilter, string SearchString)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            MigrateSchoolModels MS = new MigrateSchoolModels();

            string Schl = loginSession.SCHL.ToString();

            var itemFilter = new SelectList(new[]{new {ID="1",Name="Student Unique ID"},new {ID="2",Name="Reg. No"},new{ID="3",Name="Aadhar No"},
            new{ID="4",Name="Migration Id"},new{ID="5",Name="By Name"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();

            //GetAllowedGroupListBySchool
            List<SelectListItem> MyAcceptRejectList = DBClass.GetAcceptRejectDDL();
            ViewBag.MyGroup = MyAcceptRejectList;


            string Search = "MigrationId is not null  and class  in (5,8)  ";
            if (string.IsNullOrEmpty(SelFilter) || string.IsNullOrEmpty(SearchString))
            {
                ViewBag.TotalCount = -1;
            }
            else
            {

                ViewBag.SelectedItem = SelFilter;
                int SelValueSch = Convert.ToInt32(SelFilter.ToString());
                if (SearchString != "")
                {
                    if (SelValueSch == 1)
                    { Search += " and StdId='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 2)
                    { Search += " and  Registration_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 3)
                    { Search += " and Aadhar_num ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 4)
                    { Search += " and MigrationId ='" + frm["SearchString"].ToString() + "'"; }
                    else if (SelValueSch == 5)
                    { Search += " and Name like '%" + frm["SearchString"].ToString() + "%'"; }
                }
            }

            MS.StoreAllData = SchoolDB.StudentSchoolMigrationsSearch(1, Search, Schl); // type=1 for receieved
            if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
            }
            return View(MS);
        }

        #endregion



        #region Challan and Payment Details EAffiliation

        [SessionCheckFilter]
        public ActionResult StudentMigrationPayFee(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string Schl = loginSession.SCHL.ToString();
            StudentSchoolMigrationViewModel studentSchoolMigrationViewModel = new StudentSchoolMigrationViewModel();
            StudentSchoolMigrationFee _StudentSchoolMigrationFee = new StudentSchoolMigrationFee();

            try
            {
                string Search = "MigrationId =" + id;
                List<StudentSchoolMigrationViewModel> studentSchoolMigrationViewModelList = SchoolDB.StudentSchoolMigrationsSearchModel(2, Search, Schl); // type=1 for receieved
                if (studentSchoolMigrationViewModelList.Count() > 0)
                {
                    ViewBag.TotalCount = studentSchoolMigrationViewModelList.Count;
                    studentSchoolMigrationViewModel = studentSchoolMigrationViewModelList.Where(s => s.MigrationId == Convert.ToInt32(id)).FirstOrDefault();

                    //

                    DataSet ds = SchoolDB.GetStudentSchoolMigrationsPayment(studentSchoolMigrationViewModel.MigrationId, studentSchoolMigrationViewModel.StdId);
                    _StudentSchoolMigrationFee.PaymentFormData = ds;
                    if (_StudentSchoolMigrationFee.PaymentFormData == null || _StudentSchoolMigrationFee.PaymentFormData.Tables[0].Rows.Count == 0)
                    { ViewBag.TotalCount = 0; Session["StudentSchoolMigrationFee"] = null; }
                    else
                    {
                        Session["StudentSchoolMigrationFee"] = ds;
                        ViewBag.TotalFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("fee")));
                        ViewBag.TotalLateFee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("latefee")));
                        ViewBag.TotalTotfee = Convert.ToInt32(ds.Tables[0].AsEnumerable().Sum(x => x.Field<int>("Totfee")));

                        ViewBag.Total = ViewBag.TotalFee + ViewBag.TotalLateFee;
                        ViewData["result"] = 10;
                        ViewData["FeeStatus"] = "1";
                        ViewBag.TotalCount = 1;
                        return View(_StudentSchoolMigrationFee);
                    }
                }
                else
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }

            }
            catch (Exception ex)
            {
            }


            return View(_StudentSchoolMigrationFee);
        }



        [HttpPost]
        [SessionCheckFilter]
        public ActionResult StudentMigrationPayFee(string id, FormCollection frm, string PayModValue, string AllowBanks)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            string Schl = loginSession.SCHL.ToString();
            try
            {

                StudentSchoolMigrationFee pfvm = new StudentSchoolMigrationFee();
                ChallanMasterModel CM = new ChallanMasterModel();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("ApplyStudentSchoolMigration", "School");
                }
                if (Session["StudentSchoolMigrationFee"] == null)
                {
                    return RedirectToAction("ApplyStudentSchoolMigration", "School");
                }
                string appno = id;
                DataSet ds = (DataSet)Session["StudentSchoolMigrationFee"];
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

                string stdid = ds.Tables[0].Rows[0]["StdId"].ToString();

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
                    CM.SchoolCode = ds.Tables[0].Rows[0]["StdId"].ToString();
                    CM.DIST = "";
                    CM.DISTNM = "";
                    CM.LOT = 1;
                    CM.SCHLREGID = Schl;
                    CM.FeeStudentList = ds.Tables[0].Rows[0]["MigrationId"].ToString();
                    CM.APPNO = ds.Tables[0].Rows[0]["MigrationId"].ToString();

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
                    string result = _challanRepository.InsertPaymentForm(CM, out SchoolMobile);
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
                        else if (result.Length > 5)
                        {

                            //string Sms = "Challan no. " + result + " of Ref no  " + CM.APPNO + " successfully generated and valid till Dt " + bnkLastDate + ". Regards PSEB";
                            //try
                            //{
                            //    string getSms = objCommon.gosms(SchoolMobile, Sms);
                            //}
                            //catch (Exception) { }
                            return RedirectToAction("GenerateChallaan", "Home", new { ChallanId = result });

                        }
                    }
                }
                return View(pfvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ApplyStudentSchoolMigration", "School");
            }
        }

        #endregion Challan and Payment Details




        public ActionResult StudentMigrationView(string id)
        {

            // string Schl = Session["SCHL"].ToString();          
            StudentSchoolMigrationViewModel studentSchoolMigrationViewModel = new StudentSchoolMigrationViewModel();
            try
            {
                string Search = "MigrationId =" + id;
                List<StudentSchoolMigrationViewModel> studentSchoolMigrationViewModelList = SchoolDB.StudentSchoolMigrationsSearchModel(2, Search, ""); // type=2 for search any
                if (studentSchoolMigrationViewModelList.Count() > 0)
                {
                    ViewBag.TotalCount = studentSchoolMigrationViewModelList.Count;
                    studentSchoolMigrationViewModel = studentSchoolMigrationViewModelList.Where(s => s.MigrationId == Convert.ToInt32(id)).FirstOrDefault();
                }
                else
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }

            }
            catch (Exception ex)
            {
            }

            return View(studentSchoolMigrationViewModel);
        }



        public ActionResult StudentMigrationPrintCertificate(string id)
        {

            // string Schl = Session["SCHL"].ToString();
            StudentSchoolMigrationViewModel studentSchoolMigrationViewModel = new StudentSchoolMigrationViewModel();
            try
            {
                string Search = "MigrationId =" + id;
                List<StudentSchoolMigrationViewModel> studentSchoolMigrationViewModelList = SchoolDB.StudentSchoolMigrationsSearchModel(2, Search, ""); // type=2 for any
                if (studentSchoolMigrationViewModelList.Count() > 0)
                {
                    ViewBag.TotalCount = studentSchoolMigrationViewModelList.Count;
                    studentSchoolMigrationViewModel = studentSchoolMigrationViewModelList.Where(s => s.MigrationId == Convert.ToInt32(id)).FirstOrDefault();
                }
                else
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }

            }
            catch (Exception ex)
            {
            }

            return View(studentSchoolMigrationViewModel);
        }


        #endregion



        #region Meritorious Exam Centre
        [SessionCheckFilter]
        public ActionResult MeritoriousExamCentre(int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.IsMeritoriousSchool == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            Printlist obj = new Printlist();
            #region Circular

            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            string Search = string.Empty;
            Search = "Id like '%' and CircularTypes like '%10%' and Convert(Datetime,Convert(date,ExpiryDateDD))>=Convert(Datetime,Convert(date,getdate()))";
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


            obj.StoreAllData = SchoolDB.MeritoriousCentreMaster(loginSession.SCHL);
            if (obj.StoreAllData == null || obj.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "0";
                ViewBag.TotalCount = 0;
            }
            else
            {
                ViewBag.Message = "1";
                ViewBag.TotalCount = 1;
            }
            return View(obj);
        }


        #region  Signature Chart and Confidential Meritorious 
        [SessionCheckFilter]
        public ActionResult ConfidentialListMeritorious(SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.IsMeritoriousSchool == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            List<SelectListItem> schllist = new List<SelectListItem>();
            ViewBag.MySchCode = schllist;
            try
            {

                DataSet Dresult = SchoolDB.GetMeritoriousCentCodeBySchl(loginSession.SCHL);
                foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                {
                    schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                }
                ViewBag.MySchCode = schllist;
            }
            catch (Exception ex)
            {
            }
            return View(sm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ConfidentialListMeritorious(SchoolModels sm, FormCollection frc, string ExamCent)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.IsMeritoriousSchool == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            List<SelectListItem> schllist = new List<SelectListItem>();
            ViewBag.MySchCode = schllist;
            try
            {

                DataSet Dresult = SchoolDB.GetMeritoriousCentCodeBySchl(loginSession.SCHL);
                foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                {
                    schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                }
                ViewBag.MySchCode = schllist;

                if (!string.IsNullOrEmpty(ExamCent))
                {
                    sm.ExamCent = ExamCent;
                    sm.CLASS = "";
                }
                sm.StoreAllData = SchoolDB.GetMeritoriousConfidentialList(sm);
                //sm.ExamCent = Session["cent"].ToString();
                ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
            }
            catch (Exception ex)
            {
            }
            return View(sm);

        }


        [SessionCheckFilter]
        public ActionResult SignatureChartMeritorious(SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.IsMeritoriousSchool == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            List<SelectListItem> schllist = new List<SelectListItem>();
            ViewBag.MySchCode = schllist;
            try
            {

                DataSet Dresult = SchoolDB.GetMeritoriousCentCodeBySchl(loginSession.SCHL);
                foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                {
                    schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                }
                ViewBag.MySchCode = schllist;

            }
            catch (Exception ex)
            {
            }
            return View(sm);

        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SignatureChartMeritorious(SchoolModels sm, FormCollection frc, string ExamCent, string ExamRoll)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.IsMeritoriousSchool == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            List<SelectListItem> schllist = new List<SelectListItem>();
            ViewBag.MySchCode = schllist;
            try
            {

                DataSet Dresult = SchoolDB.GetMeritoriousCentCodeBySchl(loginSession.SCHL);
                foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                {
                    schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                }
                ViewBag.MySchCode = schllist;

                if (!string.IsNullOrEmpty(ExamCent))
                {
                    sm.ExamCent = ExamCent;
                }
                if (!string.IsNullOrEmpty(ExamRoll))
                {
                    sm.ExamRoll = ExamRoll;
                }
                sm.StoreAllData = SchoolDB.GetMeritoriousConfidentialList(sm);
                //sm.ExamCent = Session["cent"].ToString();
                ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
            }

            catch (Exception ex)
            {
            }
            return View(sm);

        }

        #endregion  Signature Chart and Confidential Meritorious 





        #endregion Meritorious Exam Centre

        #region  Online Centre Creation

        //[SessionCheckFilter]
        //public ActionResult OnlineCentreCreation(CenterMasterForOnlineCentreCreationViews centerMasterForOnlineCentreCreationViews)
        //{
        //    LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //    //if (loginSession.IsMeritoriousSchool == 0)
        //    //{
        //    //    return RedirectToAction("Index", "Home");
        //    //}

        //    centerMasterForOnlineCentreCreationViews = _context.CenterMasterForOnlineCentreCreationViews.Where(s => s.CSCHL == loginSession.SCHL).SingleOrDefault();

        //    return View(centerMasterForOnlineCentreCreationViews);

        //}


        //[SessionCheckFilter]
        //[HttpPost]
        //public ActionResult OnlineCentreCreation(CenterMasterForOnlineCentreCreationViews centerMasterForOnlineCentreCreationViews, FormCollection frc, string submit)
        //{
        //    LoginSession loginSession = (LoginSession)Session["LoginSession"];
        //    //if (loginSession.IsMeritoriousSchool == 0)
        //    //{
        //    //    return RedirectToAction("Index", "Home");
        //    //}
        //    CenterMasterForOnlineCentreCreationViews centerMasterForOnlineCentreCreationViews1 = _context.CenterMasterForOnlineCentreCreationViews.Where(s => s.CSCHL == loginSession.SCHL).SingleOrDefault();


        //    if (string.IsNullOrEmpty(submit))
        //    { }
        //    else
        //    {
        //        if (submit.ToLower().Contains("new".ToLower()))
        //        {
        //            //  create new centre
        //        }
        //        else
        //        {

        //        }

        //    }



        //    return View(centerMasterForOnlineCentreCreationViews);

        //}

        #endregion  Online Centre Creation

        public ActionResult ExportDataFromDataTable(DataTable dt, string filename)
        {
            try
            {
                if (Session["SchoolResultDeclare"] != null)
                {
                    dt = (DataTable)Session["SchoolResultDeclare"];

                    if (dt.Columns.Contains("ResultShowHide"))
                    { dt.Columns.Remove("ResultShowHide"); }
                    if (dt.Columns.Contains("FULLNM"))
                    { dt.Columns.Remove("FULLNM"); }
                    if (dt.Columns.Contains("class"))
                    { dt.Columns.Remove("class"); }

                }

                if (dt.Rows.Count == 0)
                {
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        string fileName1 = filename + "_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";  //103_230820162209_347
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

                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Login");
            }

        }



        #region Re-Exam For Absent Student in Term-1

        [SessionCheckFilter]
        public ActionResult ViewReExamTermStudentList(string id, ReExamTermStudentsModelList onDemandCertificateSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Index", "Home");
                }
                string Search = "", RP = "", cls = "";
                ViewBag.id = id;

                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();


                switch (id)
                {
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
                onDemandCertificateSearchModel.ReExamTermStudentsSearchModel = SchoolDB.GetReExamTermStudentList("GET", RP, cls, SCHL, Search, out dsOut);
                onDemandCertificateSearchModel.StoreAllData = dsOut;
                return View(onDemandCertificateSearchModel);
            }
            catch (Exception ex)
            {
                return View(id);
            }
        }


        [SessionCheckFilter]
        public JsonResult JqReExamTermApplyStudents(string studentlist, string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            int result = 0;
            if (!string.IsNullOrEmpty(studentlist))
            {
                studentlist = studentlist.Remove(studentlist.Length - 1);


                List<int> listComma = studentlist.Split(',').Select(int.Parse).ToList();
                List<ReExamTermStudents> list = new List<ReExamTermStudents>();

                foreach (var stdid in listComma)
                {
                    list.Add(new ReExamTermStudents { ReExamId = 0, Std_id = stdid, Schl = loginSession.SCHL, Cls = Convert.ToInt32(cls), IsActive = 1, IsPrinted = 0, SubmitOn = DateTime.Now, SubmitBy = "SCHL" });
                }

                if (list.Count() > 0)
                {
                    result = new SchoolDB().InsertReExamTermStudentList(list);
                }
                else
                {
                    result = -1;
                }
            }
            return Json(new { Result = result }, JsonRequestBehavior.AllowGet);
        }


        [SessionCheckFilter]
        public ActionResult ReExamTermAppliedStudentList(string id, ReExamTermStudentsModelList onDemandCertificateSearchModel)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("Index", "Home");
                }
                string Search = "", RP = "", cls = "";
                ViewBag.id = id;

                string SCHL = loginSession.SCHL;
                ViewBag.Primary = loginSession.fifth.ToString();
                ViewBag.Middle = loginSession.middle.ToString();

                switch (id)
                {
                    case "Primary":
                        RP = "R";
                        cls = "5";
                        break;
                    case "Middle":
                        RP = "R";
                        cls = "8";
                        break;
                    //case "S":
                    //    RP = "R";
                    //    cls = "4";
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

                    default:
                        RP = "";
                        cls = "";
                        break;
                }
                ViewBag.RP = RP;
                ViewBag.cls = cls;

                //Search,
                DataSet dsOut = new DataSet();
                onDemandCertificateSearchModel.ReExamTermStudentsSearchModel = SchoolDB.GetReExamTermStudentList("ADDED", RP, cls, SCHL, Search, out dsOut);
                onDemandCertificateSearchModel.StoreAllData = dsOut;
                return View(onDemandCertificateSearchModel);
            }
            catch (Exception ex)
            {
                return View(id);
            }
        }


        [SessionCheckFilter]
        public JsonResult JqRemoveReExamTermApplyStudents(string demandIdList, string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            int result = 0;
            if (!string.IsNullOrEmpty(demandIdList))
            {
                demandIdList = demandIdList.Remove(demandIdList.Length - 1);


                List<int> listComma = demandIdList.Split(',').Select(int.Parse).ToList();
                List<ReExamTermStudents> list = new List<ReExamTermStudents>();

                foreach (var did in listComma)
                {
                    list.Add(new ReExamTermStudents { ReExamId = did });
                }

                if (list.Count() > 0)
                {
                    result = new SchoolDB().RemoveRangeOnDemandCertificateStudentList(list);
                }
                else
                {
                    result = -1;
                }
            }
            return Json(new { Result = result }, JsonRequestBehavior.AllowGet);
        }


        [SessionCheckFilter]
        public ActionResult ReExamTermStudents_ChallanList(ReExamTermStudents_ChallanDetailsViewsModelList obj)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                DataSet dsOut = new DataSet();
                obj.ReExamTermStudents_ChallanDetailsViews = SchoolDB.ReExamTermStudents_ChallanList(loginSession.SCHL, out dsOut);
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
        public ActionResult ReExamTermStudentCalculateFee(string id, string D)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {
                Session["ReExamTermStudentCalculateFee"] = null;
                FeeHomeViewModel fhvm = new FeeHomeViewModel();

                string Search = string.Empty;
                Search = "SCHL=" + loginSession.SCHL.ToString();
                DataSet ds = new DataSet();
                ds = SchoolDB.ReExamTermStudentsCountRecordsClassWise(Search, loginSession.SCHL.ToString());
                if ((ds == null) || (ds.Tables[0].Rows.Count == 0 && ds.Tables[1].Rows.Count == 0))
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.SR = ViewBag.MR = ViewBag.TotalCount = 0;
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
                    fhvm.StoreAllData = SchoolDB.ReExamTermStudentCalculateFee(id, date, Search, loginSession.SCHL.ToString());

                    if (fhvm.StoreAllData == null || fhvm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        ViewData["FeeStatus"] = "3";
                    }
                    else
                    {
                        ViewData["FeeStatus"] = "1";
                        if (Session["ReExamTermStudentCalculateFee"] != null)
                        {
                            Session["ReExamTermStudentCalculateFee"] = null;
                        }

                        Session["ReExamTermStudentCalculateFee"] = fhvm.StoreAllData;
                        ViewBag.TotalCount = fhvm.StoreAllData.Tables[0].Rows.Count;
                        fhvm.TotalFeesInWords = fhvm.StoreAllData.Tables[1].Rows[0]["TotalFeesInWords"].ToString();
                        fhvm.EndDate = fhvm.StoreAllData.Tables[0].Rows[0]["EndDateDay"].ToString() + " " + fhvm.StoreAllData.Tables[0].Rows[0]["FeeValidDate"].ToString();
                    }
                }
                else
                {
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
        public ActionResult ReExamTermStudentPaymentForm()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            try
            {

                PaymentformViewModel pfvm = new PaymentformViewModel();
                if (Session["ReExamTermStudentCalculateFee"] == null || Session["ReExamTermStudentCalculateFee"].ToString() == "")
                {
                    return RedirectToAction("ReExamTermStudentCalculateFee", " School");
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

                DataSet dscalFee = (DataSet)Session["ReExamTermStudentCalculateFee"];
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
                Session["ReExamTermStudentPaymentForm"] = pfvm;

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
                    DataSet ds1 = new BankDB().GetBankDataByBCODE(BM, out OutStatus);
                    BM.BANKNAME = ds1.Tables[0].Rows[0]["BANKNAME"].ToString();
                    pfvm.bankList.Add(new BankListModel { BCode = BM.BCODE, BankName = BM.BANKNAME, Img = "" });
                }
                ///////////////



                if (pfvm.TotalFinalFees == 0 && pfvm.TotalFees == 0)
                {
                    ViewBag.CheckForm = 1; // only verify for M1 and T1 
                    TempData["ReExamTermStudentCheckFormFee"] = 0;
                }
                else
                {
                    ViewBag.CheckForm = 0; // only verify for M1 and T1 
                    TempData["ReExamTermStudentCheckFormFee"] = 1;
                }
                return View(pfvm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("ReExamTermStudentCalculateFee", "School");
            }
        }


        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ReExamTermStudentPaymentForm(PaymentformViewModel pfvm, FormCollection frm, string PayModValue, string AllowBanks, string IsOnline)
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
                //if (Session["ReExamTermStudentCheckFormFee"].ToString() == "0")
                //{ pfvm.BankCode = "203"; }


                if (Session["ReExamTermStudentPaymentForm"] == null || Session["ReExamTermStudentPaymentForm"].ToString() == "")
                {
                    return RedirectToAction("ReExamTermStudentPaymentForm", "School");
                }
                if (Session["ReExamTermStudent_FeeStudentList"] == null || Session["ReExamTermStudent_FeeStudentList"].ToString() == "")
                {
                    return RedirectToAction("ReExamTermStudentPaymentForm", "School");
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
                    string FeeStudentList = Session["ReExamTermStudent_FeeStudentList"].ToString();
                    CM.FeeStudentList = FeeStudentList.Remove(FeeStudentList.LastIndexOf(","), 1);
                    PaymentformViewModel PFVMSession = (PaymentformViewModel)Session["ReExamTermStudentPaymentForm"];
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
                        DataSet ds1 = new BankDB().GetBankDataByBCODE(BM, out OutStatus);
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
                            return RedirectToAction("ReExamTermStudentPaymentForm", "School");
                        }
                    }

                    string SchoolMobile = "";
                    // string result = "0";

                    string result = _challanRepository.InsertPaymentForm(CM, out SchoolMobile);
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
            return RedirectToAction("ReExamTermStudentPaymentForm", "School");
        }

        #endregion


        #endregion


        #region Begin SchoolBasedExams Portal
        [SessionCheckFilter]
        public ActionResult SchoolBasedExamsPortal()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["ReAppearCCEMarksSearch"] = null;
            return View();
        }
        [SessionCheckFilter]
        public ActionResult SchoolBasedExamsAgree(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["SchoolBasedExamsClass"] = id;
            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SchoolBasedExamsAgree(string id, FormCollection frm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            try
            {
                string cc = id;
                string s = frm["Agree"].ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolBasedExamsPortal", "School");
                }
                else
                {
                    if (s == "Agree")
                    {
                        return RedirectToAction("SchoolBasedExamsMarks", "School", new { id = cc });
                    }
                }
                return RedirectToAction("SchoolBasedExamsPortal", "School");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }
        }


        [SessionCheckFilter]
        public ActionResult SchoolBasedExamsMarks(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolBasedExamsPortal", "School");
                }

                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                string SelectedAction = "0";


                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    if (TempData["SchoolBasedExamsMarksSearch"] != null)
                    {
                        Search += TempData["SchoolBasedExamsMarksSearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelFilter"] = ViewBag.SelectedFilter;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["SchoolBasedExamsMarksSearch"] = Search;
                    }
                    else
                    {
                        Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                    }



                    MS.StoreAllData = _schoolRepository.GetSchoolBasedExamsMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, Convert.ToInt32(SelectedAction));
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsSchoolBasedExamsFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {

                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);

                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsSchoolBasedExamsFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }

                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SchoolBasedExamsMarks(string id, FormCollection frm, int? page)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolBasedExamsPortal", "School");
                }
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "";
                    Search = "  a.SCHL = '" + loginSession.SCHL + "' and  a.class='" + CLASS + "' ";

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "")
                        {
                            if (SelValueSch == 3)
                            {
                                SelAction = 2;
                            }
                            //  { Search += " and  IsSchoolBasedExamsFilled=1 "; } // Filled
                            else if (SelValueSch == 2)
                            { SelAction = 1; }
                            //{ Search += " and (IsSchoolBasedExamsFilled is null or IsSchoolBasedExamsFilled=0) "; } // pending
                        }
                        ViewBag.SelectedAction = frm["SelAction"];
                    }

                    if (frm["SelFilter"] != "")
                    {

                        ViewBag.SelectedFilter = frm["SelFilter"];
                        int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                        if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and b.Roll='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelFilter"] = frm["SelFilter"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["SchoolBasedExamsMarksSearch"] = Search;
                    // string class1 = "4";
                    MS.StoreAllData = _schoolRepository.GetSchoolBasedExamsMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, SelAction);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsSchoolBasedExamsFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsSchoolBasedExamsFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqSchoolBasedExamsMarks(string stdid, string CandSubject, string cls)
        {

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;

            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubjectReExamTermStudents>>(CandSubject);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTTEST1");
            dtSub.Columns.Add("MAXTEST1");
            dtSub.Columns.Add("OBTTEST2");
            dtSub.Columns.Add("MAXTEST2");
            dtSub.Columns.Add("OBTTEST3");
            dtSub.Columns.Add("MAXTEST3");
            dtSub.Columns.Add("OBTTEST4");
            dtSub.Columns.Add("MAXTEST4");
            dtSub.Columns.Add("OBTTEST5");
            dtSub.Columns.Add("MAXTEST5");
            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                row = dtSub.NewRow();
                if (rowObj.OBTTEST1 == "A" || rowObj.OBTTEST1 == "ABS")
                {
                    rowObj.OBTTEST1 = "ABS";
                }
                else if (rowObj.OBTTEST1 == "C" || rowObj.OBTTEST1 == "CAN")
                {
                    rowObj.OBTTEST1 = "CAN";
                }
                else if (!string.IsNullOrEmpty(rowObj.OBTTEST1))
                {
                    rowObj.OBTTEST1 = rowObj.OBTTEST1.PadLeft(3, '0');
                }

                if (rowObj.OBTTEST2 == "A" || rowObj.OBTTEST2 == "ABS")
                {
                    rowObj.OBTTEST2 = "ABS";
                }
                else if (rowObj.OBTTEST2 == "C" || rowObj.OBTTEST2 == "CAN")
                {
                    rowObj.OBTTEST2 = "CAN";
                }
                else if (!string.IsNullOrEmpty(rowObj.OBTTEST2))
                {
                    rowObj.OBTTEST2 = rowObj.OBTTEST2.PadLeft(3, '0');
                }
                //
                if (rowObj.OBTTEST3 == "A" || rowObj.OBTTEST3 == "ABS")
                {
                    rowObj.OBTTEST3 = "ABS";
                }
                else if (rowObj.OBTTEST3 == "C" || rowObj.OBTTEST3 == "CAN")
                {
                    rowObj.OBTTEST3 = "CAN";
                }
                else if (!string.IsNullOrEmpty(rowObj.OBTTEST3))
                {
                    rowObj.OBTTEST3 = rowObj.OBTTEST3.PadLeft(3, '0');
                }
                //
                if (rowObj.OBTTEST4 == "A" || rowObj.OBTTEST4 == "ABS")
                {
                    rowObj.OBTTEST4 = "ABS";
                }
                else if (rowObj.OBTTEST4 == "C" || rowObj.OBTTEST4 == "CAN")
                {
                    rowObj.OBTTEST4 = "CAN";
                }
                else if (!string.IsNullOrEmpty(rowObj.OBTTEST4))
                {
                    rowObj.OBTTEST4 = rowObj.OBTTEST4.PadLeft(3, '0');
                }
                //
                if (rowObj.OBTTEST5 == "A" || rowObj.OBTTEST5 == "ABS")
                {
                    rowObj.OBTTEST5 = "ABS";
                }
                else if (rowObj.OBTTEST5 == "C" || rowObj.OBTTEST5 == "CAN")
                {
                    rowObj.OBTTEST5 = "CAN";
                }
                else if (!string.IsNullOrEmpty(rowObj.OBTTEST5))
                {
                    rowObj.OBTTEST5 = rowObj.OBTTEST5.PadLeft(3, '0');
                }
                //if (rowObj.MINMARKS == "--" || rowObj.MINMARKS == "")
                //{
                //    rowObj.MINMARKS = "000";
                //}
                dtSub.Rows.Add(stdid, rowObj.SUB, rowObj.OBTTEST1, rowObj.MAXTEST1, rowObj.OBTTEST2, rowObj.MAXTEST2, rowObj.OBTTEST3, rowObj.MAXTEST3, rowObj.OBTTEST4, rowObj.MAXTEST4, rowObj.OBTTEST5, rowObj.MAXTEST5);
            }
            dtSub.AcceptChanges();


            foreach (DataRow dr1 in dtSub.Rows)
            {
                if (dr1["OBTTEST1"].ToString() == "" || dr1["OBTTEST1"].ToString() == "ABS" || dr1["OBTTEST1"].ToString() == "CAN")
                { }
                else if (dr1["OBTTEST1"].ToString() == "0" || dr1["OBTTEST1"].ToString().Contains("A") || dr1["OBTTEST1"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTTEST1"].ToString());
                    int max = Convert.ToInt32(dr1["MAXTEST1"].ToString());
                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
                //
                if (dr1["OBTTEST2"].ToString() == "" || dr1["OBTTEST2"].ToString() == "ABS" || dr1["OBTTEST2"].ToString() == "CAN")
                { }
                else if (dr1["OBTTEST2"].ToString() == "0" || dr1["OBTTEST2"].ToString().Contains("A") || dr1["OBTTEST2"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTTEST2"].ToString());
                    int max = Convert.ToInt32(dr1["MAXTEST2"].ToString());
                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
                //
                if (dr1["OBTTEST3"].ToString() == "" || dr1["OBTTEST3"].ToString() == "ABS" || dr1["OBTTEST3"].ToString() == "CAN")
                { }
                else if (dr1["OBTTEST3"].ToString() == "0" || dr1["OBTTEST3"].ToString().Contains("A") || dr1["OBTTEST3"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTTEST3"].ToString());
                    int max = Convert.ToInt32(dr1["MAXTEST3"].ToString());
                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
                //
                if (dr1["OBTTEST4"].ToString() == "" || dr1["OBTTEST4"].ToString() == "ABS" || dr1["OBTTEST4"].ToString() == "CAN")
                { }
                else if (dr1["OBTTEST4"].ToString() == "0" || dr1["OBTTEST4"].ToString().Contains("A") || dr1["OBTTEST4"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTTEST4"].ToString());
                    int max = Convert.ToInt32(dr1["MAXTEST4"].ToString());
                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
                //
                if (dr1["OBTTEST5"].ToString() == "" || dr1["OBTTEST5"].ToString() == "ABS" || dr1["OBTTEST5"].ToString() == "CAN")
                { }
                else if (dr1["OBTTEST5"].ToString() == "0" || dr1["OBTTEST5"].ToString().Contains("A") || dr1["OBTTEST5"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTTEST5"].ToString());
                    int max = Convert.ToInt32(dr1["MAXTEST5"].ToString());
                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                // string class1 = "5";
                string class1 = cls == "Senior" ? "4" : cls == "Matric" ? "2" : "5";
                int OutStatus = 0;
                dee = _schoolRepository.AllotSchoolBasedExamsMarks(loginSession.SCHL, stdid, dtSub, class1, out OutStatus);
                var results = new
                {
                    status = OutStatus
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        [SessionCheckFilter]
        public ActionResult SchoolBasedExamsReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["SchoolBasedExamsMarksSearch"] = null;
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                MS.StoreAllData = _schoolRepository.SchoolBasedExamsMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        public ActionResult SchoolBasedExamsFinalReport(string id, FormCollection frm)
        {
            TempData["SchoolBasedExamsMarksSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolBasedExamsPortal", "School");
                }
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                DataSet dsFinal = new DataSet();
                MS.StoreAllData = dsFinal = _schoolRepository.SchoolBasedExamsMarksEntryReport(loginSession.SCHL, 1, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowCCE = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowCCE = 1;
                    MS.StoreAllData = _schoolRepository.SchoolBasedExamsMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolBasedExamsDate = MS.StoreAllData.Tables[0].Rows[0]["SchoolBasedExamsDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }
                    if (dsFinal.Tables[3].Rows.Count > 0)
                    {
                        MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                        {
                            Cls = dsFinal.Tables[3].Rows[0]["Cls"].ToString(),
                            IsActive = Convert.ToInt32(dsFinal.Tables[3].Rows[0]["IsActive"].ToString()),
                            IsAllow = dsFinal.Tables[3].Rows[0]["IsAllow"].ToString(),
                            LastDate = Convert.ToString(dsFinal.Tables[3].Rows[0]["LastdateDT"].ToString()),
                            Panel = Convert.ToString(dsFinal.Tables[3].Rows[0]["Panel"].ToString())
                        };
                    }
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowCCE = 2;
                    ViewBag.IsFinal = 0;
                    ViewBag.TotalCount1 = 0;
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SchoolBasedExamsFinalReport(string id)
        {
            TempData["SchoolBasedExamsMarksSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("SchoolBasedExamsPortal", "School");
                }
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;
                string Search = string.Empty;
                string OutError = "";
                MS.StoreAllData = _schoolRepository.SchoolBasedExamsMarksEntryReport(loginSession.SCHL, 2, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolBasedExamsDate = MS.StoreAllData.Tables[0].Rows[0]["SchoolBasedExamsDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }


        #endregion  End SchoolBasedExams Portal




        #region Elective Theory Panel For Middle

        [SessionCheckFilter]
        public ActionResult ElectiveTheory_Portal()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["ElectiveTheory_MarksSearch"] = null;
            return View();
        }


        [SessionCheckFilter]
        public ActionResult ElectiveTheoryMarksPanel(FormCollection frm, string id, int? page)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                string SelectedAction = "0";


                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Filled" }, new { ID = "3", Name = "Final Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {

                    if (TempData["ElectiveTheoryMarksPanelSearch"] != null)
                    {
                        Search += TempData["ElectiveTheoryMarksPanelSearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        SelectedAction = TempData["SelAction"].ToString();
                        ViewBag.SelectedAction = TempData["SelActionValue"];

                        TempData["SelFilter"] = ViewBag.SelectedFilter;
                        TempData["SelAction"] = SelectedAction;
                        TempData["SelActionValue"] = ViewBag.SelectedAction;
                        TempData["ElectiveTheoryMarksPanelSearch"] = Search;
                    }
                    else
                    {
                        Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                    }



                    MS.StoreAllData = _schoolRepository.GetElectiveTheoryMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, Convert.ToInt32(SelectedAction));
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsElectiveMarksFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);

                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsElectiveMarksFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }

                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }


                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ElectiveTheoryMarksPanel(string id, FormCollection frm, int? page)
        {


            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();
                ViewBag.SelectedFilter = "0";


                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Pending" }, new { ID = "2", Name = "Filled" }, new { ID = "3", Name = "Final Submitted" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();
                ViewBag.SelectedAction = "0";
                //------------------------

                if (loginSession.SCHL != null)
                {
                    string Search = "";
                    Search = "  a.SCHL = '" + loginSession.SCHL + "' and  a.class='" + CLASS + "' ";

                    int SelAction = 0;
                    if (frm["SelAction"] != "")
                    {
                        int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                        if (frm["SelAction"] != "")
                        {

                            SelAction = SelValueSch;
                            //if (SelValueSch == 3)
                            //{
                            //    SelAction = 2;
                            //}
                            //else if (SelValueSch == 2)
                            //{ SelAction = 1; }
                        }
                        ViewBag.SelectedAction = frm["SelAction"];
                    }

                    if (frm["SelFilter"] != "")
                    {

                        ViewBag.SelectedFilter = frm["SelFilter"];
                        int SelValueSch = Convert.ToInt32(frm["SelFilter"].ToString());
                        if (frm["SelFilter"] != "" && frm["SearchString"].ToString() != "")
                        {
                            if (SelValueSch == 1)
                            { Search += " and b.Roll='" + frm["SearchString"].ToString() + "'"; }
                            if (SelValueSch == 2)
                            { Search += " and Std_id='" + frm["SearchString"].ToString() + "'"; }
                            else if (SelValueSch == 3)
                            { Search += " and  Registration_num like '%" + frm["SearchString"].ToString() + "%'"; }
                            else if (SelValueSch == 4)
                            { Search += " and  Candi_Name like '%" + frm["SearchString"].ToString() + "%'"; }
                        }
                    }

                    TempData["SelFilter"] = frm["SelFilter"];
                    TempData["SelAction"] = SelAction;
                    TempData["SelActionValue"] = frm["SelAction"];
                    TempData["ElectiveTheoryMarksPanelSearch"] = Search;
                    // string class1 = "4";
                    MS.StoreAllData = _schoolRepository.GetElectiveTheoryMarksDataBySCHL(Search, loginSession.SCHL, pageIndex, CLASS, SelAction);
                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsElectiveMarksFilled"]);
                        }
                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        int count = Convert.ToInt32(MS.StoreAllData.Tables[1].Rows[0]["TotalCnt"]);
                        if (MS.StoreAllData.Tables[3].Rows.Count > 0)
                        {
                            ViewBag.IsFinal = Convert.ToInt32(MS.StoreAllData.Tables[3].Rows[0]["IsElectiveMarksFilled"]);
                        }

                        if (MS.StoreAllData.Tables[4].Rows.Count > 0)
                        {
                            ViewBag.TotalPen = Convert.ToInt32(MS.StoreAllData.Tables[4].Rows[0]["TotalPen"]);
                            ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[4].Rows[0]["FinalSubmitLastDate"];
                        }
                        if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                        {
                            MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                            {
                                Cls = MS.StoreAllData.Tables[5].Rows[0]["Cls"].ToString(),
                                IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                                IsAllow = MS.StoreAllData.Tables[5].Rows[0]["IsAllow"].ToString(),
                                LastDate = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["LastdateDT"].ToString()),
                                Panel = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["Panel"].ToString())
                            };
                        }
                        ViewBag.TotalCount1 = count;
                        int tp = Convert.ToInt32(count);
                        int pn = tp / 20;
                        int cal = 20 * pn;
                        int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                        if (res >= 1)
                            ViewBag.pn = pn + 1;
                        else
                            ViewBag.pn = pn;

                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                //return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [SessionCheckFilter]
        [HttpPost]
        public JsonResult JqElectiveTheoryMarks(string stdid, string CandSubject, string cls)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            var flag = 1;

            var objResponse1 = JsonConvert.DeserializeObject<List<CandSubject>>(CandSubject);
            DataTable dtSub = new DataTable();
            dtSub.Columns.Add("CANDID");
            dtSub.Columns.Add("SUB");
            dtSub.Columns.Add("OBTMARKS");
            dtSub.Columns.Add("MINMARKS");
            dtSub.Columns.Add("MAXMARKS");
            DataRow row = null;
            foreach (var rowObj in objResponse1)
            {
                row = dtSub.NewRow();
                if (rowObj.OBTMARKS == "A" || rowObj.OBTMARKS == "ABS")
                {
                    rowObj.OBTMARKS = "ABS";
                }
                else if (rowObj.OBTMARKS == "C" || rowObj.OBTMARKS == "CAN")
                {
                    rowObj.OBTMARKS = "CAN";
                }
                else if (rowObj.OBTMARKS != "")
                {
                    rowObj.OBTMARKS = rowObj.OBTMARKS.PadLeft(3, '0');
                }
                else if (string.IsNullOrEmpty(rowObj.OBTMARKS))
                {
                    rowObj.OBTMARKS = "";
                }


                if (rowObj.MINMARKS == "--" || rowObj.MINMARKS == "")
                {
                    rowObj.MINMARKS = "000";
                }

                dtSub.Rows.Add(stdid, rowObj.SUB, rowObj.OBTMARKS, rowObj.MINMARKS, rowObj.MAXMARKS);
            }
            dtSub.AcceptChanges();


            foreach (DataRow dr1 in dtSub.Rows)
            {
                if (dr1["OBTMARKS"].ToString() == "" || dr1["OBTMARKS"].ToString() == "ABS" || dr1["OBTMARKS"].ToString() == "CAN")
                { }
                else if (dr1["OBTMARKS"].ToString() == "0" || dr1["OBTMARKS"].ToString().Contains("A") || dr1["OBTMARKS"].ToString().Contains("C"))
                {
                    flag = -2;
                    var results = new
                    {
                        status = flag
                    };
                    return Json(results);
                }
                else
                {
                    int obt = Convert.ToInt32(dr1["OBTMARKS"].ToString());
                    int min = Convert.ToInt32(dr1["MINMARKS"].ToString());
                    int max = Convert.ToInt32(dr1["MAXMARKS"].ToString());

                    if ((obt < 0) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                // string class1 = "5";
                string class1 = cls == "Primary" ? "5" : cls == "Middle" ? "8" : "5";
                int OutStatus = 0;
                dee = _schoolRepository.AllotElectiveTheoryMarks(loginSession.SCHL, stdid, dtSub, class1, out OutStatus);
                var results = new
                {
                    status = OutStatus
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    status = flag
                };
                return Json(results);
            }

            //  return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
            // Do Stuff
        }

        [SessionCheckFilter]
        public ActionResult ElectiveTheoryReport(string id)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            TempData["ElectiveTheoryMarksPanelSearch"] = null;
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                MS.StoreAllData = _schoolRepository.ElectiveTheoryMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        public ActionResult ElectiveTheoryFinalReport(string id, FormCollection frm)
        {
            TempData["ElectiveTheoryMarksPanelSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;

                string Search = string.Empty;
                Search = "  a.schl = '" + loginSession.SCHL + "' and  a.class= '" + CLASS + "'";
                string OutError = "";
                DataSet dsFinal = new DataSet();
                MS.StoreAllData = dsFinal = _schoolRepository.ElectiveTheoryMarksEntryReport(loginSession.SCHL, 1, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowElectiveTheory = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowElectiveTheory = 1;
                    MS.StoreAllData = _schoolRepository.ElectiveTheoryMarksEntryReport(loginSession.SCHL, 0, Search, loginSession.SCHL, CLASS, out OutError);
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.ElectiveMarksFilledDate = MS.StoreAllData.Tables[0].Rows[0]["ElectiveMarksFilledDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }

                    if (dsFinal.Tables[3].Rows.Count > 0)
                    {
                        MS.schoolAllowForMarksEntry = new SchoolAllowForMarksEntry()
                        {
                            Cls = dsFinal.Tables[3].Rows[0]["Cls"].ToString(),
                            IsActive = Convert.ToInt32(dsFinal.Tables[3].Rows[0]["IsActive"].ToString()),
                            IsAllow = dsFinal.Tables[3].Rows[0]["IsAllow"].ToString(),
                            LastDate = Convert.ToString(dsFinal.Tables[3].Rows[0]["LastdateDT"].ToString()),
                            Panel = Convert.ToString(dsFinal.Tables[3].Rows[0]["Panel"].ToString())
                        };
                    }
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowElectiveTheory = 2;
                    ViewBag.IsFinal = 0;
                    ViewBag.TotalCount1 = 0;
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;

                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ElectiveTheoryFinalReport(string id)
        {
            TempData["ElectiveTheoryMarksPanelSearch"] = null;
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string CLASS = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = loginSession.SCHL;
                ViewBag.cid = id;
                string Search = string.Empty;
                string OutError = "";
                MS.StoreAllData = _schoolRepository.ElectiveTheoryMarksEntryReport(loginSession.SCHL, 2, Search, loginSession.SCHL, CLASS, out OutError);
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.ElectiveMarksFilledDate = MS.StoreAllData.Tables[0].Rows[0]["ElectiveMarksFilledDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    return View(MS);
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }



        #endregion


        #region Signature Chart and Confidential List Private  
        [SessionCheckFilter]
        public ActionResult SignatureChartPrivate(string id, SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChartPrivate", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            string Schl = loginSession.SCHL.ToString();
            string Cent = "";
            try
            {
                if (Schl != "")
                {
                    sm.ExamCent = loginSession.SCHL;
                    DataSet Dresult = new DataSet();
                    if (id.ToLower() == "primary")
                    {
                        Dresult = _schoolRepository.SignatureChartPrimarySub(sm);
                    }
                    else
                    {
                        Dresult = _schoolRepository.SignatureChartMiddleSub(sm);

                    }
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    List<SelectListItem> Sublist = new List<SelectListItem>();

                    if (Dresult.Tables[0].Rows.Count > 0)
                    {
                        Cent = Dresult.Tables[0].Rows[0]["cent"].ToString();
                        sm.ExamCent = Cent;
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                        }

                    }

                    if (Dresult.Tables[1].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in Dresult.Tables[1].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                    }

                    ViewBag.MySchCode = schllist;
                    ViewBag.MyExamSub = Sublist;

                    sm.ExamCent = Cent;
                    sm.ExamRoll = sm.ExamSub = "";

                    return View();
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult SignatureChartPrivate(string id, SchoolModels sm, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("SignatureChartPrivate", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = frc["ExamCent"].ToString();
                string roll = frc["ExamRoll"].ToString();
                if (Cent != "")
                {

                    sm.ExamCent = Cent;
                    sm.ExamSub = frc["ExamSub"].ToString();
                    sm.ExamRoll = frc["ExamRoll"].ToString();
                    DataSet Dresult = new DataSet();
                    if (id.ToLower() == "primary")
                    {
                        Dresult = _schoolRepository.SignatureChartPrimarySub(sm);
                    }
                    else
                    {
                        Dresult = _schoolRepository.SignatureChartMiddleSub(sm);
                    }
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                    {
                        schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                    }

                    ViewBag.MySchCode = schllist;
                    List<SelectListItem> Sublist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[1].Rows)
                    {
                        Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                    }

                    ViewBag.MyExamSub = Sublist;
                    if (id.ToLower() == "primary")
                    {
                        sm.StoreAllData = _schoolRepository.SignatureChartPrimary(sm);
                    }
                    else
                    {
                        sm.StoreAllData = _schoolRepository.SignatureChartMiddle(sm);
                    }
                    sm.ExamCent = Cent;
                    ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                    if (ViewBag.SearchMsg == 0)
                    {
                        ViewBag.Message = "No Record Found";
                    }
                    return View(sm);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        public ActionResult ConfidentialListPrivate(string id, SchoolModels sm)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ConfidentialListPrivate", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;

            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = "";
                if (Schl != "")
                {
                    sm.ExamCent = loginSession.SCHL;
                    DataSet Dresult = _schoolRepository.ConfidentialListMiddle(sm);
                    List<SelectListItem> schllist = new List<SelectListItem>();

                    if (Dresult.Tables[0].Rows.Count > 0)
                    {
                        Cent = Dresult.Tables[0].Rows[0]["cent"].ToString();
                        sm.ExamCent = Cent;
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                        }

                    }
                    ViewBag.MySchCode = schllist;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ConfidentialListPrivate(string id, SchoolModels sm, FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ConfidentialListPrivate", "School");
            }

            ViewBag.cid = id;
            sm.CLASS = id.ToString().ToLower().Trim() == "primary" ? "5" : id.ToString().ToLower().Trim() == "middle" ? "8" : "";
            ViewBag.Cls = sm.CLASS;
            try
            {

                string Schl = loginSession.SCHL.ToString();
                string Cent = frc["ExamCent"].ToString();
                if (Cent != "")
                {

                    sm.ExamCent = loginSession.SCHL;
                    DataSet Dresult = _schoolRepository.ConfidentialListMiddle(sm);
                    List<SelectListItem> schllist = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                    {
                        schllist.Add(new SelectListItem { Text = @dr["block"].ToString(), Value = @dr["cent"].ToString() });
                    }

                    ViewBag.MySchCode = schllist;
                    if (frc["ExamCent"].ToString() != "")
                    {
                        sm.ExamCent = frc["ExamCent"].ToString();
                    }

                    sm.StoreAllData = Dresult;// _schoolRepository.ConfidentialListMiddleDetail(sm);
                    ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                    return View(sm);
                }
                else
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.msg = "Data Not Found";
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        #endregion  Signature Chart and Confidential List Private  Pankaj/Dheeraj


        #region Signature Chart and Confidential List Primary-Middle

        #region Prac SignatureChart Primary
        public ActionResult PracSignatureChartPrimary()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = loginSession.EXAMCENT == null ? loginSession.SCHL : loginSession.EXAMCENT;
                    if (Schl != "")
                    {
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 2);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        sm.ExamCent = Cent;
                        sm.ExamSub = "";
                        sm.ExamRoll = "";
                        sm.CLASS = "5";

                        DataSet Subresult = objDB.GetSubFromSubMasters(5, "P", Schl, Cent); // for Primary prac sub
                        List<SelectListItem> Sublist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Subresult.Tables[0].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                        ViewBag.MyExamSub = Sublist;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        [HttpPost]
        public ActionResult PracSignatureChartPrimary(FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = frc["ExamCent"].ToString();
                    //string sub = frc["ExamSub"].ToString();
                    string roll = frc["ExamRoll"].ToString();
                    if (Cent != "")
                    {

                        sm.ExamCent = Cent;
                        sm.ExamSub = frc["ExamSub"].ToString();
                        sm.ExamRoll = frc["ExamRoll"].ToString();
                        sm.CLASS = "5";
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 5);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        //GetSubFromSubMasters(int cls , string type)
                        DataSet Subresult = objDB.GetSubFromSubMasters(5, "P", Schl, Cent); // for Primary prac sub
                        List<SelectListItem> Sublist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Subresult.Tables[0].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                        ViewBag.MyExamSub = Sublist;

                        sm.StoreAllData = objDB.PracSignatureChart(sm);
                        sm.ExamCent = Cent;
                        ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.SearchMsg = sm.StoreAllData.Tables[0].Rows.Count;
                        if (ViewBag.SearchMsg == 0)
                        {
                            ViewBag.Message = "No Record Found";
                        }
                        return View(sm);
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }


        public ActionResult PracConfidentialListPrimary()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = loginSession.EXAMCENT == null ? loginSession.SCHL : loginSession.EXAMCENT;
                    if (Schl != "")
                    {

                        sm.ExamCent = Cent;
                        sm.CLASS = "5";
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 5);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        [HttpPost]
        public ActionResult PracConfidentialListPrimary(FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = frc["ExamCent"].ToString();
                    if (Cent != "")
                    {

                        sm.ExamCent = Cent;

                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 2);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        string search;
                        if (frc["ExamCent"].ToString() != "")
                        {
                            sm.ExamCent = frc["ExamCent"].ToString();
                            sm.CLASS = "5";
                            //search = search + "'" + sm.ExamCent + "'";
                        }
                        //if (frc["ExamRoll"].ToString().Trim() !="")
                        //{
                        //    sm.ExamRoll = frc["ExamRoll"].ToString();
                        //    //search = search + "'" + sm.ExamCent + "'";
                        //}

                        sm.StoreAllData = objDB.PracConfidentialList(sm);
                        sm.ExamCent = loginSession.SCHL;
                        ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                        return View(sm);
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        #endregion  Prac SignatureChart Primary

        #region Prac SignatureChart Middle
        public ActionResult PracSignatureChartMiddle()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = loginSession.EXAMCENT;
                    if (Schl != "")
                    {
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 8);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        sm.ExamCent = Cent;
                        sm.ExamSub = "";
                        sm.ExamRoll = "";


                        //GetSubFromSubMasters(int cls , string type)
                        DataSet Subresult = objDB.GetSubFromSubMasters(4, "P", Schl, Cent); // for matric prac sub
                        List<SelectListItem> Sublist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Subresult.Tables[0].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                        ViewBag.MyExamSub = Sublist;

                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [HttpPost]
        public ActionResult PracSignatureChartMiddle(FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = frc["ExamCent"].ToString();
                    //string sub = frc["ExamSub"].ToString();
                    string roll = frc["ExamRoll"].ToString();
                    if (Cent != "")
                    {

                        sm.ExamCent = Cent;
                        sm.ExamSub = frc["ExamSub"].ToString();
                        sm.ExamRoll = frc["ExamRoll"].ToString();
                        sm.CLASS = "8";
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 4);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }
                        ViewBag.MySchCode = schllist;

                        //GetSubFromSubMasters(int cls , string type)
                        DataSet Subresult = objDB.GetSubFromSubMasters(4, "P", Schl, Cent); // for matric prac sub
                        List<SelectListItem> Sublist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Subresult.Tables[0].Rows)
                        {
                            Sublist.Add(new SelectListItem { Text = @dr["subnm"].ToString(), Value = @dr["sub"].ToString() });
                        }

                        ViewBag.MyExamSub = Sublist;

                        sm.StoreAllData = objDB.PracSignatureChart(sm);
                        sm.ExamCent = Cent;
                        ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.SearchMsg = sm.StoreAllData.Tables[1].Rows.Count;
                        if (ViewBag.SearchMsg == 0)
                        {
                            ViewBag.TotalCount = 0;
                            ViewBag.Message = "No Record Found";
                        }
                        return View(sm);
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        public ActionResult PracConfidentialListMiddle()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = Session["cent"].ToString();
                    if (Schl != "")
                    {

                        sm.ExamCent = Cent;
                        sm.CLASS = "8";
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 4);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }

        [HttpPost]
        public ActionResult PracConfidentialListMiddle(FormCollection frc)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            SchoolModels sm = new SchoolModels();
            PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
            try
            {
                if (loginSession.SCHL == null)
                { return RedirectToAction("Index", "Login"); }
                else
                {
                    string Schl = loginSession.SCHL;
                    string Cent = frc["ExamCent"].ToString();
                    if (Cent != "")
                    {

                        sm.ExamCent = Cent;
                        sm.CLASS = "8";
                        //DataSet Dresult = objDB.GetCentcode(Schl);
                        DataSet Dresult = objDB.GetPracCentcodeByClass(Schl, 4);
                        List<SelectListItem> schllist = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in Dresult.Tables[0].Rows)
                        {
                            schllist.Add(new SelectListItem { Text = @dr["pcentNM"].ToString(), Value = @dr["pcent"].ToString() });
                        }

                        ViewBag.MySchCode = schllist;
                        string search;
                        if (frc["ExamCent"].ToString() != "")
                        {
                            sm.ExamCent = frc["ExamCent"].ToString();
                            //search = search + "'" + sm.ExamCent + "'";
                        }
                        //if (frc["ExamRoll"].ToString().Trim() !="")
                        //{
                        //    sm.ExamRoll = frc["ExamRoll"].ToString();
                        //    //search = search + "'" + sm.ExamCent + "'";
                        //}

                        sm.StoreAllData = objDB.PracConfidentialList(sm);
                        sm.ExamCent = Session["cent"].ToString();
                        ViewBag.TotalCount = sm.StoreAllData.Tables[0].Rows.Count;
                        return View(sm);
                    }
                    else
                    {
                        ViewBag.TotalCount = 0;
                        ViewBag.msg = "Data Not Found";
                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Logout", "Login");
            }

        }
        #endregion  Prac SignatureChart Middle

        #endregion Signature Chart and Confidential List Primary-Middle

        #region ExamCentreResources
        [SessionCheckFilter]
        public ActionResult ExamCentreResources(ExamCentreResources model)
        {
            ViewBag.YesNoList = DBClass.GetYesNoText();
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            model = new ExamCentreResources();

            if (string.IsNullOrEmpty(loginSession.EXAMCENT))
            {
                return RedirectToAction("Index", "Home");
            }

            bool isExists = _context.ExamCentreResources.Where(s => s.schl == loginSession.SCHL).Count() > 0 ? true : false;
            if (!isExists)
            {
                model.id = 0;
                model.schl = loginSession.SCHL;
            }
            else
            {
                model = _context.ExamCentreResources.Where(s => s.schl == loginSession.SCHL).FirstOrDefault();
            }

            if (TempData["resultIns"] != null)
            {
                ViewData["resultIns"] = TempData["resultIns"];
            }

            return View(model);
        }


        [SessionCheckFilter]
        [HttpPost]
        public async Task<ActionResult> ExamCentreResources(ExamCentreResources model, string cmd, FormCollection fc)
        {
            ViewBag.YesNoList = DBClass.GetYesNoText();

            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (string.IsNullOrEmpty(loginSession.EXAMCENT))
            {
                return RedirectToAction("Index", "Home");
            }

            bool isExists = _context.ExamCentreResources.Where(s => s.schl == loginSession.SCHL).Count() > 0 ? true : false;
            if (!isExists)
            {
                model.id = 0;
                model.schl = loginSession.SCHL;
            }

            try
            {

                if (cmd.ToLower().Contains("save") && model.id == 0 && !isExists)
                {

                    ExamCentreResources examCentreResources = new ExamCentreResources()
                    {
                        id = 0,
                        schl = model.schl,
                        internetAvailability = model.internetAvailability,
                        printerAvailability = model.printerAvailability,
                        perMinutePrintingSpeed = model.perMinutePrintingSpeed,
                        powerBackup = model.powerBackup,
                        photostateAvailability = model.photostateAvailability,
                        submitBy = model.schl,
                        submitOn = DateTime.Now
                    };

                    _context.ExamCentreResources.Add(examCentreResources);
                    int insertedRecords = await _context.SaveChangesAsync();

                    if (insertedRecords > 0)
                    {
                        TempData["resultIns"] = "S";
                    }
                }

                else if (cmd.ToLower().Contains("update") && model.id > 0)
                {
                    using (DbContextTransaction transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            ExamCentreResources examCentreResources = _context.ExamCentreResources.Find(model.id);
                            examCentreResources.internetAvailability = model.internetAvailability;
                            examCentreResources.printerAvailability = model.printerAvailability;
                            examCentreResources.perMinutePrintingSpeed = model.perMinutePrintingSpeed;
                            examCentreResources.powerBackup = model.powerBackup;
                            examCentreResources.photostateAvailability = model.photostateAvailability;
                            examCentreResources.modifiedBy = model.schl;
                            examCentreResources.modifiedOn = DateTime.Now;
                            _context.Entry(examCentreResources).State = EntityState.Modified;
                            _context.SaveChanges();
                            TempData["resultIns"] = "M";

                            transaction.Commit();//transaction commit
                        }
                        catch (Exception ex1)
                        {

                            transaction.Rollback();
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                TempData["resultIns"] = "Error : " + ex.Message;
            }
            return RedirectToAction("ExamCentreResources", "School");
        }

        #endregion


        #region Exam Centre Confidential Resources
        [SessionCheckFilter]
        public ActionResult ExamCentreConfidentialResources(ExamCentreConfidentialResources model)
        {
            ViewBag.YesNoList = DBClass.GetYesNoText();
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            model = new ExamCentreConfidentialResources();

            if (string.IsNullOrEmpty(loginSession.EXAMCENT))
            {
                return RedirectToAction("Index", "Home");
            }

            model.id = 0;
            model.schl = loginSession.SCHL;
            ViewBag.schlnme = loginSession.SCHLNME;
            model.principal = loginSession.PRINCIPAL;
            model.mobile = loginSession.MOBILE;

            if (string.IsNullOrEmpty(loginSession.MOBILE))
            { ViewBag.MOBILE = loginSession.MOBILE; }
            else
            {
                ViewBag.MOBILE = "xxxxxx" + loginSession.MOBILE.Substring(Math.Max(0, loginSession.MOBILE.Length - 4));
            }

            string FilepathExist = Path.Combine(Server.MapPath("~/ConfidentialFiles"));
            if (!Directory.Exists(FilepathExist))
            {
                Directory.CreateDirectory(FilepathExist);
            }

            string targetDirectory = Server.MapPath("~/ConfidentialFiles/");
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.pdf", SearchOption.AllDirectories);
            model.confidentialFiles = fileEntries;

            if (TempData["resultIns"] != null)
            {
                ViewData["resultIns"] = TempData["resultIns"];
            }
            return View(model);
        }

        [HttpGet]
        public virtual ActionResult DownloadConfidentialFiles(string fileName)
        {
            if (Path.GetExtension(fileName) == ".pdf")
            {
                string targetDirectory = Server.MapPath("~/ConfidentialFiles/");
                string fullPath = Path.Combine(targetDirectory, fileName);

                return File(fullPath, "application/pdf", fileName);
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }
        }
        #endregion

        [SessionCheckFilter]
        public async Task<ActionResult> InfrasturePerforma(InfrasturePerformas ipm)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                string SCHL = Convert.ToString(loginSession.SCHL);
                ViewBag.Dist = Convert.ToString(loginSession.DIST);
                if (SCHL != "")
                {
                    PsebJunior.AbstractLayer.RegistrationDB objDB = new PsebJunior.AbstractLayer.RegistrationDB();
                    ViewBag.Eighth = loginSession.middle == "Y" ? "1" : "0";
                    ViewBag.Fifth = loginSession.fifth == "Y" ? "1" : "0";
                    ipm = await new PsebJunior.AbstractLayer.SchoolDB().GetInfrasturePerformaBySCHL(loginSession);
                    DataSet dschool = new DataSet();
                    //var lst = await new AbstractLayer.SchoolDB().GetInfrastureTblSchUserPerformaBySCHL(loginSession);
                    dschool = new PsebJunior.AbstractLayer.SchoolDB().GetSchoolUserTableData(loginSession.SCHL);
                    if (dschool.Tables[0].Rows.Count > 0)
                    {
                        ViewData["geolocation"] = dschool.Tables[0].Rows[0]["geolocation"].ToString();
                        ViewData["imgpath"] = ConfigurationManager.AppSettings["AWSURL"] + dschool.Tables[0].Rows[0]["imgpath"].ToString();
                    }


                    DataSet ds = new DataSet();
                    ds = new PsebJunior.AbstractLayer.SchoolDB().PanelEntryLastDate("InfrasturePerforma");
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        ViewData["lastDate"] = ds.Tables[1].Rows[0]["LastDate"].ToString();
                    }
                    else
                    {
                        ViewData["lastDate"] = "";
                    }
                    if (ds.Tables[0].Rows.Count == 0)
                    {
                        ViewData["lastDateOver"] = 1;
                    }
                    else
                    {
                        ViewData["lastDateOver"] = 0;
                    }
                    DataSet dss = new DataSet();
                    dss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterName(loginSession.SCHL.ToString());
                    if (dss.Tables[0].Rows.Count > 0)
                    {
                        var schoolCenterNames = dss.Tables[0].AsEnumerable().Select(dataRow => new SchoolCenterName
                        {
                            cschl = dataRow.Field<string>("cschl").ToString(),
                            CENT = dataRow.Field<string>("CENT").ToString(),
                            CLASS = dataRow.Field<string>("CLASS").ToString(),
                            schlnme = dataRow.Field<string>("schlnme").ToString()
                        }).ToList();
                        ViewBag.SchoolCenterName = schoolCenterNames;
                    }
                    DataSet dsss = new DataSet();
                    dsss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterNameNearest(loginSession.DIST.ToString());
                    if (dsss.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemsTeh = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dsss.Tables[0].Rows)
                        {
                            itemsTeh.Add(new SelectListItem { Text = @dr["schlnme"].ToString(), Value = @dr["schlnme"].ToString() });
                        }
                        ViewBag.SchoolCenterNameNearest = new SelectList(itemsTeh, "Value", "Text");
                    }
                    DataSet dssss = new DataSet();
                    dssss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterAnswerSheetNearest(loginSession.DIST.ToString());
                    if (dssss.Tables[0].Rows.Count > 0)
                    {
                        List<SelectListItem> itemsTeh = new List<SelectListItem>();
                        foreach (System.Data.DataRow dr in dssss.Tables[0].Rows)
                        {
                            itemsTeh.Add(new SelectListItem { Text = @dr["SchoolCode"].ToString() + "," + @dr["SchoolName"].ToString(), Value = @dr["SchoolCode"].ToString() + "," + @dr["SchoolName"].ToString() });
                        }
                        ViewBag.SchoolCenterAnwerNameNearest = new SelectList(itemsTeh, "Value", "Text");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View(ipm);
        }

        [SessionCheckFilter]
        [HttpPost]
        public async Task<ActionResult> InfrasturePerforma(InfrasturePerformas ipm, string cmd)
        {
            try
            {
                LoginSession loginSession = (LoginSession)Session["LoginSession"];
                string geolocation = "";
                string imgpath = "";
                DataSet dschool = new DataSet();
                dschool = new PsebJunior.AbstractLayer.SchoolDB().GetSchoolUserTableData(loginSession.SCHL.ToString());
                if (dschool.Tables[0].Rows.Count > 0)
                {
                    ViewData["geolocation"] = dschool.Tables[0].Rows[0]["geolocation"].ToString();
                    ViewData["imgpath"] = ConfigurationManager.AppSettings["AWSURL"] + dschool.Tables[0].Rows[0]["imgpath"].ToString();
                    geolocation = dschool.Tables[0].Rows[0]["geolocation"].ToString();
                    imgpath = dschool.Tables[0].Rows[0]["imgpath"].ToString();
                }

                PsebJunior.AbstractLayer.RegistrationDB objDB = new PsebJunior.AbstractLayer.RegistrationDB();
                DataSet result = objDB.schooltypes(loginSession.SCHL.ToString()); // passing Value to DBClass from model
                if (result == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.Tables[1].Rows.Count > 0)
                {
                    ViewBag.Fifth = result.Tables[1].Rows[0]["Fifth"].ToString();
                    ViewBag.Eighth = result.Tables[1].Rows[0]["Eighth"].ToString();
                    ViewBag.Senior = result.Tables[1].Rows[0]["Senior"].ToString();
                    ViewBag.Matric = result.Tables[1].Rows[0]["Matric"].ToString();
                    ViewBag.N3M1threclock = result.Tables[2].Rows[0]["reclock9th"].ToString();
                    ViewBag.E1T1threclock = result.Tables[3].Rows[0]["reclock11th"].ToString();
                }

                DataSet ds = new DataSet();
                ds = new PsebJunior.AbstractLayer.SchoolDB().PanelEntryLastDate("InfrasturePerforma");
                if (ds.Tables[1].Rows.Count > 0)
                {
                    ViewData["lastDate"] = ds.Tables[1].Rows[0]["LastDate"].ToString();
                }
                else
                {
                    ViewData["lastDate"] = "";
                }
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ViewData["lastDateOver"] = 1;
                }
                else
                {
                    ViewData["lastDateOver"] = 0;
                }

                bool bCheck = true;
                if (ipm.IFSC1 == "")
                {
                    ViewData["result"] = "IFSC 1 is required";
                    bCheck = false;
                }
                else if (ipm.BankBranch1 == "")
                {
                    ViewData["result"] = "Bank Branch 1 is required";
                    bCheck = false;
                }

                if (new PsebJunior.AbstractLayer.SchoolDB().IFSCCheck(ipm.IFSC1, ipm.Bank1) == false)
                {
                    ViewData["result"] = "IFSC 1 is not correct.";
                    bCheck = false;
                }
                if (ipm.IFSC2 != null)
                {
                    if (ipm.IFSC1.ToLower() == ipm.IFSC2.ToLower())
                    {
                        ViewData["result"] = "IFSC 1 and IFSC 2 should be diffrent.";
                        bCheck = false;
                    }
                    else if (new PsebJunior.AbstractLayer.SchoolDB().IFSCCheck(ipm.IFSC2, ipm.Bank2) == false)
                    {
                        ViewData["result"] = "IFSC 2 is not correct.";
                        bCheck = false;
                    }
                }
                if (ipm.IFSC3 != null)
                {
                    if (ipm.IFSC3.ToLower() == ipm.IFSC2.ToLower())
                    {
                        ViewData["result"] = "IFSC 2 and IFSC 3 should be diffrent.";
                        bCheck = false;
                    }
                    else if (new PsebJunior.AbstractLayer.SchoolDB().IFSCCheck(ipm.IFSC3, ipm.Bank3) == false)
                    {
                        ViewData["result"] = "IFSC 3 is not correct.";
                        bCheck = false;
                    }
                }

                //if (ipm.SchoolName1 == "Select School Name" && ipm.SchoolName2 == "Select School Name" && ipm.SchoolName3 == "Select School Name")
                //{

                //    ViewData["result"] = "Please Select Atleast One School for Answer Sheet Collection Centre";
                //    bCheck = false;

                //}
                //if (ipm.SchoolName1 == null && ipm.SchoolName2 == null && ipm.SchoolName3 == null)
                //{

                //    ViewData["result"] = "Please Select Atleast One School for Answer Sheet Collection Centre";
                //    bCheck = false;

                //}
                //if (ipm.Statisfied8th == "No" && ipm.SchoolCenterNewFor8th == null)
                //{

                //    ViewData["result"] = "Please Select School Name for New Exam Centre";
                //    bCheck = false;

                //}
                //if (ipm.Statisfied10th == "No" && ipm.SchoolCenterNewFor10th == null)
                //{

                //    ViewData["result"] = "Please Select School Name for New Exam Centre";
                //    bCheck = false;

                //}
                //if (ipm.Statisfied12th == "No" && ipm.SchoolCenterNewFor12th == null)
                //{

                //    ViewData["result"] = "Please Select School Name for New Exam Centre";
                //    bCheck = false;

                //}






                DataSet dss = new DataSet();
                dss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterName(loginSession.SCHL.ToString());
                if (dss.Tables[0].Rows.Count > 0)
                {
                    var schoolCenterNames = dss.Tables[0].AsEnumerable().Select(dataRow => new SchoolCenterName
                    {
                        cschl = dataRow.Field<string>("cschl").ToString(),
                        CENT = dataRow.Field<string>("CENT").ToString(),
                        CLASS = dataRow.Field<string>("CLASS").ToString(),
                        schlnme = dataRow.Field<string>("schlnme").ToString()
                    }).ToList();
                    ViewBag.SchoolCenterName = schoolCenterNames;
                }
                DataSet dsss = new DataSet();
                dsss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterNameNearest(loginSession.DIST.ToString());
                if (dsss.Tables[0].Rows.Count > 0)
                {
                    List<SelectListItem> itemsTeh = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in dsss.Tables[0].Rows)
                    {
                        itemsTeh.Add(new SelectListItem { Text = @dr["schlnme"].ToString(), Value = @dr["schlnme"].ToString() });
                    }
                    ViewBag.SchoolCenterNameNearest = new SelectList(itemsTeh, "Value", "Text");
                }
                DataSet dssss = new DataSet();
                dssss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterAnswerSheetNearest(loginSession.DIST.ToString());
                if (dssss.Tables[0].Rows.Count > 0)
                {
                    List<SelectListItem> itemsTeh = new List<SelectListItem>();
                    foreach (System.Data.DataRow dr in dssss.Tables[0].Rows)
                    {
                        itemsTeh.Add(new SelectListItem { Text = @dr["SchoolCode"].ToString() + "," + @dr["SchoolName"].ToString(), Value = @dr["SchoolCode"].ToString() + "," + @dr["SchoolName"].ToString() });
                    }
                    ViewBag.SchoolCenterAnwerNameNearest = new SelectList(itemsTeh, "Value", "Text");
                }

                if (bCheck == true)
                {


                    if (cmd == null)
                    {
                        return RedirectToAction("ViewInfrasturePerforma", "School");
                    }
                    else if (cmd.ToLower() == "save")
                    {
                        string SCHL = Convert.ToString(loginSession.SCHL.ToString());
                        if (SCHL != "")
                        {
                            int a = 0;
                            ipm.FinalSubmitStatus = 0;
                            ipm.FinalSubmitDate = null;
                            ipm = await new PsebJunior.AbstractLayer.SchoolDB().UpdateInfrasturePerformaBySCHL(ipm, out a);
                            ViewData["result"] = a;
                        }
                    }
                    else if (cmd.ToLower() == "final submit")
                    {
                        if (!string.IsNullOrEmpty(geolocation) && !string.IsNullOrEmpty(imgpath))
                        {
                            string SCHL = Convert.ToString(loginSession.SCHL.ToString());
                            if (SCHL != "")
                            {
                                int a = 0;
                                ipm.FinalSubmitStatus = 1;
                                ipm.FinalSubmitDate = DateTime.Now.ToString();
                                ipm = await new PsebJunior.AbstractLayer.SchoolDB().UpdateInfrasturePerformaBySCHL(ipm, out a);
                                ViewData["result"] = a;
                            }
                        }
                        else
                        {
                            ViewData["result"] = "Update the image of the entry gate of the school using the PSEB ExamInfraLocate app. You can download the app from\r\nthe Play Store or directly from the App link.";
                            bCheck = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return View(ipm);
        }

        public async Task<ActionResult> ViewInfrasturePerformas(string SCHL)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            InfrasturePerformasviewModel ipm = new InfrasturePerformasviewModel();
            try
            {
                PsebJunior.AbstractLayer.SchoolDB objDB = new PsebJunior.AbstractLayer.SchoolDB();
                if (loginSession.SCHL != "")
                {
                    DataSet dss = new DataSet();
                    dss = new PsebJunior.AbstractLayer.SchoolDB().SchoolCenterName(loginSession.SCHL);
                    if (dss.Tables[0].Rows.Count > 0)
                    {
                        var schoolCenterNames = dss.Tables[0].AsEnumerable().Select(dataRow => new SchoolCenterName
                        {
                            cschl = dataRow.Field<string>("cschl").ToString(),
                            CENT = dataRow.Field<string>("CENT").ToString(),
                            CLASS = dataRow.Field<string>("CLASS").ToString(),
                            schlnme = dataRow.Field<string>("schlnme").ToString()
                        }).ToList();
                        ViewBag.SchoolCenterName = schoolCenterNames;
                    }

                    InfrasturePerformasList oInfrasturePerformas = new InfrasturePerformasList();
                    oInfrasturePerformas = await new PsebJunior.AbstractLayer.SchoolDB().GetInfrasturePerformaBySCHLList(loginSession.SCHL);
                    ipm.ipf = oInfrasturePerformas;
                    DataSet ds = new DataSet();
                    SchoolModels sm = objDB.GetSchoolDataBySchl(Convert.ToString(loginSession.SCHL), out ds);
                    ipm.schlmodel = sm;
                }

            }
            catch (Exception ex)
            {

            }
            return View(ipm);
        }


        #region ExamCentreDetails

        public ActionResult ExamCentreDetails()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            if (loginSession.SCHL == null)
            {
                return RedirectToAction("Index", "Login");
            }

            DataSet result = RegistrationDB.Get_School_Center_Choice_All();
            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();

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



            DataSet dsss = new DataSet();
            dsss = new SchoolDB().SchoolCenterNameNearestWithSchool(loginSession.DIST.ToString());
            if (dsss.Tables[0].Rows.Count > 0)
            {
                string type = "";
                List<SelectListItem> itemsTehnew = new List<SelectListItem>();
                List<SelectListItem> itemsTehold = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in dsss.Tables[0].Rows)
                {
                    string schl = @dr["schl"].ToString();
                    if (loginSession.SCHL != schl)
                    {
                        type = @dr["type"].ToString();
                        if (objGroupList.Count() > 0)
                        {
                            string schoolcode = @dr["SCHL"].ToString();
                            var counts = objGroupList.Any(x => x.choiceschoolcode == schoolcode);
                            if (counts)
                            {

                            }
                            else
                            {
                                if (type == "OLD")
                                {
                                    itemsTehold.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                                }
                                else
                                {
                                    itemsTehnew.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                                }
                            }

                        }
                        else
                        {
                            if (type == "OLD")
                            {
                                itemsTehold.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                            }
                            else
                            {
                                itemsTehnew.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                            }
                        }
                    }

                }
                ViewBag.SchoolCenterNameNearestNew = new SelectList(itemsTehnew, "Value", "Text");
                ViewBag.SchoolCenterNameNearestOld = new SelectList(itemsTehold, "Value", "Text");
            }


            return View(loginSession);
        }

        [HttpPost]
        public ActionResult ExamCentreDetails(FormCollection frm, string id, string cmd)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];

            int result = 0;
            string PricipleName = frm["PricipleName"].ToString();
            string stdCode = frm["stdCode"].ToString();
            string phone = frm["phone"].ToString();
            string Mobile = frm["Mobile"].ToString();
            string Priciple2Name = frm["Priciple2Name"].ToString();
            string Priciple2Mobile = frm["Priciple2Mobile"].ToString();

            if (cmd.ToLower() == "final submit")
            {
                result = SchoolDB.sp_Update_school_center_choice(frm, 1);
                if (result == 1)
                {
                    loginSession.Finalsubmittedforchoice = 1;
                    ViewData["Status"] = 1;
                }
                else
                {
                    ViewData["Status"] = 0;
                }
            }
            else
            {
                result = SchoolDB.sp_Update_school_center_choice(frm, 0);
                if (result == 1)
                {
                    loginSession.PRINCIPAL = PricipleName;
                    loginSession.STDCODE = stdCode;
                    loginSession.PHONE = phone;
                    loginSession.MOBILE = Mobile;
                    loginSession.PrincipalName2 = Priciple2Name;
                    loginSession.PrincipalMobile2 = Priciple2Mobile;
                    ViewData["Status"] = 1;
                }
                else
                {
                    ViewData["Status"] = 0;
                }
            }

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();
            DataSet dsss = new DataSet();
            dsss = new SchoolDB().SchoolCenterNameNearestWithSchool(loginSession.DIST.ToString());
            if (dsss.Tables[0].Rows.Count > 0)
            {
                string type = "";
                List<SelectListItem> itemsTehnew = new List<SelectListItem>();
                List<SelectListItem> itemsTehold = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in dsss.Tables[0].Rows)
                {
                    if (objGroupList.Count() > 0)
                    {
                        string schoolcode = @dr["SCHL"].ToString();
                        type = @dr["type"].ToString();
                        var counts = objGroupList.Any(x => x.choiceschoolcode == schoolcode);
                        if (counts)
                        {

                        }
                        else
                        {
                            if (type == "OLD")
                            {
                                itemsTehold.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                            }
                            else
                            {
                                itemsTehnew.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                            }
                        }

                    }
                    else
                    {
                        if (type == "OLD")
                        {
                            itemsTehold.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                        }
                        else
                        {
                            itemsTehnew.Add(new SelectListItem { Text = @dr["schoole"].ToString(), Value = @dr["schoole"].ToString() });
                        }
                    }

                }
                ViewBag.SchoolCenterNameNearestNew = new SelectList(itemsTehnew, "Value", "Text");
                ViewBag.SchoolCenterNameNearestOld = new SelectList(itemsTehold, "Value", "Text");
            }



            return View(loginSession);

        }

        [SessionCheckFilter]
        public async Task<ActionResult> ExamCentreDetailsPerforma()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.SCHL == null)
            {
                return RedirectToAction("Index", "Login");
            }

            List<ExamCenterDetail> objGroupList = new List<ExamCenterDetail>();
            DataSet result = RegistrationDB.Get_School_Center_Choice_All();
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
            DataSet ds = new DataSet();
            SchoolModels sm = _schoolRepository.GetSchoolDataBySchl(loginSession.SCHL, out ds);
            ViewBag.SchoolModel = sm;
            ViewBag.objGroupList = objGroupList;
            return View(loginSession);
        }

        #endregion



    }
}