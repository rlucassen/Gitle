using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model.Mapping
{
    public class TouchableMap<T> : ModelBaseMap<T> where T : Touchable
    {
        public TouchableMap()
        {
            HasMany(x => x.Touches).Cascade.All();
        }
    }
}
