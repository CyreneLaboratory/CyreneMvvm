using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CyreneMvvm.Model;

public class ObservableList<T> : ICollection<T>, IEnumerable<T>, IEnumerable,
    IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyCallback
{
    #region Internal

    private readonly List<T> Internal;

    public ObservableList()
    {
        Internal = [];
    }

    public ObservableList(IEnumerable<T> collection)
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

    public void Clear()
    {
        foreach (var item in Internal) UnregisterValue(item);
        Internal.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        return Internal.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Internal.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Internal.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return Internal.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        Internal.Insert(index, item);
        RegisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

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

    public void RemoveAt(int index)
    {
        var item = Internal[index];
        Internal.RemoveAt(index);
        if (!Internal.Contains(item)) UnregisterValue(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
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
