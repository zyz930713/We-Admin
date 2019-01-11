using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wenba.BLL
{
    public class UserLogin
    {
        /// <summary>
        /// 账号
        /// </summary>
        public static String loginname
        {
            get
            {
                Object loginname = HttpContext.Current.Session["loginname"];


                if (loginname != null)
                {
                    return loginname.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["loginname"] = value;
            }
        }

        /// <summary>
        /// 用户身份
        /// </summary>
        public static String userrole
        {
            get
            {
                Object userrole = HttpContext.Current.Session["userrole"];

                if (userrole != null)
                {
                    return userrole.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["userrole"] = value;
            }
        }

        /// <summary>
        /// 用户自定义身份
        /// </summary>
        public static String userroles
        {
            get
            {
                Object userroles = HttpContext.Current.Session["userroles"];

                if (userroles != null)
                {
                    return userroles.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["userroles"] = value;
            }
        }



        /// <summary>
        /// 用户id
        /// </summary>
        public static String userid
        {
            get
            {
                Object userid = HttpContext.Current.Session["userid"];


                if (userid != null)
                {
                    return userid.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["userid"] = value;
            }
        }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public static String username
        {
            get
            {
                Object username = HttpContext.Current.Session["username"];

                if (username != null)
                {
                    return username.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["username"] = value;
            }
        }
        /// 用户头像
        /// </summary>
        public static String userhead
        {
            get
            {
                Object userhead = HttpContext.Current.Session["userhead"];

                if (userhead != null)
                {
                    return userhead.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["userhead"] = value;
            }
        }


        /// <summary>
        /// 用户类别
        /// </summary>
        public static String managetype
        {
            get
            {
                Object managetype = HttpContext.Current.Session["managetype"];

                if (managetype != null)
                {
                    return managetype.ToString();
                }
                return "";
            }
            set
            {
                HttpContext.Current.Session["managetype"] = value;
            }
        }
    }
}