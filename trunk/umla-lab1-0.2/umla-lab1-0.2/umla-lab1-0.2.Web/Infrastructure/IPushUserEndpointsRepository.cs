namespace umla_lab1_0._2.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using umla_lab1_0._2.Web.Models;

    [CLSCompliant(false)]
    public interface IPushUserEndpointsRepository
    {
        void AddPushUserEndpoint(PushUserEndpoint pushUserEndpoint);

        void UpdatePushUserEndpoint(PushUserEndpoint pushUserEndpoint);

        void RemovePushUserEndpoint(PushUserEndpoint pushUserEndpoint);

        IEnumerable<string> GetAllPushUsers();

        IEnumerable<PushUserEndpoint> GetPushUsersByName(string userId);

        PushUserEndpoint GetPushUserByApplicationAndDevice(string applicationId, string deviceId);
    }
}
