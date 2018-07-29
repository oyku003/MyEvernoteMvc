using MyEvernote.BusinessLayer_1.Abstract;
using MyEvernote.BusinessLayer_1.Results;
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

    public class EvernoteUserManager :ManagerBase<EvernoteUser>
    {
        public BusinessLayerResult<EvernoteUser> RegisterUser(RegisterViewModel data)
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            EvernoteUser user = Find(x => x.Username == data.UserName || x.Email == data.EMail);
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
            {//base'deki insert'ü kullan dedik.Aşağıdaki insert ile çakışmaması için
                int dbResult =base.Insert(new EvernoteUser()
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
                    res.Result = Find(x => x.Email == data.EMail && x.Username == data.UserName);

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

            res.Result = Find(x => x.Username == data.UserName && x.Password == data.Password);

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
            res.Result = Find(x => x.ActivateGuid == activateId);

            if (res.Result != null)
            {
                if (res.Result.IsActive)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UserAlreadyActive, "Kullanıcı zaten aktif kullanıcıdır");
                    return res;
                }
                res.Result.IsActive = true;//ilgili kullanıcının ısActivate özelliğini true'ya set ettik.
               Update(res.Result);
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
            res.Result = Find(x => x.Id == id);
            if (res.Result == null)
            {
                res.AddError(Entities_1.Messages.ErrorMessageCode.UserNotFound, "Kullanıcı Bulunamadı");
                return res;
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> UpdateProfile(EvernoteUser data)
        {
            EvernoteUser db_user = Find(x => x.Username == data.Username || x.Email == data.Email);
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
            res.Result = Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;
            if (string.IsNullOrEmpty(data.ProfileImageFilename) == false)
            {
                res.Result.ProfileImageFilename = data.ProfileImageFilename;

            }
            if (base.Update(res.Result) == 0)//update başarılı olsaydı 1 dönmesi lazımdı.
            {
                res.AddError(ErrorMessageCode.ProfileCouldNotUpdate, "Profil güncellenemedi");
            }
            return res;
        }

        public BusinessLayerResult<EvernoteUser> RemoveUserById(int ıd)
        {
            EvernoteUser user = Find(x => x.Id == ıd);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            if (Delete(user) == 0)
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

        public new BusinessLayerResult<EvernoteUser> Insert(EvernoteUser data) //base'deki metoda değil buraya gelmesini sağladık.override etseydik tipi değiştiremiyoduk.
        {
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();
            EvernoteUser user = Find(x => x.Username == data.Username || x.Email == data.Email);

            res.Result = data;
            if (user != null)
            {
                if (user.Username == data.Username)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UsernameAlreadyExists, "Kullanıcı adı kayıtlı");
                }
                if (user.Email == data.Email)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.EmailAlreadyExists, "E-Mail zaten kayıtlı");
                }
            }
            else
            {//base'deki insert'ü kullan dedik.Aşağıdaki insert ile çakışmaması için

                data.ProfileImageFilename = "";
                data.ActivateGuid = Guid.NewGuid();
                int dbResult = base.Insert(res.Result);
                if (base.Insert(res.Result) == 0)
                {
                    res.AddError(Entities_1.Messages.ErrorMessageCode.UserCouldNotInserted, "Kullanıcı eklenemedi");
                }
            }
            return res;
        }
        public new BusinessLayerResult<EvernoteUser> Update(EvernoteUser data)
        {
            EvernoteUser db_user = Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinessLayerResult<EvernoteUser> res = new BusinessLayerResult<EvernoteUser>();

            res.Result = data;
            if (db_user != null && db_user.Id != data.Id)
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
            res.Result = Find(x => x.Id == data.Id);//gelen dataya göre değil db'den cektik bütün verileri
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;
            res.Result.IsActive = data.IsActive;
            res.Result.IsAdmin = data.IsAdmin;
            if (base.Update(res.Result) == 0)//update başarılı olsaydı 1 dönmesi lazımdı.
            {
                res.AddError(ErrorMessageCode.UserCouldNotUpdated, "Kullanıcı Güncellenemedi");
            }
            return res;
        }
    }
}
