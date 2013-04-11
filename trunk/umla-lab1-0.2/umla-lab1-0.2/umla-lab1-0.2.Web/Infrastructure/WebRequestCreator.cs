namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Net;

    internal class WebRequestCreator : IWebRequestCreate
    {
        public WebRequest Create(Uri newRequestUri)
        {
            return WebRequest.Create(newRequestUri);
        }
    }
}