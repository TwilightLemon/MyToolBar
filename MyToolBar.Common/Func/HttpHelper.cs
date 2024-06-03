using System.Net;
using System.Net.Http;

namespace MyToolBar.Common.Func
{
    public static class HttpHelper
    {
        public static async Task<string> Get(string url,bool useGzip=true)
        {
            SocketsHttpHandler hd = new();
            if(useGzip)hd.AutomaticDecompression=DecompressionMethods.GZip;
            HttpClient hc = new(hd);
            hc.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
            return await hc.GetStringAsync(url);
        }

        /// <summary>
        /// 查找AB中间的文本
        /// </summary>
        /// <param name="str"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static string FindByAB(string str,string A,string B)
        {
            int a = str.IndexOf(A);
            if (a == -1) return "";
            int b = str.IndexOf(B, a + A.Length);
            if (b == -1) return "";
            return str.Substring(a + A.Length, b - a - A.Length);
        }
    }
}
