namespace Gitle.Web.Controllers
{
    #region Usings

    using System;
    using Model;
    using Model.Enum;
    using Model.Nested;
    using NHibernate;

    #endregion

    public class DatabaseController : BaseController
    {
        public DatabaseController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void DemoData(long id)
        {
            Redirect("home", "index");
        }

        public void Index()
        {
            var user = new User
                                {
                                    EmailAddress = "robin@lucassen.me",
                                    Name = "user",
                                    Password = new Password("toeter")
                                };
            session.Save(user);
            
            //var adminUser = new User
            //                    {
            //                        EmailAddress = "robin@lucassen.me",
            //                        Name = "admin",
            //                        Password = new Password("toeter")
            //                    };
            //session.Save(adminUser);

            Redirect("home", "index");
        }
    }
}