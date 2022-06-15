using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public static class Constants
    {
        public static Organization SCB = new Organization() { name = "SCB - Statistiska Centralbyrån", reference = "https://www.scb.se"};
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

    public struct Dataset
    {
        public string title;
        public string description;
        public string[] languages;
        public ContactPerson[] contactPersons;
        public Organization publisher;
        public string identifier;
        public string modified;
        public string updateFrequency;
        public string url()
        {
            return url(true);
        }
        public string url(bool swe)
        {
            string lang = swe ? "sv" : "en";
            return "http://www.statistikdatabasen.scb.se/goto/"+ lang + "/ssd/" + identifier;
        }
    }

    public struct Organization
    {
        public string name;
        public string reference;
    }

    public struct ContactPerson
    { 
        public string url;
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
