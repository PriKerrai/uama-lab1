namespace umla_lab1_0._2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web.Mvc;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using WindowsPhone.Recipes.Push.Messages;
    using umla_lab1_0._2.Web.Infrastructure;
    using umla_lab1_0._2.Web.Models;
    using umla_lab1_0._2.Web.UserAccountWrappers;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public class PushNotificationsController : Controller
    {
        private readonly CloudQueueClient cloudQueueClient;
        private readonly IPushUserEndpointsRepository pushUserEndpointsRepository;

        private readonly IMembershipService membershipService;

        public PushNotificationsController()
            : this(null, new UserTablesServiceContext(), new AccountMembershipService())
        {
        }

        [CLSCompliant(false)]
        public PushNotificationsController(CloudQueueClient cloudQueueClient, IPushUserEndpointsRepository pushUserEndpointsRepository, IMembershipService membershipService)
        {
            if (GetStorageAccountFromConfigurationSetting() == null)
            {
                if (cloudQueueClient == null)
                {
                    throw new ArgumentNullException("cloudQueueClient", "Cloud Queue Client cannot be null if no configuration is loaded.");
                }
            }

            this.cloudQueueClient = cloudQueueClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudQueueClient();
            this.membershipService = membershipService;
            this.pushUserEndpointsRepository = pushUserEndpointsRepository;
        }

        public ActionResult Microsoft()
        {
            var users = this.pushUserEndpointsRepository
                .GetAllPushUsers()
                .Select(userId => new UserModel { UserId = userId, UserName = this.membershipService.GetUserByProviderUserKey(new Guid(userId)).UserName });

            return this.View(users);
        }


        [HttpPost]
        public ActionResult SendMicrosoftToast(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var uris = this.pushUserEndpointsRepository.GetPushUsersByName(userId).Select(u => u.ChannelUri);
            var pushUserEndpoint = this.pushUserEndpointsRepository.GetPushUsersByName(userId).FirstOrDefault();
            var toast = new ToastPushNotificationMessage
            {
                SendPriority = MessageSendPriority.High,
                Title = message
            };

            foreach (var uri in uris)
            {
                var messageResult = toast.SendAndHandleErrors(new Uri(uri));
                resultList.Add(messageResult);
                if (messageResult.Status.Equals(MessageSendResultLight.Success))
                {
                    this.QueueMessage(pushUserEndpoint, message);
                }
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendMicrosoftTile(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var pushUserEndpointList = this.pushUserEndpointsRepository.GetPushUsersByName(userId);
            foreach (var pushUserEndpoint in pushUserEndpointList)
            {
                var tile = new TilePushNotificationMessage
                {
                    SendPriority = MessageSendPriority.High,
                    Count = ++pushUserEndpoint.TileCount
                };

                var messageResult = tile.SendAndHandleErrors(new Uri(pushUserEndpoint.ChannelUri));
                resultList.Add(messageResult);
                if (messageResult.Status.Equals(MessageSendResultLight.Success))
                {
                    this.QueueMessage(pushUserEndpoint, message);

                    this.pushUserEndpointsRepository.UpdatePushUserEndpoint(pushUserEndpoint);
                }
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SendMicrosoftRaw(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var uris = this.pushUserEndpointsRepository.GetPushUsersByName(userId).Select(u => u.ChannelUri);
            var raw = new RawPushNotificationMessage
            {
                SendPriority = MessageSendPriority.High,
                RawData = Encoding.UTF8.GetBytes(message)
            };

            foreach (var uri in uris)
            {
                resultList.Add(raw.SendAndHandleErrors(new Uri(uri)));
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
        }

        private static CloudStorageAccount GetStorageAccountFromConfigurationSetting()
        {
            CloudStorageAccount account = null;
            try
            {
                account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            }
            catch (InvalidOperationException)
            {
                account = null;
            }

            return account;
        }

        private void QueueMessage(PushUserEndpoint pushUser, string message)
        {
            var queueName = GetQueueName(pushUser.PartitionKey, pushUser.RowKey, pushUser.UserId);
            var queue = this.cloudQueueClient.GetQueueReference(queueName);

            queue.CreateIfNotExist();
            queue.AddMessage(new CloudQueueMessage(message));
        }

        private static string GetQueueName(string applicationId, string deviceId, string userId)
        {
            var uniqueName = string.Concat(applicationId, deviceId, userId);

            return string.Concat("notification", uniqueName.GetHashCode());
        }
    }
}
