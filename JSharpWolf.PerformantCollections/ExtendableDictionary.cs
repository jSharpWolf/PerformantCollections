using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSharpWolf.PerformantCollections
{
    
    public class ExtendableDictionary<TKey, TValue>
    {
        public IList<int> Buckets;
        public IList<Entry> Entries;
        public int Count { get; private set; }
        private int Capacity { get; set; }
        private IComparer<TKey> _comparer;
        private int _free;
        public ExtendableDictionary()
        {
            Entries = new Entry[4];
            Buckets= new int[4];
            Capacity = 4;
            Count = 0;
            _free = 16;
            for (var i = 0; i < 4; ++i)
                Buckets[i] = -1;
        }
        
        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        protected void Insert(TKey key, TValue value, bool add)
        {
            var n = key.GetHashCode() & int.MaxValue;
            var bucketIx = n%Capacity;
            int addIx;
            for (var i = Buckets[bucketIx]; i >= 0; i = Entries[i].Next) // Replace an existing entry in the dictionary
            {
                if (Entries[i].HashCode == n && (_comparer != null
                    ? _comparer.Compare(Entries[i].Key, key) == 0
                    : Entries[i].Key.Equals(key)))
                {
                    if (add) throw new ArgumentException("Cannot add a duplicate key to the dictionary", nameof(key));
                    Entries[i] = new Entry(Entries[i], value);
                }
            }
            var insertIx = Count;
            Count++;
            if (insertIx == Count)
            {
                ResizeArray(Count*2);
            }
            var e = new Entry(n, bucketIx,key,value);
            Entries[insertIx] = e;
            Buckets[bucketIx] = insertIx;
        }

        protected void ResizeArray(int size)
        {
            Buckets = new int[size];
            for (var i = 0; i < size; ++i) Buckets[i] = -1;
            var newEntries = new Entry[size];
            var x = Entries as Entry[] as Array;
            if (x==null) throw new Exception("Excpected a reziable array for the dictionary");
            Array.Copy(x, newEntries, Count);
            for (var i = 0; i < Count; ++i)
            {
                if (newEntries[i].HashCode >= 0)
                {
                    var nextIndex = newEntries[i].HashCode%size;
                    newEntries[i].Next = Buckets[nextIndex];
                }
            }
            Entries = newEntries;
        }
        public struct Entry
        {
            public int HashCode;
            public int Next;
            public TKey Key;
            public TValue Value;

            public Entry(Entry oe, TValue tv)
            {
                HashCode = oe.HashCode;
                Next = oe.Next;
                Key = oe.Key;
                Value = oe.Value;
            }

            public Entry(int hashCode, int next, TKey key, TValue value)
            {
                HashCode = hashCode;
                Next = next;
                Key = key;
                Value = value;
            }
        }
    }
}
