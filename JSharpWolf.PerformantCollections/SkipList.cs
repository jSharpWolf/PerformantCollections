using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JSharpWolf.PerformantCollections
{
    internal class SkipListNode<TKey, TValue>
    {
        public SkipListNode<TKey, TValue>[] Next;
        public TKey Key;
        public TValue Value;
        public bool Nil;
        public volatile bool Marked;
        public volatile bool Linked;
        public volatile object Sync = new object();

        public SkipListNode()
        {
            
        }

        public SkipListNode(int height)
        {
            Next = new SkipListNode<TKey, TValue>[height];
            Nil = true;
        }
        public SkipListNode(TKey key, TValue value, int height)
        {
            Next = new SkipListNode<TKey, TValue>[height];
            Key = key;
            Value = value;
        }
        public void IncreaseHeight(int by)
        {
            var arr = new SkipListNode<TKey, TValue>[Next.Length+by];
            Array.Copy(Next, arr,Next.Length);
            Next = arr;
        }

        public int Height
        {
            get { return Next.Length; }
        }
    }

    public class SkipList<TKey, TValue>
    {
        private IComparer _comparer;
        private SkipListNode<TKey, TValue> _head; 
        private volatile int _height;
        private int _count;
        private Random _rnd;
        private double _prob;
        private int _maxLevel;
        private int _lockage;
        public SkipList()
        {
            _comparer = Comparer.Default;
            Initialize();
        }

        private void Initialize()
        {
            unchecked
            {
                _rnd = new Random((int) DateTime.Now.Ticks);
                _maxLevel = 128;
                _height = 1;
                _count = 0;
                _head = new SkipListNode<TKey, TValue>(default(TKey), default(TValue), _maxLevel);
                _prob = 0.25;
            }
        }

        private int GetRandomLevel()
        {
            var h = 0;
            while (_rnd.NextDouble() <= _prob && h < _height && h < _maxLevel-1)
            {
                h++;
            }
            return h;
        }

        private SkipListNode<TKey, TValue> FindAdjacent(TKey key, out int level)
        {
            var current = _head;
            level = 0;

            for (var i = _height - 1; i >= 0; --i)
            {
                level = i;
                while (current.Next[i] != null && _comparer.Compare(current.Next[i].Key, key) < 0) current = current.Next[i];
            }
            return current;
        }

        private SkipListNode<TKey, TValue>[] GetNodesToUpdate(TKey key)
        {
            var nodes = new SkipListNode<TKey, TValue>[_height];

            var current = _head;
            for (var i = _height - 1; i >= 0; --i)
            {
                while (current.Next[i] != null && _comparer.Compare(current.Next[i].Key, key) < 0) current = current.Next[i];
                nodes[i] = current;
            }
            return nodes;
        }

        private int FindNode(TKey key, SkipListNode<TKey, TValue>[] preds, SkipListNode<TKey, TValue>[] succs, int nh)
        {
            int layerFound = -1;
            var pred = _head;
            SkipListNode<TKey, TValue> cur; 
            for (var i = _maxLevel-1; i >= 0; i--)
            {
                cur = pred.Next[i];

                while (cur != null && _comparer.Compare(cur.Key, key) > 0)
                {
                    pred = cur;
                    cur = pred.Next[i];
                }
                if (cur != null && layerFound == -1 && _comparer.Compare(key, cur.Key) == 0)
                {
                    layerFound = i;
                }
                preds[i] = pred;
                succs[i] = cur;
            }
            return layerFound;
        }

        

        public bool TryAdd(TKey key, TValue value)
        {
            var topLayer = GetRandomLevel();
            var nh = topLayer == _height ? _height + 1 : _height;

            SkipListNode<TKey, TValue>[] preds=new SkipListNode<TKey, TValue>[_maxLevel], 
                                         succs=new SkipListNode<TKey, TValue>[_maxLevel];
            while (true)
            {
                var layerFound = FindNode(key, preds, succs, nh);
                _height = nh;
                if (layerFound != -1)
                {
                    var nf = succs[layerFound];
                    if (!nf.Marked)
                    {
                        while (!nf.Linked)
                        {
                        }
                        return false;
                    }
                    continue;
                }

                int highestLocked = -1;
                try
                {
                    SkipListNode<TKey, TValue> pred = null, succ = null, prevPred = null;
                    var valid = true;
                    for (var i = 0; valid && i <= topLayer; i++)
                    {
                        pred = preds[i];
                        succ = succs[i];
                        if (pred != prevPred)
                        {
                            Interlocked.Increment(ref _lockage);
                            Monitor.Enter(pred.Sync);
                            highestLocked = i;
                            prevPred = pred;
                        }
                        valid = (pred == null || !pred.Marked) && (succ==null || !succ.Marked) &&  pred.Next[i] == succ;
                    }
                    if (!valid) continue;
                    var newNode = new SkipListNode<TKey, TValue>(key, value, topLayer+1);
                    for (var i = 0; i <= topLayer; i++)
                    {
                        newNode.Next[i] = succs[i];
                        preds[i].Next[i] = newNode;
                    }
                    newNode.Linked = true;
                    Interlocked.Increment(ref _count);
                    return true;
                }
                finally
                {
                    Unlock(preds, highestLocked);
                }
            }
        }

        private void Unlock(SkipListNode<TKey, TValue>[] preds, int highestLocked )
        {
            //Monitor.Exit(preds[highestLocked]);
            SkipListNode<TKey, TValue> prev = null;
            for (var i = 0; i <= highestLocked; ++i)
            {
                if (prev != preds[i])
                {
                    Interlocked.Decrement(ref _lockage);
                    Monitor.Exit(preds[i].Sync);
                    prev = preds[i];
                }
            }
        }
        public TValue Find(TKey key)
        {
            int lvl;
            var node = FindAdjacent(key, out lvl);
            return node.Next[lvl].Value;
        }
        public void AddNode(TKey key, TValue value)
        {
            var insertLevel = GetRandomLevel();
            var ntu = GetNodesToUpdate(key);
            var n = ntu[0];
            if (n != null && !n.Nil && _comparer.Compare(ntu[0].Value, value) ==0)
                throw new ArgumentException("Cannot add duplicate values to the SkipList", "key");
            //var uhArr = new SkipListNode<TKey, TValue>[_height];
            var nodeToAdd = new SkipListNode<TKey, TValue>(key, value, insertLevel + 1);

            if (insertLevel > _height)
            {
                _head.IncreaseHeight(1);
                _head.Next[_height] = nodeToAdd;
                _height++;
            }
            for (var i = 0; i < nodeToAdd.Height; ++i)
            {
                if (i < ntu.Length)
                {
                    nodeToAdd.Next[i] = ntu[i].Next[i];
                    ntu[i].Next[i] = nodeToAdd;
                }
            }
        }


    }
}
