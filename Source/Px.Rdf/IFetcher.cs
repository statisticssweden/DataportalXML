using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;


namespace Px.Rdf
{
    public interface IFetcher {
        Item GetBaseItem(string nodeID, string menuID, string lang, string dbid);
        IPXModelBuilder GetBuilder(string selection);
    }
}
