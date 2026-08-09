using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CyreneMvvm.Model;

public class ObList<T> : ICollection<T>, IEnumerable<T>, IEnumerable,
    IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyCallback
{
    private readonly object SyncRoot = new();

    #region Internal

    private readonly List<T> Internal;

    public ObList()
    {
        Internal = [];
    }

#pragma warning disable IDE0028

    public ObList(int capacity)
    {
        Internal = new(capacity);
    }

#pragma warning restore IDE0028

    public ObList(IEnumerable<T> collection)
    {
        Internal = [.. collection];
        foreach (var item in Internal)
        {
            if (TryIncrementCount(item)) RegisterValue(item);
        }
    }

    public T this[int index]
    {
        get
        {
            lock (SyncRoot)
            {
                return Internal[index];
            }
        }
        set
        {
            T oldItem;
            bool shouldUnregisterOld;
            bool shouldRegisterNew;
            lock (SyncRoot)
            {
                oldItem = Internal[index];
                if (EqualityComparer<T>.Default.Equals(oldItem, value)) return;

                Internal[index] = value;
                shouldUnregisterOld = TryDecrementCount(oldItem);
                shouldRegisterNew = TryIncrementCount(value);
                if (shouldUnregisterOld) EnqueueUnregisterValue(oldItem);
                if (shouldRegisterNew) EnqueueRegisterValue(value);
            }

            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }

    public int Count
    {
        get
        {
            lock (SyncRoot)
            {
                return Internal.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public int Capacity
    {
        get
        {
            lock (SyncRoot)
            {
                return Internal.Capacity;
            }
        }
        set
        {
            lock (SyncRoot)
            {
                Internal.Capacity = value;
            }
        }
    }

    public void Add(T item)
    {
        int newIndex;
        bool shouldRegister;
        lock (SyncRoot)
        {
            Internal.Add(item);
            newIndex = Internal.Count - 1;
            shouldRegister = TryIncrementCount(item);
            if (shouldRegister) EnqueueRegisterValue(item);
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, newIndex));
    }

    public void AddRange(IEnumerable<T> items)
    {
        var list = items.ToList();
        if (list.Count == 0) return;

        lock (SyncRoot)
        {
            Internal.AddRange(list);
            foreach (var item in list)
            {
                if (TryIncrementCount(item)) EnqueueRegisterValue(item);
            }
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly()
    {
        return new System.Collections.ObjectModel.ReadOnlyCollection<T>(this);
    }

    public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
    {
        lock (SyncRoot)
        {
            return Internal.BinarySearch(index, count, item, comparer);
        }
    }

    public int BinarySearch(T item)
    {
        lock (SyncRoot)
        {
            return Internal.BinarySearch(item);
        }
    }

    public int BinarySearch(T item, IComparer<T>? comparer)
    {
        lock (SyncRoot)
        {
            return Internal.BinarySearch(item, comparer);
        }
    }

    public void Clear()
    {
        lock (SyncRoot)
        {
            if (Internal.Count == 0) return;

            foreach (var sub in CallbackCounts.Keys) EnqueueUnregisterValue(sub);
            CallbackCounts.Clear();
            Internal.Clear();
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        lock (SyncRoot)
        {
            return Internal.Contains(item);
        }
    }

    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        lock (SyncRoot)
        {
            return Internal.ConvertAll(converter);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (SyncRoot)
        {
            Internal.CopyTo(array, arrayIndex);
        }
    }

    public void CopyTo(T[] array)
    {
        lock (SyncRoot)
        {
            Internal.CopyTo(array);
        }
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        lock (SyncRoot)
        {
            Internal.CopyTo(index, array, arrayIndex, count);
        }
    }

    public int EnsureCapacity(int capacity)
    {
        lock (SyncRoot)
        {
            return Internal.EnsureCapacity(capacity);
        }
    }

    public bool Exists(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.Exists(match);
        }
    }

    public T? Find(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.Find(match);
        }
    }

    public List<T> FindAll(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindAll(match);
        }
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindIndex(startIndex, count, match);
        }
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindIndex(startIndex, match);
        }
    }

    public int FindIndex(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindIndex(match);
        }
    }

    public T? FindLast(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindLast(match);
        }
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindLastIndex(startIndex, count, match);
        }
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindLastIndex(startIndex, match);
        }
    }

    public int FindLastIndex(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.FindLastIndex(match);
        }
    }

    public void ForEach(Action<T> action)
    {
        List<T> snapshot;
        lock (SyncRoot)
        {
            snapshot = [.. Internal];
        }

        snapshot.ForEach(action);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<T> GetEnumerator()
    {
        List<T> snapshot;
        lock (SyncRoot)
        {
            snapshot = [.. Internal];
        }

        return snapshot.GetEnumerator();
    }

    public List<T> GetRange(int index, int count)
    {
        lock (SyncRoot)
        {
            return Internal.GetRange(index, count);
        }
    }

    public int IndexOf(T item, int index, int count)
    {
        lock (SyncRoot)
        {
            return Internal.IndexOf(item, index, count);
        }
    }

    public int IndexOf(T item, int index)
    {
        lock (SyncRoot)
        {
            return Internal.IndexOf(item, index);
        }
    }

    public int IndexOf(T item)
    {
        lock (SyncRoot)
        {
            return Internal.IndexOf(item);
        }
    }

    public void Insert(int index, T item)
    {
        bool shouldRegister;
        lock (SyncRoot)
        {
            Internal.Insert(index, item);
            shouldRegister = TryIncrementCount(item);
            if (shouldRegister) EnqueueRegisterValue(item);
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        var list = collection.ToList();
        if (list.Count == 0) return;

        lock (SyncRoot)
        {
            Internal.InsertRange(index, list);
            foreach (var item in list)
            {
                if (TryIncrementCount(item)) EnqueueRegisterValue(item);
            }
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public int LastIndexOf(T item)
    {
        lock (SyncRoot)
        {
            return Internal.LastIndexOf(item);
        }
    }

    public int LastIndexOf(T item, int index)
    {
        lock (SyncRoot)
        {
            return Internal.LastIndexOf(item, index);
        }
    }

    public int LastIndexOf(T item, int index, int count)
    {
        lock (SyncRoot)
        {
            return Internal.LastIndexOf(item, index, count);
        }
    }

    public bool Remove(T item)
    {
        int index;
        bool removed;
        var shouldUnregister = false;
        lock (SyncRoot)
        {
            index = Internal.IndexOf(item);
            removed = Internal.Remove(item);
            if (removed)
            {
                shouldUnregister = TryDecrementCount(item);
                if (shouldUnregister) EnqueueUnregisterValue(item);
            }
        }

        if (removed)
        {
            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
        return removed;
    }

    public int RemoveAll(Predicate<T> match)
    {
        List<T> toRemove;
        int removed;
        lock (SyncRoot)
        {
            toRemove = Internal.FindAll(match);
            removed = Internal.RemoveAll(match);

            if (removed > 0)
                foreach (var item in toRemove)
                {
                    if (TryDecrementCount(item)) EnqueueUnregisterValue(item);
                }
        }

        if (removed > 0)
        {
            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        return removed;
    }

    public void RemoveAt(int index)
    {
        T item;
        bool shouldUnregister;
        lock (SyncRoot)
        {
            item = Internal[index];
            Internal.RemoveAt(index);
            shouldUnregister = TryDecrementCount(item);
            if (shouldUnregister) EnqueueUnregisterValue(item);
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    public void RemoveRange(int index, int count)
    {
        if (count == 0) return;

        List<T> toRemove;
        lock (SyncRoot)
        {
            toRemove = Internal.GetRange(index, count);
            Internal.RemoveRange(index, count);

            foreach (var item in toRemove)
            {
                if (TryDecrementCount(item)) EnqueueUnregisterValue(item);
            }
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reverse(int index, int count)
    {
        if (count <= 1) return;

        lock (SyncRoot)
        {
            Internal.Reverse(index, count);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reverse()
    {
        lock (SyncRoot)
        {
            if (Internal.Count <= 1) return;

            Internal.Reverse();
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public List<T> Slice(int start, int length)
    {
        lock (SyncRoot)
        {
            return Internal.GetRange(start, length);
        }
    }

    public void Sort(IComparer<T>? comparer)
    {
        lock (SyncRoot)
        {
            if (Internal.Count <= 1) return;

            Internal.Sort(comparer);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort(Comparison<T> comparison)
    {
        lock (SyncRoot)
        {
            if (Internal.Count <= 1) return;

            Internal.Sort(comparison);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        if (count <= 1) return;

        lock (SyncRoot)
        {
            Internal.Sort(index, count, comparer);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort()
    {
        lock (SyncRoot)
        {
            if (Internal.Count <= 1) return;

            Internal.Sort();
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public T[] ToArray()
    {
        lock (SyncRoot)
        {
            return [.. Internal];
        }
    }

    public void TrimExcess()
    {
        lock (SyncRoot)
        {
            Internal.TrimExcess();
        }
    }

    public bool TrueForAll(Predicate<T> match)
    {
        lock (SyncRoot)
        {
            return Internal.TrueForAll(match);
        }
    }

    #endregion

    private readonly ConcurrentDictionary<object, Action> ParentObservers = [];
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    private readonly Dictionary<INotifyCallback, int> CallbackCounts = new(ReferenceEqualityComparer.Instance);
    private readonly Queue<Action> PendingValueCallbacks = new();
    private bool IsDrainingValueCallbacks;

    private bool TryIncrementCount(T item)
    {
        if (item is INotifyCallback sub)
        {
            CallbackCounts.TryGetValue(sub, out var count);
            CallbackCounts[sub] = count + 1;
            return count == 0;
        }
        return false;
    }

    private bool TryDecrementCount(T item)
    {
        if (item is INotifyCallback sub)
        {
            if (CallbackCounts.TryGetValue(sub, out var count))
            {
                if (count <= 1)
                {
                    CallbackCounts.Remove(sub);
                    return true;
                }
                CallbackCounts[sub] = count - 1;
            }
        }
        return false;
    }

    protected virtual void OnParentChanged()
    {
        foreach (var callback in ParentObservers.Values.ToArray()) callback();
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        OnParentChanged();
    }

    public void RegisterParent(object owner, Action callback)
    {
        ParentObservers[owner] = callback;
    }

    public void UnregisterParent(object owner)
    {
        ParentObservers.TryRemove(owner, out _);
    }

    private void RegisterValue(T item)
    {
        if (item is INotifyCallback sub)
            sub.RegisterParent(this, OnParentChanged);
    }

    private void EnqueueRegisterValue(T item)
    {
        if (item is INotifyCallback sub)
            PendingValueCallbacks.Enqueue(() => sub.RegisterParent(this, OnParentChanged));
    }

    private void EnqueueUnregisterValue(T item)
    {
        if (item is INotifyCallback sub)
            PendingValueCallbacks.Enqueue(() => sub.UnregisterParent(this));
    }

    private void EnqueueUnregisterValue(INotifyCallback sub)
    {
        PendingValueCallbacks.Enqueue(() => sub.UnregisterParent(this));
    }

    private void DrainPendingValueCallbacks()
    {
        Action action;
        lock (SyncRoot)
        {
            if (IsDrainingValueCallbacks || PendingValueCallbacks.Count == 0) return;

            IsDrainingValueCallbacks = true;
            action = PendingValueCallbacks.Dequeue();
        }

        try
        {
            while (true)
            {
                action();

                lock (SyncRoot)
                {
                    if (PendingValueCallbacks.Count == 0)
                    {
                        IsDrainingValueCallbacks = false;
                        return;
                    }

                    action = PendingValueCallbacks.Dequeue();
                }
            }
        }
        catch
        {
            lock (SyncRoot)
            {
                IsDrainingValueCallbacks = false;
            }

            throw;
        }
    }
}
