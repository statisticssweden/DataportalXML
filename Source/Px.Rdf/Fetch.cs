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
    public class Fetch
    {
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

        private static int nextNum() {
            return ++hashNum;
        }

        private static string nextString() {
            return (++hashNum).ToString();
        }
        private static string reformatString(string s)
        {
            DateTime date = DateTime.ParseExact(s, "yyyyMMdd HH:mm", null);
            string formatted = date.ToString("yyyy-MM-ddTHH:mm:ss");
            return formatted;
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

        private static string checkLang(string str)
        {
            string lang;
            if (str == "sv")
                lang = "SWE";
            else
                lang = "ENG";

            return "http://publications.europa.eu/resource/authority/language/" + lang;
        }

        private static string[] getLanguages(PXMeta meta)
        {
            string[] langs = meta.GetAllLanguages();
            if (langs is null)
            {
                langs = new string[] { meta.Language };
            }
            for (int i = 0; i < langs.Length; i++)
            {
                langs[i] = checkLang(langs[i]);
            }
            return langs;
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

        private static void addRecursive(Item item, List<Dataset> d, int max)
        {

            if (item is PxMenuItem)
            {
                var menu = item as PxMenuItem;
                if (menu.HasSubItems)
                {
                    foreach (var subItem in menu.SubItems)
                    {
                        addRecursive(subItem, d, max);
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
                    dataset.modified = reformatString(builder.Model.Meta.CreationDate);
                    dataset.updateFrequency = getUpdateFrequency(builder.Model.Meta);
                    dataset.languages = getLanguages(builder.Model.Meta);
                    dataset.contactPersons = getContacts(builder.Model.Meta);
                   
                    d.Add(dataset);
                    //dataset.contact = builder.Model.Meta.ContentInfo;
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
            addRecursive(menu.CurrentItem, datasets, n);
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