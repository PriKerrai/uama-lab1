namespace umla_lab1_0._2.Web.Infrastructure
{
    public static class PrivilegeConstants
    {
        public const string AdminPrivilege = "admin";

        public const string TablesUsagePrivilege = "TablesUsage";

        public const string BlobContainersUsagePrivilege = "BlobContainersUsage";

        public const string QueuesUsagePrivilege = "QueuesUsage";

        public const string QueuePrivilegeSuffix = "_queue_access";

        public const string PublicQueuePrivilegeSuffix = "_queue_publicaccess";

        public const string TablePrivilegeSuffix = "_table_access";

        public const string PublicTablePrivilegeSuffix = "_table_publicaccess";

        public const string BlobContainerPrivilegeSuffix = "_blobcontainer_access";

        public const string PublicBlobContainerPrivilegeSuffix = "_blobcontainer_publicaccess";
    }
}