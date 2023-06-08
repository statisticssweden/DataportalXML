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
                Languages = new List<string>{"sv", "en"}, 
                CatalogTitles = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB Tabeller"), new KeyValuePair<string, string>("en", "SCB Tables") }, 
                CatalogDescriptions = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB - Beskrivning"), new KeyValuePair<string, string>("en", "SCB - Description") }, 
                PublisherName = "SCB", 
                DBid = @"C:\Temp\Databases\Example\Menu.xml",
                Fetcher = new PXFetcher(@"C:\Temp\Databases\"),
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/",
                License = "http://creativecommons.org/publicdomain/zero/1.0/",
                ThemeMapping = "C:/temp/TMapping.json"
            };
            XML.WriteToFile("../../../test.xml", settings);
        }
    }

}