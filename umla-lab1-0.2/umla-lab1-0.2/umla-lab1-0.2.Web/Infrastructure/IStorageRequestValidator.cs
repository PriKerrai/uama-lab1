namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Web;

    public interface IStorageRequestValidator
    {
        bool ValidateRequest(HttpContext context);

        string GetUserId(HttpContext context);
    }
}
