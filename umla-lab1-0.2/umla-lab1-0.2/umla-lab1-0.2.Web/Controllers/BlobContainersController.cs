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

    public class BlobContainersController : StorageItemController
    {
        private readonly CloudBlobClient cloudBlobClient;


        private readonly IMembershipService membershipService;

        public BlobContainersController()
            : this(null, new UserTablesServiceContext(), new AccountMembershipService())
        {
        }

        [CLSCompliant(false)]
        public BlobContainersController(CloudBlobClient cloudBlobClient, IUserPrivilegesRepository userPrivilegesRepository, IMembershipService membershipService)
            : base(userPrivilegesRepository)
        {
            if ((GetStorageAccountFromConfigurationSetting() == null) && (cloudBlobClient == null))
            {
                throw new ArgumentNullException("cloudBlobClient", "Cloud Blob Client cannot be null if no configuration is loaded.");
            }

            this.cloudBlobClient = cloudBlobClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudBlobClient();
            this.membershipService = membershipService;
        }

        public ActionResult Index()
        {
            var permissions = new List<StorageItemPermissionsModel>();
            var blobContainers = this.cloudBlobClient.ListContainers().Select(q => q.Name);
            foreach (var blobContainerName in blobContainers)
            {
                var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainerName, PrivilegeConstants.BlobContainerPrivilegeSuffix);
                var publicBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainerName, PrivilegeConstants.PublicBlobContainerPrivilegeSuffix);
                permissions.Add(new StorageItemPermissionsModel
                {
                    StorageItemName = blobContainerName,
                    IsPublic = this.UserPrivilegesRepository.PublicPrivilegeExists(publicBlobContainerPrivilege),
                    AllowedUserIds = this.UserPrivilegesRepository.GetUsersWithPrivilege(accessBlobContainerPrivilege).Select(us => us.UserId)
                });
            }

            this.ViewBag.users = this.membershipService.GetAllUsers()
                .Cast<MembershipUser>()
                .Select(user => new UserModel { UserName = user.UserName, UserId = user.ProviderUserKey.ToString() });

            return this.View(permissions);
        }


        [HttpPost]
        public void AddBlobContainerPermission(string blobContainer, string userId)
        {
            var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainer, PrivilegeConstants.BlobContainerPrivilegeSuffix);
            this.AddPrivilegeToUser(userId, accessBlobContainerPrivilege);
        }

        [HttpPost]
        public void RemoveBlobContainerPermission(string blobContainer, string userId)
        {
            var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainer, PrivilegeConstants.BlobContainerPrivilegeSuffix);
            this.RemovePrivilegeFromUser(userId, accessBlobContainerPrivilege);
        }

        [HttpPost]
        public void SetBlobContainerPublic(string blobContainer, bool isPublic)
        {
            var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobContainer, PrivilegeConstants.PublicBlobContainerPrivilegeSuffix);
            this.SetPublicPrivilege(accessBlobContainerPrivilege, isPublic);
        }
    }
}
