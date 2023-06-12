using Newtonsoft.Json; // Json
using System.Collections.Generic;
using System.IO;

namespace Px.Dcat.Helpers
{
    public class JsonReader
    {
        public static Dictionary<string, string> ReadDictionary(string fileName)
        {
            string json = File.ReadAllText(fileName);
            Dictionary<string, string> mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return mapping;
        }
    }
}