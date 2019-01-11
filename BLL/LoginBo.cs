using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wenba.Models;

namespace Wenba.BLL
{
    public class LoginBo
    {
        WenbaDBContext db = new WenbaDBContext();
        public string ValidateUser(string username,string password)
        {

            if (String.IsNullOrEmpty(username))
            {
                return "账号名不能为空，请重新输入！";
            }
            if (String.IsNullOrEmpty(password))
            {
                return "密码不能为空，请重新输入！";
            }
            if (!String.IsNullOrEmpty(username))
            {
                User man = db.Users.FirstOrDefault(p => p.UserName == username);
                if (man == null)
                {
                    return "该账号名错误或者不存在！请重新输入！";
                }
                else {
                    if (man.Password == password)
                    {
                        return "";
                    }
                    else
                    {
                        return "密码错误！请重新输入！";
                    }
                }
            }
            return "";
        }
    }
}