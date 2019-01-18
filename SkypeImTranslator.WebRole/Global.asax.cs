namespace SkypeImTranslator.WebRole
{
    using System;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using Unity;

    public class Global : HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(RegisterWebApplication);
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }

        private static void RegisterWebApplication(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.DependencyResolver = new Utils.UnityResolver(RegisterDependencies());
        }

        private static IUnityContainer RegisterDependencies()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterInstance<HttpClient>(new HttpClient(), new Unity.Lifetime.ContainerControlledLifetimeManager());

            return container;
        }
    }
}