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
        public Organization publisher;
        public string identifier;
        public string contact;
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

}
