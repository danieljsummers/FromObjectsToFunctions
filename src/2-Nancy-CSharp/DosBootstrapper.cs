namespace Dos
{
    using Dos.Data;
    using Nancy;
    using Nancy.TinyIoc;
    using System.IO;

    public class DosBootstrapper : DefaultNancyBootstrapper
    {
        public DosBootstrapper() : base() { }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var cfg = DataConfig.FromJson(File.ReadAllText("data-config.json"));
            var conn = cfg.CreateConnection();
            conn.EstablishEnvironment(cfg.Database).GetAwaiter().GetResult();
            container.Register(conn);
        }
    }
}