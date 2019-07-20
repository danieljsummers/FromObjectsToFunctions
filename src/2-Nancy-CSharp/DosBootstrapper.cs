using Dos.Data;
using Dos.Data.Indexes;
using Nancy;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using System.IO;

namespace Dos
{
    class DosBootstrapper : DefaultNancyBootstrapper
    {
        public DosBootstrapper() : base() { }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var cfg = JsonConvert.DeserializeObject<DataConfig>(File.ReadAllText("data-config.json"));
            var store = new DocumentStore
            {
                Urls = cfg.Urls,
                Database = cfg.Database
            };
            container.Register(store.Initialize());
            IndexCreation.CreateIndexes(typeof(Categories_ByWebLogIdAndSlug).Assembly, store);
        }
    }
}