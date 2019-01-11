using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wenba.BLL;
using Wenba.Filters;
using Wenba.Models;

namespace Wenba.Controllers
{
    [AuthorizationAttribute]
    public class UsersController : Controller
    {
        public class viewModel
        {
            public IPagedList<Managerlist> ManagerModel { get; set; }
            public IPagedList<Studentlist> StudentModel { get; set; }


            public viewModel(IPagedList<Managerlist> ManagerList, IPagedList<Studentlist> StudentList)
            {
                this.ManagerModel = ManagerList;
                this.StudentModel = StudentList;
            }
        }
        public class Managerlist
        {
            public int ManagerId;
            public int UserId;
            public string ManagerNum;
            public string ManagerName;
            public string ManagerType;
            public string Gender;
            public string Mobile;
            public string Password;
            public DateTime StartDate;
            public string Role;
            public int PersonId;
            public string UCreateBy;
            public string UCreationDate;
            public string ULastUpdateDate;
            public string ULastUpdated;
            public DateTime? Birthday;
            public DateTime? HireDate;
            public string HeadImage;
            public string Comments;
            public string MCreateBy;
            public string MCreationDate;
            public string MLastUpdateDate;
            public string MLastUpdated;
        }
        public class Studentlist
        {
            public int StudentId;
            public int UserId;
            public int ProjectId;
            public string StudentName;
            public string StudentNum;
            public string ProjectName;
            public string Gender;
            public string Mobile;
            public string Password;
            public DateTime StartDate;
            public DateTime? Birthday;
            public string Collegue;
            public string Nationality;
            public string HeadImage;
            public string SCreateBy;
            public string SCreationDate;
            public string SLastUpdateDate;
            public string SLastUpdated;
            public string Comments;
        }


        private WenbaDBContext db = new WenbaDBContext();
        // GET: Users
        [CustAuthorize("A")]
        public ActionResult UsersList(string username, string sex, string mobile, string managernum, string usernames, string sexs, string mobiles, int? page, int? pages, string projectId1, string projectId2)
        {
            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            List<Project> pList = new List<Project>();
            pList = db.Projects.ToList();
            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList1"] = projectList;
            //初始化项目名称列表
            List<SelectListItem> projectList2 = new List<SelectListItem>();
            List<Project> pList2 = new List<Project>();
            pList2 = db.Projects.ToList();
            foreach (var item in pList2)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList2.Add(listItem);
            }
            ViewData["ProjectList2"] = projectList2;

            //项目经理列表
            var list = from Users in db.Users
                       join Managers in db.Managers on Users.PersonId equals Managers.id
                       where Users.Role == "M"
                       select new Managerlist
                       {
                           ManagerId = Managers.id,
                           UserId = Users.id,
                           ManagerNum = Managers.ManagerNum,
                           ManagerName = Managers.ManagerName,
                           Gender = Managers.Gender,
                           Mobile = Managers.Mobile,
                           ManagerType = Managers.ManagerType
                       };
            //第几页
            int pageNumber = page ?? 1;
            //每页显示多少条
            int pageSize = int.Parse(ConfigurationManager.AppSettings["pageSize"]);
            //根据ID排序
            list = list.OrderByDescending(x => x.ManagerId);
            if (!String.IsNullOrEmpty(username))
            {
                list = list.Where(x => x.ManagerName.Contains(username));
            }
            if (!String.IsNullOrEmpty(sex) && sex != "全部")
            {
                list = list.Where(x => x.Gender.Contains(sex));
            }
            if (!String.IsNullOrEmpty(mobile))
            {
                list = list.Where(x => x.Mobile.Contains(mobile));
            }
            if (!String.IsNullOrEmpty(managernum))
            {
                list = list.Where(x => x.ManagerNum.Contains(managernum));
            }

            if (!String.IsNullOrEmpty(projectId1))
            {
                int proId = Convert.ToInt32(projectId1);
                var project = db.Projects.Where(x => x.id == proId).FirstOrDefault();
                list = list.Where(x => x.ManagerId.Equals(project.ManagerId) || x.ManagerId.Equals(project.CustManagerId));
            }

