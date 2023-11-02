using Px.Dcat.Interfaces;
using System.Collections.Generic;
using Px.Dcat.Helpers;

namespace Px.Dcat
{
    public struct DcatSettings
    {
        public string BaseUri; // Base uri, can be anything

        public string BaseApiUrl; // Base url for api request

        public List<string> Languages; // Read from settings
        public List<KeyValuePair<string, string>> CatalogTitles;
        public List<KeyValuePair<string, string>> CatalogDescriptions;
        public List<KeyValuePair<string, string>> PublisherNames;

        public string DatabaseId;
        public DatabaseType DatabaseType;
        public string LandingPageUrl;
        public string License;
        public string ThemeMapping;
        public string OrganizationMapping;

        public string MainLanguage;
    }
}
