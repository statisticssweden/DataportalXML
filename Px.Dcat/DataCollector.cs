using PCAxis.Menu;
using PCAxis.Paxiom;
using Px.Dcat.DataClasses;
using Px.Dcat.Helpers;
using Px.Dcat.Interfaces;
using Px.Dcat.Fetchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Px.Dcat
{
    public class DataCollector
    {
        private const string PCAXIS_DATE_FORMAT = PXConstant.PXDATEFORMAT;
        private const string DCAT_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";

        private int _hashNum;

        private Organization _publisher;
        private Dictionary<string, Organization> _organizations = new Dictionary<string, Organization>(); // (lang, source) -> organization
        private Dictionary<(string, string), Keyword> _keywords = new Dictionary<(string, string), Keyword>(); // (menuID, language) -> Array of keywords
        private Dictionary<string, ContactPerson> _contacts = new Dictionary<string, ContactPerson>(); // email to contactPerson
        private DcatSettings _settings;
        private IFetcher _fetcher;

        private Dictionary<string, string> _themeMapping; // Mapping from PcAxis categories to DCAT standard themes https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcat_theme
        private Dictionary<string, string> _organizationMapping;
        private Dictionary<string, string> _languageMapping // Mapping from ISO 639-1 to ISO 639-3 (2 letters to 3) used for DCAT languages https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_language
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
        // Mapping from PcAxis TimeScaleType to DCAT standard https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_accrualPeriodicity
        private Dictionary<TimeScaleType, string> _timeScaleMapping
            = new Dictionary<TimeScaleType, string>
        {
            { TimeScaleType.NotSet, "OTHER"},
            { TimeScaleType.Annual, "ANNUAL"},
            { TimeScaleType.Halfyear, "ANNUAL_2"},
            { TimeScaleType.Monthly, "MONTHLY"},
            { TimeScaleType.Quartely, "QUARTERLY"},
            { TimeScaleType.Weekly, "ANNUAL"},
        };


        // Get all unique Organizations
        public List<Organization> UniqueOrgs()
        {
            return _organizations.Values.Distinct().ToList();
        }

        // Get all unique Contacts
        public List<ContactPerson> UniqueContacts()
        {
            return _contacts.Values.ToList();
        }

        /// <summary>
        /// Load _settings
        /// </summary>
        /// <param name="dcatSettings">Settings to load</param>
        private void loadSettings(DcatSettings dcatSettings)
        {
            _settings = dcatSettings;
            _themeMapping = JsonReader.ReadDictionary(_settings.ThemeMapping);
            _organizationMapping = JsonReader.ReadDictionary(_settings.OrganizationMapping);

            if (_settings.DatabaseType == DatabaseType.CNMM)
            {
                _fetcher = new CNMMFetcher();
            }
            else
            {
                string pxBasepoint = Path.Combine(_settings.DatabaseId, @"..\..\");
                _fetcher = new PXFetcher(pxBasepoint);
            }
        }

        /// <summary>
        /// Get next unique integer
        /// </summary>
        /// <returns>Integer</returns>
        protected int nextNum()
        {
            return ++_hashNum;
        }

        /// <summary>
        /// Get next unique string
        /// </summary>
        /// <returns>String</returns>
        protected string nextString()
        {
            return (++_hashNum).ToString();
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

            foreach (string lang in langs)
            {
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
        private List<string> getTitles(PXMeta meta, List<string> langs)
        {
            List<string> titles = new List<string>(langs.Count());
            foreach (string lang in langs)
            {
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
            Predicate<Variable> isTime = (v) => v.IsTime;
            Variable timeVar = meta.Variables.Find(isTime);
            if (timeVar is null)
            {
                return null;
            }
            TimeScaleType timeScale = timeVar.TimeScale;
            string baseURI = "http://publications.europa.eu/Resource/authority/frequency/";
            return baseURI + _timeScaleMapping[timeScale];
        }

        /// <summary>
        /// Convert language from 2 letter code to Dcat-ap standard
        /// </summary>
        /// <param name="str">Language to convert</param>
        /// <returns>The converted language, url</returns>
        private string convertLanguage(string str)
        {
            string lang = _languageMapping[str];
            return "http://publications.europa.eu/Resource/authority/Language/" + lang.ToUpper();
        }

        /// <summary>
        /// Converts given languages from 2 letter code to Dcat-ap standard
        /// </summary>
        /// <param name="languages">List of languages to convert</param>
        /// <returns>Returns each URL for every language available</returns>
        private List<string> convertLanguages(List<string> languages)
        {
            List<string> converted = new List<string>(languages.Count());
            foreach (string lang in languages)
            {
                converted.Add(convertLanguage(lang));
            }
            return converted;
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
            foreach (string date in dates)
            {
                dateTimes.Add(DateTime.ParseExact(date, PCAXIS_DATE_FORMAT, null));
            }
            int maxIndex = dateTimes.IndexOf(dateTimes.Max());
            return dates[maxIndex];
        }

        /// <summary>
        /// Get _contacts of a table
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns>List of _contacts</returns>
        public List<ContactPerson> getContacts(PXMeta meta)
        {
            List<ContactPerson> contactPersons = new List<ContactPerson>();
            List<Contact> contactList = meta.ContentInfo.ContactInfo;

            if (contactList is null)
            {
                Variable variable = meta.ContentVariable;
                if (variable is null) return contactPersons;
                Values values = variable.Values;
                if (values is null) return contactPersons;
                foreach (Value v in values)
                {
                    List<ContactPerson> cps = new List<ContactPerson>();
                    List<Contact> allContacts = v.ContentInfo.ContactInfo;
                    if (allContacts is null) continue;
                    foreach (Contact c in allContacts)
                    {
                        ContactPerson cp;
                        if (_contacts.ContainsKey(c.Email)) cp = _contacts[c.Email];
                        else
                        {
                            cp = new ContactPerson
                            {
                                Name = c.Forname + " " + c.Surname + ", " + c.OrganizationName,
                                Email = c.Email,
                                Phone = c.PhonePrefix + c.PhoneNo,
                                Resource = _settings.BaseUri + "contactperson/" + nextString()
                            };
                            _contacts.Add(c.Email, cp);
                        }
                        cps.Add(cp);
                    }
                    contactPersons = contactPersons.Union(cps).ToList();
                }
            }

            else
            {
                foreach (Contact c in contactList)
                {
                    ContactPerson cp;
                    if (_contacts.ContainsKey(c.Email)) cp = _contacts[c.Email];
                    else
                    {
                        cp = new ContactPerson
                        {
                            Name = c.Forname + " " + c.Surname + ", " + c.OrganizationName,
                            Email = c.Email,
                            Phone = c.PhonePrefix + c.PhoneNo,
                            Resource = _settings.BaseUri + "contactperson/" + nextString()
                        };
                        _contacts.Add(c.Email, cp);
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
        private string getCategory(List<PxMenuItem> path)
        {
            if (path.Count < 2)
            {
                return null;
            }
            string category = path[1].ID.Selection;
            if (category is null) return null;
            if (!_themeMapping.ContainsKey(category))
            {
                return null;
            }
            return "http://publications.europa.eu/Resource/authority/data-theme/" + _themeMapping[category];
        }

        /// <summary>
        /// Get producer of table
        /// </summary>
        /// <param name="meta">Metadata of table</param>
        /// <returns>Organization with the producer info</returns>
        private Organization getProducer(PXMeta meta, List<string> langs)
        {
            HashSet<(string, string)> names = new HashSet<(string, string)>();
            List<Organization> matchingOrgs = new List<Organization>();

            foreach (string lang in langs)
            {
                meta.SetLanguage(lang);
                string name = meta.Source;
                if (_organizations.ContainsKey(name))
                {
                    matchingOrgs.Add(_organizations[name]);
                }
                names.Add((lang, name));
            }

            Organization newOrg = new Organization();
            if (matchingOrgs.Count > 0)
            {
                foreach (Organization org in matchingOrgs)
                {
                    names.UnionWith(org.Names);
                }

                newOrg.Names = names;
                newOrg.Resource = _settings.BaseUri + "organization/" + nextString();

                foreach (string name in names.Select(x => x.Item2).Distinct())
                {
                    _organizations[name] = newOrg;
                }

            }
            else
            {
                newOrg.Names = names;
                newOrg.Resource = _settings.BaseUri + "organization/" + nextString();

                // Add a reference to the organization for each language
                foreach (string name in names.Select(x => x.Item2).Distinct())
                {
                    _organizations.Add(name, newOrg);
                }
            }

            return newOrg;
        }

        /// <summary>
        /// Get publisher from _settings
        /// </summary>
        /// <returns>Publisher</returns>
        private Organization getPublisher()
        {
            string name = _settings.PublisherName;
            Organization org;
            if (!_organizations.TryGetValue(name, out org))
            {
                HashSet<(string, string)> names = new HashSet<(string, string)>();
                names.Add((null, name));
                org = new Organization { Names = names, Resource = _settings.BaseUri + "organization/" + nextString() };
                _organizations.Add(name, org);
            }
            return org;
        }

        /// <summary>
        /// Get keywords in a specific language
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="lang">Language to get keywords in</param>
        /// <returns>List of keywords</returns>
        private List<Keyword> getKeywords(List<PxMenuItem> path, string lang)
        {
            List<Keyword> keywords = new List<Keyword>();
            foreach (PxMenuItem menu in path.Skip(1))
            {
                Keyword keyword;

                if (_keywords.ContainsKey((menu.ID.Selection, lang)))
                {
                    keyword = _keywords[(menu.ID.Selection, lang)];
                }
                else
                {
                    PxMenuItem menuInLang = getMenuInLanguage(menu, lang);
                    if (menuInLang is null) continue;
                    string text = menuInLang.Text;
                    keyword = new Keyword { Text = text, Language = lang };
                    // Add keyword to dict
                    _keywords[(menu.ID.Selection, lang)] = keyword;
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
        private List<Keyword> getKeywords(List<PxMenuItem> path, List<string> langs)
        {
            List<Keyword> keywords = new List<Keyword>();
            foreach (string lang in langs)
            {
                List<Keyword> keywordsInLang = getKeywords(path, lang);
                keywords.AddRange(keywordsInLang);
            }
            return keywords;
        }
        /// <summary>
        /// Get url for a table
        /// </summary>
        /// <param name="tableID">ID of table</param>
        /// <param name="lang">Language (2 letter code)</param>
        /// <returns></returns>
        private string getDatasetUrl(string tableSelection, PXMeta meta, string lang)
        {
            string url;
            if (_settings.DatabaseType == DatabaseType.CNMM) {
                url = Path.Combine(_settings.LandingPageUrl, lang, _settings.DatabaseId, meta.MainTable.Replace(" ", ""));
            }
            else {
                string fileName = Path.GetFileName(tableSelection);
                url = Path.Combine(_settings.LandingPageUrl, lang, getDatabaseName(), fileName);
            }
            url = url.Replace("\\", "/");
            return url;
        }

        /// <summary>
        /// Get urls in multiple languages for a table
        /// </summary>
        /// <param name="tableID">ID of table</param>
        /// <param name="langs">Languages to get urls for</param>
        /// <returns>List of urls</returns>
        private List<string> getDatasetUrls(string selection, PXMeta meta, List<string> langs)
        {
            List<string> urls = new List<string>();
            foreach (string lang in langs)
            {
                urls.Add(getDatasetUrl(selection, meta, lang));
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
        private string getDistributionUrl(string tableSelection, List<PxMenuItem> path, PXMeta meta, string lang)
        {
            string url = Path.Combine(_settings.BaseApiUrl, lang, getDatabaseName());
            foreach (PxMenuItem menu in path.Skip(1))
            {
                string selection = menu.ID.Selection;
                string leaf = selection.Split(new char[] { '/', '\\' }).Last();
                url = Path.Combine(url, leaf);
            }
            if (_settings.DatabaseType == DatabaseType.PX)
            {
                string fileName = Path.GetFileName(tableSelection);
                url = Path.Combine(url, fileName);
            }
            else
            {
                url = Path.Combine(url, meta.MainTable.Replace(" ", ""));
            }
            url = url.Replace("\\", "/");
            return url;
        }

        /// <summary>
        /// Get identifier for a table
        /// </summary>
        /// <param name="meta">Metadata of the table</param>
        /// <returns>Identifer of the table</returns>
        private string getIdentifier(string selection, PXMeta meta)
        {
            if (_settings.DatabaseType == DatabaseType.PX) return Path.GetFileName(selection);
            else return meta.TableID;
        }

        /// <summary>
        /// Get distributions, one for each language
        /// </summary>
        /// <param name="path">Path of table</param>
        /// <param name="tableID">Table ID</param>
        /// <param name="langs">Languages to generate distributions for (2 letter code)</param>
        /// <returns>List of distributions</returns>
        private List<Distribution> getDistributions(string selection, List<PxMenuItem> path, PXMeta meta, List<string> langs)
        {
            List<Distribution> distrs = new List<Distribution>(langs.Count());
            foreach (string lang in langs)
            {
                Distribution distr = new Distribution();
                distr.Title = "Metadata (" + lang + ")";
                distr.Format = "application/json";
                distr.AccessUrl = getDistributionUrl(selection, path, meta, lang);
                distr.Language = convertLanguage(lang);
                distr.License = "http://creativecommons.org/publicdomain/zero/1.0/";
                distr.Resource = _settings.BaseUri + "distribution/" + nextString();

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

        private Dataset getDataset(string selection, PXMeta meta, List<PxMenuItem> path)
        {
            Dataset dataset = new Dataset();

            dataset.Identifier = getIdentifier(selection, meta);
            dataset.Modified = getLastModified(meta);
            dataset.UpdateFrequency = getUpdateFrequency(meta);

            List<string> langs = getLanguages(meta).Intersect(_settings.Languages).ToList();
            dataset.Languages = langs;
            dataset.LanguageURIs = convertLanguages(langs);

            dataset.Descriptions = getDescriptions(meta, langs);
            dataset.Titles = getTitles(meta, langs);

            dataset.ContactPersons = getContacts(meta);
            dataset.Category = getCategory(path);

            dataset.Keywords = getKeywords(path, langs);
            dataset.Distributions = getDistributions(selection, path, meta, langs);

            dataset.Resource = _settings.BaseUri + "dataset/" + dataset.Identifier;
            dataset.Urls = getDatasetUrls(selection, meta, langs);

            dataset.Sources = getSources(meta, langs);

            getProducer(meta, langs); // Wait until all organizations are created before assigning producer

            return dataset;
        }

        private List<string> getSources(PXMeta meta, List<string> langs)
        {
            List<string> sources = new List<string>();
            foreach (string lang in langs)
            {
                meta.SetLanguage(lang);
                sources.Add(meta.Source);
            }
            return sources.Distinct().ToList();
        }

        /// <summary>
        /// Get menu in a specific language
        /// </summary>
        /// <param name="menuItem">Menu</param>
        /// <param name="lang">Language (2 letter code)</param>
        /// <returns>Menu with information in specified language</returns>
        private PxMenuItem getMenuInLanguage(PxMenuItem menuItem, string lang)
        {
            string nodeID = menuItem.ID.Selection;
            string menuID = menuItem.ID.Menu;
            try
            {
                return _fetcher.GetBaseItem(nodeID, menuID, lang, _settings.DatabaseId) as PxMenuItem;
            }
            catch (PCAxis.Menu.Exceptions.InvalidMenuFromXMLException)
            {
                return null;
            }
        }

        /// <summary>
        /// Recursively go through all items
        /// </summary>
        /// <param name="item">Root item to recurse from</param>
        /// <param name="path">Path of the current item</param>
        /// <param name="d">List of datasets to add found datasets to</param>
        private void addRecursive(Item item, List<PxMenuItem> path, List<Dataset> d)
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
                        addRecursive(subItem, newPath, d);
                    }
                }
            }
            else if (item is TableLink)
            {
                var table = item as TableLink;
                try
                {
                    string selection = table.ID.Selection;
                    IPXModelBuilder builder = _fetcher.GetBuilder(selection);
                    builder.ReadAllLanguages = true;
                    builder.SetPreferredLanguage(_settings.MainLanguage);
                    builder.BuildForSelection();

                    Dataset dataset = getDataset(selection, builder.Model.Meta, path);
                    d.Add(dataset);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.ToString());
                }
            }
        }

        /// <summary>
        /// Get datasets from a database specified in _settings
        /// </summary>
        /// <returns>List of datasets</returns>
        private List<Dataset> getDatasets()
        {
            var datasets = new List<Dataset>();
            var path = new List<PxMenuItem>();

            Item baseItem = _fetcher.GetBaseItem("", "", _settings.MainLanguage, _settings.DatabaseId);

            addRecursive(baseItem, path, datasets);

            _publisher = getPublisher();
            foreach (Dataset d in datasets)
            {
                d.Publisher = _publisher;
            }
            return datasets;
        }

        private string getDatabaseName()
        {
            if (_settings.DatabaseType == DatabaseType.CNMM) return _settings.DatabaseId;

            var directories = _settings.DatabaseId.Split(Path.DirectorySeparatorChar);
            string databaseName = directories[directories.Length - 2];
            return databaseName;
        }
        /// <summary>
        /// Generate catalog from loaded _settings
        /// </summary>
        /// <returns>Generated catalog</returns>
        private Catalog getCatalog()
        {
            Catalog c = new Catalog();
            c.Titles = _settings.CatalogTitles;
            c.Descriptions = _settings.CatalogDescriptions;
            c.License = _settings.License;
            c.Datasets = getDatasets();
            c.Languages = convertLanguages(_settings.Languages);
            setProducers(c.Datasets);
            c.Publisher = _publisher;
            setOrganizationResources();
            return c;
        }

        private void setProducers(List<Dataset> datasets)
        {
            foreach (Dataset d in datasets)
            {
                string source = d.Sources.First();
                d.Producer = _organizations[source];
            }
        }

        private void setOrganizationResources()
        {
            foreach (string source in _organizations.Keys)
            {
                if (_organizationMapping.ContainsKey(source))
                {
                    _organizations[source].Resource = _organizationMapping[source];
                }
            }
        }

        /// <summary>
        /// Get data from database specified in dcatSettings
        /// </summary>
        /// <param name="dcatSettings">Settings used to get data</param>
        /// <returns>Generated catalog</returns>
        public Catalog Get(DcatSettings dcatSettings)
        {
            loadSettings(dcatSettings);
            Catalog c = getCatalog();
            return c;
        }
    }
}