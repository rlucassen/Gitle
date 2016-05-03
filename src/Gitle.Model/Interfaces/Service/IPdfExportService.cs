using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model.Interfaces.Service
{
    public interface IPdfExportService
    {
        byte[] ConvertHtmlToPdf(string template, Dictionary<string, object> propertyBag);
    }
}
