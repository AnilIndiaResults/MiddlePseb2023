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
    public class AdminController : Controller
    {

        private readonly ISchoolRepository _schoolRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly ICenterHeadRepository _centerheadrepository;

        public AdminController(ICenterHeadRepository centerheadrepository, ISchoolRepository schoolRepository, IAdminRepository adminRepository)
        {
            _centerheadrepository = centerheadrepository;
            _schoolRepository = schoolRepository;
            _adminRepository = adminRepository;
        }

        // GET: Admin
        // GET: Admin
        #region Admin Login 
        [Route("Admin")]
        [Route("Admin/login")]
        public ActionResult Index()
        {
            if (TempData["result"] != null)
            {
                ViewData["result"] = TempData["result"];
            }
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            FormsAuthentication.SignOut();
            Session.Clear();
            TempData.Clear();
            Session.Abandon();
            Session.RemoveAll();
            try
            {
                ViewBag.SessionList = DBClass.GetSession().ToList();
                return View();
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [HttpPost]
        [Route("Admin")]
        [Route("Admin/login")]
        public ActionResult Index(LoginModel lm)
        {
            try
            {
                AdminLoginSession adminLoginSession = _adminRepository.CheckAdminLogin(lm);
                //AdminLoginSession adminLoginSession = _schoolRepository.CheckLogin(lm); // passing Value to _schoolRepository.from model and Type 1 For regular              

                adminLoginSession.CurrentSession = lm.Session;
                TempData["result"] = adminLoginSession.LoginStatus;

                if (adminLoginSession.LoginStatus == 1)
                {
                    Session["AdminLoginSession"] = adminLoginSession;
                    return RedirectToAction("Welcome", "Admin");
                }
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["result"] = "ERR";
                ViewData["resultMsg"] = ex.Message;
                return View();
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
            return RedirectToAction("Index", "Admin");

        }

        #endregion Admin Login 

        [AdminLoginCheckFilter]
        public ActionResult PageNotAuthorized()
        {           
            return View();
        }


        [AdminLoginCheckFilter]
        public ActionResult Welcome()
        {            
            return View();
        }


        #region  Change_Password

        [AdminLoginCheckFilter]
        public ActionResult Change_Password()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"]; 
            ViewBag.User = adminLoginSession.USERNAME.ToString();
            return View();
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult Change_Password(FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            ViewBag.User = adminLoginSession.USERNAME.ToString();

            string CurrentPassword = string.Empty;
            string NewPassword = string.Empty;

            if (frm["ConfirmPassword"] != "" && frm["NewPassword"] != "")
            {
                if (frm["ConfirmPassword"].ToString() == frm["NewPassword"].ToString())
                {
                    CurrentPassword = frm["CurrentPassword"].ToString();
                    NewPassword = frm["NewPassword"].ToString();
                    int result = _adminRepository.ChangePassword(Convert.ToInt32(adminLoginSession.AdminId.ToString()), CurrentPassword, NewPassword);
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

        #region Menu Master   

        public MultiSelectList MenuSessionList(string sel)
        {
            var SessionList = DBClass.GetSessionAdmin().ToList().Take(1).Select(c => new
            {
                Text = c.Text,
                Value = c.Value.Substring(0, 4)
            }).OrderByDescending(s => s.Value).ToList();
            if (sel == "")
            { return new MultiSelectList(SessionList, "Value", "Text"); }
            else
            {
                int[] myArray1 = StaticDB.StringToIntArray(sel, ',');
                return new MultiSelectList(SessionList, "Value", "Text", myArray1);
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult MenuMaster(int id = 0)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            List<SelectListItem> menuList = new List<SelectListItem>();
            ViewBag.Id = id;
            ViewBag.SelMenu = menuList;
            ViewBag.SelectedSelMenu = 0;
            SiteMenu oModel = new SiteMenu();
            var itemParent = new SelectList(new[] { new { ID = "1", Name = "Menu" }, new { ID = "2", Name = "SubMenu" }, new { ID = "3", Name = "Action" }, }, "ID", "Name", 1);
            ViewBag.Parent = itemParent.ToList();

            if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
            {   
                ViewBag.MySession = MenuSessionList("");  

                if (id == 0)
                {
                    DataSet result = _adminRepository.GetAllMenu(0);
                    if (result == null)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (result.Tables[0].Rows.Count > 0)
                    {
                        oModel.StoreAllData = result;
                        ViewBag.TotalCount = oModel.StoreAllData.Tables[0].Rows.Count;
                    }
                }
                else
                {
                    int outstatus = 0;
                    DataSet result = _adminRepository.ListingMenuJunior(1, id, out outstatus);
                    if (result == null)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (result.Tables[0].Rows.Count > 0)
                    {
                        oModel.StoreAllData = result;
                        ViewBag.TotalCount = oModel.StoreAllData.Tables[0].Rows.Count;
                        oModel.MenuName = oModel.StoreAllData.Tables[0].Rows[0]["MenuName"].ToString();
                        oModel.MenuUrl = oModel.StoreAllData.Tables[0].Rows[0]["MenuUrl"].ToString();
                        ViewBag.SelectedParent = oModel.StoreAllData.Tables[0].Rows[0]["SelRole"].ToString();
                        ViewBag.SelectedSelMenu = oModel.StoreAllData.Tables[0].Rows[0]["ParentMenuId"].ToString();
                        ViewBag.MySession = MenuSessionList(oModel.StoreAllData.Tables[0].Rows[0]["AssignYear"].ToString());

                        if (ViewBag.SelectedParent == "1")
                        { }
                        else
                        {
                            DataSet ds = _adminRepository.GetAllMenu(Convert.ToInt32(oModel.StoreAllData.Tables[0].Rows[0]["SelRole"].ToString()));
                            ViewBag.SelMenu = ds.Tables[0];                          
                            foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
                            {
                                if (dr["MenuId"].ToString() == oModel.StoreAllData.Tables[0].Rows[0]["ParentMenuId"].ToString())
                                {
                                    menuList.Add(new SelectListItem { Text = @dr["MenuName"].ToString(), Value = @dr["MenuId"].ToString(), Selected = true });
                                }
                                else
                                {
                                    menuList.Add(new SelectListItem { Text = @dr["MenuName"].ToString(), Value = @dr["MenuId"].ToString() });
                                }
                            }
                            ViewBag.SelMenu = menuList;                          
                        }
                    }
                }
                return View(oModel);
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }

        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult MenuMaster(SiteMenu oModel, FormCollection frm, int id = 0)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            ViewBag.MySession = MenuSessionList("");
            //Parent
            var itemParent = new SelectList(new[] { new { ID = "1", Name = "Menu" }, new { ID = "2", Name = "SubMenu" }, new { ID = "3", Name = "Action" }, }, "ID", "Name", 1);
            ViewBag.Parent = itemParent.ToList();

            string SelectedSession = "";        
            ViewBag.Id = id;
            List<SelectListItem> menuList = new List<SelectListItem>();
            ViewBag.SelMenu = menuList;
            ViewBag.SelectedSelMenu = 0;
            
            if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
            {

                if (frm["SelectedSession"] == "" || frm["SelectedSession"] == null)
                {
                    ViewData["Result"] = 20;
                    return View(oModel);
                }
                else
                { SelectedSession = frm["SelectedSession"].ToString(); } 
                if (id == 0)
                {

                    int IsParent = 0, ParentMenuId = 0, IsMenu = 1;
                    if (frm["Parent"] != "")
                    {
                        IsParent = Convert.ToInt32(frm["Parent"]);
                        if (IsParent == 1)
                        { IsParent = 0; IsMenu = 1; }
                        else if (IsParent == 2)
                        { IsMenu = 1; }
                        else if (IsParent == 3)
                        { IsMenu = 0; }
                    }
                    if (frm["SelMenu"] != "")
                    { ParentMenuId = Convert.ToInt32(frm["SelMenu"]); }

                    if (IsParent != 0 && ParentMenuId == 0)
                    {
                        ViewData["Result"] = -1;
                    }
                    else
                    {
                        // string result = "";
                        string result = _adminRepository.CreateMenuJunior(oModel, IsParent, ParentMenuId, IsMenu, SelectedSession);
                        if (Convert.ToInt32(result) > 0)
                        {
                            ViewData["Result"] = 1;
                        }
                        else
                        {
                            ViewData["Result"] = 0;
                        }
                    }
                }
                else
                {
                    oModel.MenuID = id;
                    int ParentMenuId = 0;
                    if (frm["SelMenu"] != "")
                    { ParentMenuId = Convert.ToInt32(frm["SelMenu"]); }
                    int OutStatus = 0;
                    DataSet result = _adminRepository.UpdateMenuJunior(oModel, ParentMenuId, out OutStatus, SelectedSession); //UpdateMenu(SiteMenu model, int ParentMenuId, out int OutStatus)
                    if (OutStatus > 0)
                    {
                        ViewData["Result"] = 11;
                    }
                    else
                    {
                        ViewData["Result"] = 10;
                    }
                }

                DataSet ds = _adminRepository.GetAllMenu(0);
                if (ds == null)
                {
                    //return RedirectToAction("Index", "Admin");
                }
                else if (ds.Tables[0].Rows.Count > 0)
                {
                    oModel.StoreAllData = ds;
                    ViewBag.TotalCount = oModel.StoreAllData.Tables[0].Rows.Count;
                }

                return View(oModel);
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }

        }


        public JsonResult GetMenu(int Parent) // Calling on http post (on Submit)
        {
            DataSet ds = _adminRepository.GetAllMenu(Parent);           
            List<SelectListItem> menuList = new List<SelectListItem>();
            menuList.Add(new SelectListItem { Text = "---Select---", Value = "0" });
            var eList = ds.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
            {
                Text = dataRow.Field<string>("MenuName"),
                Value = dataRow.Field<int>("MenuID").ToString()
                
            }).ToList();
            menuList = eList.ToList();           
            return Json(menuList);
        }

        public ActionResult UpdateMenuStatus(int id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                {                   
                    int outstatus = 0;
                    DataSet result = _adminRepository.ListingMenuJunior(0, id, out outstatus);
                    return RedirectToAction("MenuMaster", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("MenuMaster", "Admin");
            }
        }





        #endregion Menu Master    

        #region Admin User Master

        public MultiSelectList GetAllPSEBCLASSList(string sel)
        {
            var SessionList = DBClass.GetAllPSEBCLASS().ToList().Select(c => new
            {
                Text = c.Text,
                Value = c.Value
            }).OrderBy(s => s.Value).ToList();
            if (sel == "")
            { return new MultiSelectList(SessionList, "Value", "Text"); }
            else
            {
                int[] myArray1 = StaticDB.StringToIntArray(sel, ',');
                return new MultiSelectList(SessionList, "Value", "Text", myArray1);
            }
        }


        [AdminLoginCheckFilter]
        public ActionResult CreateUser(AdminUserModel aum)
        {           
             AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            if (adminLoginSession.AdminType.ToUpper() == "ADMIN")
            {               
                //GetAllPSEBCLASS
                ViewBag.GetAllPSEBCLASS = GetAllPSEBCLASSList("");

                //GetAll District                
                DataSet ds = DBClass.GetAllDistrict();
                var _DistList = ds.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("DISTNM").ToString(),
                    Value = dataRow.Field<string>("DIST").ToString(),
                }).ToList();
                aum.DistList = _DistList.ToList();               

                // End
                //Branch
                var _branchList = ds.Tables[2].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("BranchName").ToString(),
                    Value = dataRow.Field<string>("BranchName").ToString(),
                }).ToList();
                aum.BranchList = _branchList.ToList();              



                // All Pages List    
                var menuList = ds.Tables[1].AsEnumerable().Select(dataRow => new SiteMenu
                {
                    MenuID = dataRow.Field<int>("MenuID"),
                    MenuName = dataRow.Field<string>("MenuName"),
                    MenuUrl = dataRow.Field<string>("MenuUrl"),
                    ParentMenuID = dataRow.Field<int>("ParentMenuID"),
                    IsMenu = dataRow.Field<int>("IsMenu"),
                }).ToList();               
                aum.SiteMenuModel = menuList;


                //GetAll Set
                var _SetList = ds.Tables[4].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("AdminSet").ToString(),
                    Value = dataRow.Field<string>("AdminSet").ToString(),
                }).ToList();
                aum.SetList = _SetList;
               
                return View(aum);
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }


        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult JqCreateUser(string User, string distid, string pageid, string setid)
        {
            string status = "";
            if (User == null || User == "")
            {
                var results = new
                {
                    status = ""
                };
                return Json(results);
            }
            else
            {
                var Jsonobject = JsonConvert.DeserializeObject<AdminUserModel>(User);
                AdminUserModel oModel = JsonConvert.DeserializeObject<AdminUserModel>(User);
                oModel.id = 0;
                oModel.Dist_Allow = distid;
                oModel.PAccessRight = pageid;
                if (setid != "")
                {
                    oModel.Set_Allow = setid;
                }

                oModel.pass = Guid.NewGuid().ToString().Substring(0, 6); // Autogenerated password
                if (oModel.listOfActionRight.Count > 0)
                {
                    oModel.ActionRight = string.Join(",", oModel.listOfActionRight);
                }
                int OutStatus = 0;
                status = _adminRepository.CreateAdminUser(oModel, out OutStatus);
                if (OutStatus == 1)
                {
                    string Sms = "Admin User Details, User Id: " + oModel.user + " and Password: " + oModel.pass + " for login. Regards PSEB";
                    if (oModel.Mobno != "")
                    {
                        string getSms = DBClass.gosms(oModel.Mobno, Sms);
                    }
                    if (oModel.EmailID != "")
                    {
                        string body = "<table width=" + 600 + " cellpadding=" + 4 + " cellspacing=" + 4 + " border=" + 0 + "><tr><td><b>Dear " + oModel.User_fullnm + "</b>,</td></tr><tr><td><b>Your Admin User Details are given Below:-</b><br /><b>User Id :</b> " + oModel.user + "<br /><b>Password :</b> " + oModel.pass + "<br /></td></tr><tr><td height=" + 30 + "><b>Click Here To Login</b> <a href=https://middleprimary.pseb.ac.in/Admin target = _blank>www.registration.pseb.ac.in/Admin</a></td></tr><tr><td><b>Note:</b> Please Read Instruction Carefully Before filling the Online Form .</td></tr><tr><td>This is a system generated e-mail and please do not reply. Add <a target=_blank href=mailto:psebhelpdesk@gmail.com>psebhelpdesk@gmail.com</a> to your white list / safe sender list. Else, your mailbox filter or ISP (Internet Service Provider) may stop you from receiving e-mails.</td></tr><tr><td><b><i>Regards</b><i>,<br /> Tech Team, <br />Punjab School Education Board<br /></td></tr>";
                        // bool result = DBClass.mail("PSEB - Admin User Details", body, "rohit.nanda@ethical.in");
                        bool result = DBClass.mail("PSEB - Admin User Details", body, oModel.EmailID);
                    }
                }

                var results = new
                {
                    status = OutStatus,
                };
                return Json(results);
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult ViewUser(AdminUserModel aum)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];           
            string Search = string.Empty;
            Search = " id like '%%' ";
            DataSet result = _adminRepository.GetAllAdminUser(0, Search);
            if (result == null)
            {
                return RedirectToAction("Index", "Admin");
            }
            if (result.Tables[0].Rows.Count > 0)
            {
                aum.StoreAllData = result;
                ViewBag.TotalCount = aum.StoreAllData.Tables[0].Rows.Count;
            }

            if (result.Tables[1].Rows.Count > 0)
            {               
                var _branchList = result.Tables[1].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("BranchName").ToString(),
                    Value = dataRow.Field<string>("BranchName").ToString(),
                }).ToList();                
                aum.BranchList = _branchList.ToList();
            }

            //ViewBag.Branch = new SelectList(result.Tables[1].Rows, "BranchName", "BranchName", 0);

            ViewBag.SelectedBranch = "";
            ViewBag.SearchUserId = "";
            ViewBag.SearchMobile = "";
            #region Action Assign Method
            if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
            { ViewBag.IsModiFy = 1; ViewBag.IsDelete = 1; ViewBag.IsView = 1; ViewBag.IsModiFyOpen = 1; }
            else
            {

                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                
                string AdminType = adminLoginSession.AdminType.ToString();
                //GetActionOfSubMenu(string cont, string act)
                DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                if (aAct.Tables[0].Rows.Count > 0)
                {
                    bool exists = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFY")).Count() > 0;
                    ViewBag.IsModiFy = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFY")).Count();
                    ViewBag.IsDelete = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("DELETE")).Count();
                    ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("VIEW")).Count();
                    ViewBag.IsModiFyOpen = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFYOPEN")).Count();


                }
            }
            #endregion Action Assign Method          
            return View(aum);
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewUser(AdminUserModel aum,FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            ViewBag.SelectedBranch = "";
            ViewBag.SearchUserId = "";
            ViewBag.SearchMobile = "";

            if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
            {
                string Search = string.Empty;
                Search = " id like '%%' ";
              
                if (frm["Branch"] != "")
                {
                    ViewBag.SelectedBranch = frm["Branch"];
                    Search += " and Branch='" + frm["Branch"].ToString().Trim() + "'";
                }
                if (frm["SearchUserId"] != "")
                {
                    ViewBag.SearchUserId = frm["SearchUserId"];
                    Search += " and  [user] like '%" + frm["SearchUserId"].ToString().Trim() + "%'";
                }
                if (frm["SearchMobile"] != "")
                {
                    ViewBag.SearchMobile = frm["SearchMobile"];
                    Search += " and Mobno='" + frm["SearchMobile"].ToString().Trim() + "'";
                }


                DataSet result = _adminRepository.GetAllAdminUser(0, Search);
                if (result == null)
                {
                    return RedirectToAction("Index", "Admin");
                }
                if (result.Tables[0].Rows.Count > 0)
                {
                    aum.StoreAllData = result;
                    ViewBag.TotalCount = aum.StoreAllData.Tables[0].Rows.Count;
                }
               
                if (result.Tables[1].Rows.Count > 0)
                {
                    var _branchList = result.Tables[1].AsEnumerable().Select(dataRow => new SelectListItem
                    {
                        Text = dataRow.Field<string>("BranchName").ToString(),
                        Value = dataRow.Field<string>("BranchName").ToString(),
                    }).ToList();
                    aum.BranchList = _branchList.ToList();
                   
                }

                #region Action Assign Method
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                { ViewBag.IsModiFy = 1; ViewBag.IsDelete = 1; ViewBag.IsView = 1; ViewBag.IsModiFyOpen = 1; }
                else
                {

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();                    
                  
                    DataSet aAct = DBClass.GetActionOfSubMenu(adminLoginSession.AdminId, controllerName, actionName);
                    if (aAct.Tables[0].Rows.Count > 0)
                    {
                        bool exists = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFY")).Count() > 0;
                        ViewBag.IsModiFy = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFY")).Count();
                        ViewBag.IsDelete = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("DELETE")).Count();
                        ViewBag.IsView = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("VIEW")).Count();
                        ViewBag.IsModiFyOpen = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuName").Equals("MODIFYOPEN")).Count();


                    }
                }
                #endregion Action Assign Method          
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            return View(aum);
        }


        public JsonResult JqSendPassword(string storeid)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            string dee = "1";
            storeid = storeid.Remove(storeid.Length - 1);
            string[] split1 = storeid.Split(',');
            int sCount = split1.Length;
            if (sCount > 0)
            {
                int i = 0;
                foreach (string s in split1)
                {
                    i++;
                    string userid = s.ToString();
                    if (userid != "")
                    {
                        // dee = _adminRepository.ErrorAllotRegno(stdid, errid, Convert.ToInt32(Action), userid);
                        DataSet result = DBClass.GetAdminDetailsById(Convert.ToInt32(userid), Convert.ToInt32(adminLoginSession.CurrentSession.ToString().Substring(0, 4)));
                        if (result.Tables[0].Rows.Count > 0)
                        {
                            dee = "1";
                            string Mobile = result.Tables[0].Rows[0]["Mobno"].ToString();
                            string Password = result.Tables[0].Rows[0]["pass"].ToString();
                            string user = result.Tables[0].Rows[0]["user"].ToString();
                            string Sms = "Admin User Details, User Id: " + user + " and Password: " + Password + " for login. Regards PSEB";
                            if (Mobile != "")
                            {
                                string getSms = DBClass.gosms(Mobile, Sms);
                            }
                        }
                    }
                }
            }

            return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult JqComposeSms(string storeid, string SMS)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            string dee = "1";
            storeid = storeid.Remove(storeid.Length - 1);
            string[] split1 = storeid.Split(',');
            int sCount = split1.Length;
            if (sCount > 0)
            {
                int i = 0;
                foreach (string s in split1)
                {
                    i++;
                    string userid = s.ToString();
                    if (userid != "")
                    {
                        // dee = _adminRepository.ErrorAllotRegno(stdid, errid, Convert.ToInt32(Action), userid);
                        DataSet result = DBClass.GetAdminDetailsById(Convert.ToInt32(userid), Convert.ToInt32(adminLoginSession.CurrentSession.ToString().Substring(0, 4)));
                        if (result.Tables[0].Rows.Count > 0)
                        {
                            dee = "1";
                            string Mobile = result.Tables[0].Rows[0]["Mobno"].ToString();
                            string Sms = SMS + " Regards PSEB";
                            if (Mobile != "" && SMS != "")
                            {
                                string getSms = DBClass.gosms(Mobile, Sms);
                            }
                        }
                    }
                }
            }

            return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
        }

        [AdminLoginCheckFilter]
        public ActionResult UpdateUserStatus(int id)
        {
            try
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                {                 
                    int outstatus = 0;
                    string result = _adminRepository.ListingUser(1, id, out outstatus);
                    return RedirectToAction("ViewUser", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("ViewUser", "Admin");
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult ModifyUser(int id, AdminUserModel aum)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            if (id == 0 || id.ToString() == null)
            { return RedirectToAction("Index", "Admin"); }

            ViewBag.userid = id.ToString();           
            DataSet result = _adminRepository.GetAllAdminUser(id, "");          
            if (result.Tables[0].Rows.Count > 0)
            {
                aum.user = result.Tables[0].Rows[0]["user"].ToString();
                aum.pass = result.Tables[0].Rows[0]["pass"].ToString();
                aum.Usertype = result.Tables[0].Rows[0]["Usertype"].ToString();
                aum.User_fullnm = result.Tables[0].Rows[0]["User_fullnm"].ToString();
                ViewBag.SelectedBranch = aum.Branch = result.Tables[0].Rows[0]["Branch"].ToString();
                aum.Designation = result.Tables[0].Rows[0]["Designation"].ToString();
                aum.EmailID = result.Tables[0].Rows[0]["EmailID"].ToString();
                aum.Mobno = result.Tables[0].Rows[0]["Mobno"].ToString();
                aum.Remarks = result.Tables[0].Rows[0]["Remarks"].ToString();
                aum.STATUS = Convert.ToInt32(result.Tables[0].Rows[0]["STATUS"].ToString().ToUpper() == "ACTIVE" ? 1 : 0);
                ViewBag.SelectedStatus = aum.STATUS;
                ViewBag.DistUser = result.Tables[0].Rows[0]["Dist_Allow"].ToString();
                ViewBag.MenuUser = result.Tables[0].Rows[0]["PAccessRight"].ToString();
                //Set
                ViewBag.Set_Allow = result.Tables[0].Rows[0]["Set_Allow"].ToString();

                if (result.Tables[0].Rows[0]["ActionRight"].ToString() == "")
                { ViewBag.GetAllPSEBCLASS = GetAllPSEBCLASSList(""); }
                else
                {
                    ViewBag.GetAllPSEBCLASS = GetAllPSEBCLASSList(result.Tables[0].Rows[0]["ActionRight"].ToString());
                }
            }



            //GetAll District
            List<SelectListItem> DistList = new List<SelectListItem>();
            DataSet ds = DBClass.GetAllDistrict();
            foreach (System.Data.DataRow dr in ds.Tables[0].Rows)
            {
                if (StaticDB.ContainsValue(ViewBag.DistUser, @dr["DIST"].ToString()))
                {
                    DistList.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString(), Selected = true });
                }
                else
                { DistList.Add(new SelectListItem { Text = @dr["DISTNM"].ToString(), Value = @dr["DIST"].ToString() }); }
            }
            aum.DistList = DistList;


           
            //Branch
            var _branchList = ds.Tables[2].AsEnumerable().Select(dataRow => new SelectListItem
            {
                Text = dataRow.Field<string>("BranchName").ToString(),
                Value = dataRow.Field<string>("BranchName").ToString(),
            }).ToList();
            aum.BranchList = _branchList.ToList();

            // All Pages List               
            List<SiteMenu> all = new List<SiteMenu>();
            foreach (System.Data.DataRow dr in ds.Tables[1].Rows)
            {
                if (StaticDB.ContainsValue(ViewBag.MenuUser, @dr["MenuID"].ToString()))
                {
                    // all.Add(new SiteMenu { MenuID = Convert.ToInt32(@dr["MenuID"]), MenuName = @dr["MenuName"].ToString(), MenuUrl = @dr["MenuUrl"].ToString(), ParentMenuID = Convert.ToInt32(@dr["ParentMenuID"]), IsSelected = true });
                    all.Add(new SiteMenu { MenuID = Convert.ToInt32(@dr["MenuID"]), MenuName = @dr["MenuName"].ToString(), MenuUrl = @dr["MenuUrl"].ToString(), ParentMenuID = Convert.ToInt32(@dr["ParentMenuID"]), IsMenu = Convert.ToInt32(@dr["IsMenu"]), IsSelected = true });
                }
                else
                {
                    //   all.Add(new SiteMenu { MenuID = Convert.ToInt32(@dr["MenuID"]), MenuName = @dr["MenuName"].ToString(), MenuUrl = @dr["MenuUrl"].ToString(), ParentMenuID = Convert.ToInt32(@dr["ParentMenuID"]) });
                    all.Add(new SiteMenu { MenuID = Convert.ToInt32(@dr["MenuID"]), MenuName = @dr["MenuName"].ToString(), MenuUrl = @dr["MenuUrl"].ToString(), ParentMenuID = Convert.ToInt32(@dr["ParentMenuID"]), IsMenu = Convert.ToInt32(@dr["IsMenu"]) });

                }

            }
            aum.SiteMenuModel = all;

            //GetAll Set
            List<SelectListItem> SetList = new List<SelectListItem>();
            foreach (System.Data.DataRow dr in ds.Tables[4].Rows)
            {
                if (StaticDB.ContainsValue(ViewBag.Set_Allow, @dr["AdminSet"].ToString()))
                {
                    SetList.Add(new SelectListItem { Text = @dr["AdminSet"].ToString(), Value = @dr["AdminSet"].ToString(), Selected = true });
                }
                else
                { SetList.Add(new SelectListItem { Text = @dr["AdminSet"].ToString(), Value = @dr["AdminSet"].ToString() }); }
            }
            aum.SetList = SetList;

            return View(aum);

        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult JqModifyUser(string User, string distid, string pageid, string setid)
        {
            string status = "";
            if (User == null || User == "")
            {
                var results = new
                {
                    status = ""
                };
                return Json(results);
            }
            else
            {
                var Jsonobject = JsonConvert.DeserializeObject<AdminUserModel>(User);
                AdminUserModel oModel = JsonConvert.DeserializeObject<AdminUserModel>(User);
                // oModel.id = 0;
                oModel.Dist_Allow = distid;
                oModel.PAccessRight = pageid;

                if (setid != "")
                {
                    oModel.Set_Allow = setid;
                }
                if (oModel.user.ToString().ToUpper() == "ADMIN" || oModel.id == 1)
                {
                    oModel.Usertype = "ADMIN";
                }

                if (oModel.listOfActionRight.Count > 0)
                {
                    oModel.ActionRight = string.Join(",", oModel.listOfActionRight);
                }
                //  oModel.Usertype = "";
                int OutStatus = 0;
                status = _adminRepository.CreateAdminUser(oModel, out OutStatus);
                var results = new
                {
                    status = OutStatus,
                };
                return Json(results);
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult DeleteUser(string id)
        {
            try
            {
                AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
                if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
                {                   
                    int outstatus = 0;
                    string result = _adminRepository.ListingUser(0, Convert.ToInt32(id), out outstatus);
                    return RedirectToAction("ViewUser", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("ViewUser", "Admin");
            }
        }

        [AdminLoginCheckFilter]
        public ActionResult AssignMenuToUser(AdminUserModel aum)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            if (adminLoginSession.AdminType.ToString().ToUpper() == "ADMIN")
            {                
                //GetAll District
                List<SelectListItem> AdminList = new List<SelectListItem>();
                DataSet ds = DBClass.GetAllDistrict();

                var _AdminList = ds.Tables[3].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("AdminName").ToString(),
                    Value = dataRow.Field<int>("id").ToString(),
                }).ToList();
                aum.AdminList = _AdminList.ToList();


                var itemMenu = ds.Tables[1].AsEnumerable().Select(dataRow => new SelectListItem
                {
                    Text = dataRow.Field<string>("SubMenuText").ToString(),
                    Value = dataRow.Field<int>("MenuID").ToString(),
                }).Where(s => s.Text!="").ToList();
                aum.MenuList = itemMenu.ToList();

                return View(aum);
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }

        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult AssignMenuToUser(string adminlist, string pagelist)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            string status = "";
            if (adminlist == null || adminlist == "")
            {
                var results = new
                {
                    status = ""
                };
                return Json(results);
            }
            else if (pagelist == null || pagelist == "")
            {
                var results = new
                {
                    status = ""
                };
                return Json(results);
            }
            else
            {
                string OutError = "0";
                status = _adminRepository.AssignMenuToUser(adminLoginSession.AdminId.ToString(), adminlist, pagelist, out OutError);
                var results = new
                {
                    status = OutError,
                };
                return Json(results);
            }
        }


        public JsonResult GetUserbyMenu(int Menu) // Calling on http post (on Submit)
        {
            string Usertd = string.Empty;
            DataSet result = _adminRepository.GetAdminIdWithMenuId(Menu); // passing Value to DBClass from model

            if (result.Tables[0].Rows.Count > 0)
            {
                List<SelectListItem> _UserList = new List<SelectListItem>();
                var _List = result.Tables[0].AsEnumerable().Select(dataRow => new SelectListItem
                {                   
                    Text = dataRow.Field<string>("User").ToString(),
                    Value = dataRow.Field<int>("Id").ToString(),
                }).ToList();

                ViewBag.MyUser = _List.ToList();


                if (result.Tables[1].Rows.Count > 0)
                {
                    Usertd = result.Tables[1].Rows[0]["UserList"].ToString();
                }

                var results = new
                {
                    UserList = _UserList,
                    Usertd = Usertd,
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    UserList = "",
                    Usertd = "",
                };
                return Json(results);
            }

        }


        #endregion Admin User Master

        #region Registration Menu Panel

        [AdminLoginCheckFilter]
        public ActionResult FinalSubmittedRecordsAllAdmin(RegistrationAllStudentAdminModelList registrationAllStudentAdminModelList)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];            
            return View(registrationAllStudentAdminModelList);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public async Task<ActionResult> FinalSubmittedRecordsAllAdmin(RegistrationAllStudentAdminModelList registrationAllStudentAdminModelList, string schl)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];        
            if (string.IsNullOrEmpty(schl))
            {
                return View(registrationAllStudentAdminModelList);
            }
            DataSet dsOut = new DataSet();
            registrationAllStudentAdminModelList.RegistrationAllStudentAdminModel = await RegistrationDB.GetAllFinalSubmittedStudentBySchlPM("ALL", schl, out dsOut);
            return View(registrationAllStudentAdminModelList);
        }

      
        public ActionResult CommonFormView(string id, string formname)
        {
            RegistrationModels rm = new RegistrationModels();           
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("FinalSubmittedRecordsAllAdmin", "Admin");
            }
            ViewBag.FormName = formname;
            if (id != null)
            {
                id = encrypt.QueryStringModule.Decrypt(id);
                string search = "";
                DataSet ds = new DataSet();
                rm = RegistrationDB.RegDataModalForAllClassSP(2, id, formname, search, out ds);
                if (rm == null)
                {
                    return RedirectToAction("FinalSubmittedRecordsAllAdmin", "Admin");
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
                                    rm.s9 = ds.Tables[1].Rows[8]["SUBNM"].ToString();
                                }
                            }

                        }
                    }
                    #endregion

                }


            }
            return View(rm);
        }

        #endregion Registration Menu Panel


        #region Circular
        [AdminLoginCheckFilter]
        public ActionResult Circular(int? ID, CircularModels fm, int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {                
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                ViewBag.Id = ID == null ? 0 : ID;
                string Search = string.Empty;
                Search = "Id like '%' ";

                DataSet ds1 = _adminRepository.CircularTypeMaster();
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    List<CircularTypeMaster> ci = new List<CircularTypeMaster>();
                    ci = (from DataRow row in ds1.Tables[0].Rows

                          select new CircularTypeMaster
                          {
                              Id = Convert.ToInt32(row["Id"]),
                              CircularType = row["CircularType"].ToString(),
                              IsSelected = false
                          }).ToList();

                    fm.CircularTypeMasterList = ci;
                }
                else { fm.CircularTypeMasterList = null; }


                if (ID > 0)
                {
                    Search += " and Id=" + ID;
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;

                    DataSet ds = _adminRepository.CircularMaster(Search, pageIndex);//GetAllFeeMaster2016SP
                    fm.StoreAllData = ds;
                    if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        if (ID > 0)
                        {
                            fm.ID = Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString());
                            fm.CircularNo = ds.Tables[0].Rows[0]["CircularNo"].ToString();
                            fm.Session = ds.Tables[0].Rows[0]["Session"].ToString();
                            fm.Title = ds.Tables[0].Rows[0]["Title"].ToString();
                            fm.Attachment = ds.Tables[0].Rows[0]["Attachment"].ToString();
                            fm.UrlLink = ds.Tables[0].Rows[0]["UrlLink"].ToString();
                            fm.UploadDate = ds.Tables[0].Rows[0]["UploadDate"].ToString();
                            fm.ExpiryDate = ds.Tables[0].Rows[0]["ExpiryDate"].ToString();
                            fm.Category = ds.Tables[0].Rows[0]["Category"].ToString();
                            fm.IsMarque = Convert.ToInt32(ds.Tables[0].Rows[0]["IsMarque"].ToString());
                            fm.IsActive = Convert.ToInt32(ds.Tables[0].Rows[0]["IsActive"].ToString().ToLower() == "true" ? "1" : "0");
                            fm.UploadDate = ds.Tables[0].Rows[0]["UploadDate"].ToString();
                            fm.SelectedCircularTypes = ds.Tables[0].Rows[0]["CircularTypes"].ToString();
                            if (!string.IsNullOrEmpty(fm.SelectedCircularTypes))
                            {
                                List<CircularTypeMaster> ci = new List<CircularTypeMaster>();
                                foreach (System.Data.DataRow dr in ds1.Tables[0].Rows)
                                {
                                    if (StaticDB.ContainsValue(fm.SelectedCircularTypes, @dr["Id"].ToString()))
                                    {
                                        ci.Add(new CircularTypeMaster { CircularType = @dr["CircularType"].ToString(), Id = Convert.ToInt32(@dr["Id"]), IsSelected = true });
                                    }
                                    else
                                    { ci.Add(new CircularTypeMaster { CircularType = @dr["CircularType"].ToString(), Id = Convert.ToInt32(@dr["Id"]) }); }
                                }
                                fm.CircularTypeMasterList = ci;
                            }
                        }
                        ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.TotalCircularCount = fm.StoreAllData.Tables[1].Rows[0]["TotalCount"].ToString();
                    }
                }
                else
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(fm);
                }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult Circular(CircularModels fm, FormCollection frm, string cmd, string ChkId)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

           
            int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
            ViewBag.Id = fm.ID == null ? 0 : fm.ID;
            //Check server side validation using data annotation

            if (ModelState.IsValid)
            {
                if (fm.ID == null)
                {
                    fm.ID = 0;
                    if (fm.file == null && string.IsNullOrEmpty(fm.UrlLink))
                    {
                        ViewData["result"] = 20;
                        return View(fm);
                    }
                }
                else
                {
                    if (fm.ID > 0 && fm.file == null && !string.IsNullOrEmpty(fm.Attachment) && !string.IsNullOrEmpty(fm.UrlLink))
                    {
                        fm.Attachment = "";
                    }
                    else if (fm.ID > 0 && fm.file != null && !string.IsNullOrEmpty(fm.Attachment) && !string.IsNullOrEmpty(fm.UrlLink))
                    {
                        ViewData["result"] = 20;
                        return View(fm);
                    }
                }
                string fileExt, fileName = "";
                string outCircularNo = "";
                fm.CreatedBy = fm.UpdatedBy = AdminId;
                fm.IsMarque = (fm.Category.ToLower() == "marquee" ? 1 : 0);
                fm.Type = 0;
                fm.Session = adminLoginSession.CurrentSession.ToString();
                fm.SelectedCircularTypes = string.Join(",", fm.CircularTypeMasterList.Where(s => s.IsSelected == true).Select(s => s.Id.ToString()));




                int result = _adminRepository.InsertCircularMaster(fm, out outCircularNo);//InsertFeeMaster2016SP
                ViewBag.outCircularNo = outCircularNo;
                if (result > 0)
                {
                    if (fm.ID == 0) { ViewData["result"] = 1; fm.ID = result; }
                    else { ViewData["result"] = 2;}

                    if (fm.file != null)
                    {
                        fileExt = Path.GetExtension(fm.file.FileName).ToLower();
                        fileName = outCircularNo + fileExt;

                        var path = Path.Combine(Server.MapPath("~/Upload/" + "Circular"), fileName);
                        string FilepathExist = Path.Combine(Server.MapPath("~/Upload/" + "/Circular"));
                        if (!Directory.Exists(FilepathExist))
                        {
                            Directory.CreateDirectory(FilepathExist);
                        }
                        fm.file.SaveAs(path);
                        fm.Attachment = "Upload/Circular/" + fileName;
                        fm.CircularNo = outCircularNo;
                        fm.Type = 1;
                        int result2 = _adminRepository.InsertCircularMaster(fm, out outCircularNo);
                        if (result > 0)
                        {
                            ModelState.Clear();
                        }
                    }
                }
                else if (result == -1)
                {
                    //-----alredy exist
                    ViewData["result"] = -1;
                }
                else
                {
                    //Not Saved                 
                    ViewData["result"] = 0;
                }

            }


            DataSet ds1 = _adminRepository.CircularTypeMaster();
            if (ds1.Tables[0].Rows.Count > 0)
            {
                List<CircularTypeMaster> ci = new List<CircularTypeMaster>();
                ci = (from DataRow row in ds1.Tables[0].Rows

                      select new CircularTypeMaster
                      {
                          Id = Convert.ToInt32(row["Id"]),
                          CircularType = row["CircularType"].ToString(),
                          IsSelected = true
                      }).ToList();

                fm.CircularTypeMasterList = ci;
            }
            else { fm.CircularTypeMasterList = null; }
            return View(fm);
        }


        [AdminLoginCheckFilter]
        public ActionResult ViewCircular(int? ID, CircularModels fm, int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {
               
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());

                DataSet ds1 = _adminRepository.CircularTypeMaster();
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    List<CircularTypeMaster> ci = new List<CircularTypeMaster>();
                    ci = (from DataRow row in ds1.Tables[0].Rows

                          select new CircularTypeMaster
                          {
                              Id = Convert.ToInt32(row["Id"]),
                              CircularType = row["CircularType"].ToString(),
                              IsSelected = true
                          }).ToList();

                    fm.CircularTypeMasterList = ViewBag.CircularTypeMasterList = ci;

                }
                else { fm.CircularTypeMasterList = null; }

                string Search = string.Empty;
                Search = "Id like '%' ";
                if (ID > 0)
                {
                    Search += " and Id=" + ID;
                }
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;

                DataSet ds = _adminRepository.CircularMaster(Search, pageIndex);//GetAllFeeMaster2016SP
                fm.StoreAllData = ds;
                if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                    //
                    int count = Convert.ToInt32(fm.StoreAllData.Tables[1].Rows[0]["TotalCount"]);
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
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewCircular(int? ID, CircularModels fm, int? page, string SearchString, string FromDate, string ToDate)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {
                
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;


                string Search = string.Empty;
                Search = "Id like '%' ";
                if (!string.IsNullOrEmpty(SearchString))
                {

                    Search += " and Title like '%" + SearchString.ToString() + "%'";
                    ViewBag.SearchString = SearchString;
                    TempData["SearchString"] = SearchString;
                }
                if (FromDate != "")
                {
                    ViewBag.FromDate = FromDate;
                    TempData["FromDate"] = FromDate;
                    Search += " and  CONVERT(DATETIME, DATEDIFF(DAY, 0, UploadDate)) >=convert(DATETIME,'" + FromDate.ToString() + "',103) ";
                }
                if (ToDate != "")
                {
                    ViewBag.ToDate = ToDate;
                    TempData["ToDate"] = ToDate;
                    Search += "   and CONVERT(DATETIME, DATEDIFF(DAY, 0, UploadDate)) <=  convert(DATETIME,'" + ToDate.ToString() + "',103)";
                }

                DataSet ds = _adminRepository.CircularMaster(Search, pageIndex);//GetAllFeeMaster2016SP
                fm.StoreAllData = ds;
                if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                    //
                    int count = Convert.ToInt32(fm.StoreAllData.Tables[1].Rows[0]["TotalCount"]);
                    ViewBag.TotalCircularCount = count;
                    int tp = Convert.ToInt32(count);
                    int pn = tp / 15;
                    int cal = 15 * pn;
                    int res = Convert.ToInt32(ViewBag.TotalCount1) - cal;
                    if (res >= 1)
                    { ViewBag.pn = pn + 1; }
                    else
                    { ViewBag.pn = pn; }

                }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        public ActionResult UpdateCircularStatus(int id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            int outstatus = 0;
            string result = _adminRepository.ListingCircular(1, id, out outstatus);
            return RedirectToAction("ViewCircular", "Admin");
        }
        #endregion


        #region Firm Exam Data Download

        [AdminLoginCheckFilter]
        public ActionResult FirmExamDataDownload(string id, AdminModels am)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            string firmuser = adminLoginSession.USERNAME.ToString();
            string ErrStatus = string.Empty;

            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "Correction Data Only" }, new { ID = "2", Name = "All Updated Data" }, }, "ID", "Name", 1);
            ViewBag.MySchData = itemsch1.ToList();


            //var itemsch = new SelectList(new[]{new {ID="1",Name="Regular"},new {ID="2",Name="Open"},
            //new {ID="3",Name="Pvt"},}, "ID", "Name", 1);
            var itemsch = new SelectList(new[] { new { ID = "1", Name = "Regular" }, }, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();
            ViewBag.SelectedItem = "0";
            am.StoreAllData = _adminRepository.FirmExamDataDownload(Convert.ToInt32(1), "", firmuser, "", out ErrStatus);
            if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
            {
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                ViewBag.TotalCount1 = am.StoreAllData.Tables[1].Rows[0]["Total"].ToString();
            }
            else
            {
                ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                ViewBag.TotalCount1 = am.StoreAllData.Tables[1].Rows[0]["Total"].ToString();
            }

            if (string.IsNullOrEmpty(id))
            {
                // return RedirectToAction("FirmExamDataDownload", "Admin");
            }
            else
            {
                DataSet ds = null;
                int type = 0;
                string fileName1 = string.Empty;
                string Search = string.Empty;
                int OutStatus = 0;
                string RPLot = "";

                if (id.ToLower() == "pending")
                {
                    fileName1 = "Pending_" + firmuser;
                    type = 5;
                    Search = "std_id like '%%' and DataDownloadLot is null";
                }
                else if (id.ToLower() == "pendingpvt")
                {
                    fileName1 = "Pendingpvt_" + firmuser;
                    type = 8;
                    Search = "std_id like '%%' and DOWNLOT is null";
                }
                else
                {
                    string FileExport = id.ToString();
                    string splitFile = FileExport.Split('-')[0];
                    string splitLot = FileExport.Split('-')[1];
                    fileName1 = "LOT" + splitLot;
                    if (splitFile.ToLower().Contains("pvt"))
                    {
                        RPLot = "P";
                        if (splitFile.ToLower().Contains("data"))
                        { type = 2; fileName1 += "_DATA_"; }                       
                        Search = "std_id like '%%'  and DOWNLOT=" + splitLot + "";
                    }
                    else
                    {
                        if (splitFile.ToLower().Contains("data"))
                        { type = 2; fileName1 += "_DATA_"; }
                        else if (splitFile.ToLower().Contains("subject"))
                        { type = 3; fileName1 += "_SUB_"; }             
                        Search = "std_id like '%%'  and DataDownloadLot=" + splitLot + "";
                    }
                    fileName1 += firmuser + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                }

                if (type > 0)
                {
                    ds = _adminRepository.FirmExamDataDownload(Convert.ToInt32(type), RPLot, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                    #region Download Data or Subjects
                    if (type == 2 || type == 3)
                    {
                        if (ds == null)
                        {
                            return RedirectToAction("FirmExamDataDownload", "Admin", new { id = "" });
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
                                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + ".xls");
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
                    #endregion Data
                                     

                    #region Pending
                    else if (type == 5)
                    {
                        if (ds == null)
                        {
                            return View(am);
                        }
                        else
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                ViewData["Result"] = "10";
                                ViewBag.Message = ds.Tables[0].Rows[0]["DataDownloadLot"].ToString();
                            }
                        }
                    }
                    #endregion Pending

                  

                }

            }
            return View(am);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult FirmExamDataDownload(string id, AdminModels am, FormCollection frm, string submit)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            string firmuser = adminLoginSession.USERNAME.ToString();           
            try
            {

                var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "Correction Data Only" }, new { ID = "2", Name = "All Updated Data" }, }, "ID", "Name", 1);
                ViewBag.MySchData = itemsch1.ToList();

                //var itemsch = new SelectList(new[]{new {ID="1",Name="Regular"},new {ID="2",Name="Open"},new {ID="3",Name="Pvt"},}, "ID", "Name", 1);
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Regular" },}, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";
                string ErrStatus = string.Empty;
                string Search = string.Empty;
                DataSet ds1 = new DataSet();            
                am.StoreAllData = _adminRepository.FirmExamDataDownload(Convert.ToInt32(1), "", firmuser, "", out ErrStatus);
                if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = am.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.TotalCount1 = am.StoreAllData.Tables[1].Rows[0]["Total"].ToString();
                }

                string Filevalue = string.Empty;
                if (frm["Filevalue"] == null) { }
                else
                {
                    Filevalue = frm["Filevalue"].ToString();
                }

                //  Download Data by file
                string AdminType = adminLoginSession.AdminType.ToString();
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                string fileLocation = "";
                string filename = "";
                if (Filevalue.ToUpper() == "STDIDMIS")
                {
                    if (am.file != null)
                    {
                        filename = Path.GetFileName(am.file.FileName);

                        DataSet ds = new DataSet();
                        if (am.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                        {
                            string fileName1 = "FirmExam_" + firmuser + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                            string fileExtension = System.IO.Path.GetExtension(am.file.FileName);
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
                                am.file.SaveAs(fileLocation);
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

                                DataTable dtexport;
                                string RP1 = "";
                                if (frm["SelList"] != "")
                                {
                                    RP1 = frm["SelList"] == "1" ? "R" : frm["SelList"] == "2" ? "O" : frm["SelList"] == "3" ? "P" : "";
                                }
                                string CheckMis = _adminRepository.CheckFirmExamDataDownloadMis(ds, out dtexport, RP1);
                                if (CheckMis == "")
                                {
                                    DataTable dt1 = ds.Tables[0];
                                    if (dt1.Columns.Contains("Status"))
                                    {
                                        dt1.Columns.Remove("Status");
                                    }


                                    // string Result1 = "";
                                    int OutStatus = 0;
                                    // int REGNOLOT = 0;
                                    string commaString = "";
                                    for (int i = 0; i < dt1.Rows.Count; i++)
                                    {
                                        commaString = commaString + dt1.Rows[i]["Std_id"].ToString();
                                        //commaString += (i < dt1.Rows.Count) ? "," : string.Empty;
                                        commaString += (i < dt1.Rows.Count - 1) ? "," : string.Empty;
                                    }

                                    // Download
                                    #region DownloadFile

                                    if (submit.ToUpper().Contains("DOWNLOAD"))
                                    {
                                        string result = string.Empty;
                                        string RP = string.Empty;
                                        if (frm["SelList"] != "")
                                        {
                                            ViewBag.SelectedItem = frm["SelList"];
                                            TempData["SelectedItem"] = frm["SelList"];
                                            RP = frm["SelList"] == "1" ? "R" : frm["SelList"] == "2" ? "O" : frm["SelList"] == "3" ? "P" : "";
                                        }
                                        Search = "std_id like '%%' and std_id in (" + commaString + ")";

                                        if (submit.ToLower().Contains("data"))
                                        {
                                            ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(2), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                                        }
                                        else if (submit.ToLower().Contains("subjects"))
                                        {
                                            ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(6), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                                        }
                                        // DataSet ds1 = objDB.DownloadRegNoAgainstID(commaString, "O", out OutStatus); // For all Regno alloted 
                                        fileName1 = submit + "_" + firmuser + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                                        if (ds1.Tables[0].Rows.Count > 0)
                                        {
                                            ViewBag.Message = "Exam Data Downloaded Successfully";
                                            ViewData["Result"] = "1";
                                            ViewBag.TotalCount12 = ds1.Tables[0].Rows.Count;                                          
                                            
                                                //DataTable dt = ds1.Tables[0];
                                                using (XLWorkbook wb = new XLWorkbook())
                                                {
                                                    wb.Worksheets.Add(ds1.Tables[0]);
                                                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    wb.Style.Font.Bold = true;
                                                    Response.Clear();
                                                    Response.Buffer = true;
                                                    Response.Charset = "";
                                                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + ".xls");
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
                                    #endregion DownloadFile                          
                                    return View(am);
                                }
                                else
                                {
                                    if (dtexport != null)
                                    {
                                        ExportDataFromDataTable(dtexport, "Error_FirmDataDownload");
                                    }
                                    ViewData["Result"] = "-1";
                                    ViewBag.Message = CheckMis;
                                    return View(am);
                                }
                            }
                            else
                            {

                                ViewData["Result"] = "-2";
                                ViewBag.Message = "Please Upload Only .xls file only";
                                return View(am);
                            }
                        }

                    }
                    else
                    {
                        //ViewData["Result"] = "-4";
                        // ViewBag.Message = "Please select .xls file only";
                        //return View();
                    }
                }
                else if (Filevalue.ToUpper() == "DATE")
                {
                    #region DownloadFile

                    if (submit.ToUpper().Contains("DOWNLOAD"))
                    {
                        string type1 = string.Empty;
                        string RP = string.Empty;
                        Search = "a.std_id like '%%' ";

                        if (frm["SelData"] != "")
                        {
                            ViewBag.SelectedItemData = type1 = frm["SelData"];
                            TempData["SelectedItemData"] = frm["SelData"];
                        }


                        if (frm["SelList"] != "")
                        {
                            ViewBag.SelectedItem = frm["SelList"];
                            TempData["SelectedItem"] = frm["SelList"];
                            RP = frm["SelList"] == "1" ? "R" : frm["SelList"] == "2" ? "O" : frm["SelList"] == "3" ? "P" : "";
                        }
                        // updt between convert(date,'21/07/2017',103) and convert(date,'01/08/2017',103)
                        if (frm["FromDate"] != "")
                        {
                            ViewBag.FromDate = frm["FromDate"];
                            TempData["FromDate"] = frm["FromDate"];
                            if (frm["SelList"] == "3")
                            { Search += " and  CONVERT(DATETIME, DATEDIFF(DAY, 0, UPDT)) >=convert(DATETIME,'" + frm["FromDate"].ToString() + "',103) "; }
                            else
                            {
                                if (type1 == "1")
                                {
                                    Search += " and  CONVERT(DATETIME, DATEDIFF(DAY, 0, FirmCorrectionLotDT)) >=convert(DATETIME,'" + frm["FromDate"].ToString() + "',103) ";
                                }
                                else
                                { Search += " and  CONVERT(DATETIME, DATEDIFF(DAY, 0, UPDT)) >=convert(DATETIME,'" + frm["FromDate"].ToString() + "',103) "; }

                            }
                        }
                        if (frm["ToDate"] != "")
                        {
                            ViewBag.ToDate = frm["ToDate"];
                            TempData["ToDate"] = frm["ToDate"];
                            // Search += " and  convert(date,'" + frm["ToDate"].ToString() + "',103)";
                            //Search += " and  format(UPDT,'dd/MM/yyyy') <='" + frm["ToDate"].ToString() + "'";
                            if (frm["SelList"] == "3")
                            {
                                Search += "   and CONVERT(DATETIME, DATEDIFF(DAY, 0,UPDT)) <=  convert(DATETIME,'" + frm["ToDate"].ToString() + "',103)";
                            }
                            else
                            {

                                if (type1 == "1")
                                {
                                    Search += "   and CONVERT(DATETIME, DATEDIFF(DAY, 0,FirmCorrectionLotDT)) <=  convert(DATETIME,'" + frm["ToDate"].ToString() + "',103)";

                                }
                                else
                                { Search += "   and CONVERT(DATETIME, DATEDIFF(DAY, 0,UPDT)) <=  convert(DATETIME,'" + frm["ToDate"].ToString() + "',103)"; }

                            }
                        }

                        if (type1 == "1")
                        {
                            if (submit.ToLower().Contains("data"))
                            {
                                ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(12), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                            }
                            else if (submit.ToLower().Contains("subjects"))
                            {
                                ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(13), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                            }                          
                        }
                        else
                        {
                            if (submit.ToLower().Contains("data"))
                            {
                                ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(2), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                            }
                            else if (submit.ToLower().Contains("subjects"))
                            {
                                ds1 = _adminRepository.FirmExamDataDownload(Convert.ToInt32(3), RP, firmuser, Search, out ErrStatus); // FirmExamDataDownloadSPNew
                            }                           
                        }
                        // DataSet ds1 = objDB.DownloadRegNoAgainstID(commaString, "O", out OutStatus); // For all Regno alloted 
                        string fileName1 = submit + "_" + firmuser + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                        if (ds1 == null)
                        {
                            ViewBag.Message = ErrStatus.ToString();
                            ViewData["Result"] = "5";
                            ViewBag.TotalCount13 = 0;
                        }
                        else if (ds1.Tables[0].Rows.Count == 0)
                        {
                            ViewData["Result"] = "15";
                            ViewBag.TotalCount13 = 0;
                        }
                        else if (ds1.Tables[0].Rows.Count > 0)
                        {
                            ViewBag.Message = "Exam Data Downloaded Successfully";
                            ViewData["Result"] = "1";
                            ViewBag.TotalCount13 = ds1.Tables[0].Rows.Count;
                            if (submit.ToLower().Contains("photo"))
                            {
                                if (ds1 == null)
                                {
                                    return RedirectToAction("FirmExamDataDownload", "Admin", new { id = "" });
                                }                              
                            }
                            else
                            {

                                //DataTable dt = ds1.Tables[0];
                                using (XLWorkbook wb = new XLWorkbook())
                                {
                                    wb.Worksheets.Add(ds1.Tables[0]);
                                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    wb.Style.Font.Bold = true;
                                    Response.Clear();
                                    Response.Buffer = true;
                                    Response.Charset = "";
                                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName1 + ".xls");
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

                    }
                    #endregion DownloadFile                          
                }
                return View(am);
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewBag.Message = "Error: " + ex.Message;
                ViewData["Result"] = "50";
                return View();
            }
        }

        #endregion Firm Exam Data Download


        #region Begin AllotRegNo

        [AdminLoginCheckFilter]       
        public ActionResult AdminAllotRegNo()
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

                    //string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    //string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    //int AdminId = Convert.ToInt32(adminLoginSession.AdminId);
                    //DataSet aAct = DBClass.GetActionOfSubMenu(AdminId, controllerName, actionName);
                    //if (aAct.Tables[0].Rows.Count > 0)
                    //{
                    //    ViewBag.IsREG = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("ADMIN/ALLOTREGNO")).Count();
                    //}
                }
                #endregion Action Assign Method
                //ViewBag.AdminType = Session["AdminType"].ToString();
                //AbstractLayer._adminRepository.objDB = new AbstractLayer._adminRepository.);
                //AdminModels MS = new AdminModels();                //
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
        public ActionResult AdminAllotRegNo(FormCollection frm, string SelDist)
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
                    //string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    //string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    //int AdminId = Convert.ToInt32(Session["AdminId"]);
                    //DataSet aAct = DBClass.GetActionOfSubMenu(AdminId, controllerName, actionName);
                    //if (aAct.Tables[0].Rows.Count > 0)
                    //{
                    //    ViewBag.IsREG = aAct.Tables[0].AsEnumerable().Where(c => c.Field<string>("MenuUrl").ToUpper().Equals("Admin/AllotRegNo")).Count();
                    //}
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
                    
                    MS.StoreAllData = _schoolRepository.SchoolMasterViewSP(4,"", Search);
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


        [AdminLoginCheckFilter]
        public ActionResult AllotRegNo(FormCollection frm, string id, int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];            
            AdminModels MS = new AdminModels();
            string SCHL = string.Empty;
            try
            {

                SCHL = id;
                DataSet result1 = _adminRepository.GetAllFormNameBySchl(id);
                ViewBag.MyForm = result1.Tables[1];
                ViewBag.AllErrorList = result1.Tables[2];
                /******* By Rohit Error List */
                List<SelectListItem> ErrorList5 = new List<SelectListItem>();
                List<SelectListItem> ErrorList8 = new List<SelectListItem>();               
                foreach (System.Data.DataRow dr in result1.Tables[2].Rows)
                {
                    if (dr["FORM"].ToString() == "F2")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "A2")
                    {
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }                 
                    if (dr["FORM"].ToString() == "AL")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                      
                    }
                }
                ViewBag.ErrorList5 = ErrorList5;
                ViewBag.ErrorList8 = ErrorList8;              
                /*******/

                List<SelectListItem> FormList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
                {
                    FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
                }
                ViewBag.MyForm = FormList;
                
                //////
                //DataSet result2 = _adminRepository.GetAllLot(id);
                ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;


                var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
             new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Allot Descrepancy" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();


                //------------------------

                string schlid = string.Empty;

                if (id != null)
                {
                    int pageIndex = 1;
                    pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                    ViewBag.pagesize = pageIndex;

                    string Search = string.Empty;
                    Search = "";

                    //

                    if (TempData["AllotRegnoSearch"] != null)
                    {
                        Search += TempData["AllotRegnoSearch"].ToString();
                        ViewBag.SelectedFilter = TempData["SelFilter"];
                        ViewBag.SelectedAction = TempData["SelAction"];
                        ViewBag.SelectedForm = TempData["SelForm"];
                        ViewBag.SelectedLot = TempData["SelLot"];

                    }
                    else
                    {
                        ViewBag.SelectedFilter = 0;
                        ViewBag.SelectedAction = 0;
                        ViewBag.SelectedForm = 0;
                        ViewBag.SelectedLot = 0;

                    }
                   
                    MS.StoreAllData = _adminRepository.GetStudentRegNoNotAlloted(Search, SCHL, pageIndex);

                    ViewBag.schlCode = MS.StoreAllData.Tables[2].Rows[0]["schlCode"].ToString();
                    ViewBag.schlID = MS.StoreAllData.Tables[2].Rows[0]["schlID"].ToString();
                    ViewBag.schlName = MS.StoreAllData.Tables[2].Rows[0]["schlNM"].ToString();

                    if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
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
            }
            catch (Exception ex)
            {               
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult AllotRegNo(FormCollection frm, int? page, string SchlCode,string SearchString, string SelAction, string SelForm, string SelLot,string SelFilter)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels MS = new AdminModels();
            string id = string.Empty;
            id = SchlCode.ToString();
            //string id = string.Empty;          
            //string schlid = string.Empty;
            //string SCHL = string.Empty;
            try
            {            
              
                DataSet result1 = _adminRepository.GetAllFormNameBySchl(id);
                ViewBag.MyForm = result1.Tables[1];
                /******* By Rohit Error List */
                List<SelectListItem> ErrorList5 = new List<SelectListItem>();
                List<SelectListItem> ErrorList8 = new List<SelectListItem>();               
                foreach (System.Data.DataRow dr in result1.Tables[2].Rows)
                {
                    if (dr["FORM"].ToString() == "F2")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "A2")
                    {
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }                  
                    if (dr["FORM"].ToString() == "AL")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });   
                    }
                }
                ViewBag.ErrorList5 = ErrorList5;
                ViewBag.ErrorList8 = ErrorList8;
                          // MS.ErrorList = ErrorList5;
                /*******/

                List<SelectListItem> FormList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
                {
                    FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
                }
                ViewBag.MyForm = FormList;

                //MS.SelForm = frm["SelForm"].ToString();
                //MS.SelLot = frm["SelLot"].ToString();
                //ViewBag.SelLot = frm["SelLot"].ToString();
                //MS.SelFilter = frm["SelFilter"].ToString();

                //DataSet result2 = _adminRepository.GetAllLot(id);
                ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;
            }
            catch (Exception ex)
            {
                Session["Search"] = null;
                return RedirectToAction("Index", "Admin");
            }



            //var itemFilter = new SelectList(new[] { new { ID = "1", Name = "Student Name" }, new { ID = "2", Name = "Roll No" }, }, "ID", "Name", 1);
            var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();

            var itemAction = new SelectList(new[] { new { ID = "1", Name = "Allot Descrepancy" }, }, "ID", "Name", 1);
            ViewBag.MyAction = itemAction.ToList();
            ////------------------------
            TempData["SelFilter"] = ViewBag.SelectedFilter = "0";
            TempData["SelAction"] = ViewBag.SelectedAction = "0";
            TempData["SelForm"] = ViewBag.SelectedForm = "0";
            TempData["SelLot"] = ViewBag.SelectedLot = "0";
         
            if (id != null && id != "")
            {
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                string Search = string.Empty;
                Search = "SCHL='" + id + "'";
                if (frm["SelAction"] != "")
                {
                    TempData["SelAction"] = ViewBag.SelectedAction = SelAction;            
                    int SelValueSch = Convert.ToInt32(SelAction.ToString());
                    if (frm["SelAction"] != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and  Registration_num!='' "; }
                        else
                        { Search += "  and (Registration_num is null or Registration_num='') "; }
                    }
                }
                else
                {

                    Search += "  and  (Registration_num is null or Registration_num='')";
                }

                if (frm["SelForm"] != "")
                {
                    TempData["SelForm"] = ViewBag.SelectedForm = SelForm;                 
                    Search += " and form_name='" + SelForm.ToString() + "' ";
                }
                if (frm["SelLot"] != "")
                {
                    TempData["SelLot"] = ViewBag.SelectedLot = SelLot;                 
                    Search += " and LOT='" + SelLot.ToString() + "' ";
                }
                else
                {
                    ViewBag.SelectedLot = "0";
                    Search += " and LOT >0 ";
                }
                if (frm["SelFilter"] != "")
                {
                    TempData["SelFilter"] = ViewBag.SelectedFilter = SelFilter;
                    int SelValueSch = Convert.ToInt32(SelFilter.ToString());
                    if (SelFilter != "" && SearchString.ToString() != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and Std_id='" + SearchString.ToString() + "'"; }
                        else if (SelValueSch == 2)
                        { Search += " and  Registration_num like '%" + SearchString.ToString() + "%'"; }
                        else if (SelValueSch == 3)
                        { Search += " and  Candi_Name like '%" + SearchString.ToString() + "%'"; }
                        else if (SelValueSch == 4)
                        { Search += " and  Father_Name  like '%" + SearchString.ToString() + "%'"; }
                        else if (SelValueSch == 5)
                        { Search += " and Mother_Name like '%" + SearchString.ToString() + "%'"; }
                        else if (SelValueSch == 6)
                        { Search += " and DOB='" + SearchString.ToString() + "'"; }
                    }
                }

                // Session["AllotRegnoSearch"] = Search;
                TempData["AllotRegnoSearch"] = Search;

                //[GetStudentRegNoNotAllotedSP]
                MS.StoreAllData = _adminRepository.GetStudentRegNoNotAlloted(Search, SchlCode, pageIndex);//GetStudentRegNoNotAllotedSPPaging
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.TotalCount = 0;
                    ViewBag.schlCode = id;
                    ViewBag.schlID = id;
                    ViewBag.schlName = MS.StoreAllData.Tables[2].Rows[0]["schlNM"].ToString();
                    //ViewBag.schlName = 0;
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount1 = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.schlCode = MS.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                    ViewBag.schlID = MS.StoreAllData.Tables[0].Rows[0]["IDNO"].ToString();
                    ViewBag.schlName = MS.StoreAllData.Tables[2].Rows[0]["schlNM"].ToString();

                    // ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
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
            return View();
        }



        [AdminLoginCheckFilter]
        public JsonResult JqAllotRegNo(string storeid, string Action)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            //455651(1,2,),103559(9,19,),

            string dee = "1";
            //std(id1,id2),
            storeid = storeid.Remove(storeid.Length - 1);
            //string[] split1 = storeid.Split('^');
            string[] split1 = storeid.Split('$');
            int sCount = split1.Length;
            if (sCount > 0)
            {
                foreach (string s in split1)
                {
                    //180000500()^(test)$180231608()^(rohit)$
                    string[] s2 = s.Split('^');
                    string stdid = s2[0].Split('(')[0];
                    string errid = s2[0].Split('(', ')')[1];
                    string remarks = s2[1].Split('(', ')')[1];

                    //string stdid = s.Split('(')[0];
                    //string errid = s.Split('(', ')')[1];                   
                    if (stdid != "")
                    {
                        dee = _adminRepository.ErrorAllotRegno(stdid, errid, Convert.ToInt32(Action), adminLoginSession.AdminId, remarks);//ErrorAllotRegnoSP
                    }
                }
            }

            return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult JqAllotManualRegNo(string stdid, string regno, string remarks)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            int OutStatus = 0;
            DataTable dt = _adminRepository.ManualAllotRegno(stdid, regno, out OutStatus, adminLoginSession.AdminId, remarks);
            var results = new
            {
                status = OutStatus
            };
            return Json(results);
        }

        #endregion End AllotRegNo


        #region Begin ViewAllotRegNo
        [AdminLoginCheckFilter]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ViewAllotRegNo(FormCollection frm, string id)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels MS = new AdminModels();
            string SCHL = string.Empty;
            try
            {

                SCHL = id;
                DataSet result1 = _adminRepository.GetAllFormNameBySchl(id);
                ViewBag.MyForm = result1.Tables[1];
                ViewBag.AllErrorList = result1.Tables[2];
                /******* By Rohit Error List */
                List<SelectListItem> ErrorList5 = new List<SelectListItem>();
                List<SelectListItem> ErrorList8 = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in result1.Tables[2].Rows)
                {
                    if (dr["FORM"].ToString() == "F2")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "A2")
                    {
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "AL")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });

                    }
                }
                ViewBag.ErrorList5 = ErrorList5;
                ViewBag.ErrorList8 = ErrorList8;
                /*******/

                List<SelectListItem> FormList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
                {
                    FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
                }
                ViewBag.MyForm = FormList;

                //////
                //DataSet result2 = _adminRepository.GetAllLot(id);
                ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;


                var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
             new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
                ViewBag.MyFilter = itemFilter.ToList();

                var itemAction = new SelectList(new[] { new { ID = "1", Name = "Error List" }, new { ID = "2", Name = "Descrepancy List" }, }, "ID", "Name", 1);
                ViewBag.MyAction = itemAction.ToList();

                ViewBag.SelectedFilter = "0";
                ViewBag.SelectedAction = "0";
                ViewBag.SelectedForm = "0";
                ViewBag.SelectedLot = "0";
                //------------------------

                string schlid = string.Empty;

                if (id != null)
                {
                    string Search = string.Empty;
                    Search = "";
                    MS.StoreAllData = _adminRepository.ViewAllotRegNo(Search, SCHL);

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
                        // ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(MS);
                    }
                }
            }
            catch (Exception ex)
            {
                Session["Search"] = null;
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }


        [AdminLoginCheckFilter]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ViewAllotRegNo(FormCollection frm, string SchlCode, string SearchString, string SelAction, string SelForm, string SelLot, string SelFilter)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels MS = new AdminModels();
            string id = string.Empty;
            id = SchlCode.ToString();
            //string id = string.Empty;          
            //string schlid = string.Empty;
            //string SCHL = string.Empty;
            try
            {

                DataSet result1 = _adminRepository.GetAllFormNameBySchl(id);
                ViewBag.MyForm = result1.Tables[1];
                /******* By Rohit Error List */
                List<SelectListItem> ErrorList5 = new List<SelectListItem>();
                List<SelectListItem> ErrorList8 = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in result1.Tables[2].Rows)
                {
                    if (dr["FORM"].ToString() == "F2")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "A2")
                    {
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                    if (dr["FORM"].ToString() == "AL")
                    {
                        ErrorList5.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                        ErrorList8.Add(new SelectListItem { Text = @dr["Error_Code"].ToString(), Value = @dr["Error_desc"].ToString() });
                    }
                }
                ViewBag.ErrorList5 = ErrorList5;
                ViewBag.ErrorList8 = ErrorList8;
                // MS.ErrorList = ErrorList5;
                /*******/

                List<SelectListItem> FormList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyForm.Rows)
                {
                    FormList.Add(new SelectListItem { Text = @dr["form_name"].ToString(), Value = @dr["form_name"].ToString() });
                }
                ViewBag.MyForm = FormList;

                //MS.SelForm = frm["SelForm"].ToString();
                //MS.SelLot = frm["SelLot"].ToString();
                //ViewBag.SelLot = frm["SelLot"].ToString();
                //MS.SelFilter = frm["SelFilter"].ToString();

                //DataSet result2 = _adminRepository.GetAllLot(id);
                ViewBag.MyLot = result1.Tables[5];
                List<SelectListItem> LotList = new List<SelectListItem>();
                foreach (System.Data.DataRow dr in ViewBag.MyLot.Rows)
                {
                    LotList.Add(new SelectListItem { Text = @dr["LOT"].ToString(), Value = @dr["LOT"].ToString() });
                }
                ViewBag.MyLot = LotList;
            }
            catch (Exception ex)
            {
                Session["Search"] = null;
                return RedirectToAction("Index", "Admin");
            }


            var itemFilter = new SelectList(new[]{new {ID="1",Name="By UniqueID"},new {ID="2",Name="REGNO"},new{ID="3",Name="Candidate Name"},
            new{ID="4",Name="Father's Name"},new{ID="5",Name="Mother's Name"},new{ID="6",Name="DOB"},}, "ID", "Name", 1);
            ViewBag.MyFilter = itemFilter.ToList();

            var itemAction = new SelectList(new[] { new { ID = "1", Name = "Error List" }, new { ID = "2", Name = "Descrepancy List" }, }, "ID", "Name", 1);
            ViewBag.MyAction = itemAction.ToList();

            ViewBag.SelectedFilter = "0";
            ViewBag.SelectedAction = "0";
            ViewBag.SelectedForm = "0";
            ViewBag.SelectedLot = "0";
            ////------------------------

            if (id != null && id != "")
            {
                string Search = string.Empty;
                Search = "a.SCHL='" + id + "'";
                if (frm["SelAction"] != "")
                {
                    ViewBag.SelectedAction = frm["SelAction"];
                    int SelValueSch = Convert.ToInt32(frm["SelAction"].ToString());
                    if (frm["SelAction"] != "")
                    {
                        if (SelValueSch == 1)
                        { Search += " and  Registration_num like 'ERR%'"; }
                        else if (SelValueSch == 2)
                        { Search += " and  Registration_num like '%:ERR%'"; }
                    }
                }
                else { ViewBag.SelectedAction = "0"; }

                if (frm["SelForm"] != "")
                {
                    ViewBag.SelectedForm = frm["SelForm"];
                    Search += " and form_name='" + frm["SelForm"].ToString() + "' ";
                }
                if (frm["SelLot"] != "")
                {
                    ViewBag.SelectedLot = frm["SelLot"];
                    Search += " and LOT='" + frm["SelLot"].ToString() + "' ";
                }
                else
                {
                    Search += " and LOT >0 ";
                }
                if (frm["SelFilter"] != "")
                {
                    ViewBag.SelectedFilter = frm["SelFilter"];
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

                //[GetStudentRegNoNotAllotedSP]
                MS.StoreAllData = _adminRepository.ViewAllotRegNo(Search, SchlCode);//ViewAllotRegNoSP
                if (MS.StoreAllData == null || MS.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.schlCode = id;
                    ViewBag.schlID = id;
                    ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();

                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = MS.StoreAllData.Tables[0].Rows.Count;
                    ViewBag.schlCode = MS.StoreAllData.Tables[0].Rows[0]["SCHL"].ToString();
                    ViewBag.schlID = MS.StoreAllData.Tables[0].Rows[0]["IDNO"].ToString();
                    ViewBag.schlName = MS.StoreAllData.Tables[1].Rows[0]["schlNM"].ToString();
                    return View(MS);
                }
            }
            return View();
        }
        #endregion End AllotRegNo


        [AdminLoginCheckFilter]
        public JsonResult JqRemoveRegNo(string storeid, string Action)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            string dee = "";            
            int userid = Convert.ToInt32(adminLoginSession.AdminId.ToString());
            storeid = storeid.Remove(storeid.Length - 1);
            // string[] split1 = storeid.Split('^');
            string[] split1 = storeid.Split(',');
            int sCount = split1.Length;
            if (sCount > 0)
            {
                foreach (string s in split1)
                {
                    string stdid = s.ToString();
                    //string errid = s.Split('(', ')')[1];
                    if (stdid != "")
                    {
                        dee = _adminRepository.RemoveRegno(stdid, Convert.ToInt32(Action), userid);
                    }
                }
            }
            return Json(new { dee = dee }, JsonRequestBehavior.AllowGet);
        }

        [AdminLoginCheckFilter]
        public ActionResult SendReg(string Schl, string Act)
        {                   
            try
            {
                DataSet ds = new DataSet();
                ds = _schoolRepository.SchoolMasterViewSP(1, Schl, "");
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string SchoolNameWithCode = ds.Tables[0].Rows[0]["SCHLE"].ToString() + "(" + Schl + ")";
                        //  string to = ds.Tables[0].Rows[0]["EMAILID"].ToString();
                        string to = "rohit.nanda@ethical.in";
                        if (Act == "E")
                        {
                            string body = "<table width=" + 600 + " cellpadding=" + 4 + " cellspacing=" + 4 + " border=" + 0 + "><tr><td><b>Dear " + SchoolNameWithCode + "</b>,</td></tr><tr><td height=" + 30 + ">Registration No for Form's (A2/F2) of Schl Code " + Schl + " is Updated on " + DateTime.Now.ToString("dd/MM/yyyy") + "</b>.</td></tr><tr><td height=" + 10 + "></td></tr><tr><td><b>Note:</b> For Detail Kindly Check your School Login Under->Registration Portal->View Registration of A2/F2</td></tr><tr><td></td></tr><tr><td><b><i>Thanks & Regards</b><i>,<br /> Registration Branch, <br />Punjab School Education Board Mohali<br /></td></tr>";
                           


                            string subject = "PSEB-View Registration of A2/F2";
                            bool result = DBClass.mail(subject, body, to);
                            if (result == true)
                            {
                                ViewData["result"] = "1";
                                ViewBag.Message = "Email Sent Successfully....";
                                // ModelState.Clear();
                            }
                            else
                            {
                                ViewData["result"] = "0";
                                ViewBag.Message = "Email Not Sent....";
                            }
                        }
                        else if (Act == "S")
                        {
                            string Mobile = ds.Tables[0].Rows[0]["Mobile"].ToString();
                            string Sms = "Registration No for Form's (A2/F2) of Schl Code " + Schl + " is Updated on " + DateTime.Now.ToString("dd/MM/yyyy") + ".For Detail Kindly Check your School Login Under->Registration Portal->View Registration of A2/F2.";
                            if (Mobile != "0" || Mobile != "")
                            {
                                // string getSms = DBClass.gosms(Mobile, Sms);
                                string getSms = DBClass.gosms("9711819184", Sms);
                                if (getSms != "")
                                {
                                    ViewData["result"] = "1";
                                    ViewBag.Message = "SMS Sent Successfully....";
                                    // ModelState.Clear();
                                }
                                else
                                {
                                    ViewData["result"] = "0";
                                    ViewBag.Message = "SMS Not Sent....";
                                }
                            }
                        }
                        else
                        {
                            ViewData["result"] = "-1";
                            ViewBag.Message = "Invalid Action ...";
                        }

                    }
                    else
                    {
                        ViewData["result"] = "-2";
                        ViewBag.Message = "School Not Found....";
                    }
                }

            }
            catch (Exception)
            {
            }
            return RedirectToAction("ViewAllotRegNo", "Admin", new { id = Schl });
        }


        [AdminLoginCheckFilter]
        public JsonResult jqSendReg(string Schl, string Act)
        {
            string status = "";
            try
            {
                DataSet ds = new DataSet();
                ds = _schoolRepository.SchoolMasterViewSP(1, Schl,"");
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string SchoolNameWithCode = ds.Tables[0].Rows[0]["SCHLE"].ToString() + "(" + Schl + ")";
                        string to = ds.Tables[0].Rows[0]["EMAILID"].ToString();
                        //  string to = "rohit.nanda@ethical.in";
                        if (Act == "E")
                        {
                            if (to != "")
                            {
                                string body = "<table width=" + 600 + " cellpadding=" + 4 + " cellspacing=" + 4 + " border=" + 0 + "><tr><td><b>Dear " + SchoolNameWithCode + "</b>,</td></tr><tr><td height=" + 10 + ">Registration No for Form's (A2/F2) of Schl Code " + Schl + " is Updated on " + DateTime.Now.ToString("dd/MM/yyyy") + "</b>.</td></tr><tr><td height=" + 10 + "></td></tr><tr><td><b>Note:</b> For Detail Kindly Check your School Login Under->Registration Portal->View Registration of A2/F2</td></tr><tr><td></td></tr><tr><td><b><i>Thanks & Regards</b><i>,<br /> Registration Branch, <br />Punjab School Education Board Mohali<br /></td></tr>";
                                string subject = "PSEB-View Registration of A2/F2";
                                bool result = DBClass.mail(subject, body, to);
                                if (result == true)
                                {
                                    status = "1";
                                }
                            }
                        }
                        else if (Act == "S")
                        {
                            string Mobile = ds.Tables[0].Rows[0]["Mobile"].ToString();
                            string Sms = "Registration No for Form's (A2/F2) of Schl Code " + Schl + " is Updated on " + DateTime.Now.ToString("dd/MM/yyyy") + ".For Detail Kindly Check your School Login Under->Registration Portal->View Registration of A2/F2.";
                            if (Mobile != "0" || Mobile != "")
                            {
                                string getSms = DBClass.gosms(Mobile, Sms);
                                //  string getSms = DBClass.gosms("9711819184", Sms);
                                if (getSms != "")
                                {
                                    status = "1";
                                }
                            }
                        }

                    }
                }

            }
            catch (Exception)
            {
            }
            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ExportDataFromDataTable(DataTable dt, string filename)
        {
            try
            {
                if (dt.Rows.Count == 0)
                {
                    return RedirectToAction("Index", "Admin");
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

                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Admin");
            }

        }

        #region FeeEntry
        [AdminLoginCheckFilter]
        public ActionResult FeeEntry(int? ID, FeeModels fm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {              
                List<SelectListItem> itemType = new List<SelectListItem>();
                List<SelectListItem> itemBank = new List<SelectListItem>();
                DataSet dsType = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecode
                if (dsType.Tables.Count > 0)
                {
                    // fee code
                    if (dsType.Tables[0].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in dsType.Tables[0].Rows)
                        {
                            itemType.Add(new SelectListItem { Text = @dr["FeeCat"].ToString(), Value = @dr["FeeCode"].ToString() });
                        }
                        ViewBag.FeeCodeList = itemType.ToList();
                    }
                    // bank code
                    if (dsType.Tables[1].Rows.Count > 0)
                    {
                        foreach (System.Data.DataRow dr in dsType.Tables[1].Rows)
                        {
                            itemBank.Add(new SelectListItem { Text = @dr["Bankname"].ToString(), Value = @dr["Bcode"].ToString() });
                        }
                        // ViewBag.BankCodeList = itemBank.ToList();

                        ViewBag.BankCodeList = new MultiSelectList(itemBank.ToList(), "Value", "Text");
                    }
                }


                ViewBag.FormNameList = DBClass.GetAllFormName();

                string Search = string.Empty;
                Search = "Id like '%' ";
                if (ID > 0)
                {
                    Search += " and Id=" + ID;
                }
                DataSet ds = _adminRepository.GetAllFeeMaster2016(Search);//GetAllFeeMaster2016SP
                fm.StoreAllData = ds;
                if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    fm.IsActive = 1;
                    if (ID > 0)
                    {

                        fm.FeeCode = Convert.ToInt32(ds.Tables[0].Rows[0]["FeeCode"].ToString());
                        fm.FORM = ds.Tables[0].Rows[0]["FORM"].ToString();
                        fm.StartDate = ds.Tables[0].Rows[0]["sDate"].ToString();
                        fm.EndDate = ds.Tables[0].Rows[0]["eDate"].ToString();
                        fm.BankLastDate = ds.Tables[0].Rows[0]["BankLastDate"].ToString();
                        fm.Fee = Convert.ToInt32(ds.Tables[0].Rows[0]["Fee"].ToString());
                        fm.LateFee = Convert.ToInt32(ds.Tables[0].Rows[0]["LateFee"].ToString());
                        fm.RP = ds.Tables[0].Rows[0]["RP"].ToString();
                        fm.AllowBanks = ds.Tables[0].Rows[0]["AllowBanks"].ToString();
                        fm.Type = ds.Tables[0].Rows[0]["Type"].ToString();
                        fm.IsActive = Convert.ToInt32(ds.Tables[0].Rows[0]["IsActive"].ToString());

                        // ViewBag.BankCodeList = itemBank.ToList();
                        int[] myArray1 = StaticDB.StringToIntArray(fm.AllowBanks, ',');
                        ViewBag.BankCodeList = new MultiSelectList(itemBank.ToList(), "Value", "Text", myArray1);

                    }
                    ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult FeeEntry(FeeModels fm, FormCollection frm, string cmd)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            List<SelectListItem> itemType = new List<SelectListItem>();
            List<SelectListItem> itemBank = new List<SelectListItem>();
            DataSet dsType = _adminRepository.GetFeeCodeMaster(1, 0);//for all feecode
            if (dsType.Tables.Count > 0)
            {
                // fee code
                if (dsType.Tables[0].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow dr in dsType.Tables[0].Rows)
                    {
                        itemType.Add(new SelectListItem { Text = @dr["FeeCat"].ToString(), Value = @dr["FeeCode"].ToString() });
                    }
                    ViewBag.FeeCodeList = itemType.ToList();
                }
                // bank code
                if (dsType.Tables[1].Rows.Count > 0)
                {
                    foreach (System.Data.DataRow dr in dsType.Tables[1].Rows)
                    {
                        itemBank.Add(new SelectListItem { Text = @dr["Bankname"].ToString(), Value = @dr["Bcode"].ToString() });
                    }
                    //ViewBag.BankCodeList = itemBank.ToList();
                    ViewBag.BankCodeList = new MultiSelectList(itemBank.ToList(), "Value", "Text");
                }
            }

            ViewBag.FormNameList = DBClass.GetAllFormName();

            if (cmd.ToLower().Contains("upload"))
            {
                string fileLocation = "";
                string filename = "";
                DataSet ds = new DataSet();
                if (fm.file != null)
                {
                    filename = Path.GetFileName(fm.file.FileName);
                    if (fm.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "MIS_" + filename + '_' + DateTime.Now.ToString("ddMMyyyyHHmm");  //MIS_201_110720161210
                        string fileExtension = System.IO.Path.GetExtension(fm.file.FileName);
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
                            fm.file.SaveAs(fileLocation);
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

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataTable dt1 = ds.Tables[0];
                                int OutStatus = 0;
                                DataTable dtResult = _adminRepository.BulkFeeMaster(dt1, Convert.ToInt32(adminLoginSession.AdminId), out OutStatus);  // BulkFeeMaster
                                if (OutStatus > 0)
                                {
                                    ViewData["result"] = "11";
                                    ViewBag.Result1 = "All Fees Uploaded Successfully";
                                }
                                else
                                {
                                    ViewData["result"] = "0";
                                    ViewBag.Result1 = "Fees Upload Failure";
                                }
                            }
                            else
                            {
                                ViewData["result"] = "12";
                                return View();
                            }
                        }
                        else
                        {
                            ViewData["result"] = "13";
                            return View();
                        }
                    }
                }
                else
                {
                    //Not Saved                 
                    ViewData["result"] = "14";
                }
                return View(fm);
            }


            if (cmd.ToLower().Contains("search"))
            {
                string Search = string.Empty;
                Search = "Id like '%' ";
                if (fm.FeeCode > 0)
                {
                    Search += " and FeeCode=" + fm.FeeCode;
                    if (fm.FeeCode == 20 || fm.FeeCode == 64)
                    {
                        if (fm.FORM != null && fm.FORM != "" && fm.FORM != "0")
                        {
                            Search += " and FORM='" + fm.FORM + "'";
                        }
                    }
                }

                DataSet ds = _adminRepository.GetAllFeeMaster2016(Search);//GetAllFeeMaster2016SP
                fm.StoreAllData = ds;
                if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                }
                else
                {
                    ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                }
                return View(fm);
            }

            //Check server side validation using data annotation
            if (ModelState.IsValid)
            {
                string SelectedSession = "";
                if (frm["SelectedSession"] == "" || frm["SelectedSession"] == null)
                {
                    ViewData["Result"] = 20;
                    return View(fm);
                }
                else
                {
                    fm.AllowBanks = SelectedSession = frm["SelectedSession"].ToString();
                }

                fm.FeeCat = itemType.ToList().Where(x => x.Value == fm.FeeCode.ToString()).Select(x => x.Text).FirstOrDefault();

                if (fm.FeeCode == 20 || fm.FeeCode == 64)
                {
                    fm.Class = DBClass.GetClassNumber(fm.FORM);
                }
                else
                {
                    fm.FORM = fm.FeeCat;
                    //if (fm.FeeCat.ToLower().Contains("matric")) { fm.Class = 10; }
                    //else { fm.Class = 12; }
                }

                if (fm.ID == null) { fm.ID = 0; }
                int result = _adminRepository.Insert_FeeMaster(fm);//InsertFeeMaster2016SP
                if (result > 0)
                {
                    //----Data Inserted Successfully
                    //ViewBag.Message = "File has been uploaded successfully";
                    ModelState.Clear();
                    //--For Showing Message---------//
                    ViewData["result"] = 1;
                }
                else if (result == -1)
                {
                    //-----alredy exist
                    ViewData["result"] = -1;
                }
                else
                {
                    //Not Saved                 
                    ViewData["result"] = 0;
                }

            }
            return View(fm);
        }

        #endregion FeeEntry

        #region Begin ExamErrorListMIS

       
        public ActionResult DownloadErrorData()
        {
            try
            {
                if (TempData["dtResultError"] == null)
                {
                    return RedirectToAction("ErrorListMIS", "Admin");
                }
                if (Request.QueryString["File"] == null)
                {
                    return RedirectToAction("ErrorListMIS", "Admin");
                }
                else
                {
                    string file1 = Request.QueryString["File"].ToString();
                    DataSet ds = (DataSet)TempData["dtResultError"];
                    DataTable dt;
                    dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        return RedirectToAction("ErrorListMIS", "Admin");
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            string fileName1 = "DownloadErrorData_" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xls";  //103_230820162209_347
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                // wb.Worksheets.Add(dt);
                                wb.Worksheets.Add(dt, "DownloadErrorData");
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
                }
                return RedirectToAction("ErrorListMIS", "Admin");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorListMIS", "Admin");
            }

        }

        [AdminLoginCheckFilter]
        public ActionResult ErrorListMIS()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];              
                int type = 1; //for private compt
                string OutResult1 = "0";
                DataSet dtError = _adminRepository.GetErrorListMISByFirmId(type, adminLoginSession.AdminId, out OutResult1);// OutStatus mobile
                if (OutResult1 == "1")
                {
                    if (dtError.Tables.Count > 0)
                    {                        
                        ViewBag.ErrRegTotal = dtError.Tables[0].Rows.Count;//reg
                        TempData["dtResultError"] = dtError;
                    }
                }
                else { ViewBag.Errorcount = 0; }
                return View();
           
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult ErrorListMIS(AdminModels AM, FormCollection frm) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {               
                
                int type = 1; //for private compt
                string OutResult1 = "0";
                DataSet dtError = _adminRepository.GetErrorListMISByFirmId(type, adminLoginSession.AdminId, out OutResult1);// OutStatus mobile
                if (OutResult1 == "1")
                {
                    if (dtError.Tables.Count > 0)
                    {                      
                        ViewBag.ErrRegTotal = dtError.Tables[0].Rows.Count;//reg
                        TempData["dtResultError"] = dtError;
                    }
                }
                else { ViewBag.Errorcount = 0; }

                // string firm = AbstractLayer.StaticDB.GetFirmName(Session["UserName"].ToString());
                // string firm = adminLoginSession.USERNAME;
                string id = frm["Filevalue"].ToString();

                string fileLocation = "";
                string filename = "";
                if (AM.file != null)
                {
                    filename = Path.GetFileName(AM.file.FileName);
                }
                else
                {
                    ViewData["Result"] = "-4";
                    ViewBag.Message = "Please select .xls file only";
                    return View();
                }
                DataSet ds = new DataSet();
                if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                {
                    // string fileName1 = "ErrorMIS_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                    string fileName1 = "ErrorMIS_" + id.ToString().ToUpper() + '_' + adminLoginSession.AdminType + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");

                    string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
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
                        AM.file.SaveAs(fileLocation);
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


                        string CheckMis = "";



                        DataTable dtexport = new DataTable();
                        var duplicates = ds.Tables[0].AsEnumerable()
                             .GroupBy(i => new { Name = i.Field<string>("CANDID"), Subject = i.Field<string>("ERRCODE") })
                             .Where(g => g.Count() > 1)
                             .Select(g => new { g.Key.Name, g.Key.Subject }).ToList();
                        if (duplicates.Count() > 0)
                        {
                            ViewData["Result"] = "11";
                            ViewBag.Message = "Duplicate Record";
                            return View();
                        }
                        CheckMis = _adminRepository.CheckExamMisExcel(ds, out dtexport); // REG

                        if (CheckMis == "")
                        {
                            DataTable dt1 = ds.Tables[0];
                            if (dt1.Columns.Contains("ErrStatus"))
                            {
                                dt1.Columns.Remove("ErrStatus");
                            }
                            dt1.AcceptChanges();                            
                            int OutStatus = 0;
                            string OutResult = "0";
                            /// DataTable dtResult = objDB.ExamErrorListMIS(dt1, AdminId, out OutStatus);// OutStatus mobile


                            DataSet dtResult = _adminRepository.ExamErrorListMIS(dt1, adminLoginSession.AdminId, out OutStatus);// OutStatus mobile
                            if (OutStatus > 0 || OutResult == "1")
                            {
                                ViewBag.Message = "File Uploaded Successfully";
                                ViewData["Result"] = "1";
                            }
                            else
                            {
                                ViewBag.Message = "File Not Uploaded Successfully";
                                ViewData["Result"] = "0";
                            }
                            return View();
                        }
                        else
                        {
                            if (dtexport != null)
                            {
                                ExportDataFromDataTable(dtexport, id.ToString().ToUpper() + "_ErrorReport");
                            }
                            ViewData["Result"] = "-1";
                            ViewBag.Message = CheckMis;
                            return View();
                        }
                    }
                    else
                    {

                        ViewData["Result"] = "-2";
                        ViewBag.Message = "Please Upload Only .xls file only";
                        return View();
                    }
                }

            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion  ExamErrorListMIS



        #region Begin StudentRollNoMIS
        [AdminLoginCheckFilter]
        public ActionResult StudentRollNoMIS()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            return View();
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult StudentRollNoMIS(AdminModels AM, FormCollection frm, string Category, string submit) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                if (submit != null)
                {
                    if (submit.ToUpper() == "DOWNLOAD")
                    {
                    }
                }

               
                string id = frm["Filevalue"].ToString();
                Category = id;
                if (adminLoginSession.USERNAME != null)
                {
                    //HttpContext.Session["AdminType"]
                    //string AdminType = Session["AdminType"].ToString();
                    //int AdminId = Convert.ToInt32(Session["AdminId"].ToString());
                    string fileLocation = "";
                    string filename = "";
                    if (AM.file != null)
                    {
                        filename = Path.GetFileName(AM.file.FileName);
                    }
                    else
                    {
                        ViewData["Result"] = "-4";
                        ViewBag.Message = "Please select .xls file only";
                        return View();
                    }
                    DataSet ds = new DataSet();
                    if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "";


                        if (id.ToString().ToUpper() == "ROLL" || id.ToString().ToUpper() == "ROLLONLY")
                        {
                            fileName1 = "StdRollNo_" + adminLoginSession.AdminType + adminLoginSession.AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                        }
                        else if (id.ToString().ToUpper() == "RANGE")
                        {
                            fileName1 = "StdRollNoRange_" + adminLoginSession.AdminType + adminLoginSession.AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                        }                       
                        else
                        {
                            return RedirectToAction("StudentRollNoMIS", "Admin");
                        }

                        // string fileName1 = "StdRollNo_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                        string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
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
                            AM.file.SaveAs(fileLocation);
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

                            string CheckMis = "";
                            string ColName = "";
                            ColName = ds.Tables[0].Columns[1].ColumnName.ToString();


                            DataTable dtexport = new DataTable();
                            if (id.ToString().ToUpper() == "ROLL" && ColName.ToUpper() == "CANDID")
                            {
                                string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[1].ToString()).ToArray();
                                bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                                if (CheckChln == true)
                                {
                                    ViewData["Result"] = "11";
                                    ViewBag.Message = "Duplicate Candidate Id";
                                    return View();
                                }
                                CheckMis = _adminRepository.CheckStdRollNoMis(ds, out dtexport);
                            }
                            else if (id.ToString().ToUpper() == "RANGE" && ColName.ToUpper() == "SROLL")
                            {
                                CheckMis = _adminRepository.CheckStdRollNoRangeMis(ds, out dtexport);
                            }                           
                            else if (id.ToString().ToUpper() == "ROLLONLY" && ColName.ToUpper() == "CANDID")
                            {
                                string[] arrayChln = ds.Tables[0].Rows.OfType<DataRow>().Select(k => k[1].ToString()).ToArray();
                                bool CheckChln = StaticDB.CheckArrayDuplicates(arrayChln);
                                if (CheckChln == true)
                                {
                                    ViewData["Result"] = "11";
                                    ViewBag.Message = "Duplicate Candidate Id";
                                    return View();
                                }
                                CheckMis = _adminRepository.CheckStdRollNoMisOnly(ds, out dtexport);
                            }
                            else
                            {
                                ViewBag.Message = "Please Check File Structure";
                                ViewData["Result"] = "5";
                                return View();
                            }
                            if (CheckMis == "")
                            {
                                DataSet ds1 = new DataSet();
                                DataTable dt1 = ds.Tables[0];
                                if (dt1.Columns.Contains("STATUS"))
                                {
                                    dt1.Columns.Remove("STATUS");
                                }
                                dt1.AcceptChanges();                                
                                int OutStatus = 0;
                                string OutResult = "0";
                                DataSet dtResult = new DataSet();
                                if (id.ToString().ToUpper() == "ROLL")
                                {
                                    dtResult = _adminRepository.StudentRollNoMIS(dt1, adminLoginSession.AdminId, out OutStatus);// OutStatus mobile
                                }
                                else if (id.ToString().ToUpper() == "ROLLONLY")
                                {
                                    dtResult = _adminRepository.StudentRollNoMISONLY(dt1, adminLoginSession.AdminId, out OutStatus);// OutStatus mobile
                                }
                                else if (id.ToString().ToUpper() == "RANGE")
                                {
                                    dtResult = _adminRepository.StudentRollNoRangeMIS(dt1, adminLoginSession.AdminId, out OutStatus);// OutStatus mobile
                                }
                               else
                                {
                                    ViewBag.Message = "Please Check File Structure";
                                    ViewData["Result"] = "5";
                                    return View();
                                }


                                if (OutStatus > 0 || OutResult == "1")
                                {
                                    ViewBag.Message = "File Uploaded Successfully";
                                    ViewData["Result"] = "1";
                                }
                                else
                                {
                                    if (OutResult.ToLower().Contains("update"))
                                    {
                                        ViewBag.Message = "File Not Uploaded Successfully- Duplicate Records";
                                    }
                                    else
                                    { ViewBag.Message = "File Not Uploaded Successfully"; }                                   
                                    ViewData["Result"] = "0";
                                }
                                return View();
                            }
                            else
                            {
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(dtexport, adminLoginSession.USERNAME);
                                }

                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }

                        }
                        else
                        {

                            ViewData["Result"] = "-2";
                            ViewBag.Message = "Please Upload Only .xls file only";
                            return View();
                        }
                    }
                }
                else { return RedirectToAction("Index", "Admin"); }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }




        #endregion  StudentRollNoMIS

        #region  ViewCandidateExamErrorList
        [AdminLoginCheckFilter]
        public ActionResult ViewCandidateExamErrorList(int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {

                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                //string Catg = CrType;                        

                //---- Paging end               
                string UserNM = adminLoginSession.USER.ToString();


                AdminModels am = new AdminModels();
                if (ModelState.IsValid)
                {
                    if (page != null && page > 0 && TempData["ExamErrorStatus"] != null)
                    {
                        string Search = TempData["ExamErrorStatus"].ToString();
                        am.StoreAllData = _adminRepository.ViewCandidateExamErrorList(0, Search, UserNM, "", pageIndex);
                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCountP = ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["TotalCount"].ToString());
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
                }
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult ViewCandidateExamErrorList(AdminModels am, FormCollection frc, int? page, string ERRcode, string cmd, string SearchString, string refno)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {

                //------ Paging Srt
                int pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
                ViewBag.pagesize = pageIndex;
                //string Catg = CrType;                        


                string UserNM = adminLoginSession.USER.ToString();


                if (ModelState.IsValid)
                {
                    string Search = "Refno like '%%' ";
                    string drp = "";
                    string SelType = "";
                    int SelValueSch = 0;


                    if (frc["SelType"] != null)
                    {
                        
                        ViewBag.SelType= SelType = frc["SelType"].ToString();
                        Search += " and ExamType= '" + SelType + "'";
                    }

                    if (frc["ERRcode"] != null)
                    {
                        drp = frc["ERRcode"].ToString();
                        SelValueSch = Convert.ToInt32(drp.ToString());
                        if (drp.ToString().Trim() != "" && drp != null)
                        {
                            if (SelValueSch == 1)
                            {
                                Search += " and refno='" + SearchString.ToString().Trim() + "'";
                            }
                            if (SelValueSch == 2)
                            {
                                Search += " and RollNo='" + SearchString.ToString().Trim() + "'";
                            }
                            if (SelValueSch == 3)
                            {
                                Search += " and Errcode='" + SearchString.ToString().Trim() + "'";
                            }
                            if (SelValueSch == 4)
                            {
                                Search += " and SCHL='" + SearchString.ToString().Trim() + "'";
                            }
                        }
                    }

                    if (cmd == "Search")
                    {
                        //---- Paging end
                        am.ERRcode = ERRcode;
                        ViewBag.Searchstring = SearchString;
                        ViewBag.CorrectionType = frc["ERRcode"].ToString();

                        TempData["ExamErrorStatus"] = Search;
                        am.StoreAllData = _adminRepository.ViewCandidateExamErrorList(0, Search, UserNM, "", pageIndex);

                        if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                        {
                            ViewBag.Message = "Record Not Found";
                            ViewBag.TotalCountP = ViewBag.TotalCount = 0;
                            return View(am);
                        }
                        else
                        {
                            ViewBag.TotalCountP = am.StoreAllData.Tables[0].Rows.Count;
                            ViewBag.TotalCount = Convert.ToInt32(am.StoreAllData.Tables[1].Rows[0]["TotalCount"].ToString());
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

                    if (cmd == "Download Error List")
                    {
                        if (Search == null || Search == "")
                        {
                            if (adminLoginSession.AdminType.ToLower() != "admin")
                            {
                                Search = " and CreatedBy='" + adminLoginSession.USER + "'";
                            }
                        }
                        DataSet ds = _adminRepository.ViewCandidateExamErrorList(1, Search, UserNM, "", pageIndex);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataTable dt = ds.Tables[0];
                                string fname = DateTime.Now.ToString("ddMMyyyyHHmm");
                                Response.Clear();
                                Response.Buffer = true;
                                Response.Charset = "";
                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.AddHeader("content-disposition", "attachment;filename=ErrList" + fname + ".xlsx");
                                using (MemoryStream MyMemoryStream = new MemoryStream())
                                {
                                    XLWorkbook wb = new XLWorkbook();
                                    var WS = wb.Worksheets.Add(dt, "ErrList" + fname);
                                    WS.Tables.FirstOrDefault().ShowAutoFilter = false;
                                    wb.SaveAs(MyMemoryStream);
                                    MyMemoryStream.WriteTo(Response.OutputStream);
                                    WS.AutoFilter.Enabled = false;
                                    Response.Flush();
                                    Response.End();
                                }
                            }
                        }

                    }

                }
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }


        }

        [AdminLoginCheckFilter]
        public ActionResult RemoveCandidateExamError(string id, string errcode)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            AdminModels am = new AdminModels();
            try
            {
               
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Ref No." }, new { ID = "2", Name = "Roll Num" }, new { ID = "3", Name = "Error Code" }, }, "ID", "Name", 1);
                ViewBag.CorrectionType = itemsch.ToList();
                
              
                if (ModelState.IsValid)
                {
                  
                    am.StoreAllData = _adminRepository.RemoveCandidateExamError(adminLoginSession.USER,id, errcode);
                    if (am.StoreAllData == null || am.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(am);
                    }
                    else
                    {
                        return RedirectToAction("ViewCandidateExamErrorList", "Admin");
                    }
                }
                ViewBag.Message = "Record Not Found";
                ViewBag.TotalCount = 0;
                return View();
            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                //return RedirectToAction("Logout", "Login");
                return View();
            }
        }

        #endregion  ViewCandidateExamErrorList



        #region AllowCCE
        [AdminLoginCheckFilter]
        public ActionResult AllowCCE(int? ID, SchoolAllowForMarksEntry fm, int? page)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];


            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = " Receipt No" }, new { ID = "3", Name = " Roll No" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";

            try
            {                
                int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                ViewBag.Id = ID == null ? 0 : ID;
                string Search = string.Empty;
                Search = "Id like '%' ";
                int Outstatus = 0;

                if (ID > 0)
                {
                    Search += " and Id=" + ID;                  
                    DataSet ds = _adminRepository.ListingSchoolAllowForMarksEntry(2, 0, Search, out Outstatus);
                    fm.StoreAllData = ds;
                    if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        if (ID > 0)
                        {
                            fm.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"].ToString());
                            fm.Schl = Convert.ToString(ds.Tables[0].Rows[0]["Schl"].ToString());
                            fm.Cls = Convert.ToString(ds.Tables[0].Rows[0]["Cls"].ToString());
                            fm.LastDate = Convert.ToString(ds.Tables[0].Rows[0]["LastDate"].ToString());
                            fm.AllowTo = Convert.ToString(ds.Tables[0].Rows[0]["AllowTo"].ToString());
                            fm.ReceiptNo = Convert.ToString(ds.Tables[0].Rows[0]["ReceiptNo"].ToString());
                            fm.DepositDate = Convert.ToString(ds.Tables[0].Rows[0]["DepositDate"].ToString());
                            fm.Amount = Convert.ToInt32(ds.Tables[0].Rows[0]["Amount"].ToString());
                            fm.Panel = Convert.ToString(ds.Tables[0].Rows[0]["Panel"].ToString().ToLower());
                        }
                        ViewBag.TotalCountId = fm.StoreAllData.Tables[0].Rows.Count;
                    }
                }
                else
                {
                    DataSet ds = _adminRepository.ListingSchoolAllowForMarksEntry(2, 0, Search, out Outstatus);
                    fm.StoreAllData = ds;
                    if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View();
                    }
                    else
                    {
                        ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                    }
                    return View(fm);
                }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));               
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult AllowCCE(SchoolAllowForMarksEntry fm, FormCollection frm, string cmd, string SearchList, string SearchString)
        {

            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            var itemsch1 = new SelectList(new[] { new { ID = "1", Name = "School Code" }, new { ID = "2", Name = " Receipt No" }, new { ID = "3", Name = " Roll No" }, }, "ID", "Name", 1);
            ViewBag.MySearch = itemsch1.ToList();
            ViewBag.SelectedSearch = "0";

          
            int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
            ViewBag.Id = fm.Id == null ? 0 : fm.Id;
           
            if (!cmd.ToLower().Contains("search"))
            {
                int type = 0;
                if (fm.Id == null)
                {
                    fm.Id = 0;
                    type = 0;
                }
                else { type = 1; }
                string SchlMobile = "";
                int result = _adminRepository.InsertSchoolAllowForMarksEntry(type, fm, out SchlMobile);//InsertFeeMaster2016SP          
                if (result > 0)
                {
                    if (fm.Id == 0)
                    {
                        ViewData["result"] = 1;
                        //string cls1 = fm.Cls == "4" ? "Sr. Sec" : "Matric";
                        //if (!string.IsNullOrEmpty(SchlMobile))
                        //{
                        //    string Sms = "";
                        //    if (fm.Cls == "10")
                        //    {
                        //        Sms = "As per your school request, Elective Theory has been unlocked for " + cls1 + " upto date  " + fm.LastDate + ". Kindly fill & final submit.";
                        //    }
                        //    else
                        //    {
                        //        //As per your school request, CCE has been unlocked for +class+ upto date +allowedupto .Kindly fill &final submit CCE. 
                        //        Sms = "As per your school request, CCE has been unlocked for " + cls1 + " upto date  " + fm.LastDate + ". Kindly fill &final submit CCE.";
                        //    }
                        //    string getSms = DBClass.gosms(SchlMobile, Sms);
                        //}
                    }
                    else { ViewData["result"] = 2; }
                }
                else if (result == -1)
                {
                    //-----alredy exist
                    ViewData["result"] = -1;
                }
                else
                {
                    //Not Saved                 
                    ViewData["result"] = 0;
                }

            }
            else
            {
                string Search = string.Empty;
                Search = "Id like '%' ";
                int Outstatus = 0;

                if (!string.IsNullOrEmpty(SearchList))
                {
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        if (SearchList == "1")
                        { Search += " and Schl ='" + SearchString.ToString() + "'"; }
                        else if (SearchList == "2")
                        { Search += " and ReceiptNo ='" + SearchString.ToString() + "'"; }
                        else if (SearchList == "3")
                        { Search += " and AllowTo like '%" + SearchString.ToString() + "%'"; }

                        ViewBag.SearchString = SearchString;
                        TempData["SearchString"] = SearchString;
                    }
                }



                DataSet ds = _adminRepository.ListingSchoolAllowForMarksEntry(2, 0, Search, out Outstatus);
                fm.StoreAllData = ds;
                if (fm.StoreAllData == null || fm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View();
                }
                else
                {
                    ViewBag.TotalCount = fm.StoreAllData.Tables[0].Rows.Count;
                }
                return View(fm);
            }
            return View(fm);
        }

        [AdminLoginCheckFilter]
        public ActionResult ListingCCE(int? Id, SchoolAllowForMarksEntry fm)
        {           
            if (Id > 0)
            {
                string Search = string.Empty;
                Search = "Id like '%' ";
                int Outstatus = 0;

                DataSet ds = _adminRepository.ListingSchoolAllowForMarksEntry(0, Convert.ToInt32(Id), Search, out Outstatus);
                if (Outstatus > 0)
                {
                    TempData["DeleteCCE"] = "1";
                }
                else
                {
                    TempData["DeleteCCE"] = null;
                }
            }
            return RedirectToAction("AllowCCE");
        }
        #endregion AllowCCE


        #region Practical Final Submission Unlock
        [AdminLoginCheckFilter]
        public ActionResult PracticalSubmissionUnlocked()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            return View();
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult PracticalSubmissionUnlocked(AdminModels AM, FormCollection frm, string Category, string submit) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            { 
              
                string id = frm["Filevalue"].ToString();
                Category = id;
                if (adminLoginSession.USER != null)
                {                    

                  
                    string AdminType = adminLoginSession.AdminType.ToString();
                    int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                    string fileLocation = "";
                    string filename = "";
                    if (AM.file != null)
                    {
                        filename = Path.GetFileName(AM.file.FileName);
                    }
                    else
                    {
                        ViewData["Result"] = "-4";
                        ViewBag.Message = "Please select .xls file only";
                        return View();
                    }
                    DataSet ds = new DataSet();
                    if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "";

                        if (id.ToString().ToUpper() == "PRAC")
                        {
                            fileName1 = "PracticalSubmissionUnlocked_" + "_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                        }                       
                        else
                        {
                            return RedirectToAction("PracticalSubmissionUnlocked", "Admin");
                        }
                        string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
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
                            AM.file.SaveAs(fileLocation);
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

                            string CheckMis = "";
                            DataTable dtexport = new DataTable();                           
                            int flg = 0;
                            if (id.ToString().ToUpper() == "PRAC")
                            {
                                flg = 1;
                                CheckMis = _adminRepository.CheckPracticalSubmissionUnlocked(ds, out dtexport);
                            }

                            if (CheckMis == "" && flg == 1)
                            {
                                DataSet ds1 = new DataSet();
                                DataTable dt1 = ds.Tables[0];                                
                                dt1.AcceptChanges();
                                //string Result1 = "";
                                //int OutStatus = 0;
                                //string OutResult = "0";
                                string OutError = "0";
                                if (id.ToString().ToUpper() == "PRAC")
                                {
                                    for (int i = 0; i < dt1.Rows.Count; i++)
                                    {

                                        string CLASS = dt1.Rows[i][0].ToString();
                                        string RP = dt1.Rows[i][1].ToString();
                                        string PCENT = dt1.Rows[i][2].ToString();
                                        string SUB = dt1.Rows[i][3].ToString();
                                        string LOT = dt1.Rows[i][4].ToString();
                                        DataSet dtResult = _adminRepository.PracticalSubmissionUnlocked(CLASS, RP, PCENT, SUB, LOT, AdminId, out OutError);// OutStatus mobile
                                        if (OutError == "1")
                                        { dt1.Rows[i]["Status"] = "Successfully Unlocked"; }
                                        else
                                        { dt1.Rows[i]["Status"] = "Failure : " + OutError; }

                                    }
                                    ViewBag.Message = "File Uploaded Successfully";
                                    ViewData["Result"] = "1";
                                    if (dt1 != null)
                                    {
                                        ExportDataFromDataTable(dt1, "PracticalSubmissionUnlocked");
                                    }
                                    return View();
                                }
                                else
                                {
                                    ViewBag.Message = "Please Check File Structure";
                                    ViewData["Result"] = "5";
                                    return View();
                                }

                              
                            }
                            else
                            {
                                // CheckMis = "Selected Class and File class not matched , please check ";
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(dtexport, adminLoginSession.USER);
                                }

                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }

                        }
                        else
                        {

                            ViewData["Result"] = "-2";
                            ViewBag.Message = "Please Upload Only .xls file only";
                            return View();
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {               
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }

        #endregion  Practical Final Submission Unlock


        #region PracticalCentUpdateMaster
        [AdminLoginCheckFilter]
        public ActionResult PracticalCentUpdateMaster()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            // var itemsch = new SelectList(new[] { new { ID = "1", Name = "Regular" }, new { ID = "2", Name = "Pvt" }, }, "ID", "Name", 1);
            var itemsch = new SelectList(new[] { new { ID = "1", Name = "Regular" } }, "ID", "Name", 1);
            ViewBag.MySch = itemsch.ToList();
            ViewBag.SelectedItem = "0";

            var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
            ViewBag.Mycls = itemcls.ToList();
            ViewBag.Selectedcls = "0";
            return View();
        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult PracticalCentUpdateMaster(AdminModels AM, FormCollection frm, string Category, string submit) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                
                var itemsch = new SelectList(new[] { new { ID = "1", Name = "Regular" } }, "ID", "Name", 1);
                ViewBag.MySch = itemsch.ToList();
                ViewBag.SelectedItem = "0";

                var itemcls = new SelectList(new[] { new { ID = "5", Name = "Primary" }, new { ID = "8", Name = "Middle" }, }, "ID", "Name", 1);
                ViewBag.Mycls = itemcls.ToList();
                ViewBag.Selectedcls = "0";

                               
                string id = frm["Filevalue"].ToString();
                Category = id;
                if (adminLoginSession.USER != null)
                {
                    string SelList = string.Empty;
                    if (frm["SelList"] == null) { }
                    else
                    {
                        SelList = frm["SelList"].ToString();
                    }


                    string SelClass = string.Empty;
                    if (frm["SelClass"] == null) { }
                    else
                    {
                        SelClass = frm["SelClass"].ToString();
                    }

                   
                    string AdminType = adminLoginSession.AdminType.ToString();
                    int AdminId = Convert.ToInt32(adminLoginSession.AdminId.ToString());
                    string fileLocation = "";
                    string filename = "";
                    if (AM.file != null)
                    {
                        filename = Path.GetFileName(AM.file.FileName);
                    }
                    else
                    {
                        ViewData["Result"] = "-4";
                        ViewBag.Message = "Please select .xls file only";
                        return View();
                    }
                    DataSet ds = new DataSet();
                    if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "";

                        if (id.ToString().ToUpper() == "CENT")
                        {
                            fileName1 = "PracCENT_" + SelList + "_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                        }
                        else if (id.ToString().ToUpper() == "STD")
                        {
                            fileName1 = "PracCENT_STD_" + "_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210
                        }
                        else
                        {
                            return RedirectToAction("PracticalCentUpdateMaster", "Admin");
                        }
                        string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
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
                            AM.file.SaveAs(fileLocation);
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

                            string CheckMis = "";
                            DataTable dtexport = new DataTable();
                            string ColNameCls = "";
                            int flg = 0;
                            if (id.ToString().ToUpper() == "STD")
                            {
                                flg = 1;
                                CheckMis = _adminRepository.CheckStdPracticalCentUpdateMaster(ds, out dtexport);
                            }

                            else
                            {

                                if (SelList.ToString() == "1")
                                {
                                    ColNameCls = ds.Tables[0].Rows[0]["class"].ToString();
                                    if (SelClass == ColNameCls)
                                    {
                                        flg = 1;
                                        CheckMis = _adminRepository.CheckRegOpenPracticalCentUpdateMaster(ds, ColNameCls, out dtexport);
                                    }
                                }
                                //else if (SelList.ToString() == "2")
                                //{
                                //    flg = 1;
                                //    CheckMis = _adminRepository.CheckPrivatePracticalCentUpdateMaster(ds, out dtexport);
                                //}
                                else
                                {
                                    ViewBag.Message = "Please Check File Structure";
                                    ViewData["Result"] = "5";
                                    return View();
                                }

                            }

                            if (CheckMis == "" && flg == 1)
                            {
                                DataSet ds1 = new DataSet();
                                DataTable dt1 = ds.Tables[0];
                                if (dt1.Columns.Contains("STATUS"))
                                {
                                    dt1.Columns.Remove("STATUS");
                                }
                                dt1.AcceptChanges();    
                                string OutError = "0";
                                if (id.ToString().ToUpper() == "STD")
                                {
                                    DataSet dtResult = _adminRepository.PracCentSTD(dt1, AdminId, SelList, SelClass, out OutError);// OutStatus mobile
                                }
                                else if (id.ToString().ToUpper() == "CENT")
                                {
                                    DataSet dtResult = _adminRepository.PracticalCentUpdateMaster(dt1, AdminId, SelList, SelClass, out OutError);// OutStatus mobile
                                }
                                else
                                {
                                    ViewBag.Message = "Please Check File Structure";
                                    ViewData["Result"] = "5";
                                    return View();
                                }

                                if (OutError == "1")
                                {
                                    ViewBag.Message = "File Uploaded Successfully";
                                    ViewData["Result"] = "1";
                                }
                                else
                                {
                                    if (OutError.ToLower().Contains("update"))
                                    {
                                        ViewBag.Message = "File Not Uploaded Successfully- Duplicate Records";
                                    }
                                    else
                                    { ViewBag.Message = "File Not Uploaded Successfully"; }                                   
                                    ViewData["Result"] = "0";
                                }
                                return View();
                            }
                            else
                            {
                                
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(dtexport, adminLoginSession.USER);
                                }

                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }

                        }
                        else
                        {

                            ViewData["Result"] = "-2";
                            ViewBag.Message = "Please Upload Only .xls file only";
                            return View();
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }

        #endregion PracticalCentUpdateMaster

        #region View Generate Ticket List

        public JsonResult BindGenerateTicketList(int ticketId)
        {
           // AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            List<GenerateTicketModel> _model = new List<GenerateTicketModel>();
            if (ticketId == 0)
            {
                _model = _centerheadrepository.GetGenerateTicketData(0, "", 0, "");
                return Json(new { data = _model.ToList() }, JsonRequestBehavior.AllowGet);
            }
            else if (ticketId > 0)
            {
                _model = _centerheadrepository.GetGenerateTicketData(1, "", ticketId, "");
            }
            return Json(_model.ToList(), JsonRequestBehavior.AllowGet);
        }


        [AdminLoginCheckFilter]
        public ActionResult ViewTicketList(GenerateTicketModel gtm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            return View(gtm);
        }

     
        [HttpPost]
        public ActionResult ViewTicketList(FormCollection frm)
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            string statusOut = "";
            try
            {
                var Jsonobject = frm["modelData"];
                var actionType = frm["actionType"];
                GenerateTicketModel oModel = JsonConvert.DeserializeObject<GenerateTicketModel>(Jsonobject);
                string outTicketNo = "";

                if (actionType.ToLower() == "resolve")
                {                    //
                    oModel.ActionType = 3;
                    oModel.AdminId = adminLoginSession.AdminId;
                    oModel.TicketStatusBy = adminLoginSession.USER;
                    oModel.TicketStatus = 1;
                }               

                //
                int result = _centerheadrepository.InsertGenerateTicket(oModel, out outTicketNo);//InsertFeeMaster2016SP
                ViewBag.outTicketNo = outTicketNo;
                if (result > 0)
                {
                    statusOut = "1";                   
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


        //[AdminLoginCheckFilter]
        //public ActionResult ViewAllCluster(CenterHeadMasterModelList centerHeadMasterModelList)
        //{
        //    try
        //    {
        //        AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

        //        DataSet ds = _centerheadrepository.CenterHeadMaster(1,0, "");

        //        //centerHeadMasterModelList = new List<CenterHeadMasterModel>();

        //        var _list = ds.Tables[0].AsEnumerable().Select(dataRow => new CenterHeadMasterModel
        //        {
        //            CenterHeadId = dataRow.Field<int>("CenterHeadId"),
        //            UserName = dataRow.Field<string>("UserName"),
        //            CenterHeadName = dataRow.Field<string>("CenterHeadName"),
        //            Mobile = dataRow.Field<string>("Mobile"),
        //            EmailId = dataRow.Field<string>("EmailId"),
        //        }).ToList();

        //        centerHeadMasterModelList = _list;



        //        return View(centerHeadMasterModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
        //        return View();
        //    }
        //}



        #endregion


        #region  UnlockClusterTheoryMarks


        [AdminLoginCheckFilter]
        public ActionResult UnlockClusterTheoryMarks()
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            return View();

        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult UnlockClusterTheoryMarks(AdminModels AM, FormCollection frm) // HttpPostedFileBase file
        {
            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];
            try
            {
                
                string id = frm["Filevalue"].ToString();

                string fileLocation = "";
                string filename = "";
                if (AM.file != null)
                {
                    filename = Path.GetFileName(AM.file.FileName);
                }
                else
                {
                    ViewData["Result"] = "-4";
                    ViewBag.Message = "Please select .xls file only";
                    return View();
                }
                DataSet ds = new DataSet();
                if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                {
                    // string fileName1 = "ErrorMIS_" + AdminType + AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                    string fileName1 = "UnlockClusterTheory_" + id.ToString().ToUpper() + '_' + adminLoginSession.AdminType + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");

                    string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
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
                        AM.file.SaveAs(fileLocation);
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


                        string CheckMis = "";
                        if (ds.Tables[0].Rows.Count == 0)
                        {
                            ViewData["Result"] = "20";
                            ViewBag.Message = "Empty Excel file";
                            return View();
                        }


                        DataTable dtexport = new DataTable();
                        var duplicates = ds.Tables[0].AsEnumerable()
                             .GroupBy(i => new { Name = i.Field<string>("SCHL")})
                             .Where(g => g.Count() > 1)
                             .Select(g => new { g.Key.Name }).ToList();
                        if (duplicates.Count() > 0)
                        {
                            ViewData["Result"] = "11";
                            ViewBag.Message = "Duplicate Record";
                            return View();
                        }
                        CheckMis = _adminRepository.CheckUnlockClusterTheoryExcel(ds, out dtexport); // REG

                        if (CheckMis == "")
                        {
                            DataTable dt1 = ds.Tables[0];
                            if (dt1.Columns.Contains("ErrStatus"))
                            {
                                dt1.Columns.Remove("ErrStatus");
                            }
                            dt1.AcceptChanges();
                            int OutStatus = 0;
                            string OutResult = "0";
                           
                            DataSet dtResult = _adminRepository.UnlockClusterTheoryMarks(dt1, adminLoginSession.AdminId, out OutResult);// OutStatus mobile
                            if (OutStatus > 0 || OutResult == "1")
                            {
                                ViewBag.Message = "File Uploaded Successfully";
                                ViewData["Result"] = "1";
                            }
                            else
                            {
                                ViewBag.Message = "File Not Uploaded Successfully";
                                ViewData["Result"] = "0";
                            }
                            return View();
                        }
                        else
                        {
                            if (dtexport != null)
                            {
                                ExportDataFromDataTable(dtexport, id.ToString().ToUpper() + "_ErrorReport");
                            }
                            ViewData["Result"] = "-1";
                            ViewBag.Message = CheckMis;
                            return View();
                        }
                    }
                    else
                    {

                        ViewData["Result"] = "-2";
                        ViewBag.Message = "Please Upload Only .xls file only";
                        return View();
                    }
                }

            }
            catch (Exception ex)
            {
                ////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion  UnlockClusterTheoryMarks


        #region Begin Admin Result Update MIS

        [AdminLoginCheckFilter]
        public ActionResult AdminResultUpdateMIS()
        {

            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {
                if (adminLoginSession.USERNAME == null && (adminLoginSession.USERNAME.ToString().ToUpper() != "ADMIN" || adminLoginSession.USERNAME.ToString().ToUpper() != "PERF" || adminLoginSession.USERNAME.ToString().ToUpper() != "SAI"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Admin");
            }

        }

        [AdminLoginCheckFilter]
        [HttpPost]
        public ActionResult AdminResultUpdateMIS(AdminModels AM) // HttpPostedFileBase file
        {

            AdminLoginSession adminLoginSession = (AdminLoginSession)Session["AdminLoginSession"];

            try
            {
                // firm login // dist 
                if (adminLoginSession.USERNAME == null && (adminLoginSession.USERNAME.ToString().ToUpper() != "ADMIN" || adminLoginSession.USERNAME.ToString().ToUpper() != "PERF" || adminLoginSession.USERNAME.ToString().ToUpper() != "SAI"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {                   
                    string fileLocation = "";
                    string filename = "";
                    if (AM.file != null)
                    {
                        filename = Path.GetFileName(AM.file.FileName);
                    }
                    else
                    {
                        ViewData["Result"] = "-4";
                        ViewBag.Message = "Please select .xls file only";
                        return View();
                    }
                    DataSet ds = new DataSet();
                    if (AM.file.ContentLength > 0)  //(Request.Files["file"].ContentLength > 0
                    {
                        string fileName1 = "FirmResultMIS_" + adminLoginSession.AdminType + adminLoginSession.AdminId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss");  //MIS_201_110720161210

                        string fileExtension = System.IO.Path.GetExtension(AM.file.FileName);
                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            // fileLocation = Server.MapPath("~/BankUpload/") + BM.file.FileName;
                            // fileLocation = Server.MapPath("~/BankUpload/") + BM.file.FileName;
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
                            AM.file.SaveAs(fileLocation);
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
                            string UserNM = adminLoginSession.USERNAME.ToString();
                            switch (UserNM)
                            {
                                case "CREA": UserNM = "CIPL"; break;
                                case "SAI": UserNM = "SAI"; break;
                                case "DATA": UserNM = "DATA"; break;
                                case "PERF": UserNM = "PERF"; break;
                            }
                            DataTable dt1 = ds.Tables[0];
                            dt1.AcceptChanges();
                            // Get Unique and  noe empty records
                            dt1 = dt1.AsEnumerable().GroupBy(x => x.Field<string>("ROLL")).Select(g => g.First()).Where(r => r.ItemArray[1].ToString() != "").CopyToDataTable();

                            DataTable dtexport;

                            string CheckMis = _adminRepository.CheckResultMisExcel(dt1, UserNM, out dtexport);
                            if (CheckMis == "")
                            {
                                if (dt1.Columns.Contains("Status"))
                                {
                                    dt1.Columns.Remove("Status");
                                }                                
                                string OutError = "";
                                DataSet dtResult = _adminRepository.AdminResultUpdateMIS(dt1, adminLoginSession.AdminId, out OutError);// OutStatus mobile
                                if (OutError == "1")
                                {
                                    ViewBag.Message = "File Uploaded Successfully";
                                    ViewData["Result"] = "1";
                                }
                                else
                                {
                                    ViewBag.Message = "File Not Uploaded Successfully : " + OutError.ToString();
                                    ViewData["Result"] = "0";
                                }
                                return View();
                            }
                            else
                            {
                                if (dtexport != null)
                                {
                                    ExportDataFromDataTable(dtexport, "Error_ResultUpdate");
                                }
                                ViewData["Result"] = "-1";
                                ViewBag.Message = CheckMis;
                                return View();
                            }
                        }
                        else
                        {

                            ViewData["Result"] = "-2";
                            ViewBag.Message = "Please Upload Only .xls file only";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //////oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                ViewData["Result"] = "-3";
                ViewBag.Message = ex.Message;
                return View();
            }
            return View();
        }
        #endregion  Admin Result Update MIS


    }
}