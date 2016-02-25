using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSharpWolf.PerformantCollections
{
    /// <summary>
    /// Supports fast iterating of a fragmented list
    /// </summary>
    public class FragmentedListEnumerator<T> : IEnumerator<T>
    {
        private List<T> _currentList;

        private readonly IEnumerator<List<T>> _llEnumerator;
        private readonly FragmentedList<T> _fl;
        private int _lSIndex;
        private int _numLlList;
        public FragmentedListEnumerator(FragmentedList<T> fl)
        {
            _fl = fl;
            _llEnumerator = fl.Lists.GetEnumerator();
            Reset();
        }
        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (_currentList == null || _lSIndex == _fl.FragmentSize)
            {
                if (!_llEnumerator.MoveNext()) return false;
                _numLlList++;
                _currentList = _llEnumerator.Current;
                _lSIndex = 0;
            }
            else
            {
                _lSIndex++;
                if (_lSIndex == _currentList.Count)
                    if (_numLlList == _fl.Lists.Count)
                        return false;
                    else
                    {
                        if (!_llEnumerator.MoveNext()) return false;
                        _numLlList++;
                        _currentList = _llEnumerator.Current;
                        _lSIndex = 0;
                    }
            }
            Current = _currentList[_lSIndex];
            return true;
        }

        public void Reset()
        {
            _currentList = null;
            _llEnumerator.Reset();
            _lSIndex = 0;
            _numLlList = 0;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;
    }
}
