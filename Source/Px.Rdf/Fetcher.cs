using System;
using System.Data;
using System.IO;
using System.Web;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;

namespace Px.Rdf
{
    public class Fetcher
    {
        private const string PCAXIS_DATE_FORMAT = PXConstant.PXDATEFORMAT;
        private const string DCAT_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private int hashNum;
        private Dictionary<string, Organization> organizations = new Dictionary<string, Organization>(); // name to organization
        private Dictionary<(string, string), Keyword> menuLangKeyword = new Dictionary<(string, string), Keyword>(); // (menuID, language) -> Array of keywords
        protected Dictionary<string, ContactPerson> contacts = new Dictionary<string, ContactPerson>(); // email to contactPerson
        protected RdfSettings settings;

        // Mapping from PcAxis categories to DCAT standard themes https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcat_theme
        private Dictionary<string, string> themeMapping;
        
        // Get all unique Organizations
        public List<Organization> UniqueOrgs() {
            return organizations.Values.ToList();
        }

        // Get all unique Contacts
        public List<ContactPerson> UniqueContacts() {
            return contacts.Values.ToList();
        }

        // Mapping from PcAxis TimeScaleType to DCAT standard https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_accrualPeriodicity
        private Dictionary<TimeScaleType, string> timeScaleToUpdateFreq
            = new Dictionary<TimeScaleType, string>
        {
            { TimeScaleType.NotSet, "OTHER"},
            { TimeScaleType.Annual, "ANNUAL"},
            { TimeScaleType.Halfyear, "ANNUAL_2"},
            { TimeScaleType.Monthly, "MONTHLY"},
            { TimeScaleType.Quartely, "QUARTERLY"},
            { TimeScaleType.Weekly, "ANNUAL"},
        };

