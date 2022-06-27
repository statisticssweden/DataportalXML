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
    public class PcAxisFetcher : IFetcher
    {
        private string basepoint;

        public PcAxisFetcher(string basepoint)
        {
            this.basepoint = basepoint;
        }
        public Item GetBaseItem(string nodeID, string menuID, string lang, string dbid)
        {
            XmlMenu menu = new XmlMenu(XDocument.Load(dbid), lang,
                m =>
                {
                    m.Restriction = item =>
                    {
                        return true;
                    };
                });
            ItemSelection cid = new ItemSelection(menuID, nodeID);
            menu.SetCurrentItemBySelection(cid.Menu, cid.Selection);
            return menu.CurrentItem;
        }
        //@"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\"
        //@"C:\Temp\Databases\"
        public IPXModelBuilder GetBuilder(string selection)
        {
            IPXModelBuilder builder = new PXFileBuilder();
            string selectionPath = Path.Combine(basepoint, selection);
            builder.SetPath(selectionPath);
            return builder;
        }
    }
}
