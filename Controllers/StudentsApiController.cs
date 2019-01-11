using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Wenba.Models;
using Wenba.ViewModels;

namespace Wenba.Controllers
{
    public class StudentsApiController : ApiController
    {
        private WenbaDBContext db = new WenbaDBContext();

        // GET: api/Students
        public IQueryable<Student> GetStudents()
        {
            return db.Students;
        }

        [HttpGet]
        // GET: api/Students/5
        public IHttpActionResult GetStuByProj(int id)
        {
            List<StudentAssgin> stuAssignList = db.StudentAssgins.Where(sa => sa.ProjectId == id).ToList();
            List<Student> studentList = new List<Student>();
            StudentListJson stuListJ = new StudentListJson();

            if (stuAssignList == null)
            {
                // return "Not Found";
                return NotFound();
            }
            else
            {
                foreach(var sa in stuAssignList)
                {
                    Student stu = db.Students.FirstOrDefault(s => s.id == sa.StudentId);
                    studentList.Add(stu);
                }
                stuListJ.data = studentList;
                stuListJ.code = 0;
                stuListJ.msg = "";
                stuListJ.count = studentList.Count();
            }

            return Json<StudentListJson>(stuListJ);
        }

        // PUT: api/Students/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStudent(int id, Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != student.id)
            {
                return BadRequest();
            }

            db.Entry(student).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Students
        [ResponseType(typeof(Student))]
        public IHttpActionResult PostStudent(Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Students.Add(student);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = student.id }, student);
        }

        // DELETE: api/Students/5
        [ResponseType(typeof(StudentAssgin))]
        public IHttpActionResult DeleteStudent(int id)
        {
            StudentAssgin student = db.StudentAssgins.Where(x=>x.StudentId== id).FirstOrDefault();
            if (student == null)
            {
                return NotFound();
            }

            db.StudentAssgins.Remove(student);
            db.SaveChanges();

            return Ok(student);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StudentExists(int id)
        {
            return db.Students.Count(e => e.id == id) > 0;
        }
    }
}