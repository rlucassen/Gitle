namespace Gitle.Model
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using Enum;
    using Interfaces.Model;
    using Nested;

    #endregion

    public class User : ModelBase, IPrincipal, IIdentity
    {
        public virtual IPassword Password { get; set; }
        public virtual string Name { get; set; }
        public virtual string FullName { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual IList<UserProject> Projects { get; set; }
        public virtual bool IsAdmin { get; set; }
        public virtual string Phone { get; set; }
        public virtual string GitHubUsername { get; set; }

        #region Implementation of IPrincipal

        public virtual bool IsInRole(string role)
        {
            return GetType().Name.Equals(role);
        }

        public virtual IIdentity Identity
        {
            get { return this; }
        }

        #endregion

        #region Implementation of IIdentity

        public virtual string AuthenticationType
        {
            get { return "Forms"; }
        }

        public virtual bool IsAuthenticated
        {
            get { return false; }
        }

        #endregion

        public class NullUser : User
        {
            public override bool IsActive
            {
                get { return false; }
            }

            public override IPassword Password
            {
                get { return new Password(); }
            }
        }
    }
}