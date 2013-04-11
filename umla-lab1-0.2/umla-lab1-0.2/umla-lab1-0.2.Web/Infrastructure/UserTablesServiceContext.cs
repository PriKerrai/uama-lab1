namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Web;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using umla_lab1_0._2.Web.Models;

    public class UserTablesServiceContext : TableServiceContext, IPushUserEndpointsRepository, IUserPrivilegesRepository
    {
        public const string PushUserTableName = "PushUserEndpoints";

        public const string UserPrivilegeTableName = "UserPrivileges";

        private const string PublicUserId = "00000000-0000-0000-0000-000000000000";

        public UserTablesServiceContext()
            : this(CloudStorageAccount.FromConfigurationSetting("DataConnectionString"))
        {
        }

        public UserTablesServiceContext(CloudStorageAccount account)
            : this(account.TableEndpoint.ToString(), account.Credentials)
        {
        }

        public UserTablesServiceContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
            this.IgnoreResourceNotFoundException = true;
            this.IgnoreMissingProperties = true;
        }

        public IQueryable<PushUserEndpoint> PushUserEndpoints
        {
            get
            {
                return this.CreateQuery<PushUserEndpoint>(PushUserTableName);
            }
        }

        public IQueryable<UserPrivilege> UserPrivileges
        {
            get
            {
                return this.CreateQuery<UserPrivilege>(UserPrivilegeTableName);
            }
        }

        [CLSCompliant(false)]
        public void AddPushUserEndpoint(PushUserEndpoint pushUserEndPoint)
        {
            this.AddObject(PushUserTableName, pushUserEndPoint);
            this.SaveChanges();
        }

        [CLSCompliant(false)]
        public void RemovePushUserEndpoint(PushUserEndpoint pushUserEndpointData)
        {
            var pushUserEndpoint = this.GetPushUserByApplicationAndDevice(pushUserEndpointData.PartitionKey, pushUserEndpointData.RowKey);

            this.DeleteObject(pushUserEndpoint);

            this.SaveChanges();
        }

        public IEnumerable<string> GetAllPushUsers()
        {
            return this.PushUserEndpoints
                .ToList()
                .GroupBy(u => u.UserId)
                .Select(g => g.Key);
        }

        public IEnumerable<PushUserEndpoint> GetPushUsersByName(string userId)
        {
            return this.PushUserEndpoints
                .Where(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
        }

        [CLSCompliant(false)]
        public PushUserEndpoint GetPushUserByApplicationAndDevice(string applicationId, string deviceId)
        {
            return this.PushUserEndpoints
                .Where(u => u.PartitionKey.Equals(applicationId, StringComparison.OrdinalIgnoreCase)
                            && u.RowKey.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault<PushUserEndpoint>();
        }

        [CLSCompliant(false)]
        public void UpdatePushUserEndpoint(PushUserEndpoint pushUserEndpoint)
        {
            this.UpdateObject(pushUserEndpoint);
            this.SaveChanges();
        }

        public IEnumerable<UserPrivilege> GetUsersWithPrivilege(string privilege)
        {
            return this.UserPrivileges
                .Where(p => p.Privilege.Equals(privilege, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
        }

        public void AddPrivilegeToUser(string userId, string privilege)
        {
            if (!this.HasUserPrivilege(userId, privilege))
            {
                this.AddObject(UserPrivilegeTableName, new UserPrivilege { UserId = userId, Privilege = privilege });

                this.SaveChanges();
            }
        }

        public void AddPublicPrivilege(string privilege)
        {
            this.AddPrivilegeToUser(PublicUserId, privilege);
        }

        public void RemovePrivilegeFromUser(string userId, string privilege)
        {
            var userPrivilege = this.GetUserPrivilege(userId, privilege);
            if (userPrivilege != null)
            {
                this.DeleteObject(userPrivilege);

                this.SaveChanges();
            }
        }

        public void DeletePublicPrivilege(string privilege)
        {
            this.RemovePrivilegeFromUser(PublicUserId, privilege);
        }

        public bool HasUserPrivilege(string userId, string privilege)
        {
            return this.GetUserPrivilege(userId, privilege) != null;
        }

        public bool PublicPrivilegeExists(string privilege)
        {
            return this.HasUserPrivilege(PublicUserId, privilege);
        }

        public void DeletePrivilege(string privilege)
        {
            var userPrivileges = this.GetUsersWithPrivilege(privilege);
            foreach (var userPrivilege in userPrivileges)
            {
                this.DeleteObject(userPrivilege);
            }

            this.SaveChanges();
        }

        private UserPrivilege GetUserPrivilege(string userId, string privilege)
        {
            return this.UserPrivileges
                .Where(p => p.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase) && p.Privilege.Equals(privilege, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .FirstOrDefault();
        }
    }
}