using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wenba.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizationAttribute : FilterAttribute,IAuthorizationFilter
    {
        private string _AuthUrl = String.Empty;
        private string _AuthSaveKey = String.Empty;
        private string _AuthSaveType = String.Empty;

        public AuthorizationAttribute()
        {
            String authUrl = System.Configuration.ConfigurationManager.AppSettings["AuthUrl"];
            String saveKey = System.Configuration.ConfigurationManager.AppSettings["AuthSaveKey"];
            String saveType = System.Configuration.ConfigurationManager.AppSettings["AuthSaveType"];

            if (String.IsNullOrEmpty(authUrl))
            {
                this._AuthUrl = "/Login/Login";
            }else
            {
                this._AuthUrl = authUrl;
            }

            if (String.IsNullOrEmpty(saveKey))
            {
                this._AuthSaveKey = "LoginedUser";
            }
            else
            {
                this._AuthSaveKey = saveKey;
            }

            if (String.IsNullOrEmpty(saveType))
            {
                this._AuthSaveType = "Session";
            }else
            {
                this._AuthSaveType = saveType;
            }

        }

        public AuthorizationAttribute(String authUrl) : this()
        {
            this._AuthUrl = authUrl;
        }

        public AuthorizationAttribute(String authUrl,String saveKey) : this()
        {
            this._AuthUrl = authUrl;
            this._AuthSaveKey = saveKey;
        }

        public AuthorizationAttribute(String authUrl, String saveKey,String saveType) : this()
        {
            this._AuthUrl = authUrl;
            this._AuthSaveKey = saveKey;
            this._AuthSaveType = saveType;
        }

        public String AuthUrl
        {
            get { return _AuthUrl.Trim(); }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("用于验证用户登录信息的登录地址不能为空！");
                }
                else
                {
                    _AuthUrl = value.Trim();
                }
            }
        }

        public String AuthSaveKey
        {
            get { return _AuthSaveKey.Trim(); }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("用于保存登陆信息的键名不能为空");
                }
                else
                {
                    _AuthSaveKey = value.Trim();
                }
            }
        }

        public String AuthSaveType
        {
            get { return _AuthSaveType.Trim(); }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("用于保存登陆信息的方式不能为空，只能为【Cookie】或者【Session】！");
                }
                else
                {
                    _AuthSaveType = value.Trim();
                }
            }
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext == null)
            {
                throw new Exception("此特性只适合于Web应用程序使用！");
            }
            else
            {
                switch (AuthSaveType)
                {
                    case "SESSION":
                        if(filterContext.HttpContext.Session == null)
                        {
                            throw new Exception("服务器Session不可用！");
                        }else if(!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute),true)
                            &&!filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute),true)){
                            if(filterContext.HttpContext.Session[_AuthSaveKey] == null)
                            {
                                filterContext.Result = new RedirectResult(_AuthUrl);
                            }
                        }
                        break;
                    case "COOKIE":
                        if (!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                            && !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
                        {
                            if (filterContext.HttpContext.Request.Cookies[_AuthSaveKey] == null)
                            {
                                filterContext.Result = new RedirectResult(_AuthUrl);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentNullException("用于保存登陆信息的方式不能为空，只能为【Cookie】或者【Session】！");                    
                }
            }
        }
    }
}