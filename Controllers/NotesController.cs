using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Wenba.BLL;
using Wenba.Filters;
using Wenba.Models;

namespace Wenba.Controllers
{
    [AuthorizationAttribute]
    public class NotesController : Controller
    {
        public class viewModel
        {
            public IPagedList<Questionlist> QuestionModel { get; set; }
            public IPagedList<Notelist> NoteModel { get; set; }


            public viewModel(IPagedList<Questionlist> Questionlist, IPagedList<Notelist> Notelist)
            {
                this.QuestionModel = Questionlist;
                this.NoteModel = Notelist;
            }
        }



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
        public class Questionlist
        {
            //问题ID
            public int QuestionId;
            public string QuestionContent;
            public int ProjectId;
            public int? CourseId;
            public int UserId;
            public int PersonId;
            public string UserRole;
            public string ProjectName;
            public string CourseName;
            public string UserName;
            public string NoteContent;
            public string CommentContent;
            public string ReleaseTime;
        }

        public class Notelist
        {
            //笔记ID
            public int NoteId;
            public int UserId;
            public int PersonId;
            public string UserRole;
            public string UserName;
            public string NoteContent;
            public string CommentContent;
            public string CommentedType;
            public string ReleaseTime;
        }
        // GET: Notes
        private WenbaDBContext db = new WenbaDBContext();
        List<Questionlist> qusetionlist = new List<Questionlist>();
        List<Notelist> qusetionlists = new List<Notelist>();
        [CustAuthorize("A", "M", "S", "C")]
        public ActionResult NotesList(string ProjectId, string CourseId, string username, string time, int? page, string username1, string time1, string content1, int? pages)
        {
            //初始化项目名称列表
            List<SelectListItem> projectList = new List<SelectListItem>();
            List<SelectListItem> projectLists = new List<SelectListItem>();
            //根据用户身份做判断
            List<Project> pList = new List<Project>();
            List<Project> pLists = new List<Project>();
            if (UserLogin.userroles.Contains("M") || UserLogin.userroles.Contains("S"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pList = pros.ToList();
            }
            if (UserLogin.userroles.Contains("C"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.CustManagerId == user.PersonId
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
            else {

                BindCourse(ProjectId);
            }
        

            if (UserLogin.userroles.Contains("M")|| UserLogin.userroles.Contains("S"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.ManagerId == user.PersonId
                           select Projects;
                pLists = pros.ToList();
            }
            if (UserLogin.userroles.Contains("C"))
            {
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                var pros = from Projects in db.Projects
                           where Projects.CustManagerId == user.PersonId
                           select Projects;
                pLists = pros.ToList();
            }
            else
            {
                pLists = db.Projects.ToList();
            }

            foreach (var item in pLists)
            {
                SelectListItem listItem = new SelectListItem();
                listItem.Text = item.ProjectName;
                listItem.Value = item.id.ToString();
                projectLists.Add(listItem);
            }
            ViewData["ProjectLists"] = projectLists;



            GetCoache(ProjectId, CourseId, username, time, page);
            int pageNumber = page ?? 1;
            //每页显示多少条
            int pageSize = int.Parse(ConfigurationManager.AppSettings["pageSize"]);
            IPagedList<Questionlist> pagedList = qusetionlist.ToPagedList(pageNumber, pageSize);
            //将分页处理后的列表传给View
            this.ViewData["username"] = username;
            this.ViewData["time"] = time;
            this.ViewData["ProjectID"] = ProjectId;
            this.ViewData["CourseId"] = CourseId;


            GetCoaches(username1, time1, content1, pages);
            int pageNumbers = pages ?? 1;
            //每页显示多少条
            int pageSizes = int.Parse(ConfigurationManager.AppSettings["pageSize"]);
            IPagedList<Notelist> pagedLists = qusetionlists.ToPagedList(pageNumbers, pageSizes);
            //将分页处理后的列表传给View
            this.ViewData["username1"] = username1;
            this.ViewData["time1"] = time1;
            this.ViewData["content1"] = content1;

            var vm = new viewModel(pagedList, pagedLists);
            return View(vm);
        }


        private List<Questionlist> GetCoache(string ProjectId, string CourseId, string username, string time, int? page)
        {

            if (UserLogin.userrole == "A")
            {
                List<Questionlist> newlist = new List<Questionlist>();
                var list = from Questions in db.Questions
                           join Users in db.Users on Questions.CreatedBy equals Users.id
                           join Courses in db.Courses on Questions.CourseId equals Courses.id
                           join Projects in db.Projects on Courses.ProjectId equals Projects.id
                           where Questions.QuestionType == "Q"
                           select new Questionlist
                           {
                               QuestionId = Questions.id,
                               UserId = Questions.CreatedBy,
                               UserRole = Users.Role,
                               PersonId = Users.PersonId,
                               ProjectId = Courses.ProjectId,
                               CourseId = Questions.CourseId,
                               ProjectName = Projects.ProjectName,
                               CourseName = Courses.CourseName,
                               QuestionContent = Questions.QuestionDesc,
                               ReleaseTime = Questions.CreationDate.ToString()
                           };
                newlist = list.ToList();
                newlist.OrderByDescending(x => x.QuestionId);

                foreach (var a in newlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }
                    var answer = db.Notes.Where(x => x.QuestionId == a.QuestionId);
                    string answercontent = "";
                    string commentcontent = "";
                    foreach (var b in answer)
                    {
                        if (!String.IsNullOrEmpty(b.Notes))
                        {
                            if (answercontent == "")
                            { answercontent = "“" + b.Notes + "”"; }
                            else
                                answercontent += ",“" + b.Notes + "”";
                        }

                        var comment = db.Comments.Where(x => x.NoteId == b.id);
                        foreach (var c in comment)
                        {
                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }
                        }
                    }
                    a.NoteContent = answercontent;
                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlist.Add(a);
                }
            }

            if (UserLogin.userrole == "S")
            {
                List<Questionlist> squestionlist = new List<Questionlist>();
                List<Questionlist> snotelist = new List<Questionlist>();
                int userid = Convert.ToInt32(UserLogin.userid);
                var list = from Questions in db.Questions
                           join Users in db.Users on Questions.CreatedBy equals Users.id
                           join Courses in db.Courses on Questions.CourseId equals Courses.id
                           join Projects in db.Projects on Courses.ProjectId equals Projects.id
                           where Questions.CreatedBy == userid
                           select new Questionlist
                           {
                               QuestionId = Questions.id,
                               UserId = Questions.CreatedBy,
                               UserRole = Users.Role,
                               PersonId = Users.PersonId,
                               ProjectId = Courses.ProjectId,
                               CourseId = Questions.CourseId,
                               ProjectName = Projects.ProjectName,
                               CourseName = Courses.CourseName,
                               QuestionContent = Questions.QuestionDesc,
                               ReleaseTime = Questions.CreationDate.ToString()
                           };
                squestionlist = list.ToList();
                squestionlist.OrderByDescending(x => x.QuestionId);
                foreach (var a in squestionlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }
                    var answer = db.Notes.Where(x => x.QuestionId == a.QuestionId);
                    string answercontent = "";
                    string commentcontent = "";
                    foreach (var b in answer)
                    {
                        if (!String.IsNullOrEmpty(b.Notes))
                        {
                            if (answercontent == "")
                            { answercontent = "“" + b.Notes + "”"; }
                            else
                                answercontent += ",“" + b.Notes + "”";
                        }
                        var comment = db.Comments.Where(x => x.NoteId == b.id);

                        foreach (var c in comment)
                        {

                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }
                        }
                    }
                    a.NoteContent = answercontent;
                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlist.Add(a);
                }
                var lists = from Questions in db.Questions
                            join Users in db.Users on Questions.CreatedBy equals Users.id
                            join Courses in db.Courses on Questions.CourseId equals Courses.id
                            join Projects in db.Projects on Courses.ProjectId equals Projects.id
                            where Questions.CreatedBy != userid && Questions.QuestionType == "Q"
                            select new Questionlist
                            {
                                QuestionId = Questions.id,
                                UserId = Questions.CreatedBy,
                                UserRole = Users.Role,
                                PersonId = Users.PersonId,
                                ProjectId = Courses.ProjectId,
                                CourseId = Questions.CourseId,
                                ProjectName = Projects.ProjectName,
                                CourseName = Courses.CourseName,
                                QuestionContent = Questions.QuestionDesc,
                                ReleaseTime = Questions.CreationDate.ToString()
                            };
                snotelist = lists.ToList();
                snotelist.OrderByDescending(x => x.QuestionId);
                List<Questionlist> snoteslist = new List<Questionlist>();
                foreach (var a in snotelist)
                {
                    var notelist = db.Notes.Where(x => x.QuestionId == a.QuestionId).Where(y => y.UserId == userid).FirstOrDefault();
                    if (notelist != null)
                        snoteslist.Add(a);
                }

                foreach (var aa in snoteslist)
                {

                    if (aa.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == aa.PersonId).FirstOrDefault();
                        aa.UserName = student.StudentName;
                    }
                    if (aa.UserRole == "M" || aa.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == aa.PersonId).FirstOrDefault();
                        aa.UserName = teacher.ManagerName;
                    }

                    var answer = db.Notes.Where(x => x.QuestionId == aa.QuestionId);
                    string answercontent = "";
                    string commentcontent = "";
                    foreach (var b in answer)
                    {

                        if (!String.IsNullOrEmpty(b.Notes))
                        {
                            if (answercontent == "")
                            { answercontent = "“" + b.Notes + "”"; }
                            else
                                answercontent += ",“" + b.Notes + "”";
                        }

                        var comment = db.Comments.Where(x => x.NoteId == b.id);

                        foreach (var c in comment)
                        {

                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }

                        }
                    }
                    aa.NoteContent = answercontent;
                    aa.CommentContent = commentcontent;
                    aa.ReleaseTime = Convert.ToDateTime(aa.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlist.Add(aa);
                }
            }

