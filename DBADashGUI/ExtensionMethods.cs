﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static DBADashGUI.DBADashStatus;

namespace DBADashGUI
{
    public static class ExtensionMethods
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) { return value; }

            return value[..Math.Min(value.Length, maxLength)];
        }

        private static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

        public static bool IsNumeric(this Type myType)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        public static DataTable AsDataTable(this IEnumerable<int> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            foreach (int i in list)
            {
                var r = dt.NewRow();
                r["ID"] = i;
                dt.Rows.Add(r);
            }

            return dt;
        }

        public static Color GetColor(this DBADashStatusEnum value)
        {
            return DBADashStatus.GetStatusColour(value);
        }

        public static Color ContrastColor(this Color value)
        {
            return ((value.R * 0.299) + (value.G * 0.587) + (value.B * 0.114)) > 186 ? Color.Black : Color.White;
        }

        public static void SetStatusColor(this DataGridViewCell cell, Color StatusColor)
        {
            cell.Style.BackColor = StatusColor;
            cell.Style.ForeColor = StatusColor.ContrastColor();
            if (cell.GetType() == typeof(DataGridViewLinkCell))
            {
                ((DataGridViewLinkCell)cell).LinkColor = StatusColor.ContrastColor();
                ((DataGridViewLinkCell)cell).VisitedLinkColor = StatusColor.ContrastColor();
            }
            cell.Style.SelectionBackColor = StatusColor == Color.White || StatusColor == DashColors.NotApplicable ? Color.Empty : ControlPaint.Light(StatusColor);
        }

        public static void SetStatusColor(this DataGridViewCell cell, DBADashStatusEnum Status)
        {
            cell.SetStatusColor(Status.GetColor());
        }

        public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        /// <summary>
        /// Returns structure with column layout - size, position & visibility
        /// </summary>
        internal static List<KeyValuePair<string, PersistedColumnLayout>> GetColumnLayout(this DataGridView dgv)
        {
            return dgv.Columns.Cast<DataGridViewColumn>()
           .Select(c => new KeyValuePair<string, PersistedColumnLayout>(c.Name, new PersistedColumnLayout() { Visible = c.Visible, Width = c.Width, DisplayIndex = c.DisplayIndex }))
           .ToList();
        }

        /// <summary>
        /// Loads a saved column layout to the grid.  Size, position & visibility of columns
        /// </summary>
        internal static void LoadColumnLayout(this DataGridView dgv, List<KeyValuePair<string, PersistedColumnLayout>> savedCols)
        {
            if (savedCols == null)
            {
                return;
            }
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (savedCols.Where(savedCol => savedCol.Key == col.Name).Count() == 1)
                {
                    var savedCol = savedCols.Where(savedCol => savedCol.Key == col.Name).First();
                    col.Visible = savedCol.Value.Visible;
                    col.Width = savedCol.Value.Width;
                    if (savedCol.Value.DisplayIndex >= 0)
                    {
                        col.DisplayIndex = savedCol.Value.DisplayIndex;
                    }
                }
                else
                {
                    col.Visible = false;
                }
            }
        }

        internal static SQLTreeItem SelectedSQLTreeItem(this TreeView value)
        {
            return value.SelectedNode.AsSQLTreeItem();
        }

        internal static SQLTreeItem AsSQLTreeItem(this TreeNode value)
        {
            return (SQLTreeItem)value;
        }

        /// <summary>
        /// Add Guid SqlParameter to the collection only if parameter value is not empty
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddGuidIfNotEmpty(this SqlParameterCollection p, string parameterName, Guid value)
        {
            if (value != Guid.Empty)
            {
                return p.AddWithValue(parameterName, value);
            }
            return null;
        }

        /// <summary>
        /// Add Guid SqlParameter to the collection only if parameter value is not null or empty
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddStringIfNotNullOrEmpty(this SqlParameterCollection p, string parameterName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return p.AddWithValue(parameterName, value);
            }
            return null;
        }

        /// <summary>
        /// Add parameter with value, passing DBNull.value in place of null
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddWithNullableValue(this SqlParameterCollection p, string parameterName, object value)
        {
            if (value == null)
                return p.AddWithValue(parameterName, DBNull.Value);
            else
                return p.AddWithValue(parameterName, value);
        }

        /// <summary>
        /// Add parameter with value if value is greater than zero
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddIfGreaterThanZero(this SqlParameterCollection p, string parameterName, int value)
        {
            if (value > 0)
                return p.AddWithValue(parameterName, value);
            else
                return null;
        }

        /// <summary>
        /// Add parameter with value if value is greater than zero
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddIfGreaterThanZero(this SqlParameterCollection p, string parameterName, long value)
        {
            if (value > 0)
                return p.AddWithValue(parameterName, value);
            else
                return null;
        }

        /// <summary>
        /// Add parameter with value if value is less than max value
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="value">Parameter value</param>
        internal static SqlParameter AddIfLessThanMaxValue(this SqlParameterCollection p, string parameterName, long value)
        {
            if (value != Int64.MaxValue)
                return p.AddWithValue(parameterName, value);
            else
                return null;
        }

        /// <summary>
        /// Check a single ToolStripMenuItem.  Other menu items to be unchecked
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="checkedItem">Item to be checked.  Other drop down items will be unchecked</param>
        internal static void CheckSingleItem(this ToolStripDropDownButton dropdown, ToolStripMenuItem checkedItem)
        {
            foreach (ToolStripMenuItem mnu in dropdown.DropDownItems.OfType<ToolStripMenuItem>())
            {
                mnu.Checked = mnu == checkedItem;
            }
        }

        internal static void OpenAsTextFile(this string value)
        {
            string path = Common.GetTempFilePath(".txt");
            System.IO.File.WriteAllText(path, value);
            ProcessStartInfo psi = new() { FileName = path, UseShellExecute = true };
            Process.Start(psi);
        }
    }
}