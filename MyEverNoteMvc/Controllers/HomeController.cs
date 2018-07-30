using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEvernote.BusinessLayer_1;
using MyEvernote.Entities;
using System.Net;
using MyEverNoteMvc.Entities.ValueObjects;
using MyEvernote.Entities_1.Messages;
using MyEverNoteMvc.ViewModels;
using MyEvernote.BusinessLayer_1.Results;
using MyEverNoteMvc.Models;

namespace MyEverNoteMvc.Controllers
{
    public class HomeController : Controller
    {
        private NoteManager noteManager = new NoteManager();
        private CategoryManager categoryManager = new CategoryManager();
        private EvernoteUserManager evernoteUserManager = new EvernoteUserManager();

        // GET: Home
        public ActionResult Index()
        {
            //if (TempData["mm"] != null)
            //{
            //    return View(TempData["mm"] as List<Note>);
            //}
            /* Test test = new Test();
             //test.InsertTest();
             //test.UpdateTest();
             test.DeleteTest();*/

          
            return View(noteManager.ListQueryable().OrderByDescending(x => x.ModifiedOn).ToList());
            //return View(nm.GetAllNoteQueryable().OrderByDescending(x=>x.ModifiedOn).ToList());
        }
        public ActionResult ByCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           
            Category cat = categoryManager.Find(x => x.Id == id.Value);

            if (cat == null)
            {
                return HttpNotFound();
                //return Redirect.RedirectToAction("Index", "Home");
            }

            return View("Index", cat.Notes.OrderByDescending(x => x.ModifiedOn).ToList());
        }
        public ActionResult MostLiked()
        {
           
            return View("Index", noteManager.ListQueryable().OrderByDescending(x => x.LikeCount).ToList());
        }
        public ActionResult About()
        {
            return View();

        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)//model uygunsa
            {
               
                BusinessLayerResult<EvernoteUser> res = evernoteUserManager.LoginUser(model);
                if (res.Errors.Count > 0)
                {
                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    if (res.Errors.Find(x => x.Code == ErrorMessageCode.UserIsNotActive) != null)
                    {
                        ViewBag.SetLink = "E-Posta Gönder";//hata kodu bu şekilde düzenlenebilir!!
                    }
                    return View(model);
                }
                CurrentSession.Set<EvernoteUser>("login", res.Result);//session'a kullanıcı bilgi saklama
                return RedirectToAction("Index");//yönlendirme...
            }
            return View(model);
        }

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                BusinessLayerResult<EvernoteUser> res = evernoteUserManager.RegisterUser(model);
                if (res.Errors.Count > 0)
                {
                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    return View(model);
                }

                //EvernoteUser user = null;
                //try
                //{
                //   user=eum.RegisterUser(model);
                //}
                //catch (Exception ex)
                //{

                //    ModelState.AddModelError("", ex.Message);
                //}
                //if (model.UserName == "aaa")
                //{
                //    ModelState.AddModelError("", "Kullanıcı adı kullanılıyor.");

                //}
                //if (model.EMail == "aaa@aa.com")
                //{
                //    ModelState.AddModelError("", "E-mail adresi kullanılıyor.");

                //}
                //foreach (var item in ModelState)
                //{
                //    if (item.Value.Errors.Count > 0)
                //    {
                //        return View(model);
                //    }
                //}
                //if (user==null)
                //{
                //    return View(model);
                //}
                OkViewModel notifyObj = new OkViewModel()
                {
                    Title = "Kayıt Başarılı",
                    RedirectingUrl="/Home/Login",
                   
                };
                notifyObj.Items.Add("  Lütfen e-posta adresinize gönderdiğimiz aktivasyon linkine tıklayarak hesabınızı aktive ediniz.Hesabınızı aktive etmeden not eklyemez ve beğenme yapamazsınız.");
                return View("Ok", notifyObj);
            }
            return View(model);
        }
     

        public ActionResult UserActivate(Guid id)
        {
          
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.ActivateUser(id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel notifyObj = new ErrorViewModel()
                {
                    Title="Geçersiz İşlem",
                    Items=res.Errors
                };
               
                return RedirectToAction("Error", notifyObj);

            }
            OkViewModel okNotifyObj = new OkViewModel {
                Title= " Hesabınız aktifleştirildi",
                RedirectingUrl="/Home/Login",
            };
            okNotifyObj.Items.Add("Hesabınız aktifleştirildi.Artık not paylaşabilir ve beğenme yapabilirsiniz.");
            return RedirectToAction("Ok", okNotifyObj);
        }
      
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
        public ActionResult ShowProfile()
        {
            BusinessLayerResult<EvernoteUser> res= evernoteUserManager.GetUserById(CurrentSession.User.Id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Geçersiz İşlem",
                    Items = res.Errors
                };
                return View("Error", errorNotifyObj);
            }

            return View(res.Result);
        }
        public ActionResult EditProfile()
        {
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.GetUserById(CurrentSession.User.Id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Geçersiz İşlem",
                    Items = res.Errors
                };
                return View("Error", errorNotifyObj);
            }

            return View(res.Result);
          
        }
        [HttpPost]
        public ActionResult EditProfile(EvernoteUser model, HttpPostedFileBase ProfileImage)
        {
            ModelState.Remove("ModifiedUsername");//sayfayı kontrol ederken modeldeki bütün zorunlu alanlara bakar.Bu alan ilgili sayfada olmadığı için kontrolden önce remove yapıyoruz.
            if (ModelState.IsValid)
            {
                if (ProfileImage != null &&
               (ProfileImage.ContentType == "image/jpeg" ||
                ProfileImage.ContentType == "image/jpg" ||
                ProfileImage.ContentType == "image/png"))
                {
                    string filename = $"user_{model.Id}.{ProfileImage.ContentType.Split('/')[1]}";

                    ProfileImage.SaveAs(Server.MapPath($"~/iamges/{filename}"));
                    model.ProfileImageFilename = filename;

                }
                
                BusinessLayerResult<EvernoteUser> res = evernoteUserManager.UpdateProfile(model);
                if (res.Errors.Count > 0)
                {
                    ErrorViewModel errorNotifyObj = new ErrorViewModel()
                    {
                        Items = res.Errors,
                        Title = "Profil Güncellenemedi",
                        RedirectingUrl = "/Home/EditProfile"
                    };
                    return View("Error", errorNotifyObj);
                }
                CurrentSession.Set<EvernoteUser>("login", res.Result);//Profil güncellendiği için session güncellendi.

               
            }return RedirectToAction("ShowProfile");
        } 
        public ActionResult DeleteProfile()
        {
            BusinessLayerResult<EvernoteUser> res = evernoteUserManager.RemoveUserById(CurrentSession.User.Id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Title = "Profil silinemedi",
                    Items = res.Errors,
                    RedirectingUrl="/Home/ShowProfile"
                };
                return View("Error", errorNotifyObj);
            }

            Session.Clear();
            return RedirectToAction("Index");
        }

        public ActionResult TestNotify()
        {
            ErrorViewModel model = new ErrorViewModel() {
                Header = "Yönlendirme",
                Title="Ok Test",
                RedirectingTimeout=10000,
                Items=new List<ErrorMessageObj>()
                {
                    new ErrorMessageObj(){Message="Test başarılı"},
                    new ErrorMessageObj(){ Message="2.Test de başarılı"}
                    
                }
            };

            return View("Error", model);
        }

    }
}