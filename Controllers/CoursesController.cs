using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wenba.Models;
using Wenba.ViewModels;
using PagedList;
using System.Configuration;
using Wenba.BLL;
using Wenba.Filters;

namespace Wenba.Controllers
{
    [AuthorizationAttribute]
    public class CoursesController : Controller
    {
        private WenbaDBContext db = new WenbaDBContext();

        [CustAuthorize("A", "M")]
        // GET: CoursesList
        public ActionResult CoursesList(string coursename, string TeacherId, string ProjectId, string startDate, string endDate, int? page)
        {
            //使用linq查询所有的课程信息。
            //var courses = from s in db.Courses
            //                   //where 
            //                   orderby s.ProjectId,s.CourseNum
            //               select s;


            //foreach (var cou in courses)
            //{
            //    var project = from c in db.Projects
            //                 where c.id == cou.ProjectId
            //                 select c;
            //    cou.Project = project.FirstOrDefault();

            //    var teacher = from t in db.Managers
            //                  where t.id == cou.TeacherId
            //                  select t;
            //    cou.Teacher = teacher.FirstOrDefault();
            //}

            //初始化任课老师列表
            List<SelectListItem> teacherList = new List<SelectListItem>();
            //teacherList.Add(new SelectListItem() { Text = "所有老师", Value = "NULL" });          
            foreach (var item in db.Managers.Where(p => p.ManagerType == "T"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                teacherList.Add(listItem);
            }
            ViewData["TeacherList"] = teacherList;

            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.ToList();
            }
            else
            {
                pList = db.Projects.ToList();
            }

            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            //List<Course> courseList = db.Courses.OrderByDescending(c => c.ProjectId).ThenBy(c => c.CourseNum).ToList();
            //根据用户身份做判断
            List<Course> courseList = new List<Course>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var courses = from Courses in db.Courses
                              join Projects in db.Projects on Courses.ProjectId equals Projects.id
                              where Projects.ManagerId == user.PersonId
                              select Courses;
                courseList = courses.OrderBy(c => c.CourseNum).ToList();

            }
            else
            {
                courseList = db.Courses.OrderByDescending(c => c.ProjectId).ThenBy(c => c.CourseNum).ToList();
            }

            List<CompositeCourse> ccourseList = new List<CompositeCourse>();

            foreach (var course in courseList)
            {
                CompositeCourse ccourse = new CompositeCourse();
                ccourse.Course = course;
                Project project = db.Projects.FirstOrDefault(p => p.id == course.ProjectId);
                ccourse.Project = project;
                Manager teacher = db.Managers.FirstOrDefault(m => m.id == course.TeacherId);
                ccourse.Teacher = teacher;

                ccourseList.Add(ccourse);
            }

            //第几页
            int pageNumbers = page ?? 1;
            //每页显示多少条
            int pageSizes = int.Parse(ConfigurationManager.AppSettings["pageSize"]);

            //筛选条件
            if (!String.IsNullOrEmpty(coursename))
            {
                ccourseList = ccourseList.Where(x => x.Course.CourseName.Contains(coursename)).ToList();
            }
            if (!String.IsNullOrEmpty(TeacherId) && TeacherId != "所有老师")
            {
                ccourseList = ccourseList.Where(x => x.Course.TeacherId.Equals(Convert.ToInt32(TeacherId))).ToList();
            }
            if (!String.IsNullOrEmpty(ProjectId) && ProjectId != "所有项目")
            {
                ccourseList = ccourseList.Where(x => x.Course.ProjectId.Equals(Convert.ToInt32(ProjectId))).ToList();
            }

            if (!String.IsNullOrEmpty(startDate))
            {
                ccourseList = ccourseList.Where(p => p.Course.StartDate >= Convert.ToDateTime(startDate)).ToList();
            }
            if (!String.IsNullOrEmpty(endDate))
            {
                ccourseList = ccourseList.Where(p => p.Course.EndDate <= Convert.ToDateTime(endDate).AddDays(1)).ToList();
            }
            //if (!String.IsNullOrEmpty(site))
            //{
            //    ccourseList = ccourseList.Where(x => !string.IsNullOrEmpty(x.Course.Site)&& x.Course.Site.Contains(site)).ToList();
            //}

            //通过ToPagedList扩展方法进行分页
            IPagedList<CompositeCourse> pagedList = ccourseList.ToPagedList(pageNumbers, pageSizes);

            //将分页处理后的列表传给View
            this.ViewData["coursename"] = coursename;
            this.ViewData["TeacherId"] = TeacherId;
            this.ViewData["ProjectId"] = ProjectId;
            this.ViewData["StartDate"] = startDate;
            this.ViewData["EndDate"] = endDate;
            //this.ViewData["site"] = site;

            return View(pagedList);
            //return View(ccourseList);
        }

        // GET: Courses/Details/5
        [CustAuthorize("A", "M")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }

