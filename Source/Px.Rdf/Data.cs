using System.Collections.Generic;

namespace Px.Rdf
{
    public class Catalog
    {
        public List<KeyValuePair<string, string>> titles;
        public List<KeyValuePair<string, string>> descriptions;
        public string license;
        public List<string> languages;
        public List<Dataset> datasets;
        public Organization publisher;
    }

    public class Keyword {
        public string lang;
        public string text;
    }

    public class Distribution
    {
        public string title;
        public string accessUrl;
        public string resource;
        public string license;
        public string language;
        public string format;
    }
    public class Dataset
    {
        public List<string> titles;
        public List<string> descriptions;
        public List<string> languageURIs;
        public List<string> languages;
        public string category;
        public List<ContactPerson> contactPersons;
        public Organization publisher;
        public List<Keyword> keywords;
        public string identifier;
        public string modified;
        public string updateFrequency;
        public Organization producer;
        public List<Distribution> distributions;
        public string resource;
        public List<string> sources;

        public List<string> urls;
    }

    public class Organization
    {
        public HashSet<(string,string)> names; // (language, name)
        public string resource;

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != this.GetType())
                return false;
            Organization other = (Organization)obj;
            return names.SetEquals(other.names);
        }

        public override int GetHashCode()
        {
            return names.GetHashCode();
        }
    }

    public class ContactPerson
    { 
        public string resource;
        public string name;
        public string email;
        public string telephone;

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != this.GetType())
                return false;
            ContactPerson cp = (ContactPerson) obj;
            return this.email == cp.email;
        }

        public override int GetHashCode()
        {
            return email.GetHashCode();
        }
    }
}
