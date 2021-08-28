using System.Linq;
using Newtonsoft.Json.Linq;

namespace SBExplorer.Core.Extensions
{
    public static class JsonExtensions
    {
        public static string GetKey(this JToken token)
        {
            return token.Path.Split('.').Last();
        }
    }
}