        // Mapping from ISO 639-1 to ISO 639-3 (2 letters to 3) used for DCAT languages https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_language
        private Dictionary<string, string> languageToDcatLang
            = new Dictionary<string, string>  {
            { "aa", "aar"}, 
            { "ab", "abk"}, 
            { "af", "afr"}, 
            { "ak", "aka"}, 
            { "am", "amh"}, 
            { "ar", "ara"}, 
            { "an", "arg"}, 
            { "as", "asm"}, 
            { "av", "ava"}, 
            { "ae", "ave"}, 
            { "ay", "aym"}, 
            { "az", "aze"}, 
            { "ba", "bak"}, 
            { "bm", "bam"}, 
            { "be", "bel"}, 
            { "bn", "ben"}, 
            { "bi", "bis"}, 
            { "bo", "bod"}, 
            { "bs", "bos"}, 
            { "br", "bre"}, 
            { "bg", "bul"}, 
            { "ca", "cat"}, 
            { "cs", "ces"}, 
            { "ch", "cha"}, 
            { "ce", "che"}, 
            { "cu", "chu"}, 
            { "cv", "chv"}, 
            { "kw", "cor"}, 
            { "co", "cos"}, 
            { "cr", "cre"}, 
            { "cy", "cym"}, 
            { "da", "dan"}, 
            { "de", "deu"}, 
            { "dv", "div"}, 
            { "dz", "dzo"}, 
            { "el", "ell"}, 
            { "en", "eng"}, 
            { "eo", "epo"}, 
            { "et", "est"}, 
            { "eu", "eus"}, 
            { "ee", "ewe"}, 
            { "fo", "fao"}, 
            { "fa", "fas"}, 
            { "fj", "fij"}, 
            { "fi", "fin"}, 
            { "fr", "fra"}, 
            { "fy", "fry"}, 
            { "ff", "ful"}, 
            { "gd", "gla"}, 
            { "ga", "gle"}, 
            { "gl", "glg"}, 
            { "gv", "glv"}, 
            { "gn", "grn"}, 
            { "gu", "guj"}, 
            { "ht", "hat"}, 
            { "ha", "hau"}, 
            { "sh", "hbs"}, 
            { "he", "heb"}, 
            { "hz", "her"}, 
            { "hi", "hin"}, 
            { "ho", "hmo"}, 
            { "hr", "hrv"}, 
            { "hu", "hun"}, 
            { "hy", "hye"}, 
            { "ig", "ibo"}, 
            { "io", "ido"}, 
            { "ii", "iii"}, 
            { "iu", "iku"}, 
            { "ie", "ile"}, 
            { "ia", "ina"}, 
            { "id", "ind"}, 
            { "ik", "ipk"}, 
            { "is", "isl"}, 
            { "it", "ita"}, 
            { "jv", "jav"}, 
            { "ja", "jpn"}, 
            { "kl", "kal"}, 
            { "kn", "kan"}, 
            { "ks", "kas"}, 
            { "ka", "kat"}, 
            { "kr", "kau"}, 
            { "kk", "kaz"}, 
            { "km", "khm"}, 
            { "ki", "kik"}, 
            { "rw", "kin"}, 
            { "ky", "kir"}, 
            { "kv", "kom"}, 
            { "kg", "kon"}, 
            { "ko", "kor"}, 
            { "kj", "kua"}, 
            { "ku", "kur"}, 
            { "lo", "lao"}, 
            { "la", "lat"}, 
            { "lv", "lav"}, 
            { "li", "lim"}, 
            { "ln", "lin"}, 
            { "lt", "lit"}, 
            { "lb", "ltz"}, 
            { "lu", "lub"}, 
            { "lg", "lug"}, 
            { "mh", "mah"}, 
            { "ml", "mal"}, 
            { "mr", "mar"}, 
            { "mk", "mkd"}, 
            { "mg", "mlg"}, 
            { "mt", "mlt"}, 
            { "mn", "mon"}, 
            { "mi", "mri"}, 
            { "ms", "msa"}, 
            { "my", "mya"}, 
            { "na", "nau"}, 
            { "nv", "nav"}, 
            { "nr", "nbl"}, 
            { "nd", "nde"}, 
            { "ng", "ndo"}, 
            { "ne", "nep"}, 
            { "nl", "nld"}, 
            { "nn", "nno"}, 
            { "nb", "nob"}, 
            { "no", "nor"}, 
            { "ny", "nya"}, 
            { "oc", "oci"}, 
            { "oj", "oji"}, 
            { "or", "ori"}, 
            { "om", "orm"}, 
            { "os", "oss"}, 
            { "pa", "pan"}, 
            { "pi", "pli"}, 
            { "pl", "pol"}, 
            { "pt", "por"}, 
            { "ps", "pus"}, 
            { "qu", "que"}, 
            { "rm", "roh"}, 
            { "ro", "ron"}, 
            { "rn", "run"}, 
            { "ru", "rus"}, 
            { "sg", "sag"}, 
            { "sa", "san"}, 
            { "si", "sin"}, 
            { "sk", "slk"}, 
            { "sl", "slv"}, 
            { "se", "sme"}, 
            { "sm", "smo"}, 
            { "sn", "sna"}, 
            { "sd", "snd"}, 
            { "so", "som"}, 
            { "st", "sot"}, 
            { "es", "spa"}, 
            { "sq", "sqi"}, 
            { "sc", "srd"}, 
            { "sr", "srp"}, 
            { "ss", "ssw"}, 
            { "su", "sun"}, 
            { "sw", "swa"}, 
            { "sv", "swe"}, 
            { "ty", "tah"}, 
            { "ta", "tam"}, 
            { "tt", "tat"}, 
            { "te", "tel"}, 
            { "tg", "tgk"}, 
            { "tl", "tgl"}, 
            { "th", "tha"}, 
            { "ti", "tir"}, 
            { "to", "ton"}, 
            { "tn", "tsn"}, 
            { "ts", "tso"}, 
            { "tk", "tuk"}, 
            { "tr", "tur"}, 
            { "tw", "twi"}, 
            { "ug", "uig"}, 
            { "uk", "ukr"}, 
            { "ur", "urd"}, 
            { "uz", "uzb"}, 
            { "ve", "ven"}, 
            { "vi", "vie"}, 
            { "vo", "vol"}, 
            { "wa", "wln"}, 
            { "wo", "wol"}, 
            { "xh", "xho"}, 
            { "yi", "yid"}, 
            { "yo", "yor"}, 
            { "za", "zha"}, 
            { "zh", "zho"}, 
            { "zu", "zul"}
        };

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="rdfSettings">Settings to load</param>
        public void LoadSettings(RdfSettings rdfSettings) {
            settings = rdfSettings;
            themeMapping = JsonReader.ReadThemeMapping(settings.ThemeMapping);
        }

