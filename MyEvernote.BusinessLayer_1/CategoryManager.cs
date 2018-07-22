using MyEvernote.BusinessLayer_1.Abstract;
using MyEvernote.DataAccessLayer_1.EntityFramework;
using MyEvernote.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.BusinessLayer_1
{
  public  class CategoryManager : ManagerBase<Category>//managerbase'de category olarak repo newlenecek
    {
       public override int Delete(Category category)//Kategorinin ilişkili olduğu notlar, likelar ve commentler var önce bunlar silinip en son kategori silinmeli.Note'un da silinmesi için öncelikle like'larının ve commentlerinin silinmesi lazım.içiçe foreach ile bu sorunu çözebildik.(override ederek)
        {
            NoteManager noteManager = new NoteManager();
            LikedManager likedMananager = new LikedManager();
            CommentManager commentManager = new CommentManager();

            //Kategori ile ilişkili notların silinmesi gerekiyor.
            foreach (Note note in category.Notes.ToList())//her seferinde sildiğimizde tolist ile kendsini ayarlayıpi listeyi sildin dnemiyorum içinde hatası almamak için tolist ekledik.
            {
                //Note ile ilişkili ilk önce like ların silinmesi gereklidir.
                foreach (Liked like in note.Likes.ToList())
                {
                    likedMananager.Delete(like);
                }

                //note ile ilişkili comment'lerin silinmesi lazım
                foreach (Comment comment in note.Commnets.ToList())
                {
                    commentManager.Delete(comment);
                }
                noteManager.Delete(note);
            }
            return base.Delete(category);
        }
    }
}
