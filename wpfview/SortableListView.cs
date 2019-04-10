using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;

namespace SortableWPFGridView
{

    // if the GridView exposed any methods at all that allowed for overriding at a control level, I would be
    // able to do all of this work inside it rather than the ListView. However, b/c it doesn't, I have to do the 
    // work inside the ListView.

    // The GridView has access to the ItemSource on the ListView through the dependency property mechanism.

    public class SortableListView : ListView
    {
        SortableGridViewColumn lastSortedOnColumn = null;
        ListSortDirection lastDirection = ListSortDirection.Ascending;


        #region New Dependency Properties

        public string ColumnHeaderSortedAscendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedAscendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedAscendingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderSortedAscendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedAscendingTemplateProperty =
            DependencyProperty.Register("ColumnHeaderSortedAscendingTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));


        public string ColumnHeaderSortedDescendingTemplate
        {
            get { return (string)GetValue(ColumnHeaderSortedDescendingTemplateProperty); }
            set { SetValue(ColumnHeaderSortedDescendingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderSortedDescendingTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderSortedDescendingTemplateProperty =
            DependencyProperty.Register("ColumnHeaderSortedDescendingTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));


        public string ColumnHeaderNotSortedTemplate
        {
            get { return (string)GetValue(ColumnHeaderNotSortedTemplateProperty); }
            set { SetValue(ColumnHeaderNotSortedTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnHeaderNotSortedTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnHeaderNotSortedTemplateProperty =
            DependencyProperty.Register("ColumnHeaderNotSortedTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));

        #endregion

        /// <summary>
        /// Executes when the control is initialized completely the first time through. Runs only once.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            // add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            // cast the ListView's View to a GridView
            GridView gridView = this.View as GridView;
            if (gridView != null)
            {
                // determine which column is marked as IsDefaultSortColumn. Stops on the first column marked this way.
                SortableGridViewColumn sortableGridViewColumn = null;
                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    sortableGridViewColumn = gridViewColumn as SortableGridViewColumn;
                    if (sortableGridViewColumn != null)
                    {
                        if (sortableGridViewColumn.IsDefaultSortColumn)
                        {
                            break;
                        }
                        sortableGridViewColumn = null;
                    }
                }

                // if the default sort column is defined, sort the data and then update the templates as necessary.
                if (sortableGridViewColumn != null)
                {
                    lastSortedOnColumn = sortableGridViewColumn;
                    Sort(sortableGridViewColumn.SortPropertyName, ListSortDirection.Ascending);

                    if (!String.IsNullOrEmpty(this.ColumnHeaderSortedAscendingTemplate))
                    {
                        sortableGridViewColumn.HeaderTemplate = this.TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                    }

                    this.SelectedIndex = 0;
                }
            }

            base.OnInitialized(e);
        }

        /// <summary>
        /// Event Handler for the ColumnHeader Click Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            // ensure that we clicked on the column header and not the padding that's added to fill the space.
            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                // attempt to cast to the sortableGridViewColumn object.
                SortableGridViewColumn sortableGridViewColumn = (headerClicked.Column) as SortableGridViewColumn;

                // ensure that the column header is the correct type and a sort property has been set.
                if (sortableGridViewColumn != null && !String.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
                {

                    ListSortDirection direction;
                    bool newSortColumn = false;

                    // determine if this is a new sort, or a switch in sort direction.
                    if (lastSortedOnColumn == null
                        || String.IsNullOrEmpty(lastSortedOnColumn.SortPropertyName)
                        || !String.Equals(sortableGridViewColumn.SortPropertyName, lastSortedOnColumn.SortPropertyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        newSortColumn = true;
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    // get the sort property name from the column's information.
                    string sortPropertyName = sortableGridViewColumn.SortPropertyName;

                    List<object> sel = new List<object>();
                    foreach (object s in SelectedItems)
                    {
                        sel.Add(s);
                    }

                    // Sort the data.
                    Sort(sortPropertyName, direction);

                    this.SelectedItems.Clear();
                    foreach (object s in sel)
                    {
                        this.SelectedItems.Add(s);
                    }

                    if (direction == ListSortDirection.Ascending)
                    {
                        if (!String.IsNullOrEmpty(this.ColumnHeaderSortedAscendingTemplate))
                        {
                            sortableGridViewColumn.HeaderTemplate = this.TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                        }
                        else
                        {
                            sortableGridViewColumn.HeaderTemplate = null;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(this.ColumnHeaderSortedDescendingTemplate))
                        {
                            sortableGridViewColumn.HeaderTemplate = this.TryFindResource(ColumnHeaderSortedDescendingTemplate) as DataTemplate;
                        }
                        else
                        {
                            sortableGridViewColumn.HeaderTemplate = null;
                        }
                    }

                    // Remove arrow from previously sorted header
                    if (newSortColumn && lastSortedOnColumn != null)
                    {
                        if (!String.IsNullOrEmpty(this.ColumnHeaderNotSortedTemplate))
                        {
                            lastSortedOnColumn.HeaderTemplate = this.TryFindResource(ColumnHeaderNotSortedTemplate) as DataTemplate;
                        }
                        else
                        {
                            lastSortedOnColumn.HeaderTemplate = null;
                        }
                    }
                    lastSortedOnColumn = sortableGridViewColumn;
                }
            }
        }

        /// <summary>
        /// Helper method that sorts the data.
        /// </summary>
        /// <param name="sortBy"></param>
        /// <param name="direction"></param>
        private void Sort(string sortBy, ListSortDirection direction)
        {
            lastDirection = direction;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
