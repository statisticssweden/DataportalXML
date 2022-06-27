﻿using Px.Rdf;
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
                PreferredLanguage = "sv", // For this example have English as preferredLang
                Languages = new List<string>{"sv", "en"}, 
                CatalogTitle = "SCB Tabeller", 
                CatalogDescription = "-", 
                PublisherName = "SCB", 
                DBid = @"C:\Temp\StatFin2018\StatFin\Menu.xml",//@"C:\Temp\Databases\Example\Menu.xml", 
                Fetcher = new PcAxisFetcher(@"C:\Temp\StatFin2018\"),
                LandingPageUrl = "http://www.statistikdatabasen.scb.se/goto/"
            };
            XML.writeToFile("../../../test.xml", settings);
            Console.ReadKey();
        }
    }

}


// @"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml", 
// @"C:\Temp\Databases\Example\Menu.xml