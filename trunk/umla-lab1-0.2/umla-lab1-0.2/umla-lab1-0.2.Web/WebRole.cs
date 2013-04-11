namespace umla_lab1_0._2.Web
{
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using umla_lab1_0._2.Web.Infrastructure;

    public class WebRole : RoleEntryPoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method initializes the Web role.")]
        public override bool OnStart()
        {
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) =>
                {
                    if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        // The corresponding configuration setting has changed, propagate the value
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            // In this case, the change to the storage account credentials in the
                            // service configuration is significant enough that the role needs to be
                            // recycled in order to use the latest settings. (for example, the 
                            // endpoint has changed)
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
