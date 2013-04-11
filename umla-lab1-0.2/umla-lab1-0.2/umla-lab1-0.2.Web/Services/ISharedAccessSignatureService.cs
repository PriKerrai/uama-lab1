namespace umla_lab1_0._2.Web.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using umla_lab1_0._2.Web.Models;

    [ServiceContract]
    public interface ISharedAccessSignatureService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/container/{containerName}")]
        Uri GetContainerSharedAccessSignature(string containerName);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/blob?containerName={containerName}&blobPrefix={blobPrefix}&useFlatBlobListing={useFlatBlobListing}")]
        CloudBlobCollection GetBlobsSharedAccessSignatures(string containerName, string blobPrefix, bool useFlatBlobListing);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/containers?containerPrefix={containerPrefix}")]
        CloudBlobContainerCollection ListContainers(string containerPrefix);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/container/{containerName}?createIfNotExists={createIfNotExists}&isPublic={isPublic}")]
        string CreateContainer(string containerName, bool createIfNotExists, bool isPublic);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/container/{containerName}")]
        string DeleteContainer(string containerName);
    }
}
