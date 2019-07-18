using Raven.Client.Documents.Indexes;
using System.Linq;
using Uno.Entities;

namespace Uno.Data.Indexes
{
    public class Categories_ByWebLogId : AbstractIndexCreationTask<Category>
    {
        public Categories_ByWebLogId()
        {
            Map = categories => from category in categories select category.WebLogId;
        }
    }
}
