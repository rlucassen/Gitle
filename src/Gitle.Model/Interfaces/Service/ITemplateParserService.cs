using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model.Interfaces.Service
{
    public interface ITemplateParserService
    {
        string Parse(string templateName, Dictionary<string, object> propertyBag);
    }
}