            //通过ToPagedList扩展方法进行分页
            IPagedList<Managerlist> pagedList = list.ToPagedList(pageNumber, pageSize);
            //学生列表
            var lists = from Students in db.Students
                        join Users in db.Users on Students.id equals Users.PersonId
                        join StudentAssgins in db.StudentAssgins on Students.id equals StudentAssgins.StudentId
                        join Projects in db.Projects on StudentAssgins.ProjectId equals Projects.id
                        where Users.Role == "S"
                        select new Studentlist
                        {
                            StudentId = Students.id,
                            UserId = Users.id,
                            ProjectId = Projects.id,
                            StudentName = Students.StudentName,
                            StudentNum = Students.StudentNum,
                            ProjectName = Projects.ProjectName,
                            Gender = Students.Gender,
                            Mobile = Students.Mobile,
                        };
            //第几页
            int pageNumbers = pages ?? 1;
            //每页显示多少条
            int pageSizes = int.Parse(ConfigurationManager.AppSettings["pageSize"]);
            //根据ID排序
            lists = lists.OrderByDescending(x => x.StudentId);
            if (!String.IsNullOrEmpty(usernames))
            {
                lists = lists.Where(x => x.StudentName.Contains(usernames));
            }
            if (!String.IsNullOrEmpty(sexs) && sexs != "全部")
            {
                lists = lists.Where(x => x.Gender.Contains(sexs));
            }
            if (!String.IsNullOrEmpty(mobiles))
            {
                lists = lists.Where(x => x.Mobile.Contains(mobiles));
            }

            if (!String.IsNullOrEmpty(projectId2))
            {
                int proId = Convert.ToInt32(projectId2);    
                lists = lists.Where(x => x.ProjectId.Equals(proId));
            }

            //通过ToPagedList扩展方法进行分页
            IPagedList<Studentlist> pagedLists = lists.ToPagedList(pageNumbers, pageSizes);
            //将分页处理后的列表传给View
            var vm = new viewModel(pagedList, pagedLists);

            this.ViewData["username"] = username;
            this.ViewData["mobile"] = mobile;
            this.ViewData["sex"] = sex;
            this.ViewData["managernum"] = managernum;
            this.ViewData["projectId1"] = projectId1;

            this.ViewData["usernames"] = usernames;
            this.ViewData["projectId2"] = projectId2;
            this.ViewData["sexs"] = sexs;
            this.ViewData["mobiles"] = mobiles;
            return View(vm);
        }

        [CustAuthorize("A")]
        public ActionResult UserDetail(int id)
        {
            var user = from Users in db.Users
                       join Managers in db.Managers on Users.PersonId equals Managers.id
                       select new Managerlist
                       {
                           ManagerId = Managers.id,
                           UserId = Users.id,
                           ManagerNum = Managers.ManagerNum,
                           ManagerName = Managers.ManagerName,
                           Gender = Managers.Gender,
                           Mobile = Managers.Mobile,
                           Password = Users.Password,
                           StartDate = Users.StartDate,
                           Role = Users.Role,
                           PersonId = Users.PersonId,
                           Birthday = Managers.Birthday,
                           HireDate = Managers.HireDate,
                           ManagerType = Managers.ManagerType,
                           HeadImage = Managers.HeadImage,
                           Comments = Managers.Comments,

                       };
            var a = user.Where(x => x.ManagerId == id).FirstOrDefault();
            Managerlist userone = new Managerlist();
            userone.ManagerId = a.ManagerId;
            userone.UserId = a.ManagerId;
            userone.ManagerNum = a.ManagerNum;
            userone.ManagerName = a.ManagerName;
            if (a.Gender == "0")
                userone.Gender = "男";
            if (a.Gender == "1")
                userone.Gender = "女";
            userone.Mobile = a.Mobile;
            userone.Password = a.Password;
            userone.StartDate = a.StartDate;
            userone.Role = a.Role;
            userone.PersonId = a.PersonId;
            userone.Birthday = a.Birthday;
            userone.HireDate = a.HireDate;
            if (a.ManagerType == "M")
                userone.ManagerType = "项目经理";
            if (a.ManagerType == "C")
                userone.ManagerType = "客户经理";
            userone.HeadImage = a.HeadImage;
            userone.Comments = a.Comments;
            return View(userone);
        }

