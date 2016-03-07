using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace JSharpWolf.PerformantCollections
{
    internal class SkipListNode<TKey, TValue>
    {
        public SkipListNode<TKey, TValue>[] Next;
        public TKey Key;
        public TValue Value;

        public SkipListNode()
        {
            
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
        private int _height;
        private int _count;
        private Random _rnd;
        private double _prob;
        private int _maxLevel;
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
                _height = 1;
                _count = 0;
                _head = new SkipListNode<TKey, TValue>(default(TKey), default(TValue), 1);
                _prob = 0.25;
                _maxLevel = 64;
            }
        }

        private int GetRandomLevel()
        {
            var h = 0;
            while (_rnd.NextDouble() <= _prob && h < _height+1  && h<_maxLevel)
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

        public TValue Find(TKey key)
        {
            int lvl;
            var node = FindAdjacent(key, out lvl);
            return node.Next[lvl].Value;
        }
        public void AddNode(TKey key, TValue value)
        {
            int adjLevel;
            var adjacent = FindAdjacent(key, out adjLevel);
            if (adjacent.Next[adjLevel] != null && _comparer.Compare(adjacent.Next[adjLevel].Key, key) ==0)
                throw new ArgumentException("Cannot add duplicate values to the SkipList", "key");
            //var uhArr = new SkipListNode<TKey, TValue>[_height];
            var insertLevel = GetRandomLevel();
            var nodeToAdd = new SkipListNode<TKey, TValue>(key, value, insertLevel + 1);
            if (insertLevel > _height)
            {
                _head.IncreaseHeight(1);
                _height++;
            }
            for (var i = 0; i < adjacent.Height &&  i < nodeToAdd.Height; ++i)
            {
                nodeToAdd.Next[i] = adjacent.Next[i];
            }
            adjacent.Next[adjLevel] = nodeToAdd;
        }

        public void AddNode2(TKey key, TValue value)
        {
            var current= _head;
            var uhArr = new SkipListNode<TKey, TValue>[_height];
            var insertLevel = GetRandomLevel();
            var nodeToAdd = new SkipListNode<TKey, TValue>(key, value, insertLevel+1);
            for (var i = _height - 1; i >= 0; --i)
            {
                while (current.Next[i] != null && _comparer.Compare(current.Next[i], key) < 0)
                    current = current.Next[i];
                uhArr[i] = current;
            }

            for (var i = 0; i < _height; ++i)
            {
                nodeToAdd.Next[i] = uhArr[i].Next[i];
                uhArr[i].Next[i] = nodeToAdd;
            }
            if (insertLevel > _height)
            {
                _head.IncreaseHeight(1);
                _head.Next[_height - 1] = nodeToAdd;
                _height++;
            }
        }

    }
}
