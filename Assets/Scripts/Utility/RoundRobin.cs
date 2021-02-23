using System;
using System.Collections.Generic;
using System.Linq;


public class RoundRobin<T>
{
        private readonly T[] _items;
        private readonly object _syncLock = new object();

        private int _currentIndex = -1;

        public RoundRobin(IEnumerable<T> sequence)
        {
            _items = sequence.ToArray();
            if (_items.Length == 0)
                throw new ArgumentException("Sequence contains no elements.", nameof(sequence));
        }

        public T GetNextItem()
        {
            lock (this._syncLock)
            {
                _currentIndex++;
                if (_currentIndex >= _items.Length)
                    _currentIndex = 0;
                return _items[_currentIndex];
            }
        }
    public int GetCurrentIndex()
    {
        return _currentIndex;
    }
}
