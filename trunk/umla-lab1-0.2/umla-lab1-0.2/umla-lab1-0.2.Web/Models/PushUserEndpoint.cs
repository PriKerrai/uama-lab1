namespace umla_lab1_0._2.Web.Models
{
    using System;
    using System.Globalization;
    using Microsoft.WindowsAzure.StorageClient;

    [CLSCompliant(false)]
    public class PushUserEndpoint : TableServiceEntity
    {
        // Empty contructor for serialization purposes
        public PushUserEndpoint()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The PartitionKey and RowKey properties are set to uniquely identify the PushUserEndpoint entity.")]
        public PushUserEndpoint(string applicationId, string deviceId)
        {
            this.PartitionKey = applicationId;
            this.RowKey = deviceId;
        }

        public string UserId { get; set; }

        public string ChannelUri { get; set; }

        public int TileCount { get; set; }
    }
}