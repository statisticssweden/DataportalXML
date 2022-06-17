  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Px.Rdf;
using System.Xml;
using Data;

namespace XMLWriter
{
    class XML
    {
        public static void writeToFile(Catalog c, Organization[] orgs, ContactPerson[] contacts ,string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = false,
            };
            XmlWriter w = XmlWriter.Create(fileName, settings);

            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
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

            XmlElement catalog = generateCatalog(doc, c, nsm);
            rdf.AppendChild(catalog);

            XmlElement org = generateOrg(doc, c.publisher, nsm);
            rdf.AppendChild(org);

            foreach (Dataset d in c.datasets)
            {
                XmlElement dElem = generateDataset(doc, d, nsm);
                rdf.AppendChild(dElem);
            }

            foreach (ContactPerson cp in contacts) {
                XmlElement cElem = generateContact(doc, cp, nsm);
                rdf.AppendChild(cElem);
            }


            foreach (Organization o in orgs) {
                XmlElement oElem = generateOrg(doc, o, nsm);
                rdf.AppendChild(oElem);
            }

            foreach (Dataset ds in c.datasets)
            {
                foreach (Distribution distr in ds.distributions)
                {
                    XmlElement distrElem = generateDistribution(doc, distr, nsm);
                    rdf.AppendChild(distrElem);
                }
            }

            doc.WriteTo(w);
            w.Close();
        }
        public static XmlElement generateCatalog(XmlDocument doc, Catalog c, XmlNamespaceManager nsm)
        {
            XmlElement catElem = doc.CreateElement("dcat", "Catalog", nsm.LookupNamespace("dcat"));
            XmlElement titleElem = doc.CreateElement("dcterms", "title", nsm.LookupNamespace("dcterms"));
            titleElem.InnerText = c.title;
            catElem.AppendChild(titleElem);

            // description 
            XmlElement descElem = doc.CreateElement("dcterms", "description", nsm.LookupNamespace("dcterms"));
            descElem.InnerText = c.description;
            catElem.AppendChild(descElem);

            // Publisher reference
            XmlElement pubElem = doc.CreateElement("dcterms", "publisher", nsm.LookupNamespace("dcterms"));

            XmlAttribute pubAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            pubAbout.InnerText = c.publisher.resource;

            pubElem.SetAttributeNode(pubAbout);
            catElem.AppendChild(pubElem);

            // Licence
            XmlElement licenseElem = doc.CreateElement("dcterms", "license", nsm.LookupNamespace("dcterms"));

            XmlAttribute licRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            licRes.InnerText = c.license;
            licenseElem.SetAttributeNode(licRes);

            catElem.AppendChild(licenseElem);

            // Language
            XmlElement langElem = doc.CreateElement("dcterms", "language", nsm.LookupNamespace("dcterms"));

            XmlAttribute langAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            langAbout.InnerText = c.language;

            langElem.SetAttributeNode(langAbout);
            catElem.AppendChild(langElem);

            // Dataset references

            foreach (Dataset d in c.datasets)
            {
                XmlElement dElem = doc.CreateElement("dcat", "Dataset", nsm.LookupNamespace("dcat"));

                XmlAttribute about = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
                about.InnerText = d.resource;
                dElem.SetAttributeNode(about);

                catElem.AppendChild(dElem);
            }
            return catElem;
        }

