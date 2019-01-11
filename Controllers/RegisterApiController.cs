using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wenba.Models;

namespace Wenba.Controllers
{
    public class RegisterApiController : ApiController
    {
        WenbaDBContext db = new WenbaDBContext();

        //POST: 处理学生填写的个人信息
        [HttpPost]
        public IHttpActionResult PostStudent(dynamic stu)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Student student = new Student();
            student.ActiveFlag = "Y";
            //student.Birthday = stu.Birthday;
            //student.Collegue = stu.Collegue;
            student.Gender = stu.Gender;
            student.HeadImage = stu.HeadImage;
            student.Mobile = stu.Mobile;
            //student.Nationality = stu.Nationality;
            student.OpenId = stu.OpenId;
            student.StudentName = stu.StudentName;
            student.StudentNum = stu.StudentNum;
            student.CreatedBy = 0;
            student.CreationDate = DateTime.Now;
            student.LastUpdateDate = DateTime.Now;
            student.LastUpdatedBy = 0;
            db.Students.Add(student);
            db.SaveChanges();

            User stuUser = new User();
            stuUser.CreatedBy = 0;
            stuUser.CreationDate = DateTime.Now;
            stuUser.LastUpdateDate = DateTime.Now;
            stuUser.LastUpdatedBy = 0;
            stuUser.Password = "123456";
            stuUser.PersonId = student.id;
            stuUser.Role = "S";
            stuUser.StartDate = DateTime.Now;
            stuUser.UserName = student.StudentNum;
            db.Users.Add(stuUser);
            db.SaveChanges();

            StudentAssgin sAssgin = new StudentAssgin();
            sAssgin.ActiveFlag = "Y";
            string ProjectNum = stu.ProjectNum;
            var project = db.Projects.Where(x => x.ProjectNum == ProjectNum && x.Status != "C").FirstOrDefault();
            if (project == null)
            {
                Student s = db.Students.Find(student.id);
                db.Students.Remove(s);
                User u = db.Users.Find(stuUser.id);
                db.Users.Remove(u);
                db.SaveChanges();

                JObject obj1 = new JObject();
                obj1["code"] = false;
                obj1["message"] = "项目编号输入错误！";

                //string result1 = JsonConvert.SerializeObject(obj1);

                return Ok(obj1);

            }
            sAssgin.ProjectId = project.id;
            sAssgin.StudentId = student.id;
            sAssgin.CreatedBy = 0;
            sAssgin.CreationDate = DateTime.Now;
            sAssgin.LastUpdateDate = DateTime.Now;
            sAssgin.LastUpdatedBy = 0;
            db.StudentAssgins.Add(sAssgin);
            db.SaveChanges();


            JObject obj = new JObject();
            obj["code"] = true;
            obj["Role"] = "S";
            obj["UserId"] = stuUser.id;
            obj["StudentId"] = student.id;
            obj["ProjectId"] = sAssgin.ProjectId;
            obj["message"] = "注册成功!";
            return Ok(obj);
            //return CreatedAtRoute("DefaultApi", new { UserId = stuUser.id, StudengtId = student.id, ProjectId = sAssgin.ProjectId }, stu);
        }


        //POST: 处理老师填写的个人信息
        [HttpPost]
        public IHttpActionResult PostTeacher(dynamic tea)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Manager teacher = new Manager();
            teacher.ActiveFlag = "Y";
            //teacher.Birthday = tea.Birthday;
            teacher.ManagerType = "T";
            teacher.Gender = tea.Gender;
            teacher.HeadImage = tea.HeadImage;
            teacher.Mobile = tea.Mobile;
            teacher.OpenId = tea.OpenId;
            teacher.ManagerName = tea.TeacherName;
            teacher.ManagerNum = tea.TeacherNum;
            teacher.CreatedBy = 0;
            teacher.CreationDate = DateTime.Now;
            teacher.LastUpdateDate = DateTime.Now;
            teacher.LastUpdatedBy = 0;
            db.Managers.Add(teacher);
            db.SaveChanges();

            User teaUser = new User();
            teaUser.CreatedBy = 0;
            teaUser.CreationDate = DateTime.Now;
            teaUser.LastUpdateDate = DateTime.Now;
            teaUser.LastUpdatedBy = 0;
            teaUser.Password = "123456";
            teaUser.PersonId = teacher.id;
            teaUser.Role = "M";
            teaUser.StartDate = DateTime.Now;
            teaUser.UserName = teacher.ManagerNum;
            db.Users.Add(teaUser);
            db.SaveChanges();

            JObject obj = new JObject();
            obj["code"] = true;
            obj["Role"] = "T";
            obj["UserId"] = teaUser.id;
            obj["TeacherId"] = teacher.id;
            obj["message"] = "注册成功!";
            return Ok(obj);
            //return CreatedAtRoute("DefaultApi", new { UserId = teaUser.id, TeacherId = teacher.id }, tea);
        }


        //[HttpGet]
        //// GET: 学生选择加入项目
        //public IHttpActionResult GetProjectList()
        //{
        //    var pro = (from Projects in db.Projects
        //               where Projects.Status != "C"
        //               select Projects).ToList();

        //    if (pro.Count == 0)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return Json(pro);
        //    }

        //}




    }
}
