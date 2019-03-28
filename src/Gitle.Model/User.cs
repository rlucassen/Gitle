﻿namespace Gitle.Model
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using Enum;
    using Interfaces.Model;
    using Nested;

    #endregion

    public class User : ModelBase, IPrincipal, IIdentity, IComparable
    {
        public User()
        {
            Projects = new List<UserProject>();
        }


        public virtual IPassword Password { get; set; }
        public virtual Salutation Salutation { get; set; }
        public virtual string Name { get; set; }
        public virtual string FullName { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual IList<UserProject> Projects { get; set; }
        public virtual IList<FilterPreset> FilterPresets { get; set; }
        public virtual bool IsAdmin { get; set; }
        public virtual bool IsDanielle { get; set; }
        public virtual bool CanBookHours { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Mobile { get; set; }
        public virtual string Position { get; set; }
        public virtual string Company { get; set; }
        public virtual string GitHubUsername { get; set; }
        public virtual string GitHubAccessToken { get; set; }
        public virtual string FreckleEmail { get; set; }
        public virtual string Comments { get; set; }
        public virtual string Color { get; set; }
        public virtual int JamesEmployeeId { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual IssueState DefaultState { get; set; }

        public virtual UserProject GetUserProject(Project project)
        {
            return Projects.FirstOrDefault(x => x.Project == project) ?? new UserProject();
        }

        public virtual bool IsCustomer => !IsAdmin && !IsDanielle;

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

        public virtual int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherUser = obj as User;
            if (otherUser != null)
                return FullName.CompareTo(otherUser.FullName);
            throw new ArgumentException("Object is not a User");
        }
    }
}