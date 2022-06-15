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
        public static void writeToFile(Catalog c, string fileName)
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

            List<ContactPerson> uniqueContacts = new List<ContactPerson>();
            foreach(Dataset d in c.datasets){
                uniqueContacts = uniqueContacts.Union(d.contactPersons).ToList();
            }

            foreach (ContactPerson cp in uniqueContacts) {
                XmlElement cElem = generateContact(doc, cp, nsm);
                rdf.AppendChild(cElem);
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
            pubAbout.InnerText = c.publisher.reference;

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
                about.InnerText = d.url();
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
            about.InnerText = d.url();
            dElem.SetAttributeNode(about);

            // Title
            XmlElement titleElem = doc.CreateElement("dcterms", "title", nsm.LookupNamespace("dcterms"));
            titleElem.InnerText = d.title;
            dElem.AppendChild(titleElem);
            
            // Description
            XmlElement descElem = doc.CreateElement("dcterms", "description", nsm.LookupNamespace("dcterms"));
            descElem.InnerText = d.description;
            dElem.AppendChild(descElem);

            // Publisher reference
            XmlElement pubElem = doc.CreateElement("dcterms", "publisher", nsm.LookupNamespace("dcterms"));

            XmlAttribute pubAbout = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            pubAbout.InnerText = d.publisher.reference;

            pubElem.SetAttributeNode(pubAbout);
            dElem.AppendChild(pubElem);

            // Modified
            string dateTimeDataType = "http://www.w3.org/2001/XMLSchema#dateTime";
            XmlElement mod = doc.CreateElement("dcterms", "modified", nsm.LookupNamespace("dcterms"));
            XmlAttribute modDataType = doc.CreateAttribute("rdf", "datatype", nsm.LookupNamespace("rdf"));
            modDataType.InnerText = dateTimeDataType;
            mod.SetAttributeNode(modDataType);
            mod.InnerText = d.modified;
            dElem.AppendChild(mod);

            // languages
            
            foreach(string language in d.languages)
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
                contactData.InnerText = cp.url;

                contactPoint.SetAttributeNode(contactData);
                dElem.AppendChild(contactPoint);
            }

            
            // Landing page
            //sv
            XmlElement landingSV = doc.CreateElement("dcat", "landingPage", nsm.LookupNamespace("dcat"));
            XmlAttribute landSVRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            landSVRes.InnerText = d.url(true);
            landingSV.SetAttributeNode(landSVRes);
            dElem.AppendChild(landingSV);


            //en
            XmlElement landingEN = doc.CreateElement("dcat", "landingPage", nsm.LookupNamespace("dcat"));
            XmlAttribute landENRes = doc.CreateAttribute("rdf", "resource", nsm.LookupNamespace("rdf"));
            landENRes.InnerText = d.url(false);
            landingEN.SetAttributeNode(landENRes);
            dElem.AppendChild(landingEN);

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
            about.InnerText = org.reference;
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
            about.InnerText = cp.url;
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
    }
}