            if (UserLogin.userrole == "M")
            {
                List<Questionlist> newlist = new List<Questionlist>();
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                if (UserLogin.userroles == "M")
                {
                    var list = from Questions in db.Questions
                               join Users in db.Users on Questions.CreatedBy equals Users.id
                               join Courses in db.Courses on Questions.CourseId equals Courses.id
                               join Projects in db.Projects on Courses.ProjectId equals Projects.id
                               where Questions.QuestionType == "Q" && Projects.ManagerId == user.PersonId
                               select new Questionlist
                               {
                                   QuestionId = Questions.id,
                                   UserId = Questions.CreatedBy,
                                   UserRole = Users.Role,
                                   PersonId = Users.PersonId,
                                   ProjectId = Courses.ProjectId,
                                   CourseId = Questions.CourseId,
                                   ProjectName = Projects.ProjectName,
                                   CourseName = Courses.CourseName,
                                   QuestionContent = Questions.QuestionDesc,
                                   ReleaseTime = Questions.CreationDate.ToString()
                               };

                    newlist = list.ToList();
                }
                if (UserLogin.userroles == "C")
                {
                    var list = from Questions in db.Questions
                               join Users in db.Users on Questions.CreatedBy equals Users.id
                               join Courses in db.Courses on Questions.CourseId equals Courses.id
                               join Projects in db.Projects on Courses.ProjectId equals Projects.id
                               where Questions.QuestionType == "Q" && Projects.CustManagerId == user.PersonId
                               select new Questionlist
                               {
                                   QuestionId = Questions.id,
                                   UserId = Questions.CreatedBy,
                                   UserRole = Users.Role,
                                   PersonId = Users.PersonId,
                                   ProjectId = Courses.ProjectId,
                                   CourseId = Questions.CourseId,
                                   ProjectName = Projects.ProjectName,
                                   CourseName = Courses.CourseName,
                                   QuestionContent = Questions.QuestionDesc,
                                   ReleaseTime = Questions.CreationDate.ToString()
                               };

                    newlist = list.ToList();
                }
                newlist.OrderByDescending(x => x.QuestionId);
                foreach (var a in newlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }
                    var answer = db.Notes.Where(x => x.QuestionId == a.QuestionId).Where(y => y.PublicFlag == "Y");
                    string answercontent = "";
                    string commentcontent = "";
                    foreach (var b in answer)
                    {
                        if (!String.IsNullOrEmpty(b.Notes))
                        {
                            if (answercontent == "")
                            { answercontent = "“" + b.Notes + "”"; }
                            else
                                answercontent += ",“" + b.Notes + "”";
                        }


                        var comment = db.Comments.Where(x => x.NoteId == b.id).Where(y => y.PublicFlag == "Y");
                        foreach (var c in comment)
                        {

                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }
                        }
                    }
                    a.NoteContent = answercontent;
                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlist.Add(a);
                }
            }

            if (!String.IsNullOrEmpty(ProjectId))
            {
                int proId = Convert.ToInt32(ProjectId);
                qusetionlist = qusetionlist.Where(x => x.ProjectId.Equals(proId)).ToList();
            }
            if (!String.IsNullOrEmpty(CourseId))
            {
                int couId = Convert.ToInt32(CourseId);
                qusetionlist = qusetionlist.Where(x => x.CourseId.Equals(couId)).ToList();
            }
            if (!String.IsNullOrEmpty(username))
            {
                qusetionlist = qusetionlist.Where(x => x.UserName.Contains(username)).ToList();
            }
            if (!String.IsNullOrEmpty(time))
            {
                qusetionlist = qusetionlist.Where(x => x.ReleaseTime.Contains(time)).ToList();
            }
            return qusetionlist;
        }


        private List<Notelist> GetCoaches(string username1, string time1, string content1, int? pages)
        {

            if (UserLogin.userrole == "A")
            {
                List<Notelist> newlist = new List<Notelist>();
                var list = from Notes in db.Notes
                           join Users in db.Users on Notes.UserId equals Users.id
                           where Notes.QuestionId == null
                           select new Notelist
                           {
                               NoteId = Notes.id,
                               UserId = Notes.UserId,
                               UserRole = Users.Role,
                               PersonId = Users.PersonId,
                               NoteContent = Notes.Notes,
                               ReleaseTime = Notes.CreationDate.ToString()
                           };
                newlist = list.ToList();
                newlist.OrderByDescending(x => x.NoteId);

                foreach (var a in newlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }

                    string commentcontent = "";
                    var comment = db.Comments.Where(x => x.NoteId == a.NoteId);
                    foreach (var c in comment)
                    {
                        if (!String.IsNullOrEmpty(c.Comments))
                        {
                            if (commentcontent == "")
                            { commentcontent = "“" + c.Comments + "”"; }
                            else
                                commentcontent += ",“" + c.Comments + "”";
                        }
                    }

                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlists.Add(a);
                }
            }


            if (UserLogin.userrole == "S")
            {
                List<Notelist> newlist = new List<Notelist>();
                int userid = Convert.ToInt32(UserLogin.userid);
                var list = from Notes in db.Notes
                           join Users in db.Users on Notes.UserId equals Users.id
                           where Notes.QuestionId == null && Notes.UserId == userid
                           select new Notelist
                           {
                               NoteId = Notes.id,
                               UserId = Notes.UserId,
                               UserRole = Users.Role,
                               PersonId = Users.PersonId,
                               NoteContent = Notes.Notes,
                               ReleaseTime = Notes.CreationDate.ToString()
                           };
                newlist = list.ToList();
                newlist.OrderByDescending(x => x.NoteId);

                foreach (var a in newlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }

                    string commentcontent = "";
                    var comment = db.Comments.Where(x => x.NoteId == a.NoteId);
                    foreach (var c in comment)
                    {
                        var collects = db.Collections.Where(x => x.CommentId == c.id).Where(y => y.UserId == userid).FirstOrDefault();
                        if (collects != null)
                        {
                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }
                        }
                    }

                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlists.Add(a);
                }

                List<Notelist> newslist = new List<Notelist>();
                var lists = from Notes in db.Notes
                            join Users in db.Users on Notes.UserId equals Users.id
                            where Notes.QuestionId == null && Notes.UserId != userid
                            select new Notelist
                            {
                                NoteId = Notes.id,
                                UserId = Notes.UserId,
                                UserRole = Users.Role,
                                PersonId = Users.PersonId,
                                NoteContent = Notes.Notes,
                                ReleaseTime = Notes.CreationDate.ToString()
                            };
                newslist = lists.ToList();
                newslist.OrderByDescending(x => x.NoteId);
                List<Notelist> snoteslist = new List<Notelist>();
                foreach (var a in newslist)
                {
                    var comlist = db.Comments.Where(x => x.NoteId == a.NoteId).Where(y => y.CreatedBy == userid).FirstOrDefault();
                    if (comlist != null)
                        snoteslist.Add(a);
                }
                foreach (var a in snoteslist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }

                    string commentcontent = "";
                    var comment = db.Comments.Where(x => x.NoteId == a.NoteId);
                    foreach (var c in comment)
                    {
                      
                            if (!String.IsNullOrEmpty(c.Comments))
                            {
                                if (commentcontent == "")
                                { commentcontent = "“" + c.Comments + "”"; }
                                else
                                    commentcontent += ",“" + c.Comments + "”";
                            }
                    }
                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlists.Add(a);
                }
            }

            if (UserLogin.userrole == "M")
            {

                List<Notelist> newlist = new List<Notelist>();
                int userid = Convert.ToInt32(UserLogin.userid);
                var user = db.Users.Where(x => x.id == userid).FirstOrDefault();
                if(UserLogin.userroles == "M")
                { 
                var list = from Notes in db.Notes
                           join Users in db.Users on Notes.UserId equals Users.id
                           join Students in db.Students on Users.PersonId equals Students.id
                           join StudentAssgins in db.StudentAssgins on Students.id equals StudentAssgins.StudentId
                           join Projects in db.Projects on StudentAssgins.ProjectId equals Projects.id
                           where Notes.QuestionId == null && Projects.ManagerId == user.PersonId && Notes.PublicFlag=="Y"
                           select new Notelist
                           {
                               NoteId = Notes.id,
                               UserId = Notes.UserId,
                               UserRole = Users.Role,
                               PersonId = Users.PersonId,
                               NoteContent = Notes.Notes,
                               ReleaseTime = Notes.CreationDate.ToString()
                           };
                    newlist = list.ToList();
                }
                if (UserLogin.userroles == "C")
                {
                    var list = from Notes in db.Notes
                               join Users in db.Users on Notes.UserId equals Users.id
                               join Students in db.Students on Users.PersonId equals Students.id
                               join StudentAssgins in db.StudentAssgins on Students.id equals StudentAssgins.StudentId
                               join Projects in db.Projects on StudentAssgins.ProjectId equals Projects.id
                               where Notes.QuestionId == null && Projects.CustManagerId == user.PersonId && Notes.PublicFlag == "Y"
                               select new Notelist
                               {
                                   NoteId = Notes.id,
                                   UserId = Notes.UserId,
                                   UserRole = Users.Role,
                                   PersonId = Users.PersonId,
                                   NoteContent = Notes.Notes,
                                   ReleaseTime = Notes.CreationDate.ToString()
                               };
                    newlist = list.ToList();
                }

                newlist.OrderByDescending(x => x.NoteId);

                foreach (var a in newlist)
                {
                    if (a.UserRole == "S")
                    {
                        var student = db.Students.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = student.StudentName;
                    }
                    if (a.UserRole == "M" || a.UserRole == "A")
                    {
                        var teacher = db.Managers.Where(x => x.id == a.PersonId).FirstOrDefault();
                        a.UserName = teacher.ManagerName;
                    }

                    string commentcontent = "";
                    var comment = db.Comments.Where(x => x.NoteId == a.NoteId);
                    foreach (var c in comment)
                    {
                        if (!String.IsNullOrEmpty(c.Comments))
                        {
                            if (commentcontent == "")
                            { commentcontent = "“" + c.Comments + "”"; }
                            else
                                commentcontent += ",“" + c.Comments + "”";
                        }
                    }
                    a.CommentContent = commentcontent;
                    a.ReleaseTime = Convert.ToDateTime(a.ReleaseTime).ToString("yyyy-MM-dd");
                    qusetionlists.Add(a);
                }
            }
            if (!String.IsNullOrEmpty(username1))
            {
                qusetionlists = qusetionlists.Where(x => x.UserName.Contains(username1)).ToList();
            }
            if (!String.IsNullOrEmpty(time1))
            {
                qusetionlists = qusetionlists.Where(x => x.ReleaseTime.Contains(time1)).ToList();
            }
            if (!String.IsNullOrEmpty(content1))
            {
                qusetionlists = qusetionlists.Where(x => x.NoteContent.Contains(content1)).ToList();
            }
            return qusetionlists;

        }

        public ActionResult Export(string ProjectId, string CourseId, string username, string time, int? page)
        {
            var grid = new System.Web.UI.WebControls.GridView();
            int i = 1;
            grid.DataSource = from item in GetCoache(ProjectId, CourseId, username, time, page)
                              select new
                              {
                                  序号 = i++,
                                  项目名称 = item.ProjectName,
                                  课程名称 = item.CourseName,
                                  提问人 = item.UserName,
                                  问题 = item.QuestionContent,
                                  内容 = item.NoteContent,
                                  评论 = item.CommentContent,
                                  发布时间 = item.ReleaseTime
                              };

            grid.DataBind();
            Response.ClearContent();
            string name = "问与答统计表" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            Response.AddHeader("content-disposition", "attachment; filename='" + name + "'.xls");
            Response.Buffer = true;
            Response.Charset = "UTF-8";
            Response.ContentEncoding = Encoding.UTF8;
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Write(sw.ToString());
            Response.End();
            return null;
        }

        public ActionResult Exports( string username, string time,string content, int? page)
        {
            var grid = new System.Web.UI.WebControls.GridView();
            int i = 1;
            grid.DataSource = from item in GetCoaches( username, time,content, page)
                              select new
                              {
                                  序号 = i++,
                                  记录人 = item.UserName,
                                  内容 = item.NoteContent,
                                  评论 = item.CommentContent,
                                  发布时间 = item.ReleaseTime
                              };
            grid.DataBind();
            Response.ClearContent();
            string name = "记与思统计表" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            Response.AddHeader("content-disposition", "attachment; filename='" + name + "'.xls");
            Response.Buffer = true;
            Response.Charset = "UTF-8";
            Response.ContentEncoding = Encoding.UTF8;
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Write(sw.ToString());
            Response.End();
            return null;
        }
    }
}