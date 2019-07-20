using Raven.Client.Documents.Indexes;
using System.Linq;
using Dos.Entities;

namespace Dos.Data.Indexes
{
    public class Users_ByEmailAddressAndPasswordHash : AbstractIndexCreationTask<User>
    {
        public Users_ByEmailAddressAndPasswordHash()
        {
            Map = users => from user in users
                           select new
                           {
                               user.EmailAddress,
                               user.PasswordHash
                           };
        }
    }
}