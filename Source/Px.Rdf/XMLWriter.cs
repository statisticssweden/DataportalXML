using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Px.Rdf
{
    public class XML
    {
        private static XmlDocument doc = new XmlDocument();
        private static XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);

        /// <summary>
        /// Create xml file based on settings
        /// </summary>
        /// <param name="fileName">File to write to</param>
        /// <param name="settings">Settings to use</param>
        public static void WriteToFile(string fileName, RdfSettings settings)
        {
            doc = new XmlDocument();
            Fetcher fetcher = new Fetcher();
            fetcher.LoadSettings(settings);
            int numberOfTables = 0; // No cap
            Catalog c = fetcher.GetCatalog(numberOfTables);
            List<Organization> orgs = fetcher.UniqueOrgs();
            List<ContactPerson> contacts = fetcher.UniqueContacts();
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
            nsm.AddNamespace("adms", "http://www.w3.org/ns/adms#");
            nsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            nsm.AddNamespace("odrs", "http://schema.theodi.org/odrs#");
            nsm.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema#");
            nsm.AddNamespace("dcterms", "http://purl.org/dc/terms/");
            nsm.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
            nsm.AddNamespace("vcard", "http://www.w3.org/2006/vcard/ns#");
            nsm.AddNamespace("store", "https://catalog.scb.se/store/");
            nsm.AddNamespace("dcat", "http://www.w3.org/ns/dcat#");
            nsm.AddNamespace("foaf", "http://xmlns.com/foaf/0.1/");
            nsm.AddNamespace("es", "http://entrystore.org/terms/");
            nsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);

            XmlElement rdf = doc.CreateElement("rdf", "RDF", nsm.LookupNamespace("rdf"));
            foreach (string prefix in nsm)
            {
                if (prefix == "" || prefix == "rdf" || prefix.StartsWith("xml")) continue;
                rdf.SetAttribute("xmlns:" + prefix, nsm.LookupNamespace(prefix));
            }

            doc.AppendChild(rdf);

            XmlElement catalog = generateCatalog(c);
            rdf.AppendChild(catalog);

            XmlElement org = generateOrg(c.publisher);
            rdf.AppendChild(org);
            
            foreach (Dataset d in c.datasets)
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

            foreach (Dataset ds in c.datasets)
            {
                foreach (Distribution distr in ds.distributions)
                {
                    XmlElement distrElem = generateDistribution(distr);
                    rdf.AppendChild(distrElem);
                }
            }

            doc.Save(fileName);
        }
        /// <summary>
        /// Creates an xml element from a namespace with an attribute from a namespace
        /// </summary>
        private static XmlElement createElem(string elemNamespace, string elemName, string attrNamespace, string attributeName, string attrValue)
        {
            XmlElement elem = doc.CreateElement(elemNamespace, elemName, nsm.LookupNamespace(elemNamespace));
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
            XmlElement elem = doc.CreateElement(elemNamespace, elemName, nsm.LookupNamespace(elemNamespace));
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
            XmlAttribute attr = doc.CreateAttribute(ns, tagName, nsm.LookupNamespace(ns));
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
            XmlElement catElem = createElem("dcat", "catalog");
            XmlElement titleElem = createElem("dcterms", "title", c.title);
            catElem.AppendChild(titleElem);

            // description 
            XmlElement descElem = createElem("dcterms", "description", c.description);
            catElem.AppendChild(descElem);

            // Publisher reference
            XmlElement pubElem = createElem("dcterms", "publisher", "rdf", "resource", c.publisher.resource);
            catElem.AppendChild(pubElem);

            // Licence
            XmlElement licenseElem = createElem("dcterms", "license", "rdf", "resource", c.license);
            catElem.AppendChild(licenseElem);

            // Language
            XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", c.language);
            catElem.AppendChild(langElem);

            // Dataset references
            foreach (Dataset d in c.datasets)
            {
                XmlElement dElem = createElem("dcat", "Dataset", "rdf", "resource", d.resource);
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
            XmlElement dElem = createElem("dcat", "Dataset", "rdf", "about", d.resource);

            int numLangs = d.languages.Count();
            // Title
            for (int i = 0; i < numLangs; i++)
            {
                string title = d.titles[i];
                string lang = d.languages[i];

                XmlElement titleElem = createElem("dcterms", "title", "xml", "lang", lang);
                titleElem.InnerText = title;
                dElem.AppendChild(titleElem);
            }

            // Description
            for (int i = 0; i < numLangs; i++)
            {
                string desc = d.descriptions[i];
                string lang = d.languages[i];

                XmlElement descElem = createElem("dcterms", "description", "xml", "lang", lang);
                descElem.InnerText = desc;
                dElem.AppendChild(descElem);
            }

            // Publisher reference
            XmlElement pubElem = createElem("dcterms", "publisher", "rdf", "resource", d.publisher.resource);
            dElem.AppendChild(pubElem);

            // Producer reference
            XmlElement prodElem = createElem("dcterms", "creator", "rdf", "resource", d.producer.resource);
            dElem.AppendChild(prodElem);

            // Distribution
            foreach (Distribution distribution in d.distributions)
            {
                XmlElement distElem = createElem("dcat", "distribution", "rdf", "resource", distribution.resource);
                dElem.AppendChild(distElem);
            }

            // Category/Theme
            if (!(d.category is null)){
                XmlElement themeElem = createElem("dcat", "theme", "rdf", "resource", d.category);
                dElem.AppendChild(themeElem);
            }

            // Modified
            string dateTimeDataType = "http://www.w3.org/2001/XMLSchema#dateTime";
            XmlElement mod = createElem("dcterms", "modified", "rdf", "datatype", dateTimeDataType);
            mod.InnerText = d.modified;
            dElem.AppendChild(mod); 
            

            // Identifier
            XmlElement identifier = createElem("dcterms", "identifier", d.identifier);
            dElem.AppendChild(identifier);

            // Keyword
            foreach (Keyword keyword in d.keywords)
            {
                XmlElement keyElem = createElem("dcat", "keyword", "xml", "lang", keyword.lang);
                keyElem.InnerText = keyword.text;
                dElem.AppendChild(keyElem);
            }

            // languages
            foreach (string language in d.languageURIs)
            {
                XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", language);
                dElem.AppendChild(langElem);
            }

            // Contacts
            foreach (ContactPerson cp in d.contactPersons)
            {
                XmlElement contactPoint = createElem("dcat", "contactPoint", "rdf", "resource", cp.resource);
                dElem.AppendChild(contactPoint);
            }


            // Landing page
            foreach (string url in d.urls)
            {
                XmlElement landing = createElem("dcat", "landingPage", "rdf", "resource", url);
                dElem.AppendChild(landing);
            }

            // updatefreq
            if (d.updateFrequency != null){
                XmlElement updateElem = createElem("dcterms", "accrualPeriodicity", "rdf", "resource", d.updateFrequency);
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
            XmlElement orgElem = createElem("foaf", "Organization", "rdf", "about", org.resource);

            // Name
            XmlElement nameElem = createElem("foaf", "name", org.name);
            orgElem.AppendChild(nameElem);

            return orgElem;
        }

        /// <summary>
        /// Convert contact into xml element
        /// </summary>
        /// <param name="cp">Contact person to be converted</param>
        /// <returns>XmlElement</returns>
        public static XmlElement generateContact(ContactPerson cp)
        {
            XmlElement individual = createElem("vcard", "Individual", "rdf", "about", cp.resource);

            // Name
            XmlElement nameElem = createElem("vcard", "fn", cp.name);
            individual.AppendChild(nameElem);

            // Email
            XmlElement emailElem = createElem("vcard", "hasEmail", "rdf", "resource", "mailto:" + cp.email);
            individual.AppendChild(emailElem);

            // Phone
            XmlElement phoneElem = createElem("vcard", "hasTelephone");
            XmlElement descElem = createElem("dcterms", "description");

            string phone = "tel:" + cp.telephone.Replace(" ", "");
            XmlElement phoneVal = createElem("vcard", "hasValue", "rdf", "resource", phone);

            phoneElem.AppendChild(descElem);
            descElem.AppendChild(phoneVal);
            individual.AppendChild(phoneElem);

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
            XmlElement distr = createElem("dcat", "Distribution", "rdf", "about", dst.resource);

            // title
            XmlElement titleElem = createElem("dcterms", "title", dst.title);
            distr.AppendChild(titleElem);

            // format
            XmlElement formatElem = createElem("dcterms", "format", dst.format);
            distr.AppendChild(titleElem);

            //accessURL
            XmlElement accessElem = createElem("dcat", "accessURL", "rdf", "resource", dst.accessUrl);
            distr.AppendChild(accessElem);

            //language
            XmlElement langElem = createElem("dcterms", "language", "rdf", "resource", dst.language);
            distr.AppendChild(langElem);

            //license
            XmlElement licenseElem = createElem("dcterms", "license", "rdf", "resource", dst.license);
            distr.AppendChild(licenseElem);

            return distr;
        }

    }
}