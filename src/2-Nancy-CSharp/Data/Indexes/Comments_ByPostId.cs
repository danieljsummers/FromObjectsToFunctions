using Raven.Client.Documents.Indexes;
using System.Linq;
using Dos.Entities;

namespace Dos.Data.Indexes
{
    public class Comments_ByPostId : AbstractIndexCreationTask<Comment>
    {
        public Comments_ByPostId()
        {
            Map = comments => from comment in comments select new { comment.PostId };
        }
    }
}