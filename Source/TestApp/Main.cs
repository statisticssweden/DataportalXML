using Px.Rdf;
using System;

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
                PreferredLanguage = "en", // For this example have English as preferredLang
                CatalogTitle = "SCB Tabeller", 
                CatalogDescription = "-", 
                PublisherName = "SCB", 
                CatalogLanguage = "sv", 
                DBid = @"H:\Mina Dokument\github\summerprojekt\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml", 
                DBLang = "sv", 
                Fetcher = new PcAxisFetcher(),
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/"
            };
            XML.writeToFile("../../../test.xml", settings);
            Console.ReadKey();
        }
    }

}

// @"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml", 
// @"H:\Mina Dokument\github\summerprojekt\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml