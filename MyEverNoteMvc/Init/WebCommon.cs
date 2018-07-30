using MyEvernote.Common;
using MyEvernote.Entities;
using MyEverNoteMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyEverNoteMvc.Init
{
    public class WebCommon : ICommon
    {
        public string GetCurrentUsernam()
        {
            EvernoteUser user = CurrentSession.User;
            if (user != null)
            {
                return user.Username;
            }

            return "system";
        }
    }
}