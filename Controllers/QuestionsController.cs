using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
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
    public class QuestionsController : Controller
    {
        private WenbaDBContext db = new WenbaDBContext();

        [CustAuthorize("A", "M")]
        // GET: Questions
        public ActionResult Index(string ProjectId, string CourseId, string QuestionDesc, int? page)
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
            if (String.IsNullOrEmpty(ProjectId))
            {
                List<SelectListItem> courseList = new List<SelectListItem>();
                ViewData["CouserList"] = courseList;
            }
            else
            {

                BindCourse(ProjectId);
            }

            //List<Question> questionList = db.Questions.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
            //根据用户身份做判断
            List<Question> questionList = new List<Question>();
            if (UserLogin.userroles.Contains("M"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var questions = from Questions in db.Questions
                                join Courses in db.Courses on Questions.CourseId equals Courses.id
                                join Projects in db.Projects on Courses.ProjectId equals Projects.id
                                where Projects.ManagerId == user.PersonId
                                select Questions;
                questionList = questions.OrderBy(c => c.QuestionNum).ToList();
                //questionList = questions.ToList();

            }
            else
            {
                //questionList = db.Questions.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
                questionList = db.Questions.OrderByDescending(c => c.id).ToList();
            }

            List<CompositeQuestion> qquestionList = new List<CompositeQuestion>();

            foreach (var question in questionList)
            {
                CompositeQuestion qquestion = new CompositeQuestion();
                qquestion.Question = question;
                //与课程表关联
                Course course = db.Courses.FirstOrDefault(m => m.id == question.CourseId);
                qquestion.Course = course;

                //通过课程与项目表关联
                Project project = db.Projects.FirstOrDefault(p => p.id == course.ProjectId);
                qquestion.Project = project;

                qquestionList.Add(qquestion);
            }


            //第几页
            int pageNumbers = page ?? 1;
            //每页显示多少条
            int pageSizes = int.Parse(ConfigurationManager.AppSettings["pageSize"]);

            //筛选条件
            if (!String.IsNullOrEmpty(ProjectId) && ProjectId != "所有项目")
            {
                qquestionList = qquestionList.Where(x => x.Course.ProjectId.Equals(Convert.ToInt32(ProjectId))).ToList();
            }
            if (!String.IsNullOrEmpty(CourseId) && CourseId != "所有课程")
            {
                qquestionList = qquestionList.Where(x => x.Course.id.Equals(Convert.ToInt32(CourseId))).ToList();
            }

            if (!String.IsNullOrEmpty(QuestionDesc))
            {
                qquestionList = qquestionList.Where(x => x.Question.QuestionDesc.Contains(QuestionDesc)).ToList();
            }

            //通过ToPagedList扩展方法进行分页
            IPagedList<CompositeQuestion> pagedList = qquestionList.ToPagedList(pageNumbers, pageSizes);

            //将分页处理后的列表传给View
            this.ViewData["ProjectId"] = ProjectId;
            this.ViewData["CourseId"] = CourseId;
            this.ViewData["QuestionDesc"] = QuestionDesc;

            return View(pagedList);

            //return View(db.Questions.ToList());
        }

        // GET: Questions/Details/5
        [CustAuthorize("A", "M")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }

            CompositeQuestion qquestion = new CompositeQuestion();
            qquestion.Question = question;
            Course course = db.Courses.FirstOrDefault(m => m.id == question.CourseId);
            qquestion.Course = course;
            Project project = db.Projects.FirstOrDefault(p => p.id == course.ProjectId);
            qquestion.Project = project;

            return View(qquestion);


        }


        //联动 绑定课程
        public JsonResult BindCourse(string id)
        {
            List<SelectListItem> courseList = new List<SelectListItem>();
            int projectId = Convert.ToInt32(id);
            var list = from s in db.Courses.Where(c => c.ProjectId.Equals(projectId)).AsEnumerable()
                       select new SelectListItem
                       {
                           Value = s.id.ToString(),
                           Text = s.CourseName
                       };
            foreach (var item in list)
            {
                SelectListItem couserItem = new SelectListItem();
                couserItem.Text = item.Text;
                couserItem.Value = item.Value;
                courseList.Add(couserItem);
            }
            ViewData["CouserList"] = courseList;
            return Json(list.ToList());

        }


        // GET: Questions/Create
        [CustAuthorize("A", "M")]
        public ActionResult Create()
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

            return View();
        }


        //// POST: Questions/Create  单个添加
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "id,QuestionTitle,QuestionDesc,QuestionType,QuestionNum,CourseId,Comments,CreatedBy,CreationDate,LastUpdateDate,LastUpdatedBy,Attribute1,Attribute2,Attribute3,Attribute4,Attribute5,Attribute6,Attribute7,Attribute8,Attribute9,Attribute10,Status")] Question question)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Questions.Add(question);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(question);
        //}


        // POST: Questions/Create 批量添加
        [CustAuthorize("A", "M")]
        [HttpPost]
        public ActionResult Create(QuestionsMassVM questions)
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

            List<Question> quests = new List<Question>();

            for (int i = 0; i < questions.QuestionDesc.Count; i++)
            {

                Question q = new Question()
                {
                    CourseId = questions.CourseId,
                    Status = "C",     //默认都为保存未发布状态
                    QuestionType = questions.QuestionType,
                    QuestionDesc = questions.QuestionDesc[i],
                    QuestionNum = questions.QuestionNum[i],
                    Comments = questions.Comments[i],
                    CreatedBy = Convert.ToInt32(UserLogin.userid),      //Need to change: to get userid from session!!!
                    CreationDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    LastUpdatedBy = Convert.ToInt32(UserLogin.userid)       //Need to change: to get userid from session!!!
                };
                quests.Add(q);
            }

            QuestionBO questionBO = new QuestionBO();
            try
            {
                //Validation logic is needed before save to database  验证序号是否已经存在
                string msg = questionBO.ValidateProject(quests);

                db.Questions.AddRange(quests);
                if (!String.IsNullOrEmpty(msg))
                {
                    ViewBag.ErrorMsg = msg;
                    //return View(questions);
                    return Content("<script >alert('" + msg + "'); window.history.back();</script >", "text/html");
                    //return Content( msg );
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View(questions);

            }
        }

        // GET: Questions/Edit/5
        [CustAuthorize("A", "M")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Edit/5
        [CustAuthorize("A", "M")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Question question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(question).State = EntityState.Modified;

                    //取消更新前的字段验证
                    db.Configuration.ValidateOnSaveEnabled = false;
                    var entry = db.Entry(question);
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
                    entry.Property(e => e.QuestionTitle).IsModified = false;
                    entry.Property(e => e.QuestionType).IsModified = false;
                    entry.Property(e => e.CourseId).IsModified = false;
                    entry.Property(e => e.Status).IsModified = false;

                    question.LastUpdateDate = DateTime.Now;
                    question.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);      //从系统获取

                    //Validate Question before commit to Database
                    QuestionBO questioneBO = new QuestionBO();
                    string errorMsg = "";
                    errorMsg = questioneBO.ValidateQuestionEdit(question);
                    if (!String.IsNullOrEmpty(errorMsg))
                    {
                        ViewBag.ErrorMsg = errorMsg;
                        return View(question);
                    }
                    db.SaveChanges();

                    db.Configuration.ValidateOnSaveEnabled = true;

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View(question);
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        [CustAuthorize("A", "M")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [CustAuthorize("A", "M")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.Questions.Find(id);
            db.Questions.Remove(question);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // POST: Questions/批量发布
        [CustAuthorize("A", "M")]
        [HttpPost]
        public ActionResult Release(FormCollection fc)
        {
            var tempString = fc["tempString"];
            string[] questionId = tempString.Split(',');   // 截取字符串，获得各个checkBox的值
            try
            {

                for (int i = 0; i < questionId.Length; i++)
                {

                    var qu = db.Questions.Find(Convert.ToInt32(questionId[i]));
                    qu.Status = "O";
                    qu.LastUpdatedBy = Convert.ToInt32(UserLogin.userid);      //系统获取
                    qu.LastUpdateDate = DateTime.Now;
                    db.Entry(qu).State = EntityState.Modified;
                    //取消更新前的字段验证
                    db.Configuration.ValidateOnSaveEnabled = false;

                    var entry = db.Entry(qu);
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
                    entry.Property(e => e.QuestionTitle).IsModified = false;
                    entry.Property(e => e.QuestionType).IsModified = false;
                    entry.Property(e => e.CourseId).IsModified = false;
                    entry.Property(e => e.QuestionDesc).IsModified = false;
                    entry.Property(e => e.QuestionNum).IsModified = false;

                }

                int intCount = db.SaveChanges();
                db.Configuration.ValidateOnSaveEnabled = true;
                if (intCount > 0)
                {
                    return RedirectToAction("Index");
                    //return Content("<script >alert('发布成功！'); window.history.back();</script >", "text/html");
                }
                else
                {
                    return Content("<script >alert('未选择问题发布！'); window.history.back();</script >", "text/html");
                }

            }
            catch (Exception)
            {
                //throw ex;
                return Content("<script >alert('发布失败！'); window.history.back();</script >", "text/html");
            }
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
