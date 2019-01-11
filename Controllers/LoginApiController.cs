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
    public class LoginApiController : ApiController
    {
        WenbaDBContext db = new WenbaDBContext();

        //public class UserInfo
        //{
        //    public string Role;
        //    public int UserId;
        //    public int StudentId;
        //    public int projectId;
        //    public int TeacherId;

        //}

        //GET: 登陆验证
        [HttpGet]
        public IHttpActionResult Login(string id)
        {
            var stu = db.Students.Where(x => x.OpenId == id).FirstOrDefault();
            var tea = db.Managers.Where(x => x.OpenId == id && x.ActiveFlag == "Y" && x.ManagerType == "T").FirstOrDefault();

            if (stu == null && tea == null)
            {
                JObject obj = new JObject();
                obj["code"] = false;
                obj["message"] = "该用户还未注册！";
                return Ok(obj);
                //return Ok("该用户未提交注册信息");
            }

            if (stu != null && tea == null)
            {
                //UserInfo student = new UserInfo();
                var user = db.Users.Where(x => x.Role == "S" && x.PersonId == stu.id).FirstOrDefault();
                var sAssgin = db.StudentAssgins.Where(x => x.StudentId == stu.id && x.ActiveFlag == "Y").FirstOrDefault();

                JObject obj = new JObject();
                obj["code"] = true;
                obj["Role"] = "S";
                obj["UserId"] = user.id;
                obj["StudentId"] = stu.id;
                obj["ProjectId"] = sAssgin.ProjectId;
                obj["message"] = "该用户已注册为学生";

                return Ok(obj);
                //return Ok("true");
                //return Ok("该用户已注册为学生");
            }

            if (stu == null && tea != null)
            {
                var user = db.Users.Where(x => x.Role == "M" && x.PersonId == tea.id).FirstOrDefault();
                JObject obj = new JObject();
                obj["code"] = true;
                obj["Role"] = "T";
                obj["UserId"] = user.id;
                obj["TeacherId"] = tea.id;
                obj["message"] = "该用户已注册为老师";

                return Ok(obj);
                //return Ok("该用户已注册为老师");
            }

            if (stu != null && tea != null)
            {
                JObject obj = new JObject();
                obj["code"] = false;
                obj["message"] = "该用户重复注册了两种身份！";
                return Ok(obj);
                //return Ok("该用户信息有误");
            }

            return Ok();
        }

    }
}
