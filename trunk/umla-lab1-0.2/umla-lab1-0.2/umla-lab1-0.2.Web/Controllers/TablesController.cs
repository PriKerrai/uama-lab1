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

    public class TablesController : StorageItemController
    {
        private readonly CloudTableClient cloudTableClient;

        private readonly IMembershipService membershipService;

        public TablesController()
            : this(null, new UserTablesServiceContext(), new AccountMembershipService())
        {
        }

        [CLSCompliant(false)]
        public TablesController(CloudTableClient cloudTableClient, IUserPrivilegesRepository userPrivilegesRepository, IMembershipService membershipService)
            : base(userPrivilegesRepository)
        {
            if ((GetStorageAccountFromConfigurationSetting() == null) && (cloudTableClient == null))
            {
                throw new ArgumentNullException("cloudTableClient", "Cloud Table Client cannot be null if no configuration is loaded.");
            }

            this.cloudTableClient = cloudTableClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudTableClient();
            this.membershipService = membershipService;
        }

        public ActionResult Index()
        {
            var permissions = new List<StorageItemPermissionsModel>();
            var tables = this.cloudTableClient.ListTables();
            foreach (var tableName in tables)
            {
                if (!tableName.Equals(Microsoft.Samples.ServiceHosting.AspProviders.AspProvidersConfiguration.DefaultMembershipTableName, StringComparison.OrdinalIgnoreCase) &&
                    !tableName.Equals(UserTablesServiceContext.UserPrivilegeTableName, StringComparison.OrdinalIgnoreCase) &&
                    !tableName.Equals(UserTablesServiceContext.PushUserTableName, StringComparison.OrdinalIgnoreCase))
                {
                    var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.TablePrivilegeSuffix);
                    var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.PublicTablePrivilegeSuffix);
                    permissions.Add(new StorageItemPermissionsModel
                    {
                        StorageItemName = tableName,
                        IsPublic = this.UserPrivilegesRepository.PublicPrivilegeExists(publicTablePrivilege),
                        AllowedUserIds = this.UserPrivilegesRepository.GetUsersWithPrivilege(accessTablePrivilege).Select(us => us.UserId)
                    });
                }
            }

            this.ViewBag.users = this.membershipService.GetAllUsers()
                .Cast<MembershipUser>()
                .Select(user => new UserModel { UserName = user.UserName, UserId = user.ProviderUserKey.ToString() });

            return this.View(permissions);
        }

        [HttpPost]
        public void AddTablePermission(string table, string userId)
        {
            var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.TablePrivilegeSuffix);
            this.AddPrivilegeToUser(userId, accessTablePrivilege);
        }

        [HttpPost]
        public void RemoveTablePermission(string table, string userId)
        {
            var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.TablePrivilegeSuffix);
            this.RemovePrivilegeFromUser(userId, accessTablePrivilege);
        }

        [HttpPost]
        public void SetTablePublic(string table, bool isPublic)
        {
            var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.PublicTablePrivilegeSuffix);
            this.SetPublicPrivilege(publicTablePrivilege, isPublic);
        }
    }
}
