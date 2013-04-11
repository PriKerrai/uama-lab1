namespace umla_lab1_0._2.Web.Serializers
{
    using System.Xml;
    using umla_lab1_0._2.Web.Infrastructure;

    public class XmlSerializer : IFormatSerializer
    {
        public string SerializeReply(object originalReply, out string contentType)
        {
            contentType = HttpConstants.MimeApplicationAtomXml;

            return (originalReply as XmlDocument).InnerXml;
        }
    }
}