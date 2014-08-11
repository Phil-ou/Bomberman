using System;
using Bomberman.Server.Console.Interfaces;

namespace Bomberman.Server.Console
{
    public class SortedLinkedList<TPriority, TData> : IPriorityQueue<TPriority, TData>
            where TPriority : IComparable, IComparable<TPriority>
    {
        private class ListItem
        {
            public TPriority Priority { get; set; }
            public TData Data { get; set; }

            public ListItem Previous { get; set; }
            public ListItem Next { get; set; }
        }

        private int _count;
        private ListItem _head;

        public SortedLinkedList()
        {
            _head = null;
            _count = 0;
        }

        #region IPriorityQueue

        public int Count { get { return _count; } }

        public TPriority Peek()
        {
            if (_head == null)
                throw new InvalidOperationException("Collection is empty");
            return _head.Priority;
        }

        public TData Dequeue()
        {
            // Head deletion
            if (_head == null)
                throw new InvalidOperationException("Collection is empty");
            TData data = _head.Data;
            ListItem next = _head.Next;
            if (next != null)
            {
                next.Previous = null;
                _head.Next = null;
            }
            _head = next;
            _count--;
            return data;
        }

        public void Enqueue(TPriority priority, TData data) // Sorted insertion
        {
            // Search insertion point
            ListItem previous = null;
            ListItem p = _head;
            while (p != null && p.Priority.CompareTo(priority) < 0)
            {
                previous = p;
                p = p.Next;
            }
            // Create item
            ListItem item = new ListItem
            {
                Priority = priority,
                Data = data
            };
            // Insert item
            if (p == null) // tail insertion
            {
                if (previous == null) // no head
                    _head = item;
                else // tail insertion
                {
                    previous.Next = item;
                    item.Previous = previous;
                }
            }
            else
            {
                if (previous == null) // head insert (p == head)
                {
                    item.Next = p;
                    p.Previous = item;
                    _head = item;
                }
                else
                {
                    item.Next = p;
                    item.Previous = previous;
                    previous.Next = item;
                    p.Previous = item;
                }
            }
            _count++;
        }

        public void RemoveAll(Func<TData, bool> predicate)
        {
            ListItem item = _head;
            while (item != null)
            {
                if (predicate(item.Data))
                {
                    ListItem next = item.Next;
                    if (item.Previous != null)
                        item.Previous.Next = item.Next;
                    if (item.Next != null)
                        item.Next.Previous = item.Previous;
                    item.Next = null;
                    item.Previous = null;
                    item.Priority = default(TPriority);
                    item.Data = default(TData);
                    if (item == _head)
                        _head = next;
                    item = next;
                    _count--;
                }
                else
                    item = item.Next;
            }
        }

        public void Clear()
        {
            ListItem item = _head;
            while (item != null)
            {
                ListItem next = item.Next;

                item.Priority = default(TPriority);
                item.Data = default(TData);
                item.Previous = null;
                item.Next = null;

                item = next;
            }
            _head = null;
            _count = 0;
        }

        #endregion
    }
}
