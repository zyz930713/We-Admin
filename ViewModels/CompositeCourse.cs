using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.ViewModels
{
    public class CompositeCourse
    {
        public Course Course { get; set; }
        public Project Project { get; set; }
        public Manager Teacher { get; set; }
    }
}