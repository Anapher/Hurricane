using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Hurricane.DataVirtualization
{
    /// <summary>
    /// Specialized list implementation that provides data virtualization. The collection is divided up into pages,
    /// and pages are dynamically fetched from the IItemsProvider when required. Stale pages are removed after a
    /// configurable period of time.
    /// Intended for use with large collections on a network or disk resource that cannot be instantiated locally
    /// due to memory consumption or fetch latency.
    /// </summary>
    /// <remarks>
    /// The IList implmentation is not fully complete, but should be sufficient for use as read only collection 
    /// data bound to a suitable ItemsControl.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class VirtualizingCollection<T> : IList<T>, IList, INotifyCollectionChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeout">The page timeout.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
        {
            _itemsProvider = itemsProvider;
            _pageSize = pageSize;
            _pageTimeout = pageTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
        {
            _itemsProvider = itemsProvider;
            _pageSize = pageSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider)
        {
            _itemsProvider = itemsProvider;
        }

        #endregion

        #region ItemsProvider

        private readonly IItemsProvider<T> _itemsProvider;

        /// <summary>
        /// Gets the items provider.
        /// </summary>
        /// <value>The items provider.</value>
        public IItemsProvider<T> ItemsProvider
        {
            get { return _itemsProvider; }
        }

        #endregion

        #region PageSize

        private readonly int _pageSize = 200;

        /// <summary>
        /// Gets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize
        {
            get { return _pageSize; }
        }

        #endregion

        #region PageTimeout

        private readonly long _pageTimeout = 10000;

        /// <summary>
        /// Gets the page timeout.
        /// </summary>
        /// <value>The page timeout.</value>
        public long PageTimeout
        {
            get { return _pageTimeout; }
        }

        #endregion

        #region IList<T>, IList

        #region Count

        private int _count = -1;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// The first time this property is accessed, it will fetch the count from the IItemsProvider.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public virtual int Count
        {
            get
            {
                if (_count == -1)
                {
                    LoadCount();
                }
                return _count;
            }
            protected set
            {
                _count = value;
            }
        }

        #endregion

        #region Indexer

        /// <summary>
        /// Gets the item at the specified index. This property will fetch
        /// the corresponding page from the IItemsProvider if required.
        /// </summary>
        /// <value></value>
        public T this[int index]
        {
            get
            {
                // determine which page and offset within page
                int pageIndex = index / PageSize;
                int pageOffset = index % PageSize;
                System.Diagnostics.Debug.Print("fetch index {0}", index);
                // request primary page
                RequestPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                    RequestPage(pageIndex + 1);

                // if accessing lower 50% then request prev page
                if (pageOffset < PageSize / 2 && pageIndex > 0)
                    RequestPage(pageIndex - 1);

                // remove stale pages
                CleanUpPages();

                // defensive check in case of async load
                if (_pages[pageIndex] == null)
                    return default(T);

                // return requested item
                return _pages[pageIndex][pageOffset];
            }
            set { throw new NotSupportedException(); }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region IEnumerator<T>, IEnumerator

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <remarks>
        /// This method should be avoided on large collections due to poor performance.
        /// </remarks>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Add
        public void Add(T item)
        {
            this.ItemsProvider.BaseList.Add(item);
            this.RefreshView(ItemsProvider.BaseList.IndexOf(item), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Contains

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        public bool Contains(T item)
        {
            return this.ItemsProvider.BaseList.Contains(item);
        }

        #endregion

        #region Clear
        public void Clear()
        {
            this.ItemsProvider.BaseList.Clear();
            _pages.Clear();
            _pageTouchTimes.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region IndexOf

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public int IndexOf(T item)
        {
            return this.ItemsProvider.BaseList.IndexOf(item);
        }

        #endregion

        #region Insert & Move
        public void Insert(int index, T item)
        {
            this.ItemsProvider.BaseList.Insert(index, item);
            RefreshView(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Move(int oldIndex, int newIndex)
        {
            //this.ItemsProvider.BaseList.Move(oldindex, newindex);
            var item = ItemsProvider.BaseList[oldIndex];

            ItemsProvider.BaseList.RemoveAt(oldIndex);

            if (newIndex > oldIndex) newIndex--;
            // the actual index could have shifted due to the removal

            ItemsProvider.BaseList.Insert(newIndex, item);
            RefreshView(newIndex, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }
        #endregion

        #region Remove

        public void RemoveAt(int index)
        {
            var itemtoremove = ItemsProvider.BaseList[index];
            this.ItemsProvider.BaseList.RemoveAt(index);
            RefreshView(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemtoremove, index));
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            var index = ItemsProvider.BaseList.IndexOf(item);
            if (this.ItemsProvider.BaseList.Remove(item))
            {
                RefreshView(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                return true;
            }
            return false;
        }

        #endregion

        #region CopyTo

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.
        /// -or-
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// -or-
        /// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// -or-
        /// Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Misc

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        public object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns>Always false.
        /// </returns>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>Always true.
        /// </returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns>Always false.
        /// </returns>
        public bool IsFixedSize
        {
            get { return false; }
        }

        #endregion

        #endregion

        #region Paging

        private readonly Dictionary<int, IList<T>> _pages = new Dictionary<int, IList<T>>();
        private readonly Dictionary<int, DateTime> _pageTouchTimes = new Dictionary<int, DateTime>();

        /// <summary>
        /// Cleans up any stale pages that have not been accessed in the period dictated by PageTimeout.
        /// </summary>
        public void CleanUpPages()
        {
            List<int> keys = new List<int>(_pageTouchTimes.Keys);
            foreach (int key in keys)
            {
                // page 0 is a special case, since WPF ItemsControl access the first item frequently
                if (key != 0 && (DateTime.Now - _pageTouchTimes[key]).TotalMilliseconds > PageTimeout)
                {
                    _pages.Remove(key);
                    _pageTouchTimes.Remove(key);
                    Trace.WriteLine("Removed Page: " + key);
                }
            }
        }

        /// <summary>
        /// Populates the page within the dictionary.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="page">The page.</param>
        protected virtual void PopulatePage(int pageIndex, IList<T> page)
        {
            Trace.WriteLine("Page populated: " + pageIndex);
            if (_pages.ContainsKey(pageIndex))
                _pages[pageIndex] = page;
        }

        /// <summary>
        /// Makes a request for the specified page, creating the necessary slots in the dictionary,
        /// and updating the page touch time.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void RequestPage(int pageIndex)
        {
            if (!_pages.ContainsKey(pageIndex))
            {
                _pages.Add(pageIndex, null);
                _pageTouchTimes.Add(pageIndex, DateTime.Now);
                Trace.WriteLine("Added page: " + pageIndex);
                LoadPage(pageIndex);
            }
            else
            {
                _pageTouchTimes[pageIndex] = DateTime.Now;
            }
        }

        #endregion

        #region Load methods

        /// <summary>
        /// Loads the count of items.
        /// </summary>
        protected virtual void LoadCount()
        {
            Count = FetchCount();
        }

        /// <summary>
        /// Loads the page of items.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void LoadPage(int pageIndex)
        {
            PopulatePage(pageIndex, FetchPage(pageIndex));
        }

        #endregion

        #region Fetch methods

        /// <summary>
        /// Fetches the requested page from the IItemsProvider.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        protected IList<T> FetchPage(int pageIndex)
        {
            return ItemsProvider.FetchRange(pageIndex * PageSize, PageSize);
        }

        /// <summary>
        /// Fetches the count of itmes from the IItemsProvider.
        /// </summary>
        /// <returns></returns>
        protected int FetchCount()
        {
            return ItemsProvider.FetchCount();
        }

        #endregion

        #region NotifyCollectionChangedEventHandler

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(this, e);
        }

        protected void RefreshView(int itemchangedindex, NotifyCollectionChangedEventArgs args)
        {
            List<int> keys = new List<int>(_pageTouchTimes.Keys);
            int pageIndex = itemchangedindex / PageSize;
            if (keys.Count > 0 && keys.Count > pageIndex)
            {
                var key = keys[pageIndex];
                _pages.Remove(key);
                _pageTouchTimes.Remove(key);
            }
            OnCollectionChanged(args);
        }

        public void RefreshView(int itemchangedindex)
        {
            this.RefreshView(itemchangedindex, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion
    }
}
