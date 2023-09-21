using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PsebJunior.AbstractLayer;
using System.IO;
using System.Threading.Tasks;
using PsebPrimaryMiddle.Repository;
using PsebPrimaryMiddle.Filters;
using Newtonsoft.Json;
using System.Web.Caching;

namespace PsebPrimaryMiddle.Controllers
{
    public class CenterHeadController : Controller
    {

        private readonly ISchoolRepository _schoolRepository;
        private readonly ICenterHeadRepository _centerheadrepository;
        private readonly IAdminRepository _adminRepository;

        public CenterHeadController(ICenterHeadRepository centerheadrepository, ISchoolRepository schoolRepository, IAdminRepository adminRepository)
        {
            _centerheadrepository = centerheadrepository;
            _schoolRepository = schoolRepository;
            _adminRepository = adminRepository;
        }


        #region  ForgotPassword 

        public ActionResult ForgotPassword()
        {
           
            ViewBag.SubmitValue = "Send";
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(CenterHeadMasterModel centerHeadMasterModel)
        {
            ViewBag.SubmitValue = "Send";
            string sid = centerHeadMasterModel.UserName;
            DataSet ds = _centerheadrepository.CenterHeadMaster(6, Convert.ToInt32(sid), "");
            if (ds == null)            
            {
                TempData["result"] = ViewData["result"] = "0";
            }

           else if (ds.Tables[0].Rows.Count > 0)
            {

                centerHeadMasterModel = new CenterHeadMasterModel();

                var _list = ds.Tables[0].AsEnumerable().Select(dataRow => new CenterHeadMasterModel
                {
                    
                    UserName = dataRow.Field<string>("UserName"),
                    Pwd = dataRow.Field<string>("pwd"),
                    Mobile = dataRow.Field<string>("Mobile"),
                    EmailId = dataRow.Field<string>("EmailId"),
                });

                centerHeadMasterModel = _list.ToList().Where(s => s.UserName == sid)
                    .SingleOrDefault<CenterHeadMasterModel>();
                
                TempData["result"] = ViewData["result"] = "1";
            }
            else {
                TempData["result"] = ViewData["result"] = "0";
            }
            
            
            return View(centerHeadMasterModel);
        }
        #endregion  ForgotPassword 
        #region center head
        public JsonResult GetClusterByDist(string dist) // Calling on http post (on Submit)
        {
            DataSet ds = _centerheadrepository.CenterHeadMaster(5, 0, dist);
            ClusterReportModel clusterReportModel = new ClusterReportModel();
            
            var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new ClusterModel
            {
                ccode = dataRow.Field<string>("ccode"),
                clusternm = dataRow.Field<string>("clusterdetails"),
            });
            clusterReportModel.ClusterList= eList.ToList();
            return Json(clusterReportModel.ClusterList);
        }

        #endregion

        #region Center Head Login 
        [Route("CenterHead")]
        public ActionResult Index()
        {

            if (TempData["result"] != null)
            {
                ViewData["result"] = TempData["result"];
            }
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            try
            {
                ViewBag.SessionList = DBClass.GetSession().ToList();
                return View();
            }
            catch (Exception ex)
            {

                return View();
            }
        }


        [Route("CenterHead")]
        [HttpPost]
        public async Task<ActionResult> Index(LoginModel lm)
        {
            try
            {
                CenterHeadLoginSession CenterHeadLoginSession = await _centerheadrepository.CheckCenterHeadLogin(lm); // passing Value to _schoolRepository.from model and Type 1 For regular              
                CenterHeadLoginSession.CurrentSession = lm.Session;
                TempData["result"] = CenterHeadLoginSession.LoginStatus;
                if (CenterHeadLoginSession.LoginStatus == 1)
                {
                    Session["CenterHeadLoginSession"] = CenterHeadLoginSession;
                    return RedirectToAction("Welcome", "CenterHead");
                }
                return RedirectToAction("Index", "CenterHead");
            }
            catch (Exception ex)
            {
                TempData["result"] = "Error : " + ex.Message;
                return RedirectToAction("Index", "CenterHead");
            }
        }


        public ActionResult Logout()
        {
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            TempData.Clear();
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "CenterHead");
        }

        #endregion CenterHead Login


        #region  Change_Password

