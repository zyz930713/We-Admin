using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wenba.BLL;
using Wenba.Filters;
using Wenba.Models;
using Wenba.ViewModels;

namespace Wenba.Controllers
{
    [AuthorizationAttribute]
    public class PersonalController : Controller
    {
        private WenbaDBContext db = new WenbaDBContext();

        //public int id = Convert.ToInt32(UserLogin.userid);     //从系统session来

        [CustAuthorize("A", "M", "C", "S")]
        // GET: Personal
        public ActionResult Index()
        {
            if (UserLogin.userid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int id = Convert.ToInt32(UserLogin.userid);     //从系统session来          
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            CompositePersonal person = new CompositePersonal();
            person.User = user;
            Manager manager = db.Managers.FirstOrDefault(m => m.id == user.PersonId);
            person.Manager = manager;
            Student student = db.Students.FirstOrDefault(p => p.id == user.PersonId);
            person.Student = student;
            return View(person);

        }


        [HttpPost]
        [CustAuthorize("A", "M", "C", "S")]
        public ActionResult Save(FormCollection fc)
        {
            try
            {
                HttpPostedFileBase File = Request.Files["file"];
                string FileName = File.FileName; //上传的原文件名
                string guid = "";

                String path = Server.MapPath("~/upload/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (FileName != null && FileName != "")
                {
                    string FileType = FileName.Substring(FileName.LastIndexOf(".") + 1); //得到文件的后缀名
                    guid = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + "." + FileType; //得到重命名的文件名

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.SaveAs(path + guid); //保存操作
                }


                int id = Convert.ToInt32(UserLogin.userid);     //从系统session来
                var user = db.Users.Where(x => x.id == id).FirstOrDefault();

                if (user.Role.Contains('M')|| user.Role.Contains('A'))
                {
                    int ManagerId = Convert.ToInt32(fc["ManagerId"]);
                    var Manager = db.Managers.Where(x => x.id == ManagerId).FirstOrDefault();
                    if (String.IsNullOrEmpty(guid))
                    {
                        Manager.HeadImage = Manager.HeadImage;
                    }
                    else
                    {
                        Manager.HeadImage = "/upload/" + guid;
                    }
                    Manager.ManagerName = fc["ManagerName"];
                    Manager.Mobile = fc["m_Mobile"];
                    Manager.Comments = fc["m_Comments"];


                    Manager.LastUpdatedBy = id;
                    Manager.LastUpdateDate = DateTime.Now;

                    db.Managers.Attach(Manager);
                    db.Entry<Manager>(Manager).State = System.Data.Entity.EntityState.Modified;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;

                    //及时刷新页面头像
                    UserLogin.userhead = Manager.HeadImage;
                }

                if (user.Role.Contains('S'))
                {
                    int StudentId = Convert.ToInt32(fc["StudentId"]);
                    var student = db.Students.Where(x => x.id == StudentId).FirstOrDefault();
                    if (String.IsNullOrEmpty(guid))
                    {
                        student.HeadImage = student.HeadImage;
                    }
                    else
                    {
                        student.HeadImage = "/upload/" + guid;
                    }
                    student.StudentName = fc["StudentName"];
                    student.Mobile = fc["s_Mobile"];
                    student.Comments = fc["s_Comments"];


                    student.LastUpdatedBy = id;
                    student.LastUpdateDate = DateTime.Now;

                    db.Students.Attach(student);
                    db.Entry<Student>(student).State = System.Data.Entity.EntityState.Modified;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.SaveChanges();
                    db.Configuration.ValidateOnSaveEnabled = true;

                    //及时刷新页面头像
                    UserLogin.userhead = student.HeadImage;

                }

                //user.Password = fc["Password"];

                user.LastUpdateDate = DateTime.Now;
                //暂时为固定值
                user.LastUpdatedBy = id;

                db.Users.Attach(user);
                db.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;

                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;

                return RedirectToAction("Index", "Personal");
            }

            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }


        // GET: Users/Edit/5
        [CustAuthorize("A", "M", "C", "S")]
        public ActionResult ChangePwd(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View();
        }



        // POST: Courses/Edit/5
        [CustAuthorize("A", "M", "C", "S")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePwd(FormCollection fc)
        {
            int userid = Convert.ToInt32(fc["id"]);
            var user = db.Users.Find(userid);
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(user).State = EntityState.Modified;
                    var entry = db.Entry(user);
                    //以下字段不更新
                    entry.Property(e => e.CreationDate).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    entry.Property(e => e.Attribute1).IsModified = false;
                    entry.Property(e => e.Attribute2).IsModified = false;
                    entry.Property(e => e.Attribute3).IsModified = false;
                    entry.Property(e => e.Attribute4).IsModified = false;
                    entry.Property(e => e.Attribute5).IsModified = false;
                    entry.Property(e => e.Attribute6).IsModified = false;
                    entry.Property(e => e.Attribute7).IsModified = false;
                    entry.Property(e => e.Attribute8).IsModified = false;
                    entry.Property(e => e.Attribute9).IsModified = false;
                    entry.Property(e => e.Attribute10).IsModified = false;
                    entry.Property(e => e.UserName).IsModified = false;
                    entry.Property(e => e.Comments).IsModified = false;
                    entry.Property(e => e.StartDate).IsModified = false;
                    entry.Property(e => e.EndDate).IsModified = false;
                    entry.Property(e => e.Role).IsModified = false;
                    entry.Property(e => e.PersonId).IsModified = false;

                    user.LastUpdateDate = DateTime.Now;
                    user.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);      //从系统获取
                    

                    //Validate  before commit to Database
                    string str = fc["oldPwd"];
                    if (user.Password == fc["oldPwd"])
                    {
                        user.Password = fc["newPwd"];
                        db.SaveChanges();
                        db.Configuration.ValidateOnSaveEnabled = true;
                        return RedirectToAction("Login", "Login");
                    }
                    else
                    {
                        return Content("<script >alert('原密码输入错误！'); window.history.back();</script >", "text/html");
                    }

                }
            }
            catch (Exception ex)
            {
                
                return View(user);
            }
            return View(user);

        }


        // GET: log off
        [CustAuthorize("A", "M", "C", "S")]
        public ActionResult LogOff()
        {
            UserLogin.userid = null;
            UserLogin.loginname = null;
            UserLogin.managetype = null;
            UserLogin.username = null;
            UserLogin.userrole = null;
            UserLogin.userroles = null;

            return RedirectToAction("Login", "Login");
        }

    }
}