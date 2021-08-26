using Newtonsoft.Json.Linq;
using System.Linq;

namespace SBExplorer.Extensions
{
    public static class JsonExtensions
    {
        public static string GetKey(this JToken token)
        {
            return token.Path.Split('.').Last();
        }
    }
}
