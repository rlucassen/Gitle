namespace Gitle.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using Castle.MonoRail.Framework;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class FileBinderAttribute : Attribute, IParameterBinder
    {
        public int CalculateParamPoints(IEngineContext context, IController controller,
                                        IControllerContext controllerContext, ParameterInfo parameterInfo)
        {
            var key = parameterInfo.Name;
            return context.Request.Files[key] != null ? 10 : 0;
        }

        public object Bind(IEngineContext context, IController controller, IControllerContext controllerContext,
                           ParameterInfo parameterInfo)
        {
            var key = parameterInfo.Name;

            var ret = new List<HttpPostedFile>();
            var httpFileCollection = context.UnderlyingContext.Request.Files;
            for (var i = 0; i < httpFileCollection.Count; i++)
                if (httpFileCollection.GetKey(i) == key)
                    ret.Add(httpFileCollection[i]);

            return ret;

        }
    }
}