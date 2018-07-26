﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MyEverNoteMvc.Models;
using MyEvernote.Entities;
using MyEvernote.BusinessLayer_1;

namespace MyEverNoteMvc.Controllers
{
    public class NoteController : Controller
    {
        private NoteManager noteManager = new NoteManager();
      
        public ActionResult Index()//Notları listeler
        {
            return View(noteManager.List());
        }
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = noteManager.Find(x => x.Id == id.Value);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

       
        public ActionResult Create()
        {
          
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Note note)
        {
            ModelState.Remove("ModifiedUsername");
            ModelState.Remove("ModifiedOn");
            ModelState.Remove("CreatedOn");
            if (ModelState.IsValid)
            {
                noteManager.Insert(note);
                return RedirectToAction("Index");
            }
            
            return View(note);
        }

  
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = noteManager.Find(x => x.Id == id.Value);
            if (note == null)
            {
                return HttpNotFound();
            }
           
            return View(note);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Note note)
        {
            ModelState.Remove("ModifiedUsername");
            ModelState.Remove("ModifiedOn");
            ModelState.Remove("CreatedOn");
            if (ModelState.IsValid)
            {
                Note not = noteManager.Find(x => x.Id == note.Id);
                not.Title = note.Title;
                not.Text = note.Text;
                noteManager.Update(not);
                return RedirectToAction("Index");
            }
          
            return View(note);
        }

      
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Note note = noteManager.Find(x=>x.Id == id.Value);
            if (note == null)
            {
                return HttpNotFound();
            }
            return View(note);
        }

      
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Note note = noteManager.Find(x => x.Id == id);
            noteManager.Delete(note);
            return RedirectToAction("Index");
        }

      
    }
}
