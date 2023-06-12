using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using System.IO;
using System.Xml.Linq;
using Px.Dcat.Interfaces;

namespace Px.Dcat.Fetchers
{
    public class PXFetcher : IFetcher
    {
        private string basepoint;

        public PXFetcher(string basepoint)
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
        public IPXModelBuilder GetBuilder(string selection)
        {
            IPXModelBuilder builder = new PXFileBuilder();
            string selectionPath = Path.Combine(basepoint, selection);
            builder.SetPath(selectionPath);
            return builder;
        }
    }
}