        /// <summary>
        /// Get next unique integer
        /// </summary>
        /// <returns>Integer</returns>
        protected int nextNum() {
            return ++hashNum;
        }
        /// <summary>
        /// Get next unique string
        /// </summary>
        /// <returns>String</returns>
        protected string nextString() {
            return (++hashNum).ToString();
        }

        /// <summary>
        /// Gets description for specified languages
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <param name="langs">List of languages</param>
        /// <returns></returns>
        private List<string> getDescriptions(PXMeta meta, List<string> langs)
        {   
            List<string> descriptions = new List<string>(langs.Count());

            foreach(string lang in langs) {
                string desc = "-";
                meta.SetLanguage(lang);
                Notes notes = meta.Notes;
                if (notes.Count > 0 && notes[0].Mandantory)
                {
                    desc = notes[0].Text;
                }
                descriptions.Add(desc);
            }
            return descriptions;
        }
        
        /// <summary>
        /// Gets the title for each language 
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <param name="langs">List of languages</param>
        /// <returns>Return titles in each language for a table</returns>
        private List<string> getTitles(PXMeta meta, List<string> langs) {
            List<string> titles = new List<string>(langs.Count());
            foreach(string lang in langs) {
                meta.SetLanguage(lang);
                titles.Add(meta.Title);
            }
            return titles;
        }

        /// <summary> 
        /// Gets update frequency
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns></returns>
        private string getUpdateFrequency(PXMeta meta)
        {
            Predicate<Variable> isTime = (Variable v) => v.IsTime;
            Variable timeVar = meta.Variables.Find(isTime);
            if (timeVar is null) {
                return null;
            }
            TimeScaleType timeScale = timeVar.TimeScale;
            string baseURI = "http://publications.europa.eu/resource/authority/frequency/";
            return baseURI + timeScaleToUpdateFreq[timeScale];
        }

        /// <summary>
        /// Convert language from 2 letter code to Dcat-ap standard
        /// </summary>
        /// <param name="str">Language to convert</param>
        /// <returns>The converted language, url</returns>
        private string convertLanguage(string str)
        {
            string lang = languageToDcatLang[str];
            return "http://publications.europa.eu/resource/authority/language/" + lang.ToUpper();
        }

        /// <summary>
        /// Gets all existing languages for a table
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns>Returns a list of strings where it's either one or more languages</returns>
        private List<string> getLanguages(PXMeta meta)
        {
            string[] allLangs = meta.GetAllLanguages();
            if (allLangs is null)
            {
                return new List<string> { meta.Language };
            }
            return new List<string>(allLangs);
        }

        /// <summary>
        /// Converts given languages from 2 letter code to Dcat-ap standard
        /// </summary>
        /// <param name="languages">List of languages to convert</param>
        /// <returns>Returns each URL for every language available</returns>
        private List<string> convertLanguages(List<string> languages) {
            List<string> converted = new List<string>(languages.Count());
            foreach(string lang in languages)
            {
                converted.Add(convertLanguage(lang));
            }
            return converted;
        }
        
        /// <summary>
        /// Convert from pcAxis date format to xsd:dateTime format
        /// </summary>
        /// <param name="s">Date to convert</param>
        /// <returns>Returns the formatted date</returns>
        private string reformatDate(string s)
        {
            DateTime date = DateTime.ParseExact(s, PCAXIS_DATE_FORMAT, null);
            string formatted = date.ToString(DCAT_DATE_FORMAT);
            return formatted;
        }
        /// <summary>
        /// Get latest date in a list of dates (PcAxis format)
        /// </summary>
        /// <param name="dates">List of dates</param>
        /// <returns>Latest date </returns>
        private string getLatestDate(List<string> dates) // format yyyyMMdd HH:mm
        {
            List<DateTime> dateTimes = new List<DateTime>(dates.Count());
            foreach (string date in dates) {
                dateTimes.Add(DateTime.ParseExact(date, PCAXIS_DATE_FORMAT, null));
            }
            int maxIndex = dateTimes.IndexOf(dateTimes.Max());
            return dates[maxIndex];
        }

