using System.Collections.Generic;

namespace Px.Dcat.DataClasses
{
    public class Dataset
    {
        public List<string> Titles;
        public List<string> Descriptions;
        public List<string> LanguageURIs;
        public List<string> Languages;
        public string Category;
        public List<ContactPerson> ContactPersons;
        public Organization Publisher;
        public List<Keyword> Keywords;
        public string Identifier;
        public string Modified;
        public string UpdateFrequency;
        public Organization Producer;
        public List<Distribution> Distributions;
        public string Resource;
        public List<string> Sources;

        public List<string> Urls;
    }
}
