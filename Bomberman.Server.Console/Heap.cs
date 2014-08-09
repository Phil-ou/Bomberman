using System;

namespace Bomberman.Server.Console
{
    //http://www.boyet.com/Articles/PriorityQueueCSharp3.html
    public class Heap<TPriority, TData>
        where TPriority:IComparable
    {
        private int _count;
        private int _capacity;
        private HeapEntry[] _heap;

        public Heap()
        {
            _count = 0;
            _capacity = 15; // 15 is equal to 4 complete levels
            _heap = new HeapEntry[_capacity];
        }

        public TPriority Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Collection is empty.");

            TPriority priority = _heap[0].Priority;
            return priority;
        }

        public TData Dequeue()
        {
            if (_count == 0)
                throw new InvalidOperationException("Collection is empty.");

            TData result = _heap[0].Item;
            _count--;
            TrickleDown(0, _heap[_count]);
            _heap[_count].Clear();
            return result;
        }

        public void Enqueue(TData item, TPriority priority)
        {
            if (_count == _capacity)
                GrowHeap();
            _count++;
            BubbleUp(_count - 1, new HeapEntry(item, priority));
        }

        public void Clear()
        {
            _count = 0;
            for (int i = 0; i < _heap.Length; i++)
                _heap[i] = default(HeapEntry);
        }

        private void BubbleUp(int index, HeapEntry he)
        {
            int parent = GetParent(index);
            // note: (index > 0) means there is a parent
            while ((index > 0) &&
                  (_heap[parent].Priority.CompareTo(he.Priority) < 0))
            {
                _heap[index] = _heap[parent];
                index = parent;
                parent = GetParent(index);
            }
            _heap[index] = he;
        }

        private static int GetLeftChild(int index)
        {
            return (index * 2) + 1;
        }

        private static int GetParent(int index)
        {
            return (index - 1) / 2;
        }

        private void GrowHeap()
        {
            _capacity = (_capacity * 2) + 1;
            HeapEntry[] newHeap = new HeapEntry[_capacity];
            Array.Copy(_heap, 0, newHeap, 0, _count);
            _heap = newHeap;
        }

        private void TrickleDown(int index, HeapEntry he)
        {
            int child = GetLeftChild(index);
            while (child < _count)
            {
                if (((child + 1) < _count) &&
                    (_heap[child].Priority.CompareTo(_heap[child + 1].Priority) < 0))
                {
                    child++;
                }
                _heap[index] = _heap[child];
                index = child;
                child = GetLeftChild(index);
            }
            BubbleUp(index, he);
        }

        #region HeapEntry

        private struct HeapEntry
        {
            public TData Item { get; private set; }
            public TPriority Priority { get; private set; }

            public HeapEntry(TData item, TPriority priority)
                : this()
            {
                Item = item;
                Priority = priority;
            }

            public void Clear()
            {
                Item = default(TData);
                Priority = default(TPriority);
            }
        }

        #endregion
    }
}
