using System;
using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;


namespace Px.Rdf
{
    public class CMNNFetcher : IFetcher
    {
        public Item GetBaseItem(string nodeID, string menuID, string lang, string dbid)
        {
            TableLink tblFix = null;

            DatamodelMenu menu = ConfigDatamodelMenu.Create(
            lang,
            PCAxis.Sql.DbConfig.SqlDbConfigsStatic.DataBases[dbid],
            m =>
            {
                m.NumberOfLevels = 5;
                m.RootSelection = nodeID == "" ? new ItemSelection() : new ItemSelection(menuID, nodeID);
                m.AlterItemBeforeStorage = item =>
                {
                    if (item is TableLink)
                    {
                        TableLink tbl = (TableLink)item;

                        if (string.Compare(tbl.ID.Selection, nodeID, true) == 0)
                        {
                            tblFix = tbl;
                        }
                        if (tbl.StartTime == tbl.EndTime)
                        {
                            tbl.Text = tbl.Text + " " + tbl.StartTime;
                        }
                        else
                        {
                            tbl.Text = tbl.Text + " " + tbl.StartTime + " - " + tbl.EndTime;
                        }

                        if (tbl.Published.HasValue)
                        {
                            tbl.SetAttribute("modified", tbl.Published.Value.ToShortDateString());
                        }
                    }
                    if (String.IsNullOrEmpty(item.SortCode))
                    {
                        item.SortCode = item.Text;
                    }
                };
            });
            return menu.CurrentItem;
        }
        public IPXModelBuilder GetBuilder(string selection)
        {
            IPXModelBuilder builder = new PXSQLBuilder();
            builder.SetPath(selection);
            return builder;
        }
    }
}
