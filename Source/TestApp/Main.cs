using Px.Rdf;
using System;
using System.Collections.Generic;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            RdfSettings settings = new RdfSettings 
            {
                BaseUri = "https://www.baseURI.se/", 
                BaseApiUrl = "http://api.scb.se/OV0104/v1/doris/", 
                PreferredLanguage = "sv",
                Languages = new List<string>{"sv", "en"}, 
                CatalogTitle = "SCB Tabeller", 
                CatalogDescription = "-", 
                PublisherName = "SCB", 
                DBid = "ssd",
                Fetcher = new CNMMFetcher(),
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/",
                License = "http://creativecommons.org/publicdomain/zero/1.0/",
                ThemeMapping = "C:/temp/TMapping.json"
            };
            XML.WriteToFile("../../../test.xml", settings);
        }
    }

}