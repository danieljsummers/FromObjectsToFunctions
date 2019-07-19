using Raven.Client.Documents.Indexes;
using System.Linq;
using Uno.Entities;

namespace Uno.Data.Indexes
{
    public class Posts_ByWebLogIdAndCategoryId : AbstractIndexCreationTask<Post>
    {
        public Posts_ByWebLogIdAndCategoryId()
        {
            Map = posts => from post in posts
                           from category in post.CategoryIds
                           select new
                           {
                               post.WebLogId,
                               CategoryId = category
                           };
        }
    }
}