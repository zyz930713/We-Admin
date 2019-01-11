using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Wenba.Models;

namespace Wenba.Controllers
{
    public class MyDataApiController : ApiController
    {
        private WenbaDBContext db = new WenbaDBContext();

        [HttpGet]
        // GET: api/Note/5 查看我的笔记
        public IHttpActionResult GetMyNotes(int id)    //这边传的是UserId
        {
            List<Note> notes = db.Notes.Where(a => a.QuestionId == null && a.UserId == id).OrderByDescending(a => a.CreationDate).ToList();
            var notelist = (from c in notes
                            select new
                            {
                                NoteId = c.id,
                                UserId = c.UserId,
                                CourseId = c.CourseId,
                                Notes = c.Notes,
                                PublicFlag = c.PublicFlag,
                                CreationDate = c.CreationDate.ToString("f"),
                                LastUpdateDate = c.LastUpdateDate.ToString("f")
                            }).ToList();

            if (notes.Count == 0)
            {
                notelist = null;
                return Json(notelist);
            }
            else
            {
                return Json(notelist);
            }

        }


        // PUT: api/NotesApi/5   修改我的笔记
        [ResponseType(typeof(void))]
        public IHttpActionResult PutNote(int id, Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != note.id)
            {
                return BadRequest();
            }

            db.Entry(note).State = EntityState.Modified;
            var entry = db.Entry(note);
            //以下字段不更新
            entry.Property(e => e.UserId).IsModified = false;
            entry.Property(e => e.CourseId).IsModified = false;
            entry.Property(e => e.LikeNum).IsModified = false;
            entry.Property(e => e.QuestionId).IsModified = false;
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

            note.LastUpdateDate = DateTime.Now;
            note.LastUpdatedBy = 10063;      //需要改成小程序当前登陆者的ID ！！！

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }



        [HttpGet]
        // GET: 我的学习记录，即课程列表
        public IHttpActionResult GetCoursesList(int id)    //这边传的是UserId
        {
            var currentuser = db.Users.Where(x => x.id == id).FirstOrDefault();
            List<Course> courses = null;

            if (currentuser.Role == "S")
            {
                var sAssign = db.StudentAssgins.Where(x => x.StudentId == currentuser.PersonId);
                List<int> projectIdlist = new List<int>();
                projectIdlist = (from s in sAssign
                                 select s.ProjectId).ToList();
                //int projectId = sAssign.ProjectId;
                var cou = from Courses in db.Courses
                          join Projects in db.Projects on Courses.ProjectId equals Projects.id
                          //where Projects.id.Equals(projectId)
                          where projectIdlist.Contains(Projects.id)
                          where Courses.ActiveFlag.Equals("Y")
                          select Courses;
                courses = cou.OrderBy(a => a.StartDate).ToList();
            }
            if (currentuser.Role == "M")
            {
                var cou = from Courses in db.Courses
                          where Courses.TeacherId == currentuser.PersonId
                          where Courses.ActiveFlag.Equals("Y")
                          select Courses;
                courses = cou.OrderBy(a => a.StartDate).ToList();
            }

            var linq = (from c in courses
                        select new
                        {
                            CourseId = c.id,
                            CourseName = c.CourseName,
                            StartDate = c.StartDate.ToString(),
                            Site = c.Site
                        }).ToList();

            if (linq == null)
            {
                return NotFound();
            }
            else
            {
                return Json(linq);
            }

        }


        public class MyQuestions
        {
            //课程ID
            public int? CourseId;
            public string CourseName;
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
            public string AnswerTime;
            //是否被收藏
            public bool IsCollected;
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
            //是否被收藏
            public bool IsCollected;
            //评论人的userid
            public int UserId;
            public string name;
            public string HeadImg;

        }

