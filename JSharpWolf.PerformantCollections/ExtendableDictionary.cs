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
        private IList<Entry> Entries;
        public int Count { get; private set; }
        private int Capacity { get; set; }
        private IComparer<TKey> _comparer;
        private int _free;
        public ExtendableDictionary()
        {
            Entries = new Entry[16];
            Buckets= new int[16];
            Capacity = 16;
            Count = 0;
            _free = 16;
            for (var i = 0; i < 16; ++i)
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
            // TODO: HANDLE RESIZE
            var insertIx = Count;
            Count++;
            var e = new Entry(n, bucketIx,key,value);
            Entries[insertIx] = e;
            Buckets[bucketIx] = insertIx;
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
