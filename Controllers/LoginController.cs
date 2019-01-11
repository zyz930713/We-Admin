using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wenba.BLL;
using Wenba.Models;

namespace Wenba.Controllers
{
    public class LoginController : Controller
    {
       
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }
        LoginBo loginBO = new LoginBo();
        WenbaDBContext db = new WenbaDBContext();
        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            string msg = loginBO.ValidateUser(fc["username"], fc["userpassword"]);
            if (!String.IsNullOrEmpty(msg))
            {
                ViewBag.ErrorMsg = msg;
                return Content("<script >alert('" + msg + "'); window.history.back();</script >", "text/html");
            }
            else
            {
                string username = fc["username"];
                User man = db.Users.FirstOrDefault(p => p.UserName == username);
                if (man.Role == "S")
                {
                    Student student = db.Students.FirstOrDefault(p => p.id == man.PersonId);
                    UserLogin.username = student.StudentName;
                    UserLogin.userroles = "S";
                    UserLogin.userhead = student.HeadImage;
                }
               
                if(man.Role == "M"|| man.Role == "A")
                {
                    Manager manage = db.Managers.FirstOrDefault(p => p.id == man.PersonId);
                    UserLogin.username = manage.ManagerName;
                    UserLogin.managetype = manage.ManagerType;
                    UserLogin.userroles = manage.ManagerType;
                    UserLogin.userhead = manage.HeadImage;
                }
                UserLogin.loginname= fc["username"];
                UserLogin.userrole = man.Role;
                UserLogin.userid = man.id.ToString();
                return RedirectToAction("Index", "Index");
            }
               
        }
    }
}