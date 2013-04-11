namespace umla_lab1_0._2.Web.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IFormatSerializer
    {
        string SerializeReply(object originalReply, out string contentType);
    }
}
