using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wenba.BLL;
using Wenba.Filters;
using Wenba.Models;
using Wenba.ViewModels;

namespace Wenba.Controllers
{
    [AuthorizationAttribute]
    public class ProjectsController : Controller
    {
        private WenbaDBContext db = new WenbaDBContext();
        // GET: Projects
        [CustAuthorize("A", "M")]
        public ActionResult Index(string projectId, string projectNum, string managerId, string startDate, string endDate, string projStatus, int? page)
        {
            //初始化项目名称下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            projectList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Projects)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["ProjectList"] = projectList;

            //初始化项目经理下拉列表
            List<SelectListItem> managerList = new List<SelectListItem>();
            managerList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Managers.Where(m => m.ManagerType == "M"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                managerList.Add(listItem);
            }
            ViewData["ManagerList"] = managerList;

            //初始化项目状态下拉列表
            List<SelectListItem> projStatusList = new List<SelectListItem>();
            projStatusList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Dictionaries.Where(d => d.TYPE == "PROJECTSTATUS"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.VALUE;
                listItem.Value = item.CODE;
                projStatusList.Add(listItem);
            }
            ViewData["ProjStatusList"] = projStatusList;

            //查询项目列表
            List<Project> projects = new List<Project>();
            //根据用户身份做判断
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                projects = db.Projects.Where(p => p.ManagerId.Equals(user.PersonId)).ToList();
            }
            else
            {
                projects = db.Projects.ToList();
            }

            List<CompositeProject> cprojectList = new List<CompositeProject>();

            if (projects == null)
            {
                return View();
            }

            if (!String.IsNullOrEmpty(projectId) && projectId != "A")
            {
                projects = projects.Where(p => p.id == Convert.ToInt32(projectId)).ToList();
            }
            if (!String.IsNullOrEmpty(managerId) && managerId != "A")
            {
                projects = projects.Where(p => p.ManagerId == Convert.ToInt32(managerId)).ToList();
            }
            if (!String.IsNullOrEmpty(projStatus) && projStatus != "A")
            {
                projects = projects.Where(p => p.Status == projStatus).ToList();
            }

            if (!String.IsNullOrEmpty(startDate))
            {
                projects = projects.Where(p => p.StartDate >= Convert.ToDateTime(startDate)).ToList();
            }
            if (!String.IsNullOrEmpty(endDate))
            {
                projects = projects.Where(p => p.EndDate <= Convert.ToDateTime(endDate)).ToList();
            }

            if (!String.IsNullOrEmpty(projectNum))
            {
                projects = projects.Where(p => p.ProjectNum == projectNum).ToList();
            }
            foreach (var p in projects.OrderByDescending(p => p.CreationDate))
            {
                CompositeProject cproject = new CompositeProject();
                cproject.Project = p;
                Manager manager = db.Managers.FirstOrDefault(m => m.id == p.ManagerId);
                Dictionary statusDic = db.Dictionaries.FirstOrDefault(d => d.TYPE == "PROJECTSTATUS" && d.CODE == p.Status);
                cproject.Manager = manager;

                Manager customerManager = db.Managers.FirstOrDefault(m => m.id == p.CustManagerId);
                cproject.CustomerManager = customerManager;

                List<StudentAssgin> stuAssginList = db.StudentAssgins.Where(s => s.ProjectId == p.id).ToList();
                List<Student> stuList = new List<Student>();
                foreach (var sa in stuAssginList)
                {
                    stuList.Add(db.Students.FirstOrDefault(s => s.id == sa.StudentId));
                }
                cproject.StudentAssigns = stuAssginList;
                cproject.Students = stuList;
                cproject.StatusDic = statusDic;

                cprojectList.Add(cproject);
            }

            int pageNumber = page ?? 1;
            int pageSize = int.Parse(ConfigurationManager.AppSettings["pageSize"]);

            cprojectList.OrderByDescending(c => c.Project.id);
            IPagedList<CompositeProject> pagedList = cprojectList.ToPagedList(pageNumber, pageSize);
            //将分页处理后的列表传给View
            this.ViewData["managerId"] = managerId;
            this.ViewData["projStatus"] = projStatus;
            this.ViewData["StartDate"] = startDate;
            this.ViewData["EndDate"] = endDate;
            return View(pagedList);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Projects/Create
        [CustAuthorize("A", "M")]
        public ActionResult Create()
        {
            //初始化客户经理下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Manager> mList = new List<Manager>();
              
            mList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "C").ToList();
            
            foreach (var item in mList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["CostManagerList"] = projectList;


            //初始化项目经理下拉列表
            List<SelectListItem> managerList = new List<SelectListItem>();
            //managerList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            //根据登陆用户身份判断
            List<Manager> maList = new List<Manager>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Manager in db.Managers
                           where Manager.id == user.PersonId
                           select Manager;
                maList = pros.Where(p => p.ActiveFlag != "N").ToList();
            }
            else
            {
                maList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "M").ToList();
            }

            foreach (var item in maList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                managerList.Add(listItem);
            }
            ViewData["ManagerList"] = managerList;

