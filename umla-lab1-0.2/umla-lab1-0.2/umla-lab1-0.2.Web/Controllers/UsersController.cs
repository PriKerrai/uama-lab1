namespace umla_lab1_0._2.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using umla_lab1_0._2.Web.Infrastructure;
    using umla_lab1_0._2.Web.Models;
    using umla_lab1_0._2.Web.UserAccountWrappers;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public class UsersController : Controller
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        private readonly IMembershipService membershipService;

        public UsersController()
            : this(new UserTablesServiceContext(), new AccountMembershipService())
        {
        }

        [CLSCompliant(false)]
        public UsersController(IUserPrivilegesRepository userPrivilegesRepository, IMembershipService membershipService)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
            this.membershipService = membershipService;
        }

        public ActionResult Index()
        {
            var users = this.membershipService.GetAllUsers()
                .Cast<MembershipUser>()
                .Select(user => new UserPermissionsModel
                    {
                        UserName = user.UserName,
                        UserId = user.ProviderUserKey.ToString(),
                        TablesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.ProviderUserKey.ToString(), PrivilegeConstants.TablesUsagePrivilege),
                        BlobsUsage = this.userPrivilegesRepository.HasUserPrivilege(user.ProviderUserKey.ToString(), PrivilegeConstants.BlobContainersUsagePrivilege),
                        QueuesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.ProviderUserKey.ToString(), PrivilegeConstants.QueuesUsagePrivilege)
                    });

            return this.View(users);
        }

        [HttpPost]
        public void SetUserPermissions(string userId, bool useTables, bool useBlobs, bool useQueues, bool useSql)
        {
            this.SetStorageItemUsagePrivilege(useTables, userId, PrivilegeConstants.TablesUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useBlobs, userId, PrivilegeConstants.BlobContainersUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useQueues, userId, PrivilegeConstants.QueuesUsagePrivilege);
        }

        private void SetStorageItemUsagePrivilege(bool allowAccess, string user, string privilege)
        {
            if (allowAccess)
            {
                this.userPrivilegesRepository.AddPrivilegeToUser(user, privilege);
            }
            else
            {
                this.userPrivilegesRepository.RemovePrivilegeFromUser(user, privilege);
            }
        }
    }
}
