using System;
using System.Linq;
using System.Text;

namespace ClientWPF.Helpers
{
    public static class UriHelpers
    {
        public static Uri Combine(params string[] parts)
        {
            if (parts == null)
                return null;
            string url = parts.Aggregate(Combine);
            return new Uri(url);
        }

        private static string Combine(string path, string token) // workaround for local webservice address starting with :
        {
            StringBuilder sb = new StringBuilder(path);
            if (!path.EndsWith("/") && !token.StartsWith(":"))
                sb.Append('/');
            if (token.StartsWith(":"))
                sb.Append(token);
            else if (token.StartsWith("/"))
                sb.Append(token.TrimStart('/'));
            else
                sb.Append(token);
            return sb.ToString();
        }
    }
}
