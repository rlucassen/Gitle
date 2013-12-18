namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using Admin;
    using Clients.GitHub.Interfaces;
    using Clients.GitHub.Models;
    using Model;
    using Model.Interfaces.Repository;
    using Helpers;

    public class CustomerController : SecureController
    {
        private ICustomerRepository customerRepository;

        public CustomerController(ICustomerRepository customerRepository) : base()
        {
            this.customerRepository = customerRepository;
        }

        [Admin]
        public void Index()
        {
            PropertyBag.Add("items", customerRepository.FindAll());
        }

        [Admin]
        public void New()
        {
            PropertyBag.Add("item", new Customer());
            RenderView("edit");
        }

        [Admin]
        public void Edit(int customerId)
        {
            PropertyBag.Add("item", customerRepository.Get(customerId));
        }

        [Admin]
        public void Delete(int customerId)
        {
            var customer = customerRepository.Get(customerId);
            customer.Deactivate();
            customerRepository.Save(customer);
            RedirectToReferrer();
        }

        [Admin]
        public void Save(int customerId)
        {
            var item = customerRepository.Get(customerId);
            if (item != null)
            {
                BindObjectInstance(item, "item");
            }
            else
            {
                item = BindObject<Customer>("item");
            }
            customerRepository.Save(item);

            RedirectToUrl("/customers");
        }
    }
}