using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wenba.Models;

namespace Wenba.Controllers
{
    public class RankApiController : ApiController
    {
        WenbaDBContext db = new WenbaDBContext();

        public class RankInfo
        {
            public int UserId;
            public int StudentId;
            public string name;
            public string HeadImg;

            public int QuestionCount;
            public int LikeCount;
            public int AnswerCount;
            //总分
            public int score;
        }



        //GET:  排行榜信息
        [HttpGet]
        public IHttpActionResult RankList(int id)     //这边传的是UserId
        {
            List<RankInfo> Rlists = new List<RankInfo>();

            var currentuser = db.Users.Where(x => x.id == id).FirstOrDefault();
            //int projectId = 0;
            List<int> projectIdlist = new List<int>();
            if (currentuser.Role == "S")
            {
                //var sAssign = db.StudentAssgins.Where(x => x.StudentId == currentuser.PersonId && x.ActiveFlag == "Y").FirstOrDefault();
                //projectId = sAssign.ProjectId;
                var sAssign = db.StudentAssgins.Where(x => x.StudentId == currentuser.PersonId).ToList();
                projectIdlist = (from s in sAssign
                                 select s.ProjectId).ToList();

            }
            if (currentuser.Role == "M")
            {
                //var course = db.Courses.Where(x => x.TeacherId == currentuser.PersonId && x.ActiveFlag == "Y").FirstOrDefault();
                //projectId = course.ProjectId;
                var course = db.Courses.Where(x => x.TeacherId == currentuser.PersonId && x.ActiveFlag == "Y");
                projectIdlist = (from s in course
                                 select s.ProjectId).Distinct().ToList();

            }

            //var stu = from StudentAssgins in db.StudentAssgins
            //          where StudentAssgins.ProjectId == projectId
            //          where StudentAssgins.ActiveFlag == "Y"
            //          select StudentAssgins;
            var stu = from StudentAssgins in db.StudentAssgins
                      where projectIdlist.Contains(StudentAssgins.ProjectId)
                      where StudentAssgins.ActiveFlag == "Y"
                      select StudentAssgins;
            List<StudentAssgin> stuList = stu.ToList();
            if (stuList.Count == 0)
            {
                Rlists = null;
                return Ok(Rlists);    //该项目下没有学生
            }

            foreach (StudentAssgin s in stuList)
            {
                RankInfo rankitem = new RankInfo();
                rankitem.StudentId = s.StudentId;
                var user = db.Users.Where(x => x.PersonId == s.StudentId && x.Role == "S").FirstOrDefault();
                rankitem.UserId = user.id;
                var student = db.Students.Where(x => x.id == s.StudentId && x.ActiveFlag == "Y").FirstOrDefault();
                rankitem.name = student.StudentName;
                rankitem.HeadImg = student.HeadImage;

                var linq1 = from Questions in db.Questions
                            where Questions.CreatedBy == user.id
                            where Questions.QuestionType.Contains("Q")
                            where Questions.Status.Contains("O")
                            select Questions;
                rankitem.QuestionCount = linq1.ToList().Count;

                var linq2 = from Likes in db.Likes
                            join Notes in db.Notes on Likes.NoteId equals Notes.id
                            where Notes.UserId == user.id
                            select Likes;
                var linq2_ = from Likes in db.Likes
                             join Comments in db.Comments on Likes.CommentId equals Comments.id
                             where Comments.CommentedBy == user.id
                             select Likes;
                rankitem.LikeCount = linq2.ToList().Count + linq2_.ToList().Count;

                var linq3 = from a in db.Notes
                            where a.UserId == user.id
                            where a.QuestionId != null
                            select a;
                rankitem.AnswerCount = linq3.ToList().Count;

                rankitem.score = rankitem.QuestionCount * 10 + rankitem.LikeCount + rankitem.AnswerCount * 10;
                Rlists.Add(rankitem);

            }
            Rlists = Rlists.OrderByDescending(o => o.score).ToList();  //降序

            return Ok(Rlists);
        }

    }
}
