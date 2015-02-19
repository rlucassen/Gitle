using Gitle.Model.Interfaces.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    public abstract class Touchable : ModelBase, ITouchable
    {
        public Touchable()
        {
            Touches = new List<Touch>();
        }

        public virtual IList<Touch> Touches { get; set; }

        public virtual void Touch(User user)
        {
            if (!Touches.Any(x => x.User == user))
                Touches.Add(new Touch(user, this));
        }

        public virtual bool Touched(User user)
        {
            return Touches.Any(x => x.User == user);
        }

        public virtual bool TouchedBefore(User user, DateTime datetime)
        {
            return Touches.Any(x => x.User == user && x.DateTime < datetime);
        }


    }
}