        public static XmlElement generateDataset(XmlDocument doc, Dataset d, XmlNamespaceManager nsm)
        {
            // Dataset
            XmlElement dElem = doc.CreateElement("dcat", "Dataset", nsm.LookupNamespace("dcat"));

            // Reference
            XmlAttribute about = doc.CreateAttribute("rdf", "about", nsm.LookupNamespace("rdf"));
            about.InnerText = d.resource;
            dElem.SetAttributeNode(about);

            int numLangs = d.languages.Count();
            // Title
            for(int i = 0; i < numLangs; i++){
                string title = d.titles[i];
                string lang = d.languages[i];

                XmlElement titleElem = doc.CreateElement("dcterms", "title", nsm.LookupNamespace("dcterms"));
                XmlAttribute titleLang = doc.CreateAttribute("xml", "lang", nsm.LookupNamespace("xml"));

                titleLang.InnerText = lang;
                titleElem.InnerText = title;

                titleElem.SetAttributeNode(titleLang);
                dElem.AppendChild(titleElem);
            }
            
            // Description
            for(int i = 0; i < numLangs; i++){
                string desc = d.descriptions[i];
                string lang = d.languages[i]; 

                XmlElement descElem = doc.CreateElement("dcterms", "description", nsm.LookupNamespace("dcterms"));
                XmlAttribute descLang = doc.CreateAttribute("xml", "lang", nsm.LookupNamespace("xml"));

                descLang.InnerText = lang;
                descElem.InnerText = desc;

                descElem.SetAttributeNode(descLang);
                dElem.AppendChild(descElem);
            }

            // Publisher reference
            XmlElement pubElem = doc.CreateElement("dcterms", "publisher", nsm.LookupNamespace("dcterms"));

            XmlAttribute pubAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            pubAbout.InnerText = d.publisher.resource;

            pubElem.SetAttributeNode(pubAbout);
            dElem.AppendChild(pubElem);

            // Producer reference
            XmlElement prodElem = doc.CreateElement("dcterms", "creator", nsm.LookupNamespace("dcterms"));

            XmlAttribute prodAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            prodAbout.InnerText = d.producer.resource;

            prodElem.SetAttributeNode(prodAbout);
            dElem.AppendChild(prodElem);

            // Distribution
            foreach (Distribution distribution in d.distributions)
            {
                XmlElement distElem = doc.CreateElement("dcat", "distribution", nsm.LookupNamespace("dcat"));
                XmlAttribute distAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));

                distAbout.InnerText = distribution.resource;

