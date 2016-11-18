using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model.Interfaces.Model
{
    public interface ITouchable
    {
        IList<Touch> Touches { get; set; }
    }
}
