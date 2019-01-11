using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.ViewModels
{
    public class StudentListJson
    {
        public int code { get; set; }
        public string msg { get; set; }
        public int count { get; set; }
        public List<Student> data { get; set; }
    }
}