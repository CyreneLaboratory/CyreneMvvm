using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CyreneMvvm.Model;

public class ObList<T> : ICollection<T>, IEnumerable<T>, IEnumerable,
    IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyCallback
{
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
        foreach (var item in Internal) RegisterValue(item);
    }

    public T this[int index]
    {
        get => Internal[index];
        set
        {
            var oldItem = Internal[index];
            Internal[index] = value;
            if (!Internal.Contains(oldItem)) UnregisterValue(oldItem);
            RegisterValue(value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }

    public int Count => Internal.Count;
    public bool IsReadOnly => false;
    public int Capacity
    {
        get => Internal.Capacity;
        set => Internal.Capacity = value;
    }

    public void Add(T item)
    {
        Internal.Add(item);
        RegisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Internal.Count - 1));
    }

    public void AddRange(IEnumerable<T> items)
    {
        Internal.AddRange(items);
        foreach (var item in items) RegisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly() => Internal.AsReadOnly();

    public int BinarySearch(int index, int count, T item, IComparer<T>? comparer) => Internal.BinarySearch(index, count, item, comparer);

    public int BinarySearch(T item) => Internal.BinarySearch(item);

    public int BinarySearch(T item, IComparer<T>? comparer) => Internal.BinarySearch(item, comparer);

    public void Clear()
    {
        foreach (var item in Internal) UnregisterValue(item);
        Internal.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item) => Internal.Contains(item);

    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => Internal.ConvertAll(converter);

    public void CopyTo(T[] array, int arrayIndex) => Internal.CopyTo(array, arrayIndex);

    public void CopyTo(T[] array) => Internal.CopyTo(array);

    public void CopyTo(int index, T[] array, int arrayIndex, int count) => Internal.CopyTo(index, array, arrayIndex, count);

    public int EnsureCapacity(int capacity) => Internal.EnsureCapacity(capacity);

    public bool Exists(Predicate<T> match) => Internal.Exists(match);

    public T? Find(Predicate<T> match) => Internal.Find(match);

    public List<T> FindAll(Predicate<T> match) => Internal.FindAll(match);

    public int FindIndex(int startIndex, int count, Predicate<T> match) => Internal.FindIndex(startIndex, count, match);

    public int FindIndex(int startIndex, Predicate<T> match) => Internal.FindIndex(startIndex, match);

    public int FindIndex(Predicate<T> match) => Internal.FindIndex(match);

    public T? FindLast(Predicate<T> match) => Internal.FindLast(match);

    public int FindLastIndex(int startIndex, int count, Predicate<T> match) => Internal.FindLastIndex(startIndex, count, match);

    public int FindLastIndex(int startIndex, Predicate<T> match) => Internal.FindLastIndex(startIndex, match);

    public int FindLastIndex(Predicate<T> match) => Internal.FindLastIndex(match);

    public void ForEach(Action<T> action) => Internal.ForEach(action);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<T> GetEnumerator() => Internal.GetEnumerator();

    public List<T> GetRange(int index, int count) => Internal.GetRange(index, count);

    public int IndexOf(T item, int index, int count) => Internal.IndexOf(item, index, count);

    public int IndexOf(T item, int index) => Internal.IndexOf(item, index);

    public int IndexOf(T item) => Internal.IndexOf(item);

    public void Insert(int index, T item)
    {
        Internal.Insert(index, item);
        RegisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        Internal.InsertRange(index, collection);
        foreach (var item in collection) RegisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public int LastIndexOf(T item) => Internal.LastIndexOf(item);

    public int LastIndexOf(T item, int index) => Internal.LastIndexOf(item, index);

    public int LastIndexOf(T item, int index, int count) => Internal.LastIndexOf(item, index, count);

    public bool Remove(T item)
    {
        var index = Internal.IndexOf(item);
        var removed = Internal.Remove(item);
        if (removed)
        {
            if (!Internal.Contains(item)) UnregisterValue(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
        return removed;
    }

    public int RemoveAll(Predicate<T> match)
    {
        var toRemove = Internal.FindAll(match);
        var removed = Internal.RemoveAll(match);
        if (removed > 0)
        {
            foreach (var item in toRemove)
                if (!Internal.Contains(item)) UnregisterValue(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        return removed;
    }

    public void RemoveAt(int index)
    {
        var item = Internal[index];
        Internal.RemoveAt(index);
        if (!Internal.Contains(item)) UnregisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    public void RemoveRange(int index, int count)
    {
        var toRemove = Internal.GetRange(index, count);
        Internal.RemoveRange(index, count);
        foreach (var item in toRemove)
            if (!Internal.Contains(item)) UnregisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reverse(int index, int count)
    {
        Internal.Reverse(index, count);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Reverse()
    {
        Internal.Reverse();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public List<T> Slice(int start, int length) => Internal.GetRange(start, length);

    public void Sort(IComparer<T>? comparer)
    {
        Internal.Sort(comparer);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort(Comparison<T> comparison)
    {
        Internal.Sort(comparison);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        Internal.Sort(index, count, comparer);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Sort()
    {
        Internal.Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public T[] ToArray() => [.. Internal];

    public void TrimExcess() => Internal.TrimExcess();

    public bool TrueForAll(Predicate<T> match) => Internal.TrueForAll(match);

    #endregion

    private readonly Dictionary<object, Action> ParentObservers = [];
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

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
        ParentObservers.Remove(owner);
    }

    private void RegisterValue(T item)
    {
        if (item is INotifyCallback sub)
            sub.RegisterParent(this, OnParentChanged);
    }

    private void UnregisterValue(T item)
    {
        if (item is INotifyCallback sub)
            sub.UnregisterParent(this);
    }
}