            CompositeCourse ccourse = new CompositeCourse();
            ccourse.Course = course;
            Project project = db.Projects.FirstOrDefault(p => p.id == course.ProjectId);
            ccourse.Project = project;
            Manager teacher = db.Managers.FirstOrDefault(m => m.id == course.TeacherId);
            ccourse.Teacher = teacher;

            return View(ccourse);
        }

        // GET: Courses/Create
        [CustAuthorize("A", "M")]
        public ActionResult Create()
        {
            //初始化任课老师列表
            List<SelectListItem> teacherList = new List<SelectListItem>();
            foreach (var item in db.Managers.Where(p => p.ManagerType == "T"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                teacherList.Add(listItem);
            }
            ViewData["TeacherList"] = teacherList;

            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.Where(p => p.Status != "C").ToList();
            }
            else
            {
                pList = db.Projects.Where(p => p.Status != "C").ToList();
            }

            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [CustAuthorize("A", "M")]
        public ActionResult Create(Course course)
        {
            //初始化任课老师列表
            List<SelectListItem> teacherList = new List<SelectListItem>();
            foreach (var item in db.Managers.Where(p => p.ManagerType == "T"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                teacherList.Add(listItem);
            }
            ViewData["TeacherList"] = teacherList;

            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.Where(p => p.Status != "C").ToList();
            }
            else
            {
                pList = db.Projects.Where(p => p.Status != "C").ToList();
            }

            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            CoursesBO courseBO = new CoursesBO();
            try
            {
                course.CreatedBy = Convert.ToInt32(UserLogin.userid);    //从系统获取
                course.CreationDate = System.DateTime.Now;
                course.LastUpdatedate = System.DateTime.Now;
                course.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);     //从系统获取
                db.Courses.Add(course);              

                string msg = courseBO.ValidateCourse(course);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    return View(course);
                }
                db.SaveChanges();
                return RedirectToAction("CoursesList");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }
        }

        // GET: Courses/Edit/5
        [CustAuthorize("A", "M")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }

            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.Where(p => p.Status != "C").ToList();
            }
            else
            {
                pList = db.Projects.Where(p => p.Status != "C").ToList();
            }

            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            //初始化任课老师列表
            List<SelectListItem> teacherList = new List<SelectListItem>();
            foreach (var item in db.Managers.Where(p => p.ManagerType == "T"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                teacherList.Add(listItem);
            }
            ViewData["TeacherList"] = teacherList;

            return View(course);
        }

        // POST: Courses/Edit/5
        [CustAuthorize("A", "M")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Course course)
        {
            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.Where(p => p.Status != "C").ToList();
            }
            else
            {
                pList = db.Projects.Where(p => p.Status != "C").ToList();
            }

            foreach (var item in pList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            //初始化任课老师列表
            List<SelectListItem> teacherList = new List<SelectListItem>();
            foreach (var item in db.Managers.Where(p => p.ManagerType == "T"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                teacherList.Add(listItem);
            }
            ViewData["TeacherList"] = teacherList;

            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(course).State = EntityState.Modified;
                    var entry = db.Entry(course);
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
                    //entry.Property(e => e.ProjectId).IsModified = false;

                    course.LastUpdatedate = DateTime.Now;
                    course.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);      //从系统获取

                    //Validate Projects before commit to Database
                    CoursesBO courseBO = new CoursesBO();
                    string errorMsg = "";
                    errorMsg = courseBO.ValidateCourse(course);
                    if (!String.IsNullOrEmpty(errorMsg))
                    {
                        ViewBag.ErrorMsg = errorMsg;
                        return View(course);
                    }
                    db.SaveChanges();

                    db.Configuration.ValidateOnSaveEnabled = true;

                    return RedirectToAction("CoursesList");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View(course);
            }
            return View(course);

        }

        // GET: Courses/Delete/5
        [CustAuthorize("A", "M")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Find(id);
            var ques = from Questions in db.Questions
                       where Questions.CourseId.Equals(course.id)
                       select Questions;

            if(ques.Count()>0)
            {
                return Content("<script >alert('该课程下已有问题或通知发布，不可以删除！'); window.history.back();</script >", "text/html");
            }
            else
            {
                db.Courses.Remove(course);
                db.SaveChanges();
                return RedirectToAction("CoursesList");
            }
            
        }


        //根据项目Id得到相应的课程列表
        [CustAuthorize("A", "M")]
        public ActionResult Project_CoursesList(int? id)
        {

            var courses = from s in db.Courses

                          where s.ProjectId == id
                          where s.ActiveFlag == "Y"
                          select s;

            List<Course> courseList = courses.ToList();
            List<CompositeCourse> ccourseList = new List<CompositeCourse>();

            foreach (var course in courseList)
            {
                CompositeCourse ccourse = new CompositeCourse();
                ccourse.Course = course;
                Project project = db.Projects.FirstOrDefault(p => p.id == course.ProjectId);
                ccourse.Project = project;
                Manager teacher = db.Managers.FirstOrDefault(m => m.id == course.TeacherId);
                ccourse.Teacher = teacher;

                ccourseList.Add(ccourse);
            }

            return View(ccourseList);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
