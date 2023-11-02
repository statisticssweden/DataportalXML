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
                //BaseApiUrl = "http://api.scb.se/OV0104/v1/doris/",
                BaseApiUrl = "http://localhost:56338/api/v1/",
                Languages = new List<string> { "en" },
                CatalogTitles = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB Tabeller"), new KeyValuePair<string, string>("en", "SCB Tables") },
                CatalogDescriptions = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("sv", "SCB - Beskrivning"), new KeyValuePair<string, string>("en", "SCB - Description") },
                PublisherNames = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("en", "Statistics Sweden"), new KeyValuePair<string, string>("sv", "SCB") },
                DatabaseId = @"C:\Temp\Databases\Example/Menu.xml",
                //DatabaseId = @"C:\Temp\StatFin2018\StatFin\Menu.xml",
                //DatabaseId = "ssd",
                DatabaseType = DatabaseType.PX,
                //DatabaseType = DatabaseType.CNMM,
                LandingPageUrl = "http://localhost:56338/goto/",
                //LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/",
                License = "http://creativecommons.org/publicdomain/zero/1.0/",
                ThemeMapping = @"C:\Temp\DataportalXML\TestApp\Themes.json",
                OrganizationMapping = @"C:\Temp\DataportalXML\TestApp\Organizations.json",
                MainLanguage = "en",
            };
            DcatWriter.WriteToFile("../../../test.xml", settings);
        }
    }

}