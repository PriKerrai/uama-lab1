namespace umla_lab1_0._2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using Microsoft.WindowsAzure.StorageClient;
    using umla_lab1_0._2.Web.Infrastructure;
    using umla_lab1_0._2.Web.Models;
    using umla_lab1_0._2.Web.UserAccountWrappers;

    public class QueuesController : StorageItemController
    {
        private readonly CloudQueueClient cloudQueueClient;

        private readonly IMembershipService membershipService;

        public QueuesController()
            : this(null, new UserTablesServiceContext(), new AccountMembershipService())
        {
        }

        [CLSCompliant(false)]
        public QueuesController(CloudQueueClient cloudQueueClient, IUserPrivilegesRepository userPrivilegesRepository, IMembershipService membershipService)
            : base(userPrivilegesRepository)
        {
            if ((GetStorageAccountFromConfigurationSetting() == null) && (cloudQueueClient == null))
            {
                throw new ArgumentNullException("cloudQueueClient", "Cloud Queue Client cannot be null if no configuration is loaded.");
            }

            this.cloudQueueClient = cloudQueueClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudQueueClient();
            this.membershipService = membershipService;
        }

        public ActionResult Index()
        {
            var permissions = new List<StorageItemPermissionsModel>();
            var queues = this.cloudQueueClient.ListQueues().Select(q => q.Name);
            foreach (var queueName in queues)
            {
                var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.QueuePrivilegeSuffix);
                var publicQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.PublicQueuePrivilegeSuffix);
                permissions.Add(new StorageItemPermissionsModel
                {
                    StorageItemName = queueName,
                    IsPublic = this.UserPrivilegesRepository.PublicPrivilegeExists(publicQueuePrivilege),
                    AllowedUserIds = this.UserPrivilegesRepository.GetUsersWithPrivilege(accessQueuePrivilege).Select(us => us.UserId)
                });
            }

            this.ViewBag.users = this.membershipService.GetAllUsers()
                .Cast<MembershipUser>()
                .Select(user => new UserModel { UserName = user.UserName, UserId = user.ProviderUserKey.ToString() });

            return this.View(permissions);
        }

        [HttpPost]
        public void AddQueuePermission(string queue, string userId)
        {
            var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queue, PrivilegeConstants.QueuePrivilegeSuffix);
            this.AddPrivilegeToUser(userId, accessQueuePrivilege);
        }

        [HttpPost]
        public void RemoveQueuePermission(string queue, string userId)
        {
            var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queue, PrivilegeConstants.QueuePrivilegeSuffix);
            this.RemovePrivilegeFromUser(userId, accessQueuePrivilege);
        }

        [HttpPost]
        public void SetQueuePublic(string queue, bool isPublic)
        {
            var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queue, PrivilegeConstants.PublicQueuePrivilegeSuffix);
            this.SetPublicPrivilege(accessQueuePrivilege, isPublic);
        }
    }
}
