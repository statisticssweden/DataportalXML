using System;
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
            int numberOfTables = 100;
            Catalog c = Fetch.GetCatalog(numberOfTables);
            XML.writeToFile(c, "../../../test.xml");
        }
    }
}
