using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestWorkTasks.Startup))]
namespace TestWorkTasks
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
