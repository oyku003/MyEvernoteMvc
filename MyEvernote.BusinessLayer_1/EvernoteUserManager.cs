using MyEvernote.Common.Helpers;
using MyEvernote.DataAccessLayer_1.EntityFramework;
using MyEvernote.Entities;
using MyEvernote.Entities_1.Messages;
using MyEverNoteMvc.Entities.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEvernote.BusinessLayer_1
{

    public class EvernoteUserManager
    {
        private Repository<EvernoteUser> repo_user = new Repository<EvernoteUser>();

        public BusinessLayerResult<EvernoteUser> RegisterUser(RegisterViewModel data)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            EvernoteUser user = repo_user.Find(x => x.Username == data.UserName || x.Email == data.EMail);
            if (user != null)
            {
                if (user.Username == data.UserName)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı");
                }
                if (user.Email == data.EMail)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.EmailAlreadyExists, "E-Mail zaten kayıtlı");
                }
            }
            else
            {
                int dbResult = repo_user.Insert(new EvernoteUser()
                {
                    Username = data.UserName,
                    Email = data.EMail,
                    ProfileImageFilename = "user.png",
                    Password = data.Password,
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = false,
                    IsAdmin = false,

                });
                if (dbResult > 0)
                {
                    res.Result = repo_user.Find(x => x.Email == data.EMail && x.Username == data.UserName);

                    string siteUri = ConfigHelper.Get<string>("SiteRootUri");
                    string activateUri = $"{siteUri}/Home/UserActivate/{res.Result.ActivateGuid}";
                    string body = $"Merhaba {res.Result.Username};<br><br>Hesabınızı aktifleştirmek için  <a href='{activateUri}' target='_blank'>tıklayınız</a>.";
                    MailHelper.SendMail(body, res.Result.Email, "MyEvernote Hesap Aktifleştirme");
                }


            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> LoginUser(LoginViewModel data)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            res.Result = repo_user.Find(x => x.Username == data.UserName && x.Password == data.Password);

            if (res.Result != null)
            {
                if (!res.Result.IsActive)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UserIsNotActive, "Kullanıcı aktifleştirilmemiştir.");
                    res.AddError(Entities_1.Messages.ErrorMessageCode.CheckYourEmail, "Lütfen e-posta adresinizi kontrol ediniz.");
                }

            }
            else
            {
                res.AddError(Entities_1.Messages.ErrorMessageCode.UsernameOrPassWrong, "Kullanıcı adı ya da şifreniz uyuşmuyor.");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> ActivateUser(Guid activateId)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = repo_user.Find(x => x.ActivateGuid == activateId);

            if (res.Result != null)
            {
                if (res.Result.IsActive)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UserAlreadyActive, "Kullanıcı zaten aktif kullanıcıdır");
                    return res;
                }
                res.Result.IsActive = true;//ilgili kullanıcının ısActivate özelliğini true'ya set ettik.
                repo_user.Update(res.Result);
            }
            else//aktive ıd si rastgele girilmesi durumu için
            {
                res.AddError(Entities_1.Messages.ErrorMessageCode.ActivateIdDoesNotExist, "Aktifleştirilecek kullanıcı bulunamadı");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> GetUserById(int id)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            res.Result = repo_user.Find(x => x.Id == id);
            if (res.Result == null)
            {
                res.AddError(Entities_1.Messages.ErrorMessageCode.UserNotFound, "Kullanıcı Bulunamadı");
                return res;
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> UpdateProfile(EvernoteUser data)
        {
            EvernoteUser db_user = repo_user.Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            if (db_user != null && db_user.Id !=data.Id)
            {
                if (db_user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı");
                }
                if (db_user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAlreadyExists, "E-posta adresi kayıtlı.");
                }
                return res;
            }
            res.Result = repo_user.Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;
            if (string.IsNullOrEmpty(data.ProfileImageFilename) == false)
            {
                res.Result.ProfileImageFilename = data.ProfileImageFilename;

            }
            if (repo_user.Update(res.Result) == 0)//update başarılı olsaydı 1 dönmesi lazımdı.
            {
                res.AddError(ErrorMessageCode.ProfileCouldNotUpdate, "Profil güncellenemedi");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> RemoveUserById(int ıd)
        {
            EvernoteUser user = repo_user.Find(x => x.Id == ıd);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            if (repo_user.Delete(user) == 0)
            {
                res.AddError(ErrorMessageCode.UserCouldNotRemove, "Kullanıcı silinemedi");
                return res;
            }
            else
            {
                res.AddError(ErrorMessageCode.UserCouldNotFind, "Kullanıcı bulunamadı");
            }
            return res;
        }
    }
}
