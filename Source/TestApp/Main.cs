﻿using Px.Rdf;
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
                PreferredLanguage = "fi", // For this example have English as preferredLang
                CatalogTitle = "SCB Tabeller", 
                CatalogDescription = "-", 
                PublisherName = "SCB", 
                CatalogLanguage = "fi", 
                DBid = @"C:\Temp\StatFin2018\StatFin\Menu.xml",//@"C:\Temp\Databases\Example\Menu.xml", 
                DBLang = "fi", 
                Fetcher = new PcAxisFetcher(),
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/"
            };
            XML.writeToFile("../../../test.xml", settings);
            Console.ReadKey();
        }
    }

}

// @"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml", 
// @"C:\Temp\Databases\Example\Menu.xml