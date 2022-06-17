using System;
using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;

using System.Collections.Generic;
using System.Linq;


using Data;
namespace Px.Rdf
{
    public static class Fetch
    {
        private const string PCAXIS_DATE_FORMAT = "yyyyMMdd HH:mm";
        private const string DCAT_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
        private static int hashNum;

        private static Dictionary<string, Organization> organizations = new Dictionary<string, Organization>(); // name to organization
        private static Dictionary<string, ContactPerson> contacts = new Dictionary<string, ContactPerson>(); // email to contactPerson

        public static Organization[] UniqueOrgs() {
            return organizations.Values.ToArray();
        }
        public static ContactPerson[] UniqueContacts() {
            return contacts.Values.ToArray();
        }
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

        private static Dictionary<string, string> themeMapping
            = new Dictionary<string, string>
        {
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
            {"",""}
        };

        private static Dictionary<string, string> languageToDcatLang
            = new Dictionary<string, string>
        { // Iso 639-1  ->  Iso 639-3 mappings
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

        private static string[] getDescriptions(PXMeta meta, string[] langs)
        {   
            int n = langs.Count();
            string[] descriptions = new string[n];

            for(int i=0; i<n; i++){
                string lang = langs[i];
                string desc = "-";
                meta.SetLanguage(lang);
                Notes notes = meta.Notes;
                if (notes.Count > 0 && notes[0].Mandantory)
                {
                    desc = notes[0].Text;
                }
                descriptions[i]=desc;
            }
            return descriptions;
        }

        private static string[] getTitles(PXMeta meta, string[] langs) {
            int n = langs.Count();
            string[] titles = new string[n];
            for(int i=0; i<n; i++){
                string lang = langs[i];
                meta.SetLanguage(lang);
                titles[i] = meta.Title;
            }
            return titles;
        }

        private static string getUpdateFrequency(PXMeta meta)
        {
            Predicate<Variable> isTime = (Variable v) => v.IsTime;
            TimeScaleType timeScale = meta.Variables.Find(isTime).TimeScale;
            string baseURI = "http://publications.europa.eu/resource/authority/frequency/";
            return baseURI + timeScaleToUpdateFreq[timeScale];
        }

        private static string convertLanguage(string str)
        {
            string lang = languageToDcatLang[str];
            return "http://publications.europa.eu/resource/authority/language/" + lang.ToUpper();
        }

        private static string[] getLanguages(PXMeta meta)
        {
            string[] langs = meta.GetAllLanguages();
            if (langs is null)
            {
                langs = new string[] { meta.Language };
            }
            return langs;
        }

        private static string[] convertLanguages(string[] languages) {
            string[] lang = new string[languages.Count()];
            for (int i = 0; i < lang.Length; i++)
            {
                lang[i] = convertLanguage(languages[i]);
            }
            return lang;
        }

        private static ContactPerson[] getContacts(PXMeta meta)
        {
            List<ContactPerson> contactPersons = new List<ContactPerson>();
            foreach (Value v in meta.ContentVariable.Values) {
                List<ContactPerson> cps = new List<ContactPerson>();
                foreach (Contact c in v.ContentInfo.ContactInfo) {
                    ContactPerson cp;
                    if (contacts.ContainsKey(c.Email)) cp = contacts[c.Email];
                    else {
                        cp = new ContactPerson 
                        {
                            name = c.Forname + " " + c.Surname + ", " + c.OrganizationName, 
                            email = c.Email,
                            telephone = c.PhonePrefix + c.PhoneNo,
                            resource = "https://www.scb.se/contactperson/" + nextString()
                        };
                        contacts.Add(c.Email, cp);
                    }
                    cps.Add(cp);
                }
                contactPersons = contactPersons.Union(cps).ToList();
            }
            return contactPersons.ToArray();
        }
        
        private static string reformatString(string s)
        {
            DateTime date = DateTime.ParseExact(s, PCAXIS_DATE_FORMAT, null);
            string formatted = date.ToString(DCAT_DATE_FORMAT);
            return formatted;
        }

        private static string getLatestDate(string[] dates) // format yyyyMMdd HH:mm
        {
            List<DateTime> dateTimes = new List<DateTime>(dates.Length);
            for (int i = 0; i < dates.Length; i++) {
                dateTimes.Add(DateTime.ParseExact(dates[i], PCAXIS_DATE_FORMAT, null));
            }
            int maxIndex = dateTimes.IndexOf(dateTimes.Max());
            return dates[maxIndex];
        }

        private static string getLastModified(PXMeta meta)
        {
            string modified = meta.ContentInfo.LastUpdated;
            if (modified is null)
            {
                Values values = meta.ContentVariable.Values;
                string[] modifiedDates = new string[values.Count];
                for (int i = 0; i < modifiedDates.Length; i++)
                {
                    modifiedDates[i] = values[i].ContentInfo.LastUpdated;
                }
                modified = getLatestDate(modifiedDates);
            }
            return reformatString(modified);
        }

        private static string getCategory(List<PxMenuItem> path) {//"http://publications.europa.eu/resource/authority/data-theme/"+theme
            if (path.Count < 2) {
                return "";
            }
            return "http://publications.europa.eu/resource/authority/data-theme/" + themeMapping[path[1].Text];
        }

        private static Organization getProducer(PXMeta meta) {
            string name = meta.Source;
            if (organizations.ContainsKey(name)) return organizations[name];
            Organization org = new Organization {name = meta.Source, resource = "https://www.scb.se/producer/" + nextString()};
            organizations.Add(name, org);
            return org;
        }

        private static Keyword[] getKeywords(List<PxMenuItem> path, string lang) {
            List<Keyword> keywords = new List<Keyword>();
            foreach (PxMenuItem menu in path) {
                string text = menu.Text;
                if (text != "") { 
                    keywords.Add(new Keyword {text = text, lang = lang});
                }
            }
            return keywords.ToArray();
        }

        private static string getDistributionUrl(List<PxMenuItem> path, string title, string lang)
        {
            string url = "http://api.scb.se/OV0104/v1/doris/" + lang + "/ssd/";
            foreach (PxMenuItem menu in path.Skip(1))
            {
                url += menu.ID.Selection + "/";
            }
            url += title;
            return url;
        }
        private static Distribution[] getDistributions(List<PxMenuItem> path, string title, string[] langs)
        {
            int n = langs.Count();
            Distribution[] distrs = new Distribution[n];
            for (int i = 0; i < n; i++) {
                string lang = langs[i];

                Distribution distr = new Distribution();
                distr.title = "Data (" + lang + ")";
                distr.format = "application/json";
                distr.accessUrl = getDistributionUrl(path, title, lang);
                distr.language = convertLanguage(lang);
                distr.license = "http://creativecommons.org/publicdomain/zero/1.0/";
                distr.resource = "https://www.scb.se/distribution/" + nextString();

                distrs[i] = distr;
            }

            return distrs;
        }

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
                if (d.Count >= max)
                {
                    return;
                }

                var table = item as TableLink;
                Dataset dataset = new Dataset();
                try
                {
                    IPXModelBuilder builder = new PXSQLBuilder();
                    builder.SetPath(table.ID.Selection);
                    builder.ReadAllLanguages = true;
                    builder.SetPreferredLanguage("sv");
                    builder.BuildForSelection();
                    
                    dataset.publisher = Constants.SCB;
                    dataset.identifier = builder.Model.Meta.MainTable;
                    dataset.modified = getLastModified(builder.Model.Meta);
                    dataset.updateFrequency = getUpdateFrequency(builder.Model.Meta);

                    string[] langs = getLanguages(builder.Model.Meta);
                    dataset.languages = langs;
                    dataset.languageURIs = convertLanguages(langs);

                    dataset.descriptions =  getDescriptions(builder.Model.Meta, langs);
                    dataset.titles = getTitles(builder.Model.Meta, langs);

                    dataset.contactPersons = getContacts(builder.Model.Meta);
                    dataset.category = getCategory(path);
                    dataset.producer = getProducer(builder.Model.Meta);

                    dataset.keywords = getKeywords(path, "sv");
                    dataset.distributions = getDistributions(path, dataset.identifier, langs);

                    dataset.resource = "https://www.scb.se/dataset/" + nextString();

                    d.Add(dataset);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.ToString());
                }
            }
        }
        public static List<Dataset> GetDatasets(int n)
        {
            string dbLang = "sv";
            string nodeId = "";
            string dbid = "ssd";
            string menuid = "";
            TableLink tblFix = null;

            DatamodelMenu menu = ConfigDatamodelMenu.Create(
            dbLang,
            PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[dbid],
            m =>
            {
                m.NumberOfLevels = 5;
                m.RootSelection = nodeId == "" ? new ItemSelection() : new ItemSelection(menuid, nodeId);
                m.AlterItemBeforeStorage = item =>
                {
                    if (item is TableLink)
                    {
                        TableLink tbl = (TableLink)item;

                        if (string.Compare(tbl.ID.Selection, nodeId, true) == 0)
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

            var datasets = new List<Dataset>();
            var path = new List<PxMenuItem>();
            addRecursive(menu.CurrentItem, path, datasets, n);
            return datasets;
        }

        public static Catalog GetCatalog(int numberOfTables)
        {
            Catalog c = new Catalog();
            c.title = "Statistikdatabasen SCB";
            c.description = "Databas för SCB:s offentliga statistik";
            c.publisher = Constants.SCB;
            c.license = "http://creativecommons.org/publicdomain/zero/1.0/";
            c.datasets = Fetch.GetDatasets(numberOfTables).ToArray();
            c.language = "http://publications.europa.eu/resource/authority/language/SWE";
            return c;
        }
    }
}