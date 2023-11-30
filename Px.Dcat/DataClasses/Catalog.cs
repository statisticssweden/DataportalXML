using System.Collections.Generic;

namespace Px.Dcat.DataClasses
{
    public class Catalog
    {
        public List<KeyValuePair<string, string>> Titles;
        public List<KeyValuePair<string, string>> Descriptions;
        public string License;
        public List<string> Languages;
        public List<Dataset> Datasets;
        public Organization Publisher;
        public string Refrence { get; set; }
    }
}
