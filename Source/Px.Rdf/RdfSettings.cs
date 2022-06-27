
namespace Px.Rdf
{
    public enum DBType {
        SQL,
        PcAxisFile
    }
    
    public struct RdfSettings
    {
        public string BaseUri; // Base uri, can be anything
        
        public string BaseApiUrl; // Base url for api request

        public string PreferredLanguage; // language code 2 letters

        public string CatalogTitle;
        public string CatalogDescription;
        public string CatalogLanguage; // 2 letters

        public string PublisherName;

        public string DBLang;
        public string DBid;
        public IFetcher Fetcher;
        public string LandingPageUrl;

    }
}
