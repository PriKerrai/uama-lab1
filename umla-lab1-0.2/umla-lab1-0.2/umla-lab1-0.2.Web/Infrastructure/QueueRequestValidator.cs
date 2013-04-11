namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Web;
    using umla_lab1_0._2.Web.UserAccountWrappers;

    public class QueueRequestValidator : StorageRequestValidator
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public QueueRequestValidator()
            : this(new FormsAuthenticationService(), new AccountMembershipService(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public QueueRequestValidator(IFormsAuthentication formsAuth, IMembershipService membershipService, IUserPrivilegesRepository userPrivilegesRepository)
            : base(formsAuth, membershipService)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        protected override bool OnValidateRequest(string userId, HttpContext context)
        {
            if (!this.CanUseQueues(userId))
            {
                context.Response.EndWithDataServiceError(401, "Unauthorized", "You have no permission to use queues.");
            }

            var queueName = StorageRequestAnalyzer.GetRequestedQueue(context.Request);
            if (!this.CanUseQueue(userId, queueName, context.Request))
            {
                context.Response.EndWithDataServiceError(401, "Unauthorized", "You have no permission to use this queue.");
            }

            return true;
        }

        private bool CanUseQueue(string userId, string queueName, HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                return true;
            }

            var publicQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.PublicQueuePrivilegeSuffix);
            if (!this.userPrivilegesRepository.PublicPrivilegeExists(publicQueuePrivilege))
            {
                var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.QueuePrivilegeSuffix);
                if (!this.userPrivilegesRepository.HasUserPrivilege(userId, accessQueuePrivilege))
                {
                    // Check if the user is creating a new queue.
                    return StorageRequestAnalyzer.IsCreatingQueue(request);
                }
            }

            return true;
        }

        private bool CanUseQueues(string userId)
        {
            return this.userPrivilegesRepository.HasUserPrivilege(userId, PrivilegeConstants.QueuesUsagePrivilege);
        }
    }
}