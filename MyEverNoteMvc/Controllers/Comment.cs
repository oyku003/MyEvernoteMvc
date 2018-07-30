using MyEvernote.BusinessLayer_1;
using MyEvernote.Entities;

using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyEverNoteMvc.Controllers
{
    public class Comment : Controller
    {
        private NoteManager noteManager = new NoteManager();
        public ActionResult ShowNoteComments(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Note note = noteManager.ListQueryable().Include("Comments").FirstOrDefault(x=>x.Id == id);
            Note note = noteManager.Find(x => x.Id == id);
            if (note == null)
            {
                return HttpNotFound();
            }
            return PartialView("_PartialComments", note.Commnets);
        }
    }
}