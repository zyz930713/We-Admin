using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wenba.BLL;

namespace Wenba.Filters
{
    public class CustAuthorizeAttribute : AuthorizeAttribute
    {
        private string[] roles;

        public CustAuthorizeAttribute(params string[] role)
        {
            roles = role;
        }
     
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var role = UserLogin.userroles;
            if (UserLogin.userid != null)
            {
                return roles.Contains(role.ToString());
            }
            return base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            UrlHelper url = new UrlHelper(filterContext.RequestContext);
            filterContext.Result = new RedirectResult("~/Login/Login");
        }
    }
}