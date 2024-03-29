﻿namespace umla_lab1_0._2.Web.Serializers
{
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using umla_lab1_0._2.Web.Infrastructure;

    public class FormatSerializerFactory : IFormatSerializerFactory
    {
        private IFormatSerializer jsonSerializer;
        private IFormatSerializer jsonpSerializer;
        private IFormatSerializer xmlSerializer;

        private IFormatSerializer JsonSerializer
        {
            get
            {
                if (this.jsonSerializer == null)
                {
                    this.jsonSerializer = new JsonSerializer();
                }

                return this.jsonSerializer;
            }
        }

        private IFormatSerializer JsonpSerializer
        {
            get
            {
                if (this.jsonpSerializer == null)
                {
                    this.jsonpSerializer = new JsonSerializer();
                }

                return this.jsonpSerializer;
            }
        }

        private IFormatSerializer XmlSerializer
        {
            get
            {
                if (this.xmlSerializer == null)
                {
                    this.xmlSerializer = new XmlSerializer();
                }

                return this.xmlSerializer;
            }
        }

        public IFormatSerializer GetSerializer(NameValueCollection headers, NameValueCollection queryString)
        {
            // Check content type too.
            var mimeType = headers["Accept"] ?? HttpConstants.MimeApplicationAtomXml;
            string callbackName = string.Empty;

            if (HttpContext.Current != null)
            {
                callbackName = queryString["jsonCallback"] ?? queryString["callback"];
            }

            if (mimeType.Contains(HttpConstants.MimeApplicationJson))
            {
                return string.IsNullOrWhiteSpace(callbackName) ? this.JsonSerializer : this.JsonpSerializer;
            }

            if (mimeType.Contains("text/xml") || mimeType.Contains("application/xml"))
            {
                return this.XmlSerializer;
            }

            if (!string.IsNullOrWhiteSpace(callbackName))
            {
                return this.JsonpSerializer;
            }

            return this.XmlSerializer;
        }
    }
}