        [CenterHeadLoginCheckFilter]
        public ActionResult Change_Password()
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            ViewBag.User = centerHeadLoginSession.UserName.ToString();
            return View();
        }

        [CenterHeadLoginCheckFilter]
        [HttpPost]
        public ActionResult Change_Password(FormCollection frm)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            ViewBag.User = centerHeadLoginSession.UserName.ToString();

            string CurrentPassword = string.Empty;
            string NewPassword = string.Empty;

            if (frm["ConfirmPassword"] != "" && frm["NewPassword"] != "")
            {
                if (frm["ConfirmPassword"].ToString() == frm["NewPassword"].ToString())
                {
                    CurrentPassword = frm["CurrentPassword"].ToString();
                    NewPassword = frm["NewPassword"].ToString();
                    int result = _centerheadrepository.ChangePassword(centerHeadLoginSession.UserName.ToString(), CurrentPassword, NewPassword);
                    if (result > 0)
                    {
                        ViewData["resultDCP"] = 1;
                        return View();
                        // return RedirectToAction("Index", "DM");
                    }
                    else
                    {
                        ViewData["resultDCP"] = 0;
                        ModelState.AddModelError("", "Not Update");
                        return View();
                    }
                }
                else
                {
                    ViewData["resultDCP"] = 3;
                    ModelState.AddModelError("", "Fill All Fields");
                    return View();
                }
            }
            else
            {
                ViewData["resultDCP"] = 2;
                ModelState.AddModelError("", "Fill All Fields");
                return View();
            }
        }
        #endregion  Change_Password


        #region ViewProfile
        [CenterHeadLoginCheckFilter]
        public ActionResult ViewProfile(CenterHeadMasterModel centerHeadMasterModel)
        {
            try
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
                //CenterHeadMaster(int type, int id, string Search)
                DataSet ds = _centerheadrepository.CenterHeadMaster(1, centerHeadLoginSession.CenterHeadId,"");

                centerHeadMasterModel = new CenterHeadMasterModel();

                var _list = ds.Tables[0].AsEnumerable().Select(dataRow => new CenterHeadMasterModel
                {
                    CenterHeadId = dataRow.Field<int>("CenterHeadId"),
                    UserName = dataRow.Field<string>("UserName"),
                    CenterHeadName = dataRow.Field<string>("CenterHeadName"),
                    Mobile = dataRow.Field<string>("Mobile"),
                    EmailId = dataRow.Field<string>("EmailId"),
                });

                centerHeadMasterModel = _list.ToList().Where(s=>s.CenterHeadId == centerHeadLoginSession.CenterHeadId)
                                        .SingleOrDefault<CenterHeadMasterModel>();
                


                return View(centerHeadMasterModel);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }

        [CenterHeadLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewProfile(CenterHeadMasterModel centerHeadMasterModel, FormCollection frm)
        {
            try
            {                
                int OutStatus;                
                DataSet ds = _centerheadrepository.UpdateCenterHeadMaster(centerHeadMasterModel, out OutStatus);
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


        [CenterHeadLoginCheckFilter]
        public ActionResult Welcome(CenterHeadModel CenterHeadModel, int? page)
        {

            Printlist obj = new Printlist();
            #region Circular

            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            string Search = string.Empty;
            Search = "Id like '%' and CircularTypes like '%7%' and Convert(Datetime,Convert(date,ExpiryDateDD))>=Convert(Datetime,Convert(date,getdate()))";


            // Cache
            DataSet dsCircular = new DataSet();
            DataSet cacheData = HttpContext.Cache.Get("CenterHeadCircular") as DataSet;

            if (cacheData == null)
            {
                dsCircular = _adminRepository.CircularMaster(Search, pageIndex);
                cacheData = dsCircular;
                HttpContext.Cache.Insert("CenterHeadCircular", cacheData, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);

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


            return View(obj);
        }


        [CenterHeadLoginCheckFilter]
        public ActionResult ViewAllSchoolsAllowed(CenterSchoolModelList centerSchoolModelList)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];

            centerSchoolModelList.centerSchoolModels = _centerheadrepository.SchoolListByCenterId(4, centerHeadLoginSession.CenterHeadId,"");
            return View(centerSchoolModelList);
        }

        #region  Marks Entry Panel For Primary Class 

        [Route("MarksEntryPanel/{id}/{schl}")]
        [CenterHeadLoginCheckFilter]     
        [CenterHeadAfterLoginFilter]
        public ActionResult MarksEntryPanel(string id, string schl, int? page)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            SchoolModels MS = new SchoolModels();
            ViewBag.cid = id;
            ViewBag.schlCode = schl;
            string cls = id == "Primary" ? "5" : id == "Middle" ? "8" : "";

            var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();
            ViewBag.SelectedFilter = "0";

            var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
            ViewBag.MyAction = itemAction.ToList();
            ViewBag.SelectedAction = "0";

            #region  Check School Allow For MarksEntry
            //SchoolAllowForMarksEntry schoolAllowForMarksEntry = _schoolRepository.SchoolAllowForMarksEntry(loginSession.SCHL, cls);
            #endregion  Check School Allow For MarksEntry

            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            ViewBag.pagesize = pageIndex;

            string Search = string.Empty;
            string SelectedAction = "0";          
            Search = " a.class='" + cls + "' ";
            MS.StoreAllData = _schoolRepository.GetMarksEntryDataBySCHL(Search, schl, pageIndex, cls, Convert.ToInt32(SelectedAction));
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
                }
                if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                {
                    MS.schoolAllotedToCenterMaster = new SchoolAllotedToCenterMaster()
                    {
                        IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),                      
                        LastDate = Convert.ToDateTime(MS.StoreAllData.Tables[5].Rows[0]["LastDate"].ToString()),
                        SCHLNME = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["schlnme"].ToString()),
                        Schl = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["SCHL"].ToString())
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
                }

                if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                {
                    MS.schoolAllotedToCenterMaster = new SchoolAllotedToCenterMaster()
                    {
                        IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                        LastDate = Convert.ToDateTime(MS.StoreAllData.Tables[5].Rows[0]["LastDate"].ToString()),
                        SCHLNME = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["schlnme"].ToString()),
                        Schl = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["SCHL"].ToString())
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

            }

            return View(MS);
        }


        [Route("MarksEntryPanel/{id}/{schl}")]     
        [CenterHeadLoginCheckFilter]
        [CenterHeadAfterLoginFilter]
        [HttpPost]
        public ActionResult MarksEntryPanel( string id, string schl, int? page, FormCollection frm)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            SchoolModels MS = new SchoolModels();
            ViewBag.cid = id;
            ViewBag.schlCode = schl;

            string cls = id == "Primary" ? "5" : id == "Middle" ? "8" : "";



            var itemFilter = new SelectList(new[] { new { ID = "1", Name = "By RollNo" }, new { ID = "2", Name = "By UniqueID" }, new { ID = "3", Name = "By REGNO" },
                    new { ID = "4", Name = "Candidate Name" }, }, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();
            ViewBag.SelectedFilter = "0";

            var itemAction = new SelectList(new[] { new { ID = "1", Name = "All" }, new { ID = "2", Name = "Pending" }, new { ID = "3", Name = "Filled" }, }, "ID", "Name", 1);
            ViewBag.MyAction = itemAction.ToList();
            ViewBag.SelectedAction = "0";

            if (centerHeadLoginSession.SchoolAllows != null && schl!= "")
            {
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                Search = " a.class='" + cls + "' ";

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
                TempData["CCE_SeniorSearch"] = Search;
                // string class1 = "4";
                MS.StoreAllData = _schoolRepository.GetMarksEntryDataBySCHL(Search, schl, pageIndex, cls, SelAction);
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
                    }
                    if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                    {
                        MS.schoolAllotedToCenterMaster = new SchoolAllotedToCenterMaster()
                        {
                            IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                            LastDate = Convert.ToDateTime(MS.StoreAllData.Tables[5].Rows[0]["LastDate"].ToString()),
                            SCHLNME = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["schlnme"].ToString()),
                            Schl = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["SCHL"].ToString())
                        };
                    }
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
                    }
                    if (MS.StoreAllData.Tables[5].Rows.Count > 0)
                    {
                        MS.schoolAllotedToCenterMaster = new SchoolAllotedToCenterMaster()
                        {
                            IsActive = Convert.ToInt32(MS.StoreAllData.Tables[5].Rows[0]["IsActive"].ToString()),
                            LastDate = Convert.ToDateTime(MS.StoreAllData.Tables[5].Rows[0]["LastDate"].ToString()),
                            SCHLNME = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["schlnme"].ToString()),
                            Schl = Convert.ToString(MS.StoreAllData.Tables[5].Rows[0]["SCHL"].ToString())
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

                }
            }

            return View(MS);
        }




        [HttpPost]
        public JsonResult JqMarksEntry(string stdid, string CandSubject)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];

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

                    if ((obt < 1) || (obt > max))
                    {
                        flag = -2;
                    }
                }
            }
            if (flag == 1)
            {
                string dee = "1";
                string class1 = "4";
                int OutStatus = 0;



                dee = _schoolRepository.AllotMarksEntry(centerHeadLoginSession.UserName,stdid, dtSub, class1, out OutStatus);

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

        [CenterHeadLoginCheckFilter]
        [CenterHeadAfterLoginFilter]
        public ActionResult MarksRoughReport(string id, string schl)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];            
            SchoolModels MS = new SchoolModels();
            try
            {
                string OutError = "0";
                string cls = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = schl;
                ViewBag.cid = id;
                string Search = string.Empty;
                Search = "  a.schl = '" + schl + "' and  a.class= '" + cls + "'";
                MS.StoreAllData = _schoolRepository.MarksEntryReport(centerHeadLoginSession.CenterHeadId.ToString(), 0, Search, schl, cls, out OutError);
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

        [CenterHeadLoginCheckFilter]
        [CenterHeadAfterLoginFilter]
        public ActionResult MarksFinalReport(string id, string schl, FormCollection frm)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string cls = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = schl;
                ViewBag.cid = id;               

            
                string Search = string.Empty;
                Search = "  a.schl = '" + schl + "' and  a.class= '" + cls + "'";
                DataSet dsFinal = new DataSet();
                string OutError = "0";
                MS.StoreAllData = dsFinal = _schoolRepository.MarksEntryReport(centerHeadLoginSession.CenterHeadId.ToString(), 1,Search, schl, cls, out OutError);//final
                if (MS.StoreAllData == null)
                {
                    ViewBag.IsAllowMarks = 0;
                    ViewBag.IsFinal = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(MS);
                }
                else if (MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.IsAllowMarks = 1;
                    MS.StoreAllData = _schoolRepository.MarksEntryReport(centerHeadLoginSession.CenterHeadId.ToString(), 0, Search, schl, cls, out OutError);//rough
                    ViewBag.TotalCount = 0;
                    ViewBag.TotalCount1 = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.MarksFilledDate = MS.StoreAllData.Tables[0].Rows[0]["MarksFilledDate"].ToString();
                    ViewBag.SchoolName = MS.StoreAllData.Tables[1].Rows[0]["FullSchoolNameE"].ToString();
                    ViewBag.SET = MS.StoreAllData.Tables[1].Rows[0]["FIFSET"].ToString();
                    ViewBag.FinalSubmitLastDate = MS.StoreAllData.Tables[0].Rows[0]["FinalSubmitLastDate"];

                    if (dsFinal.Tables[2].Rows.Count > 0)
                    {
                        int totalFinalPending = Convert.ToInt32(dsFinal.Tables[2].Rows[0]["TotalPending"]);
                        if (totalFinalPending == 0)
                        {
                            ViewBag.IsFinal = 0;
                        }
                        else { ViewBag.IsFinal = 1; }
                    }

                    
                    return View(MS);
                }

                else
                {
                    ViewBag.IsAllowMarks = 2;
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


        
        [CenterHeadLoginCheckFilter]
        [CenterHeadAfterLoginFilter]
        [HttpPost]
        public ActionResult MarksFinalReport(string id, string schl)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            SchoolModels MS = new SchoolModels();
            try
            {
                string cls = id == "Primary" ? "5" : id == "Middle" ? "8" : "";
                ViewBag.schlCode = schl;
                ViewBag.cid = id;


                string Search = string.Empty;
                Search = "  a.schl = '" + schl + "' and  a.class= '" + cls + "'";
                DataSet dsFinal = new DataSet();
                //type= 2 for FinalSubmit
                string OutError = "0";
                MS.StoreAllData = dsFinal = _schoolRepository.MarksEntryReport(centerHeadLoginSession.CenterHeadId.ToString(), 2, Search, schl, cls, out  OutError);//type= 2 for FinalSubmit
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
                    //  return RedirectToAction("CCEFinalReport", "School", new { id= id});
                }
            }
            catch (Exception ex)
            {

            }
            return View(MS);

        }

        #endregion  Marks Entry Panel For Primary Class 


        #region GenerateTicket
        
        public JsonResult BindGenerateTicketList(int ticketId)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];

            List<GenerateTicketModel> _model = new List<GenerateTicketModel>();
            if (ticketId == 0)
            {
                _model = _centerheadrepository.GetGenerateTicketData(0,centerHeadLoginSession.UserName, 0, "");
                return Json(new { data = _model.ToList() }, JsonRequestBehavior.AllowGet);
            }
            else if (ticketId > 0)
            {
                _model = _centerheadrepository.GetGenerateTicketData(1, centerHeadLoginSession.UserName, ticketId, "");
            }
            return Json(_model.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteGenerateTicket(int TicketId)
        {
            int OutStatus = 0;
            string result = _centerheadrepository.ListingGenerateTicket(0, TicketId, out OutStatus);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [CenterHeadLoginCheckFilter]
        public ActionResult GenerateTicket(GenerateTicketModel gtm)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            DataSet ds = _centerheadrepository.CenterHeadMaster(3, centerHeadLoginSession.CenterHeadId, "");
            gtm.complaintTypeMasterModels = ds.Tables[0].AsEnumerable().Select(dataRow => new ComplaintTypeMasterModel
            {
                ComplaintTypeId = dataRow.Field<int>("ComplaintTypeId"),
                ComplaintTypeName = dataRow.Field<string>("ComplaintTypeName"),
            }).ToList();

          
            string FilepathExist = Path.Combine(Server.MapPath("~/Upload/" + "/GenerateTicket"));
            if (!Directory.Exists(FilepathExist))
            {
                Directory.CreateDirectory(FilepathExist);
            }

            return View(gtm);
        }

        [CenterHeadLoginCheckFilter]
        [HttpPost]
        public ActionResult GenerateTicket(FormCollection frm)
        {
            CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)Session["CenterHeadLoginSession"];
            string statusOut = "";
            try
            {
                var Jsonobject = frm["modelData"];
                var actionType = frm["actionType"];
                GenerateTicketModel oModel = JsonConvert.DeserializeObject<GenerateTicketModel>(Jsonobject);                
                string outTicketNo = "";

                if (actionType.ToLower() == "save")
                {                    //
                    oModel.ActionType = 0;
                    oModel.IsActive = 1;
                    oModel.CreatedBy = centerHeadLoginSession.CenterHeadId;
                    oModel.CenterHeadUserName = centerHeadLoginSession.UserName;                   
                }
                else if (actionType.ToLower() == "update")
                {
                    oModel.ActionType = 1;
                    oModel.IsActive = 1;
                    oModel.CreatedBy = centerHeadLoginSession.CenterHeadId;
                    oModel.CenterHeadUserName = centerHeadLoginSession.UserName;
                }

                //
                int result = _centerheadrepository.InsertGenerateTicket(oModel, out outTicketNo);//InsertFeeMaster2016SP
                ViewBag.outTicketNo = outTicketNo;
                if (result > 0)
                {
                    statusOut = "1";
                    if (Request.Files.Count > 0)
                    {
                        HttpFileCollectionBase files = Request.Files;
                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFileBase file = files[i];
                            string fileExt = Path.GetExtension(file.FileName).ToLower();
                            string fname = outTicketNo + fileExt;
                            var path = Path.Combine(Server.MapPath("~/Upload/" + "GenerateTicket"), fname);
                            oModel.Filepath = "Upload/GenerateTicket/" + fname;
                            file.SaveAs(path);
                        }
                        oModel.ActionType = 2;
                        oModel.TicketId = result;
                        if (oModel.TicketId == 0)
                        { ViewData["result"] = 1; oModel.TicketId = result; }


                        int resultup = _centerheadrepository.InsertGenerateTicket(oModel, out outTicketNo);
                    }

                }
                else { statusOut = result.ToString(); }
                var results = new
                {
                    status = statusOut,
                    ticketno = ViewBag.outTicketNo
                };
                return Json(results);
            }
            catch (Exception ex)
            {
                var results = new
                {
                    status = "-1",
                    ticketno = ex.Message
                };
                return Json(results);
            }

            
         
        }


      
        public ActionResult PrintTicket(string id)
        {
           
            GenerateTicketModel generateTicketModel = new GenerateTicketModel();
            List<GenerateTicketModel> _model = _centerheadrepository.GetGenerateTicketData(2, "", 0, id);// id as ticket number
            if (_model.Count > 0)
            {
                generateTicketModel = _model.Where(s => s.TicketNumber == id.ToString()).FirstOrDefault<GenerateTicketModel>();
            }
            return View(generateTicketModel);
        }

        #endregion GenerateTicket
    }
}