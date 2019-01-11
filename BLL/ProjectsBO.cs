using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.BLL
{
    public class ProjectsBO
    {
        WenbaDBContext db = new WenbaDBContext();
        public string ValidateProject(Project project)
        {
            if (!String.IsNullOrEmpty(project.ProjectNum))
            {
                Project proj = db.Projects.FirstOrDefault(p => p.ProjectNum == project.ProjectNum);
                if (proj != null && project.id != proj.id)
                {
                    return "项目编号：[" + project.ProjectNum + "]已经存在，请修改后重新提交！";
                }
            }
            if(project.StartDate !=null && project.EndDate != null)
            {
                if(project.StartDate >= project.EndDate)
                {
                    return "项目开始日期不能大于项目结束日期，请修改后重新提交！";
                }
            }
            return "";
        }

       
    }
}