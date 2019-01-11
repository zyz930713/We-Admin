using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.BLL
{
    public class UsersBo
    {
        WenbaDBContext db = new WenbaDBContext();
        public string ValidateUser(Manager manager)
        {
            if (!String.IsNullOrEmpty(manager.ManagerNum))
            {
                User man = db.Users.FirstOrDefault(p => p.UserName == manager.ManagerNum);
                if (man!=null&&man.PersonId!=manager.id)
                {
                    return "员工编号：[" + manager.ManagerNum + "]已经存在，请修改后重新提交！";
                }
            }
            if (String.IsNullOrEmpty(manager.ManagerName))
            {
                return "用户姓名不能为空，请修改后重新提交！";
            }
            if (String.IsNullOrEmpty(manager.ManagerNum))
            {
                return "员工编号不能为空，请修改后重新提交！";
            }
            if (String.IsNullOrEmpty(manager.Mobile))
            {
                return "手机号不能为空，请修改后重新提交！";
            }
            if (manager.Birthday>DateTime.Now)
            {
                return "出生日期不能大于当前时间，请修改后重新提交！";
            }
            if (manager.HireDate > DateTime.Now)
            {
                return "雇佣日期不能大于当前时间，请修改后重新提交！";
            }
            return "";
        }

        public string ValidateStudent(Student student)
        {
            if (!String.IsNullOrEmpty(student.StudentNum))
            {
                User man = db.Users.FirstOrDefault(p => p.UserName == student.StudentNum);
                if (man != null && man.PersonId != student.id)
                {
                    return "学生编号：[" + student.StudentNum + "]已经存在，请修改后重新提交！";
                }
            }
            if (String.IsNullOrEmpty(student.StudentName))
            {
                return "用户姓名不能为空，请修改后重新提交！";
            }
            if (String.IsNullOrEmpty(student.StudentNum))
            {
                return "学生编号不能为空，请修改后重新提交！";
            }
            if (String.IsNullOrEmpty(student.Mobile))
            {
                return "手机号不能为空，请修改后重新提交！";
            }
            return "";
        }
    }
}