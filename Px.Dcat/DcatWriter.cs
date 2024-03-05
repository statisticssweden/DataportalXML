using Px.Dcat.DataClasses;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Px.Dcat
{
    public static class DcatWriter
    {
        private static XmlDocument _doc = new XmlDocument();
        private static XmlNamespaceManager _nsm = new XmlNamespaceManager(_doc.NameTable);

        /// <summary>
        /// Create xml file based on settings
        /// </summary>
        /// <param name="fileName">File to write to</param>
        /// <param name="settings">Settings to use</param>
        public static void WriteToFile(string fileName, DcatSettings settings)
        {
            _doc = new XmlDocument();
            DataCollector collector = new DataCollector();
            Catalog c = collector.Get(settings);
            List<Organization> orgs = collector.UniqueOrgs();
            List<ContactPerson> contacts = collector.UniqueContacts();
            WriteToFile(c, orgs, contacts, fileName);
        }

        /// <summary>
        /// Writes a catalog along with lists of organizations and contacts to a file according to the dcat-ap standard.
        /// </summary>
        /// <param name="c">Catalog to be written</param>
        /// <param name="orgs">List of organizations to be written</param>
        /// <param name="contacts">List of contacts to be written</param>
        /// <param name="fileName">Name of the file</param>
        private static void WriteToFile(Catalog c, List<Organization> orgs, List<ContactPerson> contacts, string fileName)
        {
            _nsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            _nsm.AddNamespace("dcterms", "http://purl.org/dc/terms/");
            _nsm.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
            _nsm.AddNamespace("dcat", "http://www.w3.org/ns/dcat#");
            _nsm.AddNamespace("foaf", "http://xmlns.com/foaf/0.1/");
            _nsm.AddNamespace("vcard", "http://www.w3.org/2006/vcard/ns#");

            XmlDeclaration declaration = _doc.CreateXmlDeclaration("1.0", "utf-8", null);
            _doc.AppendChild(declaration);

            XmlElement rdf = _doc.CreateElement("rdf", "RDF", _nsm.LookupNamespace("rdf"));
            foreach (string prefix in _nsm)
            {
                if (prefix == "" || prefix == "rdf" || prefix.StartsWith("xml")) continue;
                rdf.SetAttribute("xmlns:" + prefix, _nsm.LookupNamespace(prefix));
            }

            _doc.AppendChild(rdf);

            XmlElement catalog = generateCatalog(c);
            rdf.AppendChild(catalog);

            foreach (Dataset d in c.Datasets)
            {
                XmlElement dElem = generateDataset(d);
                rdf.AppendChild(dElem);
            }

            foreach (ContactPerson cp in contacts)
            {
                XmlElement cElem = generateContact(cp);
                rdf.AppendChild(cElem);
            }


            foreach (Organization o in orgs)
            {
                XmlElement oElem = generateOrg(o);
                rdf.AppendChild(oElem);
            }

            foreach (Dataset ds in c.Datasets)
            {
                foreach (Distribution distr in ds.Distributions)
                {
                    XmlElement distrElem = generateDistribution(distr);
                    rdf.AppendChild(distrElem);
                }
            }

            _doc.Save(fileName);
        }

        /// <summary>
        /// Creates an xml element from a namespace with an attribute from a namespace
        /// </summary>
        private static XmlElement createElem(string elemNamespace, string elemName, string attrNamespace, string attributeName, string attrValue)
        {
            XmlElement elem = _doc.CreateElement(elemNamespace, elemName, _nsm.LookupNamespace(elemNamespace));
            XmlAttribute attr = createAttr(attrNamespace, attributeName, attrValue);
            elem.SetAttributeNode(attr);
            return elem;
        }

        /// <summary>
        /// Creates an xml element from a namespace with a value
        /// </summary>
        /// <param name="elemNamespace">Namespace of the tag</param>
        /// <param name="elemName">name of the tag</param>
        /// <param name="innerText">The value inside the tag</param>
        /// <returns>XmlElement</returns>
        private static XmlElement createElem(string elemNamespace, string elemName, string innerText)
        {
            XmlElement elem = _doc.CreateElement(elemNamespace, elemName, _nsm.LookupNamespace(elemNamespace));
            elem.InnerText = innerText;
            return elem;
        }

        /// <summary>
        /// Creates an element with from a namespace
        /// </summary>
        /// <param name="elemNamespace"></param>
        /// <param name="elemName"></param>
        /// <returns>XmlElement</returns>
        private static XmlElement createElem(string elemNamespace, string elemName)
        {
            return createElem(elemNamespace, elemName, "");
        }

        /// <summary>
        /// Create an xml attribute from a namespace
        /// </summary>
        /// <param name="ns">Namespace of the attribute</param>
        /// <param name="tagName">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        /// <returns>XmlAttribute</returns>
        private static XmlAttribute createAttr(string ns, string tagName, string value)
        {
            XmlAttribute attr = _doc.CreateAttribute(ns, tagName, _nsm.LookupNamespace(ns));
            attr.InnerText = value;
            return attr;
        }


        /// <summary>
        /// Converts a Catalog into an XmlElement
        /// </summary>
        /// <param name="c">Catalog to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateCatalog(Catalog c)
        {
            XmlElement catElem = createElem("dcat", "Catalog", "rdf", "about", c.Refrence);

            // languages
            foreach (string lang in c.Languages)
            {
                XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", lang);
                catElem.AppendChild(langElem);
            }

            // titles
            foreach (KeyValuePair<string, string> langTitle in c.Titles)
            {
                string lang = langTitle.Key;
                string title = langTitle.Value;
                XmlElement titleElem = createElem("dcterms", "title", "xml", "lang", lang);
                titleElem.InnerText = title;
                catElem.AppendChild(titleElem);
            }

            // descriptions
            foreach (KeyValuePair<string, string> langTitle in c.Descriptions)
            {
                string lang = langTitle.Key;
                string description = langTitle.Value;
                XmlElement descElem = createElem("dcterms", "description", "xml", "lang", lang);
                descElem.InnerText = description;
                catElem.AppendChild(descElem);
            }

            // Publisher reference
            XmlElement pubElem = createElem("dcterms", "publisher", "rdf", "resource", c.Publisher.Resource);
            catElem.AppendChild(pubElem);

            // Licence
            XmlElement licenseElem = createElem("dcterms", "license", "rdf", "resource", c.License);
            catElem.AppendChild(licenseElem);


            // Dataset references
            foreach (Dataset d in c.Datasets)
            {
                XmlElement dElem = createElem("dcat", "dataset", "rdf", "resource", d.Resource);
                catElem.AppendChild(dElem);
            }
            return catElem;
        }

        /// <summary>
        /// Convert a dataset into an XmlElement
        /// </summary>
        /// <param name="d">The dataset to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateDataset(Dataset d)
        {
            // Dataset
            XmlElement dElem = createElem("dcat", "Dataset", "rdf", "about", d.Resource);
            
            // Titles and descriptions
            for (int i = 0; i < d.Languages.Count; i++)
            {
                string lang = d.Languages[i];
                string title = d.Titles[i];
                string desc = d.Descriptions[i];

                if (title != "TABLE_HAS_NO_TITLE")
                {
                    XmlElement titleElem = createElem("dcterms", "title", "xml", "lang", lang);
                    titleElem.InnerText = title;
                    dElem.AppendChild(titleElem);
                }

                XmlElement descElem = createElem("dcterms", "description", "xml", "lang", lang);
                descElem.InnerText = desc;
                dElem.AppendChild(descElem);
            }

            // Publisher reference
            XmlElement pubElem = createElem("dcterms", "publisher", "rdf", "resource", d.Publisher.Resource);
            dElem.AppendChild(pubElem);

            // Producer reference
            XmlElement prodElem = createElem("dcterms", "creator", "rdf", "resource", d.Producer.Resource);
            dElem.AppendChild(prodElem);

            // Distribution
            foreach (Distribution distribution in d.Distributions)
            {
                XmlElement distElem = createElem("dcat", "distribution", "rdf", "resource", distribution.Resource);
                dElem.AppendChild(distElem);
            }

            // Category/Theme
            if (!(d.Category is null))
            {
                XmlElement themeElem = createElem("dcat", "theme", "rdf", "resource", d.Category);
                dElem.AppendChild(themeElem);
            }

            // Modified
            string dateTimeDataType = "http://www.w3.org/2001/XMLSchema#dateTime";
            XmlElement mod = createElem("dcterms", "modified", "rdf", "datatype", dateTimeDataType);
            mod.InnerText = d.Modified;
            dElem.AppendChild(mod);

            // Identifier
            string stringDataType = "http://www.w3.org/2001/XMLSchema#string";
            XmlElement identifier = createElem("dcterms", "identifier", "rdf", "datatype", stringDataType);
            identifier.InnerText = d.Identifier;
            dElem.AppendChild(identifier);

            // Keyword
            foreach (Keyword keyword in d.Keywords)
            {
                XmlElement keyElem = createElem("dcat", "keyword", "xml", "lang", keyword.Language);
                keyElem.InnerText = keyword.Text;
                dElem.AppendChild(keyElem);
            }

            // languages
            foreach (string language in d.LanguageURIs)
            {
                XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", language);
                dElem.AppendChild(langElem);
            }

            // Contacts
            foreach (ContactPerson cp in d.ContactPersons)
            {
                XmlElement contactPoint = createElem("dcat", "contactPoint", "rdf", "resource", cp.Resource);
                dElem.AppendChild(contactPoint);
            }

            // Landing page
            foreach (string url in d.Urls)
            {
                XmlElement landing = createElem("dcat", "landingPage", "rdf", "resource", url);
                dElem.AppendChild(landing);
            }

            // Update Frequency
            if (d.UpdateFrequency != null)
            {
                XmlElement updateElem = createElem("dcterms", "accrualPeriodicity", "rdf", "resource", d.UpdateFrequency);
                dElem.AppendChild(updateElem);
            }

            // Access rights
            string publicAccess = "http://publications.europa.eu/resource/authority/access-right/PUBLIC";
            XmlElement accessElem = createElem("dcterms", "accessRights", "rdf", "resource", publicAccess);

            dElem.AppendChild(accessElem);

            return dElem;
        }

        /// <summary>
        /// Convert organization into xml element
        /// </summary>
        /// <param name="org">Organization to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateOrg(Organization org)
        {
            // Org
            XmlElement orgElem = createElem("foaf", "Organization", "rdf", "about", org.Resource);

            // Name
            foreach ((string lang, string name) in org.Names)
            {
                if (lang is null)
                {
                    // unspecified language
                    XmlElement nameElem = createElem("foaf", "name", name);
                    orgElem.AppendChild(nameElem);
                }
                else
                {
                    XmlElement nameElem = createElem("foaf", "name", "xml", "lang", lang);
                    nameElem.InnerText = name;
                    orgElem.AppendChild(nameElem);
                }
            }

            return orgElem;
        }

        /// <summary>
        /// Convert contact into xml element
        /// </summary>
        /// <param name="cp">Contact person to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateContact(ContactPerson cp)
        {
            XmlElement individual = createElem("vcard", "Individual", "rdf", "about", cp.Resource);

            // Name
            XmlElement nameElem = createElem("vcard", "fn", cp.Name);
            individual.AppendChild(nameElem);

            // Email
            string trimmedEmail = cp.Email.Replace(" ", "");
            var isEmail = Regex.IsMatch(trimmedEmail, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

            if (isEmail)
            {
                XmlElement emailElem = createElem("vcard", "hasEmail", "rdf", "resource", "mailto:" + trimmedEmail);
                individual.AppendChild(emailElem);
            }
            // Phone
            //XmlElement phoneElem = createElem("vcard", "hasTelephone");
            //XmlElement descElem = createElem("dcterms", "description");

            //string phone = "tel:" + cp.Phone.Replace(" ", "");
            //XmlElement phoneVal = createElem("vcard", "hasValue", "rdf", "Resource", phone);

            //phoneElem.AppendChild(descElem);
            //descElem.AppendChild(phoneVal);
            //individual.AppendChild(phoneElem);

             return individual;
        }

        /// <summary>
        /// Convert Distribution into xml element
        /// </summary>
        /// <param name="dst">Distribution to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateDistribution(Distribution dst)
        {
            //about
            XmlElement distr = createElem("dcat", "Distribution", "rdf", "about", dst.Resource);

            // title
            XmlElement titleElem = createElem("dcterms", "title", "xml", "lang", dst.LanguageRaw);
            titleElem.InnerText = dst.Title;
            distr.AppendChild(titleElem);

            // format
            string stringDataType = "http://www.w3.org/2001/XMLSchema#string";
            XmlElement formatElem = createElem("dcterms", "format", "rdf", "datatype", stringDataType);
            formatElem.InnerText = dst.Format;

            //XmlElement formatElem = createElem("dcterms", "format", dst.Format);
            distr.AppendChild(formatElem);

            //accessURL
            XmlElement accessElem = createElem("dcat", "accessURL", "rdf", "resource", dst.AccessUrl);
            distr.AppendChild(accessElem);

            //language
            XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", dst.Language);
            distr.AppendChild(langElem);

            //license
            XmlElement licenseElem = createElem("dcterms", "license", "rdf", "resource", dst.License);
            distr.AppendChild(licenseElem);

            return distr;
        }

    }
}