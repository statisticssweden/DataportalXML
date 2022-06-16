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
        {
            {"sv", "SWE"},
            {"en", "ENG"},
        };

        private static int nextNum() {
            return ++hashNum;
        }

        private static string nextString() {
            return (++hashNum).ToString();
        }

        private static string getDescription(Notes notes)
        {
            string desc = "-";
            if (notes.Count > 0 && notes[0].Mandantory)
            {
                desc = notes[0].Text;
            }
            return desc;

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
            return "http://publications.europa.eu/resource/authority/language/" + lang;
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
                    ContactPerson cp = new ContactPerson 
                    {
                        name = c.Forname + " " + c.Surname + ", " + c.OrganizationName, 
                        email = c.Email,
                        telephone = c.PhonePrefix + c.PhoneNo,
                        url = "https://www.scb.se/contactperson/" + nextString()
                    };
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
            return new Organization {name = meta.Source, reference = "https://www.scb.se/producer/" + nextString()};
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
        private static Distribution[] getDistributions(List<PxMenuItem> path, string title)
        {
            Distribution sweDistr = new Distribution();
            sweDistr.title = "Datatjänst med information på svenska";
            sweDistr.format = "application/json";
            sweDistr.accessUrl = getDistributionUrl(path, title, "sv");
            sweDistr.language = convertLanguage("sv");
            sweDistr.license = "http://creativecommons.org/publicdomain/zero/1.0/";

            Distribution engDistr = new Distribution();
            engDistr.title = "Datatjänst med information på svenska";
            engDistr.format = "application/json";
            engDistr.accessUrl = getDistributionUrl(path, title, "en");
            engDistr.language = convertLanguage("sv");
            engDistr.license = "http://creativecommons.org/publicdomain/zero/1.0/";

            return new Distribution[] { sweDistr, engDistr };
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
                    
                    dataset.title = builder.Model.Meta.Title;
                    dataset.description = getDescription(builder.Model.Meta.Notes);
                    dataset.publisher = Constants.SCB;
                    dataset.identifier = builder.Model.Meta.MainTable;
                    dataset.modified = getLastModified(builder.Model.Meta);
                    dataset.updateFrequency = getUpdateFrequency(builder.Model.Meta);

                    string[] langs = getLanguages(builder.Model.Meta);
                    dataset.languages = convertLanguages(langs);

                    dataset.contactPersons = getContacts(builder.Model.Meta);
                    dataset.category = getCategory(path);
                    dataset.producer = getProducer(builder.Model.Meta);

                    dataset.keywords = getKeywords(path, "sv");
                    dataset.distributions = getDistributions(path, dataset.identifier);

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