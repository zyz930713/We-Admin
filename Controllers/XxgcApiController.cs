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
    public class XxgcApiController : ApiController
    {
        private WenbaDBContext db = new WenbaDBContext();
        // GET
        public IQueryable<Question> GetQuestions()
        {
            return db.Questions;
        }

        // GET: 
        [ResponseType(typeof(Question))]
        public IHttpActionResult GetQuestion(int id)
        {
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            return Ok(question);
        }



        // PUT: 
        [ResponseType(typeof(void))]
        public IHttpActionResult PutQuestion(int id, Question question)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != question.id)
            {
                return BadRequest();
            }

            db.Entry(question).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
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

        // DELETE: 
        [ResponseType(typeof(Question))]
        public IHttpActionResult DeleteQuestion(int id)
        {
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return NotFound();
            }

            db.Questions.Remove(question);
            db.SaveChanges();

            return Ok(question);
        }


        public class QuestionsAll
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
            //是否被点赞
            public bool IsLiked;
            //点赞数量
            public int LikeCount;
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
            //是否被点赞
            public bool IsLiked;
            //点赞数量
            public int LikeCount;
            //评论人的userid
            public int UserId;
            public string name;
            public string HeadImg;

        }

        // GET: 得到学习广场的所有数据
        [HttpGet]
        public IHttpActionResult Getxxgc(int id)      //这边传的是UserId
        {
            List<QuestionsAll> Qlists = new List<QuestionsAll>();

            var user = db.Users.Where(x => x.id == id).FirstOrDefault();
            List<Question> ques = null;
            if (user.Role == "S")
            {
                var sAssign = db.StudentAssgins.Where(x => x.StudentId == user.PersonId);
                List<int> projectIdlist = new List<int>();
                projectIdlist = (from s in sAssign
                                 select s.ProjectId).ToList();
                var linq1 = from qu in db.Questions
                            join Courses in db.Courses on qu.CourseId equals Courses.id
                            join Projects in db.Projects on Courses.ProjectId equals Projects.id
                            //where Projects.id.Equals(sAssign.ProjectId)
                            where projectIdlist.Contains(Projects.id)
                            where Courses.ActiveFlag.Equals("Y")
                            where qu.Status.Contains("O")
                            where qu.QuestionType.Contains("Q")
                            select qu;
                ques = linq1.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
            }
            if (user.Role == "M")
            {
                var linq2 = from qu in db.Questions
                            join Courses in db.Courses on qu.CourseId equals Courses.id
                            where Courses.TeacherId == user.PersonId
                            where Courses.ActiveFlag.Equals("Y")
                            where qu.Status.Contains("O")
                            where qu.QuestionType.Contains("Q")
                            select qu;
                ques = linq2.OrderByDescending(c => c.CourseId).ThenBy(c => c.QuestionNum).ToList();
            }


            if (ques.Count == 0)
            {
                Qlists = null;
                return Ok(Qlists);    //没有发布的问题
            }

            foreach (Question q in ques)
            {
                QuestionsAll questionitem = new QuestionsAll();
                questionitem.CourseId = q.CourseId;
                var course = db.Courses.Where(x => x.id == q.CourseId).FirstOrDefault();
                questionitem.CourseName = course.CourseName;
                questionitem.QuestionId = q.id;
                questionitem.QuestionContent = q.QuestionDesc;
                questionitem.ReleaseTime = q.LastUpdateDate.ToString("f");
                var answerList = from Notes in db.Notes
                                 where Notes.PublicFlag == "Y" && Notes.QuestionId == q.id
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
                    Collection col = db.Collections.FirstOrDefault(p => p.NoteId == a.id && p.UserId == id);   //当前登陆者的userid ！！！
                    if (col != null)
                    {
                        answeritem.IsCollected = true;
                    }
                    else
                    {
                        answeritem.IsCollected = false;
                    }

                    Like like = db.Likes.FirstOrDefault(p => p.NoteId == a.id && p.UserId == id);   //当前登陆者的userid ！！！
                    if (like != null)
                    {
                        answeritem.IsLiked = true;
                    }
                    else
                    {
                        answeritem.IsLiked = false;
                    }

                    answeritem.LikeCount = db.Likes.Where(p => p.NoteId == a.id).ToList().Count;

                    answeritem.UserId = a.UserId;
                    var user1 = db.Users.Where(x => x.id == a.UserId).FirstOrDefault();
                    if (user1.Role == "S")
                    {
                        var stu = db.Students.Where(x => x.id == user1.PersonId).FirstOrDefault();
                        answeritem.HeadImg = stu.HeadImage;
                        answeritem.name = stu.StudentName;
                    }
                    if (user1.Role == "M")
                    {
                        var man = db.Managers.Where(x => x.id == user1.PersonId).FirstOrDefault();
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
                        Collection col2 = db.Collections.FirstOrDefault(p => p.CommentId == c.id && p.UserId == id);   //当前登陆者的userid ！！！
                        if (col2 != null)
                        {
                            commentitem.IsCollected = true;
                        }
                        else
                        {
                            commentitem.IsCollected = false;
                        }

                        Like like2 = db.Likes.FirstOrDefault(p => p.CommentId == c.id && p.UserId == id);   //当前登陆者的userid ！！！
                        if (like2 != null)
                        {
                            commentitem.IsLiked = true;
                        }
                        else
                        {
                            commentitem.IsLiked = false;
                        }
                        commentitem.LikeCount = db.Likes.Where(p => p.CommentId == c.id).ToList().Count;
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



        // POST: 针对问题的回答发表评论
        [HttpPost]
        [ResponseType(typeof(Comment))]
        public IHttpActionResult PostComment(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //comment.CommentedBy = 10063;  //Need to change to use actual user
            //comment.CreatedBy = 10063;  //Need to change to use actual user
            comment.CreationDate = DateTime.Now;
            comment.LastUpdateDate = DateTime.Now;
            //comment.LastUpdatedBy = 10063;  //Need to change to use actual user

            db.Comments.Add(comment);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = comment.id }, comment);
        }


        //POST: 收藏回答或评论
        [ResponseType(typeof(Collection))]
        public IHttpActionResult PostCollection(Collection collection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //collection.UserId = 10063;    //需要改成小程序当前登陆用户！！！
            //collection.CreatedBy = 0;   //需要改成小程序当前登陆用户！！！
            collection.CreationDate = DateTime.Now;
            collection.LastUpdateDate = DateTime.Now;
            //collection.LastUpdatedBy = 0;   //需要改成小程序当前登陆用户！！！

            db.Collections.Add(collection);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = collection.id }, collection);

        }


        //POST: 点赞
        [ResponseType(typeof(Like))]
        public IHttpActionResult PostLike(Like like)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //like.UserId = 10063;    //需要改成小程序当前登陆用户！！！
            //like.CreatedBy = 0;   //需要改成小程序当前登陆用户！！！
            like.CreationDate = DateTime.Now;
            like.LastUpdateDate = DateTime.Now;
            //like.LastUpdatedBy = 0;   //需要改成小程序当前登陆用户！！！

            db.Likes.Add(like);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = like.id }, like);

        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool QuestionExists(int id)
        {
            return db.Questions.Count(e => e.id == id) > 0;
        }

    }
}
