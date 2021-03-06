using System.Collections.Generic;

namespace Px.Rdf
{
    public struct RdfSettings
    {
        public string BaseUri; // Base uri, can be anything
        
        public string BaseApiUrl; // Base url for api request

        public string PreferredLanguage; // language code 2 letters

        public List<string> Languages; // Read from settings

        public string CatalogTitle;
        public string CatalogDescription;

        public string PublisherName;
        public string DBid;
        public IFetcher Fetcher; // Create depending on chosen database
        public string LandingPageUrl;
        public string License;
        public string ThemeMapping;

    }
}
