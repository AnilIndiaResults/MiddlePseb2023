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

namespace PsebPrimaryMiddle.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        //PsebJunior.AbstractLayer._schoolRepository.objDB = new PsebJunior.AbstractLayer._schoolRepository.);
        private readonly ISchoolRepository _schoolRepository;

        public LoginController(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        #region School Login 
        [Route("login")]
        public ActionResult Index()
        {

            if (TempData["result"] != null)
            {
                ViewData["result"] = TempData["result"];
            }
            Session["LoginSession"] = null;
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            Session.Clear();
            //TempData.Clear();
            Session.Abandon();
            Session.RemoveAll();
            try
            {
                ViewBag.SessionList = DBClass.GetSession().ToList();
                return View();
            }
            catch (Exception ex)
            {
                //oErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                return View();
            }
        }


        [Route("login")]
        [HttpPost]
        public async Task<ActionResult> Index(LoginModel lm)
        {
            try
            {
                LoginSession loginSession = await _schoolRepository.CheckLogin(lm); // passing Value to _schoolRepository.from model and Type 1 For regular              
                if (loginSession != null)
                {
                    loginSession.Senior = "0";
                    loginSession.Matric = "0";
                    PsebJunior.AbstractLayer.RegistrationDB objDB = new PsebJunior.AbstractLayer.RegistrationDB();
                    DataSet result = objDB.schooltypes(lm.UserName);
                    if (result.Tables[1].Rows.Count > 0)
                    {
                        ViewBag.Senior = result.Tables[1].Rows[0]["Senior"].ToString();
                        ViewBag.Matric = result.Tables[1].Rows[0]["Matric"].ToString();
                        loginSession.Senior = result.Tables[1].Rows[0]["Senior"].ToString();
                        loginSession.Matric = result.Tables[1].Rows[0]["Matric"].ToString();
                    }

                    loginSession.CurrentSession = lm.Session;
                    TempData["result"] = loginSession.LoginStatus;
                    Session["Session"] = lm.Session.ToString();

                    if (loginSession.LoginStatus == 1)
                    {
                        Session["LoginSession"] = loginSession;

                        if (loginSession.DateFirstLogin == DateTime.MinValue)
                        {
                            return RedirectToAction("Change_Password", "SchoolProfile");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));
                TempData["result"] = "Error : " + ex.Message;
                return RedirectToAction("Index", "Login");
            }
        }


        //[Route("login")]
        //[HttpPost]
        //public ActionResult Index(LoginModel lm)
        //{
        //    try
        //    {               
        //        LoginSession loginSession = _schoolRepository.CheckLogin(lm); // passing Value to _schoolRepository.from model and Type 1 For regular              

        //        loginSession.CurrentSession = lm.Session;
        //        TempData["result"] = loginSession.LoginStatus;

        //        if (loginSession.LoginStatus == 1)
        //        {                
        //            Session["LoginSession"] = loginSession;

        //            if (loginSession.DateFirstLogin == DateTime.MinValue)
        //            {
        //                return RedirectToAction("Change_Password", "SchoolProfile");
        //            }
        //            return RedirectToAction("Index", "Home");
        //        }
        //        return RedirectToAction("Index", "Login");

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteErrorLog(ex.ToString(), Path.GetFileName(Request.Path));                
        //        TempData["result"] = "Error : " + ex.Message;
        //        return RedirectToAction("Index", "Login");
        //    }
        //}






        public ActionResult Logout()
        {
            //foreach (System.Collections.DictionaryEntry entry in HttpContext.Cache)
            //{
            //    HttpContext.Cache.Remove((string)entry.Key);
            //}
            Session["LoginSession"] = null;
            HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Response.Cache.SetNoStore();
            TempData.Clear();
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }

        #endregion School Login 

        #region School ForgotPassword 

        public ActionResult ForgotPassword()
        {
            //if (TempData["result"] != null)
            //{
            //    ViewData["result"] = TempData["result"];
            //}
            ViewBag.SubmitValue = "Send";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(SchoolDataBySchlModel schoolDataBySchlModel)
        {
            ViewBag.SubmitValue = "Send";
            string sid = schoolDataBySchlModel.SCHL;
            schoolDataBySchlModel = await _schoolRepository.GetSchoolDataBySchl(sid);
            if (schoolDataBySchlModel != null)
            {
                TempData["result"] = ViewData["result"] = schoolDataBySchlModel.LoginStatus;
            }
            return View(schoolDataBySchlModel);
        }
        #endregion School ForgotPassword 
    }
}