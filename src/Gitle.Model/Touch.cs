using Gitle.Model.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    public class Touch : ModelBase
    {
        protected Touch()
        {

        }

        public Touch(User user, ITouchable touchable)
        {
            User = user;
            Touchable = touchable;
            DateTime = DateTime.Now;
        }

        public virtual User User { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual ITouchable Touchable { get; set; }
    }
}
