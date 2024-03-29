﻿namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public class AutomaticFormatServiceHostFactory : WebServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new AutomaticFormatWebServiceHost(serviceType, baseAddresses);
        }
    }
}