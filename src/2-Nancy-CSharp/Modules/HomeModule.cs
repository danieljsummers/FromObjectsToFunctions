namespace Dos.Modules
{
    using Nancy;
    
    public class HomeModule : NancyModule
    {
        public HomeModule() : base()
        {
            Get("/", _ => "Hello World from Nancy C#");
        }
    }
}