            //初始化项目状态下拉列表
            List<SelectListItem> projStatusList = new List<SelectListItem>();
            //projStatusList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Dictionaries.Where(d => d.TYPE == "PROJECTSTATUS" && d.CODE != "C"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.VALUE;
                listItem.Value = item.CODE;
                projStatusList.Add(listItem);
            }
            ViewData["ProjStatusList"] = projStatusList;

            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        public ActionResult Create(Project project)
        {
            //初始化客户经理下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Manager> mList = new List<Manager>();

            mList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "C").ToList();

            foreach (var item in mList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["CostManagerList"] = projectList;


            //初始化项目经理下拉列表
            List<SelectListItem> managerList = new List<SelectListItem>();
            //managerList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            //根据登陆用户身份判断
            List<Manager> maList = new List<Manager>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Manager in db.Managers
                           where Manager.id == user.PersonId
                           select Manager;
                maList = pros.Where(p => p.ActiveFlag != "N").ToList();
            }
            else
            {
                maList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "M").ToList();
            }

            foreach (var item in maList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                managerList.Add(listItem);
            }
            ViewData["ManagerList"] = managerList;

            //初始化项目状态下拉列表
            List<SelectListItem> projStatusList = new List<SelectListItem>();
            //projStatusList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Dictionaries.Where(d => d.TYPE == "PROJECTSTATUS" && d.CODE != "C"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.VALUE;
                listItem.Value = item.CODE;
                projStatusList.Add(listItem);
            }
            ViewData["ProjStatusList"] = projStatusList;

            ProjectsBO projectBO = new ProjectsBO();
            try
            {
                // TODO: Add insert logic here
                db.Projects.Add(project);
                project.CreatedBy = Convert.ToInt32(UserLogin.userid);
                project.CreationDate = System.DateTime.Now;
                project.LastUpdateDate = System.DateTime.Now;
                project.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);

                string msg = projectBO.ValidateProject(project);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    return View(project);
                }
                db.SaveChanges();
                return RedirectToAction("Index", new { projectNum = project.ProjectNum });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }
        }

        // GET: Projects/Edit/5
        [CustAuthorize("A", "M")]
        public ActionResult Edit(int id)
        {
            //初始化客户经理下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Manager> mList = new List<Manager>();

            mList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "C").ToList();

            foreach (var item in mList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["CostManagerList"] = projectList;


            //初始化项目经理下拉列表
            List<SelectListItem> managerList = new List<SelectListItem>();
            //managerList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            //根据登陆用户身份判断
            List<Manager> maList = new List<Manager>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Manager in db.Managers
                           where Manager.id == user.PersonId
                           select Manager;
                maList = pros.Where(p => p.ActiveFlag != "N").ToList();
            }
            else
            {
                maList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "M").ToList();
            }

            foreach (var item in maList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                managerList.Add(listItem);
            }
            ViewData["ManagerList"] = managerList;

            //初始化项目状态下拉列表
            List<SelectListItem> projStatusList = new List<SelectListItem>();
            //projStatusList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Dictionaries.Where(d => d.TYPE == "PROJECTSTATUS"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.VALUE;
                listItem.Value = item.CODE;
                projStatusList.Add(listItem);
            }
            ViewData["ProjStatusList"] = projStatusList;

            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Project project)
        {
            string errorMsg = "";
            //初始化客户经理下拉列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            //根据用户身份做判断
            List<Manager> mList = new List<Manager>();

            mList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "C").ToList();

            foreach (var item in mList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                projectList.Add(listItem);
            }
            ViewData["CostManagerList"] = projectList;


            //初始化项目经理下拉列表
            List<SelectListItem> managerList = new List<SelectListItem>();
            //managerList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            //根据登陆用户身份判断
            List<Manager> maList = new List<Manager>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Manager in db.Managers
                           where Manager.id == user.PersonId
                           select Manager;
                maList = pros.Where(p => p.ActiveFlag != "N").ToList();
            }
            else
            {
                maList = db.Managers.Where(p => p.ActiveFlag != "N" && p.ManagerType == "M").ToList();
            }

            foreach (var item in maList)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ManagerName;
                listItem.Value = item.id.ToString();
                managerList.Add(listItem);
            }
            ViewData["ManagerList"] = managerList;

            //初始化项目状态下拉列表
            List<SelectListItem> projStatusList = new List<SelectListItem>();
            //projStatusList.Add(new SelectListItem() { Text = "全部", Value = "A" });
            foreach (var item in db.Dictionaries.Where(d => d.TYPE == "PROJECTSTATUS"))
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.VALUE;
                listItem.Value = item.CODE;
                projStatusList.Add(listItem);
            }
            ViewData["ProjStatusList"] = projStatusList;

            try
            {
                if (ModelState.IsValid)
                {
                    //Cancel validation before save
                    db.Configuration.ValidateOnSaveEnabled = false;
                    var entry = db.Entry(project);
                    entry.State = EntityState.Modified;

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

                    project.LastUpdateDate = DateTime.Now;
                    project.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);//Need to change to use actual login userid

                    //Validate Projects before commit to Database
                    ProjectsBO projectBO = new ProjectsBO();
                    errorMsg = projectBO.ValidateProject(project);
                    if (!String.IsNullOrEmpty(errorMsg))
                    {
                        ViewBag.ErrorMsg = errorMsg;
                        return View(project);
                    }
                    db.SaveChanges();

                    db.Configuration.ValidateOnSaveEnabled = true;

                    return RedirectToAction("Index", new { projectId = project.id });
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View(project);
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [CustAuthorize("A", "M")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Projects/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
