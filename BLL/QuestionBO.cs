using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.BLL
{
    public class QuestionBO
    {
        WenbaDBContext db = new WenbaDBContext();
        public string ValidateProject(List<Question> quests)
        {
            foreach(Question item in quests)
            {
                if (!String.IsNullOrEmpty(item.QuestionNum))
                {
                    Question qu = db.Questions.FirstOrDefault(p => p.QuestionNum == item.QuestionNum && p.QuestionType == item.QuestionType && p.CourseId == item.CourseId);
                    if (qu != null && qu.id!=item.id)
                    {
                        return "该课程对应的问题或通知编号：[" + item.QuestionNum + "]已经存在，请修改后重新提交！";
                    }
                }

                string course = item.CourseId + "";
                if (String.IsNullOrEmpty(course))
                {
                    return "项目和课程不能为空，请修改后重新提交！";
                }
                if (String.IsNullOrEmpty(item.QuestionDesc))
                {
                    return "内容不能为空，请修改后重新提交！";
                }
                
            }           

            return "";
        }


        //edit验证
        public string ValidateQuestionEdit(Question question)
        {

            if (!String.IsNullOrEmpty(question.QuestionNum))
            {
                Question qu = db.Questions.FirstOrDefault(p => p.QuestionNum == question.QuestionNum && p.QuestionType == question.QuestionType && p.CourseId == question.CourseId);
                if (qu != null && qu.id != question.id)
                {
                    return "该课程对应的问题或通知编号：[" + question.QuestionNum + "]已经存在，请修改后重新提交！";
                }
            }

            string course = question.CourseId + "";
            if (String.IsNullOrEmpty(course))
            {
                return "项目和课程不能为空，请修改后重新提交！";
            }
            if (String.IsNullOrEmpty(question.QuestionDesc))
            {
                return "内容不能为空，请修改后重新提交！";
            }


            return "";
        }

    }

}