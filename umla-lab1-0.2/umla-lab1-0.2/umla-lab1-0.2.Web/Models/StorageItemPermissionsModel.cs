namespace umla_lab1_0._2.Web.Models
{
    using System.Collections.Generic;

    public class StorageItemPermissionsModel
    {
        public string StorageItemName { get; set; }

        public IEnumerable<string> AllowedUserIds { get; set; }

        public bool IsPublic { get; set; }
    }
}