        // GET: Questions/5 我的提问
        [HttpGet]
        public IHttpActionResult GetMyQuestions(int id)      //当前登陆者的id ！！！
        {
            List<MyQuestions> Qlists = new List<MyQuestions>();
            var qus = from qu in db.Questions
                      join Courses in db.Courses on qu.CourseId equals Courses.id
                      join Projects in db.Projects on Courses.ProjectId equals Projects.id
                      where qu.CreatedBy.Equals(id)
                      where Courses.ActiveFlag.Equals("Y")
                      where qu.Status.Contains("O")
                      where qu.QuestionType.Contains("Q")
                      select qu;
            List<Question> ques = qus.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
            if (ques.Count == 0)
            {
                Qlists = null;
                return Ok(Qlists);    //我没有发布问题
            }

            foreach (Question q in ques)
            {
                MyQuestions questionitem = new MyQuestions();
                questionitem.CourseId = q.CourseId;
                var course = db.Courses.Where(x => x.id == q.CourseId).FirstOrDefault();
                questionitem.CourseName = course.CourseName;
                questionitem.QuestionId = q.id;
                questionitem.QuestionContent = q.QuestionDesc;
                questionitem.ReleaseTime = q.LastUpdateDate.ToString("f");
                var answerList = from Notes in db.Notes
                                 //where Notes.PublicFlag == "Y" && Notes.QuestionId == q.id
                                 where Notes.QuestionId == q.id
                                 select Notes;
                if (answerList.ToList().Count == 0)
                {
                    questionitem.IsAnswered = false;    //该问题暂无回答
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
                    answeritem.AnswerTime = a.CreationDate.ToString("f");

                    answeritem.UserId = a.UserId;
                    var user = db.Users.Where(x => x.id == a.UserId).FirstOrDefault();
                    if (user.Role == "S")
                    {
                        var stu = db.Students.Where(x => x.id == user.PersonId).FirstOrDefault();
                        answeritem.HeadImg = stu.HeadImage;
                        answeritem.name = stu.StudentName;
                    }
                    if (user.Role == "M")
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
                Qlists.Add(questionitem);

            }
            return Ok(Qlists);
        }


        public class MyAnswer
        {
            //课程ID
            public int? CourseId;
            public string CourseName;
            public string CouStartTime;
            //问题ID
            public int? QuestionId;
            public string QuestionContent;
            public string QuReleaseTime;

            //回答ID
            public int? AnswerId;
            public string Answer;
            public string AnswerTime;
            //答案是否公开
            public string PublicFlag;
            //我的userid
            public int UserId;
            public string name;
            public string HeadImg;
            //是否有评论
            public bool IsCommented;
            public List<AnswerComments> Comments;

        }
        public class AnswerComment
        {
            //评论ID
            public int? CommentId;
            public string Comment;
            public string CommentTime;
            //是否被收藏
            public bool IsCollected;
            //评论人的userid
            public int UserId;
            public string name;
            public string HeadImg;

        }


        //我的回答
        [HttpGet]
        public IHttpActionResult GetMyAnswers(int id)    //当前登陆者的id ！！！
        {
            List<MyAnswer> myanswerlist = new List<MyAnswer>();
            var myanswers = from Notes in db.Notes
                            join Questions in db.Questions on Notes.QuestionId equals Questions.id
                            join Courses in db.Courses on Questions.CourseId equals Courses.id
                            join Projects in db.Projects on Courses.ProjectId equals Projects.id
                            where Notes.UserId == id
                            select new MyAnswer
                            {
                                CourseId = Courses.id,
                                CourseName = Courses.CourseName,

                                QuestionId = Questions.id,
                                QuestionContent = Questions.QuestionDesc,

                                AnswerId = Notes.id,
                                Answer = Notes.Notes,

                                PublicFlag = Notes.PublicFlag,
                                UserId = id,
                            };

            List<MyAnswer> notelist = myanswers.OrderByDescending(c => c.CourseId).ToList();
            if (notelist.Count == 0)
            {
                myanswerlist = null;
                return Ok(myanswerlist);    //我没有回答任何问题
            }

            foreach (MyAnswer n in notelist)
            {
                MyAnswer myansweritem = new MyAnswer();
                myansweritem.CourseId = n.CourseId;
                myansweritem.CourseName = n.CourseName;
                var cou = db.Courses.Where(x => x.id == n.CourseId).FirstOrDefault();
                myansweritem.CouStartTime = cou.StartDate.ToString();
                myansweritem.QuestionId = n.QuestionId;
                myansweritem.QuestionContent = n.QuestionContent;
                var qu = db.Questions.Where(x => x.id == n.QuestionId).FirstOrDefault();
                myansweritem.QuReleaseTime = qu.LastUpdateDate.ToString("f");
                myansweritem.AnswerId = n.AnswerId;
                myansweritem.Answer = n.Answer;
                var an = db.Notes.Where(x => x.id == n.AnswerId).FirstOrDefault();
                myansweritem.AnswerTime = an.LastUpdateDate.ToString("f");
                myansweritem.PublicFlag = n.PublicFlag;
                myansweritem.UserId = n.UserId;
                var user = db.Users.Where(x => x.id == n.UserId).FirstOrDefault();
                if (user.Role == "S")
                {
                    var stu = db.Students.Where(x => x.id == user.PersonId).FirstOrDefault();
                    myansweritem.HeadImg = stu.HeadImage;
                    myansweritem.name = stu.StudentName;
                }
                if (user.Role == "M")
                {
                    var man = db.Managers.Where(x => x.id == user.PersonId).FirstOrDefault();
                    myansweritem.HeadImg = man.HeadImage;
                    myansweritem.name = man.ManagerName;
                }

                var commentList = from Comments in db.Comments
                                  where Comments.PublicFlag == "Y" && Comments.NoteId == n.AnswerId
                                  select Comments;
                if (commentList.ToList().Count == 0)
                {
                    myansweritem.IsCommented = false;    //该回答暂无评论
                    myansweritem.Comments = null;
                }
                else
                {
                    myansweritem.IsCommented = true;
                }

                List<AnswerComments> Clists = new List<AnswerComments>();
                foreach (Comment c in commentList.ToList())
                {
                    AnswerComments commentitem = new AnswerComments();
                    commentitem.CommentId = c.id;
                    commentitem.Comment = c.Comments;
                    commentitem.CommentTime = c.CreationDate.ToString("f");
                    Collection col2 = db.Collections.FirstOrDefault(p => p.CommentId == c.id && p.UserId == id);   //当前登陆者的userid ！！！
                    if (col2 != null)
                    {
                        commentitem.IsCollected = true;
                    }
                    else
                    {
                        commentitem.IsCollected = false;
                    }

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
                myansweritem.Comments = Clists;
                myanswerlist.Add(myansweritem);
            }
            return Ok(myanswerlist);

        }


        public class StudyData
        {
            public int userId;
            public int QuestionCount;
            public int AnswerCount;
            public int NoteCount;
        }

        //学习数据
        [HttpGet]
        public IHttpActionResult GetStudyData(int id)    //当前登陆者的id ！！！
        {
            StudyData mystuData = new StudyData();
            mystuData.userId = id;

            var qu = from Questions in db.Questions
                     where Questions.LastUpdatedBy == id
                     where Questions.Status.Contains("O")
                     where Questions.QuestionType.Contains("Q")
                     select Questions;
            mystuData.QuestionCount = qu.ToList().Count;

            var an = from answer in db.Notes
                     where answer.UserId == id
                     where answer.QuestionId != null
                     select answer;
            mystuData.AnswerCount = an.ToList().Count;

            var no = from Notes in db.Notes
                     where Notes.UserId == id
                     where Notes.QuestionId == null
                     select Notes;
            mystuData.NoteCount = no.ToList().Count;

            return Ok(mystuData);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NoteExists(int id)
        {
            return db.Notes.Count(e => e.id == id) > 0;
        }

    }
}
