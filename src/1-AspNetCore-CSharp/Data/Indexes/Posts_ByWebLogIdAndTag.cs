using Raven.Client.Documents.Indexes;
using System.Linq;
using Uno.Entities;

namespace Uno.Data.Indexes
{
    public class Posts_ByWebLogIdAndTag : AbstractIndexCreationTask<Post>
    {
        public Posts_ByWebLogIdAndTag()
        {
            Map = posts => from post in posts
                           from tag in post.Tags
                           select new
                           {
                               post.Id,
                               Tag = tag
                           };
        }
    }
}