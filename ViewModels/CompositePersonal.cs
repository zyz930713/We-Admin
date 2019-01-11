using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wenba.Models;

namespace Wenba.ViewModels
{
    public class CompositePersonal
    {
        public User User { get; set; }
        public Manager Manager { get; set; }
        public Student Student { get; set; }

    }
}