        /// <summary>
        /// Get contacts of a table
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns>List of contacts</returns>
        public List<ContactPerson> getContacts(PXMeta meta)
        {
            List<ContactPerson> contactPersons = new List<ContactPerson>();
            List<Contact> contactList = meta.ContentInfo.ContactInfo;

            if (contactList is null) {
                Variable variable = meta.ContentVariable;
                if (variable is null) return contactPersons;
                Values values = variable.Values;
                if (values is null) return contactPersons;
                foreach (Value v in values) {
                    List<ContactPerson> cps = new List<ContactPerson>();
                    List<Contact> allContacts = v.ContentInfo.ContactInfo;
                    if (allContacts is null) continue;
                    foreach (Contact c in allContacts) {
                        ContactPerson cp;
                        if (contacts.ContainsKey(c.Email)) cp = contacts[c.Email];
                        else {
                            cp = new ContactPerson 
                            {
                                name = c.Forname + " " + c.Surname + ", " + c.OrganizationName, 
                                email = c.Email,
                                telephone = c.PhonePrefix + c.PhoneNo,
                                resource = settings.BaseUri + "contactperson/" + nextString()
                            };
                            contacts.Add(c.Email, cp);
                        }
                        cps.Add(cp);
                    }
                    contactPersons = contactPersons.Union(cps).ToList();
                }
            }

            else {
                foreach (Contact c in contactList) {
                    ContactPerson cp;
                    if (contacts.ContainsKey(c.Email)) cp = contacts[c.Email];
                    else {
                        cp = new ContactPerson 
                        {
                            name = c.Forname + " " + c.Surname + ", " + c.OrganizationName, 
                            email = c.Email,
                            telephone = c.PhonePrefix + c.PhoneNo,
                            resource = settings.BaseUri + "contactperson/" + nextString()
                        };
                        contacts.Add(c.Email, cp);
                    }
                    contactPersons.Add(cp);
                }
            }
            return contactPersons;
        }

        /// <summary>
        /// Get the latest modified date of a table
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Returns the latest modified date of a table</returns>
        private string getLastModified(PXMeta meta)
        {
            string modified = meta.ContentInfo.LastUpdated;
            if (modified is null)
            {
                Values values = meta.ContentVariable.Values;
                List<string> modifiedDates = new List<string>(values.Count());
                foreach (Value value in values)
                {
                    modifiedDates.Add(value.ContentInfo.LastUpdated);
                }
                modified = getLatestDate(modifiedDates);
            }
            return reformatDate(modified);
        }

        /// <summary> 
        // Get category from a path of menu items
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns a standard base URL and the theme code</returns>
        private string getCategory(List<PxMenuItem> path) { 
            if (path.Count < 2) {
                return null;
            }
            string category = path[1].ID.Selection;
            if (category is null) return null;
            if (!themeMapping.ContainsKey(category)) {
                return null;
            }
            return "http://publications.europa.eu/resource/authority/data-theme/" + themeMapping[category];
        }

        /// <summary>
        /// Get producer of table
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns>Organization with the producer info</returns>
        private Organization getProducer(PXMeta meta) {
            string name = meta.Source;
            if (organizations.ContainsKey(name)) return organizations[name];
            Organization org = new Organization {name = meta.Source, resource = settings.BaseUri + "organization/" + nextString()};
            organizations.Add(name, org);
            return org;
        }

        /// <summary>
        /// Get publisher from settings
        /// </summary>
        /// <returns>Publisher</returns>
        private Organization getPublisher() {
            Organization org;
            string publisherName = settings.PublisherName;
            if (organizations.ContainsKey(publisherName)) {
                org = organizations[publisherName];
            }
            else {
                org = new Organization {name = settings.PublisherName, resource = settings.BaseUri + "organization/" + nextString()};
                organizations.Add(publisherName, org);
            }
            return org;
        }

