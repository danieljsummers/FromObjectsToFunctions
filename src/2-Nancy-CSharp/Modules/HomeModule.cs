using Nancy;

namespace Dos.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule() : base()
        {
            Get("/", _ => "Hello World from Nancy C#");
        }
    }
}