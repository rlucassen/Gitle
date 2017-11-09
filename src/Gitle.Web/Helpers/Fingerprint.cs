namespace Gitle.Web.Helpers
{
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;

    public class Fingerprint
    {

        /// <summary>
        /// Adds a timestamp to the URL of the static file
        /// </summary>
        /// <see cref="http://madskristensen.net/post/cache-busting-in-aspnet"/>
        /// <param name="rootRelativePath"></param>
        /// <returns></returns>
        public static string Tag(string rootRelativePath)
        {
            if (HttpRuntime.Cache[rootRelativePath] == null)
            {
                var absolute = HostingEnvironment.MapPath("~" + rootRelativePath);

                var date = File.GetLastWriteTime(absolute);
                //var index = rootRelativePath.LastIndexOf('/');

                //var result = rootRelativePath.Insert(index, "/v-" + date.Ticks);

#if DEBUG
                var result = rootRelativePath;
#else
                var result = rootRelativePath + "?v=" + date.Ticks;
#endif
                HttpRuntime.Cache.Insert(rootRelativePath, result, new CacheDependency(absolute));
            }

            return HttpRuntime.Cache[rootRelativePath] as string;
        }
    }
}