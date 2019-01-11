using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wenba.ViewModels
{
    public class QuestionsMassVM
    {
        public int ProjectId { get; set; }
        public int CourseId { get; set; }
        public string QuestionType { get; set; }
        public List<String> QuestionDesc { get; set; }
        public List<String> QuestionNum { get; set; }
        public List<String> Comments { get; set; }
    }
}