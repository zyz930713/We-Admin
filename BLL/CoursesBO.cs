using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.BLL
{
    public class CoursesBO
    {
        WenbaDBContext db = new WenbaDBContext();
        //create验证
        public string ValidateCourse(Course course)
        {
            if (!String.IsNullOrEmpty(course.CourseNum))
            {
                Course cou = db.Courses.FirstOrDefault(p => p.CourseNum == course.CourseNum && p.ProjectId == course.ProjectId);
                if (cou != null && cou.id!= course.id)
                {
                    return "该项目下的课程编号：["+ course.CourseNum+"]已经存在，请修改后重新提交！";
                }
            }
            if(course.StartDate !=null && course.EndDate != null)
            {
                if(course.StartDate > course.EndDate)
                {
                    return "课程开始日期不能大于项目结束日期，请修改后重新提交！";
                }
            }
            return "";
        }


        ////edit验证
        //public string ValidateCourseEdit(Course course)
        //{
           
        //    if (course.StartDate != null && course.EndDate != null)
        //    {
        //        if (course.StartDate > course.EndDate)
        //        {
        //            return "课程开始日期不能大于项目结束日期，请修改后重新提交！";
        //        }
        //    }
        //    return "";
        //}
    }
}