                distElem.SetAttributeNode(distAbout);
                dElem.AppendChild(distElem);
            }

            // Category/Theme
            XmlElement themeElem = doc.CreateElement("dcat", "theme", nsm.LookupNamespace("dcat"));
            XmlAttribute themeAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));

            themeAbout.InnerText = d.category;
            themeElem.SetAttributeNode(themeAbout);
            dElem.AppendChild(themeElem);

            // Modified
            string dateTimeDataType = "http://www.w3.org/2001/XMLSchema#dateTime";
            XmlElement mod = doc.CreateElement("dcterms", "modified", nsm.LookupNamespace("dcterms"));
            XmlAttribute modDataType = doc.CreateAttribute("rdf", "datatype", nsm.LookupNamespace("rdf"));
            modDataType.InnerText = dateTimeDataType;
            mod.SetAttributeNode(modDataType);
            mod.InnerText = d.modified;
            dElem.AppendChild(mod);

            // Identifier

            XmlElement identifier = doc.CreateElement("dcterms", "identifier", nsm.LookupNamespace("dcterms"));
            identifier.InnerText = d.identifier;
            dElem.AppendChild(identifier);

            // Keyword
            foreach(Keyword keywords in d.keywords){
                XmlElement keyElem = doc.CreateElement("dcat", "keyword", nsm.LookupNamespace("dcat"));
                XmlAttribute keyLang = doc.CreateAttribute("xml", "lang", nsm.LookupNamespace("xml"));
                keyElem.InnerText = keywords.text;
                keyLang.InnerText = keywords.lang;
                keyElem.SetAttributeNode(keyLang);
                dElem.AppendChild(keyElem);
            }

            // languages
            
            foreach(string language in d.languageURIs)
            {
                XmlElement langElem = doc.CreateElement("dcterms", "language", nsm.LookupNamespace("dcterms"));

                XmlAttribute langAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
                langAbout.InnerText = language;

                langElem.SetAttributeNode(langAbout);
                dElem.AppendChild(langElem);
            }

            // Contacts
            foreach(ContactPerson cp in d.contactPersons){
                XmlElement contactPoint = doc.CreateElement("dcat", "contactPoint", nsm.LookupNamespace("dcat"));
                XmlAttribute contactData = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
                contactData.InnerText = cp.resource;

                contactPoint.SetAttributeNode(contactData);
                dElem.AppendChild(contactPoint);
            }


            // Landing page
            foreach (string lang in d.languages) {
                XmlElement landing = doc.CreateElement("dcat", "landingPage", nsm.LookupNamespace("dcat"));
                XmlAttribute landRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
                landRes.InnerText = d.url(lang);
                landing.SetAttributeNode(landRes);
                dElem.AppendChild(landing);
            }

            // updatefreq
            XmlElement updateElem = doc.CreateElement("dcterms", "accrualPeriodicity", nsm.LookupNamespace("dcterms"));
            XmlAttribute updateRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            updateRes.InnerText = d.updateFrequency;
            updateElem.SetAttributeNode(updateRes);
            dElem.AppendChild(updateElem);

            // Access rights
            XmlElement accessElem = doc.CreateElement("dcterms", "accessRights", nsm.LookupNamespace("dcterms"));

            XmlAttribute accessRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            accessRes.InnerText = "http://publications.europa.eu/resource/authority/access-right/PUBLIC";
            accessElem.SetAttributeNode(accessRes);

            dElem.AppendChild(accessElem);

            return dElem;
        }

        public static XmlElement generateOrg(XmlDocument doc, Organization org, XmlNamespaceManager nsm)
        {
            // Org
            XmlElement orgElem = doc.CreateElement("foaf", "Organization", nsm.LookupNamespace("foaf"));

            // Reference
            XmlAttribute about = doc.CreateAttribute("rdf", "about", nsm.LookupNamespace("rdf"));
            about.InnerText = org.resource;
            orgElem.SetAttributeNode(about);

            // Name
            XmlElement nameElem = doc.CreateElement("foaf", "name", nsm.LookupNamespace("foaf"));
            nameElem.InnerText = org.name;
            orgElem.AppendChild(nameElem);

            return orgElem;
        }
        
        public static XmlElement generateContact(XmlDocument doc, ContactPerson cp, XmlNamespaceManager nsm){
            XmlElement individual = doc.CreateElement("vcard", "Individual", nsm.LookupNamespace("vcard"));

            XmlAttribute about = doc.CreateAttribute("rdf", "about", nsm.LookupNamespace("rdf"));
            about.InnerText = cp.resource;
            individual.SetAttributeNode(about);

            // Name
            XmlElement nameElem = doc.CreateElement("vcard", "fn", nsm.LookupNamespace("vcard"));
            nameElem.InnerText = cp.name;
            individual.AppendChild(nameElem);

            // Email
            XmlElement emailElem = doc.CreateElement("vcard", "hasEmail", nsm.LookupNamespace("vcard"));
            XmlAttribute emailAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));

            emailAbout.InnerText = "mailto:" + cp.email;
            emailElem.SetAttributeNode(emailAbout);
            individual.AppendChild(emailElem);
            
            // Phone
            XmlElement phoneElem = doc.CreateElement("vcard", "hasTelephone", nsm.LookupNamespace("vcard"));
            XmlElement descElem = doc.CreateElement("dcterms", "description", nsm.LookupNamespace("dcterms"));
            XmlElement phoneVal = doc.CreateElement("vcard", "hasValue", nsm.LookupNamespace("vcard"));
            XmlAttribute phoneAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));

            phoneAbout.InnerText = "tel:"+cp.telephone.Replace(" ", "");
            phoneVal.SetAttributeNode(phoneAbout);
            phoneElem.AppendChild(descElem);
            descElem.AppendChild(phoneVal);
            individual.AppendChild(phoneElem);
            
            return individual;
        }

        public static XmlElement generateDistribution(XmlDocument doc, Distribution dst, XmlNamespaceManager nsm)
        {
            XmlElement distr = doc.CreateElement("dcat", "Distribution", nsm.LookupNamespace("dcat"));

            //about
            XmlAttribute about = doc.CreateAttribute("rdf", "about", nsm.LookupNamespace("rdf"));
            about.InnerText = dst.resource;
            distr.SetAttributeNode(about);

            // title
            XmlElement titleElem = doc.CreateElement("dcterms", "title", nsm.LookupNamespace("dcterms"));
            titleElem.InnerText = dst.title;
            distr.AppendChild(titleElem);

            // format
            XmlElement formatElem = doc.CreateElement("dcterms", "format", nsm.LookupNamespace("dcterms"));
            formatElem.InnerText = dst.format;
            distr.AppendChild(formatElem);

            //accessURL
            XmlElement accessElem = doc.CreateElement("dcat", "accessURL", nsm.LookupNamespace("dcat"));
            XmlAttribute accessAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            accessAbout.InnerText = dst.accessUrl;
            accessElem.SetAttributeNode(accessAbout);
            distr.AppendChild(accessElem);

            //language
            XmlElement langElem = doc.CreateElement("dcterms", "language", nsm.LookupNamespace("dcterms"));
            XmlAttribute langRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            langRes.InnerText = dst.language;
            langElem.SetAttributeNode(langRes);
            distr.AppendChild(langElem);

            //license
            XmlElement licenseElem = doc.CreateElement("dcterms", "license", nsm.LookupNamespace("dcterms"));
            XmlAttribute licenseRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            licenseRes.InnerText = dst.license;
            licenseElem.SetAttributeNode(licenseRes);
            distr.AppendChild(licenseElem);

            return distr;
        }
    }
}