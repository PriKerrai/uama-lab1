﻿namespace umla_lab1_0._2.Web.UserAccountWrappers
{
    using System.Web.Security;

    public interface IFormsAuthentication
    {
        string FormsCookieName { get; }

        void SignIn(string userName, bool createPersistentCookie);

        void SignOut();

        string Encrypt(FormsAuthenticationTicket ticket);

        FormsAuthenticationTicket Decrypt(string token);
    }
}