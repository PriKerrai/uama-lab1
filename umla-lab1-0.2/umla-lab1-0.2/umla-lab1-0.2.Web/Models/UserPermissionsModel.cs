namespace umla_lab1_0._2.Web.Models
{
    public class UserPermissionsModel
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public bool TablesUsage { get; set; }

        public bool BlobsUsage { get; set; }

        public bool QueuesUsage { get; set; }
    }
}