namespace Dos.Entities
{
    using System.Collections.Generic;

    public class User
    {
        public string Id { get; set; }

        public string EmailAddress { get; set; }

        public string PasswordHash { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PreferredName { get; set; }

        public string Url { get; set; }

        public ICollection<Authorization> Authorizations { get; set; } = new List<Authorization>();
    }
}