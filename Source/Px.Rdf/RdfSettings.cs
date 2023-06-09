using System.Collections.Generic;

namespace Px.Rdf
{
    public struct RdfSettings
    {
        public string BaseUri; // Base uri, can be anything
        
        public string BaseApiUrl; // Base url for api request

        public List<string> Languages; // Read from settings
        public List<KeyValuePair<string, string>> CatalogTitles;
        public List<KeyValuePair<string, string>> CatalogDescriptions;

        public string PublisherName;
        public string DBid;
        public IFetcher Fetcher; // Create depending on chosen database
        public string LandingPageUrl;
        public string License;
        public string ThemeMapping;

        public string MainLanguage;
    }
}
