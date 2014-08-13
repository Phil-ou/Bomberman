using System;

namespace Bomberman.Server.Interfaces
{
    public interface IPriorityQueue<TPriority, TData>
        where TPriority : IComparable, IComparable<TPriority>
    {
        int Count { get; }
        TPriority Peek();
        TData Dequeue();
        void Enqueue(TPriority priority, TData data);
        void RemoveAll(Func<TData, bool> predicate);
        void Clear();
    }

}
