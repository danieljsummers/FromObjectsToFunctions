using Raven.Client.Documents.Indexes;
using System.Linq;
using Dos.Entities;

namespace Dos.Data.Indexes
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