namespace umla_lab1_0._2.Web.Serializers
{
    using System.Collections.Specialized;

    public interface IFormatSerializerFactory
    {
        IFormatSerializer GetSerializer(NameValueCollection headers, NameValueCollection queryString);
    }
}