        [CustAuthorize("A")]
        public ActionResult UserEdit(int id)
        {
            var user = from Users in db.Users
                       join Managers in db.Managers on Users.PersonId equals Managers.id
                       select new Managerlist
                       {
                           ManagerId = Managers.id,
                           UserId = Users.id,
                           ManagerNum = Managers.ManagerNum,
                           ManagerName = Managers.ManagerName,
                           Gender = Managers.Gender,
                           Mobile = Managers.Mobile,
                           Password = Users.Password,
                           StartDate = Users.StartDate,
                           Role = Users.Role,
                           PersonId = Users.PersonId,
                           Birthday = Managers.Birthday,
                           HireDate = Managers.HireDate,
                           ManagerType = Managers.ManagerType,
                           HeadImage = Managers.HeadImage,
                           Comments = Managers.Comments,

                       };
            var a = user.Where(x => x.ManagerId == id).FirstOrDefault();
            Managerlist userone = new Managerlist();
            userone.ManagerId = a.ManagerId;
            userone.UserId = a.ManagerId;
            userone.ManagerNum = a.ManagerNum;
            userone.ManagerName = a.ManagerName;
            userone.Gender = a.Gender;
            userone.Mobile = a.Mobile;
            userone.Password = a.Password;
            userone.StartDate = a.StartDate;
            userone.Role = a.Role;
            userone.PersonId = a.PersonId;
            userone.Birthday = a.Birthday;
            userone.HireDate = a.HireDate;
            userone.ManagerType = a.ManagerType;
            userone.HeadImage = a.HeadImage;
            userone.Comments = a.Comments;
            if (a.Birthday != null)
                ViewData["Birthday"] = Convert.ToDateTime(a.Birthday).ToString("yyyy-MM-dd");
            else
                ViewData["Birthday"] = null;
            if (a.HireDate != null)
                ViewData["HireDate"] = Convert.ToDateTime(a.HireDate).ToString("yyyy-MM-dd");
            else
                ViewData["HireDate"] = null;

            return View(userone);
        }

        [CustAuthorize("A")]
        public ActionResult UserAdd()
        {
            return View();
        }
        UsersBo usersBO = new UsersBo();
        [HttpPost]
        public ActionResult UserAdd(Manager manager)
        {

            try
            { //暂时为固定值
                manager.CreatedBy = Convert.ToInt32(UserLogin.userid);
                manager.CreationDate = System.DateTime.Now;
                manager.LastUpdateDate = System.DateTime.Now;
                //暂时为固定值
                manager.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);
                manager.ActiveFlag = "Y";
                manager.HeadImage = "/Content/images/mryhtx.jpg";
                db.Managers.Add(manager);

                string msg = usersBO.ValidateUser(manager);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    //return View(manager);
                    return Content("<script >alert('" + msg + "'); window.history.back();</script >", "text/html");
                }
                else
                    db.SaveChanges();

