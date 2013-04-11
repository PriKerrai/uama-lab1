namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Web;
    using System.Web.Security;
    using umla_lab1_0._2.Web.UserAccountWrappers;

    public abstract class StorageRequestValidator : IStorageRequestValidator
    {
        private readonly IFormsAuthentication formsAuth;
        private readonly IMembershipService membershipService;

        protected StorageRequestValidator(IFormsAuthentication formsAuth, IMembershipService membershipService)
        {
            this.formsAuth = formsAuth;
            this.membershipService = membershipService;
        }

        public bool ValidateRequest(HttpContext context)
        {
            var userName = this.GetUserId(context);
            if (string.IsNullOrWhiteSpace(userName))
            {
                return false;
            }

            return this.OnValidateRequest(userName, context);
        }

        public string GetUserId(HttpContext context)
        {
            string ticketValue = null;

            var cookie = context.Request.Cookies[this.formsAuth.FormsCookieName];
            if (cookie != null)
            {
                // From cookie.
                ticketValue = cookie.Value;
            }
            else if (context.Request.Headers["AuthToken"] != null)
            {
                // From http header.
                ticketValue = context.Request.Headers["AuthToken"];
            }

            if (!string.IsNullOrWhiteSpace(ticketValue))
            {
                FormsAuthenticationTicket ticket = null;
                try
                {
                    ticket = this.formsAuth.Decrypt(ticketValue);
                }
                catch (ArgumentException)
                {
                    context.Response.EndWithDataServiceError(401, "Unauthorized", "The Authorization Token is no longer valid.");
                }

                if (ticket != null)
                {
                    return this.membershipService.GetUser(new FormsIdentity(ticket).Name).ProviderUserKey.ToString();
                }
            }

            context.Response.EndWithDataServiceError(404, "Not Found", "Resource Not Found.");

            return string.Empty;
        }

        protected abstract bool OnValidateRequest(string userId, HttpContext context);
    }
}