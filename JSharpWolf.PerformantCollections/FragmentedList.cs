using System;
using System.Collections;
using System.Collections.Generic;

namespace JSharpWolf.PerformantCollections
{
    /// <summary>
    /// Represents a indexable data structure that fragments its contents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FragmentedList<T> : IList<T>
    {
        private List<List<T>> IndexedNodes { get; set; }
        internal LinkedList<List<T>> Lists { get; set; }
        internal int FragmentSize = 4096;
        private List<T> _currentList;
        private int _totalElements;
        /// <summary>
        /// Initializes a new instance of <see cref="FragmentedList{T}"/> with the default fragment size
        /// </summary>
        public FragmentedList()
        {
            IndexedNodes = new List<List<T>>();
            Lists = new LinkedList<List<T>>();
            _currentList = new List<T>(FragmentSize);
            _totalElements = 0;
            Lists.AddLast(_currentList);
            IndexedNodes.Add(_currentList);
        }
        /// <summary>
        /// Initializes a new instance of <see cref="FragmentedList{T}"/> 
        /// </summary>
        /// <param name="fragmentSize">The number of items contained in a fragment</param>
        public FragmentedList(int fragmentSize) : this()
        {
            FragmentSize = fragmentSize;
        }


        public IEnumerator<T> GetEnumerator()
        {
            return new FragmentedListEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private List<T> RetrieveList(int ix)
        {
            var lix = ix / FragmentSize;
            return lix < Lists.Count ? IndexedNodes[lix] : null;
        }
        /// <summary>
        /// Adds an item to the list
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(T item)
        {
            if (_currentList.Count == FragmentSize)
            {
                _currentList = new List<T>(FragmentSize);
                Lists.AddLast(_currentList);
                IndexedNodes.Add(_currentList);
            }
            _currentList.Add(item);
            _totalElements++;
        }
        /// <summary>
        /// Removes all content from the list
        /// </summary>
        public void Clear()
        {
            IndexedNodes = new List<List<T>>();
            Lists = new LinkedList<List<T>>();
            _currentList = new List<T>(FragmentSize);

            Lists.AddLast(_currentList);
            IndexedNodes.Add(_currentList);
        }

        public bool Contains(T item)
        {
            bool c;
            foreach (var list in Lists)
            {
                c = list.Contains(item);
                if (c) return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var ix = arrayIndex;
            foreach (var elem in this)
            {
                array[ix++] = elem;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException("Removing items is currently not supported on a fragmented list");
        }

        public int Count => _totalElements;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            foreach (var list in Lists)
            {
                var c = list.IndexOf(item);
                if (c > -1) return c;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException("Inserting items is currently not supported on a fragmented list");
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException("Removing items is currently not supported on a fragmented list");
        }

        public T this[int index]
        {
            get
            {
                var l = RetrieveList(index);
                if (l == null) throw new ArgumentOutOfRangeException(nameof(index));
                var si = index % FragmentSize;
                return l[si];
            }
            set
            {
                var l = RetrieveList(index);
                if (l == null) throw new ArgumentOutOfRangeException(nameof(index));
                var si = FragmentSize % index;
                l[si] = value;
            }
        }
    }
}
