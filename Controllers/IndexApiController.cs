using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Wenba.Models;

namespace Wenba.Controllers
{
    public class IndexApiController : ApiController
    {
        private WenbaDBContext db = new WenbaDBContext();
        // GET
        public IQueryable<Course> GetCourses()
        {
            return db.Courses;
        }

        ////GET  根据项目找课程
        //[HttpGet]
        //[ResponseType(typeof(List<Course>))]
        //public IHttpActionResult GetCoursesByProject(int id)
        //{
        //    var courses = from s in db.Courses
        //                  where s.ProjectId == id
        //                  where s.ActiveFlag == "Y"
        //                  select s;

        //    List<Course> courseList = courses.ToList();
        //    if (courseList == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(courseList);
        //}


        // POST:  提交问题
        [ResponseType(typeof(Question))]
        public IHttpActionResult PostQuestion(Question question)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
           
            //question.CreatedBy = 10063;   //Need to change to use actual user
            question.CreationDate = DateTime.Now;
            question.LastUpdateDate = DateTime.Now;
            //question.LastUpdatedBy = 10063;   //Need to change to use actual user

            db.Questions.Add(question);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = question.id }, question);
        }


        // POST: 回答问题
        [ResponseType(typeof(Note))]
        public IHttpActionResult PostAnswer(Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //note.UserId = 10063;  //Need to change to use actual user
            //note.CreatedBy = 0;  //Need to change to use actual user
            note.CreationDate = DateTime.Now;
            note.LastUpdateDate = DateTime.Now;
            //note.LastUpdatedBy = 0;  //Need to change to use actual user

            db.Notes.Add(note);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = note.id }, note);
        }


        public class CoursesAll
        {
            //课程ID
            public int? CourseId;
            public string CourseName;
            public bool HasNotice;
            public List<Notice> Notices;
            //是否有问题
            public bool HasQuestion;
            public List<QuestionsAll> QuestionsAll;

        }

        public class Notice
        {
            //通知ID
            public int? NoticeId;
            public string NoticeContent;
            public string ReleaseTime;

        }

        public class QuestionsAll
        {
            //问题ID
            public int? QuestionId;
            public string QuestionContent;
            public string ReleaseTime;
            //是否有回答
            public bool IsAnswered;
            public List<Answers> Answers;

        }

        public class Answers
        {
            //回答ID
            public int? AnswerId;
            public string Answer;
            //答案是否公开
            public string PublicFlag;
            public string AnswerTime;
            //回答人的userid
            public int UserId;
            public string name;
            public string HeadImg;
            //是否有评论
            public bool IsCommented;
            public List<AnswerComments> Comments;

        }
        public class AnswerComments
        {
            //评论ID
            public int? CommentId;
            public string Comment;
            public string CommentTime;
            //评论人的userid
            public int UserId;
            public string name;
            public string HeadImg;

        }

        //GET：得到首页的所有数据
        [HttpGet]
        public IHttpActionResult GetMyCoursesAll(int id)    //这边传的是UserId
        {
            List<CoursesAll> CoursesAllLists = new List<CoursesAll>();
            var user = db.Users.Where(x => x.id == id).FirstOrDefault();

            List<Course> courseList = null;
            if (user.Role == "S")
            {
                var sAssign = db.StudentAssgins.Where(x => x.StudentId == user.PersonId);
                List<int> projectIdlist = new List<int>();
                projectIdlist = (from s in sAssign
                                 select s.ProjectId).ToList();
                var linq1 = (from cou in db.Courses
                             where projectIdlist.Contains(cou.ProjectId)
                             where cou.ActiveFlag.Contains("Y")
                             select cou).ToList();
                courseList = linq1;
            }
            if (user.Role == "M")
            {
                var linq2 = (from cou in db.Courses
                             where cou.TeacherId == user.PersonId
                             where cou.ActiveFlag.Contains("Y")
                             select cou).ToList();
                courseList = linq2;
            }

            if (courseList.Count == 0)
            {
                CoursesAllLists = null;
                return Ok(CoursesAllLists);    //没有课程
            }

            foreach (Course cou in courseList)
            {
                CoursesAll courseitem = new CoursesAll();
                courseitem.CourseId = cou.id;
                courseitem.CourseName = cou.CourseName;

                var noticeList = from notices in db.Questions
                                 where notices.QuestionType == "N" && notices.Status.Contains("O")
                                 where notices.CourseId == cou.id
                                 select notices;
                if (noticeList.ToList().Count == 0)
                {
                    courseitem.HasNotice = false;
                    courseitem.Notices = null;    //该课程下没有通知
                }
                else
                {
                    courseitem.HasNotice = true;
                    List<Notice> Nlists = new List<Notice>();
                    foreach (Question n in noticeList.ToList())
                    {
                        Notice noticeitem = new Notice();
                        noticeitem.NoticeId = n.id;
                        noticeitem.NoticeContent = n.QuestionDesc;
                        noticeitem.ReleaseTime = n.LastUpdateDate.ToString("f");
                        Nlists.Add(noticeitem);
                    }
                    courseitem.Notices = Nlists;
                }

                var questionList = from Questions in db.Questions
                                   where Questions.QuestionType == "Q" && Questions.Status.Contains("O") && Questions.CourseId == cou.id
                                   select Questions;
                List<Question> ques = questionList.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
                if (ques.Count == 0)
                {
                    courseitem.HasQuestion = false;    //该课程下没有问题
                    courseitem.QuestionsAll = null;
                }
                else
                {
                    courseitem.HasQuestion = true;
                }

                List<QuestionsAll> QAlists = new List<QuestionsAll>();
                foreach (Question q in ques)
                {
                    QuestionsAll questionitem = new QuestionsAll();
                    questionitem.QuestionId = q.id;
                    questionitem.QuestionContent = q.QuestionDesc;
                    questionitem.ReleaseTime = q.LastUpdateDate.ToString("f");
                    var answerList = from Notes in db.Notes
                                     where Notes.UserId == id && Notes.QuestionId == q.id     //当前登录用户的所有回答   id需要改成系统获取！！！
                                     select Notes;
                    if (answerList.ToList().Count == 0)
                    {
                        questionitem.IsAnswered = false;    //该问题该用户暂未回答
                        questionitem.Answers = null;
                    }
                    else
                    {
                        questionitem.IsAnswered = true;
                    }

                    List<Answers> Alists = new List<Answers>();
                    foreach (Note a in answerList.ToList())
                    {
                        Answers answeritem = new Answers();
                        answeritem.AnswerId = a.id;
                        answeritem.Answer = a.Notes;
                        answeritem.PublicFlag = a.PublicFlag;
                        answeritem.AnswerTime = a.CreationDate.ToString("f");
                        answeritem.UserId = a.UserId;
                        var user1 = db.Users.Where(x => x.id == a.UserId).FirstOrDefault();
                        if (user1.Role == "S")
                        {
                            var stu = db.Students.Where(x => x.id == user.PersonId).FirstOrDefault();
                            answeritem.HeadImg = stu.HeadImage;
                            answeritem.name = stu.StudentName;
                        }
                        if (user1.Role == "M")
                        {
                            var man = db.Managers.Where(x => x.id == user.PersonId).FirstOrDefault();
                            answeritem.HeadImg = man.HeadImage;
                            answeritem.name = man.ManagerName;
                        }

                        var commentList = from Comments in db.Comments
                                          where Comments.PublicFlag == "Y" && Comments.NoteId == a.id
                                          select Comments;
                        if (commentList.ToList().Count == 0)
                        {
                            answeritem.IsCommented = false;    //该回答暂无评论
                            answeritem.Comments = null;
                        }
                        else
                        {
                            answeritem.IsCommented = true;
                        }

                        List<AnswerComments> Clists = new List<AnswerComments>();
                        foreach (Comment c in commentList.ToList())
                        {
                            AnswerComments commentitem = new AnswerComments();
                            commentitem.CommentId = c.id;
                            commentitem.Comment = c.Comments;
                            commentitem.CommentTime = c.CreationDate.ToString("f");
                            commentitem.UserId = c.CommentedBy;
                            var user2 = db.Users.Where(x => x.id == c.CommentedBy).FirstOrDefault();
                            if (user2.Role == "S")
                            {
                                var stu = db.Students.Where(x => x.id == user2.PersonId).FirstOrDefault();
                                commentitem.HeadImg = stu.HeadImage;
                                commentitem.name = stu.StudentName;
                            }
                            if (user2.Role == "M")
                            {
                                var man = db.Managers.Where(x => x.id == user2.PersonId).FirstOrDefault();
                                commentitem.HeadImg = man.HeadImage;
                                commentitem.name = man.ManagerName;
                            }
                            Clists.Add(commentitem);
                        }

                        answeritem.Comments = Clists;
                        Alists.Add(answeritem);
                    }

                    questionitem.Answers = Alists;
                    QAlists.Add(questionitem);

                }

                courseitem.QuestionsAll = QAlists;
                CoursesAllLists.Add(courseitem);
            }

            return Ok(CoursesAllLists);

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
