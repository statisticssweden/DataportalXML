﻿using System;
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

    public class PcAxisFetcher : IFetcher{
        public Item GetBaseItem(string nodeID, string menuID, string lang, string dbid) {
                XmlMenu menu = new XmlMenu(XDocument.Load(dbid), lang,
                    m =>
                    {
                        m.Restriction = item =>
                        {
                            return true;
                        };
                    });

                //ItemSelection cid = PathHandlerFactory.Create(PCAxis.Web.Core.Enums.DatabaseType.PX).GetSelection(nodeID);
                ItemSelection cid = new ItemSelection(menuID, nodeID);
                menu.SetCurrentItemBySelection(cid.Menu, cid.Selection);
                return menu.CurrentItem;
        }
        //@"C:\Temp\PxGit\PxWeb\PXWeb\Resources\PX\Databases\"
        //@H:\Mina Dokument\github\summerprojekt\PxWeb\PXWeb\Resources\PX\Databases\
        public IPXModelBuilder GetBuilder(string selection) {
            IPXModelBuilder builder = new PXFileBuilder();
            string selectionPath = Path.Combine(@"H:\Mina Dokument\github\summerprojekt\PxWeb\PXWeb\Resources\PX\Databases\", selection);
            builder.SetPath(selectionPath);
            return builder;
        }
    }

     public class SQLFetcher : IFetcher {
        public Item GetBaseItem(string nodeID, string menuID, string lang, string dbid) {
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
        public IPXModelBuilder GetBuilder(string selection){
            IPXModelBuilder builder  = new PXSQLBuilder();
            builder.SetPath(selection);
            return builder;
        }
    }
}
