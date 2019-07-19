using Raven.Client.Documents.Indexes;
using System.Linq;
using Uno.Entities;

namespace Uno.Data.Indexes
{
    public class Posts_ByWebLogIdAndPermalink : AbstractIndexCreationTask<Post>
    {
        public Posts_ByWebLogIdAndPermalink()
        {
            Map = posts => from post in posts
                           select new
                           {
                               post.WebLogId,
                               post.Permalink
                           };
        }
    }
}