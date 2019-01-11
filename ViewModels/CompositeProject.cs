using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.ViewModels
{
    public class CompositeProject
    {
        public Project Project { get; set; }
        public Manager Manager { get; set; }

        public Manager CustomerManager { get; set; }
        public Dictionary StatusDic { get; set; }
        public List<Student> Students { get; set; }
        public List<StudentAssgin> StudentAssigns { get; set; }
    }
}