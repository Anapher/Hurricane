// -- FILE ------------------------------------------------------------------
// name       : ListViewLayoutManager.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Hurricane.Extensions.ListViewLayoutManager
{

    // ------------------------------------------------------------------------
    public class ListViewLayoutManager
    {

        // ----------------------------------------------------------------------
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(ListViewLayoutManager),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLayoutManagerEnabledChanged)));

        // ----------------------------------------------------------------------
        public ListViewLayoutManager(ListView listView)
        {
            if (listView == null)
            {
                throw new ArgumentNullException("listView");
            }

            this.listView = listView;
            this.listView.Loaded += new RoutedEventHandler(ListViewLoaded);
            this.listView.Unloaded += new RoutedEventHandler(ListViewUnloaded);
        } // ListViewLayoutManager

        // ----------------------------------------------------------------------
        public ListView ListView
        {
            get { return listView; }
        } // ListView

        // ----------------------------------------------------------------------
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return verticalScrollBarVisibility; }
            set { verticalScrollBarVisibility = value; }
        } // VerticalScrollBarVisibility

        // ----------------------------------------------------------------------
        public static void SetEnabled(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(EnabledProperty, enabled);
        } // SetEnabled

        // ----------------------------------------------------------------------
        public void Refresh()
        {
            InitColumns();
            DoResizeColumns();
        } // Refresh

        // ----------------------------------------------------------------------
        private void RegisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        Thumb thumb = childVisual as Thumb;
                        if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                            FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                        {
                            thumb.IsHitTestVisible = false;
                        }
                        else
                        {
                            thumb.PreviewMouseMove += new MouseEventHandler(ThumbPreviewMouseMove);
                            thumb.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                            DependencyPropertyDescriptor.FromProperty(
                                GridViewColumn.WidthProperty,
                                typeof(GridViewColumn)).AddValueChanged(gridViewColumn, GridColumnWidthChanged);
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged += new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (scrollViewer == null && childVisual is ScrollViewer)
                {
                    scrollViewer = childVisual as ScrollViewer;
                    scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                    // assume we do the regulation of the horizontal scrollbar
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    scrollViewer.VerticalScrollBarVisibility = verticalScrollBarVisibility;
                }

                RegisterEvents(childVisual);  // recursive
            }
        } // RegisterEvents

        // ----------------------------------------------------------------------
        private void UnregisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        Thumb thumb = childVisual as Thumb;
                        if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                            FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                        {
                            thumb.IsHitTestVisible = true;
                        }
                        else
                        {
                            thumb.PreviewMouseMove -= new MouseEventHandler(ThumbPreviewMouseMove);
                            thumb.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                            DependencyPropertyDescriptor.FromProperty(
                                GridViewColumn.WidthProperty,
                                typeof(GridViewColumn)).RemoveValueChanged(gridViewColumn, GridColumnWidthChanged);
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged -= new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (scrollViewer == null && childVisual is ScrollViewer)
                {
                    scrollViewer = childVisual as ScrollViewer;
                    scrollViewer.ScrollChanged -= new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                }

                UnregisterEvents(childVisual);  // recursive
            }
        } // UnregisterEvents

        // ----------------------------------------------------------------------
        private GridViewColumn FindParentColumn(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            while (element != null)
            {
                GridViewColumnHeader gridViewColumnHeader = element as GridViewColumnHeader;
                if (gridViewColumnHeader != null)
                {
                    return (gridViewColumnHeader).Column;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        } // FindParentColumn

        // ----------------------------------------------------------------------
        private GridViewColumnHeader FindColumnHeader(DependencyObject start, GridViewColumn gridViewColumn)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader gridViewHeader = childVisual as GridViewColumnHeader;
                    if (gridViewHeader.Column == gridViewColumn)
                    {
                        return gridViewHeader;
                    }
                }
                GridViewColumnHeader childGridViewHeader = FindColumnHeader(childVisual, gridViewColumn);  // recursive
                if (childGridViewHeader != null)
                {
                    return childGridViewHeader;
                }
            }
            return null;
        } // FindColumnHeader

        // ----------------------------------------------------------------------
        private void InitColumns()
        {
            GridView view = listView.View as GridView;
            if (view == null)
            {
                return;
            }

            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (!RangeColumn.IsRangeColumn(gridViewColumn))
                {
                    continue;
                }

                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);
                if (!minWidth.HasValue && !maxWidth.HasValue)
                {
                    continue;
                }

                GridViewColumnHeader columnHeader = FindColumnHeader(listView, gridViewColumn);
                if (columnHeader == null)
                {
                    continue;
                }

                double actualWidth = columnHeader.ActualWidth;
                if (minWidth.HasValue)
                {
                    columnHeader.MinWidth = minWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth < columnHeader.MinWidth)
                    {
                        gridViewColumn.Width = columnHeader.MinWidth;
                    }
                }
                if (maxWidth.HasValue)
                {
                    columnHeader.MaxWidth = maxWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth > columnHeader.MaxWidth)
                    {
                        gridViewColumn.Width = columnHeader.MaxWidth;
                    }
                }
            }
        } // InitColumns

        // ----------------------------------------------------------------------
        protected virtual void ResizeColumns()
        {
            GridView view = listView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return;
            }

            // listview width
            double actualWidth = double.PositiveInfinity;
            if (scrollViewer != null)
            {
                actualWidth = scrollViewer.ViewportWidth;
            }
            if (double.IsInfinity(actualWidth))
            {
                actualWidth = listView.ActualWidth;
            }
            if (double.IsInfinity(actualWidth) || actualWidth <= 0)
            {
                return;
            }

            double resizeableRegionCount = 0;
            double otherColumnsWidth = 0;
            // determine column sizes
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    double? proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        resizeableRegionCount += proportionalWidth.Value;
                    }
                }
                else
                {
                    otherColumnsWidth += gridViewColumn.ActualWidth;
                }
            }

            if (resizeableRegionCount <= 0)
            {
                // no proportional columns present: commit the regulation to the scroll viewer
                if (scrollViewer != null)
                {
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                // search the first fill column
                GridViewColumn fillColumn = null;
                for (int i = 0; i < view.Columns.Count; i++)
                {
                    GridViewColumn gridViewColumn = view.Columns[i];
                    if (IsFillColumn(gridViewColumn))
                    {
                        fillColumn = gridViewColumn;
                        break;
                    }
                }

                if (fillColumn != null)
                {
                    double otherColumnsWithoutFillWidth = otherColumnsWidth - fillColumn.ActualWidth;
                    double fillWidth = actualWidth - otherColumnsWithoutFillWidth;
                    if (fillWidth > 0)
                    {
                        double? minWidth = RangeColumn.GetRangeMinWidth(fillColumn);
                        double? maxWidth = RangeColumn.GetRangeMaxWidth(fillColumn);

                        bool setWidth = !(minWidth.HasValue && fillWidth < minWidth.Value);
                        if (maxWidth.HasValue && fillWidth > maxWidth.Value)
                        {
                            setWidth = false;
                        }
                        if (setWidth)
                        {
                            if (scrollViewer != null)
                            {
                                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            }
                            fillColumn.Width = fillWidth;
                        }
                    }
                }
                return;
            }

            double resizeableColumnsWidth = actualWidth - otherColumnsWidth;
            if (resizeableColumnsWidth <= 0)
            {
                return; // missing space
            }

            // resize columns
            double resizeableRegionWidth = resizeableColumnsWidth / resizeableRegionCount;
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    double? proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        gridViewColumn.Width = proportionalWidth.Value * resizeableRegionWidth;
                    }
                }
            }
        } // ResizeColumns

        // ----------------------------------------------------------------------
        // returns the delta
        private double SetRangeColumnToBounds(GridViewColumn gridViewColumn)
        {
            double startWidth = gridViewColumn.Width;

            double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

            if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
            {
                return 0; // invalid case
            }

            if (minWidth.HasValue && gridViewColumn.Width < minWidth.Value)
            {
                gridViewColumn.Width = minWidth.Value;
            }
            else if (maxWidth.HasValue && gridViewColumn.Width > maxWidth.Value)
            {
                gridViewColumn.Width = maxWidth.Value;
            }

            return gridViewColumn.Width - startWidth;
        } // SetRangeColumnToBounds

        // ----------------------------------------------------------------------
        private bool IsFillColumn(GridViewColumn gridViewColumn)
        {
            if (gridViewColumn == null)
            {
                return false;
            }

            GridView view = listView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return false;
            }

            bool? isFillColumn = RangeColumn.GetRangeIsFillColumn(gridViewColumn);
            return isFillColumn.HasValue && isFillColumn.Value;
        } // IsFillColumn

        // ----------------------------------------------------------------------
        private void DoResizeColumns()
        {
            if (resizing)
            {
                return;
            }

            resizing = true;
            try
            {
                ResizeColumns();
            }
            finally
            {
                resizing = false;
            }
        } // DoResizeColumns

        // ----------------------------------------------------------------------
        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents(listView);
            InitColumns();
            DoResizeColumns();
            loaded = true;
        } // ListViewLoaded

        // ----------------------------------------------------------------------
        private void ListViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            UnregisterEvents(listView);
            loaded = false;
        } // ListViewUnloaded

        // ----------------------------------------------------------------------
        private void ThumbPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            if (thumb == null)
            {
                return;
            }
            GridViewColumn gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn == null)
            {
                return;
            }

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                thumb.Cursor = null;
                return;
            }

            // check range column bounds
            if (thumb.IsMouseCaptured && RangeColumn.IsRangeColumn(gridViewColumn))
            {
                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

                if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
                {
                    return; // invalid case
                }

                if (resizeCursor == null)
                {
                    resizeCursor = thumb.Cursor; // save the resize cursor
                }

                if (minWidth.HasValue && gridViewColumn.Width <= minWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else if (maxWidth.HasValue && gridViewColumn.Width >= maxWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else
                {
                    thumb.Cursor = resizeCursor; // between valid min/max
                }
            }
        } // ThumbPreviewMouseMove

        // ----------------------------------------------------------------------
        private void ThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            GridViewColumn gridViewColumn = FindParentColumn(thumb);

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                e.Handled = true;
            }
        } // ThumbPreviewMouseLeftButtonDown

        // ----------------------------------------------------------------------
        private void GridColumnWidthChanged(object sender, EventArgs e)
        {
            if (!loaded)
            {
                return;
            }

            GridViewColumn gridViewColumn = sender as GridViewColumn;

            // suppress column resizing for proportional and fixed columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn))
            {
                return;
            }

            // ensure range column within the bounds
            if (RangeColumn.IsRangeColumn(gridViewColumn))
            {
                // special case: auto column width - maybe conflicts with min/max range
                if (gridViewColumn != null && gridViewColumn.Width.Equals(double.NaN))
                {
                    autoSizedColumn = gridViewColumn;
                    return; // handled by the change header size event
                }

                // ensure column bounds
                if (Math.Abs(SetRangeColumnToBounds(gridViewColumn) - 0) > zeroWidthRange)
                {
                    return;
                }
            }

            DoResizeColumns();
        } // GridColumnWidthChanged

        // ----------------------------------------------------------------------
        // handle autosized column
        private void GridColumnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (autoSizedColumn == null)
            {
                return;
            }

            GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader != null && gridViewColumnHeader.Column == autoSizedColumn)
            {
                if (gridViewColumnHeader.Width.Equals(double.NaN))
                {
                    // sync column with 
                    gridViewColumnHeader.Column.Width = gridViewColumnHeader.ActualWidth;
                    DoResizeColumns();
                }

                autoSizedColumn = null;
            }
        } // GridColumnHeaderSizeChanged

        // ----------------------------------------------------------------------
        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (loaded && Math.Abs(e.ViewportWidthChange - 0) > zeroWidthRange)
            {
                DoResizeColumns();
            }
        } // ScrollViewerScrollChanged

        // ----------------------------------------------------------------------
        private static void OnLayoutManagerEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = dependencyObject as ListView;
            if (listView != null)
            {
                bool enabled = (bool)e.NewValue;
                if (enabled)
                {
                    new ListViewLayoutManager(listView);
                }
            }
        } // OnLayoutManagerEnabledChanged

        // ----------------------------------------------------------------------
        // members
        private readonly ListView listView;
        private ScrollViewer scrollViewer;
        private bool loaded;
        private bool resizing;
        private Cursor resizeCursor;
        private ScrollBarVisibility verticalScrollBarVisibility = ScrollBarVisibility.Auto;
        private GridViewColumn autoSizedColumn;

        private const double zeroWidthRange = 0.1;

    } // class ListViewLayoutManager

} // namespace Itenso.Windows.Controls.ListViewLayout
// -- EOF -------------------------------------------------------------------
