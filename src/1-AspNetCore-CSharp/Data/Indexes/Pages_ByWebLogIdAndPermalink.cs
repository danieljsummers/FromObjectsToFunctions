using Raven.Client.Documents.Indexes;
using System.Linq;
using Uno.Entities;

namespace Uno.Data.Indexes
{
    public class Pages_ByWebLogIdAndPermalink : AbstractIndexCreationTask<Page>
    {
        public Pages_ByWebLogIdAndPermalink()
        {
            Map = pages => from page in pages
                           select new
                           {
                               page.WebLogId,
                               page.Permalink
                           };
        }
    }
}