        /// <summary>
        /// Get keywords in a specific language
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="lang">Language to get keywords in</param>
        /// <returns>List of keywords</returns>
        private List<Keyword> getKeywords(List<PxMenuItem> path, string lang) {
            List<Keyword> keywords = new List<Keyword>();
            foreach (PxMenuItem menu in path.Skip(1)) {
                Keyword keyword;

                if (menuLangKeyword.ContainsKey((menu.ID.Selection, lang))) {
                    keyword = menuLangKeyword[(menu.ID.Selection, lang)];
                }
                else {
                    PxMenuItem menuInLang = getMenuInLanguage(menu,lang);
                    if (menuInLang is null) continue;
                    string text = menuInLang.Text;
                    keyword = new Keyword {text = text, lang = lang};
                    // Add keyword to dict
                    menuLangKeyword[(menu.ID.Selection, lang)] = keyword; 
                }
                keywords.Add(keyword); 
            }
            return keywords;
        }
        
        /// <summary>
        /// Gets each keyword from a specific table
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="langs">Languages to get keywords in (2 letter code)</param>
        /// <returns>List of keywords</returns>
        private List<Keyword> getKeywords(List<PxMenuItem> path, List<string> langs) {
            List<Keyword> keywords = new List<Keyword>();
            foreach (string lang in langs) {
                List<Keyword> keywordsInLang = getKeywords(path,lang);
                keywords.AddRange(keywordsInLang);
            }
            return keywords;
        }
        /// <summary>
        /// Get url for a table (Only relevant for cmnn)
        /// </summary>
        /// <param name="tableID">ID of table</param>
        /// <param name="lang">Language (2 letter code)</param>
        /// <returns></returns>
        private string getDatasetUrl(string tableID, string lang)
        {
            return settings.LandingPageUrl + lang + "/" + settings.DBid + "/" + tableID;
        }

        /// <summary>
        /// Get urls in multiple languages for a table
        /// </summary>
        /// <param name="tableID">ID of table</param>
        /// <param name="langs">Languages to get urls for</param>
        /// <returns>List of urls</returns>
        private List<string> getDatasetUrls(string tableID, List<string> langs) {
            List<string> urls = new List<string>();
            foreach(string lang in langs) {
                urls.Add(getDatasetUrl(tableID,lang));
            }
            return urls;
        }

        /// <summary>
        /// Get distribution url for a table
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="tableID">Table ID</param>
        /// <param name="lang">Language (2 letter code)</param>
        /// <returns></returns>
        private string getDistributionUrl(List<PxMenuItem> path, string tableID, string lang)
        {
            string url = settings.BaseApiUrl + lang + "/"+ settings.DBid + "/";
            foreach (PxMenuItem menu in path.Skip(1))
            {
                url += menu.ID.Selection + "/";
            }
            url += tableID;
            return url;
        }

        /// <summary>
        /// Get identifier for a table
        /// </summary>
        /// <param name="meta">Metadata of the table</param>
        /// <returns>Identifer of the table</returns>
        private string getIdentifier(PXMeta meta) {
            return Path.GetFileName(meta.MainTable.Replace(" ",""));
        }

        /// <summary>
        /// Get distributions, one for each language
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="tableID">Table ID</param>
        /// <param name="langs">Languages to generate distributions for (2 letter code)</param>
        /// <returns>List of distributions</returns>
        private List<Distribution> getDistributions(List<PxMenuItem> path, string tableID, List<string> langs)
        {
            List<Distribution> distrs = new List<Distribution>(langs.Count());
            foreach(string lang in langs) {
                Distribution distr = new Distribution();
                distr.title = "Data (" + lang + ")";
                distr.format = "application/json";
                distr.accessUrl = getDistributionUrl(path, tableID, lang);
                distr.language = convertLanguage(lang);
                distr.license = "http://creativecommons.org/publicdomain/zero/1.0/";
                distr.resource = settings.BaseUri + "distribution/" + nextString();

                distrs.Add(distr);
            }

            return distrs;
        }

        /// <summary>
        /// Get all data for a specific table
        /// </summary>
        /// <param name="meta">Metadata to fetch from</param>
        /// <param name="path">Path of the table</param>
        /// <returns>Dataset generated with the data from the table</returns>

