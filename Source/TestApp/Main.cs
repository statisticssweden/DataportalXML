using Px.Rdf;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            RdfSettings settings = new RdfSettings {BaseUri = "https://www.baseURI.se/", BaseApiUrl = "http://api.scb.se/OV0104/v1/doris/", 
            PreferredLanguage = "sv", CatalogTitle = "SCB Tabeller", CatalogDescription = "-", PublisherName = "SCB", CatalogLanguage = "sv", DBid = @"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\Example\Menu.xml", DBLang = "sv", DBtype = DBType.PcAxisFile};
            XML.writeToFile("../../../test.xml", settings);
        }
    }
}