                User user = new Models.User();
                user.UserName = manager.ManagerNum;
                user.Password = "123456";
                user.StartDate = DateTime.Now;
                user.Role = "M";
                user.PersonId = manager.id;
                //暂时为固定值
                user.CreatedBy = Convert.ToInt32(UserLogin.userid);
                user.CreationDate = DateTime.Now;
                //暂时为固定值
                user.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);
                user.LastUpdateDate = DateTime.Now;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("UsersList");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }


        [HttpPost]
        public ActionResult UserSave(FormCollection fc)
        {
            try
            {

                HttpPostedFileBase File = Request.Files["file"];
                string FileName = File.FileName; //上传的原文件名
                string guid = "";
                String path = Server.MapPath("~/upload/");
                if (FileName != null && FileName != "")
                {
                    string FileType = FileName.Substring(FileName.LastIndexOf(".") + 1); //得到文件的后缀名
                    guid = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + "." + FileType; //得到重命名的文件名

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.SaveAs(path + guid); //保存操作
                }

                int ManagerId = Convert.ToInt32(fc["ManagerId"]);
                var manager = db.Managers.Where(x => x.id == ManagerId).FirstOrDefault();
                if (String.IsNullOrEmpty(guid))
                {
                    manager.HeadImage = manager.HeadImage;
                }
                else
                {
                    manager.HeadImage = "/upload/" + guid;
                }
                manager.ManagerName = fc["ManagerName"];
                manager.ManagerNum = fc["ManagerNum"];
                manager.Gender = fc["Gender"];
                manager.Mobile = fc["Mobile"];
                if (!String.IsNullOrEmpty(fc["Birthday"]))
                {
                    DateTime? birthday = Convert.ToDateTime(fc["Birthday"]);
                    manager.Birthday = birthday;
                }
                if (!String.IsNullOrEmpty(fc["HireDate"]))
                {
                    DateTime? hiredate = Convert.ToDateTime(fc["HireDate"]);
                    manager.HireDate = hiredate;
                }
                manager.Comments = fc["Comments"];
                manager.ManagerType = fc["ManagerType"];
                //暂时为固定值
                manager.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);
                manager.LastUpdateDate = DateTime.Now;

                db.Managers.Attach(manager);
                db.Entry<Manager>(manager).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                string msg = usersBO.ValidateUser(manager);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    return Content("<script >alert('" + msg + "'); window.history.back();</script >", "text/html");
                }
                else
                    db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;


                var user = db.Users.Where(x => x.PersonId == ManagerId).FirstOrDefault();
                user.UserName = fc["ManagerNum"];
                user.Password = fc["Password"];
                user.LastUpdateDate = DateTime.Now;
                //暂时为固定值
                user.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);

                db.Users.Attach(user);
                db.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                return RedirectToAction("UsersList", "Users");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
        [CustAuthorize("A")]
        public ActionResult StudentDetail(int id)
        {
            var user = from Students in db.Students
                       join Users in db.Users on Students.id equals Users.PersonId
                       join StudentAssgins in db.StudentAssgins on Students.id equals StudentAssgins.StudentId
                       join Projects in db.Projects on StudentAssgins.ProjectId equals Projects.id
                       where Users.Role == "S"
                       select new Studentlist
                       {
                           StudentId = Students.id,
                           UserId = Users.id,
                           ProjectId = Projects.id,
                           StudentName = Students.StudentName,
                           StudentNum = Students.StudentNum,
                           ProjectName = Projects.ProjectName,
                           Gender = Students.Gender,
                           Mobile = Students.Mobile,
                           Password = Users.Password,
                           StartDate = Users.StartDate,
                           Birthday = Students.Birthday,
                           Collegue = Students.Collegue,
                           Nationality = Students.Nationality,
                           HeadImage = Students.HeadImage,
                           Comments = Students.Comments
                       };

            var a = user.Where(x => x.StudentId == id).FirstOrDefault();
            Studentlist userone = new Studentlist();
            userone.StudentId = a.StudentId;
            userone.UserId = a.UserId;
            userone.ProjectId = a.ProjectId;
            userone.StudentName = a.StudentName;
            userone.StudentNum = a.StudentNum;
            userone.ProjectName = a.ProjectName;
            if (a.Gender == "0")
                userone.Gender = "男";
            if (a.Gender == "1")
                userone.Gender = "女";
            userone.Mobile = a.Mobile;
            userone.Password = a.Password;
            userone.StartDate = a.StartDate;
            userone.Birthday = a.Birthday;
            userone.Collegue = a.Collegue;
            userone.Nationality = a.Nationality;
            userone.HeadImage = a.HeadImage;
            userone.Comments = a.Comments;
            return View(userone);
        }

        [CustAuthorize("A")]
        public ActionResult StudentEdit(int id)
        {
            var user = from Students in db.Students
                       join Users in db.Users on Students.id equals Users.PersonId
                       join StudentAssgins in db.StudentAssgins on Students.id equals StudentAssgins.StudentId
                       join Projects in db.Projects on StudentAssgins.ProjectId equals Projects.id
                       where Users.Role == "S"
                       select new Studentlist
                       {
                           StudentId = Students.id,
                           UserId = Users.id,
                           ProjectId = Projects.id,
                           StudentName = Students.StudentName,
                           StudentNum = Students.StudentNum,
                           ProjectName = Projects.ProjectName,
                           Gender = Students.Gender,
                           Mobile = Students.Mobile,
                           Password = Users.Password,
                           StartDate = Users.StartDate,
                           Birthday = Students.Birthday,
                           Collegue = Students.Collegue,
                           Nationality = Students.Nationality,
                           HeadImage = Students.HeadImage,
                           Comments = Students.Comments
                       };

            var a = user.Where(x => x.StudentId == id).FirstOrDefault();
            Studentlist userone = new Studentlist();
            userone.StudentId = a.StudentId;
            userone.UserId = a.UserId;
            userone.ProjectId = a.ProjectId;
            userone.StudentName = a.StudentName;
            userone.StudentNum = a.StudentNum;
            userone.ProjectName = a.ProjectName;
            userone.Gender = a.Gender;
            userone.Mobile = a.Mobile;
            userone.Password = a.Password;
            userone.StartDate = a.StartDate;
            userone.Birthday = a.Birthday;
            userone.Collegue = a.Collegue;
            userone.Nationality = a.Nationality;
            userone.HeadImage = a.HeadImage;
            userone.Comments = a.Comments;

            //初始化项目名称下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            foreach (var item in db.Projects)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;
            ViewData["ProjectId"] = a.ProjectId;
            if (a.Birthday != null)
                ViewData["Birthday"] = Convert.ToDateTime(a.Birthday).ToString("yyyy-MM-dd");
            else
                ViewData["Birthday"] = null;
            return View(userone);
        }

        [HttpPost]
        public ActionResult StudentSave(FormCollection fc)
        {
            try
            {
                HttpPostedFileBase File = Request.Files["file"];
                string FileName = File.FileName; //上传的原文件名
                string guid = "";
                String path = Server.MapPath("~/upload/");
                if (FileName != null && FileName != "")
                {
                    string FileType = FileName.Substring(FileName.LastIndexOf(".") + 1); //得到文件的后缀名
                    guid = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + "." + FileType; //得到重命名的文件名

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    File.SaveAs(path + guid); //保存操作
                }



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
                student.StudentNum = fc["StudentNum"];
                student.Gender = fc["Gender"];
                student.Mobile = fc["Mobile"];
                if (!String.IsNullOrEmpty(fc["Birthday"]))
                {
                    DateTime? birthday = Convert.ToDateTime(fc["Birthday"]);
                    student.Birthday = birthday;
                }
                student.Collegue = fc["Collegue"];
                student.Comments = fc["Comments"];
                student.Nationality = fc["Nationality"];
                //暂时为固定值
                student.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);
                student.LastUpdateDate = DateTime.Now;

                db.Students.Attach(student);
                db.Entry<Student>(student).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                string msg = usersBO.ValidateStudent(student);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    return Content("<script >alert('" + msg + "'); window.history.back();</script >", "text/html");
                }
                else
                    db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;


                var user = db.Users.Where(x => x.PersonId == StudentId).FirstOrDefault();
                user.Password = fc["Password"];
                user.UserName = fc["StudentNum"];
                user.LastUpdateDate = DateTime.Now;
                //暂时为固定值
                user.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);

                db.Users.Attach(user);
                db.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;

                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                var assgins = db.StudentAssgins.Where(x => x.StudentId == StudentId).FirstOrDefault();
                assgins.ProjectId = Convert.ToInt32(fc["ProjectId"]);
                assgins.LastUpdateDate = DateTime.Now;
                //暂时为固定值
                assgins.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);
                db.StudentAssgins.Attach(assgins);
                db.Entry<StudentAssgin>(assgins).State = System.Data.Entity.EntityState.Modified;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                return RedirectToAction("UsersList", "Users");
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }
    }
}