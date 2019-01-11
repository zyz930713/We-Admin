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

namespace Wenba.Controllers
{
    public class NotesApiController : ApiController
    {
        private WenbaDBContext db = new WenbaDBContext();

        // GET: api/NotesApi
        public IQueryable<Note> GetNotes()
        {
            return db.Notes;
        }

        //[HttpGet]
        //// GET: api/Note/5 查看我的笔记
        //public IHttpActionResult GetNoteByUser(int id)
        //{
        //    List<Note> notes = db.Notes.Where(a => a.QuestionId == null && a.UserId == id).ToList();

        //    if (notes == null)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Json(notes);
        //    }

        //}

        //[HttpGet]
        //// GET: api/Note/5 查看我的回答
        //public IHttpActionResult GetAnswerByUser(int id)
        //{
        //    List<Note> notes = db.Notes.Where(a => a.QuestionId != null && a.UserId == id).ToList();

        //    if (notes == null)
        //    {
        //        // return "Not Found";
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Json(notes);
        //    }

        //}

        // GET: api/NotesApi/5
        [ResponseType(typeof(Note))]
        public IHttpActionResult GetNote(int id)
        {
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        // PUT: api/NotesApi/5   更新
        [ResponseType(typeof(void))]
        public IHttpActionResult PutNote(int id, Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != note.id)
            {
                return BadRequest();
            }

            db.Entry(note).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
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

        // POST: api/NotesApi   添加
        [ResponseType(typeof(Note))]
        public IHttpActionResult PostNote(Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //note.UserId = 10063;  //Need to change to use actual user ！！！
            //note.CreatedBy = 10063;  //Need to change to use actual user ！！！
            note.CreationDate = DateTime.Now;
            note.LastUpdateDate = DateTime.Now;
            //note.LastUpdatedBy = 10063;  //Need to change to use actual user ！！！

            db.Notes.Add(note);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = note.id }, note);
        }

        //GET: 显示标签
        public IHttpActionResult GetLabels()
        {
            List<Label> labels = db.Labels.Where(a => a.LabelType == "S").ToList();

            if (labels == null)
            {
                return NotFound();
            }
            else
            {
                return Json(labels);
            }
        }


        // DELETE: api/NotesApi/5
        [ResponseType(typeof(Note))]
        public IHttpActionResult DeleteNote(int id)
        {
            Note note = db.Notes.Find(id);
            if (note == null)
            {
                return NotFound();
            }

            db.Notes.Remove(note);
            db.SaveChanges();

            return Ok(note);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NoteExists(int id)
        {
            return db.Notes.Count(e => e.id == id) > 0;
        }
    }
}