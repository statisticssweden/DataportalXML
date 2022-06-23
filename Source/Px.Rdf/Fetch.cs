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
    public static class Fetch
    {
        private const string PCAXIS_DATE_FORMAT = "yyyyMMdd HH:mm";
        private const string DCAT_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private static int hashNum;

        private static Dictionary<string, Organization> organizations = new Dictionary<string, Organization>(); // name to organization
        private static Dictionary<string, ContactPerson> contacts = new Dictionary<string, ContactPerson>(); // email to contactPerson

        private static Dictionary<(string, string), Keyword> menuLangKeyword = new Dictionary<(string, string), Keyword>(); // (menuID, language) -> Array of keywords
        
        private static RdfSettings settings;

        public static void LoadSettings(RdfSettings rdfSettings) {
            settings = rdfSettings;
        }
        // Get all unique Organizations
        public static List<Organization> UniqueOrgs() {
            return organizations.Values.ToList();
        }

        // Get all unique Contacts
        public static List<ContactPerson> UniqueContacts() {
            return contacts.Values.ToList();
        }

        // Mapping from PcAxis TimeScaleType to DCAT standard https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_accrualPeriodicity
        private static Dictionary<TimeScaleType, string> timeScaleToUpdateFreq
            = new Dictionary<TimeScaleType, string>
        {
            { TimeScaleType.NotSet, "OTHER"},
            { TimeScaleType.Annual, "ANNUAL"},
            { TimeScaleType.Halfyear, "ANNUAL_2"},
            { TimeScaleType.Monthly, "MONTHLY"},
            { TimeScaleType.Quartely, "QUARTERLY"},
            { TimeScaleType.Weekly, "ANNUAL"},
        };

        // Mapping from PcAxis categories to DCAT standard themes https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcat_theme
        private static Dictionary<string, string> themeMapping
            = new Dictionary<string, string>
        { // Dictionary for all themes by mapping them accordingly to DCAT
            { "Arbetsmarknad", "SOCI"},
            { "Befolkning", "SOCI"},
            { "Boende, byggande och bebyggelse", "SOCI"},
            { "Demokrati", "JUST"},
            { "Energi", "ENER"},
            { "Finansmarknad", "ECON"},
            { "Handel med varor och tjänster", "ECON"},
            { "Hushållens ekonomi", "ECON"},
            { "Hälso- och sjukvård", "HEAL"},
            { "Jord- och skogsbruk, fiske", "AGRI"},
            { "Kultur och fritid", "EDUC"},
            { "Levnadsförhållanden", "SOCI"},
            { "Miljö", "ENVI"},
            { "Nationalräkenskaper", "ECON"},
            { "Näringsverksamhet", "ECON"},
            { "Offentlig ekonomi", "GOVE"},
            { "Priser och konsumtion", "ECON"},
            { "Socialtjänst", "SOCI"},
            { "Transporter och kommunikationer", "TRAN"},
            { "Utbildning och forskning", "EDUC"},
            { "Ämnesövergripande statistik", "SOCI"},
            {"",""},
            {null, ""}
        };

        // Mapping from ISO 639-1 to ISO 639-3 (2 letters to 3) used for DCAT languages https://docs.dataportal.se/dcat/sv/#dcat_Dataset-dcterms_language
        private static Dictionary<string, string> languageToDcatLang
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

        private static int nextNum() {
            return ++hashNum;
        }
        private static string nextString() {
            return (++hashNum).ToString();
        }

        // Gets description for each language. First note, if mandatory
        private static List<string> getDescriptions(PXMeta meta, List<string> langs)
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

        // Gets the title for each language
        private static List<string> getTitles(PXMeta meta, List<string> langs) {
            List<string> titles = new List<string>(langs.Count());
            foreach(string lang in langs) {
                meta.SetLanguage(lang);
                titles.Add(meta.Title);
            }
            return titles;
        }

        // Gets update frequency
        private static string getUpdateFrequency(PXMeta meta)
        {
            Predicate<Variable> isTime = (Variable v) => v.IsTime;
            TimeScaleType timeScale = meta.Variables.Find(isTime).TimeScale;
            string baseURI = "http://publications.europa.eu/resource/authority/frequency/";
            return baseURI + timeScaleToUpdateFreq[timeScale];
        }

        // Returns the language URL for each description
        private static string convertLanguage(string str)
        {
            string lang = languageToDcatLang[str];
            return "http://publications.europa.eu/resource/authority/language/" + lang.ToUpper();
        }

        // Function returning a list of strings where it's either one or more languages
        private static List<string> getLanguages(PXMeta meta)
        {
            string[] allLangs = meta.GetAllLanguages();
            if (allLangs is null)
            {
                return new List<string> { meta.Language };
            }
            return new List<string>(allLangs);
        }
        // Converts and returns each url for every language available
        private static List<string> convertLanguages(List<string> languages) {
            List<string> converted = new List<string>(languages.Count());
            foreach(string lang in languages)
            {
                converted.Add(convertLanguage(lang));
            }
            return converted;
        }

        // Get all contacts for a dataset, check all ContentVariable.values.contentInfo.ContactInfo and get unique
        private static List<ContactPerson> getContacts(PXMeta meta)
        {
            List<ContactPerson> contactPersons = new List<ContactPerson>();
            foreach (Value v in meta.ContentVariable.Values) {
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
            return contactPersons;
        }
        
        
        // Convert from pcAxis date format to xsd:dateTime format
        private static string reformatDate(string s)
        {
            DateTime date = DateTime.ParseExact(s, PCAXIS_DATE_FORMAT, null);
            string formatted = date.ToString(DCAT_DATE_FORMAT);
            return formatted;
        }

        //  Returns the latest date from a list of dates (in PcAxis format)
        private static string getLatestDate(List<string> dates) // format yyyyMMdd HH:mm
        {
            List<DateTime> dateTimes = new List<DateTime>(dates.Count());
            foreach (string date in dates) {
                dateTimes.Add(DateTime.ParseExact(date, PCAXIS_DATE_FORMAT, null));
            }
            int maxIndex = dateTimes.IndexOf(dateTimes.Max());
            return dates[maxIndex];
        }

        // Return the latest modiefied date of a dataset, check all ContentVariables modification dates and return latest
        private static string getLastModified(PXMeta meta)
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

        // Get category from a path of menu items
        private static string getCategory(List<PxMenuItem> path) { 
            if (path.Count < 2) {
                return "";
            }
            return "http://publications.europa.eu/resource/authority/data-theme/" + themeMapping[path[1].Text];
        }


        // Producer of dataset, Meta.Source
        private static Organization getProducer(PXMeta meta) {
            string name = meta.Source;
            if (organizations.ContainsKey(name)) return organizations[name];
            Organization org = new Organization {name = meta.Source, resource = settings.BaseUri + "organization/" + nextString()};
            organizations.Add(name, org);
            return org;
        }

        private static Organization getPublisher() {
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

        // Get keywords for a specific language. Keywords are the menu titles
        private static List<Keyword> getKeywords(List<PxMenuItem> path, string lang) {
            List<Keyword> keywords = new List<Keyword>();
            foreach (PxMenuItem menu in path.Skip(1)) {
                Keyword keyword;

                if (menuLangKeyword.ContainsKey((menu.ID.Selection, lang))) {
                    keyword = menuLangKeyword[(menu.ID.Selection, lang)];
                }
                else {
                    PxMenuItem menuInLang = getMenuInLanguage(menu,lang);
                    string text = menuInLang.Text;
                    keyword = new Keyword {text = text, lang = lang};
                }

                keywords.Add(keyword); 
                // Add keyword to dict
                menuLangKeyword[(menu.ID.Selection, lang)] = keyword;
            }
            return keywords;
        }

        // Gets each keyword from a specific 
        private static List<Keyword> getKeywords(List<PxMenuItem> path, List<string> langs) {
            List<Keyword> keywords = new List<Keyword>();
            foreach (string lang in langs) {
                List<Keyword> keywordsInLang = getKeywords(path,lang);
                keywords.AddRange(keywordsInLang);
            }
            return keywords;
        }

        // Gets the distrubution url from a path, title and language 
        private static string getDistributionUrl(List<PxMenuItem> path, string tableID, string lang)
        {
            string url = settings.BaseApiUrl + lang + "/ssd/";
            foreach (PxMenuItem menu in path.Skip(1))
            {
                url += menu.ID.Selection + "/";
            }
            url += tableID;
            return url;
        }

        // Get distributions, one for each language
        private static List<Distribution> getDistributions(List<PxMenuItem> path, string tableID, List<string> langs)
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
        // Gets data for each dataset
        private static Dataset getDataset(PXMeta meta, List<PxMenuItem> path) {
            Dataset dataset = new Dataset();

            dataset.publisher = getPublisher();
            dataset.identifier = meta.MainTable;
            dataset.modified = getLastModified(meta);
            dataset.updateFrequency = getUpdateFrequency(meta);

            List<string> langs = getLanguages(meta);
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

            return dataset;
        }

        // Get a menu in a specific language
        private static PxMenuItem getMenuInLanguage(PxMenuItem menuItem, string lang) {
            string nodeID = menuItem.ID.Selection;
            string menuID = menuItem.ID.Menu;
            return getBaseItem(nodeID, menuID) as PxMenuItem;
        }

        // Recursively go thorugh all items and add the leaf nodes (datasets) to the list d, max is the maximum amount of datasets collected
        private static void addRecursive(Item item, List<PxMenuItem> path, List<Dataset> d, int max)
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
                var selection = "";
                try
                {
                    IPXModelBuilder builder;
                    if (settings.DBtype == DBType.SQL)
                    {
                        builder = new PXSQLBuilder();
                        selection = table.ID.Selection;
                    }
                    else 
                    { 
                        builder = new PXFileBuilder();
                        selection = Path.Combine(@"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\", table.ID.Selection);
                    }
                    
                    builder.SetPath(selection);
                    builder.ReadAllLanguages = true;
                    builder.SetPreferredLanguage(settings.PreferredLanguage);
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

        public static Item getBaseItem() {
            return getBaseItem("","");
        }

        public static Item getBaseItem(string nodeID, string menuID) {
            if (settings.DBtype == DBType.SQL) {
                return getBaseItemSQL(nodeID, menuID);
            }
            else if (settings.DBtype == DBType.PcAxisFile) {
                return getBaseItemPcAxis(nodeID, menuID);
            }
            else {
                throw new Exception("Invalid database type selected");
            }
        }
        public static Item getBaseItemSQL(string nodeID, string menuID) {
            string dbLang = settings.DBLang;
            string dbid = settings.DBid;
            TableLink tblFix = null;

            DatamodelMenu menu = ConfigDatamodelMenu.Create(
            dbLang,
            PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[dbid],
            m =>
            {
                m.NumberOfLevels = 5;
                m.RootSelection = nodeID == "" ? new ItemSelection() : new ItemSelection(menuID, nodeID);
                m.AlterItemBeforeStorage = item =>
                {
                    if (item is TableLink)
                    {
                        TableLink tbl = (TableLink)item;

                        if (string.Compare(tbl.ID.Selection, nodeID, true) == 0)
                        {
                            tblFix = tbl;
                        }
                        if (tbl.StartTime == tbl.EndTime)
                        {
                            tbl.Text = tbl.Text + " " + tbl.StartTime;
                        }
                        else
                        {
                            tbl.Text = tbl.Text + " " + tbl.StartTime + " - " + tbl.EndTime;
                        }

                        if (tbl.Published.HasValue)
                        {
                            tbl.SetAttribute("modified", tbl.Published.Value.ToShortDateString());
                        }
                    }
                    if (String.IsNullOrEmpty(item.SortCode))
                    {
                        item.SortCode = item.Text;
                    }
                };
            });
            return menu.CurrentItem;
        }
        private static Item getBaseItemPcAxis(string nodeID, string menuID) { 
            

                XmlMenu menu = new XmlMenu(XDocument.Load(settings.DBid), settings.DBLang,
                    m =>
                    {
                        m.Restriction = item =>
                        {
                            return true;
                        };
                    });

                //ItemSelection cid = PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.PX).GetSelection(nodeID);
                ItemSelection cid = new ItemSelection(menuID, nodeID);
                menu.SetCurrentItemBySelection(cid.Menu, cid.Selection);
                return menu.CurrentItem;
            
   
            
        }

        // Gets datasets, n is maximum amount
        public static List<Dataset> GetDatasets(int n)
        {
            

            var datasets = new List<Dataset>();
            var path = new List<PxMenuItem>();
            Item baseItem = getBaseItem();

            addRecursive(baseItem, path, datasets, n);
            return datasets;
        }

        // Generates a catalog, numberOfTables is maximum amount of datasets fetched
        public static Catalog GetCatalog(int numberOfTables)
        {
            Catalog c = new Catalog();
            c.title = settings.CatalogTitle;
            c.description = settings.CatalogDescription;
            c.publisher = getPublisher();
            c.license = "http://creativecommons.org/publicdomain/zero/1.0/";
            c.datasets = GetDatasets(numberOfTables);
            c.language = convertLanguage(settings.CatalogLanguage);
            return c;
        }
    }
}