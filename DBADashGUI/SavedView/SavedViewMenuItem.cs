﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DBADashGUI
{
    public class SavedViewSelectedEventArgs : EventArgs
    {
        public string Name;
        public bool IsGlobal;
        public string SerializedObject;
    }

    /// <summary>
    /// Custom ToolStipDrownDownButton that shows the saved views available for selection
    /// </summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class SavedViewMenuItem : ToolStripDropDownButton
    {
        private Dictionary<string, string> _savedViews;
        private Dictionary<string, string> _globalSavedViews;

        private readonly static string globalTag = "Global";
        private readonly static string userTag = "User";
        private readonly static string noneText = "{None}";
        private Guid connectionGUID;

        public SavedView.ViewTypes Type { get; set; }

        public event EventHandler<SavedViewSelectedEventArgs> SavedViewSelected;

        public override ToolStripItemDisplayStyle DisplayStyle { get => base.DisplayStyle; set => base.DisplayStyle = ToolStripItemDisplayStyle.Text; }
        public override string Text { get => base.Text; set => base.Text = _text; }
        private string _text = "View";

        private void SetText(string text)
        {
            _text = text;
            base.Text = text;
        }


        /// <summary>
        /// Selects the default view from the menu and raises the SavedViewSelected event for it.  
        /// </summary>
        public bool SelectDefault()
        {
            if (_savedViews.ContainsKey(SavedView.DefaultViewName)) // Check if user has a default view
            {
                SelectItem(SavedView.DefaultViewName, false);
                SavedViewSelected(this, new SavedViewSelectedEventArgs() { Name = SavedView.DefaultViewName, IsGlobal = false, SerializedObject = _savedViews[SavedView.DefaultViewName] });
                return true;
            }
            else if (_globalSavedViews.ContainsKey(SavedView.DefaultViewName)) // check for a global default view if user default view is not available
            {
                SelectItem(SavedView.DefaultViewName, true);
                SavedViewSelected(this, new SavedViewSelectedEventArgs() { Name = SavedView.DefaultViewName, IsGlobal = true, SerializedObject = _globalSavedViews[SavedView.DefaultViewName] });
                return true;
            }
            else
            {
                SelectItem(noneText, true);
                SavedViewSelected(this, new SavedViewSelectedEventArgs() { Name = noneText, IsGlobal = true, SerializedObject = string.Empty });
                return false;
            }
        }

        public bool ContainsUserView(string name)
        {
            return _savedViews.ContainsKey(name);
        }

        public bool ContainsGlobalView(string name)
        {
            return _globalSavedViews.ContainsKey(name);
        }


        /// <summary>
        /// Load saved view menu items.  Only runs if items haven't already been loaded.
        /// </summary>
        public bool LoadItems()
        {
            if (this.HasDropDownItems && this.connectionGUID == Common.ConnectionGUID)
            {
                return false;
            }
            else
            {
                RefreshItems();
                return true;
            }
        }

        /// <summary>
        /// Load saved view menu items and select the default.  Only runs if items haven't already been loaded.
        /// </summary>
        public bool LoadItemsAndSelectDefault()
        {
            var loaded = LoadItems();
            if (loaded)
            {
                SelectDefault();
            }
            return loaded;
        }


        /// <summary>
        /// Load saved view menu items - replaces any existing items with new ones from the DB.
        /// </summary>
        public void RefreshItems()
        {
            _savedViews = SavedView.GetSavedViews(Type, DBADashUser.UserID);
            _globalSavedViews = SavedView.GetSavedViews(Type, DBADashUser.SystemUserID);
            DropDownItems.Clear();
            ToolStripMenuItem mnuNone = new()
            {
                Text = noneText,
                BackColor = DashColors.TrimbleBlueDark,
                ForeColor = Color.White,
                Tag = globalTag
            };
            mnuNone.Click += SavedView_Click;
            DropDownItems.Add(mnuNone);
            foreach (KeyValuePair<string, string> view in _globalSavedViews)
            {
                ToolStripMenuItem mnu = new()
                {
                    Text = view.Key,
                    BackColor = DashColors.TrimbleBlueDark,
                    ForeColor = Color.White,
                    Tag = globalTag
                };
                mnu.Click += SavedView_Click;
                DropDownItems.Add(mnu);
            }
            foreach (KeyValuePair<string, string> view in _savedViews)
            {
                ToolStripMenuItem mnu = new()
                {
                    Text = view.Key,
                    BackColor = DashColors.TrimbleYellow,
                    ForeColor = Color.Black,
                    Tag = userTag
                };
                mnu.Click += SavedView_Click;
                DropDownItems.Add(mnu);
            }
            SetText("View");
            Font = new Font(this.Font, FontStyle.Regular);
            _selectedSavedView = String.Empty;
            connectionGUID = Common.ConnectionGUID; // Detect if we have changed connection to the repository DB for LoadItems
        }

        public void ClearSelectedItem()
        {
            SelectItem(String.Empty, false);
        }

        /// <summary>
        /// Select a saved view in the menu by name
        /// </summary>
        public void SelectItem(string selectedItem, bool isGlobal)
        {
            foreach (ToolStripMenuItem mnu in DropDownItems)
            {
                bool mnuIsGlobal = Convert.ToString(mnu.Tag) == globalTag;
                bool isSelected = mnu.Text.ToLower() == selectedItem.ToLower() && isGlobal == mnuIsGlobal;
                mnu.Checked = isSelected;
                mnu.Font = isSelected ? new Font(mnu.Font, FontStyle.Bold) : new Font(mnu.Font, FontStyle.Regular);
            }
            if (selectedItem != noneText && selectedItem != String.Empty)
            {
                SetText("View: " + selectedItem);
                Font = new Font(this.Font, FontStyle.Bold);
            }
            else
            {
                SetText(this.Text = "View");
                Font = new Font(this.Font, FontStyle.Regular);
            }
            _selectedSavedView = selectedItem;
            _selectedSavedViewIsGlobal = isGlobal;
        }

        private string _selectedSavedView;
        private bool _selectedSavedViewIsGlobal;
        public string SelectedSavedView { get => _selectedSavedView; }
        public bool SelectedSavedViewIsGlobal { get => _selectedSavedViewIsGlobal; }


        private void SavedView_Click(object sender, EventArgs e)
        {
            var mnu = (ToolStripMenuItem)sender;
            bool isGlobal = Convert.ToString(mnu.Tag) == globalTag;
            string serializedObject;
            if (isGlobal)
            {
                serializedObject = mnu.Text == noneText ? String.Empty : _globalSavedViews[mnu.Text];
            }
            else
            {
                serializedObject = _savedViews[mnu.Text];
            }
            SelectItem(mnu.Text, isGlobal);
            SavedViewSelected(this, new SavedViewSelectedEventArgs() { Name = mnu.Text, IsGlobal = isGlobal, SerializedObject = serializedObject });
        }
    }
}
