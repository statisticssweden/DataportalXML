using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public static class Constants
    {
        public static Organization SCB = new Organization() { name = "SCB - Statistiska Centralbyrån", resource = "https://www.scb.se"};
    }

    public struct Catalog
    {
        public string title;
        public string description;
        public string license;
        public string language;
        public Dataset[] datasets;
        public Organization publisher;
    }

    public struct Keyword {
        public string lang;
        public string text;
    }

    public struct Distribution
    {
        public string title;
        public string accessUrl;
        public string resource;
        public string license;
        public string language;
        public string format;
    }
    public struct Dataset
    {
        public string title;
        public string description;
        public string[] languages;
        public string category;
        public ContactPerson[] contactPersons;
        public Organization publisher;
        public Keyword[] keywords;
        public string identifier;
        public string modified;
        public string updateFrequency;
        public Organization producer;
        public Distribution[] distributions;
        public string resource;
        public string url()
        {
            return url(true);
        }
        public string url(bool swe)
        {
            string lang = swe ? "sv" : "en";
            return "http://www.statistikdatabasen.scb.se/goto/" + lang + "/ssd/" + identifier;
        }
    }

    public struct Organization
    {
        public string name;
        public string resource;

        public override bool Equals(object obj)
        {
            if (obj is null || obj.GetType() != this.GetType())
                return false;
            Organization o = (Organization) obj;
            return this.name == o.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }

    public struct ContactPerson
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
