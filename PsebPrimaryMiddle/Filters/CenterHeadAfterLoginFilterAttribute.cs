using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Collections.Specialized;
using PsebJunior.Models;
using PsebPrimaryMiddle.Repository;
using System.Collections.Generic;
using PsebJunior.AbstractLayer;
using System.Data;

namespace PsebPrimaryMiddle.Filters
{
    public class CenterHeadAfterLoginFilterAttribute : ActionFilterAttribute
    {       
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["CenterHeadLoginSession"] != null)
            {
                CenterHeadLoginSession centerHeadLoginSession = (CenterHeadLoginSession)HttpContext.Current.Session["CenterHeadLoginSession"];
                List<CenterSchoolModel> centerSchoolModels = new List<CenterSchoolModel>();
                string id = string.Empty;
                string schl = string.Empty;
                if (filterContext.ActionParameters.ContainsKey("id"))
                {
                     id = filterContext.ActionParameters["id"].ToString();
                }
                if (filterContext.ActionParameters.ContainsKey("schl"))
                {
                    schl = filterContext.ActionParameters["schl"].ToString();
                }

                bool isSchoolExists=false;
                if (!string.IsNullOrEmpty(schl))
                {
                    DataSet ds  = CenterHeadDB.CheckSchlAllowToCenterHead(0, centerHeadLoginSession.CenterHeadId.ToString(), schl, "");
                    
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                             isSchoolExists = true;
                        }

                    }
                }

                if (!isSchoolExists)
                {
                    filterContext.Result = new RedirectToRouteResult(
                      new RouteValueDictionary
                    {
                        {"controller", "CenterHead"},
                        {"action", "Index"}
                    });
                }

            }

        }
    }
}