        private Dataset getDataset(PXMeta meta, List<PxMenuItem> path) {
            Dataset dataset = new Dataset();

            dataset.publisher = getPublisher();
            dataset.identifier = getIdentifier(meta);
            dataset.modified = getLastModified(meta);
            dataset.updateFrequency = getUpdateFrequency(meta);

            List<string> langs = getLanguages(meta).Intersect(settings.Languages).ToList();
            dataset.languages = langs;
            dataset.languageURIs = convertLanguages(langs);

            dataset.descriptions =  getDescriptions(meta, langs);
            dataset.titles = getTitles(meta, langs);
        
            dataset.contactPersons = getContacts(meta);
            dataset.category = getCategory(path);
            dataset.producer = getProducer(meta);

            dataset.keywords = getKeywords(path, langs);
            dataset.distributions = getDistributions(path, dataset.identifier, langs);

            dataset.resource = settings.BaseUri + "dataset/" + nextString();
            dataset.urls = getDatasetUrls(dataset.identifier, langs);

            return dataset;
        }

        /// <summary>
        /// Get menu in a specific language
        /// </summary>
        /// <param name="menuItem">Menu</param>
        /// <param name="lang">Language (2 letter code)</param>
        /// <returns>Menu with information in specified language</returns>
        private PxMenuItem getMenuInLanguage(PxMenuItem menuItem, string lang) {
            string nodeID = menuItem.ID.Selection;
            string menuID = menuItem.ID.Menu;
            try {
                return settings.Fetcher.GetBaseItem(nodeID, menuID, lang, settings.DBid) as PxMenuItem;
            }
            catch (PCAxis.Menu.Exceptions.InvalidMenuFromXMLException) {
                return null;
            }
        }
        /// <summary>
        /// Recursively go through all items
        /// </summary>
        /// <param name="item">Root item to recurse from</param>
        /// <param name="path">Path of the current item</param>
        /// <param name="d">List of datasets to add found datasets to</param>
        /// <param name="max">Maximumu number of datasets fetched</param>
        private void addRecursive(Item item, List<PxMenuItem> path, List<Dataset> d, int max)
        {

            if (item is PxMenuItem)
            {
                var menu = item as PxMenuItem;
                if (menu.HasSubItems)
                {
                    foreach (var subItem in menu.SubItems)
                    {
                        List<PxMenuItem> newPath = new List<PxMenuItem>(path);
                        newPath.Add(menu);
                        addRecursive(subItem, newPath, d, max);
                    }
                }
            }
            else if (item is TableLink)
            {
                int count = d.Count;
                //Console.WriteLine(count);
                if (max > 0 && count >= max)
                {
                    return;
                }

                var table = item as TableLink;
                try
                {
                    IPXModelBuilder builder = settings.Fetcher.GetBuilder(table.ID.Selection);
                    builder.ReadAllLanguages = true;
                    builder.SetPreferredLanguage(settings.MainLanguage);
                    builder.BuildForSelection();
                
                    Dataset dataset = getDataset(builder.Model.Meta, path);
                    d.Add(dataset);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Get datasets from a database specified in settings
        /// </summary>
        /// <param name="n">Maximum number of fetched datasets</param>
        /// <returns>List of datasets</returns>
        public List<Dataset> GetDatasets(int n)
        {
            var datasets = new List<Dataset>();
            var path = new List<PxMenuItem>();
            Item baseItem = settings.Fetcher.GetBaseItem("","",settings.MainLanguage,settings.DBid);

            addRecursive(baseItem, path, datasets, n);
            return datasets;
        }

        /// <summary>
        /// Generate catalog from loaded settings
        /// </summary>
        /// <param name="numberOfTables">Maximum number of datasets to be fetched</param>
        /// <returns>Generated catalog</returns>
        public Catalog GetCatalog(int numberOfTables)
        {
            Catalog c = new Catalog();
            c.titles = settings.CatalogTitles;
            c.descriptions = settings.CatalogDescriptions;
            c.publisher = getPublisher();
            c.license = settings.License;
            c.datasets = GetDatasets(numberOfTables);
            c.languages = convertLanguages(settings.Languages);
            return c;
        }
    }
}