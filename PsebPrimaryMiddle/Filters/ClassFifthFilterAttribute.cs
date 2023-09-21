using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Collections.Specialized;
using PsebJunior.Models;

namespace PsebPrimaryMiddle.Filters
{   
    public class ClassFifthFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (HttpContext.Current.Session["LoginSession"] != null)
            {
                LoginSession loginSession = (LoginSession)HttpContext.Current.Session["LoginSession"];
                if (loginSession.fifth == "N")
                {
                    filterContext.Result = new RedirectToRouteResult(
                                          new RouteValueDictionary
                                        {
                        {"controller", "Login"},
                        {"action", "Index"}
                                        });

                } 
            }

        }
    }
}