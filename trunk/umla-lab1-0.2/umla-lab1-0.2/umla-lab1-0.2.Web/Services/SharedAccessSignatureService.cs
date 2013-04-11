namespace umla_lab1_0._2.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using umla_lab1_0._2.Web.Infrastructure;

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SharedAccessSignatureService : ISharedAccessSignatureService
    {
        private const SharedAccessPermissions ContainerSharedAccessPermissions = SharedAccessPermissions.Write | SharedAccessPermissions.Delete | SharedAccessPermissions.List | SharedAccessPermissions.Read;

        private readonly CloudBlobClient cloudBlobClient;
        private readonly WebOperationContext webOperationContext;
        private readonly BlobContainerRequestValidator requestValidator;
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        private readonly HttpContextBase context;

        public SharedAccessSignatureService()
            : this(null, new HttpContextWrapper(HttpContext.Current), WebOperationContext.Current, new BlobContainerRequestValidator(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public SharedAccessSignatureService(CloudBlobClient cloudBlobClient, HttpContextBase context, WebOperationContext webOperationContext, BlobContainerRequestValidator requestValidator, IUserPrivilegesRepository userPrivilegesRepository)
        {
            if ((context == null) && (HttpContext.Current == null))
            {
                throw new ArgumentNullException("context", "The context cannot be null if not running on a Web context.");
            }

            if ((cloudBlobClient == null) && (GetStorageAccountFromConfigurationSetting() == null))
            {
                throw new ArgumentNullException("cloudBlobClient", "The Cloud Blob Client cannot be null if no configuration is loaded.");
            }

            this.cloudBlobClient = cloudBlobClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudBlobClient();
            this.context = context;
            this.webOperationContext = webOperationContext;
            this.requestValidator = requestValidator;
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        protected string UserId
        {
            get { return this.requestValidator.GetUserId(this.context); }
        }

        public string CreateContainer(string containerName, bool createIfNotExists, bool isPublic)
        {
            // Authenticate.
            var userId = this.UserId;

            this.requestValidator.OnValidateRequest(userId, containerName, true);

            try
            {
                var container = this.cloudBlobClient.GetContainerReference(containerName.ToLowerInvariant());

                if (createIfNotExists)
                {
                    container.CreateIfNotExist();
                }
                else
                {
                    container.Create();
                }

                var publicBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", containerName, PrivilegeConstants.PublicBlobContainerPrivilegeSuffix);
                if (isPublic)
                {
                    BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
                    containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(containerPermissions);
                }

                var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", containerName, PrivilegeConstants.BlobContainerPrivilegeSuffix);
                this.userPrivilegesRepository.AddPrivilegeToUser(userId, accessBlobContainerPrivilege);

                return "Success";
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        public string DeleteContainer(string containerName)
        {
            // Authenticate.
            var userId = this.UserId;

            this.requestValidator.OnValidateRequest(userId, containerName);

            try
            {
                var blobRequestOptions = new BlobRequestOptions();
                var container = this.cloudBlobClient.GetContainerReference(containerName);
                container.Delete();

                // A Blob Container was deleted -> remove all permissions to that blob container.
                this.RemoveAllBlobContainerPermissions(containerName);
                return "Success";
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        public Models.CloudBlobContainerCollection ListContainers(string containerPrefix)
        {
            // Authenticate.
            var userId = this.UserId;

            this.requestValidator.OnValidateRequest(userId);

            IEnumerable<CloudBlobContainer> containers = default(IEnumerable<CloudBlobContainer>);

            if (!string.IsNullOrEmpty(containerPrefix))
            {
                containerPrefix = containerPrefix.TrimStart('/', '\\').Replace('\\', '/');
                containers = this.cloudBlobClient.ListContainers(containerPrefix);
            }
            else
            {
                containers = this.cloudBlobClient.ListContainers();
            }

            try
            {
                var result = new Models.CloudBlobContainerCollection
                {
                    Containers = containers.Where(c => c is CloudBlobContainer)
                                    .Select(c => c.ToModel())
                                    .ToArray()
                };
                if (this.webOperationContext != null)
                {
                    this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        public Uri GetContainerSharedAccessSignature(string containerName)
        {
            // Authenticate.
            var userId = this.UserId;

            this.requestValidator.OnValidateRequest(userId, containerName);

            try
            {
                // Each user has its own container.
                var container = this.cloudBlobClient.GetContainerReference(containerName);
                var containerSASExperiationTime = int.Parse(ConfigReader.GetConfigValue("ContainerSASExperiationTime"), NumberStyles.Integer, CultureInfo.InvariantCulture);
                var sas = container.GetSharedAccessSignature(new SharedAccessPolicy()
                {
                    Permissions = ContainerSharedAccessPermissions,
                    SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(containerSASExperiationTime)
                });

                if (this.webOperationContext != null)
                {
                    this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                }

                var uriBuilder = new UriBuilder(container.Uri) { Query = sas.TrimStart('?') };
                return uriBuilder.Uri;
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        public Models.CloudBlobCollection GetBlobsSharedAccessSignatures(string containerName, string blobPrefix, bool useFlatBlobListing)
        {
            // Authenticate.
            var userId = this.UserId;

            this.requestValidator.OnValidateRequest(userId, containerName);

            if (!string.IsNullOrEmpty(blobPrefix))
            {
                blobPrefix = blobPrefix.TrimStart('/', '\\').Replace('\\', '/');
            }

            try
            {
                var container = this.cloudBlobClient.GetContainerReference(containerName);
                container.CreateIfNotExist();

                SetReadOnlySharedAccessPolicy(container);
                var prefix = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", containerName, blobPrefix);

                var blobs = this.cloudBlobClient.ListBlobsWithPrefix(prefix, new BlobRequestOptions { UseFlatBlobListing = useFlatBlobListing });
                var result = new Models.CloudBlobCollection
                {
                    Blobs = blobs.Where(b => b is CloudBlob)
                                .Select(b => b.ToModel(containerName, this.cloudBlobClient.Credentials.AccountName))
                                .ToArray()
                };

                if (this.webOperationContext != null)
                {
                    this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        private static void SetReadOnlySharedAccessPolicy(CloudBlobContainer container)
        {
            var blobSASExperiationTime = int.Parse(ConfigReader.GetConfigValue("BlobSASExperiationTime"), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var permissions = container.GetPermissions();
            var options = new BlobRequestOptions
            {
                // Fail if someone else has already changed the container before we do.
                AccessCondition = AccessCondition.IfMatch(container.Properties.ETag)
            };
            var sharedAccessPolicy = new SharedAccessPolicy
            {
                Permissions = SharedAccessPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromDays(blobSASExperiationTime)
            };

            permissions.SharedAccessPolicies.Remove("readonly");
            permissions.SharedAccessPolicies.Add("readonly", sharedAccessPolicy);

            container.SetPermissions(permissions, options);
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

        private void RemoveAllBlobContainerPermissions(string containerName)
        {
            if (!string.IsNullOrEmpty(containerName))
            {
                var publicBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", containerName, PrivilegeConstants.PublicBlobContainerPrivilegeSuffix);
                this.userPrivilegesRepository.DeletePublicPrivilege(publicBlobContainerPrivilege);

                var accessBlobContainerPrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", containerName, PrivilegeConstants.BlobContainerPrivilegeSuffix);
                this.userPrivilegesRepository.DeletePrivilege(accessBlobContainerPrivilege);
            }
        }
    }
}
