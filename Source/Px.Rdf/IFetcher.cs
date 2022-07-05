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
    /// <summary>
    /// Interface for a fetcher, can fetch item and get a builder 
    /// </summary>
    public interface IFetcher {
        /// <summary>
        /// Gets an item from a database
        /// </summary>
        /// <param name="nodeID">ID of the node to get</param>
        /// <param name="menuID">Id of the menu to get</param>
        /// <param name="lang">Language to fetch in</param>
        /// <param name="dbid">ID of database</param>
        /// <returns></returns>
        Item GetBaseItem(string nodeID, string menuID, string lang, string dbid);
        
        /// <summary>
        /// Get a builder a from selection
        /// </summary>
        /// <param name="selection">Selection</param>
        /// <returns>IPXModelBuilder</returns>
        IPXModelBuilder GetBuilder(string selection);
    }
}
