﻿namespace umla_lab1_0._2.Web.Models
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Samples.WindowsPhoneCloud.StorageClient")]
    public class CloudBlobCollection
    {
        [DataMember]
        public CloudBlob[] Blobs { get; set; }
    }
}