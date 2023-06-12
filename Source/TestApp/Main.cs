using Px.Dcat;
using Px.Dcat.Fetchers;
using Px.Dcat.Helpers;
using System;
using System.Collections.Generic;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            DcatSettings settings = new DcatSettings
            {
                BaseUri = "https://www.baseURI.se/",
                BaseApiUrl = "http://api.scb.se/OV0104/v1/doris/",
                Languages = new List<string> { "sv", "en" },
                CatalogTitles = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB Tabeller"), new KeyValuePair<string, string>("en", "SCB Tables") },
                CatalogDescriptions = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB - Beskrivning"), new KeyValuePair<string, string>("en", "SCB - Description") },
                PublisherName = "Statistics Sweden",
                DatabaseId = @"C:\Temp\Databases\Example\Menu.xml",
                DatabaseType = DatabaseType.PX,
                //DBid = @"C:\Temp\StatFin2018\StatFin\Menu.xml",
                //Fetcher = new PXFetcher(@"C:\Temp\StatFin2018"),
                //DatabaseId = "ssd",
                //DatabaseType = DatabaseType.CNMM,
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/",
                License = "http://creativecommons.org/publicdomain/zero/1.0/",
                ThemeMapping = @"C:\Temp\DataportalXML\Source\Themes.json",
                OrganizationMapping = @"C:\Temp\DataportalXML\Source\Organizations.json",
                MainLanguage = "en",
            };
            DcatWriter.WriteToFile("../../../test.xml", settings);
        }
    }

}