using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model.Mapping
{
    public class TouchMap : ModelBaseMap<Touch>
    {
        public TouchMap() {
            Map(x => x.DateTime);
            References(x => x.User).Column("User_id");
            ReferencesAny(x => x.Touchable)
                .AddMetaValue<Issue>(typeof(Issue).Name)
                .AddMetaValue<Change>(typeof(Change).Name)
                .AddMetaValue<ChangeState>(typeof(ChangeState).Name)
                .AddMetaValue<Comment>(typeof(Comment).Name)
                .EntityTypeColumn("Seeable_type")
                .EntityIdentifierColumn("Seeable_id")
                .IdentityType<long>();
        }
    }
}
