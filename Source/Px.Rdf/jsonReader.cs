using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Px.Rdf {
    public class JsonReader{
        public static Dictionary<string,string> ReadThemeMapping(string fileName) {
            string json = File.ReadAllText(fileName);
            Dictionary<string, string> mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return mapping;
        }
    }
}