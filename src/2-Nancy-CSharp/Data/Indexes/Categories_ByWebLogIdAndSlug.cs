using Raven.Client.Documents.Indexes;
using System.Linq;
using Dos.Entities;

namespace Dos.Data.Indexes
{
    public class Categories_ByWebLogIdAndSlug : AbstractIndexCreationTask<Category>
    {
        public Categories_ByWebLogIdAndSlug()
        {
            Map = categories => from category in categories
                                select new
                                {
                                    category.WebLogId,
                                    category.Slug
                                };
        }
    }
}