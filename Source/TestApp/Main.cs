﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XMLWriter;
using Px.Rdf;
using Data;

namespace TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            int numberOfTables = 500;
            Catalog c = Fetch.GetCatalog(numberOfTables);
            List<Organization> orgs = Fetch.UniqueOrgs();
            List<ContactPerson> contacts = Fetch.UniqueContacts();
            XML.writeToFile(c, orgs, contacts, "../../../test.xml");
        }
    }
}
