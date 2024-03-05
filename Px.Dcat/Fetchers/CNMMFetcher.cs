using Microsoft.IdentityModel.Tokens;
using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;
using Px.Dcat.Interfaces;


namespace Px.Dcat.Fetchers
{
    public class CNMMFetcher : IFetcher
    {

        /// <summary>
        /// Retrieves the base item from the data model menu based on the provided parameters.
        /// </summary>
        /// <param name="nodeID">The ID of the node in the menu.</param>
        /// <param name="menuID">The ID of the menu.</param>
        /// <param name="lang">The language code.</param>
        /// <param name="dbid">The ID of the database.</param>
        /// <returns>The base item from the data model menu.</returns>
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

                            if (!tbl.Text.IsNullOrEmpty() && ((!IsInteger(tbl.Text[tbl.Text.Length - 1].ToString()) ||
                                                               !string.IsNullOrEmpty(tbl.StartTime) ||
                                                               !string.IsNullOrEmpty(
                                                                   tbl
                                                                       .EndTime)))) //Title ends with a number, no start- or endtime, add nothing
                            {
                                if (tbl.Text.EndsWith("-")) //Title ends with a dash, only endtime should be added
                                {
                                    tbl.Text = tbl.Text + " " + tbl.EndTime;
                                }
                                else if (tbl.StartTime == tbl.EndTime)
                                {
                                    tbl.Text = tbl.Text + " " + tbl.StartTime;
                                }
                                else if (tbl.StartTime.Contains("-"))
                                {
                                    tbl.Text = $"{tbl.Text} ({tbl.StartTime})-({tbl.EndTime})";
                                }
                                else
                                {
                                    tbl.Text = tbl.Text + " " + tbl.StartTime + " - " + tbl.EndTime;
                                }
                            }

                            if (tbl.Published.HasValue)
                            {
                                tbl.SetAttribute("Modified", tbl.Published.Value.ToShortDateString());
                            }
                        }
                        if (string.IsNullOrEmpty(item.SortCode))
                            {
                                item.SortCode = item.Text;
                            }
                        
                    };
                });
            return menu.CurrentItem;
        }
        /// <summary>
        /// Retrieves the builder for the given selection.
        /// </summary>
        /// <param name="selection">The selection for the builder.</param>
        /// <returns>The builder for the given selection.</returns>
        public IPXModelBuilder GetBuilder(string selection)
        {
            IPXModelBuilder builder = new PXSQLBuilder();
            builder.SetPath(selection);
            return builder;
        }

        /// <summary>
        /// Checks if a given value is an integer.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is an integer, false otherwise.</returns>
        private static bool IsInteger(string value)
        {
            int outValue;
            return int.TryParse(value, out outValue);
        }
    }
}
